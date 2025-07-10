using System;
using CargoWise.DbUpgrader.Scripts.Definitions.MarketingManager;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager.Testing
{
	[TestedType(typeof(vw_Report_SalesClientSummaryProfile))]
	class vw_Report_SalesClientSummaryProfilerTest : DbCreateScriptTest
	{
		public void TestCompetitors()
		{
			var orgPK = Guid.NewGuid();
			var companyPK1 = Guid.NewGuid();
			var companyPK2 = Guid.NewGuid();
			var competitorPK1 = Guid.NewGuid();
			var competitorPK2 = Guid.NewGuid();
			var insertSql = $@"
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency) VALUES
	('{companyPK1}', 'CM1', 'AU company1', 'AUD'),
	('{companyPK2}', 'CM2', 'AU company2', 'AUD')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_IsSalesLead) VALUES
	('{orgPK}', 'TESTORG', 1),
	('{competitorPK1}', 'TESCOMP1', 0),
	('{competitorPK2}', 'TESCOMP2', 0)

INSERT INTO dbo.OrgCompetitor (OCP_PK, OCP_OH_Parent, OCP_Type, OCP_OH_Competitor, OCP_SystemCreateTimeUtc, OCP_SystemCreateUser, OCP_SystemLastEditTimeUtc, OCP_SystemLastEditUser, OCP_GC_Company) VALUES
	(NEWID(), '{orgPK}', 'CMB', '{competitorPK1}', GetUtcDate(), 'E', GetUtcDate(), 'E', '{companyPK1}'),
	(NEWID(), '{orgPK}', 'CMB', '{competitorPK2}', GetUtcDate(), 'E', GetUtcDate(), 'E', NULL),
	(NEWID(), '{orgPK}', 'CMB', '{competitorPK2}', GetUtcDate(), 'E', GetUtcDate(), 'E', '{companyPK2}')";

			using (var command = TestConnection.Command(insertSql))
			{
				command.ExecuteNonQuery();
			}

			AssertCompetitor("Competitors for Company CM1", "CMB: TESCOMP1" + "\n" + "CMB: TESCOMP2", orgPK, companyPK1);
			AssertCompetitor("Competitors for Company CM2", "CMB: TESCOMP2", orgPK, companyPK2);
		}

		public void AssertCompetitor(string message, string expected, Guid orgPK, Guid companyPK)
		{
			var selectSql = $"SELECT * FROM dbo.vw_Report_SalesClientSummaryProfile WHERE CompanyPK='{companyPK}' AND OrganisationPK='{orgPK}'";
			using (var command = TestConnection.Command(selectSql))
			using (var reader = command.ExecuteReader())
			{
				Assert(reader.Read());
				AssertEquals(message, expected, reader["Competitors"]);
			}
		}
	}
}

