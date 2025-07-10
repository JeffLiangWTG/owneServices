using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using PhaseCodes = Enterprise.Customs.Common.CusInBondApplicationCodeList.Codes;

namespace Enterprise.Customs.EU.NCTS.ServiceTasks.Test
{
	class NctsLiabilityUpdaterTest : TestCaseWithFactory
	{
		[TestDate(2024, 7, 24)]
		public void TestUpdateLiability()
		{
			NCTSTestHelper.SetUpTariff(Factory);

			var oldCreateDate = ZDateTime.Now.AddYears(-2).AddSeconds(-1);
			var recentCreateDate = ZDateTime.Now.AddMonths(-1);

			var phase4OldHeader = CreateHeaderWithGoodsItems(oldCreateDate, PhaseCodes.NCTS4);
			var phase4ArrivalHeader = CreateHeaderWithGoodsItems(ZDateTime.Now, PhaseCodes.NCTS4, NctsMovementType.Codes.Arrival);
			var phase4RecentHeader = CreateHeaderWithGoodsItems(recentCreateDate, PhaseCodes.NCTS4);
			var phase4NewHeader = CreateHeaderWithGoodsItems(ZDateTime.Now, PhaseCodes.NCTS4);

			var phase5OldHeader = CreateHeaderWithGoodsItems(oldCreateDate, PhaseCodes.NCTS5);
			var phase5ArrivalHeader = CreateHeaderWithGoodsItems(ZDateTime.Now, PhaseCodes.NCTS5, NctsMovementType.Codes.Arrival);
			var phase5RecentHeader = CreateHeaderWithGoodsItems(recentCreateDate, PhaseCodes.NCTS5);
			var phase5NewHeader = CreateHeaderWithGoodsItems(ZDateTime.Now, PhaseCodes.NCTS5);

			new NctsLiabilityUpdater(Factory).Run(new TestServiceLogger(), new CancellationToken());

			CombineAssertions(() =>
			{
				AssertEquals("Movement is too old for calculation", 0, GetTotalFeesCount(phase4OldHeader));
				AssertEquals("Arrival movement should not be included", 0, GetTotalFeesCount(phase4ArrivalHeader));
				AssertGreaterThan("Has fees populated", GetTotalFeesCount(phase4RecentHeader), 0);
				AssertGreaterThan("Has fees populated", GetTotalFeesCount(phase4NewHeader), 0);

				AssertEquals("Movement is too old for calculation", 0, GetTotalFeesCount(phase5OldHeader));
				AssertEquals("Arrival movement should not be included", 0, GetTotalFeesCount(phase5ArrivalHeader));
				AssertGreaterThan("Has fees populated", GetTotalFeesCount(phase5RecentHeader), 0);
				AssertGreaterThan("Has fees populated", GetTotalFeesCount(phase5NewHeader), 0);
			});
		}

		NctsHeader CreateHeaderWithGoodsItems(ZDateTime createDate, string phaseCode, string movementType = NctsMovementType.Codes.Departure)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = phaseCode;
			header.BH_SystemCreateTimeUtc = createDate;
			header.SetMovementType(movementType);
			if (header.IsPhase5)
			{
				var bill = header.Bills.AddNew();
				AddGoodsItems<NctsCommonCargoDesc>(header.IsDepartureMovement ? bill.GoodsItems : bill.ArrivalGoodsItems);
			}
			else
			{
				AddGoodsItems<NctsCommonCargoDesc>(header.IsDepartureMovement ? header.MovementHeader.GoodsItems : header.ArrivalMovementHeader.GoodsItems);
			}
			return header;
		}

		void AddGoodsItems<T>(INctsCommonCargoDescCollection<T> goodsItems) where T : NctsCommonCargoDesc
		{
			var goodsItem1 = goodsItems.AddNew();
			goodsItem1.BY_MonetaryValue = 1;
			goodsItem1.BY_RX_NKCurrency = Constants.CurrencyCodes.EuropeanUnion;
			goodsItem1.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem1.Fees.DeleteAll();

			var goodsItem2 = goodsItems.AddNew();
			goodsItem2.BY_MonetaryValue = 2;
			goodsItem2.BY_RX_NKCurrency = Constants.CurrencyCodes.EuropeanUnion;
			goodsItem2.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
			goodsItem2.Fees.DeleteAll();
		}

		int GetTotalFeesCount(NctsHeader header) => (header.IsDepartureMovement ? header.DepartureGoodsItems : header.GetGoodsItems()).Sum(item => item.Fees.Count);
	}
}
