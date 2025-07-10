using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.EServices.DeniedPartyScreening;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.EServices.DeniedPartyScreening.Testing
{
	[TestedType(typeof(GetWorstPartyScreeningStatusOfConsolNew))]
	class GetWorstPartyScreeningStatusOfConsolNewTest : DbCreateScriptTest
	{
		public void TestExecuteFunctionWithResult()
		{
			var helper = new TestDbHelperBase(TestConnection);
			var shipmentPK = Guid.NewGuid();
			var consolPK = Guid.NewGuid();
			var jobConShipLinkPK = Guid.NewGuid();
			helper.Insert(JobConsolSchema.Constants.TableName, new
			{
				JK_PK = consolPK,
				JK_UniqueConsignRef = "C0004197",
				JK_RL_NKLoadPort = "AUSDY",
				JK_RL_NKDischargePort = "NZAKL"
			});

			helper.Insert(JobShipmentSchema.Constants.TableName, new
			{
				JS_PK = shipmentPK,
				JS_ScreeningStatus = "MAT"
			});

			helper.Insert(JobConShipLinkSchema.Constants.TableName, new
			{
				JN_PK = jobConShipLinkPK,
				JN_JS = shipmentPK,
				JN_JK = consolPK
			});

			var query = "SELECT * FROM GetWorstPartyScreeningStatusOfConsolNew(@jobPKs, @companyPK)";
			var pkTable = new DataTable();
			pkTable.Locale = CultureInfo.InvariantCulture;
			pkTable.Columns.Add("Value", typeof(Guid));
			pkTable.Rows.Add(consolPK);
			AssertWorestStatus(consolPK, query, pkTable, "BLK");

			helper.RunSQL(new { JS_PK = shipmentPK }, @"
UPDATE dbo.JobShipment
SET
	JS_ScreeningStatus = 'CLR',
	JS_SystemLastEditTimeUtc = GETUTCDATE(),
	JS_SystemLastEditUser = '~BP'
WHERE
	JS_PK = @JS_PK");
			AssertWorestStatus(consolPK, query, pkTable, "REL");
		}

		void AssertWorestStatus(Guid consolPK, string query, DataTable pkTable, string status)
		{
			using (var cmd = TestConnection.Command(query))
			{
				var results = new List<(Guid, string)>();
				cmd.AddTableValuedParameter("@jobPKs", "dbo.TVP_uniqueidentifier", pkTable);
				cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						results.Add((new Guid(reader[0].ToString()), reader[1].ToString()));
					}
				}
				AssertEquals(1, results.Count);
				var worstScreeningStatus = results.FirstOrDefault();
				AssertEquals(consolPK, worstScreeningStatus.Item1);
				AssertEquals(status, worstScreeningStatus.Item2);
			}
		}
	}
}
