using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.EU;
using Enterprise.ZArchitecture.Schema;
using YesNoList = Enterprise.Customs.Universal.CodeDescriptionPairLists.YesNoList;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class JobDeclarationValidationBaseOnlyTest : TestCaseWithFactory
	{
		public void TestCheckJE_RL_NKPortOfLoading()
		{
			var expectedError = "This port is in the EU. Please, check that is correct.";
			var loader = new RefUNLOCO.Loader(Factory);
			var sydney = loader.Load("AUSYD");
			var aglona = loader.Load("LVAGL");
			var rotterdam = loader.Load("NLRTM");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;

			CombineAssertions("Import and EntryStyle IM", () =>
			{
				declaration.JE_RL_NKPortOfLoading = "AUSYD";
				AssertNoWarningContaining("Foreign Port", declaration.JE_RL_NKPortOfLoadingInfo, expectedError);

				declaration.JE_RL_NKPortOfLoading = "LVAGL";
				AssertHasWarningContaining("Local Port is in EU", declaration.JE_RL_NKPortOfLoadingInfo, expectedError);

				declaration.JE_RL_NKPortOfLoading = "NLRTM";
				AssertHasWarningContaining("EU Port and CT Status != T1", declaration.JE_RL_NKPortOfLoadingInfo, expectedError);

				declaration.ZG_CTStatusID = "T1";
				AssertNoWarningContaining("EU Port and CT Status is T1", declaration.JE_RL_NKPortOfLoadingInfo, expectedError);

				declaration.ZG_CTStatusID = "AH";
				AssertHasWarningContaining("EU and CT Status != T1", declaration.JE_RL_NKPortOfLoadingInfo, expectedError);

				declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromEFTAMember;
				AssertNoWarningContaining("EntryStyle != IM", declaration.JE_RL_NKPortOfLoadingInfo, expectedError);
			});
		}

		public void TestValidateWarehouseDocAddressForeignKey()
		{
			var expectedMessageError = JobDeclarationValidation.ValidationMessageForWhenCpcDemandWarehouse;

			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "44", "44", "444", "Four", "EXP", group: "EFD");
			procedure1.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, "A", "66", "66", "666", "Six", "EXP", group: "EFD");
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsWarehouseClient = true;

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "EFD";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = procedure1.FullCodeCurrentPlusPreviousPlusConcession;

			CombineAssertions(() =>
			{
				declaration.WarehouseDocAddress.Validation.ValidateE2_OA_Address();
				AssertNoMessageError("E2_OA_Address is empty and BondedWarehouseEditable is false and IsWarehouseNeeded is true", declaration.WarehouseDocAddress.E2_OA_AddressInfo, expectedMessageError);

				var declarationForTest = Factory.New<JobDeclarationForTest>();
				var entryInstructionForTest = declarationForTest.CustomsEntryInstructions.AddNew();
				entryInstructionForTest.CEI_Style = "EFD";
				var invoiceHeaderForTest = declarationForTest.Invoices.AddNew();
				var invoiceLineForTest = invoiceHeaderForTest.InvoiceLines.AddNew();
				invoiceLineForTest.JI_Procedure = procedure1.FullCodeCurrentPlusPreviousPlusConcession;

				declarationForTest.WarehouseDocAddress.Validation.ValidateE2_OA_Address();
				AssertHasMessageError("E2_OA_Address is empty and BondedWarehouseEditable is true and IsWarehouseNeeded is true", declarationForTest.WarehouseDocAddress.E2_OA_AddressInfo, expectedMessageError);

				declarationForTest.WarehouseDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;
				AssertNoMessageError("E2_OA_Address isn't empty and BondedWarehouseEditable is true and IsWarehouseNeeded is true", declarationForTest.WarehouseDocAddress.E2_OA_AddressInfo, expectedMessageError);

				invoiceLineForTest.JI_Procedure = procedure2.FullCodeCurrentPlusPreviousPlusConcession;
				declarationForTest.WarehouseDocAddress.E2_OA_Address = ZGuid.Empty;
				AssertNoMessageError("E2_OA_Address is empty and BondedWarehouseEditable is true and IsWarehouseNeeded is false", declarationForTest.WarehouseDocAddress.E2_OA_AddressInfo, expectedMessageError);
			});
		}

		public void TestCheckJE_TransportIDInland_ForSeaInland_ImoShipIdentificationNumber()
		{
			const string expectedMessageError = "IMO number must be 7 characters in length";
			var declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
				declaration.JE_TransportMeans = TransportMeansList.Codes.NameOfTheSeaGoingVessel;
				declaration.JE_TransportIDInland = "123456";
				AssertNoWarning("Sea - 11", declaration.JE_TransportIDInlandInfo, expectedMessageError);
				declaration.JE_TransportMeans = TransportMeansList.Codes.ImoShipIdentificationNumber;
				declaration.Validation.ValidateJE_TransportIDInland();
				AssertHasWarning("Sea - 10", declaration.JE_TransportIDInlandInfo, expectedMessageError);
				declaration.JE_TransportIDInland = "1234567";
				AssertNoWarnings("Valid Lloyds", declaration.JE_TransportIDInlandInfo);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Air;
				declaration.JE_TransportMeans = TransportMeansList.Codes.ImoShipIdentificationNumber;
				declaration.JE_TransportIDInland = "123456";
				AssertNoWarning("Not Sea - 10", declaration.JE_TransportIDInlandInfo, expectedMessageError);
			});
		}

		public void TestGuaranteeValidation_NoCustomsGuarantee()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts).CF_ChargeAmount = 300m;

			declaration.Validation.ValidateJE_PaymentMethod();
			AssertEquals("CustomsGuarantee null, Import, customsGuaranteeAmount < amountToGuarantee", false, declaration.JE_PaymentMethodInfo.Notifications.Any(x => x.Message.Contains("The remaining balance of this guarantee is")));
		}

		public void TestGuaranteeValidation_HasCustomsGuarantee()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "Ye";
			procedure.ZZ6_PreviousProcedureCode = "12";
			procedure.ZZ6_ShipmentType = MessageTypeList.Codes.Export;
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			procedure.ZZ6_ZZZ_NKDataGrouping = currentCountry;
			procedure.ZZ6_Concession = "367";
			procedure.ZZ6_Description = "description";

			var procedure2 = Factory.New<RefCusProcedure>();
			procedure2.ZZ6_ProcedureCode = "Im";
			procedure2.ZZ6_PreviousProcedureCode = "12";
			procedure2.ZZ6_ShipmentType = MessageTypeList.Codes.Import;
			procedure2.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			procedure2.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			procedure2.ZZ6_ZZZ_NKDataGrouping = currentCountry;
			procedure2.ZZ6_Concession = "367";
			procedure2.ZZ6_Description = "description";

			var dty = helper.CreateNewOrGetExistingRateType(currentCountry, Universal.Constants.RateTypes.Duty, "DTY");
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dty.PK);
			Factory.Save();

			const string message = "The remaining balance of this guarantee is";
			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = "Ye12367";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts).CF_ChargeAmount = 300m;

			var jpyCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "JPY");
			SetExchangeRate(declaration.Factory, jpyCurrency, 100, ZDateTime.Today, Core.Constants.ExchangeRateTypes.Code.CustomsRate);

			declaration.Validation.ValidateJE_PaymentMethod();
			AssertNoMessageErrorContaining("Export, customsGuaranteeAmount < amountToGuarantee", declaration.JE_PaymentMethodInfo, message);

			var declarationImp = Factory.New<JobDeclarationForTest>();
			declarationImp.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declarationImp.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "Im12367";

			var entryImp = declarationImp.CustomsEntryHeaders.AddNew();
			var entryLineImp = entryImp.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLineImp.PK;
			entryLineImp.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts).CF_ChargeAmount = 300m;

			jpyCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "JPY");
			SetExchangeRate(declarationImp.Factory, jpyCurrency, 100, ZDateTime.Today, Core.Constants.ExchangeRateTypes.Code.CustomsRate);

			CombineAssertions(() =>
			{
				declarationImp.Validation.ValidateJE_PaymentMethod();
				AssertHasMessageErrorContaining("Import, customsGuaranteeAmount < amountToGuarantee", declarationImp.JE_PaymentMethodInfo, message);

				var customsGuarantee = declarationImp.CustomsGuarantee;
				var lineTrans = customsGuarantee.CusGuaranteeLineTransactions.AddNew();
				lineTrans.CPL_TranValue = 300m;
				declarationImp.Validation.ValidateJE_PaymentMethod();
				AssertNoMessageErrorContaining("Import, customsGuaranteeAmount = amountToGuarantee", declarationImp.JE_PaymentMethodInfo, message);

				lineTrans.CPL_TranValue = 400m;
				declarationImp.Validation.ValidateJE_PaymentMethod();
				AssertNoMessageErrorContaining("Import, customsGuaranteeAmount > amountToGuarantee", declarationImp.JE_PaymentMethodInfo, message);

				customsGuarantee.CPH_UnitOfMeasure = "JPY";
				declarationImp.Validation.ValidateJE_PaymentMethod();
				AssertHasMessageErrorContaining("Currency conversion", declarationImp.JE_PaymentMethodInfo, message);
			});
		}

		public void TestGuaranteeValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "Ye";
			procedure.ZZ6_PreviousProcedureCode = "12";
			procedure.ZZ6_ShipmentType = MessageTypeList.Codes.Import;
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			procedure.ZZ6_ZZZ_NKDataGrouping = currentCountry;
			procedure.ZZ6_Concession = "367";
			procedure.ZZ6_Description = "description";

			var dty = helper.CreateNewOrGetExistingRateType(currentCountry, Universal.Constants.RateTypes.Duty, "DTY");
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dty.PK);
			Factory.Save();

			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "Ye12367";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var feesduty = entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);
			feesduty.CF_ChargeAmount = 300m;
			var feesVat = entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.Vat);
			feesVat.CF_ChargeAmount = 100m;
			var amountToGuarantee = new Money(400.00m, RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.EuropeanUnion));
			AssertEquals(amountToGuarantee, declaration.AmountToGuarantee);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_PaymentMethod();
			AssertNoMessageError("No message error on guarantees for export", declaration.JE_PaymentMethodInfo, "The remaining balance of this guarantee is 0,00 EUR but the open entries on this declaration require 400,00 EUR");

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_PaymentMethod();
			AssertHasMessageError("Unbalanced", declaration.JE_PaymentMethodInfo, "The remaining balance of this guarantee is 0,00 EUR but the open entries on this declaration require 400,00 EUR");
		}

		public void TestCheckJE_ShipmentIncoTermPlace_RequiredForUCC6_XXX()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_ShipmentIncoTerm = Enterprise.Core.Constants.IncoTerms.Other;
				ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_ShipmentIncoTermPlaceInfo);
			}

			declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_ShipmentIncoTerm = Enterprise.Core.Constants.IncoTerms.CostAndFreight;
				declaration.EUD_AgreedPlaceCode = Core.Constants.CountryCodes.Germany;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_ShipmentIncoTermPlaceInfo);

				declaration.JE_ShipmentIncoTerm = Enterprise.Core.Constants.IncoTerms.CostAndFreight;
				declaration.EUD_AgreedPlaceCode = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_ShipmentIncoTermPlaceInfo);

				declaration.JE_ShipmentIncoTerm = Enterprise.Core.Constants.IncoTerms.Other;
				declaration.EUD_AgreedPlaceCode = ZString.Empty;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_ShipmentIncoTermPlaceInfo);

				declaration.JE_ShipmentIncoTerm = Enterprise.Core.Constants.IncoTerms.Other;
				declaration.EUD_AgreedPlaceCode = Core.Constants.CountryCodes.Germany;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_ShipmentIncoTermPlaceInfo);
			}
		}

		public void TestCheckJE_ShipmentIncoTerm_IsRuleC0729Active()
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

			var propertyInfo = declaration.JE_ShipmentIncoTermInfo;
			const string errorMessage = "You have not entered a [20.1] Incoterm";

			using (var context = new DeclarationValidationDeciderTestContext(declaration))
			{
				CombineAssertions(() =>
				{
					context.EnableRule(r => r.IsRuleC0729Active);
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("RuleC0729 is enabled, Substlyle A, Procedure 71, So, Incoterm is optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._07;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertHasMessageErrorContaining("RuleC0729 is enabled, Substlyle A, Procedure 07, So, Incoterm is required.", propertyInfo, errorMessage);

					context.DisableRule(r => r.IsRuleC0729Active);
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("RuleC0729 is disabled, Substlyle A, Procedure 07, So, Incoterm is optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("RuleC0729 is disabled, Substlyle A, Procedure 71, So, Incoterm is optional.", propertyInfo, errorMessage);
				});
			}
		}

		public void TestCheckJE_ShipmentIncoTerm_WhenNoEntryInstructionAdded_RuleC0729()
		{
			const string errorMessage = "You have not entered a [20.1] Incoterm";
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions(() =>
				{
					var propertyInfo = declaration.JE_ShipmentIncoTermInfo;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertHasMessageErrorContaining("Entry Instruction not present, Incoterm fields are required.", propertyInfo, errorMessage);

					var customEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
					invoiceLine.JI_CL = customEntryInstruction.PK;
					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;

					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("Entry Instruction present, substyle is A, procedure is 71 => Incoterm fields are optional.", propertyInfo, errorMessage);
				});
			}
		}

		public void TestCheckJE_ShipmentIncoTerm_RuleC0729()
		{
			const string errorMessage = "You have not entered a [20.1] Incoterm";
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
					var propertyInfo = declaration.JE_ShipmentIncoTermInfo;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertHasMessageErrorContaining("Incoterm fields are required.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("Substyle is A, procedure is 71, Incoterm fields are optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("Substyle is D, procedure is 71, Incoterm fields are optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("Substyle is Y, procedure is 71, Incoterm fields are optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._53;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("substyle is A, procedure is 53, Incoterm fields are optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._53;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("Substyle is D, procedure is 53, Incoterm fields are optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._53;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("Substyle is Y, procedure is 53, Incoterm fields are optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationUnderTheProcedureCoveredUnderArticle182OfTheCode;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._53;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertHasMessageErrorContaining("Substyle is V, procedure is 53, Incoterm fields are required.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._48;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertHasMessageErrorContaining("Substyle is Y, procedure is 48, Incoterm fields are required.", propertyInfo, errorMessage);
				});
			}
		}

		public void TestCheckJE_ShipmentIncoTerm_DeclarationMessageTypeAndUcc6_RuleC0729()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var customEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = customEntryInstruction.PK;

			customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
			const string errorMessage = "You have not entered a [20.1] Incoterm";
			var propertyInfo = declaration.JE_ShipmentIncoTermInfo;

			CombineAssertions(() =>
			{
				declaration.Validation.ValidateJE_ShipmentIncoTerm();
				AssertNoMessageErrorContaining("Declaration is Non-UCC6 Import, substyle is A and procedure is 71, Incoterm is optional.", propertyInfo, errorMessage);

				customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._01;
				declaration.Validation.ValidateJE_ShipmentIncoTerm();
				AssertNoMessageErrorContaining("Declaration is Non-UCC6 Import, substyle is A and procedure is 01, Incoterm is optional.", propertyInfo, errorMessage);

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("Declaration is UCC6 Import, substyle is A and procedure is 71, Incoterm is optional.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertHasMessageErrorContaining("Declaration is UCC6 Import, substyle is B and procedure is 71, Incoterm section is required.", propertyInfo, errorMessage);

					customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
					customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._01;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertHasMessageErrorContaining("Declaration is UCC6 Import, substyle is A and procedure is 01, Incoterm section is required.", propertyInfo, errorMessage);

					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("Declaration is UCC6 Export, substyle is A and procedure is 01, Incoterm section is optional.", propertyInfo, errorMessage);
				}
				customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
				declaration.Validation.ValidateJE_ShipmentIncoTerm();
				AssertNoMessageErrorContaining("Declaration is Non-UCC6 Export, substyle is A and procedure is 71, Incoterm section is optional.", propertyInfo, errorMessage);
			});
		}

		public void TestCheckJE_ShipmentIncoTerm_WithMultipleEntryInstructions_RuleC0729()
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

			const string errorMessage = "You have not entered a [20.1] Incoterm";
			var propertyInfo = declaration.JE_ShipmentIncoTermInfo;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions(() =>
				{
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertHasMessageErrorContaining("Both CustomEntryInstructions have substyle Null and procedures Null, Incoterm is required.", propertyInfo, errorMessage);

					customEntryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
					customEntryInstruction1.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;

					customEntryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
					customEntryInstruction2.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._42;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertHasMessageErrorContaining("CustomEntryInstruction2 has substyles NOT in [A, D, Y] and procedure NOT in [71, 53], so Incoterm is required.", propertyInfo, errorMessage);

					customEntryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
					customEntryInstruction2.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._53;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("Both CustomEntryInstruction2 have substyles in [A, D, Y] and procedure in [71, 53], so Incoterm is optional.", propertyInfo, errorMessage);

					customEntryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationOfSimplifiedDeclarationsCoveredByCAndF;
					customEntryInstruction1.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertHasMessageErrorContaining("CustomEntryInstruction1 has substyles NOT in [A, D, Y] and procedure is 71, so Incoterm is required.", propertyInfo, errorMessage);
				});
			}
		}

		public void TestCheckJE_ShipmentIncoTerm_RuleC0738()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

			var customEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;

			const string errorMessage = "You have not entered a [20.1] Incoterm";
			var propertyInfo = declaration.JE_ShipmentIncoTermInfo;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions(() =>
				{
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("No InvoiceLine present, So Incoterm is optional", propertyInfo, errorMessage);

					var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
					invoiceLine1.JI_CL = customEntryInstruction.PK;
					invoiceLine1.JI_ValuationCode = ValuationMethodList.Codes._1;

					var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
					invoiceLine2.JI_ValuationCode = ValuationMethodList.Codes._1;

					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertHasMessageErrorContaining("At lest one InvoiceLine has JI_ValuationCode as 1, So Incoterm is required", propertyInfo, errorMessage);

					invoiceLine1.JI_ValuationCode = ZString.Empty;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertHasMessageErrorContaining("At lest one InvoiceLine has JI_ValuationCode as 1, So Incoterm is required", propertyInfo, errorMessage);

					invoiceLine2.JI_ValuationCode = ZString.Empty;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("None of the Invoicelines have a JI_ValuationCode of 1, Incoterm is optional", propertyInfo, errorMessage);

					invoiceLine1.JI_ValuationCode = ValuationMethodList.Codes._2;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("None of the Invoicelines have a JI_ValuationCode of 1, Incoterm is optional", propertyInfo, errorMessage);

					invoiceLine1.JI_ValuationCode = ValuationMethodList.Codes._5;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("None of the Invoicelines have a JI_ValuationCode of 1, Incoterm is optional", propertyInfo, errorMessage);
				});
			}
		}

		public void TestCheckJE_ShipmentIncoTerm_DeclarationMessageTypeAndUcc6_RuleC0738()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			var customEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			customEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			customEntryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = customEntryInstruction.PK;
			invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;

			const string errorMessage = "You have not entered a [20.1] Incoterm";
			var propertyInfo = declaration.JE_ShipmentIncoTermInfo;

			CombineAssertions(() =>
			{
				declaration.Validation.ValidateJE_ShipmentIncoTerm();
				AssertNoMessageErrorContaining("Declaration is Non-UCC6 Import, JI_ValuationCode is Empty, Incoterm is optional", propertyInfo, errorMessage);

				invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;
				declaration.Validation.ValidateJE_ShipmentIncoTerm();
				AssertNoMessageErrorContaining("Declaration is Non-UCC6 Import, JI_ValuationCode is 1, Incoterm is optional", propertyInfo, errorMessage);

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._6;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("Declaration is UCC6 Import, JI_ValuationCode is 6, Incoterm is optional", propertyInfo, errorMessage);

					invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertHasMessageErrorContaining("Declaration is UCC6 Import, JI_ValuationCode is 1, Incoterm is required", propertyInfo, errorMessage);

					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("Declaration is UCC6 Export, JI_ValuationCode is 1, Incoterm is optional", propertyInfo, errorMessage);

					invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._3;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("Declaration is UCC6 Export, JI_ValuationCode is 3, Incoterm is optional", propertyInfo, errorMessage);
				}

				declaration.Validation.ValidateJE_ShipmentIncoTerm();
				AssertNoMessageErrorContaining("Declaration is Non-UCC6 Export, JI_ValuationCode is 3, Incoterm is optional", propertyInfo, errorMessage);

				invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;
				declaration.Validation.ValidateJE_ShipmentIncoTerm();
				AssertNoMessageErrorContaining("Declaration is Non-UCC6 Export, JI_ValuationCode is 1, Incoterm is optional", propertyInfo, errorMessage);
			});
		}

		public void TestCheckJE_ShipmentIncoTerm_IsRuleC0738Active()
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

			var propertyInfo = declaration.JE_ShipmentIncoTermInfo;
			const string errorMessage = "You have not entered a [20.1] Incoterm";

			using (var context = new DeclarationValidationDeciderTestContext(declaration))
			{
				CombineAssertions(() =>
				{
					context.EnableRule(r => r.IsRuleC0738Active);
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("RuleC0738 is enabled, JI_ValuationCodes is 2. So, Incoterm is optional.", propertyInfo, errorMessage);

					invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertHasMessageErrorContaining("RuleC0738 is enabled, JI_ValuationCodes is 1. So, Incoterm is required.", propertyInfo, errorMessage);

					context.DisableRule(r => r.IsRuleC0738Active);
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("RuleC0738 is disabled, JI_ValuationCodes is 1. So, Incoterm is optional.", propertyInfo, errorMessage);

					invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._3;
					declaration.Validation.ValidateJE_ShipmentIncoTerm();
					AssertNoMessageErrorContaining("RuleC0738 is disabled, JI_ValuationCodes is 3. So, Incoterm is optional.", propertyInfo, errorMessage);
				});
			}
		}

		public void TestCheckJE_DeclarantType_PowerOfAttorney_Importer()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = GlbBranch.CurrentBranch.OrgProxy.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertNotAllowPOFAlone(declaration, declaration.Importer, GetRepresentationTypeDirectToTest);
			AssertPoa(declaration, declaration.Importer, GetRepresentationTypeDirectToTest, GetRepresentationTypeDirectToTest);
		}

		public void TestCheckJE_DeclarantType_PowerOfAttorney_Supplier()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = GlbBranch.CurrentBranch.OrgProxy.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertNotAllowPOFAlone(declaration, declaration.Supplier, GetRepresentationTypeDirectToTest);
			AssertPoa(declaration, declaration.Supplier, GetRepresentationTypeDirectToTest, GetRepresentationTypeDirectToTest);
		}

		void SetExchangeRate(BusinessObjectFactory factory, RefCurrency currency, ZDecimal rate, ZDateTime effectiveDate, ZString rateType)
		{
			var filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, SQLComparisonOperator.Equal, currency.RX_Code);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExRateType, SQLComparisonOperator.Equal, rateType);

			var exchangeRate = factory.LoadTop1<RefExchangeRate>(filter);
			if (exchangeRate == null)
			{
				exchangeRate = factory.New<RefExchangeRate>();
				exchangeRate.RE_RX_NKExCurrency = currency.RX_Code;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_StartDate = effectiveDate;
				exchangeRate.RE_ExpiryDate = effectiveDate.AddDays(1);
				exchangeRate.RE_ExRateType = rateType;
			}
			exchangeRate.RE_SellRate = rate;
		}

		static void AssertNotAllowPOFAlone(JobDeclaration declaration, OrgHeader org, string directRepresentationType)
		{
			org.RequiredDocuments.RemoveAndDeleteAll();
			declaration.JE_DeclarantType = directRepresentationType;
			var warningMessage = AuthorityToActValidator.GetNoPOADocumentForImporterString(new AuthorityToActValidator().CountrySpecificNameForPOA);
			AssertHasWarning(declaration.JE_DeclarantTypeInfo, warningMessage);

			var poaDocument = org.RequiredDocuments.AddNew("POF");
			poaDocument.EQ_DocType = "POF";
			poaDocument.EQ_DocDescription = "Power of Attorney Forwarding";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today;
			poaDocument.EQ_ValidToDate = ZDateTime.Today.AddMonths(1);

			AssertHasWarning(declaration.JE_DeclarantTypeInfo, warningMessage);
		}

		static void AssertPoa(JobDeclaration declaration, OrgHeader org, string selfRepresentationType, string directRepresentationType)
		{
			declaration.JE_DeclarantType = directRepresentationType;
			var poaString = AuthorityToActValidator.GetNoPOADocumentForImporterString(new AuthorityToActValidator().CountrySpecificNameForPOA);
			AssertHasWarning(declaration.JE_DeclarantTypeInfo, poaString);
			var poaDocument = org.RequiredDocuments.AddNew("POA");
			poaDocument.EQ_DocType = "POA";
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today;
			poaDocument.EQ_ValidToDate = ZDateTime.Today.AddMonths(1);
			declaration.JE_DeclarantType = selfRepresentationType;
			declaration.JE_DeclarantType = directRepresentationType;
			AssertNoWarning(declaration.JE_DeclarantTypeInfo, poaString);
		}

		string GetRepresentationTypeDirectToTest => RepresentationTypeList.Codes._2Direct;

		class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZBool AreMultipleEntryInstructionsAllowed => false;

			protected override CusGuaranteeHeader GetCustomsGuaranteeCore => customsGuarantee ?? (customsGuarantee = Factory.New<CusGuaranteeHeader>());
			CusGuaranteeHeader customsGuarantee;
		}
	}
}
