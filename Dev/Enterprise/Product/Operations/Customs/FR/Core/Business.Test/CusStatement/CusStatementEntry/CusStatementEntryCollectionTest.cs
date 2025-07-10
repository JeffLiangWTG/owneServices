using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	[TestedType(typeof(CusStatementEntryCollection))]
	class CusStatementEntryCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(CusStatementEntryCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => new CusStatementEntryCollection(Factory.New<CusStatementHeader>());

		public void TestShouldNotLoadDCGLines()
		{
			var statement = Factory.New<CusStatementHeader>();
			var impLine = Factory.New<CusStatementEntry>();
			impLine.B3_B2 = statement.PK;
			impLine.B3_EntryType = StatementEntryTypeList.Codes.Import;
			var expLine = Factory.New<CusStatementEntry>();
			expLine.B3_B2 = statement.PK;
			expLine.B3_EntryType = StatementEntryTypeList.Codes.Export;
			var dcgLine = Factory.New<CusStatementChargesDetail>();
			dcgLine.B3_B2 = statement.PK;
			dcgLine.B3_EntryType = StatementEntryTypeList.Codes.DCG;
			AssertContainsExactElementsInAnyOrder(new[]
			{
				impLine, expLine
			}, statement.Entries);
		}
	}
}
