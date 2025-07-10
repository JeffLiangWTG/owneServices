using System;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	public class RefWRSZoneUpgradeTaskRunTest : TransactionedTestCase
	{
		// DAT must have had massive REF data and causes the test to fail intermittently with timeouts on DAT machines.
		[SnailTest, UseSnapshotProtection(skipTransaction: true)]
		public void TestRun()
		{
			// -----------------
			// PREPARE TEST DATA
			// -----------------

			PrepareLocationsTestData();
			PrepareExistingZonesTestData();

			// --------
			// RUN TASK
			// --------

			var testTask = new RefWRSZoneUpgradeTask(new RefWRSZoneHeaderDataFileForTest());
			testTask.Run();

			// --------------
			// CHECK RESULTS
			// --------------

			// Asserts
			AssertRefZoneHeaderChanges();
		}

		readonly Guid wrsZoneToAddPk = Guid.Parse("df501494-09d0-4138-8da7-f3f8e83c136a");
		readonly Guid wrsZoneToEditPk = Guid.Parse("26065b15-75a0-46c1-bbaf-8c2a0f982ccb");
		readonly Guid wrsZoneToDeletePk = Guid.Parse("05b0b830-e132-421b-89bd-f1f713de1c57");
		readonly Guid ratZoneDontTouchPk = Guid.Parse("77ea1399-c668-4176-a9b3-b0cc05cb20ad");
		readonly Guid wrsZoneWithCarrierPk = Guid.Parse("7315e68a-fb16-4297-8133-b2f3f04241d5");
		readonly Guid existingZoneWithSameCodePk = Guid.NewGuid();

		readonly Guid zone1UnlocoPk = Guid.Parse("ea1f14b4-4f6b-41cc-9705-9d6bf2c39dde");
		readonly Guid zone1CountryPk = Guid.Parse("30a79fe1-b438-4ec8-a40d-c3bab793bcde");
		readonly Guid zone2UnlocoPk = Guid.Parse("f65c3c46-8c15-4c64-b386-7ff4b6088f3f");
		readonly Guid zone2CountryPk = Guid.Parse("0b4016a8-fdd2-4a41-8f0f-5099005fad2a");

		readonly Guid zone2OldUnlocoPk = Guid.Parse("673c3c8f-a073-42ec-8cff-7c12bfbda2fc");
		readonly Guid zone2OldCountryPk = Guid.Parse("007b10cc-2fd3-4a13-b6dd-55d779269881");
		readonly Guid zone2OldPivotUnlocoPk = Guid.NewGuid();
		readonly Guid zone2OldPivotCountryPk = Guid.NewGuid();
		readonly Guid zoneToDeletePivotUnlocoPk = Guid.NewGuid();
		readonly Guid zoneToDeletePivotCountryPk = Guid.NewGuid();

		readonly Guid ratZoneUnlocoPk = Guid.Parse("38d0ea28-6a30-4053-a743-98643b1b012d");
		readonly Guid ratZoneCountryPk = Guid.Parse("7c826b0a-0d5b-4689-90b2-3a42b3553867");

		readonly Guid nonExistingUnlocoPk = Guid.Parse("1cbba75b-7a4e-455b-91d5-acf0b63a0fb7");
		readonly Guid nonExistingCountryPk = Guid.Parse("64bcb0ec-74fc-40a2-a0bb-2c6ff14b3a00");

		void PrepareLocationsTestData()
		{
			string getInsertTestUNLOCOQuery(Guid unlocoPk, string portCode, string portName) =>
					$@"INSERT {RefUNLOCOSchema.Constants.SqlSchemaName}.{RefUNLOCOSchema.Constants.TableName} ({RefUNLOCOSchema.Constants.PK}, {RefUNLOCOSchema.Constants.RL_Code}, {RefUNLOCOSchema.Constants.RL_PortName}, {RefUNLOCOSchema.Constants.RL_RN_NKCountryCode}, {RefUNLOCOSchema.Constants.RL_IsSystem}) 
					VALUES ('{unlocoPk}', '{portCode}', '{portName}', 'AU', 1)";

			string getInsertTestCountryQuery(Guid countryPk, string countryCode, string countryDesc, string countryDialingCode) =>
					$@"INSERT {RefCountrySchema.Constants.SqlSchemaName}.{RefCountrySchema.Constants.TableName} ({RefCountrySchema.Constants.PK}, {RefCountrySchema.Constants.RN_Code}, {RefCountrySchema.Constants.RN_IsActive}, {RefCountrySchema.Constants.RN_Desc}, {RefCountrySchema.Constants.RN_CountryDialingCode})
					VALUES ('{countryPk}', '{countryCode}', 1, '{countryDesc}', '{countryDialingCode}')";

			TestConnection.ExecuteNonQuery(getInsertTestUNLOCOQuery(zone1UnlocoPk, "UNLO1", "UNLOCO1"));
			TestConnection.ExecuteNonQuery(getInsertTestCountryQuery(zone1CountryPk, "C1", "COUNTRY1", "~1"));
			TestConnection.ExecuteNonQuery(getInsertTestUNLOCOQuery(zone2UnlocoPk, "UNLO2", "UNLOCO2"));
			TestConnection.ExecuteNonQuery(getInsertTestCountryQuery(zone2CountryPk, "C2", "COUNTRY2", "~2"));
			TestConnection.ExecuteNonQuery(getInsertTestUNLOCOQuery(ratZoneUnlocoPk, "UNLO3", "UNLOCO3"));
			TestConnection.ExecuteNonQuery(getInsertTestCountryQuery(ratZoneCountryPk, "C3", "COUNTRY3", "~3"));
			TestConnection.ExecuteNonQuery(getInsertTestUNLOCOQuery(zone2OldUnlocoPk, "UNLO4", "UNLOCO4"));
			TestConnection.ExecuteNonQuery(getInsertTestCountryQuery(zone2OldCountryPk, "C4", "COUNTRY4", "~3"));
		}

		void PrepareExistingZonesTestData()
		{
			string getInsertRefZoneHeaderQuery(Guid zoneHeaderPk, string zoneCode, string zoneName, string zoneType) => $@"INSERT {RefZoneHeaderSchema.Constants.SqlSchemaName}.{RefZoneHeaderSchema.Constants.TableName} ({RefZoneHeaderSchema.Constants.PK}, {RefZoneHeaderSchema.Constants.FZ_Code}, {RefZoneHeaderSchema.Constants.FZ_Description}, {RefZoneHeaderSchema.Constants.FZ_ZoneType}, {RefZoneHeaderSchema.Constants.FZ_SystemLastEditTimeUtc}, {RefZoneHeaderSchema.Constants.FZ_SystemLastEditUser}, {RefZoneHeaderSchema.Constants.FZ_SystemCreateTimeUtc}, {RefZoneHeaderSchema.Constants.FZ_SystemCreateUser})
				VALUES ('{zoneHeaderPk}', '{zoneCode}', '{zoneName}', '{zoneType}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			string getInsertRefZonePivotQuery(Guid zonePivotPk, Guid zoneHeaderPk, Guid parentPk, string parentPrefix) => $@"INSERT {RefZonePivotSchema.Constants.SqlSchemaName}.{RefZonePivotSchema.Constants.TableName} ({RefZonePivotSchema.Constants.PK}, {RefZonePivotSchema.Constants.F2_FZ}, {RefZonePivotSchema.Constants.F2_ParentID}, {RefZonePivotSchema.Constants.F2_ParentTableCode}, {RefZonePivotSchema.Constants.F2_SystemLastEditTimeUtc}, {RefZonePivotSchema.Constants.F2_SystemLastEditUser}, {RefZonePivotSchema.Constants.F2_SystemCreateTimeUtc}, {RefZonePivotSchema.Constants.F2_SystemCreateUser})
				VALUES ('{zonePivotPk}', '{zoneHeaderPk}', '{parentPk}', '{parentPrefix}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			TestConnection.ExecuteNonQuery(getInsertRefZoneHeaderQuery(wrsZoneToEditPk, "Z00E", "ZoneToEdit", "WRS"));
			TestConnection.ExecuteNonQuery(getInsertRefZoneHeaderQuery(wrsZoneToDeletePk, "Z00D", "ZoneToDelete", "WRS"));
			TestConnection.ExecuteNonQuery(getInsertRefZoneHeaderQuery(ratZoneDontTouchPk, "Z00A", "ZoneToAvoid", "RAT"));
			TestConnection.ExecuteNonQuery(getInsertRefZoneHeaderQuery(existingZoneWithSameCodePk, "ZON1", "ZoneWithSameCode", "RAT"));

			TestConnection.ExecuteNonQuery(getInsertRefZonePivotQuery(zone2OldPivotUnlocoPk, wrsZoneToEditPk, zone2OldUnlocoPk, RefUNLOCOSchema.Constants.Prefix));
			TestConnection.ExecuteNonQuery(getInsertRefZonePivotQuery(zone2OldPivotCountryPk, wrsZoneToEditPk, zone2OldCountryPk, RefCountrySchema.Constants.Prefix));
			TestConnection.ExecuteNonQuery(getInsertRefZonePivotQuery(zoneToDeletePivotUnlocoPk, wrsZoneToDeletePk, zone2OldUnlocoPk, RefUNLOCOSchema.Constants.Prefix));
			TestConnection.ExecuteNonQuery(getInsertRefZonePivotQuery(zoneToDeletePivotCountryPk, wrsZoneToDeletePk, zone2OldCountryPk, RefCountrySchema.Constants.Prefix));
			TestConnection.ExecuteNonQuery(getInsertRefZonePivotQuery(Guid.NewGuid(), existingZoneWithSameCodePk, zone1CountryPk, RefCountrySchema.Constants.Prefix));
			TestConnection.ExecuteNonQuery(getInsertRefZonePivotQuery(Guid.NewGuid(), existingZoneWithSameCodePk, zone1UnlocoPk, RefUNLOCOSchema.Constants.Prefix));
		}

		void AssertRefZoneHeaderChanges()
		{
			string getCheckZoneExistsQuery(Guid zonePk) => $"SELECT TOP 1 * FROM {RefZoneHeaderSchema.Constants.SqlSchemaName}.{RefZoneHeaderSchema.Constants.TableName} WHERE {RefZoneHeaderSchema.Constants.PK} = '{zonePk}'";
			string getCheckZonePivotExistsQuery(Guid pivotPk) => $"SELECT TOP 1 * FROM {RefZonePivotSchema.Constants.SqlSchemaName}.{RefZonePivotSchema.Constants.TableName} WHERE {RefZonePivotSchema.Constants.PK} = '{pivotPk}'";
			string getCheckPivotToObjectExistsQuery(Guid objectPk) => $"SELECT TOP 1 * FROM {RefZonePivotSchema.Constants.SqlSchemaName}.{RefZonePivotSchema.Constants.TableName} WHERE {RefZonePivotSchema.Constants.F2_ParentID} = '{objectPk}'";

			bool checkObjectExists(string query)
			{
				var objResult = TestConnection.ExecuteScalar(query);
				return objResult != null && objResult != DBNull.Value;
			}

			AssertEquals("New Zone should be added", expected: true, checkObjectExists(getCheckZoneExistsQuery(wrsZoneToAddPk)));
			AssertEquals("Old zone with same code should exist", expected: true, checkObjectExists(getCheckZoneExistsQuery(existingZoneWithSameCodePk)));
			AssertEquals("Zone to update should still exist", expected: true, checkObjectExists(getCheckZoneExistsQuery(wrsZoneToEditPk)));
			AssertEquals("Zone that is not WRS should still exist", expected: true, checkObjectExists(getCheckZoneExistsQuery(ratZoneDontTouchPk)));
			AssertEquals("WRS Zone should be deleted", expected: false, checkObjectExists(getCheckZoneExistsQuery(wrsZoneToDeletePk)));
			AssertEquals("WRS Zone with carrier should not be inserted", expected: false, checkObjectExists(getCheckZoneExistsQuery(wrsZoneWithCarrierPk)));

			AssertEquals("Old UNLOCO Pivot should be removed", expected: false, checkObjectExists(getCheckPivotToObjectExistsQuery(zone2OldUnlocoPk)));
			AssertEquals("Old Country Pivot should be removed", expected: false, checkObjectExists(getCheckPivotToObjectExistsQuery(zone2OldCountryPk)));
			AssertEquals("New UNLOCO Pivot should be added", expected: true, checkObjectExists(getCheckPivotToObjectExistsQuery(zone1UnlocoPk)));
			AssertEquals("New Country Pivot should be added", expected: true, checkObjectExists(getCheckPivotToObjectExistsQuery(zone1CountryPk)));
			AssertEquals("New UNLOCO Pivot should be added", expected: true, checkObjectExists(getCheckPivotToObjectExistsQuery(zone2UnlocoPk)));
			AssertEquals("New Country Pivot should be added", expected: true, checkObjectExists(getCheckPivotToObjectExistsQuery(zone2CountryPk)));

			AssertEquals("RAT Zone Pivot should be preserved", expected: true, checkObjectExists(getCheckPivotToObjectExistsQuery(ratZoneUnlocoPk)));
			AssertEquals("RAT Zone Pivot should be preserved", expected: true, checkObjectExists(getCheckPivotToObjectExistsQuery(ratZoneCountryPk)));

			AssertEquals("Pivot to non-existing UNLOCO should not be created", expected: false, checkObjectExists(getCheckPivotToObjectExistsQuery(nonExistingUnlocoPk)));
			AssertEquals("Pivot to non-existing Country should not be created", expected: false, checkObjectExists(getCheckPivotToObjectExistsQuery(nonExistingCountryPk)));

			AssertEquals("Pivot from deleted Zone should be deleted as well", expected: false, checkObjectExists(getCheckZonePivotExistsQuery(zoneToDeletePivotCountryPk)));
			AssertEquals("Pivot from deleted Zone should be deleted as well", expected: false, checkObjectExists(getCheckZonePivotExistsQuery(zoneToDeletePivotUnlocoPk)));
		}
	}
	class RefWRSZoneHeaderDataFileForTest : EmbeddedDataFile
	{
		public RefWRSZoneHeaderDataFileForTest() : base(DataFileRelativePathForTest, RefZoneHeaderSchema.Constants.TableName, RefZonePivotSchema.Constants.TableName)
		{
		}

		public override string ResourceRelativeName => "RefWRSZoneHeader.Test.TestFiles.RefWRSZoneHeader.xml";

		const string DataFileRelativePathForTest = @"RefWRSZoneHeader\TestFiles\RefWRSZoneHeader.xml";
	}
}
