using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetail))]
	sealed class CusLineTariffDetailTest : CusLineTariffDetailAbstractTest
	{
		public void TestBZ_Type()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BZ_Type Caption", "Type", DataBoundResourceStrings.GetDataForProperty(cusLineTariffDetail.BZ_TypeInfo).Caption);
				AssertEquals("BZ_Type Read Only", false, cusLineTariffDetail.BZ_TypeInfo.ReadOnly);
			});
		}

		public void TestDefaultingTypeFromTariffCode()
		{
			UniversalReferenceTestDataHelper universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);

			var tariffTypeX1 = universalReferenceTestDataHelper.CreateTariffType(Core.Constants.CountryCodes.Ireland, "X1");
			var tariffTypeY = universalReferenceTestDataHelper.CreateTariffType(Core.Constants.CountryCodes.Ireland, "Y");

			Factory.Save();

			var tariffX101 = universalReferenceTestDataHelper.CreateTariff(Core.Constants.CountryCodes.Ireland, tariffTypeX1.PK, "X101", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "X101 Desc");
			var tariffY303 = universalReferenceTestDataHelper.CreateTariff(Core.Constants.CountryCodes.Ireland, tariffTypeY.PK, "Y303", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Y303 Desc");

			Factory.Save();

			CombineAssertions(() =>
			{
				cusLineTariffDetail.BZ_Tariff = string.Empty;
				AssertEquals("BZ_Type should be empty when BZ_Tariff is empty", string.Empty, cusLineTariffDetail.BZ_Type);

				cusLineTariffDetail.BZ_Tariff = "%";
				AssertEquals("BZ_Type should be set to default when BZ_Tariff is not a known tariff", string.Empty, cusLineTariffDetail.BZ_Type);

				cusLineTariffDetail.BZ_Tariff = tariffX101.ZZ1_TariffCode;
				AssertEquals("BZ_Type should be set to default when BZ_Tariff is filled", tariffTypeX1.ZZI_TariffType, cusLineTariffDetail.BZ_Type);

				cusLineTariffDetail.BZ_Tariff = tariffY303.ZZ1_TariffCode;
				AssertEquals("BZ_Type should be set to default when BZ_Tariff is filled", tariffTypeY.ZZI_TariffType, cusLineTariffDetail.BZ_Type);
			});
		}

		public void TestBZ_Tariff()
		{
			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(cusLineTariffDetail.BZ_TariffInfo, string.Empty, "Excise Reference Number", "ERN");
			});
		}

		public void TestBZ_Tariff_DefaultBZ_UQ1AndBZ_UQ2()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Ireland, "X2");
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "X203", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Cigarettes containin tobaco");
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.Ireland, "EXC", "Excise");
			var rateCode = helper.CreateCusRateCode(Factory, "EXC", rateType.PK, description: "Excise", countryCode: Core.Constants.CountryCodes.Ireland);
			helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "428.48 * [MIL] + 0.0885 * [RSP]", dataGrouping: Core.Constants.CountryCodes.Ireland);
			tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "X205", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Cigarettes containing cloves");
			helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "479.37 * [MIL]", dataGrouping: Core.Constants.CountryCodes.Ireland);
			tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "X207", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Other manufactured tobacco");
			helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "0", dataGrouping: Core.Constants.CountryCodes.Ireland);
			Factory.Save();

			cusLineTariffDetail.BZ_Type = "X2";
			CombineAssertions("MIL and RSP", () =>
			{
				cusLineTariffDetail.BZ_UQ1 = ZString.Empty;
				cusLineTariffDetail.BZ_UQ2 = ZString.Empty;
				cusLineTariffDetail.BZ_Tariff = "X203";
				AssertEquals("BZ_UQ1", "MIL", cusLineTariffDetail.BZ_UQ1);
				AssertEquals("BZ_UQ2", "RSP", cusLineTariffDetail.BZ_UQ2);
			});

			CombineAssertions("MIL", () =>
			{
				cusLineTariffDetail.BZ_UQ1 = ZString.Empty;
				cusLineTariffDetail.BZ_UQ2 = ZString.Empty;
				cusLineTariffDetail.BZ_Tariff = "X205";
				AssertEquals("BZ_UQ1", "MIL", cusLineTariffDetail.BZ_UQ1);
				AssertEquals("BZ_UQ2", ZString.Empty, cusLineTariffDetail.BZ_UQ2);
			});

			CombineAssertions("empty", () =>
			{
				cusLineTariffDetail.BZ_UQ1 = ZString.Empty;
				cusLineTariffDetail.BZ_UQ2 = ZString.Empty;
				cusLineTariffDetail.BZ_Tariff = "X207";
				AssertEquals("BZ_UQ1", ZString.Empty, cusLineTariffDetail.BZ_UQ1);
				AssertEquals("BZ_UQ2", ZString.Empty, cusLineTariffDetail.BZ_UQ2);
			});
		}

		public void TestExciseReferenceNumberDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Ireland, "X2");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "X201", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Cigars");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "X203", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Cigarettes containin tobaco");
			Factory.Save();

			cusLineTariffDetail.BZ_Type = "X2";
			cusLineTariffDetail.BZ_Tariff = "X201";
			AssertEquals("Cigars", cusLineTariffDetail.ExciseReferenceNumberDescription);

			cusLineTariffDetail.BZ_Tariff = "X203";
			AssertEquals("Cigarettes containin tobaco", cusLineTariffDetail.ExciseReferenceNumberDescription);
		}

		public void TestExciseReferenceNumberDescription_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(cusLineTariffDetail.ExciseReferenceNumberDescriptionInfo, string.Empty, "ERN Description", "Desc.", "ERN Desc.", "Excise Reference Number Description");
		}

		public void TestBZ_Qty1()
		{
			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(cusLineTariffDetail.BZ_Qty1Info, string.Empty, "Quantity", "Qty");
				AssertEquals("BZ_Qty1 Read Only", false, cusLineTariffDetail.BZ_Qty1Info.ReadOnly);
			});
		}

		public void TestBZ_UQ1()
		{
			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(cusLineTariffDetail.BZ_UQ1Info, string.Empty, "Quantity Unit", "Unit", "Qty Unit");
				AssertEquals("BZ_UQ1 Read Only", false, cusLineTariffDetail.BZ_UQ1Info.ReadOnly);
			});
		}

		public void TestZG_MethodOfPayment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZG_MethodOfPayment Caption", "Payment Method", DataBoundResourceStrings.GetDataForProperty(cusLineTariffDetail.ZG_MethodOfPaymentInfo).Caption);
				AssertEquals("ZG_MethodOfPayment Read Only", false, cusLineTariffDetail.ZG_MethodOfPaymentInfo.ReadOnly);
			});
		}

		public void TestPaymentMethodDescription()
		{
			cusLineTariffDetail.ZG_MethodOfPayment = PaymentMethodList.Codes.A;
			AssertEquals("Payment in cash", cusLineTariffDetail.PaymentMethodDescription);

			cusLineTariffDetail.ZG_MethodOfPayment = PaymentMethodList.Codes.E;
			AssertEquals("Deferred or postponed payment", cusLineTariffDetail.PaymentMethodDescription);
		}

		public void TestPaymentMethodDescription_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(cusLineTariffDetail.PaymentMethodDescriptionInfo, string.Empty, "Description", "Desc.", string.Empty, "Payment Method Description");
		}

		public void TestBZ_Qty2()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(cusLineTariffDetail.BZ_Qty2Info, string.Empty, "Quantity 2", "Qty 2");
		}

		public void TestBZ_UQ2()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(cusLineTariffDetail.BZ_UQ2Info, string.Empty, "Quantity Unit 2", "Unit 2");
		}

		public void TestTypeDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Ireland, "X1");
			tariffType.ZZI_Description = "Mineral Oil Tax";

			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(cusLineTariffDetail.TypeDescriptionInfo, string.Empty, "Type Description", "Type Desc.");
				AssertEquals("Type Description Read Only", true, cusLineTariffDetail.TypeDescriptionInfo.ReadOnly);

				cusLineTariffDetail.BZ_Type = "X1";
				AssertEquals("Type Description should match with the list", "Mineral Oil Tax", cusLineTariffDetail.TypeDescription);

				cusLineTariffDetail.BZ_Type = "A1";
				AssertEquals("Type Description should be empty when unknown code is entered", ZString.Empty, cusLineTariffDetail.TypeDescription);
			});
		}

		public void TestRateFormula()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Ireland, "X2");
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "X203", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Cigarettes containing tobaco (a)");
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.Ireland, "X2", description: "Tobacco Products Tax");
			var rateCode = helper.CreateCusRateCode(Factory, "EXC", rateType.PK, description: "Excise", countryCode: Core.Constants.CountryCodes.Ireland);
			var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "402.32 * [MIL] + 0.0873 + [RSP]", dataGrouping: Core.Constants.CountryCodes.Ireland);
			helper.CreateRateUOM(rate.PK, "MIL");
			helper.CreateRateUOM(rate.PK, "RSP");
			helper.CreateTariffRelationship(tariff.PK, tariffType.PK, "2402209000");

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "MIL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "RSP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Rate Formula Caption", "Rate Formula", DataBoundResourceStrings.GetDataForProperty(cusLineTariffDetail.RateFormulaInfo).Caption);
				AssertEquals("Rate Formula Read Only", true, cusLineTariffDetail.RateFormulaInfo.ReadOnly);

				cusLineTariffDetail.BZ_Type = "X2";
				cusLineTariffDetail.BZ_Tariff = "X203";
				AssertEquals("Rate Formula should match with the selected RefCusTariff (Excise Reference Number)", "402.32 * [MIL] + 0.0873 + [RSP]", cusLineTariffDetail.RateFormula);
			});
		}

		public void TestRateFormulaUnits()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Ireland, "X2");
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "X203", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Cigarettes containin tobaco");
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.Ireland, "EXC", "Excise");
			var rateCode = helper.CreateCusRateCode(Factory, "EXC", rateType.PK, description: "Excise", countryCode: Core.Constants.CountryCodes.Ireland);
			helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "428.48 * [MIL] + 0.0885 * [RSP]", dataGrouping: Core.Constants.CountryCodes.Ireland);
			tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "X205", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Cigarettes containing cloves");
			helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "479.37 * [MIL]", dataGrouping: Core.Constants.CountryCodes.Ireland);
			tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "X207", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Other manufactured tobacco");
			helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "0", dataGrouping: Core.Constants.CountryCodes.Ireland);
			Factory.Save();

			cusLineTariffDetail.BZ_Type = "X2";
			CombineAssertions("MIL and RSP", () =>
			{
				cusLineTariffDetail.BZ_Tariff = "X203";
				var (firstPartUnit, secondPartUnit) = cusLineTariffDetail.RateFormulaUnits;
				AssertEquals("FirstPartUnit", "MIL", firstPartUnit);
				AssertEquals("SecondPartUnit", "RSP", secondPartUnit);
			});

			CombineAssertions("MIL", () =>
			{
				cusLineTariffDetail.BZ_Tariff = "X205";
				var (firstPartUnit, secondPartUnit) = cusLineTariffDetail.RateFormulaUnits;
				AssertEquals("FirstPartUnit", "MIL", firstPartUnit);
				AssertEquals("SecondPartUnit", ZString.Empty, secondPartUnit);
			});

			CombineAssertions("empty", () =>
			{
				cusLineTariffDetail.BZ_Tariff = "X207";
				var (firstPartUnit, secondPartUnit) = cusLineTariffDetail.RateFormulaUnits;
				AssertEquals("FirstPartUnit", ZString.Empty, firstPartUnit);
				AssertEquals("SecondPartUnit", ZString.Empty, secondPartUnit);
			});
		}

		public void TestLookups()
		{
			AssertType<CusLineTariffDetailLookups>(cusLineTariffDetail.Lookups);
		}

		public void TestValidation()
		{
			AssertType<CusLineTariffDetailValidation>(cusLineTariffDetail.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return cusLineTariffDetail;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			cusLineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
		}

		CusLineTariffDetail cusLineTariffDetail;
	}
}
