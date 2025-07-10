using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.EServices.DeniedPartyScreening;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.EServices.DeniedPartyScreening.Testing
{
	[TestedType(typeof(GetWorstPartyScreeningStatusOfConsol))]
	class GetWorstPartyScreeningStatusOfConsolTest : DbCreateScriptTest
	{
		//Further tested in ScreeningUpdaterTest.

		public void TestExecuteFunctionCall()
		{
			var query = "SELECT * FROM GetWorstPartyScreeningStatusOfConsol(@jobPKs, @companyPK)";
			var pkTable = new DataTable();
			pkTable.Locale = CultureInfo.InvariantCulture;
			pkTable.Columns.Add("Value", typeof(Guid));
			pkTable.Rows.Add(Guid.NewGuid());

			using (DbCommand cmd = Db.Connection.Command(query))
			{
				AssertExceptionThrown(typeof(SqlException), "Must declare the scalar variable \"@jobPKs\".", () => cmd.ExecuteNonQuery());

				cmd.AddTableValuedParameter("@jobPKs", "dbo.TVP_uniqueidentifier", pkTable);

				AssertExceptionThrown(typeof(SqlException), "Must declare the scalar variable \"@companyPK\".", () => cmd.ExecuteNonQuery());
			}

			using (var cmd = Db.Connection.Command(query))
			{
				var results = new List<(Guid, string)>();

				AssertNoExceptionThrown(() =>
				{
					cmd.AddTableValuedParameter("@jobPKs", "dbo.TVP_uniqueidentifier", pkTable);
					cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							results.Add((new Guid((string)reader[0]), reader[1].ToString()));
						}
					}
				});

				AssertEquals(0, results.Count);
			}
		}
	}
}

