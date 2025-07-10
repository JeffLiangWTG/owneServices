using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(RankingAddresses))]
	class RankingAddressesTest : EdwHashTest
	{
		public void TestSimpleCall()
		{
			const string insertSql = @"
DECLARE @OrgPk UNIQUEIDENTIFIER = '357C88A8-9CF6-4DE4-8136-5AF3FC55802C'
DECLARE @OrgAddress UNIQUEIDENTIFIER = 'F2A95FAF-D956-48C9-B05A-1BCAE6E7F45B'
DECLARE @pzPK UNIQUEIDENTIFIER = '60DD67C2-1B35-4DA8-9BFD-0DFA8EF81B3F'

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk, 'TESTORG')
INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1, OA_RL_NKRelatedPortCode, OA_Language) VALUES (@OrgAddress, @OrgPk, 'Address 1', 'GBLON', 'EN')
INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress) VALUES (@pzPK, @OrgAddress, 'ARM', 1)
";

			string querySql = $@"
SELECT Result FROM dbo.RankingAddresses('{TestDbHelper.DefaultCompanyPK}', '{TestDbHelper.BranchBrnPK}', '357C88A8-9CF6-4DE4-8136-5AF3FC55802C', 1, 'GB', 'EN', 1, 0, 0, 0)
";

			using (var command = TestConnection.Command(insertSql))
			{
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command(querySql))
			{
				var result = DataUtils.GetDataTableFromCommand(command);

				AssertEquals("F2A95FAF-D956-48C9-B05A-1BCAE6E7F45B", result.Rows[0][0].ToString().ToUpper());
			}
		}
		protected override string expectedMainDbFunctionHash => "85B214E71D612467C95FB56E2DE300B5B2D95B1FE603DAE1827114557066AB93";
		protected override string expectedEdwDbFunctionHash => "DC03C9B59FC1B3E3EF38CBEC16C64D3E574952F61F6B5399D7CD5294B9F860AA";

		protected override string edwScriptPath => "Function/Accounting/RankingAddresses.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new RankingAddresses();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
				return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ComplianceReport.RankingAddresses();
		}
	}
}

