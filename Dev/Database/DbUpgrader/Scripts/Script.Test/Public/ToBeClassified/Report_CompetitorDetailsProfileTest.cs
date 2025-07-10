using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(Report_CompetitorDetailsProfile))]
	class Report_CompetitorDetailsProfileTest : DbCreateScriptTest
	{
		public void TestResultContainsAllCompetitors()
		{
			var currentCompanyPK = TestDbHelper.DefaultCompanyPK;
			var company1PK = CreateGlbCompany("CM1");
			var company2PK = CreateGlbCompany("CM2");

			var org1PK = CreateOrgHeader("TEST01", true);
			CreateOrgCompanyData(org1PK, currentCompanyPK);

			var org2PK = CreateOrgHeader("TEST02", true);
			CreateOrgCompanyData(org2PK, company1PK);
			CreateOrgCompanyData(org2PK, company2PK);

			var org3PK = CreateOrgHeader("TEST03", false);
			CreateOrgCompanyData(org3PK, company1PK);
			CreateOrgCompanyData(org3PK, company2PK);

			var org4PK = CreateOrgHeader("TEST04", true);
			CreateOrgCompanyData(org4PK, company1PK);
			CreateOrgCompanyData(org4PK, currentCompanyPK);

			_ = CreateOrgHeader("TEST05", false);

			_ = CreateOrgHeader("TEST06", true);

			var returnedOrgCodes = new List<string>();
			using (var command = Db.Connection.Command("SELECT OrgCode FROM Report_CompetitorDetailsProfile(@currentCompanyPK) WHERE OrgCode IN (select value from @allOrgCodes)"))
			{
				command.AddParameter("@currentCompanyPK", SqlDbType.UniqueIdentifier, currentCompanyPK);
				command.AddTableValuedParameter("@allOrgCodes", "dbo.TVP_nvarchar_12", new string[] { "TEST01", "TEST02", "TEST03", "TEST04", "TEST05", "TEST06" });
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						returnedOrgCodes.Add(reader.GetString(0));
					}
				}
			}

			AssertContainsExactElementsInAnyOrder(new[] { "TEST01", "TEST02", "TEST04", "TEST06" }, returnedOrgCodes);
		}

		Guid CreateOrgHeader(string code, bool isCompetitor)
		{
			var organisationPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.OrgHeader(OH_PK, OH_Code, OH_IsCompetitor)
VALUES (@organisationPK, @code, @isCompetitor)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@organisationPK", organisationPK, OrgHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@code", code, OrgHeaderSchema.OH_Code);
				command.AddParameterBasedOnDbColumn("@isCompetitor", isCompetitor, OrgHeaderSchema.OH_IsCompetitor);
				command.ExecuteNonQuery();
			}
			return organisationPK;
		}

		Guid CreateGlbCompany(string code)
		{
			var companyPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
VALUES (@companyPK, @code, 'AU company', 'AU', 'AUD')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@companyPK", companyPK, GlbCompanySchema.PK);
				command.AddParameterBasedOnDbColumn("@code", code, GlbCompanySchema.GC_Code);
				command.ExecuteNonQuery();
			}
			return companyPK;
		}

		void CreateOrgCompanyData(Guid orgPK, Guid companyPK)
		{
			var orgCompanyDataPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.OrgCompanyData(OB_PK, OB_OH, OB_GC)
VALUES (@orgCompanyDataPK, @orgPK, @companyPK)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@orgCompanyDataPK", orgCompanyDataPK, OrgCompanyDataSchema.PK);
				command.AddParameterBasedOnDbColumn("@orgPK", orgPK, OrgCompanyDataSchema.OB_OH);
				command.AddParameterBasedOnDbColumn("@companyPK", companyPK, OrgCompanyDataSchema.OB_GC);
				command.ExecuteNonQuery();
			}
		}
	}
}

