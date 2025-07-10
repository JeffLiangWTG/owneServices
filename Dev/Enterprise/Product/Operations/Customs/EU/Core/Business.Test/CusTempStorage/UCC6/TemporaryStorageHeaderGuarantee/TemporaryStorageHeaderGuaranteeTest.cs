using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.CusTempStorage.Testing.TemporaryStorageTestHelper;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageHeaderGuarantee))]
	sealed class TemporaryStorageHeaderGuaranteeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var (_, tsGuarantee) = CreateHeaderAndGuarantee();
			return tsGuarantee;
		}

		public void TestPW_BondAmount_ReadOnlyAndPW_BondAmountInfo()
		{
			var (tsHeader, tsGuarantee) = CreateHeaderAndGuaranteeForTesting();
			tsGuarantee.PW_BondNumber = "ref";
			tsGuarantee.PW_BondAmount = new ZDecimal(10.01);

			CombineAssertions(() =>
			{
				tsGuarantee.PW_Override = false;
				AssertEquals("PW_BondAmount_ReadOnly is true if PW_Override is false.", true, tsGuarantee.PW_BondAmount_ReadOnly_Exposed);
				AssertEquals("PW_BondAmount is readonly if PW_Override is false.", true, tsGuarantee.PW_BondAmountInfo.ReadOnly);

				tsGuarantee.PW_Override = true;
				AssertEquals("PW_BondAmount_ReadOnly is false if PW_Override is true.", false, tsGuarantee.PW_BondAmount_ReadOnly_Exposed);
				AssertEquals("PW_BondAmount is not readonly if PW_Override is true.", false, tsGuarantee.PW_BondAmountInfo.ReadOnly);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		public void TestGuaranteeReadOnlyMember()
		{
			var header = Factory.New<TemporaryStorageHeader>();

			var guarantee = header.Guarantee;
			guarantee.PW_BondNumber = "ref";
			guarantee.PW_Override = true;
			guarantee.PW_BondAmount = new ZDecimal(10.01);

			AssertEquals("PW_BondAmount is not readonly if override is true.", false, guarantee.PW_BondAmountInfo.ReadOnly);
			AssertEquals("PW_RX_NKCurrency is always readonly.", true, guarantee.PW_RX_NKCurrencyInfo.ReadOnly);

			guarantee.PW_Override = false;
			AssertEquals("PW_BondAmount is readonly if override is false.", true, guarantee.PW_BondAmountInfo.ReadOnly);
			AssertEquals("I is always readonly.", true, guarantee.PW_RX_NKCurrencyInfo.ReadOnly);
		}

		public void TestGuaranteeDefaultValue()
		{
			var guarantee = Factory.New<TemporaryStorageHeaderGuarantee>();
			AssertEquals("PW_ApplicationCode default value.", "PNT", guarantee.PW_ApplicationCode);
			AssertEquals("PW_RX_NKCurrency default value.", "EUR", guarantee.PW_RX_NKCurrency);
		}

		public void TestGuaranteeMaxLength()
		{
			var guarantee = Factory.New<TemporaryStorageHeaderGuarantee>();
			AssertEquals("PW_BondNumber Should have a maximum of 35 char length.", 35, guarantee.PW_BondNumberInfo.MaxLength);
			AssertEquals("PW_RX_NKCurrency Should have a maximum of 3 char length.", 3, guarantee.PW_RX_NKCurrencyInfo.MaxLength);
		}

		public void TestLookups() => AssertType<TemporaryStorageHeaderGuaranteeLookups>(Factory.New<TemporaryStorageHeaderGuarantee>().Lookups);
		
		public void TestCusGuaranteeWithoutPermitHolder()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AH3";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var header = Factory.New<TemporaryStorageHeader>();
			var guarantee = header.Guarantee;
			guarantee.PW_BondNumber = "GUARANTEEREF";
			var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
			cusPermitHeader.CPH_Number = "NOREF";
			cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
			cusPermitHeader.CPH_Type = "XX";
			cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			cusPermitHeader.CPH_RN_NKCountryCode = "LV";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNull("CusGuarantee is null when no TST and no same Reference", guarantee.CusGuaranteeWithoutPermitHolder);

				cusPermitHeader.CPH_Type = "TST";
				AssertNull("CusGuarantee is null when TST and no same Reference", guarantee.CusGuaranteeWithoutPermitHolder);

				cusPermitHeader.CPH_Number = "GUARANTEEREF";
				AssertNotNull("CusGuarantee is not null when TST and same Reference", guarantee.CusGuaranteeWithoutPermitHolder);
			});
		}

		public void TestCusGuarantee()
		{
			TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var org1 = Factory.New<OrgHeader>();
			var guaranteeHeader1 = CreateCusGuarantee("GUA1", org1, EUGuaranteeTypeList.Codes.TST, "1");

			var header = Factory.New<TemporaryStorageHeader>();
			var guarantee = header.Guarantee;

			CombineAssertions(() =>
			{
				guarantee.PW_BondNumber = "GUA0";
				AssertNull("CusGuarantee not matched due to the wrong permit number.", guarantee.CusGuarantee);

				guarantee.PW_BondNumber = "GUA1";
				AssertEquals("CusGuarantee matched by permit number.", guaranteeHeader1.PK, guarantee.CusGuarantee.PK);
			});
		}

		public void TestLiabilityAmountChanged()
		{
			const decimal rate = 0.1m;

			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var guarantee = header.Guarantee;

			_ = AddGoodsItemLiability(240m);

			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] PW_BondAmount", expected: 0m, guarantee.PW_BondAmount);

				Factory.Save();
				AssertEquals("LiabilityAmount (on save)", 24m, guarantee.LiabilityAmount);
				AssertEquals("PW_BondAmount (on save)", 24m, guarantee.PW_BondAmount);

				var goodsItem2 = AddGoodsItemLiability(1000m);
				Factory.Save();
				AssertEquals("LiabilityAmount (on adding item)", 124m, guarantee.LiabilityAmount);
				AssertEquals("PW_BondAmount (on adding item)", 124m, guarantee.PW_BondAmount);

				goodsItem2.API_GoodsValue = 1200m;
				Factory.Save();
				AssertEquals("LiabilityAmount (on updating item)", 144m, guarantee.LiabilityAmount);
				AssertEquals("PW_BondAmount (on updating item)", 144m, guarantee.PW_BondAmount);

				guarantee.PW_BondAmount = 5m;
				guarantee.PW_Override = true;
				AssertEquals("LiabilityAmount (with override)", 144m, guarantee.LiabilityAmount);
				AssertEquals("PW_BondAmount (with override)", 5m, guarantee.PW_BondAmount);

				guarantee.PW_BondAmount = 5m;
				guarantee.PW_Override = false;
				AssertEquals("LiabilityAmount (without override)", 144m, guarantee.LiabilityAmount);
				AssertEquals("PW_BondAmount (without override)", 144m, guarantee.PW_BondAmount);
			});

			TemporaryStoragePackedItem AddGoodsItemLiability(ZDecimal monetaryValue)
			{
				var mock = Factory.NewMoq<TemporaryStoragePackedItem>();
				var goodsItem = mock.Object;
				goodsItem.API_GoodsValue = monetaryValue;
				_ = mock
					.Setup(x => x.LiabilityAmount)
					.Returns(() => goodsItem.API_GoodsValue * rate);
				bill.PackedItems.Add(goodsItem);
				return goodsItem;
			}
		}

		public void TestLiabilityAmountCalculation_with_OverrideChange_ValueInEURChange()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var guarantee = header.Guarantee;
			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_SuretyCode = "FUL";

			TemporaryStorageTestHelper.SetUpTariff(Factory, Core.Constants.CountryCodes.Latvia);

			var goodsItem = Factory.New<TemporaryStoragePackedItemForTest>();
			bill.PackedItems.Add(goodsItem);
			goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;
			goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
			goodsItem.API_GoodsValue = 100;
			goodsItem.API_RN_NKGoodsOrigin = "EU";

			CombineAssertions(() =>
			{
				guarantee.PW_Override = false;
				AssertEquals("Pre Condition: For MonetaryValue = 100 and Override Unticked, goodsItem.LiabilityAmount", 12m, goodsItem.LiabilityAmount);
				AssertEquals("Pre Condition: For MonetaryValue = 100 and Override Unticked, BondAmount", 12m, guarantee.PW_BondAmount);

				goodsItem.API_GoodsValue = 200;
				AssertEquals("For MonetaryValue = 200 and Override Unticked, goodsItem.LiabilityAmount", 24m, goodsItem.LiabilityAmount);
				AssertEquals("For MonetaryValue = 200 and Override Unticked, BondAmount", 24m, guarantee.PW_BondAmount);

				guarantee.PW_Override = true;
				goodsItem.API_GoodsValue = 150;
				AssertEquals("For MonetaryValue = 150 and Override Ticked, goodsItem.LiabilityAmount", 18m, goodsItem.LiabilityAmount);
				AssertEquals("For MonetaryValue = 150 and Override Ticked, BondAmount", 24m, guarantee.PW_BondAmount);

				goodsItem.API_GoodsValue = 250;
				AssertEquals("For MonetaryValue = 250 and Override Ticked, goodsItem.LiabilityAmount", 30m, goodsItem.LiabilityAmount);
				AssertEquals("For MonetaryValue = 250 and Override Ticked, BondAmount", 24m, guarantee.PW_BondAmount);

				guarantee.PW_BondAmount = 20m;
				AssertEquals("For MonetaryValue = 250 and Override Ticked, goodsItem.LiabilityAmount", 30m, goodsItem.LiabilityAmount);
				AssertEquals("When set BondAmount manually, BondAmount", 20m, guarantee.PW_BondAmount);

				guarantee.PW_Override = false;
				AssertEquals("For MonetaryValue = 250 and Override Unticked, goodsItem.LiabilityAmount", 30m, goodsItem.LiabilityAmount);
				AssertEquals("Recalculate Bond amount when Override Unticked and MonetaryValue = 250, BondAmount", 30m, guarantee.PW_BondAmount);

				goodsItem.API_GoodsValue = 50;
				AssertEquals("For MonetaryValue = 500 and Override Unticked, goodsItem.LiabilityAmount", 6m, goodsItem.LiabilityAmount);
				AssertEquals("For MonetaryValue = 50 and Override Unticked, BondAmount", 6m, guarantee.PW_BondAmount);
			});
		}

		CusGuaranteeHeader CreateCusGuarantee(ZString number, OrgHeader permitHolder, ZString type, ZString subType)
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Number = number;
			guarantee.CPH_OH_PermitHolder = permitHolder.PK;
			guarantee.CPH_Type = type;
			guarantee.CPH_SubType = subType;
			guarantee.CPH_StartDate = ZDate.BrettsBirthday;
			return guarantee;
		}

		(TemporaryStorageHeader Header, TemporaryStorageHeaderGuarantee Guarantee) CreateHeaderAndGuarantee()
		{
			var tsHeader = Factory.New<TemporaryStorageHeader>();
			var tsGuarantee = tsHeader.Guarantee;

			return (tsHeader, tsGuarantee);
		}

		(TemporaryStorageHeader Header, TemporaryStorageHeaderGuaranteeForTest Guarantee) CreateHeaderAndGuaranteeForTesting()
		{
			var tsHeader = Factory.New<TemporaryStorageHeader>();
			var tsGuarantee = Factory.New<TemporaryStorageHeaderGuaranteeForTest>();
			tsGuarantee.Parent = tsHeader;

			return (tsHeader, tsGuarantee);
		}

		sealed class TemporaryStorageHeaderGuaranteeForTest : TemporaryStorageHeaderGuarantee
		{
			public TemporaryStorageHeaderGuaranteeForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			internal bool PW_BondAmount_ReadOnly_Exposed => PW_BondAmount_ReadOnly;
		}
	}
}

