using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Consol;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Consol
{
	[TestedType(typeof(csfn_JobConsolWithDirectionCompanyBased))]
	class csfn_JobConsolWithDirectionCompanyBasedTest : DbCreateScriptTest
	{
		public void TestJobConsolWithDirectionWhenOriginOrDestinationFromCommunityRegions()
		{
			#region Prepare Test Data

			var companyPK = Guid.NewGuid();
			var regionPK1 = Guid.NewGuid();
			var regionPK2 = Guid.NewGuid();
			var regionGuids = regionPK1.ToString() + "," + regionPK2.ToString();

			var consolPK1 = Guid.NewGuid();
			var consolPK2 = Guid.NewGuid();
			var consolPK3 = Guid.NewGuid();
			var consolPK4 = Guid.NewGuid();
			var consolPK5 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertGlbCompanyCommand(companyPK));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPK1, "A1"));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPK2, "B1"));
			TestConnection.ExecuteNonQuery(GetInsertStmDataCommunityRegionsForDirectionCalculationCommand(Guid.NewGuid(), companyPK, regionGuids));

			TestConnection.ExecuteNonQuery(GetInsertJobConsolCommand(consolPK1, "C00005001", "A1AAA", "AUSYD"));
			TestConnection.ExecuteNonQuery(GetInsertJobConsolCommand(consolPK2, "C00005002", "AUSYD", "B1BBB"));
			TestConnection.ExecuteNonQuery(GetInsertJobConsolCommand(consolPK3, "C00005003", "GBLON", "A1AAA"));
			TestConnection.ExecuteNonQuery(GetInsertJobConsolCommand(consolPK4, "C00005004", "B1BBB", "GBLON"));
			TestConnection.ExecuteNonQuery(GetInsertJobConsolCommand(consolPK5, "C00005005", "A1AAA", "B1BBB"));

			#endregion

			const string directionColumnName = "JK_Direction";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetJobConsolWithDirectionCompanyBasedCommand(companyPK, "GB"));

			AssertEquals("Result should have five rows", 5, result.Rows.Count);
			AssertEquals("EXP", result.Rows[0][directionColumnName]);
			AssertEquals("IMP", result.Rows[1][directionColumnName]);
			AssertEquals("EXP", result.Rows[2][directionColumnName]);
			AssertEquals("IMP", result.Rows[3][directionColumnName]);
			AssertEquals("OTH", result.Rows[4][directionColumnName]);
		}

		#region Implementation

		string GetInsertGlbCompanyCommand(Guid pk)
		{
			return string.Format(
				@"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('{0}', 'DAN', 'AU company', 'AU', 'AUD')", pk);
		}

		string GetInsertRefCountryCommand(Guid pk, string countryCode)
		{
			return string.Format(
				@"INSERT INTO dbo.RefCountry (RN_PK, RN_Code) VALUES ('{0}', '{1}')",
				pk, countryCode);
		}

		string GetInsertStmDataCommunityRegionsForDirectionCalculationCommand(Guid pk, Guid owner, string guidList)
		{
			return string.Format(@"
DECLARE @RefCountryLCPKStr AS NVARCHAR(MAX) = '{0}'
INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue, SD_Owner) VALUES ('{1}', 'CommunityRegionsForDirectionCalculation', CONVERT(VARBINARY(MAX), @RefCountryLCPKStr), '{2}')",
				guidList, pk, owner);
		}

		string GetInsertJobConsolCommand(Guid pk, string consolRef, string loadPort, string dischargePort)
		{
			return string.Format(
				@"INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_RL_NKLoadPort, JK_RL_NKDischargePort) VALUES ('{0}', '{1}', '{2}', '{3}')",
				pk, consolRef, loadPort, dischargePort);
		}

		string GetJobConsolWithDirectionCompanyBasedCommand(Guid companyPk, string currentCountryCode)
		{
			return string.Format(@"SELECT * FROM csfn_JobConsolWithDirectionCompanyBased('{0}', '{1}') ORDER BY JK_UniqueConsignRef",
				currentCountryCode, companyPk);
		}

		#endregion
	}
}
