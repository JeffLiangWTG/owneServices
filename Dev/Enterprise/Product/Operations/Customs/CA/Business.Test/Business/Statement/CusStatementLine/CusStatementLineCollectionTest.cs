using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusStatementLineCollection))]
	sealed class CusStatementLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			AssertEquals(false, ((CusStatementLineCollection)Collection).AllowNew);
		}

		public void TestGetStatementLineFor()
		{
			var line = (CusStatementLine)Collection.AddNew();
			AssertEquals("Just checking dependent object set correctly", statementHeader.PK, line.B3_B2);
			line.B3_EntryNum = "100006789";
			var findLine = ((CusStatementLineCollection)Collection).GetStatementLineFor("100006789", string.Empty, string.Empty);
			AssertNotNull("Found", findLine);
			AssertEquals("The expected line", line.PK, findLine.PK);
			findLine = ((CusStatementLineCollection)Collection).GetStatementLineFor("200006789", string.Empty, string.Empty);
			AssertNull("Not Found", findLine);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			statementHeader = Factory.New<CusStatementHeader>();
			return new CusStatementLineCollection(statementHeader);
		}
		CusStatementHeader statementHeader;
	}
}
