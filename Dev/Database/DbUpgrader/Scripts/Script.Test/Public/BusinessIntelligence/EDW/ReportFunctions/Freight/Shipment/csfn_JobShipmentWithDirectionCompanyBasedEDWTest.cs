using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment.Testing
{
	[TestedType(typeof(csfn_JobShipmentWithDirectionCompanyBased))]
	class csfn_JobShipmentWithDirectionCompanyBasedEDWTest : BiCreateScriptTest
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

			TestConnection.ExecuteNonQuery(GetInsertGlbCompanyCommand(companyPk, 1));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk1, "A1", 1));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk2, "B1", 2));
			TestConnection.ExecuteNonQuery(GetInsertStmDataCommunityRegionsForDirectionCalculationCommand(Guid.NewGuid(), companyPk, regionGuids, 1));
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk1, "S00001001", "A1AAA", "AUSYD", 1));
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk2, "S00001002", "AUSYD", "B1BBB", 2));
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk3, "S00001003", "GBLON", "A1AAA", 3));
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk4, "S00001004", "B1BBB", "GBLON", 4));
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk5, "S00001005", "A1AAA", "B1BBB", 5));

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

			TestConnection.ExecuteNonQuery(GetInsertGlbCompanyCommand(companyPk, 1));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk1, "XX", 1));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk2, "YY", 2));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk3, "ZZ", 3));
			TestConnection.ExecuteNonQuery(GetInsertStmDataCommunityRegionsForDirectionCalculationCommand(Guid.NewGuid(), companyPk, regionGuids, 1));
																													  //Export
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk1, "S00001001", "XXCOM", "HK123", 1)); // org: login -> dest: not login, outside
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk2, "S00001002", "XXCOM", "YY111", 2)); // org: login -> dest: not login, within
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk3, "S00001003", "ZZ111", "HK123", 3)); // org: not login, within -> dest: not login, outside
																													  //Import
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk4, "S00001004", "YY111", "XXCOM", 4)); // org: not login, within -> dest: login
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk5, "S00001005", "HK123", "ZZ111", 5)); // org: not login, outside -> dest: not login, within
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk6, "S00001006", "HK123", "XXCOM", 6)); // org: not login, outside -> dest: login
																													  //Other
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk7, "S00001007", "YY111", "ZZ111", 7)); // cross-trade
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk8, "S00001008", "CN111", "GB111", 8)); // cross-trade
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk9, "S00001009", "XXCOM", "XX111", 9)); // domestic

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

			TestConnection.ExecuteNonQuery(GetInsertGlbCompanyCommand(companyPk, 1));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk1, "II", 1));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk2, "JJ", 2));
			TestConnection.ExecuteNonQuery(GetInsertStmDataCommunityRegionsForDirectionCalculationCommand(Guid.NewGuid(), companyPk, regionGuids, 1));
																													  //Export
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk1, "S00001001", "AACOM", "HK123", 1)); // org: login -> dest: not login, outside
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk2, "S00001002", "AACOM", "II111", 2)); // org: login -> dest: not login, within
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk3, "S00001003", "II111", "HK123", 3)); // org: not login, within -> dest: not login, outside
																													  //Import
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk4, "S00001004", "JJ111", "AACOM", 4)); // org: not login, within -> dest: login
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk5, "S00001005", "HK123", "II111", 5)); // org: not login, outside -> dest: not login, within
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk6, "S00001006", "HK123", "AACOM", 6)); // org: not login, outside -> dest: login
																													  //Other
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk7, "S00001007", "II111", "JJ111", 7)); // other/cross-trade
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk8, "S00001008", "CN111", "GB111", 8)); // other/cross-trade
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk9, "S00001009", "AACOM", "AA111", 9)); // other/domestic

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

			TestConnection.ExecuteNonQuery(GetInsertGlbCompanyCommand(companyPk, 1));
			TestConnection.ExecuteNonQuery(GetInsertRefCountryCommand(regionPk, "A1", 1));
			TestConnection.ExecuteNonQuery(GetInsertStmDataCommunityRegionsForDirectionCalculationCommand(Guid.NewGuid(), companyPk, regionGuids, 1));
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(shipmentPk, "S00001001", "A1AAA", "AUSYD", 1));

			#endregion

			const string directionColumnName = "JS_Direction";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, GetJobShipmentWithDirectionCompanyBasedCommand(companyPk, "GB"));

			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("EXP", result.Rows[0][directionColumnName]);
		}

		string GetInsertRefCountryCommand(Guid pk, string countryCode, int countryKey)
		{
			return string.Format(
				@"INSERT INTO [{0}].[Geography].[BAS__Country] (CountryID, Code, CountryKey) VALUES ('{1}', '{2}', {3})",
				ScriptDbName, pk, countryCode, countryKey);
		}

		string GetInsertJobShipmentCommand(Guid pk, string shipmentRef, string origin, string destination, int shipmentKey)
		{
			return string.Format(
				@"INSERT INTO [{0}].[InternationalLogistics].[BAS__Shipment] (ShipmentID, JobNumber, PortOfOrigin, PortOfDestination, ShipmentKey) VALUES ('{1}', '{2}', '{3}', '{4}', {5})",
				ScriptDbName, pk, shipmentRef, origin, destination, shipmentKey);
		}

		string GetInsertGlbCompanyCommand(Guid pk, int companyKey)
		{
			return string.Format(
				@"INSERT INTO [{0}].[Organization].[BAS__Company] (CompanyID, CountryCode, LocalCurrency, CompanyKey) VALUES ('{1}', 'AU', 'AUD', {2})", ScriptDbName, pk, companyKey);
		}

		string GetInsertStmDataCommunityRegionsForDirectionCalculationCommand(Guid pk, Guid owner, string guidList, int shipmentStmDataKey)
		{
			return string.Format(
				@"
DECLARE @RefCountryLCPKStr AS NVARCHAR(MAX) = '{0}'
INSERT INTO [{1}].[InternationalLogistics].[BAS__ShipmentStmData] (ShipmentStmDataID, Name, Value, Owner, ShipmentStmDataKey) VALUES ('{2}', 'CommunityRegionsForDirectionCalculation', CONVERT(VARBINARY(MAX), @RefCountryLCPKStr), '{3}', {4})",
				guidList, ScriptDbName, pk, owner, shipmentStmDataKey);
		}

		string GetJobShipmentWithDirectionCompanyBasedCommand(Guid companyPk, string currentCountryCode)
		{
			return string.Format(@"SELECT * FROM [{0}].[dbo].[csfn_JobShipmentWithDirectionCompanyBased]('{1}', '{2}') ORDER BY JS_UniqueConsignRef",
				ScriptDbName, currentCountryCode, companyPk);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
