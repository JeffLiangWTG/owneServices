using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_ItalyARStampDutySummary))]
	class Report_ItalyARStampDutySummaryTest : DbCreateScriptTest
	{
		public void TestColumnCount()
		{
			string sql = @"select * from [Report_ItalyARStampDutySummary](null, '2015-04-03', '2015-04-03', '4e20f999-8b35-46cf-be6c-e042cca5db15', null);";

			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				AssertEquals(11, reader.FieldCount);
			}
		}
	}
}

