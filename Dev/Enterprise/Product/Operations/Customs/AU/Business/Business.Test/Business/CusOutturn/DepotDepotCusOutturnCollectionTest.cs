using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DepotCusOutturnCollection))]
	sealed class DepotDepotCusOutturnCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFilter()
		{
			var header = CusOutturnHeader.New(Factory);
			var outturn1 = header.Outturns.AddNew();
			var outturn2 = header.Outturns.AddNew();
			_ = Factory.New<CusOutturn>();

			var outturns = new DepotCusOutturnCollection(Factory);
			outturns.Load();

			int numberOfOutturnsNotInDatabaseReturnedByTheQuery = 0;
			foreach (CusOutturn outturn in outturns)
			{
				if (!outturn.IsInDatabase)
				{
					numberOfOutturnsNotInDatabaseReturnedByTheQuery++;
				}
			}
			AssertEquals("only 2 items - those with a parent header", 2, numberOfOutturnsNotInDatabaseReturnedByTheQuery);
			AssertCollectionContains("contains outturn1", outturn1, outturns);
			AssertCollectionContains("contains outturn2", outturn2, outturns);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new DepotCusOutturnCollection(Factory);
	}
}
