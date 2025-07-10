using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(DailyNoticeReconciliationModuleCollection))]
	sealed class DailyNoticeReconciliationModuleCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAdditionalFilter()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_IsMonthlyStatement = true;

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_IsMonthlyStatement = false;

			Factory.Save();

			var collection = new DailyNoticeReconciliationModuleCollection(Factory);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { statement2.PK }, collection.Select(x => x.PK));

			statement2.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			Factory.Save();

			collection.Load();
			AssertContainsExactElementsInAnyOrder(Array.Empty<ZGuid>(), collection.Select(x => x.PK));
		}

		protected override Type GetExpectedCollectionType() => typeof(DailyNoticeReconciliationModuleCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => new DailyNoticeReconciliationModuleCollection(Factory);
	}
}
