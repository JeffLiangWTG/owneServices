using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_CustomsSecondQuantity_NumberOfItems()
		{
			AssertCustomsQuantityRequiresInteger(invoiceLine.JI_CustomsSecondUnitQtyInfo, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems, invoiceLine.JI_CustomsSecondQuantityInfo, SupplementaryQtyIntegerMessageError);
		}

		public void TestCheckJI_CustomsSecondQuantity_NumberOfCells()
		{
			AssertCustomsQuantityRequiresInteger(invoiceLine.JI_CustomsSecondUnitQtyInfo, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells, invoiceLine.JI_CustomsSecondQuantityInfo, SupplementaryQtyIntegerMessageError);
		}

		public void TestCheckJI_CustomsSecondQuantity_NumberOfPairs()
		{
			AssertCustomsQuantityRequiresInteger(invoiceLine.JI_CustomsSecondUnitQtyInfo, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs, invoiceLine.JI_CustomsSecondQuantityInfo, SupplementaryQtyIntegerMessageError);
		}

		public void TestCheckJI_CustomsSecondQuantity_DecimalType()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_CustomsSecondUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs;
				invoiceLine.JI_CustomsSecondQuantity = 123456789.123m;
				AssertHasMessageError("Integer Type", invoiceLine.JI_CustomsSecondQuantityInfo, SupplementaryQtyIntegerMessageError);
				invoiceLine.JI_CustomsSecondUnitQty = "ABC";
				invoiceLine.Validation.ValidateJI_CustomsSecondQuantity();
				AssertNoMessageError("Decimal Type", invoiceLine.JI_CustomsSecondQuantityInfo, SupplementaryQtyIntegerMessageError);
			});
		}

		public void TestCheckJI_CustomsThirdQuantity_NumberOfItems()
		{
			AssertCustomsQuantityRequiresInteger(invoiceLine.JI_CustomsThirdUnitQtyInfo, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems, invoiceLine.JI_CustomsThirdQuantityInfo, ThirdQtyIntegerMessageError);
		}

		public void TestCheckJI_CustomsThirdQuantity_NumberOfCells()
		{
			AssertCustomsQuantityRequiresInteger(invoiceLine.JI_CustomsThirdUnitQtyInfo, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells, invoiceLine.JI_CustomsThirdQuantityInfo, ThirdQtyIntegerMessageError);
		}

		public void TestCheckJI_CustomsThirdQuantity_NumberOfPairs()
		{
			AssertCustomsQuantityRequiresInteger(invoiceLine.JI_CustomsThirdUnitQtyInfo, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs, invoiceLine.JI_CustomsThirdQuantityInfo, ThirdQtyIntegerMessageError);
		}

		public void TestCheckJI_CustomsThirdQuantity_DecimalType()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_CustomsThirdUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs;
				invoiceLine.JI_CustomsThirdQuantity = 123456789.123m;
				AssertHasMessageError("Integer Type", invoiceLine.JI_CustomsThirdQuantityInfo, ThirdQtyIntegerMessageError);
				invoiceLine.JI_CustomsThirdUnitQty = "ABC";
				invoiceLine.Validation.ValidateJI_CustomsThirdQuantity();
				AssertNoMessageError("Decimal Type", invoiceLine.JI_CustomsThirdQuantityInfo, ThirdQtyIntegerMessageError);
			});
		}

		public void TestCheckJI_Tariff()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var info = invoiceLine.JI_TariffInfo;
			var msgError = "The Tariff Code entered is not valid for the current context.";

			invoiceLine.JI_Tariff = ZString.Empty;
			AssertNoMessageErrorContaining(info, msgError);

			invoiceLine.JI_Tariff = "20064000004";
			AssertHasMessageErrorContaining(info, msgError);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeExport = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Export);
			Factory.Save();
			var tariffExport = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeExport.PK, "08091998", ZDateTime.BrettsBirthday, ZDateTime.Today);
			invoiceLine.JI_Tariff = tariffExport.ZZ1_TariffCode;
			AssertNoMessageErrorContaining(info, msgError);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			invoiceLine.JI_Tariff = ZString.Empty;
			AssertNoMessageErrorContaining(info, msgError);

			invoiceLine.JI_Tariff = "20064000004";
			AssertHasMessageErrorContaining(info, msgError);

			invoiceLine.JI_Tariff = tariffExport.ZZ1_TariffCode;
			AssertHasMessageErrorContaining(info, msgError);

			var tariffTypeImport = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Constants.TariffTypes.Import);
			Factory.Save();
			var tariffImport = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "1006400000", ZDateTime.BrettsBirthday, ZDateTime.Today);
			invoiceLine.JI_Tariff = tariffImport.ZZ1_TariffCode;
			AssertHasMessageErrorContaining(info, msgError);

			var tariffNational = helper.CreateTariffNationalCode(Core.Constants.CountryCodes.Germany, tariffImport.PK, "4", ZDateTime.BrettsBirthday, ZDateTime.Today, ZDate.BrettsBirthday);
			invoiceLine.JI_Tariff = tariffNational.ZZ1_TariffCode;
			AssertNoMessageErrorContaining(info, msgError);
		}

		public void TestCheckJI_Tariff_SupplementaryCodeForMeursing()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var testHelper = new UniversalReferenceTestDataHelper(Factory);

			var tradeGroupStandard = testHelper.CreateTradeGroup(Core.Constants.CountryCodes.Germany, "STANDARD", date1, date4);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date4);

			var tariffType = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Constants.TariffTypes.Import);
			Factory.Save();
			var cusTariff = testHelper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffType.PK, "1234567890", date1, date4, "dummy Description 0");

			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Germany, Constants.RateTypes.Duty, "Duty");
			var rateCode11 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A00", dutyRateType.PK);
			var preferenceFou = testHelper.CreatePreferenceForCountry("FOU", "Fourth", Core.Constants.CountryCodes.Germany);
			var testRate = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, rateFormula: "0", preferencePk: preferenceFou.PK, dataGrouping: Core.Constants.CountryCodes.Germany);
			testHelper.CreateCusApplicability(testRate, tradeGroupStandard, date1, date4, additionalCode: "1");
			Factory.Save();

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			invoiceLine.JI_PrimaryPreference = "FOU";
			invoiceLine.JI_SupplementaryCode1 = "7444";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertEquals(false, invoiceLine.JI_TariffInfo.HasMessageError("No supplementary code matching pattern '7NNN' should exist when Meursing is not applicable."));
		}

		public void TestCheckJI_CEI()
		{
			invoiceLine.JI_CEI = ZGuid.Empty;
			invoiceLine.Validation.ValidateJI_CEI();
			AssertHasMessageErrorContaining(invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_ZZF_NKTaxType()
		{
			invoiceLine.JI_ZZF_NKTaxType = "DV1";
			AssertNoNotifications(invoiceLine.JI_ZZF_NKTaxTypeInfo);
		}

		public void TestCheckJI_CustomsSecondQuantity_Mandatory()
		{
			const string messageError = "You have not entered a valid Supp. Qty";
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_CustomsSecondQuantityInfo, messageError, "UnitQTY empty");

				invoiceLine.JI_CustomsSecondUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, messageError, "UnitQTY not empty");
			});
		}

		public void TestCheckOutwardMRN()
		{
			invoiceLine.Validation.ValidateOutwardMRN();
			AssertNoNotifications(invoiceLine.OutwardMRNInfo);
		}

		public void TestCheckOutwardDecisiveDate()
		{
			invoiceLine.Validation.ValidateOutwardDecisiveDate();
			AssertNoNotifications(invoiceLine.OutwardDecisiveDateInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;

		void AssertCustomsQuantityRequiresInteger(ZPropertyInfo unitQtyPropertyInfo, ZString unitQty, ZPropertyInfo qtyPropertyInfo, ZString expectedMessageError)
		{
			unitQtyPropertyInfo.Value = unitQty;
			CombineAssertions(() =>
			{
				qtyPropertyInfo.Value = (ZDecimal)123456789.123;
				AssertHasMessageError("Decimal " + unitQty, qtyPropertyInfo, expectedMessageError);

				qtyPropertyInfo.Value = (ZDecimal)123456789m;
				AssertNoMessageError("Integer " + unitQty, qtyPropertyInfo, expectedMessageError);
			});
		}

		const string SupplementaryQtyIntegerMessageError = "Only integer values are allowed for this Supplementary Quantity Unit";
		const string ThirdQtyIntegerMessageError = "Only integer values are allowed for this Third Qty Unit";
	}
}
