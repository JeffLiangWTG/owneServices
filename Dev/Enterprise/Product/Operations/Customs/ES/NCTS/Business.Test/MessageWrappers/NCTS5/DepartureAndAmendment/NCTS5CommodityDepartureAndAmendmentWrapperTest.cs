using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommodityDepartureAndAmendmentWrapperTest : WrapperHelperTest<NCTS5CommodityDepartureAndAmendmentWrapper>
	{
		public void TestDangerousGoods()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DangerousGoods", 0, wrapper.DangerousGoods.Count);

				var subs1 = Factory.New<UNDGSubstance>();
				subs1.DG_Code = "1001S";
				subs1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				var dangerousGood1 = goodsItem.UNDGs.AddNew();
				dangerousGood1.DI_DG = subs1.PK;

				var subs2 = Factory.New<UNDGSubstance>();
				subs2.DG_Code = "1002S";
				subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				var dangerousGood2 = goodsItem.UNDGs.AddNew();
				dangerousGood2.DI_DG = subs2.PK;

				wrapper = new NCTS5CommodityDepartureAndAmendmentWrapper(goodsItem);
				var dangerousGoods = wrapper.DangerousGoods;
				AssertEquals("Expected filled DangerousGoods", 2, dangerousGoods.Count);
				AssertSame("Cached DangerousGoods", wrapper.DangerousGoods, dangerousGoods);

				var dangerousGoodsList = dangerousGoods.ToArray();
				AssertEquals("Expected filled first DangerousGoods, SequenceNumber", "1", dangerousGoodsList[0].SequenceNumber);
				AssertEquals("Expected filled first DangerousGoods, UNDangerousCode", "1001", dangerousGoodsList[0].UNDangerousCode);
				AssertEquals("Expected filled second DangerousGoods, SequenceNumber", "2", dangerousGoodsList[1].SequenceNumber);
				AssertEquals("Expected filled second DangerousGoods, UNDangerousCode", "1002", dangerousGoodsList[1].UNDangerousCode);
			});
		}

		public void TestGoodsMeasure()
		{
			var goodsMeasure = wrapper.GoodsMeasure;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled GoodsMeasure", goodsMeasure);
				AssertSame("Cached GoodsMeasure", wrapper.GoodsMeasure, goodsMeasure);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsBill = nctsHeader.Bills.AddNew();
			goodsItem = nctsBill.GoodsItems.AddNew();
			wrapper = new NCTS5CommodityDepartureAndAmendmentWrapper(goodsItem);
		}

		NctsDepartureCargoDesc goodsItem;
		NCTS5CommodityDepartureAndAmendmentWrapper wrapper;

		protected override NCTS5CommodityDepartureAndAmendmentWrapper GetProvider() => wrapper;
	}
}
