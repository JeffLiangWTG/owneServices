using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(GetRootMNGNames))]
	class GetRootMNGNamesTest : EdwHashTest
	{
		public void TestSampleCall()
		{
			PrepareTestData();

			var result = GetResultSet(Array.Empty<Guid>());
			AssertEquals("Result should have one row", 3, result.Rows.Count);

			result = GetResultSet(new Guid[] { org1 });
			AssertEquals("Result should have two row", 2, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder(new[] { org1_1, org1_1_1 }, result.Rows.Cast<DataRow>().Select(x => x["OrgPK"]));
			AssertContainsExactElementsInAnyOrder(new[] { "MNG 1", "MNG 1" }, result.Rows.Cast<DataRow>().Select(x => x["MNGName"]));
			AssertContainsExactElementsInAnyOrder(new[] { "MNG 1.1", "MNG 1" }, result.Rows.Cast<DataRow>().Select(x => x["RelatedMNGName"]));

			result = GetResultSet(new Guid[] { org2 });
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder(new[] { org2_1 }, result.Rows.Cast<DataRow>().Select(x => x["OrgPK"]));
			AssertContainsExactElementsInAnyOrder(new[] { "MNG 2" }, result.Rows.Cast<DataRow>().Select(x => x["MNGName"]));
			AssertContainsExactElementsInAnyOrder(new[] { "MNG 2" }, result.Rows.Cast<DataRow>().Select(x => x["RelatedMNGName"]));

			result = GetResultSet(new Guid[] { org3 });
			AssertEquals("Result should have no row", 0, result.Rows.Count);
		}

		void PrepareTestData()
		{
			InitializeGlbDatatPK();

			org1 = TestDataCreator.CreateOrganisation("O1", "MNG 1");
			org2 = TestDataCreator.CreateOrganisation("O2", "MNG 2");
			org3 = TestDataCreator.CreateOrganisation("O3", "Org 3");

			org1_1 = TestDataCreator.CreateOrganisation("O11", "MNG 1.1");
			org1_2 = TestDataCreator.CreateOrganisation("O12", "MNG 1.2");
			org2_1 = TestDataCreator.CreateOrganisation("O21", "MNG 2.1");

			org1_1_1 = TestDataCreator.CreateOrganisation("O111", "Child 1.1.1");
			org1_1_2 = TestDataCreator.CreateOrganisation("O112", "Child 1.1.2");

			CreateOrgRelatedParty(org1_1_1, org1_1, "MNG", null);
			CreateOrgRelatedParty(org1_1_2, org1_1, "MNG", GlbCompanyPK);
			CreateOrgRelatedParty(org1_1, org1, "MNG", null);
			CreateOrgRelatedParty(org1_2, org1, "MNG", GlbCompanyPK);

			CreateOrgRelatedParty(org2_1, org2, "MNG", null);
		}
		void InitializeGlbDatatPK()
		{
			string sql = @"
			SELECT
				TOP(1) GC_PK
			FROM 
				dbo.GlbCompany
				JOIN dbo.RefCountry ON GC_RN_NKCountryCode != '' AND RN_Code = GC_RN_NKCountryCode ";

			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					GlbCompanyPK = Guid.Parse(reader["GC_PK"].ToString());
				}
			}
		}

		DataTable GetResultSet(Guid[] values)
		{
			var sql = string.Format(@" SELECT * FROM GetRootMNGNames(@ultimateMNGPKs, @MNGListIsEmpty)");
			var command = TestConnection.Command(sql);
			AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@ultimateMNGPKs", "@MNGListIsEmpty", values);
			return DataUtils.GetDataTableFromCommand(command);
		}

		void CreateOrgRelatedParty(Guid parent, Guid relatedParty, string partyType, Guid? company)
		{
			var sql = @"INSERT INTO dbo.OrgRelatedParty (PR_PK, PR_OH_Parent, PR_OH_RelatedParty, PR_PartyType, PR_GC, PR_SystemCreateTimeUtc, PR_SystemCreateUser, PR_SystemLastEditTimeUtc, PR_SystemLastEditUser) VALUES (NEWID(), @parent, @relatedParty, @partyType, @company, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@parent", SqlDbType.UniqueIdentifier, parent);
				command.AddParameter("@relatedParty", SqlDbType.UniqueIdentifier, relatedParty);
				command.AddParameter("@partyType", SqlDbType.VarChar, partyType);
				command.AddParameter("@company", SqlDbType.UniqueIdentifier, (object)company ?? DBNull.Value);

				command.ExecuteNonQuery();
			}
		}

		void AddTVP_uniqueidentifierAndIsEmptyParameters(DbCommand command, string paramName, string isEmptyParamName, Guid[] values)
		{
			var table = new DataTable();
			table.Columns.Add("Value", typeof(Guid)); // Part of SQL code

			if (values != null)
			{
				foreach (var value in values)
				{
					table.Rows.Add(value);
				}
			}

			command.AddTableValuedParameter(paramName, "dbo.TVP_uniqueidentifier", table);
			command.AddParameter(isEmptyParamName, SqlDbType.Bit, table.Rows.Count == 0);
		}

		Guid GlbCompanyPK;

		Guid org1;
		Guid org1_1;
		Guid org1_2;
		Guid org1_1_1;
		Guid org1_1_2;

		Guid org2;
		Guid org2_1;
		Guid org3;

		protected override string expectedMainDbFunctionHash => "ACD33BD82A8C5729F4C330365985A1D39E93D2A8172B5AA096DF813F8A4A92E6";
		protected override string expectedEdwDbFunctionHash => "9B60B41710E82E494073BCAACA1A12F5916A4BA2D255C5B3BC77514AD6389DA4";

		protected override string edwScriptPath => "ReportFunctions/Accounting/GetRootMNGNames.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new GetRootMNGNames();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.GetRootMNGNames();
		}
	}
}

