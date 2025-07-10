using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetail))]
	public class CusLineTariffDetailTest : EU.Business.Testing.CusLineTariffDetailTest
	{
		public new void TestLookups()
		{
			AssertType<CusLineTariffDetailLookups>(tariffDetail.Lookups);
		}

		public new void TestValidation()
		{
			AssertType<CusLineTariffDetailValidation>(tariffDetail.Validation);
		}

		public void TestBZ_Type_ReadOnly()
		{
			AssertEquals(true, tariffDetail.BZ_TypeInfo.ReadOnly);
		}

		public void TestBZ_Type_Caption()
		{
			AssertEquals("Part", DataBoundResourceStrings.GetDataForProperty(tariffDetail.BZ_TypeInfo).Caption);
		}

		public void TestBZ_Tariff_Caption()
		{
			AssertEquals("Code", DataBoundResourceStrings.GetDataForProperty(tariffDetail.BZ_TariffInfo).Caption);
		}

		public void TestBZ_PercentAlcohol_Caption()
		{
			AssertEquals("Degree Percentage", DataBoundResourceStrings.GetDataForProperty(tariffDetail.BZ_PercentAlcoholInfo).Caption);
		}

		public void TestBZ_PercentAlcohol()
		{
			tariffDetail.BZ_PercentAlcohol = 1.555m;
			AssertEquals("Should be accurate to two decimal places", 1.56m, tariffDetail.BZ_PercentAlcohol);
		}

		public void TestBZ_Qty1_DecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusLineTariffDetail), nameof(CusLineTariffDetail.BZ_Qty1), false, x => x.DecimalPlaces == 3);
		}

		public void TestBZ_Qty1_Caption()
		{
			AssertEquals("Quantity", DataBoundResourceStrings.GetDataForProperty(tariffDetail.BZ_Qty1Info).Caption);
		}

		public void TestBZ_UQ1_Caption()
		{
			AssertEquals("UOM", DataBoundResourceStrings.GetDataForProperty(tariffDetail.BZ_UQ1Info).Caption);
		}

		public void TestBZ_UQ1_DefaultValue()
		{
			var tariffType = TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			var tariff = universalReferenceTestDataHelper.CreateTariff(DECountryCode, tariffType.PK, TariffCodeForTest, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			universalReferenceTestDataHelper.CreateTariffUOM(tariff, "CU1", CusLineTariffDetailHelper.KnownUoMs.HLT, DECountryCode);
			Factory.Save();

			AssertEquals("PreReq: BZ_UQ1 empty", ZString.Empty, tariffDetail.BZ_UQ1);
			CombineAssertions(() =>
			{
				tariffDetail.BZ_Tariff = TariffCodeForTest;
				AssertEquals("BZ_UQ1 defaulted as exact 1 TariffUOM exists", CusLineTariffDetailHelper.KnownUoMs.HLT, tariffDetail.BZ_UQ1);

				universalReferenceTestDataHelper.CreateTariffUOM(tariff, "CU1", CusLineTariffDetailHelper.KnownUoMs.HLT6, DECountryCode);
				Factory.Save();
				tariffDetail.BZ_Tariff = ZString.Empty;
				tariffDetail.BZ_UQ1 = ZString.Empty;
				tariffDetail.BZ_Tariff = TariffCodeForTest;
				AssertEquals("BZ_UQ1 not defaulted as more multiple TariffUOM exist", ZString.Empty, tariffDetail.BZ_UQ1);
			});
		}

		public void TestBZ_PercentAlcohol_ReadOnly()
		{
			var tariffType = TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			var tariff = universalReferenceTestDataHelper.CreateTariff(DECountryCode, tariffType.PK, TariffCodeForTest, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var attribute20 = universalReferenceTestDataHelper.CreateTariffAttribute("ExciseType", CusLineTariffDetailHelper.ExciseTypes._20, tariff);

			tariffDetail.BZ_Tariff = TariffCodeForTest;
			AssertEquals("Valid tariff but IsPercentAlcoholMandatory = false", expected: true, tariffDetail.BZ_PercentAlcoholInfo.ReadOnly);

			tariffDetail.BZ_UQ1 = CusLineTariffDetailHelper.KnownUoMs.HLT;
			AssertEquals("Valid tariff and IsPercentAlcoholMandatory = true", expected: false, tariffDetail.BZ_PercentAlcoholInfo.ReadOnly);

			tariffDetail.BZ_Tariff = ZString.Empty;
			AssertEquals("Empty tariff", expected: false, tariffDetail.BZ_PercentAlcoholInfo.ReadOnly);

			tariffDetail.BZ_Tariff = "123";
			AssertEquals("Invalid tariff", expected: false, tariffDetail.BZ_PercentAlcoholInfo.ReadOnly);
		}

		public void TestBZ_TobaccoRetailPrice_DecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusLineTariffDetail), nameof(CusLineTariffDetail.BZ_TobaccoRetailPrice), false, x => x.DecimalPlaces == 6);
		}

		public void TestBZ_TobaccoRetailPrice_ReadOnly()
		{
			var tariffType = TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			var tariff1 = universalReferenceTestDataHelper.CreateTariff(DECountryCode, tariffType.PK, "05122000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			universalReferenceTestDataHelper.CreateTariffAttribute(CusLineTariffDetailHelper.AttributeName, CusLineTariffDetailHelper.ExciseTypes._40, tariff1);
			var tariff2 = universalReferenceTestDataHelper.CreateTariff(DECountryCode, tariffType.PK, "08091998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			universalReferenceTestDataHelper.CreateTariffAttribute(CusLineTariffDetailHelper.AttributeName, CusLineTariffDetailHelper.ExciseTypes._10, tariff2);

			CombineAssertions(() =>
			{
				AssertEquals("No tariff, BZ_TobaccoRetailPrice not readonly", false, tariffDetail.BZ_TobaccoRetailPriceInfo.ReadOnly);

				tariffDetail.BZ_Tariff = "11111111";
				AssertEquals("Invalid tariff, BZ_TobaccoRetailPrice not readonly", false, tariffDetail.BZ_TobaccoRetailPriceInfo.ReadOnly);

				tariffDetail.BZ_Tariff = "05122000";
				AssertEquals("No attribute 'ExciseType' with value '10', BZ_TobaccoRetailPrice readonly", true, tariffDetail.BZ_TobaccoRetailPriceInfo.ReadOnly);

				tariffDetail.BZ_Tariff = "08091998";
				AssertEquals("Has attribute 'ExciseType' with value '10', BZ_TobaccoRetailPrice editable", false, tariffDetail.BZ_TobaccoRetailPriceInfo.ReadOnly);
			});
		}

		public void TestBZ_TobaccoRetailPrice_Caption()
		{
			AssertEquals("Retail Price", DataBoundResourceStrings.GetDataForProperty(tariffDetail.BZ_TobaccoRetailPriceInfo).Caption);
		}

		public void TestUpdateBZ_TobaccoRetailPrice_ForTobaccoRelatedTariff_NAR()
		{
			var tariffType = TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			CreateTariffWithAttributeAndTariffUOM(tariffType, CusLineTariffDetailHelper.ExciseTypes._10, "NAR");
			TestHelper.CreateTaxOrFeeWithType(universalReferenceTestDataHelper, "TSP", "Tobacco Retail Selling Price", "CIG", 0.301491m);

			tariffDetail.BZ_Tariff = TariffCodeForTest;
			AssertEquals("TobaccoRelated Tariff", 0.301491m, tariffDetail.BZ_TobaccoRetailPrice);
		}

		public void TestUpdateBZ_Value_ForTobaccoRelatedTariff_KGM()
		{
			var tariffType = TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			CreateTariffWithAttributeAndTariffUOM(tariffType, CusLineTariffDetailHelper.ExciseTypes._10, "KGM");
			TestHelper.CreateTaxOrFeeWithType(universalReferenceTestDataHelper, "TSP", "Tobacco Retail Selling Price", "TAB", 152.2906m);

			tariffDetail.BZ_Tariff = TariffCodeForTest;
			AssertEquals("Tobacco Related Tariff", 152.2906m, tariffDetail.BZ_TobaccoRetailPrice);
		}

		public void TestUpdateBZ_Value_ForNonTobaccoRelatedTariff()
		{
			var tariffType = TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			CreateTariffWithAttributeAndTariffUOM(tariffType, CusLineTariffDetailHelper.ExciseTypes._30, "KGM");
			TestHelper.CreateTaxOrFeeWithType(universalReferenceTestDataHelper, "TSP", "Tobacco Retail Selling Price", "TAB", 152.2906m);

			tariffDetail.BZ_Tariff = TariffCodeForTest;
			AssertEquals("Non Tobacco Related Tariff", 0m, tariffDetail.BZ_TobaccoRetailPrice);
		}

		public void TestUpdateBZ_PercentAlcohol_NotMandatoryForSelectedTariff()
		{
			var tariffType = TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			var tariff = universalReferenceTestDataHelper.CreateTariff(DECountryCode, tariffType.PK, TariffCodeForTest, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			universalReferenceTestDataHelper.CreateTariffUOM(tariff, "CU1", CusLineTariffDetailHelper.KnownUoMs.HLT6, DECountryCode);
			universalReferenceTestDataHelper.CreateTariffAttribute("ExciseType", CusLineTariffDetailHelper.ExciseTypes._40, tariff);
			Factory.Save();

			tariffDetail.BZ_Type = Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise;
			tariffDetail.BZ_UQ1 = CusLineTariffDetailHelper.KnownUoMs.HLT6;
			tariffDetail.BZ_PercentAlcohol = 1m;
			tariffDetail.BZ_Tariff = TariffCodeForTest;
			AssertEquals(0m, tariffDetail.BZ_PercentAlcohol);
		}

		public void TestUpdateBZ_PercentAlcohol_MandatoryForSelectedTariff()
		{
			var tariffType = TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			var tariff = universalReferenceTestDataHelper.CreateTariff(DECountryCode, tariffType.PK, TariffCodeForTest, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			universalReferenceTestDataHelper.CreateTariffUOM(tariff, "CU1", CusLineTariffDetailHelper.KnownUoMs.HLT6, DECountryCode);
			universalReferenceTestDataHelper.CreateTariffAttribute("ExciseType", CusLineTariffDetailHelper.ExciseTypes._40, tariff);
			Factory.Save();

			tariffDetail.BZ_Type = Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise;
			tariffDetail.BZ_UQ1 = CusLineTariffDetailHelper.KnownUoMs.HLT;
			tariffDetail.BZ_PercentAlcohol = 1m;
			tariffDetail.BZ_Tariff = TariffCodeForTest;
			AssertEquals(1m, tariffDetail.BZ_PercentAlcohol);
		}

		public void TestExciseValue_Caption()
		{
			AssertEquals("Excise Value", DataBoundResourceStrings.GetDataForProperty(tariffDetail.ExciseValueInfo).Caption);
		}

		public void TestExciseValue()
		{
			var tariffType = TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper);
			var tariff = universalReferenceTestDataHelper.CreateTariff(DECountryCode, tariffType.PK, TariffCodeForTest, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			universalReferenceTestDataHelper.CreateTariffAttribute(CusLineTariffDetailHelper.AttributeName, CusLineTariffDetailHelper.ExciseTypes._10, tariff);
			tariffDetail.BZ_Qty1 = 1.556m;

			CombineAssertions(() =>
			{
				AssertEquals("Initially 0", 0m, tariffDetail.ExciseValue);

				tariffDetail.BZ_Tariff = TariffCodeForTest;
				tariffDetail.BZ_TobaccoRetailPrice = 200.74m;
				AssertEquals("1.556 * 200.74, rounded to two decimal places", 312.35m, tariffDetail.ExciseValue);
			});
		}

		public void TestBZ_TobaccoRetailPrice_Setter()
		{
			tariffDetail.BZ_TobaccoRetailPrice = 1.123456m;
			AssertEquals(112.3456m, tariffDetail.BZ_Value);
		}

		public void TestBZ_TobaccoRetailPrice_Getter()
		{
			tariffDetail.BZ_Value = 112.3456m;
			AssertEquals(1.123456m, tariffDetail.BZ_TobaccoRetailPrice);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invLine = invHeader.InvoiceLines.AddNew();
			return invLine.CusLineTariffDetails.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);

			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invLine = invHeader.InvoiceLines.AddNew();
			tariffDetail = invLine.CusLineTariffDetails.AddNew();
		}
		JobComInvoiceLine invLine;
		CusLineTariffDetail tariffDetail;
		UniversalReferenceTestDataHelper universalReferenceTestDataHelper;
		const string TariffCodeForTest = "05122000";
		const string DECountryCode = Core.Constants.CountryCodes.Germany;

		#endregion

		void CreateTariffWithAttributeAndTariffUOM(RefCusTariffType tariffType, ZString exciseType, ZString tariffUOM)
		{
			var tariff = universalReferenceTestDataHelper.CreateTariff(DECountryCode, tariffType.PK, TariffCodeForTest, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			universalReferenceTestDataHelper.CreateTariffAttribute(CusLineTariffDetailHelper.AttributeName, exciseType, tariff);
			universalReferenceTestDataHelper.CreateTariffUOM(tariff, "CU1", tariffUOM);
		}
	}
}
