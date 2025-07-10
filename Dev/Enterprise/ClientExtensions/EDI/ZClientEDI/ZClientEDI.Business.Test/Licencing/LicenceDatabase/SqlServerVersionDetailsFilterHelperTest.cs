using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	class SqlServerVersionDetailsFilterHelperTest : TestCase
	{
		public void TestGetSqlEditionList()
		{
			string expectedElements = "Blank - Not Specified\r\n" + new SqlServerEditionList().ElementsAsString;
			AssertEquals("GetSqlEditionList()", expectedElements, SqlServerVersionDetailsFilterHelper.GetSqlEditionList().ElementsAsString);
		}

		public void TestGetSqlVersionList()
		{
			var list = SqlServerVersionDetailsFilterHelper.GetSqlVersionList();
			AssertEquals("Microsoft SQL Server 2016", list.GetDescriptionFromCode("Sql2016"));
		}
	}
}