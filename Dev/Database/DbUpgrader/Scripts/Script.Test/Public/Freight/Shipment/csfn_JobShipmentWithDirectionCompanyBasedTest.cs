using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Shipment
{
	[TestedType(typeof(csfn_JobShipmentWithDirectionCompanyBased))]
	class csfn_JobShipmentWithDirectionCompanyBasedTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestJobShipmentWithDirectionWhenOriginOrDestinationFromCommunityRegions()
		{
			#region Prepare Test Data

			var companyPk = Guid.NewGuid();
			var regionPk1 = Guid.NewGuid();
			var regionPk2 = Guid.NewGuid();
			var regionGuids = regionPk1.ToString() + "," + regionPk2.ToString();
			var shipmentPk1 = Guid.NewGuid();
			var shipmentPk2 = Guid.NewGuid();
			var shipmentPk3 = Guid.NewGuid();
			var shipmentPk4 = Guid.NewGuid();
			var shipmentPk5 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertGlbCompanyCommand(companyPk));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk1, "A1"));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk2, "B1"));
			TestConnection.ExecuteNonQuery(GetInsertStmDataCommunityRegionsForDirectionCalculationCommand(Guid.NewGuid(), companyPk, regionGuids));
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk1, "S00001001", "A1AAA", "AUSYD"));
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk2, "S00001002", "AUSYD", "B1BBB"));
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk3, "S00001003", "GBLON", "A1AAA"));
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk4, "S00001004", "B1BBB", "GBLON"));
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk5, "S00001005", "A1AAA", "B1BBB"));

			#endregion

			const string directionColumnName = "JS_Direction";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetJobShipmentWithDirectionCompanyBasedCommand(companyPk, "GB"));

			AssertEquals("Result should have five rows", 5, result.Rows.Count);
			AssertEquals("EXP", result.Rows[0][directionColumnName]);
			AssertEquals("IMP", result.Rows[1][directionColumnName]);
			AssertEquals("EXP", result.Rows[2][directionColumnName]);
			AssertEquals("IMP", result.Rows[3][directionColumnName]);
			AssertEquals("OTH", result.Rows[4][directionColumnName]);
		}

		[ExpectNoExceptions]
		public void TestJobShipmentDirection_LoggedInCountryIsWithinRegion()
		{
			#region Prepare Test Data

			var companyPk = Guid.NewGuid();
			var regionPk1 = Guid.NewGuid();
			var regionPk2 = Guid.NewGuid();
			var regionPk3 = Guid.NewGuid();
			var regionGuids = regionPk1.ToString() + "," + regionPk2.ToString() + "," + regionPk3.ToString();
			var shipmentPk1 = Guid.NewGuid();
			var shipmentPk2 = Guid.NewGuid();
			var shipmentPk3 = Guid.NewGuid();
			var shipmentPk4 = Guid.NewGuid();
			var shipmentPk5 = Guid.NewGuid();
			var shipmentPk6 = Guid.NewGuid();
			var shipmentPk7 = Guid.NewGuid();
			var shipmentPk8 = Guid.NewGuid();
			var shipmentPk9 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertGlbCompanyCommand(companyPk));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk1, "XX"));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk2, "YY"));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk3, "ZZ"));
			TestConnection.ExecuteNonQuery(GetInsertStmDataCommunityRegionsForDirectionCalculationCommand(Guid.NewGuid(), companyPk, regionGuids));
			//Export
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk1, "S00001001", "XXCOM", "HK123")); // org: login -> dest: not login, outside
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk2, "S00001002", "XXCOM", "YY111")); // org: login -> dest: not login, within
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk3, "S00001003", "ZZ111", "HK123")); // org: not login, within -> dest: not login, outside
																													 //Import
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk4, "S00001004", "YY111", "XXCOM")); // org: not login, within -> dest: login
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk5, "S00001005", "HK123", "ZZ111")); // org: not login, outside -> dest: not login, within
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk6, "S00001006", "HK123", "XXCOM")); // org: not login, outside -> dest: login
																													 //Other
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk7, "S00001007", "YY111", "ZZ111")); // cross-trade
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk8, "S00001008", "CN111", "GB111")); // cross-trade
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk9, "S00001009", "XXCOM", "XX111")); // domestic

			#endregion

			const string directionColumnName = "JS_Direction";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetJobShipmentWithDirectionCompanyBasedCommand(companyPk, "XX"));

			AssertEquals("Result should have nine rows", 9, result.Rows.Count);
			AssertEquals("EXP", result.Rows[0][directionColumnName]);
			AssertEquals("EXP", result.Rows[1][directionColumnName]);
			AssertEquals("EXP", result.Rows[2][directionColumnName]);
			AssertEquals("IMP", result.Rows[3][directionColumnName]);
			AssertEquals("IMP", result.Rows[4][directionColumnName]);
			AssertEquals("IMP", result.Rows[5][directionColumnName]);
			AssertEquals("OTH", result.Rows[6][directionColumnName]);
			AssertEquals("OTH", result.Rows[7][directionColumnName]);
			AssertEquals("OTH", result.Rows[8][directionColumnName]);
		}

		[ExpectNoExceptions]
		public void TestJobShipmentDirection_LoggedInCountryIsOutsideRegion()
		{
			#region Prepare Test Data

			var companyPk = Guid.NewGuid();
			var regionPk1 = Guid.NewGuid();
			var regionPk2 = Guid.NewGuid();
			var regionPk3 = Guid.NewGuid();
			var regionGuids = regionPk1.ToString() + "," + regionPk2.ToString() + "," + regionPk3.ToString();
			var shipmentPk1 = Guid.NewGuid();
			var shipmentPk2 = Guid.NewGuid();
			var shipmentPk3 = Guid.NewGuid();
			var shipmentPk4 = Guid.NewGuid();
			var shipmentPk5 = Guid.NewGuid();
			var shipmentPk6 = Guid.NewGuid();
			var shipmentPk7 = Guid.NewGuid();
			var shipmentPk8 = Guid.NewGuid();
			var shipmentPk9 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertGlbCompanyCommand(companyPk));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk1, "II"));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk2, "JJ"));
			TestConnection.ExecuteNonQuery(GetInsertStmDataCommunityRegionsForDirectionCalculationCommand(Guid.NewGuid(), companyPk, regionGuids));
			//Export
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk1, "S00001001", "AACOM", "HK123")); // org: login -> dest: not login, outside
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk2, "S00001002", "AACOM", "II111")); // org: login -> dest: not login, within
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk3, "S00001003", "II111", "HK123")); // org: not login, within -> dest: not login, outside
																													 //Import
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk4, "S00001004", "JJ111", "AACOM")); // org: not login, within -> dest: login
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk5, "S00001005", "HK123", "II111")); // org: not login, outside -> dest: not login, within
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk6, "S00001006", "HK123", "AACOM")); // org: not login, outside -> dest: login
																													 //Other
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk7, "S00001007", "II111", "JJ111")); // other/cross-trade
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk8, "S00001008", "CN111", "GB111")); // other/cross-trade
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk9, "S00001009", "AACOM", "AA111")); // other/domestic

			#endregion

			const string directionColumnName = "JS_Direction";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetJobShipmentWithDirectionCompanyBasedCommand(companyPk, "AA"));

			AssertEquals("Result should have nine rows", 9, result.Rows.Count);
			AssertEquals("EXP", result.Rows[0][directionColumnName]);
			AssertEquals("EXP", result.Rows[1][directionColumnName]);
			AssertEquals("EXP", result.Rows[2][directionColumnName]);
			AssertEquals("IMP", result.Rows[3][directionColumnName]);
			AssertEquals("IMP", result.Rows[4][directionColumnName]);
			AssertEquals("IMP", result.Rows[5][directionColumnName]);
			AssertEquals("OTH", result.Rows[6][directionColumnName]);
			AssertEquals("OTH", result.Rows[7][directionColumnName]);
			AssertEquals("OTH", result.Rows[8][directionColumnName]);
		}

		[ExpectNoExceptions]
		public void TestJobShipmentWithDirectionWhenOriginOrDestinationFromCommunityRegions_IgnoreDuplicateCountry()
		{
			#region Prepare Test Data

			var companyPk = Guid.NewGuid();
			var regionPk = Guid.NewGuid();
			var regionGuids = regionPk.ToString() + "," + regionPk.ToString();
			var shipmentPk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(GetInsertGlbCompanyCommand(companyPk));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk, "A1"));
			TestConnection.ExecuteNonQuery(GetInsertStmDataCommunityRegionsForDirectionCalculationCommand(Guid.NewGuid(), companyPk, regionGuids));
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk, "S00001001", "A1AAA", "AUSYD"));

			#endregion

			const string directionColumnName = "JS_Direction";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetJobShipmentWithDirectionCompanyBasedCommand(companyPk, "GB"));

			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("EXP", result.Rows[0][directionColumnName]);
		}

		string GetInsertRefCountryCommand(Guid pk, string countryCode)
		{
			return string.Format(
				@"INSERT INTO dbo.RefCountry (RN_PK, RN_Code) VALUES ('{0}', '{1}')",
				pk, countryCode);
		}

		string GetInsertJobShipmentCommand(Guid pk, string shipmentRef, string origin, string destination)
		{
			return string.Format(
				@"INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{0}', '{1}', '{2}', '{3}')",
				pk, shipmentRef, origin, destination);
		}

		string GetInsertGlbCompanyCommand(Guid pk)
		{
			return string.Format(
				@"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('{0}', 'DAN', 'AU company','AU', 'AUD')", pk);
		}

		string GetInsertStmDataCommunityRegionsForDirectionCalculationCommand(Guid pk, Guid owner, string guidList)
		{
			return string.Format(
				@"
DECLARE @RefCountryLCPKStr AS NVARCHAR(MAX) = '{0}'
INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue, SD_Owner) VALUES ('{1}', 'CommunityRegionsForDirectionCalculation', CONVERT(VARBINARY(MAX), @RefCountryLCPKStr), '{2}')",
				guidList, pk, owner);
		}

		string GetJobShipmentWithDirectionCompanyBasedCommand(Guid companyPk, string currentCountryCode)
		{
			return string.Format(@"SELECT * FROM csfn_JobShipmentWithDirectionCompanyBased('{0}', '{1}') ORDER BY JS_UniqueConsignRef",
				currentCountryCode, companyPk);
		}
	}
}
