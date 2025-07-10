using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.MasterFiles.Testing
{
	[TestedType(typeof(csfn_LocoReportingZones))]
	class csfn_LocoReportingZonesTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void Testcsfn_LocoReportingZonesReport()
		{
			PrepareTestData();

			var sqlCommand = string.Format(CultureInfo.InvariantCulture, @"SELECT ZoneCode FROM {0}.dbo.csfn_LocoReportingZones(NULL) WHERE LocoCode = 'AUSYD'", ScriptDbName);
			AssertEquals("AUSR", TestConnection.ExecuteScalar(sqlCommand));
			sqlCommand = string.Format(CultureInfo.InvariantCulture, @"SELECT ZoneCode FROM {0}.dbo.csfn_LocoReportingZones('B97AEF8B-DBD4-4B20-B07F-13202F73436B')", ScriptDbName);
			AssertEquals("AUSR", TestConnection.ExecuteScalar(sqlCommand));
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT INTO {0}.Organization.BAS__RefUNLOCO
					(Code, CountryCode, PortName, RefUNLOCOID, RefUNLOCOKey)
				VALUES
					('AUSYD', 'AU', 'Sydney', 'AD87872E-96B8-4C9A-BC0B-6A4088247A64', 45266)

				INSERT INTO {0}.Organization.BAS__RefZonePivot
					(ParentID, RefZoneHeaderID, RefZonePivotID, RefZonePivotKey)
				VALUES
					('AD87872E-96B8-4C9A-BC0B-6A4088247A64', 'B97AEF8B-DBD4-4B20-B07F-13202F73436B', '9681E49B-91C2-4865-A9C1-48A635898229', 4360)


				INSERT INTO {0}.Organization.BAS__RefZoneHeader
					(Code, Description, RefZoneHeaderID, RefZoneHeaderKey, RelatedParty, ZoneType)
				VALUES
					('AUSR', 'Australia', 'B97AEF8B-DBD4-4B20-B07F-13202F73436B', 4, NULL, 'RPT')


				INSERT INTO {0}.Geography.BAS__Country
					(Code, CountryID, CountryKey, Description)
				VALUES
					('AU', 'E4EB6E97-AA78-4C46-BF86-35B7FBB5FB0E', 54, 'Australia')
				", ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}

