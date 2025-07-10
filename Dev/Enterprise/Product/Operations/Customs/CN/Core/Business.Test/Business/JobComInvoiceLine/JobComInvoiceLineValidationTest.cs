using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.CN.Business.Constants;
using static Enterprise.Customs.CN.Business.Constants.UniversalReferenceConstants;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.CN.Business.Testing
{
	class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateDangerousGoodsDGSubs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNDangerousChemical, "China Dangerous Chemical");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeType.ZZK_CodeType, "7664-41-7", "氨", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_NameOfGoods = "氨";
			var substancePK = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			invoiceLine.DangerousGoodsDGSubs = substancePK;
			AssertNoWarnings(invoiceLine.DangerousGoodsDGSubsInfo);
			invoiceLine.DangerousGoods.DI_IMOClass = "tt";
			invoiceLine.DangerousGoodsDGSubs = ZGuid.Empty;
			AssertNoNotifications(invoiceLine.DangerousGoodsDGSubsInfo);
			invoiceLine.DangerousGoodsDGSubs = ZGuid.Invalid;
			AssertHasErrorContaining(invoiceLine.DangerousGoodsDGSubsInfo, ListValidation.InvalidCodeError);
			invoiceLine.DangerousGoodsDGSubs = substancePK;
			AssertNoNotifications(invoiceLine.DangerousGoodsDGSubsInfo);
			var testCollection = invoiceLine.CargoAttributes as ICodeDescriptionOptionStorage;
			testCollection.AddNew(CargoAttributeList.Codes._31);
			invoiceLine.DangerousGoodsDGSubs = ZGuid.Empty;
			invoiceLine.Validation.ValidateDangerousGoodsDGSubs();
			AssertHasMessageErrorContaining(invoiceLine.DangerousGoodsDGSubsInfo, MandatoryValidation.YouHaveNotEntered);
			testCollection.RemoveAndDelete(testCollection.FindByCode(CargoAttributeList.Codes._31));
			testCollection.AddNew(CargoAttributeList.Codes._32);
			invoiceLine.Validation.ValidateDangerousGoodsDGSubs();
			AssertHasMessageErrorContaining(invoiceLine.DangerousGoodsDGSubsInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.DangerousGoodsDGSubs = substancePK;
			invoiceLine.Validation.ValidateDangerousGoodsDGSubs();
			AssertNoMessageErrorContaining(invoiceLine.DangerousGoodsDGSubsInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckItemNoOnCertOfOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invHeader = declaration.Invoices.AddNew();
			var invLine1 = (JobComInvoiceLine)invHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invLine1.JI_CEI = instruction.PK;
			invLine1.JI_CL = entryLine1.PK;
			invLine1.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			invLine1.Validation.ValidateItemNoOnCertOfOrigin();
			AssertHasMessageErrorContaining(invLine1.ItemNoOnCertOfOriginInfo, MandatoryValidation.ValueCannotBeZero);
			invLine1.ItemNoOnCertOfOrigin = -1;
			AssertHasMessageErrorContaining(invLine1.ItemNoOnCertOfOriginInfo, " cannot be negative.");
			invLine1.ItemNoOnCertOfOrigin = 1;
			AssertNoMessageErrors(invLine1.ItemNoOnCertOfOriginInfo);
		}

		public void TestParent()
		{
			var parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestJI_StateOrRegionOfOrigin()
		{
			var refCountry = Factory.NewWithValidTestData<RefCountry>();
			refCountry.RN_Code = "AR";
			var states1 = Factory.NewWithValidTestData<RefCountryStates>();
			states1.RW_Code = "A";
			states1.RW_RN_NKCountryCode = refCountry.RN_Code;
			var states2 = Factory.NewWithValidTestData<RefCountryStates>();
			states2.RW_Code = "YY";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "AR";
			invoiceLine.JI_StateOrRegionOfOrigin = "XX";
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_StateOrRegionOfOrigin = "YY";
			AssertHasMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_StateOrRegionOfOrigin = "A";
			AssertNoMessageErrorContaining(invoiceLine.JI_StateOrRegionOfOriginInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_CEI()
		{
			InvoiceLine.JI_CEI = ZGuid.Empty;
			var targetInfo = InvoiceLine.JI_CEIInfo;
			AssertHasMessageError(targetInfo, MandatoryValidation.YouHaveNotEnteredMessage(GetHumanReadableName(targetInfo)));
			InvoiceLine.JI_CEI = ZGuid.Invalid;
			AssertHasError(targetInfo, "Enter a valid Entry Instruction.");
			var cusProc = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.China, "", "1011", "", "", "Customs Procedure 1011", "IMP,EXP");
			var cusEntryInstruction = Declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = cusProc.ZZ6_ProcedureCode;
			InvoiceLine.JI_CEI = cusEntryInstruction.PK;
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckJI_PrimaryPreference_WarningForLowerDuty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dtyRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "DTY");
			Factory.Save();
			var dtyRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dtyRateType.PK);
			Factory.Save();
			var ftaPreference = helper.CreatePreferenceForCountry("FTA", "FTA", Core.Constants.CountryCodes.China);
			var mfnPreference = helper.CreatePreferenceForCountry("MFN", "MFN", Core.Constants.CountryCodes.China);
			var testTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.China, "TEST", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(testTradeGroup, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();
			var tariffA = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TESTA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var aFTARateA = helper.CreateRate(tariffA, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "0.1 * CV", preferencePk: ftaPreference.PK, rateFormulaDeriveFrom: "10");
			var aMFNRateA = helper.CreateRate(tariffA, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "0.2 * CV", preferencePk: mfnPreference.PK, rateFormulaDeriveFrom: "20");
			helper.CreateCusApplicability(aFTARateA, testTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, additionalCode: "02");
			helper.CreateCusApplicability(aMFNRateA, testTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariffB = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TESTB", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var aFTARateB = helper.CreateRate(tariffB, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "0.4 * CV", preferencePk: ftaPreference.PK, rateFormulaDeriveFrom: "40");
			var aMFNRateB = helper.CreateRate(tariffB, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "0.2 * CV", preferencePk: mfnPreference.PK, rateFormulaDeriveFrom: "20");
			helper.CreateCusApplicability(aFTARateB, testTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, additionalCode: "02");
			helper.CreateCusApplicability(aMFNRateB, testTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invHeader = declaration.Invoices.AddNew();
			var invLine1 = invHeader.JobComInvoiceLines.AddNew();
			var invLine2 = invHeader.JobComInvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invLine1.JI_Tariff = tariffA.ZZ1_TariffCode;
			invLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invLine1.JI_PrimaryPreference = ftaPreference.ZZS_Preference;
			invLine1.JI_SecondaryPreference = "02";
			invLine1.Validation.ValidateJI_PrimaryPreference();
			AssertNoNotifications("FTA rate is lower than MFN rate", invLine1.JI_PrimaryPreferenceInfo);
			invLine2.JI_Tariff = tariffB.ZZ1_TariffCode;
			invLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invLine2.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			invLine2.JI_SecondaryPreference = "02";
			invLine2.Validation.ValidateJI_PrimaryPreference();
			AssertHasWarning("MFN rate is lower than FTA rate then show warning", invLine2.JI_PrimaryPreferenceInfo, "Most favored nation rate (20%) is lower than the applicable rate for the selected Preference. It is recommended to select ‘MFN’.");
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckJI_Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dtyRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "DTY");
			Factory.Save();
			var dtyRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dtyRateType.PK);
			Factory.Save();
			var stdPreference = helper.CreatePreferenceForCountry("STANDARD", "STANDARD", Core.Constants.CountryCodes.China);
			var mfnPreference = helper.CreatePreferenceForCountry("MFN", "MFN", Core.Constants.CountryCodes.China);
			Factory.Save();
			var cndocConditionType = helper.CreateOrGetExistingRefCusConditionType("CN", "CTRL", "CNDOC", "Customs Required Documents");
			var impphConditionType = helper.CreateOrGetExistingRefCusConditionType("CN", "CTRL", "IMPPH", "Import Prohibition");
			var expphConditionType = helper.CreateOrGetExistingRefCusConditionType("CN", "CTRL", "EXPPH", "Export Prohibition");
			var rateConditionType = helper.CreateOrGetExistingRefCusConditionType("CN", "RATE", "RATE", "Customs Rate requirement");
			var docConditionValueType = helper.CreateOrGetExistingRefCusConditionValueType("CN", "DOC");
			var prohConditionValueType = helper.CreateOrGetExistingRefCusConditionValueType("CN", "PROH");
			var tariffA = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TESTA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariffB = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TESTB", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariffC = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TESTC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateRate(tariffA, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, preferencePk: stdPreference.PK);
			helper.CreateRate(tariffA, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, preferencePk: mfnPreference.PK);
			helper.CreateRate(tariffB, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, preferencePk: stdPreference.PK);
			helper.CreateRate(tariffC, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, preferencePk: stdPreference.PK);
			helper.CreateSingleValueConditionForTariff(tariffA.PK, true, false, cndocConditionType.PK, "02", docConditionValueType.PK, "02");
			helper.CreateSingleValueConditionForTariff(tariffA.PK, false, true, cndocConditionType.PK, "03", docConditionValueType.PK, "03");
			helper.CreateSingleValueConditionForTariff(tariffA.PK, true, true, cndocConditionType.PK, "0a", docConditionValueType.PK, "0a");
			helper.CreateSingleValueConditionForTariff(tariffA.PK, true, false, rateConditionType.PK, "0r", docConditionValueType.PK, "0r", mfnPreference.PK);
			helper.CreateSingleValueConditionForTariff(tariffB.PK, true, false, cndocConditionType.PK, "0a", docConditionValueType.PK, "0a");
			helper.CreateSingleValueConditionForTariff(tariffB.PK, false, true, expphConditionType.PK, "EXP STOP", prohConditionValueType.PK, "whatever", trueMeansStop: true);
			helper.CreateSingleValueConditionForTariff(tariffC.PK, true, false, impphConditionType.PK, "IMP STOP", prohConditionValueType.PK, "whatever", trueMeansStop: true);
			helper.CreateSingleValueConditionForTariff(tariffC.PK, false, true, expphConditionType.PK, "EXP STOP", prohConditionValueType.PK, "whatever", trueMeansStop: true);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invHeader.InvoiceLines.AddNew();
			declaration.JE_MessageType = "IMP";
			invoiceLine.JI_Tariff = ZString.Empty;
			var targetInfo = invoiceLine.JI_TariffInfo;
			AssertEquals("JI_Tariff should have 1 Message Errors", 1, targetInfo.Notifications.Count());
			invoiceLine.JI_Tariff = "XXX";
			AssertEquals("JI_Tariff should have 1 Message Errors", 1, targetInfo.Notifications.Count());
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessage);
			invoiceLine.JI_Tariff = tariffA.ZZ1_TariffCode;
			instruction.CEI_Style = CNRefCusProcedure.Codes._0130;
			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasWarningContaining(invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Customs Required Documents:
        02 AND
        0a");
				var newDoc = invoiceLine.CusSupportingDocuments.AddNew();
				newDoc.CSI_Code = "02";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasWarningContaining(invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Customs Required Documents:
        02 AND
        0a");
				newDoc.CSI_ReferenceNumber = "02";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasWarningContaining(invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Customs Required Documents:
        0a");
				declaration.JE_MessageType = "EXP";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasWarningContaining(invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Customs Required Documents:
        03 AND
        0a");
				invoiceLine.JI_Tariff = tariffB.ZZ1_TariffCode;
				AssertHasWarningContaining(invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Export Prohibition:
        EXP STOP");
				invoiceLine.JI_Tariff = tariffC.ZZ1_TariffCode;
				AssertHasWarningContaining(invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Export Prohibition:
        EXP STOP");
				declaration.JE_MessageType = "IMP";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasWarningContaining(invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Import Prohibition:
        IMP STOP");
			}

			);
			instruction.CEI_Style = CNRefCusProcedure.Codes._0110;
			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Import Prohibition:
        IMP STOP");
				var newDoc = invoiceLine.CusSupportingDocuments.AddNew();
				newDoc.CSI_Code = "02";
				newDoc.CSI_ReferenceNumber = "02";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Import Prohibition:
        IMP STOP");
				declaration.JE_MessageType = "EXP";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, @"The Customs Control condition is not satisfied
    Export Prohibition:
        EXP STOP");
			}

			);
		}

		public void TestCheckJI_Tariff_ValidateUSDForCVINUSD()
		{
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			usdCurrency.ExchangeRates.DeleteAll();
			Factory.Save();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dtyRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "DTY");
			Factory.Save();
			var dtyRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dtyRateType.PK);
			Factory.Save();
			var stdPreference = helper.CreatePreferenceForCountry("STANDARD", "STANDARD", Core.Constants.CountryCodes.China);
			var mfnPreference = helper.CreatePreferenceForCountry("MFN", "MFN", Core.Constants.CountryCodes.China);
			Factory.Save();
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.China, "TEST", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var tariffA = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TESTA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var stdRate = helper.CreateRate(tariffA, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "0.2 * CV", preferencePk: stdPreference.PK);
			var mfnRate = helper.CreateRate(tariffA, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "0.2 * CVINUSD", preferencePk: mfnPreference.PK);
			helper.CreateCusApplicability(stdRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(mfnRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			var invHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invHeader.InvoiceLines.AddNew();
			declaration.JE_MessageType = "IMP";
			invoiceLine.JI_Tariff = "TESTA";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			var targetInfo = invoiceLine.JI_TariffInfo;
			invoiceLine.JI_PrimaryPreference = "STANDARD";
			AssertNoNotifications(targetInfo);
			invoiceLine.JI_PrimaryPreference = "MFN";
			AssertHasMessageErrorContaining(targetInfo, "There is no valid customs exchange rate for USD");
		}

		public void TestCheckJI_Tariff_ValidateUsedProducts()
		{
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TESTUMF", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "UME", tariff);
			helper.CreateNewOrGetExistingCusCodeType("CNDOC", "CN Doc");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CNDOC", "01", "进口许可证", minDate, maxDate);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			var invHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invHeader.InvoiceLines.AddNew();
			declaration.JE_MessageType = "IMP";
			invoiceLine.JI_Tariff = "TESTA";
			var targetInfo = invoiceLine.JI_TariffInfo;
			var warning = "The goods seems to be used mechanical and electrical products, supporting document 进口许可证 is required.";
			CombineAssertions("Used mechanical and electrical products Warning ", () =>
			{
				invoiceLine.JI_NameOfGoods = "旧";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoWarningContaining(targetInfo, warning);
				invoiceLine.JI_Tariff = "TESTUMF";
				AssertHasWarningContaining(targetInfo, warning);
				invoiceLine.JI_NameOfGoods = "other";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoWarningContaining(targetInfo, warning);
				invoiceLine.CargoAttributes.AddNew("21");
				declaration.JE_MessageType = "EXP";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoWarningContaining(targetInfo, warning);
				declaration.JE_MessageType = "IMP";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasWarningContaining(targetInfo, warning);
				invoiceLine.JI_Tariff = "other";
				AssertNoWarningContaining(targetInfo, warning);
				invoiceLine.JI_Tariff = "TESTUMF";
				AssertHasWarningContaining(targetInfo, warning);
				invoiceLine.CusSupportingDocuments.AddNew("01", "11");
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoWarningContaining(targetInfo, warning);
			}

			);
		}

		public void TestCheckJI_Description()
		{
			InvoiceLine.JI_Description = ZString.Empty;
			var targetInfo = InvoiceLine.JI_DescriptionInfo;
			AssertHasWarning(targetInfo, MandatoryValidation.YouHaveNotEnteredMessage(GetHumanReadableName(targetInfo)));
			InvoiceLine.JI_Description = "Goods description";
			AssertNoWarnings(targetInfo);
		}

		public void TestCheckJI_InvoiceUQ()
		{
			InvoiceLine.JI_InvoiceUQ = "IUQ";
			var targetInfo = InvoiceLine.JI_InvoiceUQInfo;
			AssertHasMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_InvoiceUQ = "KG";
			AssertNoMessageErrors(targetInfo);
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckJI_CustomsQuantity()
		{
			InvoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			InvoiceLine.JI_CustomsQuantity = 100000000000000m;
			var targetInfo = InvoiceLine.JI_CustomsQuantityInfo;
			AssertHasErrorContaining(targetInfo, "is too large");
			InvoiceLine.JI_CustomsQuantity = 9999999999999.999999m;
			AssertNoErrors(targetInfo);
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckJI_CustomsUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Unit Quantity");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "003", "Unit 1", new ZDateTime(2016, 1, 1), ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", tariffUOM_CUS1.ZZ8_UOM, "Unit 2", new ZDateTime(2016, 1, 1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			InvoiceLine.JI_Tariff = "111";
			AssertEquals("Customs Unit Quantity is not readonly when tariff code is invalid", false, InvoiceLine.JI_CustomsUnitQty_ReadOnly);
			InvoiceLine.JI_CustomsUnitQty = "UQ";
			var targetInfo = InvoiceLine.JI_CustomsUnitQtyInfo;
			AssertHasMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_CustomsUnitQty = "003";
			AssertNoMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertHasMessageError(targetInfo, MandatoryValidation.YouHaveNotEnteredMessage(GetHumanReadableName(targetInfo)));
			InvoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			AssertEquals("Customs Unit Quantity is readonly when tariff code is valid", true, InvoiceLine.JI_CustomsUnitQty_ReadOnly);
			AssertEquals("Customs Unit Quantity is set to predefined tariff UOM value", tariffUOM_CUS1.ZZ8_UOM, InvoiceLine.JI_CustomsUnitQty);
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckJI_CustomsSecondQuantity()
		{
			InvoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
			AssertEquals("Customs Second Quantity is readonly when Customs Second Unit Quantity is empty", true, InvoiceLine.JI_CustomsSecondQuantity_ReadOnly);
			InvoiceLine.JI_CustomsSecondUnitQty = "AA";
			AssertEquals("Customs Second Quantity is NOT readonly when Customs Second Unit Quantity is NOT empty", false, InvoiceLine.JI_CustomsSecondQuantity_ReadOnly);
			InvoiceLine.JI_CustomsSecondQuantity = 100000000000000m;
			var targetInfo = InvoiceLine.JI_CustomsSecondQuantityInfo;
			AssertHasErrorContaining(targetInfo, "is too large");
			invoiceLine.JI_CustomsSecondQuantity = 9999999999999.99999m;
			AssertNoErrors(targetInfo);
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckJI_OA_ManufacturerAddress()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var declaration = testItems.JobDeclaration;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var manufacturer = Factory.New<OrgHeader>();
			var address = (OrgAddress)manufacturer.Addresses.FirstOrDefault() ?? manufacturer.Addresses.AddNew();
			var targetInfo = testItems.InvoiceLine.JI_OA_ManufacturerAddressInfo;
			testItems.InvoiceLine.JI_CIQTariff = "1111";
			testItems.EntryInstruction.CEI_CIQRequires = true;
			testItems.InvoiceLine.JI_OA_ManufacturerAddress = address.PK;
			var ciqRequiredMessage = "CIQ Registration Number for CN is required. Please press F3 on the Organization, go to Details -> Config -> Registration Numbers/Codes and enter the Registration Number";
			AssertHasMessageErrorContaining(targetInfo, ciqRequiredMessage);
			AssertHasMessageErrorContaining(targetInfo, "The Manufacturer Address should have a Simplified Chinese translation.");
			var ciqReg = address.CustomsCodes.AddNew();
			ciqReg.OK_CodeType = "CIQ";
			ciqReg.OK_CustomsRegNo = "CIQ0000001";
			testItems.InvoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(targetInfo, ciqRequiredMessage);
			address.TranslatedAddresses.AddNew().Language = Core.Constants.Languages.ChineseSimplified;
			testItems.InvoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			AssertNoMessageErrors(targetInfo);
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckJI_CustomsSecondUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Unit Quantity");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "001", "Unit 1", new ZDateTime(2016, 1, 1), ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", tariffUOM_CUS2_1.ZZ8_UOM, "Unit 2", new ZDateTime(2016, 1, 1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			InvoiceLine.JI_Tariff = "111";
			AssertEquals("Customs Second Unit Quantity is NOT readonly when tariff code is invalid", false, InvoiceLine.JI_CustomsSecondUnitQty_ReadOnly);
			InvoiceLine.JI_CustomsSecondUnitQty = "UQ";
			var targetInfo = InvoiceLine.JI_CustomsSecondUnitQtyInfo;
			AssertHasMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_CustomsSecondUnitQty = "001";
			AssertNoMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			AssertEquals("1. Customs Second Unit Quantity is readonly when tariff code is valid", true, InvoiceLine.JI_CustomsSecondUnitQty_ReadOnly);
			AssertEquals("1. Customs Second Unit Quantity is set to predefined tariff UOM value", tariffUOM_CUS2_1.ZZ8_UOM, InvoiceLine.JI_CustomsSecondUnitQty);
			AssertNoMessageErrors(targetInfo);
			InvoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			AssertEquals("2. Customs Second Unit Quantity is readonly when tariff code is valid", true, InvoiceLine.JI_CustomsSecondUnitQty_ReadOnly);
			AssertEquals("2. Customs Second Unit Quantity is empty due to missing tariff UOM value", "UUU", InvoiceLine.JI_CustomsSecondUnitQty);
			AssertHasMessageErrorContaining(InvoiceLine.JI_CustomsSecondUnitQtyInfo, "The code you have selected is not in the list");
		}

		public void TestCheckJI_CountryOfOrigin()
		{
			var targetInfo = InvoiceLine.JI_CountryOfOriginInfo;
			InvoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(targetInfo, "Could not find an ISO Alpha-3 Code for");
			InvoiceLine.JI_CountryOfOrigin = "12";
			AssertHasMessageErrorContaining(targetInfo, "Could not find an ISO Alpha-3 Code for");
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckJI_RN_NKCountryOfExport()
		{
			InvoiceLine.JI_RN_NKCountryOfExport = ZString.Empty;
			var targetInfo = InvoiceLine.JI_RN_NKCountryOfExportInfo;
			AssertHasMessageError(targetInfo, MandatoryValidation.YouHaveNotEnteredMessage(GetHumanReadableName(targetInfo)));
			InvoiceLine.JI_RN_NKCountryOfExport = "12";
			AssertHasMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckJI_Weight()
		{
			var lessThanOrEqualToZeroMessage = "Please enter a 'Gross Weight' greater than 0.";
			var targetInfo = InvoiceLine.JI_WeightInfo;
			InvoiceLine.JI_Weight = -1;
			AssertHasMessageErrorContaining(targetInfo, lessThanOrEqualToZeroMessage);
			InvoiceLine.JI_Weight = 0;
			AssertHasMessageErrorContaining(targetInfo, lessThanOrEqualToZeroMessage);
			InvoiceLine.JI_Weight = 1;
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckJI_NetWeight()
		{
			var lessThanOrEqualToZeroMessage = "Please enter a 'Net Weight' greater than 0.";
			var targetInfo = InvoiceLine.JI_NetWeightInfo;
			InvoiceLine.JI_NetWeight = -1;
			AssertHasMessageErrorContaining(targetInfo, lessThanOrEqualToZeroMessage);
			InvoiceLine.JI_NetWeight = 0;
			AssertHasMessageErrorContaining(targetInfo, lessThanOrEqualToZeroMessage);
			InvoiceLine.JI_NetWeight = 1;
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckJI_WeightUQ()
		{
			InvoiceLine.JI_WeightUQ = "XX";
			var targetInfo = InvoiceLine.JI_WeightUQInfo;
			AssertHasMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_WeightUQ = ZString.Empty;
			AssertHasMessageError(targetInfo, MandatoryValidation.YouHaveNotEnteredMessage(GetHumanReadableName(targetInfo)));
			InvoiceLine.JI_WeightUQ = "KG";
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckJI_NetWeightUQ()
		{
			InvoiceLine.JI_NetWeightUQ = "XX";
			var targetInfo = InvoiceLine.JI_NetWeightUQInfo;
			AssertHasMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.JI_NetWeightUQ = ZString.Empty;
			AssertHasMessageError(targetInfo, MandatoryValidation.YouHaveNotEnteredMessage(GetHumanReadableName(targetInfo)));
			InvoiceLine.JI_NetWeightUQ = "KG";
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckJI_Procedure()
		{
			var testInvoiceLine = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save()).InvoiceLine;
			testInvoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageErrors(testInvoiceLine.JI_ProcedureInfo);
		}

		public void TestCheckJI_InvoiceQuantity()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_InvoiceQuantity = -10;
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
			invoiceLine.JI_InvoiceQuantity = 10;
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckJI_LinePriceBound()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, MandatoryValidation.ValueCannotBeZero);
			invoiceLine.JI_LinePrice = -10;
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, MandatoryValidation.ValueCannotBeZero);
			invoiceLine.JI_LinePrice = 10;
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckTradeAgreementCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CountryOfOrigin = "AU";
			var info = invoiceLine.TradeAgreementCodeInfo;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			invoiceLine.Validation.ValidateTradeAgreementCode();
			AssertNoMessageErrors(info);
			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			AssertEquals("02", invoiceLine.TradeAgreementCode);
			AssertNoMessageErrors(info);
			invoiceLine.TradeAgreementCode = ZString.Empty;
			AssertHasMessageError(info, MandatoryValidation.YouHaveNotEnteredMessage(GetHumanReadableName(info)));
			invoiceLine.TradeAgreementCode = "AAA";
			invoiceLine.Validation.ValidateTradeAgreementCode();
			AssertHasMessageError(info, ListValidation.InvalidCodeMessageError);
			invoiceLine.TradeAgreementCode = "02";
			invoiceLine.Validation.ValidateTradeAgreementCode();
			AssertNoMessageErrors(info);
			invoiceLine.TradeAgreementCode = "03";
			invoiceLine.Validation.ValidateTradeAgreementCode();
			AssertHasMessageError(info, ListValidation.InvalidCodeMessageError);
			invoiceLine.Validation.ValidateTradeAgreementCode();
			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.Normal;
			AssertEquals(ZString.Empty, invoiceLine.TradeAgreementCode);
			AssertNoMessageErrors(info);
			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.MostFavouredNations;
			invoiceLine.Validation.ValidateTradeAgreementCode();
			AssertEquals(ZString.Empty, invoiceLine.TradeAgreementCode);
			AssertNoMessageErrors(info);
		}

		public void TestCheckFormulaPricingRecordNumber()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.FormulaPricingRecordNumber = "9 99";
			invoiceLine.Validation.ValidateFormulaPricingRecordNumber();
			AssertHasMessageErrorContaining(invoiceLine.FormulaPricingRecordNumberInfo, "Reference Number cannot contain white space.");
			invoiceLine.FormulaPricingRecordNumber = "999";
			invoiceLine.Validation.ValidateFormulaPricingRecordNumber();
			AssertNoMessageErrorContaining(invoiceLine.FormulaPricingRecordNumberInfo, "Reference Number cannot contain white space.");
		}

		public void TestCheckTestCheckTradeAgreementCode_SupportsDeclarationOfOrigin()
		{
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypesCodes.CNPreferentialTradeAgreement, "10", "中国—新西兰自贸协定", new[] { (CusCodeListAttributeName.SupportsDeclarationOfOrigin, "Y") });
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypesCodes.CNPreferentialTradeAgreement, "02", "中国—东盟自贸协定");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLineToTest = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLineToTest.JI_CountryOfOrigin = "AU";
			invoiceLineToTest.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			var info = invoiceLineToTest.TradeAgreementCodeInfo;

			invoiceLineToTest.TradeAgreementCode = "02";
			AssertNoMessageErrors(info);
			invoiceLineToTest.CertificateOfOriginType = "D";
			invoiceLineToTest.TradeAgreementCode = "02";
			AssertHasMessageError(info, "This Preferential Code does not support Declaration of Origin, please check the COO Type and Preferential Code before submitting.");
			invoiceLineToTest.TradeAgreementCode = "10";
			AssertNoMessageErrors(info);

			invoiceLineToTest.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.LeastDevelopedCountries;
			invoiceLineToTest.TradeAgreementCode = TradeAgreementCodes.Codes.LDC;
			AssertNoMessageErrors(info);
		}

		public void TestCheckCargoAttributesAsString()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, MandatoryValidation.YouHaveNotEntered);
			instruction.CEI_CIQRequires = true;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertHasMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, MandatoryValidation.YouHaveNotEntered);
			var cargoAttribute = Factory.New<CargoAttribute>();
			cargoAttribute.CY_ParentID = invoiceLine.PK;
			cargoAttribute.CY_ParentTableCode = invoiceLine.TablePrefix;
			cargoAttribute.CY_Type = Constants.CusCodeDataTypes.Codes.CargoAttribute;
			cargoAttribute.CY_Code = CargoAttributeList.Codes._21;
			invoiceLine.CargoAttributes.Add(cargoAttribute);
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ciqProductQualification = invoiceLine.CIQProductQualifications.AddNew();
			ciqProductQualification.CSI_ReferenceNumber = "NUM1";
			var firstAttribute = invoiceLine.CargoAttributes[0];
			firstAttribute.CY_Code = CargoAttributeList.Codes._11;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertHasMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 411 is required for Cargo Attribute 11");
			ciqProductQualification.CSI_Code = ProductQualificationCodeList.Codes._411;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 411 is required for Cargo Attribute 11");
			invoiceLine.JI_CIQEndUse = ZString.Empty;
			Assert(invoiceLine.JI_CIQEndUse.IsEmpty);
			((ICodeDescriptionOptionStorage)invoiceLine.CargoAttributes).AddNew(firstAttribute.CY_Code = CargoAttributeList.Codes._12);
			AssertEquals(EndUseList.Codes.OnlyIndustrialUse, invoiceLine.JI_CIQEndUse);
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertHasMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 410 is required for Cargo Attribute 12");
			ciqProductQualification.CSI_Code = ProductQualificationCodeList.Codes._410;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 410 is required for Cargo Attribute 12");
			ciqProductQualification.CSI_Code = ProductQualificationCodeList.Codes._411;
			firstAttribute.CY_Code = CargoAttributeList.Codes._13;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertHasMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 410 is required for Cargo Attribute 13");
			ciqProductQualification.CSI_Code = ProductQualificationCodeList.Codes._410;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 410 is required for Cargo Attribute 13");
			firstAttribute.CY_Code = CargoAttributeList.Codes._14;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertHasMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 517,519,401 is required for Cargo Attribute 14");
			ciqProductQualification.CSI_Code = ProductQualificationCodeList.Codes._517;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 517,519,401 is required for Cargo Attribute 14");
			ciqProductQualification.CSI_Code = ProductQualificationCodeList.Codes._103;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertHasMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 517,519,401 is required for Cargo Attribute 14");
			ciqProductQualification.CSI_Code = ProductQualificationCodeList.Codes._519;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 517,519,401 is required for Cargo Attribute 14");
			ciqProductQualification.CSI_Code = ProductQualificationCodeList.Codes._401;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 517,519,401 is required for Cargo Attribute 14");
			var secondAttribute = invoiceLine.CargoAttributes[1];
			secondAttribute.CY_Code = CargoAttributeList.Codes._18;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertHasMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 519 is required for Cargo Attribute 14");
			firstAttribute.CY_Code = CargoAttributeList.Codes._15;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 519 is required for Cargo Attribute 14");
			AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 517 is NOT required for Cargo Attribute 15");
			ciqProductQualification.CSI_Code = ProductQualificationCodeList.Codes._517;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertHasMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 517 is NOT required for Cargo Attribute 15");
			var codes = new[] { CargoAttributeList.Codes._25, CargoAttributeList.Codes._26, CargoAttributeList.Codes._27, CargoAttributeList.Codes._28, CargoAttributeList.Codes._29 };
			foreach (var code in codes)
			{
				ciqProductQualification.CSI_Code = ProductQualificationCodeList.Codes._517;
				firstAttribute.CY_Code = code;
				invoiceLine.Validation.ValidateCargoAttributesAsString();
				AssertHasMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 203,401 is required for Cargo Attribute " + code);
				ciqProductQualification.CSI_Code = ProductQualificationCodeList.Codes._203;
				invoiceLine.Validation.ValidateCargoAttributesAsString();
				AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 203,401 is required for Cargo Attribute " + code);
				ciqProductQualification.CSI_Code = ProductQualificationCodeList.Codes._401;
				invoiceLine.Validation.ValidateCargoAttributesAsString();
				AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, "Product Qualification 203,401 is required for Cargo Attribute " + code);
			}

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TESTUMF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "UME", tariff);
			Factory.Save();
			new[] { EndUseList.Codes.Edible, EndUseList.Codes.Cosmetics }.ForEach(x =>
			{
				var msg = $"Cargo Attribute 14,15 needs to be selected for End Use ‘{x}’.";
				invoiceLine.Validation.ValidateCargoAttributesAsString();
				AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, msg);
				invoiceLine.JI_CIQEndUse = x;
				firstAttribute.CY_Code = CargoAttributeList.Codes._13;
				invoiceLine.Validation.ValidateCargoAttributesAsString();
				AssertHasMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, msg);
				firstAttribute.CY_Code = CargoAttributeList.Codes._14;
				invoiceLine.Validation.ValidateCargoAttributesAsString();
				AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, msg);
				firstAttribute.CY_Code = CargoAttributeList.Codes._15;
				invoiceLine.Validation.ValidateCargoAttributesAsString();
				AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, msg);
				invoiceLine.JI_CIQEndUse = ZString.Empty;
				invoiceLine.Validation.ValidateCargoAttributesAsString();
				AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, msg);
			});
		}

		public void TestCheckRequiredAttachmentTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_CIQRequires = true;
			instruction.Attachments.AddNew().AttachmentType = CSDDocTypeList.Codes._80000001;
			instruction.Attachments.AddNew().AttachmentType = CSDDocTypeList.Codes._80000002;
			instruction.Attachments.AddNew().AttachmentType = CSDDocTypeList.Codes._80000003;
			instruction.Attachments.AddNew().AttachmentType = CSDDocTypeList.Codes._80000004;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_LineNo = 1;
			var expectedMessage = "should be linked to attachment type as";
			var attachmentLinks = invoiceLine.AttachmentLinks;

			var cargoAttribute = invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._33);
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, expectedMessage);

			cargoAttribute.CY_Code = CargoAttributeList.Codes._31;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			AssertNoMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, expectedMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			CombineAssertions("CargoAttribute 31", () =>
			{
				AssertHasMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, expectedMessage);
				AssertHasMessageError("80000001 link required", invoiceLine.CargoAttributesAsStringInfo, "Invoice line: 1 should be linked to attachment type as 80000001 when Cargo Attributes is 31");
				AssertHasMessageError("80000004 link required", invoiceLine.CargoAttributesAsStringInfo, "Invoice line: 1 should be linked to attachment type as 80000004 when Cargo Attributes is 31");
			});

			cargoAttribute.CY_Code = CargoAttributeList.Codes._32;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			CombineAssertions("CargoAttribute 32", () =>
			{
				AssertHasMessageErrorContaining(invoiceLine.CargoAttributesAsStringInfo, expectedMessage);
				AssertHasMessageError("80000001 link required", invoiceLine.CargoAttributesAsStringInfo, "Invoice line: 1 should be linked to attachment type as 80000001 when Cargo Attributes is 32");
				AssertHasMessageError("80000003 link required", invoiceLine.CargoAttributesAsStringInfo, "Invoice line: 1 should be linked to attachment type as 80000003 when Cargo Attributes is 32");
				AssertHasMessageError("80000004 link required", invoiceLine.CargoAttributesAsStringInfo, "Invoice line: 1 should be linked to attachment type as 80000004 when Cargo Attributes is 32");
			});

			invoiceLine.AttachmentLinks.Cast<AttachmentInvoiceLineLink>().ForEach(x => x.IsLinked = true);
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			CombineAssertions("CargoAttribute 32", () =>
			{
				AssertNoMessageError("80000001 link linked", invoiceLine.CargoAttributesAsStringInfo, "Invoice line: 1 should be linked to attachment type as 80000001 when Cargo Attributes is 32");
				AssertNoMessageError("80000003 link linked", invoiceLine.CargoAttributesAsStringInfo, "Invoice line: 1 should be linked to attachment type as 80000003 when Cargo Attributes is 32");
				AssertNoMessageError("80000004 link linked", invoiceLine.CargoAttributesAsStringInfo, "Invoice line: 1 should be linked to attachment type as 80000004 when Cargo Attributes is 32");
			});

			cargoAttribute.CY_Code = CargoAttributeList.Codes._31;
			invoiceLine.Validation.ValidateCargoAttributesAsString();
			CombineAssertions("CargoAttribute 31", () =>
			{
				AssertNoMessageError("80000001 link linked", invoiceLine.CargoAttributesAsStringInfo, "Invoice line: 1 should be linked to attachment type as 80000001 when Cargo Attributes is 31");
				AssertNoMessageError("80000004 link linked", invoiceLine.CargoAttributesAsStringInfo, "Invoice line: 1 should be linked to attachment type as 80000004 when Cargo Attributes is 31");
			});
		}

		public void TestCheckCertificateOfOriginsAcrossInstruction()
		{
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			var invoiceLine1 = testItem.InvoiceLine;
			invoiceLine1.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			var invoiceLine2 = (JobComInvoiceLine)testItem.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testItem.EntryInstruction.PK;
			invoiceLine2.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			var invLn1Doc1 = invoiceLine1.CusSupportingDocuments.AddNew();
			invLn1Doc1.CSI_Code = Constants.DocumentCodes.CertificateOfOrigin;
			var invLn2Doc1 = invoiceLine2.CusSupportingDocuments.AddNew();
			invLn2Doc1.CSI_Code = Constants.DocumentCodes.CertificateOfOrigin;

			invLn1Doc1.CSI_ReferenceNumber = "DOC001";
			var message = "Another Certificate of Origin with a different Number or Type has already been specified for the selected Entry Instruction.\nPlease select a different Entry Instruction for this Invoice Line if it is a different document.";
			invoiceLine2.Validation.ValidateCertificateOfOrigin();
			AssertNoMessageErrorContaining(invoiceLine2.CertificateOfOriginInfo, message);

			invLn2Doc1.CSI_ReferenceNumber = "DOC002";
			invoiceLine2.Validation.ValidateCertificateOfOrigin();
			AssertHasMessageErrorContaining(invoiceLine2.CertificateOfOriginInfo, message);
		}

		public void TestCheckCertificateOfOriginFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CountryOfOrigin = "AU";
			var certInfo = invoiceLine.CertificateOfOriginInfo;

			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			invoiceLine.Validation.ValidateCertificateOfOrigin();
			AssertNoMessageErrors(certInfo);

			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			invoiceLine.Validation.ValidateCertificateOfOrigin();
			AssertHasMessageError(certInfo, "Certificate of Origin is required in order to use the selected preferential duty rate.");

			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.LeastDevelopedCountries;
			invoiceLine.Validation.ValidateCertificateOfOrigin();
			AssertHasMessageError(certInfo, "Certificate of Origin is required in order to use the selected preferential duty rate.");

			invoiceLine.CertificateOfOrigin = "<12>3456798";
			AssertHasMessageError(certInfo, "Certificate of Origin does not need to start with the Preferential Code, please select a Preferential Code on this Invoice Line.");

			invoiceLine.CertificateOfOrigin = "3456798";
			AssertNoMessageErrors(certInfo);

			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.Normal;
			invoiceLine.CertificateOfOrigin = "3456798";
			AssertNoMessageErrors(certInfo);

			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.MostFavouredNations;
			invoiceLine.Validation.ValidateCertificateOfOrigin();
			AssertNoMessageErrors(certInfo);

			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			var countryInfo = invoiceLine.CertificateOfOriginCountryInfo;
			invoiceLine.Validation.ValidateCertificateOfOriginCountry();
			AssertHasMessageErrorContaining("CertificateOfOriginCountryInfo", countryInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_CountryOfOrigin = "XX";
			invoiceLine.Validation.ValidateCertificateOfOriginCountry();
			AssertHasMessageErrorContaining(countryInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.Validation.ValidateCertificateOfOriginCountry();
			AssertNoMessageErrorContaining(countryInfo, ListValidation.InvalidCodeMessageError);

			var typeInfo = invoiceLine.CertificateOfOriginTypeInfo;
			invoiceLine.Validation.ValidateCertificateOfOriginType();
			AssertHasMessageErrorContaining("CertificateOfOriginTypeInfo", typeInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.CertificateOfOriginType = "Z";
			AssertHasMessageErrorContaining(typeInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.CertificateOfOriginType = "D";
			AssertNoMessageErrorContaining(typeInfo, ListValidation.InvalidCodeMessageError);

			var numInfo = invoiceLine.ItemNoOnCertOfOriginInfo;
			invoiceLine.Validation.ValidateItemNoOnCertOfOrigin();
			AssertHasMessageErrorContaining("ItemNoOnCertOfOriginInfo", numInfo, MandatoryValidation.ValueCannotBeZero);

			invoiceLine.CertificateOfOrigin = "";
			invoiceLine.ItemNoOnCertOfOrigin = ZShort.Zero;
			invoiceLine.CertificateOfOriginType = "X";
			invoiceLine.Validation.ValidateCertificateOfOrigin();
			invoiceLine.Validation.ValidateItemNoOnCertOfOrigin();
			AssertNoMessageErrors(numInfo);
			AssertNoMessageErrors(certInfo);
		}

		public void TestCheckingCOOFieldsForExp()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.Validation.ValidateCertificateOfOriginType();
			AssertNoMessageErrors("No checking on CertificateOfOriginType for EXP", InvoiceLine.CertificateOfOriginTypeInfo);
			InvoiceLine.Validation.ValidateCertificateOfOrigin();
			AssertNoMessageErrors("No checking on CertificateOfOrigin for EXP", InvoiceLine.CertificateOfOriginInfo);
			InvoiceLine.Validation.ValidateCertificateOfOriginCountry();
			AssertNoMessageErrors("No checking on CertificateOfOriginCountry for EXP", InvoiceLine.CertificateOfOriginCountryInfo);
			InvoiceLine.Validation.ValidateItemNoOnCertOfOrigin();
			AssertNoMessageErrors("No checking on ItemNoOnCertOfOrigin for EXP", InvoiceLine.ItemNoOnCertOfOriginInfo);

			invoiceLine.CertificateOfOrigin = "<12>3456798";
			AssertHasMessageError(InvoiceLine.CertificateOfOriginInfo, "Certificate of Origin does not need to start with the Preferential Code, please select a Preferential Code on this Invoice Line.");

			InvoiceLine.Validation.ValidateCertificateOfOriginType();
			AssertHasMessageErrorContaining("Mandatory check should be done on CertificateOfOriginTypeInfo when CertificateOfOrigin entered.", InvoiceLine.CertificateOfOriginTypeInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.Validation.ValidateCertificateOfOriginCountry();
			AssertHasMessageErrorContaining("Mandatory check should be done on CertificateOfOriginCountryInfo when CertificateOfOrigin entered.", InvoiceLine.CertificateOfOriginCountryInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.Validation.ValidateItemNoOnCertOfOrigin();
			AssertHasMessageErrorContaining("Mandatory check should be done on ItemNoOnCertOfOriginInfo when CertificateOfOrigin entered.", InvoiceLine.ItemNoOnCertOfOriginInfo, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckTradeUnitPrice()
		{
			InvoiceLine.Validation.ValidateTradeUnitPrice();
			AssertHasMessageErrorContaining(InvoiceLine.TradeUnitPriceInfo, MandatoryValidation.ValueCannotBeZero);
			InvoiceLine.TradeUnitPrice = -1m;
			AssertHasMessageErrorContaining(InvoiceLine.TradeUnitPriceInfo, MandatoryValidation.ValueCannotBeNegative);
			InvoiceLine.TradeUnitPrice = 1m;
			AssertNoMessageErrors(InvoiceLine.TradeUnitPriceInfo);
		}

		public void TestCheckCIQIngredient()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("6202131000", "720c28d9e797c0d8408d34da1b0ab981");
				helper.CreateAdditionalElement("720c28d9e797c0d8408d34da1b0ab981", "面料成分含量");
				Factory.Save();
			});
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "6202131000";
			invoiceLine.XC_GoodsSpecModel = "50";
			invoiceLine.EntryInstruction.CEI_CIQRequires = true;
			invoiceLine.CIQIngredient = "100";
			AssertHasWarningContaining(invoiceLine.CIQIngredientInfo, "The value is different to Additional Information Value entered");

			invoiceLine.CIQIngredient = "50";
			AssertNoWarningContaining(invoiceLine.CIQIngredientInfo, "The value is different to Additional Information Value entered");

			invoiceLine.EntryInstruction.CEI_CIQRequires = false;
			invoiceLine.CIQIngredient = "100";
			AssertNoWarningContaining(invoiceLine.CIQIngredientInfo, "The value is different to Additional Information Value entered");
		}

		public void TestCheckManufactureDatesAsString()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("2203000000", "99997", "ef374a392b7934885636cee2f907884a");
				helper.CreateAdditionalElement("99997", "包装规格");
				helper.CreateAdditionalElement("ef374a392b7934885636cee2f907884a", "生产日期");
				Factory.Save();
			});
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "2203000000";
			invoiceLine.XC_GoodsSpecModel = "500毫升*12瓶/箱|20220211;20220213";
			invoiceLine.EntryInstruction.CEI_CIQRequires = true;

			var batch1 = invoiceLine.ProductionBatch.AddNew();
			var batch2 = invoiceLine.ProductionBatch.AddNew();
			batch1.CY_Date = new ZDateTime(2022, 02, 11);
			batch2.CY_Date = new ZDateTime(2022, 02, 12);
			AssertHasWarningContaining("Set CY_Date to a date not included in XC_GoodsSpecModel", invoiceLine.ManufactureDatesAsStringInfo, "The value is different to Additional Information Value entered");

			batch2.CY_Date = new ZDateTime(2022, 02, 13);
			AssertNoWarningContaining("Set CY_Date to match XC_GoodsSpecModel", invoiceLine.ManufactureDatesAsStringInfo, "The value is different to Additional Information Value entered");

			batch1.CY_Date = new ZDateTime(2022, 02, 12);
			AssertHasWarningContaining("Set CY_Date to a date not included in XC_GoodsSpecModel", invoiceLine.ManufactureDatesAsStringInfo, "The value is different to Additional Information Value entered");

			invoiceLine.XC_GoodsSpecModel = "500毫升*12瓶/箱|20220212;20220213";
			AssertNoWarningContaining("Set XC_GoodsSpecModel to match ManufactureDates", invoiceLine.ManufactureDatesAsStringInfo, "The value is different to Additional Information Value entered");

			batch2.Delete();
			AssertHasWarningContaining("ProductionBatchCollection OnCountChanged", invoiceLine.ManufactureDatesAsStringInfo, "The value is different to Additional Information Value entered");
		}

		public void TestJI_NDescription()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("6202131000", "99998");
				helper.CreateAdditionalElement("99998", "规格型号");
				Factory.Save();
			});
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "6202131000";
			invoiceLine.XC_GoodsSpecModel = "规格：333、型号：444";
			invoiceLine.EntryInstruction.CEI_CIQRequires = true;
			invoiceLine.JI_NDescription = "111";
			AssertHasWarningContaining(invoiceLine.JI_NDescriptionInfo, "The value is different to Additional Information Value entered");

			invoiceLine.JI_NDescription = "333";
			AssertNoWarningContaining(invoiceLine.JI_NDescriptionInfo, "The value is different to Additional Information Value entered");

			invoiceLine.EntryInstruction.CEI_CIQRequires = false;
			invoiceLine.JI_NDescription = "111";
			AssertNoWarningContaining(invoiceLine.JI_NDescriptionInfo, "The value is different to Additional Information Value entered");
		}

		public void TestCheckJI_BrandName()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("6202131000", "baeb884cae1c682fd9c48a9bbc20b849");
				helper.CreateAdditionalElement("baeb884cae1c682fd9c48a9bbc20b849", "品牌(厂商)");
				Factory.Save();
			});
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "6202131000";
			invoiceLine.XC_GoodsSpecModel = "B";
			invoiceLine.EntryInstruction.CEI_CIQRequires = true;
			invoiceLine.JI_BrandName = "A";
			AssertHasWarningContaining(invoiceLine.JI_BrandNameInfo, "The value is different to Additional Information Value entered");

			invoiceLine.JI_BrandName = "B";
			AssertNoWarningContaining(invoiceLine.JI_BrandNameInfo, "The value is different to Additional Information Value entered");

			invoiceLine.EntryInstruction.CEI_CIQRequires = false;
			invoiceLine.JI_BrandName = "A";
			AssertNoWarningContaining(invoiceLine.JI_BrandNameInfo, "The value is different to Additional Information Value entered");
		}

		public void TestCheckJI_Model()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("6202131000", "99998");
				helper.CreateAdditionalElement("99998", "规格型号");
				Factory.Save();
			});
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "6202131000";
			invoiceLine.XC_GoodsSpecModel = "规格：333、型号：444";
			invoiceLine.EntryInstruction.CEI_CIQRequires = true;
			invoiceLine.JI_Model = "333";
			AssertHasWarningContaining(invoiceLine.JI_ModelInfo, "The value is different to Additional Information Value entered");

			invoiceLine.JI_Model = "444";
			AssertNoWarningContaining(invoiceLine.JI_ModelInfo, "The value is different to Additional Information Value entered");

			invoiceLine.EntryInstruction.CEI_CIQRequires = false;
			invoiceLine.JI_Model = "333";
			AssertNoWarningContaining(invoiceLine.JI_ModelInfo, "The value is different to Additional Information Value entered");
		}

		public void TestValidateGoodsSpecModelAnd2()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00000", "00423", "00352");
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("00423", "针入度");
			helper.CreateAdditionalElement("00352", "加工方法");
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_CEI = instruction1.PK;
			invoiceLine.JI_Tariff = "2713200000";
			AssertHasMessageErrorContaining(invoiceLine.XC_GoodsSpecModelInfo, "enter");
			AssertNoMessageErrorContaining(invoiceLine.XC_GoodsSpecModel2Info, "enter");

			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_CEI_Parent = instruction1.PK;
			invoiceLine.Validation.ValidateAll();
			AssertHasMessageErrorContaining(invoiceLine.XC_GoodsSpecModelInfo, "enter");
			AssertHasMessageErrorContaining(invoiceLine.XC_GoodsSpecModel2Info, "enter");

			invoiceLine.XC_GoodsSpecModel = "XXXXX||无其他";
			invoiceLine.XC_GoodsSpecModel2 = "|YYYYY";
			AssertHasMessageErrorContaining(invoiceLine.XC_GoodsSpecModelInfo, "You have not entered some Additional Information");
			AssertHasMessageErrorContaining(invoiceLine.XC_GoodsSpecModel2Info, "You have not entered some Additional Information");

			invoiceLine.XC_GoodsSpecModel = "XXXXX|YYYYY|无其他";
			invoiceLine.XC_GoodsSpecModel2 = "XXXXX|YYYYY";
			AssertNoMessageErrorContaining(invoiceLine.XC_GoodsSpecModelInfo, "You have not entered some Additional Information");
			AssertNoMessageErrorContaining(invoiceLine.XC_GoodsSpecModel2Info, "You have not entered some Additional Information");
		}

		public void TestValidationModeProvider()
		{
			ValidationExtensionsTest.AssertValidationModeProvider(Declaration, InvoiceLine.Validation.ValidationModeProvider);
		}

		#region AddInfo Properties

		public void TestCheckJI_NonDangerousChemicalFlag()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNDangerousChemical, "China Dangerous Chemical");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeType.ZZK_CodeType, "7664-41-7", "氨", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_NameOfGoods = "氨";
			var targetInfo = invoiceLine.JI_NonDangerousChemicalFlagInfo;
			invoiceLine.JI_NonDangerousChemicalFlag = true;
			AssertHasWarningContaining(targetInfo, "Non Dangerous Chemical should not be ticked for the goods seem to be dangerous chemical.");
			invoiceLine.JI_NonDangerousChemicalFlag = false;
			AssertNoWarnings(targetInfo);
			(invoiceLine.CargoAttributes as ICodeDescriptionOptionStorage).AddNew(CargoAttributeList.Codes._33);
			invoiceLine.JI_NonDangerousChemicalFlag = false;
			invoiceLine.Validation.ValidateJI_NonDangerousChemicalFlag();
			AssertHasMessageError("JI_NonDangerousChemicalFlag", targetInfo, "Non Dangerous Goods should be ticked due to Cargo Attribute '非危险化学品' selected.");
			invoiceLine.JI_NonDangerousChemicalFlag = true;
			AssertNoMessageError("JI_NonDangerousChemicalFlag", targetInfo, "Non Dangerous Goods should be ticked due to Cargo Attribute '非危险化学品' selected.");
		}

		[TestDate(2018, 12, 13)]
		public void TestCheckJI_CIQOriginState()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNCIQStates, "China Inspection and Quarantine States");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeType.ZZK_CodeType, "000", "000", new ZDateTime(2018, 12, 01), new ZDateTime(2018, 12, 31));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeType.ZZK_CodeType, "840001", "840001", new ZDateTime(2018, 12, 01), new ZDateTime(2018, 12, 31));
			Factory.Save();
			var refCountry = Factory.NewWithValidTestData<RefCountry>();
			refCountry.RN_Code = "US";
			refCountry.RN_IsoNumericUNM49Code = "840";
			var states1 = Factory.NewWithValidTestData<RefCountryStates>();
			states1.RW_Code = "AB";
			states1.RW_RN_NKCountryCode = refCountry.RN_Code;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_CIQOriginState = "999";
			AssertHasMessageErrorContaining(invoiceLine.JI_CIQOriginStateInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(invoiceLine.JI_CIQOriginStateInfo, JobComInvoiceLineValidation.StateShouldBelongToGoodsOrigin);
			invoiceLine.JI_CIQOriginState = "000";
			AssertNoMessageErrorContaining(invoiceLine.JI_CIQOriginStateInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(invoiceLine.JI_CIQOriginStateInfo, JobComInvoiceLineValidation.StateShouldBelongToGoodsOrigin);
			invoiceLine.JI_CIQOriginState = "840001";
			AssertNoMessageErrorContaining(invoiceLine.JI_CIQOriginStateInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.JI_CIQOriginStateInfo, JobComInvoiceLineValidation.StateShouldBelongToGoodsOrigin);
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckJI_DestinationDistrict()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DistrictCode, "District Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeType.ZZK_CodeType, "12345", "New District Code", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeType.ZZK_CodeType, "12354", "New District Code", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var instruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			var invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			instruction.CEI_JE = declaration.PK;
			invoiceHeader.JZ_JE = declaration.PK;
			invoiceLine.JI_CEI = instruction.PK;
			var targetInfo = invoiceLine.JI_DestinationDistrictInfo;
			instruction.CEI_LevyType = "307";
			invoiceLine.JI_DestinationDistrict = "11111";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(targetInfo, "When Levy Type is 307, the fifth digit of Destination District should be 4.");
			invoiceLine.JI_DestinationDistrict = "12345";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_DestinationDistrict = "12354";
			AssertNoMessageErrors(targetInfo);
			instruction.CEI_LevyType = "399";
			invoiceLine.Validation.ValidateJI_DestinationDistrict();
			AssertHasMessageError(targetInfo, "When Levy Type is 399, the fifth digit of Destination District should be 5,6, or 6509A, 3723W.");
			invoiceLine.JI_DestinationDistrict = "12345";
			AssertNoMessageErrors(targetInfo);
			invoiceLine.JI_DestinationDistrict = "11116";
			AssertNoMessageError(targetInfo, "When Levy Type is 399, the fifth digit of Destination District should be 5,6, or 6509A, 3723W.");
			invoiceLine.JI_DestinationDistrict = "6509A";
			AssertNoMessageError(targetInfo, "When Levy Type is 399, the fifth digit of Destination District should be 5,6, or 6509A, 3723W.");
			invoiceLine.JI_DestinationDistrict = "3723W";
			AssertNoMessageError(targetInfo, "When Levy Type is 399, the fifth digit of Destination District should be 5,6, or 6509A, 3723W.");
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckJI_OriginDistrict()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DistrictCode, "District Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeType.ZZK_CodeType, "12345", "New District Code", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeType.ZZK_CodeType, "12354", "New District Code", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var instruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			var invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			instruction.CEI_JE = declaration.PK;
			invoiceHeader.JZ_JE = declaration.PK;
			invoiceLine.JI_CEI = instruction.PK;
			var targetInfo = invoiceLine.JI_OriginDistrictInfo;
			instruction.CEI_LevyType = "307";
			invoiceLine.JI_OriginDistrict = "11111";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(targetInfo, "When Levy Type is 307, the fifth digit of Origin District should be 4.");
			invoiceLine.JI_OriginDistrict = "12345";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_OriginDistrict = "12354";
			AssertNoMessageErrors(targetInfo);
			instruction.CEI_LevyType = "399";
			invoiceLine.Validation.ValidateJI_OriginDistrict();
			AssertHasMessageError(targetInfo, "When Levy Type is 399, the fifth digit of Origin District should be 5,6, or 6509A, 3723W.");
			invoiceLine.JI_OriginDistrict = "11115";
			AssertNoMessageError(targetInfo, "When Levy Type is 399, the fifth digit of Origin District should be 5,6, or 6509A, 3723W.");
			invoiceLine.JI_OriginDistrict = "11116";
			AssertNoMessageError(targetInfo, "When Levy Type is 399, the fifth digit of Origin District should be 5,6, or 6509A, 3723W.");
			invoiceLine.JI_OriginDistrict = "6509A";
			AssertNoMessageError(targetInfo, "When Levy Type is 399, the fifth digit of Origin District should be 5,6, or 6509A, 3723W.");
			invoiceLine.JI_OriginDistrict = "3723W";
			AssertNoMessageError(targetInfo, "When Levy Type is 399, the fifth digit of Origin District should be 5,6, or 6509A, 3723W.");
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckJI_OriginRegion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNCIQDistricts, "Region Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeType.ZZK_CodeType, "12345", "New Region Code", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var instruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			var invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_OriginRegion = "XXXXX";
			AssertHasMessageErrorContaining(invoiceLine.JI_OriginRegionInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_OriginRegion = "12345";
			AssertNoMessageErrorContaining(invoiceLine.JI_OriginRegionInfo, ListValidation.InvalidCodeMessageError);
			instruction.CEI_CIQRequires = true;
			invoiceLine.JI_OriginRegion = ZString.Empty;
			invoiceLine.Validation.ValidateJI_OriginRegion();
			AssertHasMessageErrorContaining(invoiceLine.JI_OriginRegionInfo, MandatoryValidation.YouHaveNotEntered);
			instruction.CEI_CIQRequires = false;
			invoiceLine.Validation.ValidateJI_OriginRegion();
			AssertNoMessageErrorContaining(invoiceLine.JI_OriginRegionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckJI_DestinationRegion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNCIQDistricts, "Region Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeType.ZZK_CodeType, "12345", "New Region Code", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Factory.NewWithValidTestData<CusEntryInstruction>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.Validation.ValidateJI_DestinationRegion();
			AssertHasMessageErrorContaining(invoiceLine.JI_DestinationRegionInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_DestinationRegion = "XXXXX";
			AssertNoMessageErrorContaining(invoiceLine.JI_DestinationRegionInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoiceLine.JI_DestinationRegionInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_DestinationRegion = "12345";
			AssertNoMessageErrorContaining(invoiceLine.JI_DestinationRegionInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckXJI_DutyMode()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var testEntryInstruction = testItems.EntryInstruction;
			var testInvoice = testItems.InvoiceLine;
			testEntryInstruction.CEI_LevyType = "101";
			testInvoice.Validation.ValidateJI_DutyMode();
			AssertHasMessageErrorContaining(testInvoice.JI_DutyModeInfo, MandatoryValidation.YouHaveNotEntered);
			testInvoice.JI_DutyMode = "1";
			AssertNoMessageErrors(testInvoice.JI_DutyModeInfo);
			testInvoice.JI_DutyMode = "5";
			AssertNoMessageErrorContaining(testInvoice.JI_DutyModeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(testInvoice.JI_DutyModeInfo, "The selected Duty Mode is not suitable for Levy Type");
			testInvoice.JI_DutyMode = "6";
			AssertNoMessageErrorContaining(testInvoice.JI_DutyModeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(testInvoice.JI_DutyModeInfo, "The selected Duty Mode is not suitable for Levy Type");
			testEntryInstruction.CEI_LevyType = "102";
			testInvoice.JI_DutyMode = "5";
			AssertNoMessageErrorContaining(testInvoice.JI_DutyModeInfo, "The selected Duty Mode is not suitable for Levy Type");
			testInvoice.JI_DutyMode = "X";
			AssertHasMessageErrorContaining(testInvoice.JI_DutyModeInfo, ListValidation.InvalidCodeMessageError);
			testInvoice.Declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			testInvoice.Declaration.JE_TaxInvolved = false;
			testInvoice.JI_DutyMode = "1";
			AssertHasMessageErrorContaining(testInvoice.JI_DutyModeInfo, $"Duty Mode should be {DutyModeList.Codes._3} - {DutyModeList.Descriptions._3} when the declaration is not tax involved.");
			testInvoice.Declaration.JE_TaxInvolved = true;
			testInvoice.JI_DutyMode = "1";
			AssertNoMessageErrorContaining(testInvoice.JI_DutyModeInfo, $"Duty Mode should be {DutyModeList.Codes._3} - {DutyModeList.Descriptions._3} when the declaration is not tax involved.");
			testInvoice.Declaration.JE_TaxInvolved = false;
			testInvoice.JI_DutyMode = "3";
			AssertNoMessageErrorContaining(testInvoice.JI_DutyModeInfo, $"Duty Mode should be {DutyModeList.Codes._3} - {DutyModeList.Descriptions._3} when the declaration is not tax involved.");
		}

		public void TestCheckJI_ProductManualNo()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory);
			var instruction = testItems.EntryInstruction;
			var invoiceLine = testItems.InvoiceLine;
			var targetInfo = invoiceLine.JI_ProductManualNoInfo;
			invoiceLine.JI_ProductManualNo = -10;
			AssertHasMessageError(targetInfo, MandatoryValidation.ValueCannotBeNegativeMessage(GetHumanReadableName(targetInfo)));
			invoiceLine.JI_ProductManualNo = 10;
			AssertNoMessageError(targetInfo, MandatoryValidation.ValueCannotBeNegativeMessage(GetHumanReadableName(targetInfo)));
			instruction.CEI_ManualNo = "MN991";
			invoiceLine.JI_ProductManualNo = 10;
			AssertNoMessageError(targetInfo, ManualItemNoShouldBeZeroMessage);
			AssertNoMessageError(targetInfo, ManualItemNoCannotBeZeroMessage);
			invoiceLine.JI_ProductManualNo = 0;
			AssertNoMessageError(targetInfo, ManualItemNoShouldBeZeroMessage);
			AssertHasMessageError(targetInfo, ManualItemNoCannotBeZeroMessage);
			instruction.CEI_ManualNo = "";
			invoiceLine.JI_ProductManualNo = 10;
			AssertHasMessageError(targetInfo, ManualItemNoShouldBeZeroMessage);
			AssertNoMessageError(targetInfo, ManualItemNoCannotBeZeroMessage);
			instruction.CEI_ManualNo = "";
			invoiceLine.JI_ProductManualNo = 0;
			AssertNoMessageError(targetInfo, ManualItemNoShouldBeZeroMessage);
			AssertNoMessageError(targetInfo, ManualItemNoCannotBeZeroMessage);
		}

		public void TestCheckXC_ProductManual2No()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory);
			var invoiceLine = testItems.InvoiceLine;
			var targetInfo = invoiceLine.JI_ProductManualNo2Info;
			invoiceLine.Validation.ValidateJI_ProductManualNo2();
			AssertNoMessageError(targetInfo, ManualItemNoShouldBeZeroMessage);
			AssertNoMessageError(targetInfo, ManualItemNoCannotBeZeroMessage);
			invoiceLine.JI_ProductManualNo2 = 10;
			AssertHasMessageError(targetInfo, ManualItemNoShouldBeZeroMessage);
			AssertNoMessageError(targetInfo, ManualItemNoCannotBeZeroMessage);
			var childInstruction = testItems.JobDeclaration.CustomsEntryInstructions.AddNew();
			childInstruction.CEI_CEI_Parent = testItems.EntryInstruction.PK;
			invoiceLine.JI_ProductManualNo2 = -10;
			AssertHasMessageError(targetInfo, MandatoryValidation.ValueCannotBeNegativeMessage(GetHumanReadableName(targetInfo)));
			invoiceLine.JI_ProductManualNo2 = 10;
			AssertNoMessageError(targetInfo, MandatoryValidation.ValueCannotBeNegativeMessage(GetHumanReadableName(targetInfo)));
			childInstruction.CEI_ManualNo = "MN991";
			invoiceLine.JI_ProductManualNo2 = 10;
			AssertNoMessageError(targetInfo, ManualItemNoShouldBeZeroMessage);
			AssertNoMessageError(targetInfo, ManualItemNoCannotBeZeroMessage);
			invoiceLine.JI_ProductManualNo2 = 0;
			AssertNoMessageError(targetInfo, ManualItemNoShouldBeZeroMessage);
			AssertHasMessageError(targetInfo, ManualItemNoCannotBeZeroMessage);
			childInstruction.CEI_ManualNo = "";
			invoiceLine.JI_ProductManualNo2 = 10;
			AssertHasMessageError(targetInfo, ManualItemNoShouldBeZeroMessage);
			AssertNoMessageError(targetInfo, ManualItemNoCannotBeZeroMessage);
			childInstruction.CEI_ManualNo = "";
			invoiceLine.JI_ProductManualNo2 = 0;
			AssertNoMessageError(targetInfo, ManualItemNoShouldBeZeroMessage);
			AssertNoMessageError(targetInfo, ManualItemNoCannotBeZeroMessage);
		}

		ZString ManualItemNoShouldBeZeroMessage => "Manual Item No should be zero when the Manual Number is not entered.";

		ZString ManualItemNoCannotBeZeroMessage => "Manual Item No cannot be zero when the Manual Number is entered.";

		[TestDate(2016, 5, 5)]
		public void TestCheckJI_CIQTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var ciqTariffType = helper.CreateNewOrGetExistingTariffType("CN", "CIQ");
			var cusTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			Factory.Save();
			var ciqTariff1 = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "10000012001", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			var ciqTariff2 = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "10000012002", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			var ciqTariff3 = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "10000013001", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.LoadOrCreateNewTariff("CN", cusTariffType.PK, "10000012", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.LoadOrCreateNewTariff("CN", cusTariffType.PK, "10000013", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateTariffRelationship(ciqTariff1.PK, cusTariffType.PK, "10000012");
			helper.CreateTariffRelationship(ciqTariff2.PK, cusTariffType.PK, "10000012");
			helper.CreateTariffRelationship(ciqTariff3.PK, cusTariffType.PK, "10000013");
			var ciqTariff = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "8701300090999", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			var cusTariff = helper.LoadOrCreateNewTariff("CN", cusTariffType.PK, "8701300090", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			var ciqRelation = helper.CreateTariffRelationship(ciqTariff.PK, cusTariffType.PK, "8701300090");
			ciqTariff = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "4401110000999", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			cusTariff = helper.LoadOrCreateNewTariff("CN", cusTariffType.PK, "4401110000", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			ciqRelation = helper.CreateTariffRelationship(ciqTariff.PK, cusTariffType.PK, "4401110000");
			ciqTariff = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "3208100010301", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			cusTariff = helper.LoadOrCreateNewTariff("CN", cusTariffType.PK, "3208100010", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			ciqRelation = helper.CreateTariffRelationship(ciqTariff.PK, cusTariffType.PK, "3208100010");
			ciqTariff = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "3303000010101", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			cusTariff = helper.LoadOrCreateNewTariff("CN", cusTariffType.PK, "3303000010", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			ciqRelation = helper.CreateTariffRelationship(ciqTariff.PK, cusTariffType.PK, "3303000010");
			Factory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var instruction = testItems.EntryInstruction;
			var testItem = testItems.InvoiceLine;
			testItem.JI_Tariff = "10000012";
			var targetInfo = testItem.JI_CIQTariffInfo;
			testItem.Validation.ValidateJI_CIQTariff();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			instruction.CEI_CIQRequires = true;
			testItem.Validation.ValidateJI_CIQTariff();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			testItem.JI_CIQTariff = "1111";
			AssertHasMessageErrorContaining(targetInfo, "not in the list");
			AssertNoMessageErrorContaining(targetInfo, "is not valid for the Customs Tariff Code");
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			testItem.JI_CIQTariff = "10000012001";
			AssertNoMessageErrors(targetInfo);
			testItem.JI_CIQTariff = "10000013001";
			AssertHasMessageErrorContaining(targetInfo, "is not valid for the Customs Tariff Code");
			AssertNoMessageErrorContaining(targetInfo, "not in the list");
			testItem.JI_Tariff = "8701300090";
			testItem.JI_CIQTariff = "8701300090999";
			AssertNoMessageError(targetInfo, "Product Qualification 408/409/603 is required for the selected tariff.");
			testItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testItem.Validation.ValidateJI_CIQTariff();
			AssertHasMessageError(targetInfo, "Product Qualification 408/409/603 is required for the selected tariff.");
			var newCIQ = testItem.CIQProductQualifications.AddNew();
			testItem.Validation.ValidateJI_CIQTariff();
			AssertHasMessageError(targetInfo, "Product Qualification 408/409/603 is required for the selected tariff.");
			newCIQ.CSI_Code = "408";
			newCIQ.CSI_ReferenceNumber = "NUM1";
			testItem.Validation.ValidateJI_CIQTariff();
			AssertNoMessageErrorContaining(targetInfo, "Product Qualification 408/409/603 is required for the selected tariff.");
			AssertNoWarnings(targetInfo);
			instruction.CEI_CIQRequires = false;
			testItem.JI_CIQTariff = "10000012001";
			AssertHasWarningContaining(targetInfo, "If you want to submit CIQ data when sending to Customs, please tick 'CIQ Requires' on the Entry Instruction.");
			instruction.CEI_CIQRequires = true;
			testItem.JI_Tariff = "4401110000";
			testItem.JI_CIQTariff = "4401110000999";
			AssertHasMessageError(targetInfo, "Cargo Attribute 23/24 is required.");
			var cargoAttribute = Factory.New<CargoAttribute>();
			cargoAttribute.CY_ParentID = testItem.PK;
			cargoAttribute.CY_ParentTableCode = testItem.TablePrefix;
			cargoAttribute.CY_Type = Constants.CusCodeDataTypes.Codes.CargoAttribute;
			cargoAttribute.CY_Code = CargoAttributeList.Codes._23;
			testItem.CargoAttributes.Add(cargoAttribute);
			testItem.Validation.ValidateJI_CIQTariff();
			AssertNoMessageError(targetInfo, "Cargo Attribute 23/24 is required.");
			testItem.JI_Tariff = "3208100010";
			testItem.JI_CIQTariff = "3208100010301";
			AssertHasMessageError(targetInfo, "Product Qualification 412 is required for the selected tariff.");
			newCIQ.CSI_Code = "412";
			testItem.Validation.ValidateJI_CIQTariff();
			AssertNoMessageErrorContaining(targetInfo, "Product Qualification 412 is required for the selected tariff.");
			testItem.JI_Tariff = "3303000010";
			testItem.JI_CIQTariff = "3303000010101";
			AssertHasMessageError(targetInfo, "Product Qualification 516/523 is required for the selected tariff.");
			newCIQ.CSI_Code = "516";
			testItem.Validation.ValidateJI_CIQTariff();
			AssertNoMessageErrorContaining(targetInfo, "Product Qualification 516/523 is required for the selected tariff.");
		}

		public void TestJI_CIQQualityGuaranteePeriod()
		{
			var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			var targetInfo = invoiceLine.JI_CIQQualityGuaranteePeriodInfo;
			invoiceLine.JI_CIQQualityGuaranteePeriod = -10;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			invoiceLine.JI_CIQQualityGuaranteePeriod = 10;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestJI_CIQExpiryDate()
		{
			var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			invoiceLine.ProductionBatch.AddNew();
			var targetInfo = invoiceLine.JI_CIQExpiryDateInfo;
			const string dateMessege = "The Expiry Date cannot be a past date";
			const string dateMessege1 = "The Expiry Date cannot before any Manufacture Dates";
			invoiceLine.JI_CIQExpiryDate = ZDateTime.Today.AddDays(-1);
			AssertHasNotifications(invoiceLine.JI_CIQExpiryDateInfo);
			AssertHasWarningContaining(targetInfo, dateMessege);
			AssertNoWarningContaining(targetInfo, dateMessege1);
			invoiceLine.JI_CIQExpiryDate = ZDateTime.Today;
			AssertNoWarningContaining(targetInfo, dateMessege);
			AssertNoWarningContaining(targetInfo, dateMessege1);
			invoiceLine.JI_CIQExpiryDate = ZDateTime.Today.AddDays(1);
			AssertNoWarningContaining(targetInfo, dateMessege);
			AssertNoWarningContaining(targetInfo, dateMessege1);
			var cusCodeData1 = invoiceLine.ProductionBatch.AddNew();
			cusCodeData1.CY_Date = ZDateTime.Today.AddDays(1);
			invoiceLine.JI_CIQExpiryDate = ZDateTime.Today;
			AssertNoWarningContaining(targetInfo, dateMessege);
			AssertHasWarningContaining(targetInfo, dateMessege1);
			invoiceLine.JI_CIQExpiryDate = ZDateTime.Today.AddDays(1);
			AssertNoWarningContaining(targetInfo, dateMessege);
			AssertNoWarningContaining(targetInfo, dateMessege1);
			AssertNoNotifications(targetInfo);
			invoiceLine.JI_CIQExpiryDate = ZDateTime.Today.AddDays(2);
			AssertNoWarningContaining(targetInfo, dateMessege);
			AssertNoWarningContaining(targetInfo, dateMessege1);
			AssertNoNotifications(targetInfo);
		}

		[TestDate(2016, 5, 5)]
		public void TestJI_CIQEndUse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			var targetInfo = invoiceLine1.JI_CIQEndUseInfo;
			invoiceLine1.JI_CIQEndUse = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine2.JI_CIQTariff = "1111";
			instruction.CEI_CIQRequires = true;
			invoiceLine1.Validation.ValidateJI_CIQEndUse();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine1.JI_CIQEndUse = "~";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine1.JI_CIQEndUse = "99";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			for (var endUse = 11; endUse <= 33; endUse++)
			{
				invoiceLine1.JI_CIQEndUse = endUse.ToString();
				AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			}

			invoiceLine1.JI_CIQEndUse = EndUseList.Codes.Cosmetics;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var qualification = invoiceLine1.CIQProductQualifications.AddNew();
			qualification.CSI_Code = ProductQualificationCodeList.Codes._422;
			qualification.CSI_ReferenceNumber = "NUM1";
			invoiceLine1.Validation.ValidateJI_CIQEndUse();
			AssertHasMessageErrorContaining(targetInfo, "Product Qualification 516,523 is required for the selected End Use.");
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			invoiceLine1.Validation.ValidateJI_CIQEndUse();
			AssertNoMessageErrorContaining(targetInfo, "Product Qualification 516,523 is required for the selected End Use.");
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			qualification.CSI_Code = ProductQualificationCodeList.Codes._516;
			invoiceLine1.Validation.ValidateJI_CIQEndUse();
			AssertNoMessageErrorContaining(targetInfo, "Product Qualification 516,523 is required for the selected End Use.");
			qualification.CSI_Code = ProductQualificationCodeList.Codes._523;
			invoiceLine1.Validation.ValidateJI_CIQEndUse();
			AssertNoMessageErrorContaining(targetInfo, "Product Qualification 516,523 is required for the selected End Use.");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var ciqTariffType = helper.CreateNewOrGetExistingTariffType("CN", "CIQ");
			var cusTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			Factory.Save();
			var ciqTariff = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "8701300090999", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.LoadOrCreateNewTariff("CN", cusTariffType.PK, "8701300090", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateTariffRelationship(ciqTariff.PK, cusTariffType.PK, "8701300090");
			invoiceLine1.JI_Tariff = "3303000010";
			invoiceLine1.JI_CIQTariff = "3303000010101";
			invoiceLine1.JI_CIQEndUse = EndUseList.Codes.PlantOrReproduce;
			AssertHasMessageError(targetInfo, "End Use should be 27 for CIQ Tariff ‘3303000010101’.");
			invoiceLine1.JI_CIQEndUse = EndUseList.Codes.Cosmetics;
			invoiceLine1.Validation.ValidateJI_CIQEndUse();
			AssertNoMessageError(targetInfo, "End Use should be 27 for CIQ Tariff ‘3303000010101’.");
		}

		public void TestCheckJI_TradeQuantity()
		{
			var invoiceLine = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { }).InvoiceLine;
			invoiceLine.JI_TradeQuantity = -1m;
			AssertHasMessageErrorContaining(invoiceLine.JI_TradeQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
			invoiceLine.JI_TradeQuantity = 0m;
			AssertHasMessageErrorContaining(invoiceLine.JI_TradeQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			invoiceLine.JI_TradeQuantity = 1m;
			AssertNoMessageErrors(invoiceLine.JI_TradeQuantityInfo);
		}

		public void TestCheckJI_TradeUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("1010101011", "00000", "00423", "00352", "99999");
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Unit Quantity");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "035", "Meters", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "002", "Unit2", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			Factory.Save();
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = CNCusEntryHeaderHelper.CreateNewProduct(Factory, supplier, "NEWPROD10", "PRODUCT10");
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_OH = supplier.PK;
			pivot.CI_TariffNum = "1010101011";
			pivot.CNC_TradeUnitQty = "002";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "BTH";
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			var targetInfo = invoiceLine.JI_TradeUnitQtyInfo;
			invoiceLine.JI_TradeUnitQty = "";
			invoiceLine.Validation.ValidateJI_TradeUnitQty();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_TradeUnitQty = "XX";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_TradeUnitQty = "035";
			AssertNoMessageErrors(targetInfo);
			var differentUnitPrice = "The value entered is different from Trade Unit Price on Product (002), please confirm that this is correct.";
			AssertHasWarning(targetInfo, differentUnitPrice);
			invoiceLine.JI_TradeUnitQty = "002";
			AssertNoWarnings(targetInfo);
			pivot.CNC_TradeUnitQty = "";
			invoiceLine.Validation.ValidateJI_TradeUnitQty();
			AssertNoWarningContaining(targetInfo, differentUnitPrice);
		}

		public void TestCheckJI_OrigContainerFlag()
		{
			var invoiceLine = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory).InvoiceLine;
			var targetInfo = invoiceLine.JI_OrigContainerFlagInfo;
			invoiceLine.Validation.ValidateJI_OrigContainerFlag();
			AssertNoMessageErrors(targetInfo);
			invoiceLine.JI_OrigContainerFlag = "X";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_OrigContainerFlag = "0";
			AssertNoMessageErrors(targetInfo);
			invoiceLine.JI_OrigContainerFlag = "1";
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckJI_PackageTypeOfUNDG_MandatoryCheck()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory);
			var invoiceLine = testItems.InvoiceLine;
			var targetInfo = invoiceLine.JI_PackageTypeOfUNDGInfo;
			var validation = invoiceLine.Validation;
			validation.ValidateJI_PackageTypeOfUNDG();
			AssertNoMessageErrorContaining("No CargoAttribute selected, Non DangerousGoods unselected, should not check JI_PackageTypeOfUNDG", targetInfo, MandatoryValidation.YouHaveNotEntered);
			(invoiceLine.CargoAttributes as ICodeDescriptionOptionStorage).AddNew(CargoAttributeList.Codes._32);
			invoiceLine.JI_NonDangerousChemicalFlag = false;
			validation.ValidateJI_PackageTypeOfUNDG();
			AssertHasMessageErrorContaining("No CargoAttribute selected, Non DangerousGoods unselected, should check JI_PackageTypeOfUNDG", targetInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PackageTypeOfUNDG = "1A1";
			AssertNoMessageErrorContaining("No CargoAttribute selected, Non DangerousGoods unselected, should check JI_PackageTypeOfUNDG", targetInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_NonDangerousChemicalFlag = true;
			invoiceLine.JI_PackageTypeOfUNDG = "";
			AssertNoMessageErrorContaining("No CargoAttribute selected, Non DangerousGoods unselected, should NOT check JI_PackageTypeOfUNDG", targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_PackageTypeOfUNDG_Characters()
		{
			var messageInvalidCharacters = "UN Markings for Packaging can only contain alphanumeric characters and '/', ' ', '-', '.'.";
			var messageMandatoryCharacter = "UN Markings for Packaging must contain '/'.";

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var targetInfo = invoiceLine.JI_PackageTypeOfUNDGInfo;

			CombineAssertions(() =>
			{
				for (var c = 32; c <= 126; c++)
				{
					var value = (char)c;
					invoiceLine.JI_PackageTypeOfUNDG = value.ToString();
					if (char.IsLetterOrDigit(value) || new[] { '/', ' ', '-', '.' }.Contains(value))
					{
						AssertNoMessageError(targetInfo, messageInvalidCharacters);
					}
					else
					{
						AssertHasMessageError(targetInfo, messageInvalidCharacters);
					}
				}

				invoiceLine.JI_PackageTypeOfUNDG = "";
				AssertNoMessageError(targetInfo, messageInvalidCharacters);
				AssertNoMessageError(targetInfo, messageMandatoryCharacter);

				invoiceLine.JI_PackageTypeOfUNDG = "ABC_CBA";
				AssertHasMessageError(targetInfo, messageInvalidCharacters);
				AssertHasMessageError(targetInfo, messageMandatoryCharacter);

				invoiceLine.JI_PackageTypeOfUNDG = "ABC CBA";
				AssertNoMessageError(targetInfo, messageInvalidCharacters);
				AssertHasMessageError(targetInfo, messageMandatoryCharacter);

				invoiceLine.JI_PackageTypeOfUNDG = "ABC-CBA";
				AssertNoMessageError(targetInfo, messageInvalidCharacters);
				AssertHasMessageError(targetInfo, messageMandatoryCharacter);

				invoiceLine.JI_PackageTypeOfUNDG = "ABC.CBA";
				AssertNoMessageError(targetInfo, messageInvalidCharacters);
				AssertHasMessageError(targetInfo, messageMandatoryCharacter);

				invoiceLine.JI_PackageTypeOfUNDG = "ABC/CBA";
				AssertNoMessageError(targetInfo, messageInvalidCharacters);
				AssertNoMessageError(targetInfo, messageMandatoryCharacter);

				invoiceLine.JI_PackageTypeOfUNDG = "1H1/X1.3/250/23CN/C22 10-01";
				AssertNoMessageError(targetInfo, messageInvalidCharacters);
				AssertNoMessageError(targetInfo, messageMandatoryCharacter);
			});
		}

		public void TestValidateNameOfGoods()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00000", "00423", "00352", "99999");
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("00423", "针入度");
			helper.CreateAdditionalElement("00352", "加工方法");
			helper.CreateAdditionalElement("99999", "其他");
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Tariff = "2713200000";
			invoiceLine.JI_CEI = instruction1.PK;
			AssertHasMessageErrorContaining(invoiceLine.JI_NameOfGoodsInfo, "enter");
			AssertNoMessageErrorContaining(invoiceLine.JI_NameOfGoods2Info, "enter");
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_CEI_Parent = instruction1.PK;
			invoiceLine.Validation.ValidateAll();
			AssertHasMessageErrorContaining(invoiceLine.JI_NameOfGoodsInfo, "enter");
			AssertHasMessageErrorContaining(invoiceLine.JI_NameOfGoods2Info, "enter");
			invoiceLine.JI_NameOfGoods = new string('X', 50);
			invoiceLine.JI_NameOfGoods2 = new string('X', 50);
			AssertNoMessageErrorContaining(invoiceLine.JI_NameOfGoodsInfo, "enter");
			AssertNoMessageErrorContaining(invoiceLine.JI_NameOfGoods2Info, "enter");
		}

		#endregion

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
		JobComInvoiceHeader Invoice
		{
			get
			{
				if (invoice == null)
				{
					invoice = Declaration.Invoices.AddNew();
				}

				return invoice;
			}
		}

		JobComInvoiceHeader invoice;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = Invoice.JobComInvoiceLines.AddNew();
				}

				return invoiceLine;
			}
		}

		JobComInvoiceLine invoiceLine;
		ZGuid tariffTypePK;
		TariffView tariff1;
		TariffView tariff2;
		TariffUOMView tariffUOM_CUS1;
		TariffUOMView tariffUOM_CUS2_1;
		UniversalReferenceTestDataHelper helper;
		[TestDate(2016, 5, 5)]
		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem).PK;
			Factory.Save();
			tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "12345678", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "11111111", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			tariffUOM_CUS1 = helper.CreateTariffUOM(tariff1, "CU1", "008");
			tariffUOM_CUS2_1 = helper.CreateTariffUOM(tariff1, "CU2", "003");
			helper.CreateTariffUOM(tariff2, "CU2", "UUU");
			helper.CreateNewOrGetExistingCusCodeType("CURR", "Currency");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CURR", "110", "Australian Dollas", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeType("CNPTA", "CN Prefential Trade Agreement");
			var cnpta_02 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CNPTA", "02", "02", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var cnpta_03 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CNPTA", "03", "03", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(cnpta_02.PK, Constants.UniversalReferenceConstants.CusCodeListAttributeName.ApplicableCountry, Core.Constants.CountryCodes.Australia);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cnpta_03.PK, Constants.UniversalReferenceConstants.CusCodeListAttributeName.ApplicableCountry, Core.Constants.CountryCodes.NewZealand);
			Factory.Save();
		}

		static ZString GetHumanReadableName(ZPropertyInfo targetInfo)
		{
			return targetInfo?.HasHumanReadableName ?? false ? targetInfo.HumanReadableName.ToString() : Res.GetString("03c6543b-78ab-4af1-b352-7001e43828fa", "value");
		}
	}
}
