using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DailyNoticeCusStatementHeaderCollection))]
	sealed class DailyNoticeCusStatementHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestAddNew()
		{
			base.TestAddNew();

			var collection = (DailyNoticeCusStatementHeaderCollection)GetCollectionToTest();
			var header = collection.AddNew();

			Assert("Should default to false.", !header.B2_IsMonthlyStatement);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<CusStatementHeader>();
			parent.B2_IsMonthlyStatement = true;

			return new DailyNoticeCusStatementHeaderCollection(parent);
		}
	}
}
