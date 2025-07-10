using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class ViewUniqueIndexInfoTest : TestCase
	{
		public void TestCreateDropStatement()
		{
			var viewIndex = new ViewUniqueIndexInfo("vw_ZoneUNLOCOByCountry", "FZ_Code, RL_Code");
			AssertEquals("CREATE UNIQUE CLUSTERED INDEX NR_UC__vw_ZoneUNLOCOByCountry ON vw_ZoneUNLOCOByCountry (FZ_Code, RL_Code)", viewIndex.CreateStatement);
			AssertEquals("DROP INDEX NR_UC__vw_ZoneUNLOCOByCountry ON vw_ZoneUNLOCOByCountry", viewIndex.DropStatement);
		}
	}
}
