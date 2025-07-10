using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class CommodityType02ProviderTest : DataProviderTestCase<CommodityType02Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(
				"When 'item' is null",
				() => new CommodityType02Provider(item: null));

			AssertExceptionThrown<ArgumentNullException>(
				"When 'item.Header' is null",
				() => new CommodityType02Provider(item: Factory.New<NctsArrivalCargoDesc>()));
		}

		public void TestDescriptionOfGoods()
		{
			item.BY_Description = "desc";
			AssertEquals("desc", Provider.DescriptionOfGoods);
		}

		public void TestCusCode()
		{
			item.BY_CusC4Number = "cuscode";
			AssertEquals("cuscode", Provider.CusCode);
		}

		public void TestHarmonizedSystemSubHeadingCode()
		{
			item.BY_HarmonisedTariff = "12345678";
			AssertEquals("123456", Provider.HarmonizedSystemSubHeadingCode);
		}

		public void TestCombinedNomenclatureCode()
		{
			item.BY_HarmonisedTariff = "12345678";
			AssertEquals("78", Provider.CombinedNomenclatureCode);
		}

		public void TestCombinedNomenclatureCode_Conditional()
		{
			item.BY_HarmonisedTariff = "123456";
			AssertEquals(string.Empty, Provider.CombinedNomenclatureCode);
		}

		public void TestGrossMass()
		{
			item.BY_GrossWeight = 100.0000m;
			item.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
			AssertEquals("0.1", Provider.GrossMass.ToString());
		}

		public void TestGrossMass_InTransitionPeriod()
		{
			item.BY_GrossWeight = 12345678.123456789m;
			item.BY_GrossWeightUnit = "KG";

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				AssertEquals("GrossMass is rounded to 6 digits", 12345678.123457m, GetProvider().GrossMass);
			});

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals("GrossMass is rounded to 3 digits", 12345678.123m, GetProvider().GrossMass);
			});
		}

		public void TestNetMass()
		{
			item.BY_NetWeight = 100.0000m;
			item.BY_NetWeightUnit = Core.Constants.Weight.Grams;
			AssertEquals("0.1", Provider.NetMass.ToString());
		}

		public void TestNetMass_InTransitionPeriod()
		{
			item.BY_NetWeight = 12345678.123456789m;
			item.BY_NetWeightUnit = "KG";

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				AssertEquals("NetMass is rounded to 6 digits", 12345678.123457m, GetProvider().NetMass);
			});

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals("NetMass is rounded to 3 digits", 12345678.123m, GetProvider().NetMass);
			});
		}

		public void TestDangerousGoods()
		{
			var dangerousGoods = Provider.DangerousGoods.Single();
			AssertEquals("SequenceNumber", 1, dangerousGoods.SequenceNumber);
		}

		protected override CommodityType02Provider GetProvider() => provider;

		protected override void SetUp()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			item = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			provider = new CommodityType02Provider(item);
			var dangerousGood = Factory.New<UNDGDataItem>();
			dangerousGood.DI_ParentID = item.PK;
			dangerousGood.DI_ParentTableCode = item.TablePrefix;
		}

		CommodityType02Provider provider;
		NctsArrivalCargoDesc item;
	}
}
