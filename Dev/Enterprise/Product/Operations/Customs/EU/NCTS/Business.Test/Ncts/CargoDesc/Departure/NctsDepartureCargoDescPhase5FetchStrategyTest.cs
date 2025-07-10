using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing.Ncts.CargoDesc.Departure
{
	internal class NctsDepartureCargoDescPhase5FetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForView_BY_Supplements()
		{
			AssertFetchForView(nameof(NctsDepartureCargoDesc.BY_Supplements), new Dictionary<string, int>
			{
				{ CusCodeData.Schema.TableName, 1 },
				{ CusInBondMoveHeader.Schema.TableName, 11 },
			});
		}

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateNctsDepartureCargoDesc();
			}
			Factory.Save();

			var newFactory = NewFactory();
			var goodsItems = newFactory.Load<NctsDepartureCargoDesc>(new ZQuery());
			newFactory.ResetDatabaseLoadCount();

			foreach (var goodsItem in goodsItems)
			{
				goodsItem.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(string.Empty, propertyName)
				});
			}

			foreach (var goodsItem in goodsItems)
			{
				_ = goodsItem.ZPropertyInfoHash.GetPropertySafe(propertyName).Value;
			}

			AssertDbHits(expectedDbHits, newFactory);
		}

		NctsDepartureCargoDesc CreateNctsDepartureCargoDesc()
		{
			return (NctsDepartureCargoDesc)CreateCollectionToTest(Factory).AddNew();
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.Bills.AddNew().GoodsItems;
		}
	}
}
