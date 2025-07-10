using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.GB.Business.GBCommonConstants;
using Eu = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Declaration.Testing
{
	class CDSJobComInvoiceLineValidationTests : TestCaseWithFactory
	{
		public void TestValidateZG_MethodOfPayment()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");

			var gBImportMOP = helper.CreateNewOrGetExistingCusCodeList("CDS", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "A", "Immediate payment by cash or equivalent (Paper declarations)", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBImportMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBImportMOP.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);
			var gBExportMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "L", "CAP Export Licence", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBExportMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBExportMOP.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);

			var dEMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "D", "Andere (z.B. Abbuchung vom Konto eines Zollagenten)", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeListAttribute(dEMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);

			var iTMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "T", "Garanzia sul conto dello spedizioniere doganale", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeListAttribute(iTMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);

			Factory.Save();

			var declaration = CDSJobComInvoiceLineTest.CreateCDSJobDeclaration(Factory);
			declaration.JE_MessageType = "IMP";
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertNoMessageErrors(invLine.ZG_MethodOfPaymentInfo);

			invLine.ZG_MethodOfPayment = "X";
			AssertHasMessageErrorContaining(invLine.ZG_MethodOfPaymentInfo, ListValidation.InvalidCodeMessageError);
			invLine.ZG_MethodOfPayment = "A";
			AssertNoMessageErrors(invLine.ZG_MethodOfPaymentInfo);
		}

		public void TestCheckJI_ValuationCode()
		{
			var declaration = GetJobDeclarationForTest();
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertNoMessageErrors(invLine.JI_ValuationCodeInfo);
			invLine.JI_ValuationCode = "7";
			AssertHasMessageError(invLine.JI_ValuationCodeInfo, "Simplified Procedure Value (SPV, code '7') is now declared using an Additional Procedure Code 'E01' in DE 1/11 and Valuation Method Code '4' should be declared in this field.");
			invLine.JI_ValuationCode = "3";
			AssertNoMessageErrors(invLine.JI_ValuationCodeInfo);
		}

		public void TestValidateCountryOfOriginAndSupply()
		{
			var declaration = GetJobDeclarationForTest();
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertNoMessageErrors(invLine.JI_CountryOfOriginInfo);
			invLine.ZG_CountryOfSupply = "US";
			AssertHasMessageError(invLine.JI_CountryOfOriginInfo, "When country/region of supply [UCC 5/15] is supplied, country/region of origin [UCC 5/16] must be present");
			invLine.JI_CountryOfOrigin = "US";
			AssertNoMessageErrors(invLine.JI_CountryOfOriginInfo);
		}

		public void TestCheckJI_ProcedureWithApportion()
		{
			var warning = "No group charges or invoice charges would be apportioned into invoice lines whose procedure code ends with E01 or E02";
			var declaration = GetJobDeclarationForTest();
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertNoWarning(invLine.JI_ProcedureInfo, warning);

			invLine.JI_Procedure = "4001E01";
			AssertHasWarning(invLine.JI_ProcedureInfo, warning);

			invLine.JI_Procedure = "4001E02";
			AssertHasWarning(invLine.JI_ProcedureInfo, warning);

			invLine.JI_Procedure = "4001E03";
			AssertNoWarning(invLine.JI_ProcedureInfo, warning);
		}

		public void TestEntryLineGrossMass()
		{
			const string expectedError = @"[UCC 6/5] Gross Weight (GWT) value for the associated entry line cannot be zero and must be less than 10000000000000000 KG";
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MasterUCR = "ZZZ";
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			invLine1.JI_Weight = 0;

			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.JobComInvoiceLines.AddNew();
			invLine2.JI_Tariff = "2203001010";
			invLine2.JI_Weight = 0;

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);
			invLine1.Validation.ValidateAll();
			invLine2.Validation.ValidateAll();

			AssertHasMessageError(invLine1.JI_WeightInfo, expectedError);
			AssertHasMessageError(invLine2.JI_WeightInfo, expectedError);

			invLine1.JI_Weight = 20;
			invLine1.Validation.ValidateAll();
			invLine2.Validation.ValidateAll();

			AssertNoMessageError(invLine1.JI_WeightInfo, expectedError);
			AssertNoMessageError(invLine2.JI_WeightInfo, expectedError);

			invLine1.JI_Weight = 9999999999999999;
			invLine2.JI_Weight = 1;
			invLine1.Validation.ValidateAll();
			invLine2.Validation.ValidateAll();

			AssertHasMessageError(invLine1.JI_WeightInfo, expectedError);
			AssertHasMessageError(invLine2.JI_WeightInfo, expectedError);

			invLine1.JI_Weight = 999999.999;
			invLine2.JI_Weight = 999999.999;
			invLine1.Validation.ValidateAll();
			invLine2.Validation.ValidateAll();

			AssertNoMessageError(invLine1.JI_WeightInfo, expectedError);
			AssertNoMessageError(invLine2.JI_WeightInfo, expectedError);
		}

		public void TestCheckAdditionalProcedureCodesAsStringWithApportion()
		{
			var warning = "No group charges or invoice charges would be apportioned into invoice lines whose procedure code ends with E01 or E02";
			var declaration = GetJobDeclarationForTest();
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var additionalCode = invLine.AdditionalProcedureCodes.AddNew();
			additionalCode.CY_Code = "4000E01";
			AssertNoWarning(invLine.AdditionalProcedureCodesAsStringInfo, warning);

			invLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
			additionalCode = invLine.AdditionalProcedureCodes.AddNew();
			additionalCode.CY_Code = "4000E01";
			invLine.Validation.ValidateAdditionalProcedureCodesAsString();
			AssertHasWarning(invLine.AdditionalProcedureCodesAsStringInfo, warning);

			invLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
			additionalCode = invLine.AdditionalProcedureCodes.AddNew();
			additionalCode.CY_Code = "4000E02";
			invLine.Validation.ValidateAdditionalProcedureCodesAsString();
			AssertHasWarning(invLine.AdditionalProcedureCodesAsStringInfo, warning);

			invLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
			additionalCode = invLine.AdditionalProcedureCodes.AddNew();
			additionalCode.CY_Code = "4000E03";
			AssertNoWarning(invLine.AdditionalProcedureCodesAsStringInfo, warning);
		}

		public void TestTariffInfoMessageError_UCCCompliant()
		{
			var declaration = GetJobDeclarationForTest();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals(true, declaration.IsUCCCompliant);

			var validation = new CDSJobComInvoiceLineValidation(invoiceLine);
			validation.ValidateJI_Tariff();
			var msg = invoiceLine.GetMessageErrors().FirstOrDefault(x => x.Message.StartsWith("Message Error - Invoice Line: This line has no packaging details.")).Message;
			Assert(msg.Contains("[UCC 6/10] Packages"));
		}

		public void TestCCRDemandsNoTariff()
		{
			RunCCRTariffTest("IMP", "21i", "IFD");
			RunCCRTariffTest("EXP", "21e", "EFD");
		}

		public void TestCheckInvoiceLines_UnderH8Declaration_Contains_GBNI_Movements()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoice = dec.Invoices.AddNew();
			var inv1 = invoice.InvoiceLines.AddNew();
			var inv2 = invoice.InvoiceLines.AddNew();

			var cusEntryInstruction = dec.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ReducedDataSetDeclaration;

			inv1.Validation.ValidateJI_CEI();
			AssertNoMessageError(inv1.JI_CEIInfo, "H8 is only used with GB-NI movements and only for not-at-risk");
			AssertNoMessageError(inv2.JI_CEIInfo, "H8 is only used with GB-NI movements and only for not-at-risk");

			inv1.JI_CEI = cusEntryInstruction.PK;

			AssertHasMessageError(inv1.JI_CEIInfo, "H8 is only used with GB-NI movements and only for not-at-risk");
			AssertNoMessageError(inv2.JI_CEIInfo, "H8 is only used with GB-NI movements and only for not-at-risk");

			inv1.AdditionalInfos.AddNew().CSI_Code = AdditonalInfoCodes.NIDOM;
			inv1.Validation.ValidateJI_CEI();

			AssertHasMessageError(inv1.JI_CEIInfo, "H8 is only used with GB-NI movements and only for not-at-risk");
			inv1.AdditionalInfos.AddNew().CSI_Code = AdditonalInfoCodes.NIREM;
			inv1.Validation.ValidateJI_CEI();
			AssertNoMessageError(inv1.JI_CEIInfo, "H8 is only used with GB-NI movements and only for not-at-risk");
		}

		public void TestCountryOfOriginAndOriginOverride()
		{
			var declaration = GetJobDeclarationForTest();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.EntryInstruction.CEI_Style = "H1";
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();

			var error1 = "[UCC 5/15,16] Country/Region of Origin is required";
			var error2 = "If 5/15 Origin Override is supplied it must be different to 5/16";
			var warn1 = "With preference starting 1, 5/15 Origin Override is not needed and is not sent";

			var msg = invoiceLine.JI_CountryOfOriginInfo.GetMessageErrors().FirstOrDefault(x => x.Message.Contains(error1));
			AssertNotNull("Should be an error message", msg);

			invoiceLine.EntryInstruction.CEI_Style = "H2";
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();

			msg = invoiceLine.JI_CountryOfOriginInfo.GetMessageErrors().FirstOrDefault(x => x.Message.Contains(error1));
			AssertNull("Should not be an error message", msg);

			invoiceLine.EntryInstruction.CEI_Style = "H1";
			invoiceLine.JI_CountryOfOrigin = "US";

			msg = invoiceLine.JI_CountryOfOriginInfo.GetMessageErrors().FirstOrDefault(x => x.Message.Contains(error1));
			AssertNull("Should not be an error message", msg);

			invoiceLine.JI_PrimaryPreference = "400";
			invoiceLine.ZG_CountryOfSupply = "US";
			msg = invoiceLine.ZG_CountryOfSupplyInfo.GetMessageErrors().FirstOrDefault(x => x.Message.Contains(error2));
			AssertNotNull("Should be an error message", msg);

			invoiceLine.ZG_CountryOfSupply = "ZA";
			msg = invoiceLine.ZG_CountryOfSupplyInfo.GetMessageErrors().FirstOrDefault(x => x.Message.Contains(error2));
			AssertNull("Should not be an error message", msg);
			var warning = invoiceLine.ZG_CountryOfSupplyInfo.Notifications.FirstOrDefault(x => x.Message.Contains(warn1));
			AssertNull("Should not be a warning message", warning);

			invoiceLine.JI_PrimaryPreference = "100";
			invoiceLine.Validation.ValidateAll();
			warning = invoiceLine.ZG_CountryOfSupplyInfo.Notifications.FirstOrDefault(x => x.Message.Contains(warn1));
			AssertNotNull("Should be a warning message", warning);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			invoiceLine.EntryInstruction.CEI_Style = "H1";
			invoiceLine.ZG_CountryOfSupply = "";
			invoiceLine.JI_CountryOfOrigin = "";
			msg = invoiceLine.JI_CountryOfOriginInfo.GetMessageErrors().FirstOrDefault(x => x.Message.Contains(error1));
			AssertNull("Should not be an error message", msg);
		}

		public void TestCheckJI_NetWeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoices = declaration.Invoices.AddNew();
			var invLine = invoices.JobComInvoiceLines.AddNew();

			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			invLine.JI_Weight = 100m;
			invLine.JI_NetWeight = 100.01;
			AssertHasMessageError(invLine.JI_NetWeightInfo, CDSJobComInvoiceLineValidation.NetWeightIsGreaterThanGrossWeight);

			invLine.JI_WeightUQ = Enterprise.Core.Constants.Weight.Grams;
			invLine.JI_Weight = 1000m;
			invLine.JI_NetWeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
			invLine.JI_NetWeight = 2m;
			AssertHasMessageError(invLine.JI_NetWeightInfo, CDSJobComInvoiceLineValidation.NetWeightIsGreaterThanGrossWeight);

			invLine.JI_WeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
			invLine.JI_Weight = 10m;
			invLine.JI_NetWeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
			invLine.JI_NetWeight = 10m;
			AssertNoMessageErrors(invLine.JI_NetWeightInfo);
		}

		public void TestCheckJI_CustomsQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			invLine.JI_WeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
			invLine.JI_CustomsUnitQty = Enterprise.Core.Constants.Weight.Kilograms;
			invLine.JI_Weight = 999m;
			invLine.JI_CustomsQuantity = 1000m;
			AssertHasMessageErrorContaining(invLine.JI_CustomsQuantityInfo, CDSJobComInvoiceLineValidation.CustomsQtyLessThanGrossWeight);

			invLine.JI_Weight = 1000m;
			invLine.JI_CustomsQuantity = 999.01;
			AssertNoMessageErrors(invLine.JI_CustomsQuantityInfo);

			invLine.JI_WeightUQ = Enterprise.Core.Constants.Weight.Grams; //1000 grammes
			invLine.JI_CustomsQuantity = 999; // kilos
			AssertHasMessageErrorContaining(invLine.JI_CustomsQuantityInfo, CDSJobComInvoiceLineValidation.CustomsQtyLessThanGrossWeight);
			invLine.JI_CustomsUnitQty = Enterprise.Core.Constants.Weight.Grams;
			invLine.JI_CustomsQuantity = 998;
			AssertNoMessageErrors(invLine.JI_CustomsQuantityInfo);
		}

		public void TestCheckJI_CustomsSecondQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			invLine.JI_CustomsUnitQty = "KGM";
			invLine.JI_CustomsQuantity = 1.234;
			invLine.JI_CustomsSecondUnitQty = "GRM";
			invLine.JI_CustomsSecondQuantity = 1234;
			AssertNoMessageErrors(invLine.JI_CustomsSecondQuantityInfo);

			invLine.JI_CustomsSecondQuantity = 1234.456;
			AssertHasMessageErrorContaining(invLine.JI_CustomsSecondQuantityInfo, CDSJobComInvoiceLineValidation.CustomsSecondQtyMismatch);

			invLine.JI_CustomsSecondQuantity = 6543121.456;
			AssertHasMessageErrorContaining(invLine.JI_CustomsSecondQuantityInfo, CDSJobComInvoiceLineValidation.CustomsSecondQtyMismatch);

			invLine.JI_CustomsQuantity = 1.23;
			invLine.JI_CustomsSecondQuantity = 1230;
			AssertNoMessageErrors(invLine.JI_CustomsSecondQuantityInfo);

			invLine.JI_CustomsSecondQuantity = 1231;
			AssertHasMessageErrorContaining(invLine.JI_CustomsSecondQuantityInfo, CDSJobComInvoiceLineValidation.CustomsSecondQtyMismatch);

			invLine.JI_CustomsQuantity = 1.231;
			invLine.JI_CustomsSecondQuantity = 1230;
			AssertHasMessageErrorContaining(invLine.JI_CustomsSecondQuantityInfo, CDSJobComInvoiceLineValidation.CustomsSecondQtyMismatch);

			invLine.JI_CustomsQuantity = 1.234;
			invLine.JI_CustomsSecondUnitQty = "KGMG";
			invLine.JI_CustomsSecondQuantity = 1000;
			AssertNoMessageErrors(invLine.JI_CustomsSecondQuantityInfo);
		}

		public void TestCheckJI_Tariff_SupplementaryCodesExist()
		{
			const string TariffCode = "1234567890";
			const string TariffSupplementaryCode = "7444";

			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDate(2079, 06, 06);
			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupingCode = "CDS";

			var tradeGroupStandard = testHelper.CreateTradeGroup(dataGroupingCode, "STANDARD", date1, date2);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date2);

			var tariffType = testHelper.CreateNewOrGetExistingTariffType(dataGroupingCode, Universal.Constants.TariffTypes.Import);
			Factory.Save();

			var cusTariff = testHelper.CreateTariff(dataGroupingCode, tariffType.PK, TariffCode, date1, date2, "dummy Description 0");

			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(dataGroupingCode, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = testHelper.LoadOrCreateNewCusRateCode(Factory, "A00", dutyRateType.PK);
			var testRate = testHelper.CreateRate(cusTariff, rateCode.PK, date1, date2, dataGrouping: dataGroupingCode);
			testHelper.CreateCusApplicability(testRate, tradeGroupStandard, date1, date2, additionalCode: TariffSupplementaryCode);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = TariffCode;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			CombineAssertions(() =>
			{
				invoiceLine.JI_Tariff = "TEST";
				invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
				AssertSupplementaryCodesExistValidationMessage("No supplementary code exists");

				invoiceLine.JI_Tariff = TariffCode;
				invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
				AssertSupplementaryCodesExistValidationMessage("Supplementary code exists but not specified; no validation severity registry setting", expectWarning: true);

				invoiceLine.JI_Tariff = TariffCode;
				invoiceLine.JI_SupplementaryCode1 = TariffSupplementaryCode;
				AssertSupplementaryCodesExistValidationMessage("Supplementary code exists and specified");

				using (CustomsDataRegistry.Instance.TariffAdditionalCodeNotificationType.SetTemporaryValue(Guid.Empty, invoice.RegistryBranchPK, Guid.Empty, TariffAdditionalCodeNotificationTypeList.Codes.Warning))
				{
					invoiceLine.JI_Tariff = TariffCode;
					invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
					AssertSupplementaryCodesExistValidationMessage("Supplementary code exists but not specified; validation severity registry setting is Warning", expectWarning: true);
				}

				invoiceLine.JI_Tariff = "TEST";
				invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
				AssertSupplementaryCodesExistValidationMessage("No supplementary code exists");

				using (CustomsDataRegistry.Instance.TariffAdditionalCodeNotificationType.SetTemporaryValue(Guid.Empty, invoice.RegistryBranchPK, Guid.Empty, TariffAdditionalCodeNotificationTypeList.Codes.MessageError))
				{
					invoiceLine.JI_Tariff = TariffCode;
					invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
					AssertSupplementaryCodesExistValidationMessage("Supplementary code exists but not specified; validation severity registry setting is MessageError", expectMessageError: true);
				}
			});

			void AssertSupplementaryCodesExistValidationMessage(string testCase, bool expectWarning = false, bool expectMessageError = false)
			{
				const string SupplementaryCodesExistMessage = "Supplementary Codes exist for this tariff";
				var tariffInfo = invoiceLine.JI_TariffInfo;

				if (expectWarning)
				{
					AssertHasWarningContaining($"{testCase} - has Warning", tariffInfo, SupplementaryCodesExistMessage);
				}
				else
				{
					AssertNoWarningContaining($"{testCase} - no Warning", tariffInfo, SupplementaryCodesExistMessage);
				}

				if (expectMessageError)
				{
					AssertHasMessageErrorContaining($"{testCase} - has MessageError", tariffInfo, SupplementaryCodesExistMessage);
				}
				else
				{
					AssertNoMessageErrorContaining($"{testCase} - no Warning", tariffInfo, SupplementaryCodesExistMessage);
				}
			}
		}

		public void TestCheckJI_Tariff_UniqueRateForRateType()
		{
			const string TariffCode = "1234567890";

			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDate(2079, 06, 06);
			var gbCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var testHelper = new UniversalReferenceTestDataHelper(Factory);

			var tradeGroupStandard = testHelper.CreateTradeGroup(gbCountryCode, "STANDARD", date1, date2);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date2);

			var tariffType = testHelper.CreateNewOrGetExistingTariffType(gbCountryCode, Universal.Constants.TariffTypes.Import);
			Factory.Save();

			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(gbCountryCode, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode11 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A00", dutyRateType.PK);
			var rateCode12 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A20", dutyRateType.PK);

			var addRateType = testHelper.CreateNewOrGetExistingRateType(gbCountryCode, Universal.Constants.RateTypes.AntiDumping, "Anti-Dumping");
			var rateCode21 = testHelper.LoadOrCreateNewCusRateCode(Factory, "AD1", addRateType.PK);
			var rateCode22 = testHelper.LoadOrCreateNewCusRateCode(Factory, "AD2", addRateType.PK);

			var cvdRateType = testHelper.CreateNewOrGetExistingRateType(gbCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty, "CVD");
			var rateCode31 = testHelper.LoadOrCreateNewCusRateCode(Factory, "CV1", cvdRateType.PK);
			var rateCode32 = testHelper.LoadOrCreateNewCusRateCode(Factory, "CV2", cvdRateType.PK);

			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", gbCountryCode);
			var preferenceTWO = testHelper.CreatePreferenceForCountry("TWO", "TWO Matched", gbCountryCode);
			var preferenceZRO = testHelper.CreatePreferenceForCountry("ZRO", "ZERO Matched", gbCountryCode);

			var cusTariff = testHelper.CreateTariff(gbCountryCode, tariffType.PK, TariffCode, date1, date2, "dummy Description 0");

			var testRate1 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date2, "0", preferencePk: preferenceSTD.PK, dataGrouping: gbCountryCode);
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date2);
			var testRate2 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date2, "0", preferencePk: preferenceTWO.PK, dataGrouping: gbCountryCode);
			testHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date2);
			var testRate3 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date2, "0", preferencePk: preferenceTWO.PK, dataGrouping: gbCountryCode);
			testHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date2);
			var testRate4 = testHelper.CreateRate(cusTariff, rateCode12.PK, date1, date2, "0", preferencePk: preferenceSTD.PK, dataGrouping: gbCountryCode);
			testHelper.CreateCusApplicability(testRate4, tradeGroupStandard, date1, date2);
			var testRate5 = testHelper.CreateRate(cusTariff, rateCode21.PK, date1, date2, "0", preferencePk: preferenceSTD.PK, dataGrouping: gbCountryCode);
			testHelper.CreateCusApplicability(testRate5, tradeGroupStandard, date1, date2);
			var testRate6 = testHelper.CreateRate(cusTariff, rateCode31.PK, date1, date2, "0", preferencePk: preferenceSTD.PK, dataGrouping: gbCountryCode);
			testHelper.CreateCusApplicability(testRate6, tradeGroupStandard, date1, date2);
			var testRate7 = testHelper.CreateRate(cusTariff, rateCode21.PK, date1, date2, "0", preferencePk: preferenceTWO.PK, dataGrouping: gbCountryCode);
			testHelper.CreateCusApplicability(testRate7, tradeGroupStandard, date1, date2);
			var testRate8 = testHelper.CreateRate(cusTariff, rateCode21.PK, date1, date2, "0", preferencePk: preferenceTWO.PK, dataGrouping: gbCountryCode);
			testHelper.CreateCusApplicability(testRate8, tradeGroupStandard, date1, date2);
			var testRate9 = testHelper.CreateRate(cusTariff, rateCode31.PK, date1, date2, "0", preferencePk: preferenceTWO.PK, dataGrouping: gbCountryCode);
			testHelper.CreateCusApplicability(testRate9, tradeGroupStandard, date1, date2);
			var testRate10 = testHelper.CreateRate(cusTariff, rateCode31.PK, date1, date2, "0", preferencePk: preferenceTWO.PK, dataGrouping: gbCountryCode);
			testHelper.CreateCusApplicability(testRate10, tradeGroupStandard, date1, date2);
			var testRate11 = testHelper.CreateRate(cusTariff, rateCode31.PK, date1, date2, "0", preferencePk: preferenceZRO.PK, dataGrouping: gbCountryCode);
			testHelper.CreateCusApplicability(testRate11, tradeGroupStandard, date1, date2, "", "ORD11");
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = TariffCode;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			var effectiveDate = invoiceLine.EffectiveAssessmentDate;

			CombineAssertions(() =>
			{
				invoiceLine.JI_PrimaryPreference = "TWO";
				AssertNoMessageErrorContaining("no applicable Duty rate", invoiceLine.JI_TariffInfo, $"There is no applicable Duty rate for the Tariff '{TariffCode}' and Country Of Origin 'AU' as at {effectiveDate} in combination with other data entered on the form.");
				AssertNoMessageErrorContaining("has valid Duty rate", invoiceLine.JI_TariffInfo, "Valid Duty rates exist where\r\n1: Preference = STD\r\n2: Preference = TWO\r\n");

				invoiceLine.JI_PrimaryPreference = "ZRO";
				AssertHasMessageErrorContaining("no applicable Duty rate", invoiceLine.JI_TariffInfo, $"There is no applicable Duty rate for the Tariff '{TariffCode}' and Country Of Origin 'AU' as at {effectiveDate} in combination with other data entered on the form.");
				AssertHasMessageErrorContaining("has valid Duty rate", invoiceLine.JI_TariffInfo, "Valid Duty rates exist where\r\n1: Preference = STD\r\n2: Preference = TWO\r\n");
			});
		}

		public void TestCheckJI_Tariff_NoMeursingCheck()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var testHelper = new UniversalReferenceTestDataHelper(Factory);

			var dataGroup = testHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			testHelper.CreateNewOrGetExistingDataGrouping("GB", parent: dataGroup);
			testHelper.CreateNewOrGetExistingDataGrouping("CDS", parent: dataGroup);

			var tradeGroupStandard = testHelper.CreateTradeGroup("EUN", "STANDARD", date1, date4);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date4);
			Factory.Save();

			var tariffType = testHelper.CreateNewOrGetExistingTariffType("CDS", "IMP");
			Factory.Save();
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType("EUN", "DTY", "Duty");
			var rateCode11 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A00", dutyRateType.PK);
			var rateCode12 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A20", dutyRateType.PK);
			Factory.Save();
			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", "EUN");
			var preferenceTWO = testHelper.CreatePreferenceForCountry("TWO", "TWO Matched", "EUN");
			var preferenceTRD = testHelper.CreatePreferenceForCountry("TRD", "TRD Matched", "EUN");
			Factory.Save();

			var cusTariff = testHelper.CreateTariff("EUN", tariffType.PK, "1234567890", date1, date4, "dummy Description 0");
			Factory.Save();

			var testRate1 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, rateFormula: "#ADFM(5)#", preferencePk: preferenceSTD.PK, dataGrouping: "EUN");
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, additionalCode: "7444");
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, additionalCode: "7454");
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, additionalCode: "8444");
			var testRate2 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, rateFormula: "10", preferencePk: preferenceSTD.PK, dataGrouping: "EUN");
			testHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "CDS";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			CombineAssertions(() =>
			{
				invoiceLine.JI_PrimaryPreference = "STD";
				AssertNoMessageErrors("No supplementary code error and unique rate error", invoiceLine.JI_TariffInfo);
				invoiceLine.JI_SupplementaryCode1 = "888";
				AssertNoMessageErrors("No supplementary code error and unique rate error", invoiceLine.JI_TariffInfo);
				invoiceLine.JI_SupplementaryCode1 = "7444";
				AssertNoMessageErrors("No supplementary code error and unique rate error", invoiceLine.JI_TariffInfo);
				invoiceLine.JI_SupplementaryCode2 = "7454";
				AssertNoMessageErrors("No error(preference='STD')", invoiceLine.JI_TariffInfo);
				invoiceLine.JI_SupplementaryCode2 = "8444";
				AssertNoMessageErrors("No supplementary code error", invoiceLine.JI_TariffInfo);

				declaration.JE_ApplicationCode = "CHF";
				invoiceLine.JI_SupplementaryCode2 = "7454";
				AssertNoMessageErrors("No error(preference='STD') for CHF declaration", invoiceLine.JI_TariffInfo);
			});
		}

		public void TestCheckJI_Description()
		{
			// Warning if desc contains >< AND it's via CNS.
			var dec = GetJobDeclarationForTest();
			dec.ZG_Gateway = GatewayList.Codes.CNS_CUSDECOnly;
			var line = (Eu.JobComInvoiceLine)(dec.Invoices.AddNew().InvoiceLines.AddNew());

			dec.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
			line.JI_Description = "LAMP HOLDERS, PLUGS AND SOCKETS: OTHER THAN LAMP HOLDERS OTHER THAN THOSE FOR COAXIAL CABLES, FOR PRINTED CIRCUITS EXCLUDING THOSE FOR USE IN CERTAIN TYPES OF AIRCRAFT<004><100>";

			AssertNoMessageErrorContaining(line.JI_DescriptionInfo, CDSJobComInvoiceLineValidation.E00662);
			line.JI_Description = "";
			AssertHasMessageErrorContaining(line.JI_DescriptionInfo, CDSJobComInvoiceLineValidation.E00662);
			dec.JE_MessageType = "IMP";
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration;
			dec.JE_EntrySubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration;
			line.JI_Procedure = JobComInvoiceLine.CfspFsdCPCCode;
			Assert("PreReq - IsFSD", dec.IsFSD);
			line.JI_Description = "x";
			line.JI_Description = "";
			AssertNoMessageErrorContaining(line.JI_DescriptionInfo, CDSJobComInvoiceLineValidation.E00662);
		}

		public void TestEmptyPackaging()
		{
			var declaration = GetJobDeclarationForTest();
			var invHeader = declaration.Invoices.AddNew();
			var invoiceLine = invHeader.InvoiceLines.AddNew();
			declaration.JE_MasterBill = "M";
			var cw = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			cw.CW_PackQty = 1;

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_DeclarationType = ExportSADDeclarationTypeList.Codes.ExitSummaryDeclaration;
			invoiceLine.JI_Tariff = "1.2.3";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
			declaration.JE_DeclarationType = ExportSADDeclarationTypeList.Codes.ExportFullDeclaration;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
			invoiceLine.ClearRowNotificationsContaining("This line has no packaging details");
			var pack1 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw);
			pack1.CHC_NumberOfPacks = 1;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
			pack1.Delete();
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
			invoiceLine.ClearRowNotificationsContaining("This line has no packaging details");

			pack1 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw);
			pack1.CHC_NumberOfPacks = 1;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
			pack1.Delete();
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
			invoiceLine.ClearRowNotificationsContaining("This line has no packaging details");

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration;
			declaration.JE_EntrySubStyle =
				GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration;
			invoiceLine.JI_Tariff = "1.2.3";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoRowMessageErrorContaining(invoiceLine, "This line has no packaging details");

			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration;
			declaration.JE_EntrySubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSfdGoodsNotArrived;
			invoiceLine.JI_Tariff = "1.2.3";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoRowMessageErrorContaining(invoiceLine, "This line has no packaging details");

			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			declaration.JE_EntrySubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSfdGoodsNotArrived;
			invoiceLine.JI_Tariff = "1.2.3";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
		}

		public void TestTypeOfValidation()
		{
			JobDeclaration dec = GetJobDeclarationForTest();
			var line = (Eu.JobComInvoiceLine)(dec.Invoices.AddNew().InvoiceLines.AddNew());
			AssertEquals("JobComInvoiceLine on a British dec should have correct validation", ExpectedValidationType, line.Validation.GetType());

			var standAloneLine = Factory.New<Eu.JobComInvoiceLine>();
			AssertEquals("Standalone JobComInvoiceLine should have EU validation", typeof(Eu.JobComInvoiceLineValidation), standAloneLine.Validation.GetType());
		}

		public void TestCheckJI_Procedure()
		{
			const string youHaveNotEntered = MandatoryValidation.YouHaveNotEntered;
			var dec = GetJobDeclarationForTest();
			var line = (Eu.JobComInvoiceLine)(dec.Invoices.AddNew().InvoiceLines.AddNew());
			line.JI_Procedure = "123";
			AssertNoMessageErrorContaining(line.JI_ProcedureInfo, youHaveNotEntered);
			line.JI_Procedure = "1234567";
			AssertNoMessageErrorContaining(line.JI_ProcedureInfo, youHaveNotEntered);
			line.JI_Procedure = "";
			AssertHasMessageErrorContaining(line.JI_ProcedureInfo, youHaveNotEntered);
		}

		public void TestCpcDemandsGrossMass()
		{
			var declaration = GetJobDeclarationForTest();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupCode = declaration.GetDefaultDataGroupingCode();
			var fullDecCpc = helper.CreateOrFindExistingRefCusProcedure(dataGroupCode, "00", "10", "00", "056", "Test", "EXP", "EFD");
			var otherCpc = helper.CreateOrFindExistingRefCusProcedure(dataGroupCode, "00", "10", "00", "046", "Test", "EXP", "EFD");
			fullDecCpc.Attributes.AddNew(Universal.AttributeNames.Codes.GrossMassMandatory, "anything");
			Factory.Save();

			declaration.JE_MessageType = "EXP";
			declaration.JE_DeclarationType = "EFD";
			var invHeader = declaration.Invoices.AddNew();
			var line = invHeader.InvoiceLines.AddNew();
			line.JI_Procedure = fullDecCpc.FullCodeCurrentPlusPreviousPlusConcession;
			line.JI_Weight = 0;
			line.Validation.ValidateJI_Weight();
			AssertHasMessageErrorContaining(line.JI_WeightInfo, "may not be empty");
			line.JI_Procedure = otherCpc.FullCodeCurrentPlusPreviousPlusConcession;
			line.Validation.ValidateJI_Weight();
			AssertNoMessageErrorContaining(line.JI_WeightInfo, "may not be empty");
		}
		public void TestTariffCanBeEmptyWhenAllowedByCpc()
		{
			var declaration = GetJobDeclarationForTest();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupCode = declaration.GetDefaultDataGroupingCode();
			var fullDecCpc = helper.CreateOrFindExistingRefCusProcedure(dataGroupCode, "00", "11", "11", "111", "Test", "EXP", "EFD");
			var personalEffectsCpc = helper.CreateOrFindExistingRefCusProcedure(dataGroupCode, "00", "22", "22", "222", "Test", "EXP", "EFD");
			var cfspCpc = helper.CreateOrFindExistingRefCusProcedure(dataGroupCode, "00", "33", "33", "333", "Test", "EXP", "EFD");
			var hsCodeOptional = helper.CreateOrFindExistingRefCusProcedure(dataGroupCode, "00", "44", "44", "444", "Test", "EXP", "EFD");
			personalEffectsCpc.Attributes.AddNew(Universal.AttributeNames.Codes.PersonalEffects, "anything");
			cfspCpc.Attributes.AddNew(Universal.AttributeNames.Codes.CfspNcgds, "anything");
			hsCodeOptional.Attributes.AddNew(Universal.AttributeNames.Codes.HSCodeOptional, Universal.AttributeNames.Descriptions.HSCodeOptional);
			Factory.Save();

			declaration.JE_MessageType = "EXP";
			declaration.JE_DeclarationType = "EFD";
			var invHeader = declaration.Invoices.AddNew();
			var line = invHeader.InvoiceLines.AddNew();
			line.JI_Procedure = fullDecCpc.FullCodeCurrentPlusPreviousPlusConcession;
			line.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(line.JI_TariffInfo, "may not be empty");
			line.JI_Procedure = personalEffectsCpc.FullCodeCurrentPlusPreviousPlusConcession;
			line.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(line.JI_TariffInfo, "may not be empty");
			line.JI_Procedure = cfspCpc.FullCodeCurrentPlusPreviousPlusConcession;
			line.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(line.JI_TariffInfo, "may not be empty");
			line.JI_Procedure = hsCodeOptional.FullCodeCurrentPlusPreviousPlusConcession;
			line.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(line.JI_TariffInfo, "may not be empty");
			line.JI_Procedure = fullDecCpc.FullCodeCurrentPlusPreviousPlusConcession;
			line.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(line.JI_TariffInfo, "may not be empty");
			line.JI_Tariff = "123";
			AssertNoMessageErrorContaining(line.JI_TariffInfo, "may not be empty");
		}

		public void TestPreviousDocuments()
		{
			var dec = GetJobDeclarationForTest();
			var hdr = dec.Invoices.AddNew();
			var line1 = hdr.InvoiceLines.AddNew();
			var line2 = hdr.InvoiceLines.AddNew();
			var grp = hdr.GroupHeader;

			hdr.JZ_InvoiceNumber = "";
			hdr.JZ_IncoTerm = "CFR";
			AssertHasWarningContaining(hdr.JZ_InvoiceNumberInfo, "previous document");
			var pdHdr = hdr.PreviousDocuments.AddNew();
			pdHdr.CSI_SubType = "Z";
			AssertNoWarningContaining(hdr.JZ_InvoiceNumberInfo, "previous document");

			line1.JI_LineNo = 1;
			line2.JI_LineNo = 2;
			AssertNoWarningContaining(line1.JI_LineNoInfo, "previous document");
			AssertNoWarningContaining(line2.JI_LineNoInfo, "previous document");

			hdr.PreviousDocuments.RemoveAndDeleteAll();
			line1.JI_LineNo = 1;
			line2.JI_LineNo = 2;
			AssertHasWarningContaining(line1.JI_LineNoInfo, "previous document");
			AssertHasWarningContaining(line2.JI_LineNoInfo, "previous document");

			var pdLine = line2.PreviousDocuments.AddNew();
			pdLine.CSI_SubType = "Z";
			line2.JI_LineNo = 2;
			AssertHasWarningContaining(line1.JI_LineNoInfo, "previous document");
			AssertNoWarningContaining(line2.JI_LineNoInfo, "previous document");

			pdHdr = hdr.PreviousDocuments.AddNew();
			pdHdr.CSI_SubType = "Z";
			line1.JI_LineNo = 1;
			AssertNoWarningContaining(line1.JI_LineNoInfo, "previous document");

			hdr.PreviousDocuments.RemoveAndDeleteAll();
			line1.JI_LineNo = 1;
			line2.JI_LineNo = 2;
			AssertHasWarningContaining(line1.JI_LineNoInfo, "previous document");
			var pdGrp = dec.PreviousDocuments.AddNew();
			pdGrp.CSI_SubType = "Z";
			line1.JI_LineNo = 1;
			AssertNoWarningContaining(line1.JI_LineNoInfo, "previous document");

			var shutup = new SendsMessagesToCustomsShutterUpperer(false);
			dec.DoMerge(shutup);
			line1.JI_LineNo = 1;
			line2.JI_LineNo = 2;
			AssertNoWarningContaining(line1.JI_LineNoInfo, "previous document");
			AssertNoWarningContaining(line2.JI_LineNoInfo, "previous document");
			AssertNoMessageErrorContaining(line1.JI_LineNoInfo, "previous document");
			AssertNoMessageErrorContaining(line2.JI_LineNoInfo, "previous document");

			dec.PreviousDocuments.RemoveAndDeleteAll();
			AssertNoWarningContaining(line1.JI_LineNoInfo, "previous document");
			AssertNoWarningContaining(line2.JI_LineNoInfo, "previous document");

			line2.PreviousDocuments.RemoveAndDeleteAll();
			line1.JI_LineNo = 1;
			line2.JI_LineNo = 2;
			AssertNoMessageErrorContaining(line1.JI_LineNoInfo, "previous document");
			AssertNoMessageErrorContaining(line2.JI_LineNoInfo, "previous document");

			pdHdr = hdr.PreviousDocuments.AddNew();
			pdHdr.CSI_SubType = "Z";
			line1.JI_LineNo = 1;
			line2.JI_LineNo = 2;
			AssertNoMessageErrorContaining(line1.JI_LineNoInfo, "previous document");
			AssertNoMessageErrorContaining(line2.JI_LineNoInfo, "previous document");

			hdr.PreviousDocuments.RemoveAndDeleteAll();
			dec.PreviousDocuments.RemoveAndDeleteAll();
			line2.PreviousDocuments.RemoveAndDeleteAll();
			line1.JI_LineNo = 1;
			line2.JI_LineNo = 2;
			hdr.JZ_InvoiceNumber = "";
			AssertHasWarningContaining(hdr.JZ_InvoiceNumberInfo, "previous document");
			AssertNoMessageErrorContaining(line1.JI_LineNoInfo, "previous document");
			AssertNoMessageErrorContaining(line2.JI_LineNoInfo, "previous document");

			dec.JE_DeclarationType = ExportSADDeclarationTypeList.Codes.ExitSummaryDeclaration;
			hdr.JZ_InvoiceNumber = "123";
			hdr.JZ_InvoiceNumber = "";
			AssertNoWarningContaining(hdr.JZ_InvoiceNumberInfo, "previous document");
			line1.JI_LineNo = 1;
			line2.JI_LineNo = 2;
			AssertNoMessageErrorContaining(line1.JI_LineNoInfo, "previous document");
			AssertNoMessageErrorContaining(line2.JI_LineNoInfo, "previous document");

			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration;
			hdr.JZ_InvoiceNumber = "123";
			hdr.JZ_InvoiceNumber = "";
			AssertNoWarningContaining(hdr.JZ_InvoiceNumberInfo, "previous document");
			line1.JI_LineNo = 1;
			line2.JI_LineNo = 2;
			AssertNoMessageErrorContaining(line1.JI_LineNoInfo, "previous document");
			AssertNoMessageErrorContaining(line2.JI_LineNoInfo, "previous document");
		}

		void RunCCRTariffTest(string importExport, string clearanceRequestCode, string fullDeclarationCode)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var clearanceRequestCPC = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "00", "11", "11", "111", "Test", importExport, clearanceRequestCode);
			var fullDecCPC = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "00", "22", "22", "222", "Test", importExport, fullDeclarationCode);
			Factory.Save();
			var declaration = GetJobDeclarationForTest();

			declaration.JE_MessageType = importExport;
			declaration.JE_DeclarationType = clearanceRequestCode;
			var invHeader = declaration.Invoices.AddNew();
			var line = invHeader.InvoiceLines.AddNew();
			line.JI_Procedure = clearanceRequestCPC.FullCodeCurrentPlusPreviousPlusConcession;
			line.JI_Tariff = "123456";
			AssertHasMessageErrorContaining(line.JI_TariffInfo, "Clearance request demands no commodity code");
			AssertNoMessageErrorContaining(line.JI_TariffInfo, "may not be empty");
			line.JI_Tariff = "";
			AssertNoMessageErrorContaining(line.JI_TariffInfo, "Clearance request demands no commodity code");
			AssertNoMessageErrorContaining(line.JI_TariffInfo, "may not be empty");
			line.JI_Procedure = fullDecCPC.FullCodeCurrentPlusPreviousPlusConcession;
			declaration.JE_DeclarationType = fullDeclarationCode;
			line.JI_Tariff = "123456";
			AssertNoMessageErrorContaining(line.JI_TariffInfo, "Clearance request demands no commodity code");
			line.JI_Tariff = "";
			AssertHasMessageErrorContaining(line.JI_TariffInfo, "may not be empty");
		}

		public void TestSupervisingOfficeValidation()
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var eun = universalReferenceTestDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			var uk = universalReferenceTestDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);
			universalReferenceTestDataHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupervisingOffice, "SupervisingOffice");
			var cusCode = universalReferenceTestDataHelper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupervisingOffice, "CKSPCODE1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "CK1";
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "CKSPCODE1");

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CK2";

			Factory.Save();

			JobDeclaration dec = GetJobDeclarationForTest();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.SupervisingOfficeDocAddress.E2_OA_Address = org.MainAddress.PK;

			AssertNoMessageErrors(invLine.SupervisingOfficeDocAddress.E2_OA_AddressInfo);

			invLine.SupervisingOfficeDocAddress.E2_OA_Address = org2.MainAddress.PK;
			AssertHasMessageErrorContaining(invLine.SupervisingOfficeDocAddress.E2_OA_AddressInfo, Business.Declaration.JobDeclarationValidation.InvalidSupervisingOffice_ErrorMessage);

			org.CustomsCodes.RemoveAll();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "CKSPCODE2");
			invLine.SupervisingOfficeDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertHasMessageErrorContaining(invLine.SupervisingOfficeDocAddress.E2_OA_AddressInfo, Business.Declaration.JobDeclarationValidation.SupervisingOfficeDoesNotExist_ErrorMessage);

			org.CustomsCodes.RemoveAll();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "CKSPCODE1");
			invLine.SupervisingOfficeDocAddress.E2_OA_Address = ZGuid.Empty;
			invLine.SupervisingOfficeDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertNoMessageErrors(invLine.SupervisingOfficeDocAddress.E2_OA_AddressInfo);

			invLine.SupervisingOfficeDocAddress.E2_OA_Address = org2.MainAddress.PK;
			AssertHasMessageErrorContaining(invLine.SupervisingOfficeDocAddress.E2_OA_AddressInfo, Business.Declaration.JobDeclarationValidation.InvalidSupervisingOffice_ErrorMessage);

			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			invLine.RunPreSaveValidation();
			AssertNoMessageErrors(invLine.SupervisingOfficeDocAddress.E2_OA_AddressInfo);
		}

		public void TestCheckZG_GoodsCategory()
		{
			var declaration = GetJobDeclarationForTest();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ReducedDataSetDeclaration;
			invoiceLine.ZG_GoodsCategory = GoodsCategoryList.Codes.Category1;
			AssertHasMessageError(invoiceLine.ZG_GoodsCategoryInfo, CDSAddInfoJobComInvoiceLineValidation.SPIMMCategory1Error);

			invoiceLine.ZG_GoodsCategory = "X";
			AssertHasMessageError(invoiceLine.ZG_GoodsCategoryInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.ZG_GoodsCategory = GoodsCategoryList.Codes.Category2;
			AssertNoMessageError(invoiceLine.ZG_GoodsCategoryInfo, CDSAddInfoJobComInvoiceLineValidation.SPIMMCategory1Error);
			AssertNoMessageError(invoiceLine.ZG_GoodsCategoryInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_RN_NKCountryOfExport_ForImport()
		{
			SetUpImportCountryRefData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			invoiceLine.JI_RN_NKCountryOfExport = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_RN_NKCountryOfExportInfo);

			invoiceLine.JI_RN_NKCountryOfExport = "XX";
			AssertHasMessageErrorContaining(invoiceLine.JI_RN_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_RN_NKCountryOfExport = "ZZ";
			AssertNoMessageErrors(invoiceLine.JI_RN_NKCountryOfExportInfo);
		}

		public void TestCheckJI_RN_NKCountryOfExport_ForExport()
		{
			SetUpImportCountryRefData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			invoiceLine.JI_RN_NKCountryOfExport = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_RN_NKCountryOfExportInfo);

			invoiceLine.JI_RN_NKCountryOfExport = "XX";
			AssertNoMessageErrors(invoiceLine.JI_RN_NKCountryOfExportInfo);

			invoiceLine.JI_RN_NKCountryOfExport = "ZZ";
			AssertNoMessageErrors(invoiceLine.JI_RN_NKCountryOfExportInfo);
		}

		void SetUpImportCountryRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping("CDS", parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM15, "Origin country/territory for entry style IM");
			helper.CreateCusCodeList("CDS", EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM15, "ZZ", "Test ZZ", ZDateTime.Now.AddMonths(-2), ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}

		protected JobDeclaration GetJobDeclarationForTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			return declaration;
		}

		protected Type ExpectedValidationType => typeof(CDSJobComInvoiceLineValidation);
	}
}
