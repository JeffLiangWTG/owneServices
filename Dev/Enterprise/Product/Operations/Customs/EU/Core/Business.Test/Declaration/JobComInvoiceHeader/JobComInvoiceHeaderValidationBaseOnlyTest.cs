using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.EU;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class JobComInvoiceHeaderValidationBaseOnlyTest : TestCaseWithFactory
	{
		public void TestCheckJZ_ValuationCode_RuleC0627()
		{
			const string C0627ErrorMessageWhenSubStyleIsCOrF = "[C0627] This field must be empty in case of Declaration Sub Type C or F.";
			const string C0627ErrorMessageWhenSubStyleIsNotCOrF = "[C0627] This field is mandatory for this declaration Sub Type.";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine2.JI_CEI = entryInstruction2.PK;

			using (var context = new InvoiceHeaderValidationDeciderTestContext(declaration, true))
			{
				context.EnableRule(r => r.IsRuleC0627Active);
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
				invoice.JZ_ValuationCode = "12";
				AssertHasMessageError("IsRuleC0627Active is true, Sub Style in C/F, JZ_ValuationCode of invoice header should be empty", invoice.JZ_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsCOrF);

				context.DisableRule(r => r.IsRuleC0627Active);
				invoice.Validation.ValidateJZ_ValuationCode();
				AssertNoMessageError("IsRuleC0627Active is false, Rule C0627 does not apply", invoice.JZ_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsCOrF);

				context.EnableRule(r => r.IsRuleC0627Active);
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				invoice.JZ_ValuationCode = ZString.Empty;
				AssertNoMessageError(invoice.JZ_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsCOrF);

				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
				invoiceLine1.ZG_TransNature = "12";
				invoice.JZ_ValuationCode = ZString.Empty;
				AssertHasMessageError("UCC6, Import, Sub Style is not C/F, not all NT on GI have values, JZ_ValuationCode of invoice header is mandatory", invoice.JZ_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsNotCOrF);

				context.DisableRule(r => r.IsRuleC0627Active);
				invoice.Validation.ValidateJZ_ValuationCode();
				AssertNoMessageError("IsRuleC0627Active is false, Rule C0627 does not apply to EXP", invoice.JZ_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsNotCOrF);

				context.EnableRule(r => r.IsRuleC0627Active);
				invoiceLine2.ZG_TransNature = "34";
				invoice.Validation.ValidateJZ_ValuationCode();
				AssertNoMessageError("All NT on GI have values", invoice.JZ_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsNotCOrF);
				invoiceLine2.ZG_TransNature = ZString.Empty;
				invoice.JZ_ValuationCode = "56";
				AssertNoMessageError(invoice.JZ_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsNotCOrF);
			}

			using (var context = new InvoiceHeaderValidationDeciderTestContext(declaration, false))
			{
				context.DisableRule(r => r.IsRuleC0627Active);
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
				invoice.JZ_ValuationCode = "12";
				AssertNoMessageError("Rule C0627 does not apply to non UCC6", invoice.JZ_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsCOrF);

				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
				invoiceLine1.ZG_TransNature = "12";
				invoice.JZ_ValuationCode = ZString.Empty;
				AssertNoMessageError("Rule C0627 does not apply to non UCC6", invoice.JZ_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsNotCOrF);
			}
		}

		public void TestCheckRelatedIndicators_RuleC0624()
		{
			const string C0624ErrorString = "[C0624] Value indicator should not be entered for the provided Declaration Type and Requested Procedure.";

			var testCase = new MultiFactorTestCase<JobDeclaration>(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				return declaration;
			});

			var isUCC = new ConfigurationPreq<JobDeclaration>(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration).Values(true);
			var isImport = new FieldPreq<JobDeclaration>(dec => dec.JE_MessageTypeInfo).Values(MessageTypeList.Codes.Import);
			var validCEI_Procedure = new FieldPreq<JobDeclaration>(dec => dec.CustomsEntryInstructions[0].CEI_ProcedureInfo).Values("53", "71");
			var validCEI_SubStyle = new FieldPreq<JobDeclaration>(dec => dec.CustomsEntryInstructions[0].CEI_SubStyleInfo).Values("C", "F").NotValues("D");

			var relatedIndicatorsToTest = new List<Func<JobDeclaration, ZPropertyInfo>>()
			{
				dec => dec.Invoices[0].RelatedIndicatorInfo,
				dec => dec.Invoices[0].ZG_RelatedIndicator2Info,
				dec => dec.Invoices[0].ZG_RelatedIndicator3Info,
				dec => dec.Invoices[0].ZG_RelatedIndicator4Info,
			};

			relatedIndicatorsToTest.ForEach(getTargetRelatedIndicator =>
			{
				var relatedIndicatorTicked = new FieldPreq<JobDeclaration>(getTargetRelatedIndicator).Values(true);
				testCase.SetUpCondition(isUCC && isImport && (validCEI_Procedure || validCEI_SubStyle) && relatedIndicatorTicked);
				testCase.SetUpProcessAction(dec =>
				{
					dec.Invoices[0].Validation.ValidateJZ_RelatedIndicator();
					dec.Invoices[0].AddInfoValidation.ValidateAll();
				});
				testCase.RunAssertion(dec => AssertHasMessageError(getTargetRelatedIndicator(dec), C0624ErrorString), dec => AssertNoMessageErrors(getTargetRelatedIndicator(dec)));
			});
		}

		public void TestCheckJZ_WeightWhitTotalOfInvoiceLine()
		{
			var dec = Factory.New<JobDeclaration>();
			var inv = dec.Invoices.AddNew();
			var invoiceLine1 = inv.InvoiceLines.AddNew();
			invoiceLine1.JI_Weight = 1000;
			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			var invoiceLine2 = inv.InvoiceLines.AddNew();
			invoiceLine2.JI_Weight = 100000;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Grams;

			inv.JZ_WeightUQ = Core.Constants.Weight.Tonnes;
			inv.JZ_Weight = 2.35;
			CombineAssertions("", () =>
			{
				AssertHasWarningContaining("The sum of gross weight 1.1 Tonnes in Invoice Lines does not match with the total gross weight of the invoice.", inv.JZ_WeightInfo, "The sum of gross weight");
				inv.JZ_Weight = 1.1;
				AssertNoWarnings(inv.JZ_WeightInfo);
			});
		}

		public void TestCheckJZ_NetWeightWhitTotalOfInvoiceLine()
		{
			var dec = Factory.New<JobDeclaration>();
			var inv = dec.Invoices.AddNew();
			var invoiceLine1 = inv.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 1000;
			invoiceLine1.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;

			var invoiceLine2 = inv.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 100000;
			invoiceLine2.JI_CustomsUnitQty = Core.Constants.Weight.Grams;

			inv.JZ_NetWeightUQ = Core.Constants.Weight.Tonnes;
			inv.JZ_NetWeight = 2.35;
			CombineAssertions("", () =>
			{
				AssertHasWarningContaining("The sum of customs quantity 1.1 Tonnes in Invoice Lines does not match with the total net weight of the invoice.", inv.JZ_NetWeightInfo, $"The sum of {inv.JZ_NetWeightInfo.HumanReadableName}");
				inv.JZ_NetWeight = 1.1;
				AssertNoWarnings(inv.JZ_NetWeightInfo);
			});
		}

		public void TestCheckJZ_NetWeightWhitTotalOfInvoiceLineInKGM()
		{
			var dec = Factory.New<JobDeclaration>();
			var inv = dec.Invoices.AddNew();
			var invoiceLine1 = inv.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 1000;
			invoiceLine1.JI_CustomsUnitQty = "KGM";

			var invoiceLine2 = inv.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 1000;
			invoiceLine2.JI_CustomsUnitQty = "KGM";

			inv.JZ_NetWeightUQ = Core.Constants.Weight.Kilograms;
			inv.JZ_NetWeight = 1000;
			CombineAssertions("", () =>
			{
				AssertHasWarningContaining("The sum of customs quantity 1000 KGM in Invoice Lines does not match with the total net weight of the invoice.", inv.JZ_NetWeightInfo, $"The sum of {inv.JZ_NetWeightInfo.HumanReadableName}");
				inv.JZ_NetWeight = 2000;
				AssertNoWarnings(inv.JZ_NetWeightInfo);
			});
		}

		public void TestCheckJZ_IncoTermPlace_RequiredForUCC6_XXX()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetAgreedPlaceCodeSupport(declaration, false))
			{
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.Other;
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.JZ_IncoTermPlaceInfo);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetAgreedPlaceCodeSupport(declaration, true))
			{
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.CostAndFreight;
				invoiceHeader.ZG_AgreedPlaceCode = Core.Constants.CountryCodes.Germany;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.JZ_IncoTermPlaceInfo);

				invoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.CostAndFreight;
				invoiceHeader.ZG_AgreedPlaceCode = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.JZ_IncoTermPlaceInfo);

				invoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.Other;
				invoiceHeader.ZG_AgreedPlaceCode = ZString.Empty;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.JZ_IncoTermPlaceInfo);

				invoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.Other;
				invoiceHeader.ZG_AgreedPlaceCode = Core.Constants.CountryCodes.Germany;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.JZ_IncoTermPlaceInfo);
			}
		}

		public void TestCheckJZ_IncotermIsRuleC0729Active()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

			var customEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;

			var validation = invoiceHeader.Validation;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = customEntryInstruction.PK;

			const string errorMessage = "Please enter an Incoterm";
			var propertyInfo = invoiceHeader.JZ_IncoTermInfo;

			var ucc6ImportInvoiceHeaderValidationDeciderMock = new Mock<IInvoiceHeaderValidationDecider>();
			ucc6ImportInvoiceHeaderValidationDeciderMock.Setup(x => x.IsRuleC0729Active).Returns(true);
			ucc6ImportInvoiceHeaderValidationDeciderMock.Setup(x => x.IsRuleC0738Active).Returns(true);

			var invoiceHeaderConfigurationMock = new Mock<InvoiceHeaderConfiguration>();
			invoiceHeaderConfigurationMock.Protected()
				.Setup<IInvoiceHeaderValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<JobComInvoiceHeader>())
				.Returns(ucc6ImportInvoiceHeaderValidationDeciderMock.Object);

			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "GetNew" + nameof(DeclarationConfiguration.InvoiceHeaderConfiguration), invoiceHeaderConfigurationMock.Object), true))
			{
				CombineAssertions(() =>
				{
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("RuleC0729 is enabled, Substlyle A, Procedure 71, So, Incoterm is optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._48;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("RuleC0729 is enabled, Substlyle A, Procedure 48, So, Incoterm is required.", propertyInfo, errorMessage);

					ucc6ImportInvoiceHeaderValidationDeciderMock.Setup(x => x.IsRuleC0729Active).Returns(false);
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("RuleC0729 is disabled, Substlyle A, Procedure 48, So, Incoterm is required.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._53;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("RuleC0729 is disabled, Substlyle A, Procedure 53, So, Incoterm is required.", propertyInfo, errorMessage);
				});
			}
		}

		public void TestCheckJZ_IncoTerm_WhenNoEntryInstructionAdded_RuleC0729()
		{
			const string errorMessage = "Please enter an Incoterm";
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions(() =>
				{
					var propertyInfo = invoiceHeader.JZ_IncoTermInfo;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("Entry Instruction not present, Incoterm fields are required.", propertyInfo, errorMessage);

					var customEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
					invoiceLine.JI_CL = customEntryInstruction.PK;
					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;

					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("Entry Instruction present, substyle is A, procedure is 71 => Incoterm fields are optional.", propertyInfo, errorMessage);
				});
			}
		}

		public void TestCheckJZ_IncoTerm_RuleC0729()
		{
			const string errorMessage = "Please enter an Incoterm";
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var customEntryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = customEntryInstruction.PK;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = customEntryInstruction.PK;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions(() =>
				{
					var propertyInfo = invoiceHeader.JZ_IncoTermInfo;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining(propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("Substyle is A, procedure is 71, Incoterm fields are optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("Substyle is D, procedure is 71, Incoterm fields are optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("Substyle is Y, procedure is 71, Incoterm fields are optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._53;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("Substyle is A, procedure is 53, Incoterm fields are optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._53;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("Substyle is D, procedure is 53, Incoterm fields are optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._53;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("Substyle is Y, procedure is 53, Incoterm fields are optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationUnderTheProcedureCoveredUnderArticle182OfTheCode;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._53;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("Substyle is V, procedure is 53, Incoterm fields are required.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._48;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("Substyle is Y, procedure is 48, Incoterm fields are rquired.", propertyInfo, errorMessage);
				});
			}
		}

		public void TestCheckJZ_IncoTerm_DeclarationMessageTypeAndUcc6_RuleC0729()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var customEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = customEntryInstruction.PK;

			customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
			const string errorMessage = "Please enter an Incoterm";
			var propertyInfo = invoiceHeader.JZ_IncoTermInfo;

			CombineAssertions(() =>
			{
				invoiceHeader.Validation.ValidateJZ_IncoTerm();
				AssertHasMessageErrorContaining("Declaration is Non-UCC6 Import, substyle is A and procedure is 71, Incoterm is required.", propertyInfo, errorMessage);

				customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._01;
				invoiceHeader.Validation.ValidateJZ_IncoTerm();
				AssertHasMessageErrorContaining("Declaration is Non-UCC6 Import, substyle is A and procedure is 01, Incoterm is required.", propertyInfo, errorMessage);

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("Declaration is UCC6 Import, substyle is A and procedure is 71, Incoterm is optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("Declaration is UCC6 Import, substyle is B and procedure is 71, Incoterm section is required.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._01;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("Declaration is UCC6 Import, substyle is A and procedure is 01, Incoterm section is required.", propertyInfo, errorMessage);

					declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("Declaration is UCC6 Export, substyle is A and procedure is 01, Incoterm section is required.", propertyInfo, errorMessage);
				}
				customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
				invoiceHeader.Validation.ValidateJZ_IncoTerm();
				AssertHasMessageErrorContaining("Declaration is Non-UCC6 Export, substyle is A and procedure is 71, Incoterm section is required.", propertyInfo, errorMessage);
			});
		}

		public void TestCheckJZ_IncoTerm_WithMultipleEntryInstructions_RuleC0729()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();

			var customEntryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var customEntryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = customEntryInstruction1.PK;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = customEntryInstruction2.PK;

			const string errorMessage = "Please enter an Incoterm";
			var propertyInfo = invoiceHeader.JZ_IncoTermInfo;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions(() =>
				{
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("Both CustomEntryInstructions have substyle Null and procedures Null, Incoterm is required.", propertyInfo, errorMessage);

					customEntryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
					customEntryInstruction1.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;

					customEntryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
					customEntryInstruction2.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._42;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("CustomEntryInstruction2 has substyle NOT in [A, D, Y] and procedure NOT in [71, 53], so Incoterm is required.", propertyInfo, errorMessage);

					customEntryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
					customEntryInstruction2.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._53;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("Both CustomEntryInstruction2 have substyles in [A, D, Y] and procedure in [71, 53], so Incoterm is optional.", propertyInfo, errorMessage);

					customEntryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationOfSimplifiedDeclarationsCoveredByCAndF;
					customEntryInstruction1.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("CustomEntryInstruction1 has substyle NOT in [A, D, Y] and procedure is 71, so Incoterm is required.", propertyInfo, errorMessage);
				});
			}
		}

		public void TestCheckJZ_IncoTerm_RuleC0738()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

			var customEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;

			const string errorMessage = "Please enter an Incoterm";
			var propertyInfo = invoiceHeader.JZ_IncoTermInfo;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions(() =>
				{
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("No InvoiceLine present, So Incoterm is optional", propertyInfo, errorMessage);

					var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
					invoiceLine1.JI_CL = customEntryInstruction.PK;
					invoiceLine1.JI_ValuationCode = ValuationMethodList.Codes._1;

					var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
					invoiceLine2.JI_ValuationCode = ValuationMethodList.Codes._1;

					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("At lest one InvoiceLine has JI_ValuationCode as 1 So Incoterm is required", propertyInfo, errorMessage);

					invoiceLine1.JI_ValuationCode = ZString.Empty;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("At lest one InvoiceLine has JI_ValuationCode as 1, So Incoterm is required", propertyInfo, errorMessage);

					invoiceLine2.JI_ValuationCode = ZString.Empty;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("None of the Invoicelines have a JI_ValuationCode of 1, Incoterm is optional", propertyInfo, errorMessage);

					invoiceLine1.JI_ValuationCode = ValuationMethodList.Codes._2;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("None of the Invoicelines have a JI_ValuationCode of 1, Incoterm is optional", propertyInfo, errorMessage);

					invoiceLine1.JI_ValuationCode = ValuationMethodList.Codes._5;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("None of the Invoicelines have a JI_ValuationCode of 1, Incoterm is optional", propertyInfo, errorMessage);
				});
			}
		}

		public void TestCheckJZ_IncoTerm_DeclarationMessageTypeAndUcc6_RuleC0738()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

			var customEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = customEntryInstruction.PK;
			invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;

			var propertyInfo = invoiceHeader.JZ_IncoTermInfo;
			const string error = "Please enter an Incoterm";

			CombineAssertions(() =>
			{
				invoiceHeader.Validation.ValidateJZ_IncoTerm();
				AssertHasMessageErrorContaining("Declaration is Non-UCC6 Import, JI_ValuationCode is Empty, Incoterm is optional", propertyInfo, error);

				invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;
				invoiceHeader.Validation.ValidateJZ_IncoTerm();
				AssertHasMessageErrorContaining("Declaration is Non-UCC6 Import, JI_ValuationCode is 1, Incoterm is optional", propertyInfo, error);

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._6;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("Declaration is UCC6 Import, JI_ValuationCode is 6, Incoterm is optional", propertyInfo, error);

					invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("Declaration is UCC6 Import, JI_ValuationCode is 1, Incoterm is required", propertyInfo, error);

					declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("Declaration is UCC6 Export, JI_ValuationCode is 1, Incoterm section is optional", propertyInfo, error);

					invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._3;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("Declaration is UCC6 Export, JI_ValuationCode is 3, Incoterm is optional", propertyInfo, error);
				}

				invoiceHeader.Validation.ValidateJZ_IncoTerm();
				AssertHasMessageErrorContaining("Declaration is Non-UCC6 Export, JI_ValuationCode is 3, Incoterm section is optional", propertyInfo, error);

				invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;
				invoiceHeader.Validation.ValidateJZ_IncoTerm();
				AssertHasMessageErrorContaining("Declaration is Non-UCC6 Export, JI_ValuationCode is 1, Incoterm section is optional", propertyInfo, error);
			});
		}

		public void TestCheckJZ_IncotermIsRuleC0738Active()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

			var customEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;

			var validation = invoiceHeader.Validation;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = customEntryInstruction.PK;
			invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._2;

			const string errorMessage = "Please enter an Incoterm";
			var propertyInfo = invoiceHeader.JZ_IncoTermInfo;

			var ucc6ImportInvoiceHeaderValidationDeciderMock = new Mock<IInvoiceHeaderValidationDecider>();
			ucc6ImportInvoiceHeaderValidationDeciderMock.Setup(x => x.IsRuleC0729Active).Returns(true);
			ucc6ImportInvoiceHeaderValidationDeciderMock.Setup(x => x.IsRuleC0738Active).Returns(true);

			var invoiceHeaderConfigurationMock = new Mock<InvoiceHeaderConfiguration>();
			invoiceHeaderConfigurationMock.Protected()
				.Setup<IInvoiceHeaderValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<JobComInvoiceHeader>())
				.Returns(ucc6ImportInvoiceHeaderValidationDeciderMock.Object);

			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "GetNew" + nameof(DeclarationConfiguration.InvoiceHeaderConfiguration), invoiceHeaderConfigurationMock.Object), true))
			{
				CombineAssertions(() =>
				{
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining("RuleC0738 is enabled, JI_ValuationCodes is 2. So, Incoterm is optional.", propertyInfo, errorMessage);

					invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("RuleC0738 is enabled, JI_ValuationCodes is 1. So, Incoterm is required.", propertyInfo, errorMessage);

					ucc6ImportInvoiceHeaderValidationDeciderMock.Setup(x => x.IsRuleC0738Active).Returns(false);
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("RuleC0738 is disabled, JI_ValuationCodes is 1. So, Incoterm is required.", propertyInfo, errorMessage);

					invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._5;
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining("RuleC0738 is disabled, JI_ValuationCodes is 5. So, Incoterm is required.", propertyInfo, errorMessage);
				});
			}
		}

		public void TestCheckJZ_ValuationCode_MandatoryValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationCode = "";
			AssertHasMessageErrorContaining(invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_ValuationCode = "X";
			AssertNoMessageErrorContaining(invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJZ_ValuationCode_InvalidCodeValidation()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "Tran Nature");

			const string Direction = "Direction";
			const string Level = "Level";

			var cusCodeList1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "11", "11 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Direction, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Level, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			cusCodeList1.Attributes.AddNew(Direction, UniversalReferenceConstants.RefCusCodeListDirectionType.Import);
			cusCodeList1.Attributes.AddNew(Level, UniversalReferenceConstants.RefCusCodeListLevelType.Both);

			var cusCodeList2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "12", "12 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCodeList2.Attributes.AddNew(Direction, UniversalReferenceConstants.RefCusCodeListDirectionType.Import);
			cusCodeList2.Attributes.AddNew(Level, UniversalReferenceConstants.RefCusCodeListLevelType.Both);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];

			var invoiceHeader1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader1.Validation.ValidateJZ_ValuationCode();

			invoiceHeader1.JZ_ValuationCode = "31";
			AssertHasMessageError("A non-existent code should have a message error", invoiceHeader1.JZ_ValuationCodeInfo, ListValidation.InvalidCodeMessageError);

			invoiceHeader1.JZ_ValuationCode = "11";
			AssertNoMessageError("Valid codes should be found in the list.", invoiceHeader1.JZ_ValuationCodeInfo, ListValidation.InvalidCodeMessageError);

			var invoiceHeader2 = groupHeader.JobComInvoiceHeaders.AddNew();
			AssertEquals("Should default to '11'", "11", invoiceHeader2.JZ_ValuationCode);
			AssertNoMessageError(invoiceHeader2.JZ_ValuationCodeInfo, ListValidation.InvalidCodeMessageError);

			invoiceHeader2.JZ_ValuationCode = "31";
			AssertHasMessageError("A non-existent code should have a message error", invoiceHeader2.JZ_ValuationCodeInfo, ListValidation.InvalidCodeMessageError);

			invoiceHeader2.JZ_ValuationCode = "12";
			groupHeader.JobComInvoiceHeaders.AddNew();
			AssertEquals("Should default to '12'", "12", invoiceHeader2.JZ_ValuationCode);
		}

		public void TestCheckJZ_InvoiceNumber()
		{
			const string expectedWarning = "This invoice has no previous documents and not all of its invoice lines have one. Without a previous document the entry may be rejected.";
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			invoice.JZ_InvoiceNumber = "123";
			AssertHasWarningContaining("No previous documents at any level", invoice.JZ_InvoiceNumberInfo, expectedWarning);

			declaration.PreviousDocuments.AddNew();
			invoice.Validation.ValidateJZ_InvoiceNumber();
			AssertNoWarningContaining("One previous document at job level", invoice.JZ_InvoiceNumberInfo, expectedWarning);

			declaration.PreviousDocuments.RemoveAndDeleteAll();
			invoice.Validation.ValidateJZ_InvoiceNumber();
			AssertHasWarningContaining("No previous documents at any level (job level previous document has been deleted)", invoice.JZ_InvoiceNumberInfo, expectedWarning);

			invoice.PreviousDocuments.AddNew();
			invoice.Validation.ValidateJZ_InvoiceNumber();
			AssertNoWarningContaining("One previous document at invoice level", invoice.JZ_InvoiceNumberInfo, expectedWarning);

			invoice.PreviousDocuments.RemoveAndDeleteAll();
			invoice.Validation.ValidateJZ_InvoiceNumber();
			AssertHasWarningContaining("No previous documents at any level (invoice level previous document has been deleted)", invoice.JZ_InvoiceNumberInfo, expectedWarning);

			var invoiceLine1 = Factory.New<JobComInvLineForTest>();
			invoiceLine1.JI_JZ = invoice.PK;
			invoiceLine1.PreviousDocuments.AddNew();
			var invoiceLine2 = Factory.New<JobComInvLineForTest>();
			invoiceLine2.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Reload(true);
			invoiceLine2.PreviousDocuments.AddNew();
			invoice.Validation.ValidateJZ_InvoiceNumber();
			AssertNoWarningContaining("Two previous documents at invoice line level", invoice.JZ_InvoiceNumberInfo, expectedWarning);

			invoiceLine1.PreviousDocuments.RemoveAndDeleteAll();
			invoice.Validation.ValidateJZ_InvoiceNumber();
			AssertHasWarningContaining("One previous document at invoice line level", invoice.JZ_InvoiceNumberInfo, expectedWarning);

			invoiceLine2.PreviousDocuments.RemoveAndDeleteAll();
			invoice.Validation.ValidateJZ_InvoiceNumber();
			AssertHasWarningContaining("When No previous documents at any level (invoice line level previous documents have been deleted) and all invoice lines Should Check Missing Previous Documents", invoice.JZ_InvoiceNumberInfo, expectedWarning);

			using (invoiceLine1.SuspendCheckMissingPreviousDocuments())
			{
				invoice.Validation.ValidateJZ_InvoiceNumber();
				AssertHasWarningContaining("When No previous documents at any level (invoice line level previous documents have been deleted) and at least one of invoice line Should Check Missing Previous Documents", invoice.JZ_InvoiceNumberInfo, expectedWarning);

				using (invoiceLine2.SuspendCheckMissingPreviousDocuments())
				{
					invoice.Validation.ValidateJZ_InvoiceNumber();
					AssertNoWarningContaining("When No previous documents at any level (invoice line level previous documents have been deleted) and none of invoice line Should Check Missing Previous Documents", invoice.JZ_InvoiceNumberInfo, expectedWarning);
				}
			}

			invoiceLine1.PreviousDocuments.AddNew();
			invoice.PreviousDocuments.AddNew();
			invoice.Validation.ValidateJZ_InvoiceNumber();
			AssertNoWarningContaining("One invoice line is missing previous document but an invoice level previous document exists", invoice.JZ_InvoiceNumberInfo, expectedWarning);
		}

		public void TestShouldCheckMissingPreviousDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var validation = new JobComInvoiceHeaderValidationForTest(invoiceHeader);

			AssertEquals("When Invoice Header does not have any Invoice Line, ShouldCheckMissingPreviousDocuments", ZBool.True, validation.ShouldCheckMissingPreviousDocumentsExposed);

			var invoiceLine1 = Factory.New<JobComInvLineForTest>();
			invoiceLine1.JI_JZ = invoiceHeader.PK;
			var invoiceLine2 = Factory.New<JobComInvLineForTest>();
			invoiceLine2.JI_JZ = invoiceHeader.PK;
			invoiceHeader.InvoiceLines.Reload(true);

			CombineAssertions("[PRE-CONDITION] Assert ShouldCheckMissingPreviousDocuments are true for both Invoice Lines", () =>
			{
				AssertEquals("ShouldCheckMissingPreviousDocuments", ZBool.True, invoiceLine1.ShouldCheckMissingPreviousDocuments);
				AssertEquals("ShouldCheckMissingPreviousDocuments", ZBool.True, invoiceLine2.ShouldCheckMissingPreviousDocuments);
			});

			AssertEquals("When All invoice lines Should Check Missing Previous Documents, ShouldCheckMissingPreviousDocuments", ZBool.True, validation.ShouldCheckMissingPreviousDocumentsExposed);

			using (invoiceLine1.SuspendCheckMissingPreviousDocuments())
			{
				AssertEquals("[PRE-CONDITION] Invoice Line 1, ShouldCheckMissingPreviousDocuments", ZBool.False, invoiceLine1.ShouldCheckMissingPreviousDocuments);
				AssertEquals("When at least one invoice line Should Check Missing Previous Documents, ShouldCheckMissingPreviousDocuments", ZBool.True, validation.ShouldCheckMissingPreviousDocumentsExposed);

				using (invoiceLine2.SuspendCheckMissingPreviousDocuments())
				{
					AssertEquals("[PRE-CONDITION] Invoice Line 2, ShouldCheckMissingPreviousDocuments", ZBool.False, invoiceLine2.ShouldCheckMissingPreviousDocuments);
					AssertEquals("When none of the invoice lines Should Check Missing Previous Documents, ShouldCheckMissingPreviousDocuments", ZBool.False, validation.ShouldCheckMissingPreviousDocumentsExposed);
				}
			}
		}

		public void TestCheckRuleR0012AndC0002()
		{
			var messageErrorForInvoiceHeader = "[R0012 & C0002] If Nature Of transaction is not entered in all invoice lines, it must be entered in Invoice Header also.";
			var messageWarningForInvoiceHeader = "Invoice lines without value in Nature of Transaction will be mapped from the Invoice header value.";

			var exporterForInvoice = Factory.New<OrgHeader>();
			var exporterForInvoiceLine = Factory.New<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceheader1 = declaration.Invoices.AddNew();
			var invoiceheader2 = declaration.Invoices.AddNew();

			var invoiceLine1_1 = invoiceheader1.InvoiceLines.AddNew();
			var invoiceLine1_2 = invoiceheader1.InvoiceLines.AddNew();

			var invoiceLine2_1 = invoiceheader2.InvoiceLines.AddNew();
			var invoiceLine2_2 = invoiceheader2.InvoiceLines.AddNew();

			using (var context = new InvoiceHeaderValidationDeciderTestContext(declaration, isUCC6: true))
			{
				context.EnableRule(r => r.IsRuleC0002Active);
				context.EnableRule(r => r.IsRuleR0012Active);

				CleanupScenario();

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;

				RunScenario("Rules C0002 and R0012 are active, declaration is UCC6 and Export", false);

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

				RunScenario("Rules C0002 and R0012 are active, declaration is UCC6 and Import", true);

				context.DisableRule(r => r.IsRuleC0002Active);
				context.DisableRule(r => r.IsRuleR0012Active);

				CleanupScenario();

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;

				RunScenario("Rules C0002 and R0012 are inactive, declaration is UCC6 and Export", false);

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

				RunScenario("Rules C0002 and R0012 are inactive, declaration is UCC6 and Import", false);
			}

			using (var context = new InvoiceHeaderValidationDeciderTestContext(declaration, isUCC6: false))
			{
				context.EnableRule(r => r.IsRuleC0002Active);
				context.EnableRule(r => r.IsRuleR0012Active);

				CleanupScenario();

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;

				RunScenario("Rules C0002 and R0012 are active, declaration is not UCC6 and Export", false);

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

				RunScenario("Rules C0002 and R0012 are active, declaration is not UCC6 and Import", false);

				context.DisableRule(r => r.IsRuleC0002Active);
				context.DisableRule(r => r.IsRuleR0012Active);

				CleanupScenario();

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;

				RunScenario("Rules C0002 and R0012 are inactive, declaration is not UCC6 and Export", false);

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

				RunScenario("Rules C0002 and R0012 are inactive, declaration is not UCC6 and Import", false);
			}

			void CleanupScenario()
			{
				invoiceLine1_1.ZG_TransNature = ZString.Empty;
				invoiceLine1_2.ZG_TransNature = ZString.Empty;
				invoiceheader1.JZ_ValuationCode = ZString.Empty;
				invoiceLine2_1.ZG_TransNature = ZString.Empty;
				invoiceLine2_2.ZG_TransNature = ZString.Empty;
				invoiceheader2.JZ_ValuationCode = ZString.Empty;
			}

			void RunScenario(string scenarioTitle, bool negativeAwaitForFailure)
			{
				invoiceLine1_1.ZG_TransNature = "11";
				invoiceLine1_2.ZG_TransNature = "12";
				invoiceheader1.JZ_ValuationCode = "13";

				CombineAssertions($"{scenarioTitle}, invoiceHeader1 and both invoice lines have value for Transaction Nature", () =>
				{
					AssertNoMessageErrorContaining("InvoiceHeader1 error", invoiceheader1.JZ_ValuationCodeInfo, messageErrorForInvoiceHeader);
					AssertNoWarning("InvoiceHeader1 warning", invoiceheader1.JZ_ValuationCodeInfo, messageWarningForInvoiceHeader);
					AssertNoMessageErrorContaining("InvoiceHeader2 error", invoiceheader2.JZ_ValuationCodeInfo, messageErrorForInvoiceHeader);
					AssertNoWarning("InvoiceHeader2 warning", invoiceheader2.JZ_ValuationCodeInfo, messageWarningForInvoiceHeader);
				});

				invoiceLine1_1.ZG_TransNature = ZString.Empty;
				invoiceheader1.JZ_ValuationCode = ZString.Empty;

				CombineAssertions("Rules C0002 and R0012 are Active : declaration is UCC6 and Export, one invoice line and invoice header misses value for Transaction Nature", () =>
				{
					if (negativeAwaitForFailure)
					{
						AssertHasMessageErrorContaining("InvoiceHeader1 error", invoiceheader1.JZ_ValuationCodeInfo, messageErrorForInvoiceHeader);
					}
					else
					{
						AssertNoMessageErrorContaining("InvoiceHeader1 error", invoiceheader1.JZ_ValuationCodeInfo, messageErrorForInvoiceHeader);
					}
					AssertNoWarning("InvoiceHeader1 warning", invoiceheader1.JZ_ValuationCodeInfo, messageWarningForInvoiceHeader);
				});

				invoiceheader1.JZ_ValuationCode = "14";

				CombineAssertions("Rules C0002 and R0012 are Active : declaration is UCC6 and Export, invoiceHeader1 and one invoice line have value for Transaction Nature", () =>
				{
					AssertNoMessageErrorContaining("InvoiceHeader1 error", invoiceheader1.JZ_ValuationCodeInfo, messageWarningForInvoiceHeader);
					if (negativeAwaitForFailure)
					{
						AssertHasWarning("InvoiceHeader1 warning", invoiceheader1.JZ_ValuationCodeInfo, messageWarningForInvoiceHeader);
					}
					else
					{
						AssertNoWarning("InvoiceHeader1 warning", invoiceheader1.JZ_ValuationCodeInfo, messageWarningForInvoiceHeader);
					}
				});
			}
		}

		public void TestValidateHasAtLeastOneInvoiceSupportingDocument_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertValidateHasAtLeastOneInvoiceSupportingDocument(declaration);
		}

		public void TestValidateHasAtLeastOneInvoiceSupportingDocument_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			AssertValidateHasAtLeastOneInvoiceSupportingDocument(declaration);
		}

		void AssertValidateHasAtLeastOneInvoiceSupportingDocument(JobDeclaration declaration)
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeaderForTest>();
			declaration.Invoices.Add(invoiceHeader);

			var invoiceValidation = (JobComInvoiceHeaderValidationForTest)invoiceHeader.Validation;
			var expectedMessage = invoiceValidation.GetAtLeastOneSupportingDocumentErrorMessage();
			var notifications = invoiceHeader.Notifications.GetNotifications(CargoWise.EntityFramework.NotificationType.MessageError);

			if (invoiceHeader.AddingSupportingDocumentAutomaticallyEnabled)
			{
				invoiceValidation.SetShouldValidateNeedAtLeastOneInvoiceSupportingDocument(false);
				invoiceValidation.ValidateAll();
				AssertEquals("When HeaderValidation: False, The count of AtLeastOneSupportingDocumentMessage", 0, notifications.Count(x => x.Message.Contains(expectedMessage)));

				invoiceValidation.SetShouldValidateNeedAtLeastOneInvoiceSupportingDocument(true);
				invoiceValidation.SetShouldValidateNeedAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLine(false);
				CombineAssertions($"DeclarationType: {declaration.JE_MessageType}, HeaderValidation: True, InvoiceLines Validation: False", () =>
				{
					invoiceValidation.ValidateAll();
					Assert("No Supporting Document Added to the Header, Error message should be added", notifications.Any(x => x.Message.Contains(expectedMessage)));

					var supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
					supportingDocument.CSI_Code = "AAA";
					invoiceValidation.ValidateAll();
					Assert($"Supporting Document: {supportingDocument.CSI_Code}, Added to the Header, Error message should be found", notifications.Any(x => x.Message.Contains(expectedMessage)));

					var supportingTypes = invoiceHeader.SupportingDocuments.Helper.GetInvoiceSupportingDocumentTypes().ToArray();
					foreach (var supportingType in supportingTypes)
					{
						supportingDocument.CSI_Code = supportingType;
						invoiceValidation.ValidateAll();
						Assert($"Supporting Document: {supportingDocument.CSI_Code}, Added to the Header, Error message should not be found", !notifications.Any(x => x.Message.Contains(expectedMessage)));
					}
				});

				invoiceHeader.SupportingDocuments.RemoveAll();
				invoiceValidation.SetShouldValidateNeedAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLine(true);
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

				CombineAssertions($"DeclarationType: {declaration.JE_MessageType}, HeaderValidation: True, InvoiceLines Validation: True", () =>
				{
					invoiceValidation.ValidateAll();
					Assert("No Supporting Document Added to the Header and InvoiceLines, Error message should be added", notifications.Any(x => x.Message.Contains(expectedMessage)));

					var supportingDocumentInvLine1 = invoiceLine1.SupportingDocuments.AddNew();
					var supportingDocumentInvLine2 = invoiceLine2.SupportingDocuments.AddNew();

					supportingDocumentInvLine1.CSI_Code = "AAA";
					supportingDocumentInvLine2.CSI_Code = "AAA";
					invoiceValidation.ValidateAll();
					Assert($"Supporting Document: {supportingDocumentInvLine1.CSI_Code}, Added to all InvoiceLines, Error message should be found", notifications.Any(x => x.Message.Contains(expectedMessage)));

					var supportingTypes = invoiceHeader.SupportingDocuments.Helper.GetInvoiceSupportingDocumentTypes().ToArray();
					if (supportingTypes.Length > 0)
					{
						supportingDocumentInvLine1.CSI_Code = supportingTypes[0];
						invoiceValidation.ValidateAll();
						Assert($"Valid Supporting Document: {supportingDocumentInvLine1.CSI_Code}, Added to only one InvoiceLine, Error message should be found", notifications.Any(x => x.Message.Contains(expectedMessage)));
					}

					foreach (var supportingType in supportingTypes)
					{
						supportingDocumentInvLine1.CSI_Code = supportingType;
						supportingDocumentInvLine2.CSI_Code = supportingType;
						invoiceValidation.ValidateAll();
						Assert($"Supporting Document: {supportingDocumentInvLine1.CSI_Code}, Added to all InvoiceLines, Error message should not be found", !notifications.Any(x => x.Message.Contains(expectedMessage)));
					}
				});
			}
			else
			{
				Assert("When AddingSupportingDocumentAutomatically feature is disabled don't need this test", true);
			}
		}

		public void TestJZ_OA_SupplierAddressNotEmptyWarning()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.SupplierOrgPK = ZGuid.NewZGuid();
			invoiceHeader.JZ_OA_SupplierAddress = ZGuid.Empty;
			invoiceHeader.Validation.ValidateJZ_OA_SupplierAddress();

			CombineAssertions(() =>
			{
				AssertHasWarning(invoiceHeader.JZ_OA_SupplierAddressInfo, "Supplier organization will not be saved because no address is selected.");

				invoiceHeader.JZ_OA_SupplierAddress = ZGuid.NewZGuid();
				AssertNoWarning(invoiceHeader.JZ_OA_SupplierAddressInfo, "Supplier organization will not be saved because no address is selected.");

				invoiceHeader.SupplierOrgPK = ZGuid.Empty;
				invoiceHeader.JZ_OA_SupplierAddress = ZGuid.Empty;
				AssertNoWarning(invoiceHeader.JZ_OA_SupplierAddressInfo, "Supplier organization will not be saved because no address is selected.");
			});
		}

		class JobComInvoiceHeaderValidationForTest : JobComInvoiceHeaderValidation
		{
			public JobComInvoiceHeaderValidationForTest(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
			{
			}

			public ZBool ShouldCheckMissingPreviousDocumentsExposed => ShouldCheckMissingPreviousDocuments;

			protected override bool ShouldValidateNeedAtLeastOneInvoiceSupportingDocument => shouldValidateNeedAtLeastOneInvoiceSupportingDocument;
			bool shouldValidateNeedAtLeastOneInvoiceSupportingDocument = true;

			protected override bool ShouldValidateNeedAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLine => shouldValidateNeedAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLine;
			bool shouldValidateNeedAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLine = true;

			public void SetShouldValidateNeedAtLeastOneInvoiceSupportingDocument(bool value)
			{
				shouldValidateNeedAtLeastOneInvoiceSupportingDocument = value;
			}

			public void SetShouldValidateNeedAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLine(bool value)
			{
				shouldValidateNeedAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLine = value;
			}

			public ZString GetAtLeastOneSupportingDocumentErrorMessage()
			{
				return "At least one Supporting Document of type: " + GetSupportingDocumentTypesString() + " must be present at the Invoice Header level or in all of its Invoice Lines.";

				string GetSupportingDocumentTypesString()
				{
					var supportingDocumentArray = Parent.SupportingDocuments.Helper.GetInvoiceSupportingDocumentTypes().ToArray();

					if (supportingDocumentArray.Length == 1)
					{
						return supportingDocumentArray[0];
					}

					if (supportingDocumentArray.Length > 1)
					{
						return new ZStringBuilder(string.Join(", ", supportingDocumentArray.Take(supportingDocumentArray.Length - 1)))
						   .Append(" or ")
						   .Append(supportingDocumentArray[supportingDocumentArray.Length - 1])
						   .ToString();
					}

					return string.Empty;
				}
			}
		}

		class JobComInvoiceHeaderForTest : JobComInvoiceHeader
		{
			public JobComInvoiceHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
			{
				return new JobComInvoiceHeaderValidationForTest(this);
			}
		}
	}
}
