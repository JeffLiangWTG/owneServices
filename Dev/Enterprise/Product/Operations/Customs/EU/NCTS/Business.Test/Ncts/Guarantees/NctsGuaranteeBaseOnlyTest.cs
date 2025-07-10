using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsGuarantee))]
	public sealed class NctsGuaranteeBaseOnlyTest : TestCaseWithFactory
	{
		public void TestRecalculateGuaranteeAmount_Phase5DefaultLiabilityAmount()
		{
			var phase5DepartureHeader = Factory.NewWithValidTestData<NctsHeader>();
			phase5DepartureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			phase5DepartureHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var guarantee = Factory.New<NctsGuaranteeWithPhase5DefaultLiabilityAmountAt777>();
			guarantee.PW_ParentTableCode = phase5DepartureHeader.MovementHeader.TablePrefix;
			guarantee.PW_SuretyCode = "HAL";
			guarantee.PW_ParentID = phase5DepartureHeader.MovementHeader.PK;

			CombineAssertions(() =>
			{
				guarantee.SetLiabilityAmount(10000m);
				AssertEquals("set to value", 5000m, guarantee.PW_BondAmount);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
				{
					guarantee.SetLiabilityAmount(0m);
					AssertEquals("after setting to 0, in transition", 777m, guarantee.PW_BondAmount);
				}
			});
		}

		class NctsGuaranteeWithPhase5DefaultLiabilityAmountAt777 : NctsGuarantee
		{
			public NctsGuaranteeWithPhase5DefaultLiabilityAmountAt777(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
			protected override ZDecimal Phase5DefaultLiabilityAmount => 777m;
		}

		public void TestCheckPW_Override()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			AssertEquals("Default value of PW_Override", false, guarantee.PW_Override);
		}

		public void TestPW_OverrideReadOnly()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			AssertEquals("Default value of PW_OverrideReadOnly", false, guarantee.PW_OverrideInfo.ReadOnly);
		}

		public void TestLiabilityAmountCalculation_with_OverrideChange_CommodityCodeChange()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);

			var guarantee = nctsHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_SuretyCode = "FUL";

			NCTSTestHelper.SetUpTariff(Factory, new Dictionary<string, string>()
			{
				{ NCTSTestHelper.TestTariffCode, "VFD * 0.12" },
				{ "03457832", "VFD * 0.13" },
				{ "03039847", "VFD * 0.14" },
				{ "03726391", "VFD * 0.15" },
				{ "03198374", "VFD * 0.16" }
			});

			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_MonetaryValue = 100;

			guarantee.PW_Override = false;
			AssertEquals("Pre Condition: For Tariff = 0304798000 and Override Unticked, BondAmount", 34.4m, guarantee.PW_BondAmount);

			goodsItem.BY_HarmonisedTariff = "03457832";
			AssertEquals("For Tariff = 03457832 and Override Unticked, BondAmount", 35.6m, guarantee.PW_BondAmount);

			guarantee.PW_Override = true;
			goodsItem.BY_HarmonisedTariff = "03039847";
			AssertEquals("For Tariff = 03039847 and Override Ticked, BondAmount", 35.6m, guarantee.PW_BondAmount);

			goodsItem.BY_HarmonisedTariff = "03726391";
			AssertEquals("For Tariff = 03726391 and Override Ticked, BondAmount", 35.6m, guarantee.PW_BondAmount);

			guarantee.PW_BondAmount = 18m;
			AssertEquals("When set BondAmount manually, BondAmount", 18m, guarantee.PW_BondAmount);

			guarantee.PW_Override = false;
			AssertEquals("Recalculate Bond amount when Override Unticked and Tariff = 03726391, BondAmount", 38m, guarantee.PW_BondAmount);

			goodsItem.BY_HarmonisedTariff = "03198374";
			AssertEquals("For Tariff = 03198374 and Override Ticked, BondAmount", 39.2m, guarantee.PW_BondAmount);
		}

		public void TestLiabilityAmountCalculation_with_OverrideChange_ValueInEURChange()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);

			var guarantee = nctsHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_SuretyCode = "FUL";

			NCTSTestHelper.SetUpTariff(Factory);

			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem.BY_MonetaryValue = 100;

			guarantee.PW_Override = false;
			AssertEquals("Pre Condition: For MonetaryValue = 100 and Override Unticked, BondAmount", 34.4m, guarantee.PW_BondAmount);

			goodsItem.BY_MonetaryValue = 200;
			AssertEquals("For MonetaryValue = 200 and Override Unticked, BondAmount", 68.8m, guarantee.PW_BondAmount);

			guarantee.PW_Override = true;
			goodsItem.BY_MonetaryValue = 150;
			AssertEquals("For MonetaryValue = 150 and Override Ticked, BondAmount", 68.8m, guarantee.PW_BondAmount);

			goodsItem.BY_MonetaryValue = 250;
			AssertEquals("For MonetaryValue = 250 and Override Ticked, BondAmount", 68.8m, guarantee.PW_BondAmount);

			guarantee.PW_BondAmount = 18m;
			AssertEquals("When set BondAmount manually, BondAmount", 18m, guarantee.PW_BondAmount);

			guarantee.PW_Override = false;
			AssertEquals("Recalculate Bond amount when Override Unticked and MonetaryValue = 250, BondAmount", 86m, guarantee.PW_BondAmount);

			goodsItem.BY_MonetaryValue = 50;
			AssertEquals("For MonetaryValue = 50 and Override Ticked, BondAmount", 17.2m, guarantee.PW_BondAmount);
		}

		public void TestGetTotalAmountWithRevertedFactor()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondAmount = 100;
			guarantee.PW_SuretyCode = "FUL";

			AssertEquals("Expected for FUL Fraction", 100m, guarantee.GetTotalAmountWithRevertedFactor());

			guarantee.PW_SuretyCode = "HAL";
			AssertEquals("Expected for HAL Fraction", 200m, guarantee.GetTotalAmountWithRevertedFactor());

			guarantee.PW_SuretyCode = "THI";
			AssertEquals("Expected for THI Fraction", 333.33m, guarantee.GetTotalAmountWithRevertedFactor());

			guarantee.PW_SuretyCode = "ZER";
			AssertEquals("Expected for ZER Fraction", 0m, guarantee.GetTotalAmountWithRevertedFactor());

			guarantee.PW_SuretyCode = ZString.Empty;
			AssertEquals("Expected for Empty Fraction", 100m, guarantee.GetTotalAmountWithRevertedFactor());
		}
	}
}
