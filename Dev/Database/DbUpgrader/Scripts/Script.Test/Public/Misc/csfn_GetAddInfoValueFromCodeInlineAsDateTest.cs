using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(csfn_GetAddInfoValueFromCodeInlineAsDate))]
	class csfn_GetAddInfoValueFromCodeInlineAsDateTest : DbCreateScriptTest
	{
		public void Testcsfn_GetAddInfoValueFromCodeInlineAsDate()
		{
			var insertSql = @"
IF object_id('tempdb..#TemTable') IS NOT NULL BEGIN DROP TABLE #TemTable END;
CREATE TABLE #TemTable (AddInfo varchar(100));
Insert into #TemTable values('AddInfo1=10*AddInfo2=2020-07-09 07:40:00.000*AddInfo3=abc')";
			using (var command = Db.Connection.Command(insertSql))
			{
				command.ExecuteNonQuery();
			}

			var selectSql = @"select * from #TemTable
cross apply csfn_GetAddInfoValueFromCodeInlineAsDate(AddInfo,'AddInfo2')";
			using (var command = Db.Connection.Command(selectSql))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals(new DateTime(2020, 07, 09, 00, 00, 0, 0), (DateTime)reader["ValueAsDate"]);
						AssertEquals("*AddInfo1=10*AddInfo2=2020-07-09 07:40:00.000*AddInfo3=abc*", (string)reader["Info"]);
						AssertEquals("*AddInfo2=", (string)reader["SearchText"]);
						AssertEquals(13, (int)reader["StartIndex"]);
						AssertEquals(23, (int)reader["NewIndex"]);
						AssertEquals("2020-07-09 07:40:00.000", (string)reader["Value"]);
					}
				}
			}
		}
	}
}
