using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(csfn_GetEdifactElementInline))]
	class csfn_GetEdifactElementInlineTest : DbCreateScriptTest
	{
		public void TestGetEdifactElement()
		{
			string functionSql = "SELECT * FROM csfn_GetEdifactElementInline(@Text, @SegmentPattern, @Element, @SubElement, @topN)";

			using (var command = Db.Connection.Command(functionSql))
			{
				command.AddParameter("@Text", SqlDbType.VarChar, "EQD+CN+CONTAINER1'EQD+CN+CONTAINER2");
				command.AddParameter("@SegmentPattern", SqlDbType.VarChar, "EQD");
				command.AddParameter("@Element", SqlDbType.Int, 1);
				command.AddParameter("@SubElement", SqlDbType.Int, 0);
				command.AddParameter("@topN", SqlDbType.Int, 0);

				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("CONTAINER1", reader.GetString(0));
					reader.Read();
					AssertEquals("CONTAINER2", reader.GetString(0));
				}
			}

			using (var command = Db.Connection.Command(functionSql))
			{
				command.AddParameter("@Text", SqlDbType.VarChar, "EQD+CN+CONTAINER1'EQD+CN+CONTAINER2");
				command.AddParameter("@SegmentPattern", SqlDbType.VarChar, "EQD");
				command.AddParameter("@Element", SqlDbType.Int, 1);
				command.AddParameter("@SubElement", SqlDbType.Int, 0);
				command.AddParameter("@topN", SqlDbType.Int, 1);

				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("CONTAINER1", reader.GetString(0));
					Assert("only one record", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(functionSql))
			{
				command.AddParameter("@Text", SqlDbType.VarChar, "");
				command.AddParameter("@SegmentPattern", SqlDbType.VarChar, "EQD");
				command.AddParameter("@Element", SqlDbType.Int, 1);
				command.AddParameter("@SubElement", SqlDbType.Int, 0);
				command.AddParameter("@topN", SqlDbType.Int, 1);

				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("", reader.GetString(0));
					Assert("only one record", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(functionSql))
			{
				command.AddParameter("@Text", SqlDbType.VarChar, "BGM+:::174+10207000001568");
				command.AddParameter("@SegmentPattern", SqlDbType.VarChar, "BGM");
				command.AddParameter("@Element", SqlDbType.Int, 0);
				command.AddParameter("@SubElement", SqlDbType.Int, 3);
				command.AddParameter("@topN", SqlDbType.Int, 1);

				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("174", reader.GetString(0));
					Assert("only one record", !reader.Read());
				}
			}
		}
	}
}

