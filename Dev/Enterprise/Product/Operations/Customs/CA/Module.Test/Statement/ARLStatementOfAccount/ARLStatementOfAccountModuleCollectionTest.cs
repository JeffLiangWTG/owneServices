using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(ARLStatementOfAccountModuleCollection))]
	sealed class ARLStatementOfAccountModuleCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAdditionalFilter()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_IsMonthlyStatement = true;

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_IsMonthlyStatement = false;

			Factory.Save();

			var collection = new ARLStatementOfAccountModuleCollection(Factory);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { statement1.PK }, collection.Select(x => x.PK));

			statement1.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			Factory.Save();

			collection.Load();
			AssertContainsExactElementsInAnyOrder(Array.Empty<ZGuid>(), collection.Select(x => x.PK));
		}

		protected override Type GetExpectedCollectionType() => typeof(ARLStatementOfAccountModuleCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => new ARLStatementOfAccountModuleCollection(Factory);
	}
}
