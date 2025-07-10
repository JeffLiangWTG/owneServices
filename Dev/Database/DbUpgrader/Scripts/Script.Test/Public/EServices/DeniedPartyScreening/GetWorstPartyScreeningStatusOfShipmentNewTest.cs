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
	[TestedType(typeof(GetWorstPartyScreeningStatusOfShipmentNew))]
	class GetWorstPartyScreeningStatusOfShipmentNewTest : DbCreateScriptTest
	{
		public void TestExecuteFunctionWithResult()
		{
			var helper = new TestDbHelperBase(TestConnection);
			var shipmentPK = Guid.NewGuid();
			var declarationPK = Guid.NewGuid();
			var branchPK = Guid.NewGuid();
			var companyPK = Guid.NewGuid();
			helper.Insert(GlbCompanySchema.Constants.TableName, new
			{
				GC_PK = companyPK,
				GC_Code = "AU1",
				GC_Name = "AU company",
				GC_RN_NKCountryCode = "AU",
				GC_RX_NKLocalCurrency = "AUD"
			});

			helper.Insert(GlbBranchSchema.Constants.TableName, new
			{
				GB_PK = branchPK,
				GB_GC = companyPK,
				GB_Code = "AU1"
			});

			helper.Insert(JobShipmentSchema.Constants.TableName, new
			{
				JS_PK = shipmentPK
			});

			helper.Insert(JobDeclarationSchema.Constants.TableName, new
			{
				JE_PK = declarationPK,
				JE_JS = shipmentPK,
				JE_OverrideFreightDefaults = 1,
				JE_ScreeningStatus = "MAT",
				JE_GB = branchPK,
				JE_GC = companyPK,
				JE_ClusterKey = 1,
				JE_DataModel = "AU"
			});

			var query = "SELECT * FROM GetWorstPartyScreeningStatusOfShipmentNew(@jobPKs, @companyPK)";
			var pkTable = new DataTable();
			pkTable.Locale = CultureInfo.InvariantCulture;
			pkTable.Columns.Add("Value", typeof(Guid));
			pkTable.Rows.Add(shipmentPK);
			AssertWorestStatus(shipmentPK, query, pkTable, "BLK");

			helper.RunSQL(new { JE_PK = declarationPK }, @"
UPDATE dbo.JobDeclaration
SET
	JE_ScreeningStatus = 'CLR',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = @JE_PK");
			AssertWorestStatus(shipmentPK, query, pkTable, "REL");
		}

		void AssertWorestStatus(Guid pk, string query, DataTable pkTable, string status)
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
				AssertEquals(pk, worstScreeningStatus.Item1);
				AssertEquals(status, worstScreeningStatus.Item2);
			}
		}
	}
}
