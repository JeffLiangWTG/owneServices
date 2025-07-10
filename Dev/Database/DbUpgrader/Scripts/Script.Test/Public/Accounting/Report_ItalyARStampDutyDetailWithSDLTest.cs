using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_ItalyARStampDutyDetailWithSDL))]
	class Report_ItalyARStampDutyDetailWithSDLTest : DbCreateScriptTest
	{
		public void TestColumnCount()
		{
			string sql = @"select * from Report_ItalyARStampDutyDetailWithSDL (
			 null, '1900-1-1 00:00:00', '2079-1-1 00:00:00' , N'4E20F999-8B35-46CF-BE6C-E042CCA5DB15', null, 'ALL', 'ALL', 0 )";

			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				AssertEquals(34, reader.FieldCount);
			}

			sql = @"select * from Report_ItalyARStampDutyDetailWithSDL (
			 null, '1900-1-1 00:00:00', '2079-1-1 00:00:00' , N'4E20F999-8B35-46CF-BE6C-E042CCA5DB15', null, 'ALL', 'ALL', 1 )";

			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				AssertEquals(34, reader.FieldCount);
			}

			sql = @"select * from Report_ItalyARStampDutyDetailWithSDL (
			 null, '1900-1-1 00:00:00', '2079-1-1 00:00:00' , N'4E20F999-8B35-46CF-BE6C-E042CCA5DB15', null, 'ALL', 'ALL', 2 )";

			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				AssertEquals(34, reader.FieldCount);
			}
		}
	}
}

