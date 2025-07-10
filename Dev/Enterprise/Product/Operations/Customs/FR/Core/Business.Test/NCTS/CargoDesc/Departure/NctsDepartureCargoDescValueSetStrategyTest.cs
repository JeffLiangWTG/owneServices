using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class NctsDepartureCargoDescValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestPW_BondAmountChanged()
		{
			SetUpTariff(Core.Constants.CountryCodes.France);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var guarantee = nctsHeader.Guarantees.AddNew();
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_HarmonisedTariff = "0304798000";
			goodsItem.BY_ZZF_NKTaxType = ZString.Empty;
			goodsItem.BY_MonetaryValue = 1_000m;

			AssertEquals(344m, guarantee.PW_BondAmount);

			goodsItem.BY_MonetaryValue = 500m;
			AssertEquals(172m, guarantee.PW_BondAmount);

			goodsItem.BY_HarmonisedTariff = "0403909900";
			AssertEquals(304m, guarantee.PW_BondAmount);

			goodsItem.BY_RN_NKCountryOfOrigin = "FR";
			AssertEquals(100m, guarantee.PW_BondAmount);

			goodsItem.BY_ZZF_NKTaxType = "RID";
			AssertEquals(25m, guarantee.PW_BondAmount);
		}

		void SetUpTariff(string countryCode)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "test country", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			Factory.Save();

			var tariff1 = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "0304798000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff2 = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "0403909900", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.CreateCusRateCode(Factory, "A00", rateType1.PK);
			var reference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var reference2 = helper.CreatePreferenceForCountryAndGrouping("200", "200", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");

			var rateType2 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "SEC");
			var rateCode2 = helper.CreateCusRateCode(Factory, "SEC", rateType2.PK);

			var rate11 = helper.CreateRefCusRate(tariff1.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.12", reference1.PK);
			var rate12 = helper.CreateRefCusRate(tariff1.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", reference2.PK);
			var rate13 = helper.CreateRefCusRate(tariff1.PK, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", reference1.PK);

			var rate21 = helper.CreateRefCusRate(tariff2.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.34", reference1.PK);
			var rate22 = helper.CreateRefCusRate(tariff2.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", reference2.PK);
			var rate23 = helper.CreateRefCusRate(tariff2.PK, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", reference1.PK);

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var t1 = helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate11.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate12.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate13.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateCusApplicability(rate21.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate22.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate23.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateTaxOrFee("RID", 0.05m, countryCode);
			helper.CreateTaxOrFee("ORD", 0.2m, countryCode);
			helper.CreateNewOrGetExistingVATApplicability(tariff1, countryCode, "RID");
			helper.CreateNewOrGetExistingVATApplicability(tariff1, countryCode, "ORD");

			helper.CreateNewOrGetExistingVATApplicability(tariff2, countryCode, "RID");
			helper.CreateNewOrGetExistingVATApplicability(tariff2, countryCode, "ORD");

			Factory.Save();
		}

		public void TestSettingBY_GrossWeight()
		{
			var movementHeader = PrepareCalculationData();
			new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(movementHeader);

			var goodItem1 = movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().FirstOrDefault(x => x.BY_LineNo == 1);
			var goodItem2 = movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().FirstOrDefault(x => x.BY_LineNo == 2);

			AssertEquals(1, goodItem1.Fees.Count);
			AssertEquals(0, goodItem2.Fees.Count);

			var fee = goodItem1.Fees.FirstOrDefault();
			AssertEquals("V905", fee.BFE_ChargeType);
			AssertEquals(2m, fee.BFE_ChargeAmount);

			goodItem1.BY_GrossWeight = 3000m;
			AssertEquals(1, goodItem1.Fees.Count);
			AssertEquals(0, goodItem2.Fees.Count);
			fee = goodItem1.Fees.FirstOrDefault();
			AssertEquals("V905", fee.BFE_ChargeType);
			AssertEquals(3m, fee.BFE_ChargeAmount);
		}

		public void TestSettingBY_GrossWeightUnit()
		{
			var movementHeader = PrepareCalculationData();
			new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(movementHeader);

			var goodItem1 = movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().FirstOrDefault(x => x.BY_LineNo == 1);
			var goodItem2 = movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().FirstOrDefault(x => x.BY_LineNo == 2);

			AssertEquals(1, goodItem1.Fees.Count);
			AssertEquals(0, goodItem2.Fees.Count);

			var fee = goodItem1.Fees.FirstOrDefault();
			AssertEquals("V905", fee.BFE_ChargeType);
			AssertEquals(2m, fee.BFE_ChargeAmount);

			goodItem1.BY_GrossWeightUnit = "T";
			AssertEquals(1, goodItem1.Fees.Count);
			AssertEquals(0, goodItem2.Fees.Count);
			fee = goodItem1.Fees.FirstOrDefault();
			AssertEquals("V905", fee.BFE_ChargeType);
			AssertEquals(2268m, fee.BFE_ChargeAmount);
		}

		NctsDepartureMovementHeader PrepareCalculationData()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "108", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[TFCL] * 1.1341", "FR");
			referenceDataHelper.CreateHarbourRate("IMP", "395", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[TFCL] * 2.1341", "FR");
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var headerContainer1 = header.DepartureHeaderContainers.AddNew();
			headerContainer1.BC_Mode = "FCL";
			var headerContainer2 = header.DepartureHeaderContainers.AddNew();
			headerContainer2.BC_Mode = "LCL";

			var movementHeader = header.MovementHeader;
			var goodItem1 = movementHeader.GoodsItems.AddNew();
			goodItem1.BY_LineNo = 1;
			goodItem1.BY_GrossWeight = 2000m;
			goodItem1.BY_GrossWeightUnit = "KG";
			var containersPivot = goodItem1.ContainersPivots.AddNew();
			containersPivot.Container = headerContainer1;
			containersPivot.ContainerSelected = true;
			containersPivot.ContainerNumber = "1";
			var goodItem2 = movementHeader.GoodsItems.AddNew();
			goodItem2.BY_LineNo = 2;
			goodItem2.BY_GrossWeight = 3000m;
			goodItem2.BY_GrossWeightUnit = "KG";
			var containersPivot2 = goodItem1.ContainersPivots.AddNew();
			containersPivot2.Container = headerContainer2;
			containersPivot2.ContainerSelected = true;
			containersPivot2.ContainerNumber = "2";

			movementHeader.ChargePaymentOrDestinationID = "108";

			Factory.Save();
			return movementHeader;
		}
	}
}
