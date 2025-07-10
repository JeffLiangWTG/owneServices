using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class JobComInvoiceHeaderValidationTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderValidationTest
	{
		public void TestCheckJZ_ValuationCode_RuleC0627()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine2.JI_CEI = entryInstruction2.PK;

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "Tran Nature");

			const string Direction = "Direction";
			const string Level = "Level";

			var cusCodeList1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "11", "11 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Direction, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Level, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			cusCodeList1.Attributes.AddNew(Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);
			cusCodeList1.Attributes.AddNew(Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Both);
			Factory.Save();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
				invoice.JZ_ValuationCode = "11";
				AssertNoMessageErrors("Rule C0627 does not apply to FR invoice header", invoice.JZ_ValuationCodeInfo);

				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
				invoiceLine1.ZG_TransNature = "11";
				invoice.JZ_ValuationCode = ZString.Empty;
				AssertNoMessageErrors("Rule C0627 does not apply to FR invoice header", invoice.JZ_ValuationCodeInfo);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
				invoice.JZ_ValuationCode = "11";
				AssertNoMessageErrors("Rule C0627 does not apply to FR invoice header", invoice.JZ_ValuationCodeInfo);

				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
				invoiceLine1.ZG_TransNature = "11";
				invoice.JZ_ValuationCode = ZString.Empty;
				AssertNoMessageErrors("Rule C0627 does not apply to FR invoice header", invoice.JZ_ValuationCodeInfo);
			}
		}

		public void TestCheckJZ_IncoTermPlace_UCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoiceHeader1 = declaration.Invoices.AddNew();

			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";

			var cei = declaration.CustomsEntryInstructions.AddNew();
			var cei2 = declaration.CustomsEntryInstructions.AddNew();

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceHeader3 = declaration.Invoices.AddNew();
			var invoiceHeader1ForTest = new JobComInvoiceHeaderValidation(invoiceHeader1);
			var invoiceHeader2ForTest = new JobComInvoiceHeaderValidation(invoiceHeader2);
			var invoiceHeader3ForTest = new JobComInvoiceHeaderValidation(invoiceHeader3);
			invoiceHeader1ForTest.ValidateJZ_InvoiceNumber();
			invoiceHeader2ForTest.ValidateJZ_InvoiceNumber();
			invoiceHeader3ForTest.ValidateJZ_InvoiceNumber();

			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = cei.PK;

			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = cei.PK;

			var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = cei2.PK;

			var shutUp = new SendsMessagesToCustomsShutterUpperer();

			var mergeResult = declaration.DoMerge(shutUp);

			Assert("Merge failed", mergeResult);

			invoiceHeader1.JZ_IncoTermPlace = "ENTRY1";
			invoiceHeader2.JZ_IncoTermPlace = "ENTRY1";
			AssertNoMessageError(invoiceHeader2.JZ_IncoTermPlaceInfo, "All Agreed places of an Entry must be equal");

			invoiceHeader2.JZ_IncoTermPlace = "ENTRY1ERR";
			AssertHasMessageError(invoiceHeader2.JZ_IncoTermPlaceInfo, "All Agreed places of an Entry must be equal");

			invoiceHeader2.JZ_IncoTermPlace = "ENTRY1";
			invoiceHeader3.JZ_IncoTermPlace = "ENTRY2";
			AssertNoMessageError(invoiceHeader3.JZ_IncoTermPlaceInfo, "All Agreed places of an Entry must be equal");

			invoiceHeader1.JZ_IncoTerm = ZString.Empty;
			invoiceHeader1.ZG_AgreedPlaceCode = "SYD";
			invoiceHeader1.JZ_IncoTermPlace = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceHeader1.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoiceHeader1.ZG_AgreedPlaceCode = ZString.Empty;
			invoiceHeader1.JZ_IncoTermPlace = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceHeader1.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoiceHeader1.ZG_AgreedPlaceCode = "SYD";
			invoiceHeader1.JZ_IncoTermPlace = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceHeader1.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader1.ZG_AgreedPlaceCode = "SYD";
			invoiceHeader1.JZ_IncoTermPlace = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceHeader1.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoiceHeader1.JZ_IncoTermPlace = "YYY";
			AssertNoMessageErrorContaining(invoiceHeader1.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader1.ZG_AgreedPlaceCode = ZString.Empty;
			invoiceHeader1.JZ_IncoTermPlace = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceHeader1.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader1.ZG_AgreedPlaceCode = ZString.Empty;
			invoiceHeader1.JZ_IncoTermPlace = "YYY";
			AssertNoMessageErrorContaining(invoiceHeader1.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJZ_IncoTermPlace_NotUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var invoiceHeader1 = declaration.Invoices.AddNew();

			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";

			invoiceHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoiceHeader1.JZ_IncoTermPlace = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceHeader1.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader1.JZ_IncoTerm = string.Empty;
			invoiceHeader1.JZ_IncoTermPlace = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceHeader1.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJZ_AdditionalTerms()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoiceHeader = declaration.Invoices.AddNew();

			AssertNoMessageErrorContaining(invoiceHeader.JZ_AdditionalTermsInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_AdditionalTerms = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_AdditionalTermsInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoiceHeader.JZ_AdditionalTerms = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_AdditionalTermsInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoiceHeader.JZ_AdditionalTerms = "BBB";
			AssertNoMessageErrorContaining(invoiceHeader.JZ_AdditionalTermsInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJZ_InvoiceNumber()
		{
			const string expectedWarning = "This invoice has no previous documents and not all of its invoice lines have one. Without a previous document the entry may be rejected.";
			var dec = Factory.New<JobDeclaration>();
			var cei = dec.CustomsEntryInstructions[0];
			cei.CEI_Style = "BBB";

			var inv = dec.Invoices.AddNew();
			inv.JZ_InvoiceAmount = 100;

			CombineAssertions(() =>
			{
				inv.Validation.ValidateJZ_InvoiceNumber();
				AssertHasWarningContaining("Warning", inv.JZ_InvoiceNumberInfo, expectedWarning);

				inv.PreviousDocuments.AddNew();
				inv.Validation.ValidateJZ_InvoiceNumber();
				AssertNoWarningContaining("No warning", inv.JZ_InvoiceNumberInfo, expectedWarning);
			});
		}

		public void TestCheckZGValuationMethod()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			AssertNoMessageError(invoice.ZG_ValuationMethodInfo, ListValidation.InvalidCodeMessageError);

			invoice.ZG_ValuationMethod = ZString.Empty;
			invoice.AddInfoValidation.ValidateZG_ValuationMethod();
			AssertHasMessageError(invoice.ZG_ValuationMethodInfo, "You have not entered a value.");

			invoice.ZG_ValuationMethod = "9";
			invoice.AddInfoValidation.ValidateZG_ValuationMethod();
			AssertHasMessageError(invoice.ZG_ValuationMethodInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJZ_ValuationCode_MandatoryValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationCode = "";
			AssertNoMessageErrors("IsJZ_ValuationCodeMandatory is false in FR, so there should be no error.", invoice.JZ_ValuationCodeInfo);
		}

		public void TestJZ_IncoTerm_RuleC0729()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			const string errorMessage = "Please enter an Incoterm";

			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertNoMessageErrorContaining("DeltaG declaration, Incoterm is NOT empty, So there is no error message", invoiceHeader.JZ_IncoTermInfo, errorMessage);

			invoiceHeader.JZ_IncoTerm = ZString.Empty;
			AssertHasMessageErrorContaining("DeltaG declaration, Incoterm is Empty, So there should be error message", invoiceHeader.JZ_IncoTermInfo, errorMessage);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertHasMessageErrorContaining("DeltaIE declaration, Incoterm is Empty, So there should be error message", invoiceHeader.JZ_IncoTermInfo, errorMessage);

			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertNoMessageErrorContaining("DeltaIE declaration, Incoterm is NOT Empty, So there is no error message", invoiceHeader.JZ_IncoTermInfo, errorMessage);
		}

		public void TestCheckJZ_Calc_BalanceCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var header = declaration.Invoices.AddNew();

			var validation = new JobComInvoiceHeaderValidation(header);
			AssertEquals("IsRuleTNAT_078Active should be true when declaration.IsUCC6AndIsImport == true", true, ((IInvoiceHeaderValidationDecider)validation.ValidationDecider).IsRuleTNAT_078Active);

			header.JZ_InvoiceAmount = 100m;
			var line = header.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 110m;

			declaration.ResumeApportionment();
			AssertHasMessageError("Conditions not met, base logic should run and message error need to be raised", header.JZ_Calc_BalanceInfo, InvoiceHeaderValidation.UnbalancedInvoiceMessage);

			line.EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
			declaration.ResumeApportionment();
			AssertHasMessageError("Conditions not met, base logic should run and message error need to be raised", header.JZ_Calc_BalanceInfo, InvoiceHeaderValidation.UnbalancedInvoiceMessage);

			declaration.AdditionalInfos.AddNew().CSI_Code = FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode;
			declaration.ResumeApportionment();
			AssertNoMessageError("Base logic should not run when IsRuleTNAT_078Active is true, CEI_SubStyle is Y (or U) and CSI_Code is 1AVP.", header.JZ_Calc_BalanceInfo, InvoiceHeaderValidation.UnbalancedInvoiceMessage);

			line.EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationOfSimplifiedDeclarationsCoveredByCAndF;
			declaration.ResumeApportionment();
			AssertNoMessageError("Base logic should not run when IsRuleTNAT_078Active is true, CEI_SubStyle is Y (or U) and CSI_Code is 1AVP.", header.JZ_Calc_BalanceInfo, InvoiceHeaderValidation.UnbalancedInvoiceMessage);

			line.EntryInstruction.CEI_SubStyle = "A";
			declaration.ResumeApportionment();
			AssertHasMessageError("CEI_SubStyle is not Y (or U), base logic should run and message error need to be raised", header.JZ_Calc_BalanceInfo, InvoiceHeaderValidation.UnbalancedInvoiceMessage);

			line.EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationOfSimplifiedDeclarationsCoveredByCAndF;
			declaration.AdditionalInfos.RemoveAll();
			declaration.AdditionalInfos.AddNew().CSI_Code = "0001";
			declaration.ResumeApportionment();
			AssertHasMessageError("CSI_Code is not 1AVP, base logic should run and message error need to be raised", header.JZ_Calc_BalanceInfo, InvoiceHeaderValidation.UnbalancedInvoiceMessage);

			using var context = new JobComInvoiceHeaderValidationDeciderTestContext(header, true);
			context.DisableRule(x => x.IsRuleTNAT_078Active);
			AssertEquals("IsRuleTNAT_078Active should be false", false, ((IInvoiceHeaderValidationDecider)validation.ValidationDecider).IsRuleTNAT_078Active);

			declaration.ResumeApportionment();
			AssertHasMessageError("IsRuleTNAT_078Active is false or null, base logic should run and message error need to be raised", header.JZ_Calc_BalanceInfo, InvoiceHeaderValidation.UnbalancedInvoiceMessage);
		}

		public void TestCheckJZ_InvoiceAmount()
		{
			var message = "[NAT_240] Invoice Amount must be equal to 0 EUR when Additional Code 0097 (free goods) is selected in one of the Invoice Lines.";

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			var context = new InvoiceHeaderValidationTestContext(invoice);
			context.EnableRule(x => x.IsRuleNAT_240Active);

			invoiceLine1.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.FreeGoodsSupplementaryCode;
			invoice.JZ_InvoiceAmount = 10.00;
			invoice.Validation.ValidateJZ_InvoiceAmount();
			Assert("Prerequisite: HasFreeGoods should be true as one of the InvoiceLines has supplementary code equal to 0097.", invoiceLine1.HasFreeGoods);
			Assert("Prerequisite: HasFreeGoods should be true if any associated InvoiceLine has a supplementary code equal to 0097.", invoice.HasFreeGoods);
			AssertHasMessageError("A message error is expected when Invoice amount > 0 and invoice has free goods.", invoice.JZ_InvoiceAmountInfo, message);

			context.DisableRule(x => x.IsRuleNAT_240Active);
			invoice.Validation.ValidateJZ_InvoiceAmount();
			AssertNoMessageError("No message error is expected when rule NAT_240 is disabled.", invoice.JZ_InvoiceAmountInfo, message);

			context.EnableRule(x => x.IsRuleNAT_240Active);
			invoiceLine2.JI_SupplementaryCode2 = FRConstants.SupplementaryCodes.FreeGoodsSupplementaryCode;
			invoice.Validation.ValidateJZ_InvoiceAmount();
			Assert("Prerequisite: HasFreeGoods should be true as InvoiceLine has supplementary code equal to 0097.", invoiceLine2.HasFreeGoods);
			Assert("Prerequisite: HasFreeGoods should be true if any associated InvoiceLine has a supplementary code equal to 0097.", invoice.HasFreeGoods);
			AssertHasMessageError("A message error is expected when Invoice amount > 0 and invoice has free goods.", invoice.JZ_InvoiceAmountInfo, message);

			invoice.JZ_InvoiceAmount = 0.00;
			invoice.Validation.ValidateJZ_InvoiceAmount();
			AssertNoMessageError("No message error is expected when total invoice amount is 0.", invoice.JZ_InvoiceAmountInfo, message);

			invoice.JZ_InvoiceAmount = 1.00;
			invoiceLine1.JI_SupplementaryCode1 = ZString.Empty;
			invoiceLine2.JI_SupplementaryCode2 = ZString.Empty;
			invoice.Validation.ValidateJZ_InvoiceAmount();
			Assert("Prerequisite: HasFreeGoods should be false as no InvoiceLine has no supplementaryCode equal to 0097.", !invoiceLine1.HasFreeGoods);
			Assert("Prerequisite: HasFreeGoods should be false if no InvoiceLine has supplementaryCode equal to 0097.", !invoice.HasFreeGoods);
			AssertNoMessageError("No message error is expected when invoice has no free goods.", invoice.JZ_InvoiceAmountInfo, message);
		}

		protected override Type GetTypeForTest() => ((JobDeclaration)declaration).IsUCC6
			? typeof(DeltaIEJobComInvoiceHeaderValidation)
			: typeof(DeltaGJobComInvoiceHeaderValidation);
	}
}
