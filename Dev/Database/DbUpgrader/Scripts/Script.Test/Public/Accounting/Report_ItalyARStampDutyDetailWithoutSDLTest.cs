using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_ItalyARStampDutyDetailWithoutSDL))]
	class Report_ItalyARStampDutyDetailWithoutSDLTest : DbCreateScriptTest
	{
		public void TestColumnCount()
		{
			string sql = @"select * from Report_ItalyARStampDutyDetailWithoutSDL (
 null, '1900-1-1 00:00:00', '2079-1-1 00:00:00' , N'4E20F999-8B35-46CF-BE6C-E042CCA5DB15', null, 'ALL', 'ALL' )";

			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				AssertEquals(31, reader.FieldCount);
			}
		}
	}
}

