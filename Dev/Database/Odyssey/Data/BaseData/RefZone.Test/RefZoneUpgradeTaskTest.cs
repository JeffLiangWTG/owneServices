using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Data.BaseData.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.RefZone
{
	sealed class RefZoneUpgradeTaskTest : TransactionedTestCase
	{
		public void TestXmlIsUpToDate()
		{
			string zoneTab = RefZoneHeaderSchema.Constants.TableName;
			string pivotTab = RefZonePivotSchema.Constants.TableName;
			RefZoneUpgradeTask task = new RefZoneUpgradeTask();
			var ds = task.ResourceFile.DataSet;

			List<Zone> zonesInDb = new List<Zone>();
			using (DbCommand cmd = Db.Connection.Command("select fz_pk, fz_code, fz_description, fz_zoneType, fz_zoneMode from " + zoneTab))
			using (var rd = cmd.ExecuteReader())
			{
				while (rd.Read())
				{
					zonesInDb.Add(new Zone(rd.GetGuid(0).ToString(), rd.GetString(1), rd.GetString(2), rd.GetString(3), rd.GetString(4)));
				}
			}

			List<string> additionalZonesInXml = new List<string>();

			foreach (DataRow r in ds.Tables[zoneTab].Rows)
			{
				string fz_code = r["FZ_Code"].ToString();
				string fz_descr = r["FZ_Description"].ToString();
				string fz_zoneType = r["FZ_ZoneType"].ToString();
				string fz_zoneMode = r["FZ_ZoneMode"].ToString();

				if (!zonesInDb.Exists(p => (p.FZ_Code == fz_code && p.FZ_Description == fz_descr && p.FZ_ZoneType == fz_zoneType && p.FZ_ZoneMode == fz_zoneMode)))
				{
					additionalZonesInXml.Add("FZ_Code: " + fz_code + ", FZ_Description: " + fz_descr + ", FZ_ZoneType: " + fz_zoneType + ", FZ_ZoneMode: " + fz_zoneMode);
				}
			}

			List<Pivot> pivotsInDb = new List<Pivot>();
			using (DbCommand cmd = Db.Connection.Command("select f2_fz, f2_parentid, f2_parenttablecode from " + pivotTab))
			using (var rd = cmd.ExecuteReader())
			{
				while (rd.Read())
				{
					pivotsInDb.Add(new Pivot(rd.GetGuid(0).ToString(), rd.GetGuid(1).ToString(), rd.GetString(2)));
				}
			}

			List<string> additionalPivotsInXml = new List<string>();

			foreach (DataRow r in ds.Tables[pivotTab].Rows)
			{
				string f2_fz = r["F2_FZ"].ToString();
				string f2_parentId = r["F2_ParentID"].ToString();
				string f2_parentTableCode = r["F2_ParentTableCode"].ToString();

				if (Array.Exists(zonesInDb.ToArray(), z => z.FZ_PK == f2_fz) && !pivotsInDb.Exists(p => (p.F2_FZ == f2_fz && p.F2_ParentID == f2_parentId && p.F2_ParentTableCode == f2_parentTableCode)))
				{
					additionalPivotsInXml.Add("F2_FZ: " + f2_fz + ", F2_ParentID: " + f2_parentId + ", F2_ParentTableCode: " + f2_parentTableCode);
				}
			}

			if (additionalZonesInXml.Count > 0 || additionalPivotsInXml.Count > 0)
			{
				Fail("To fix, add transformations.\r\nZones in xml, which are not in Database (plus all their pivots):\r\n" + string.Join("\r\n", additionalZonesInXml.ToArray()) +
					"\r\n\r\nPivots in xml, which are not in Database(for existing zones only):\r\n" + string.Join("\r\n", additionalPivotsInXml.ToArray()));
			}
			else
			{
				Assert(true);
			}
		}

		class Zone
		{
			public Zone(string fz_pk, string fz_code, string fz_description, string fz_zoneType, string fz_zoneMode)
			{
				FZ_PK = fz_pk;
				FZ_Code = fz_code;
				FZ_Description = fz_description;
				FZ_ZoneType = fz_zoneType;
				FZ_ZoneMode = fz_zoneMode;
			}

			public string FZ_PK { get; private set; }
			public string FZ_Code { get; private set; }
			public string FZ_Description { get; private set; }
			public string FZ_ZoneType { get; private set; }
			public string FZ_ZoneMode { get; private set; }
		}

		class Pivot
		{
			public Pivot(string f2_fz, string f2_parentid, string f2_parentTableCode)
			{
				F2_FZ = f2_fz;
				F2_ParentID = f2_parentid;
				F2_ParentTableCode = f2_parentTableCode;
			}

			public string F2_FZ { get; private set; }
			public string F2_ParentID { get; private set; }
			public string F2_ParentTableCode { get; private set; }
		}

		public void TestIsRequired()
		{
			RefZoneUpgradeTask task = new RefZoneUpgradeTask();
			task.ResourceFile.VersionInDatabase = 0;
			Assert(task.IsRequired);

			task.ResourceFile.VersionInDatabase = 1;
			Assert(!task.IsRequired);
		}

		public void TestAllOhCarrierReferencesInXmlAreNull()
		{
			RefZoneUpgradeTask testTask = new RefZoneUpgradeTask();
			testTask.Run();

			string sqlText = "SELECT TOP 1 FZ_PK FROM dbo.RefZoneHeader WHERE FZ_OH_RelatedParty is not null";
			AssertEquals("Non null FZ_OH_RelatedParty exists?", false, BaseDataUpgradeTask.IsRecordInDatabase(sqlText));
		}

		/// <summary>
		/// If other tables are added to F2_ParentTableCode in the XML,
		/// the assert SQL query must be changed to include them.
		/// </summary>
		public void TestAllRefZonePivotParentIdInXmlAreValid()
		{
			RefZoneUpgradeTask testTask = new RefZoneUpgradeTask();
			testTask.Run();

			string sqlText = @"
				SELECT TOP 1 F2_ParentID
				FROM
					dbo.RefZonePivot
					LEFT JOIN dbo.RefCountry ON (F2_ParentTableCode = 'RN' AND RN_PK = F2_ParentID)
					LEFT JOIN dbo.RefUNLOCO ON (F2_ParentTableCode = 'RL' AND RL_PK = F2_ParentID)
				WHERE
					RN_PK is null
					AND RL_PK is null";
			AssertEquals("Invalid Parent ID references exist?", false, BaseDataUpgradeTask.IsRecordInDatabase(sqlText));
		}

		public void TestRun()
		{
			// Prepare test data
			Guid newZonePk = Guid.NewGuid();
			Guid newPivotPk = Guid.NewGuid();
			Guid auZonePk = new Guid("B97AEF8B-DBD4-4B20-B07F-13202F73436B");

			string sqlText = String.Format(@"
				-- RefZoneHeader
				INSERT dbo.RefZoneHeader (FZ_PK, FZ_Code, FZ_Description, FZ_ZoneType, FZ_SystemCreateTimeUtc, FZ_SystemCreateUser, FZ_SystemLastEditTimeUtc, FZ_SystemLastEditUser) VALUES ('{0}', '~Z1', '~ZonaUno', 'ALL', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				UPDATE dbo.RefZoneHeader SET FZ_Description = 'Some~Desc~Not~To~Be~Changed' WHERE FZ_PK = '{1}';
				-- RefZonePivot
				INSERT dbo.RefZonePivot (F2_PK, F2_FZ, F2_ParentID, F2_ParentTableCode, F2_SystemCreateTimeUtc, F2_SystemCreateUser, F2_SystemLastEditTimeUtc, F2_SystemLastEditUser) VALUES ('{2}', '{0}', '00000000-0000-0000-0000-000000000000', 'RN', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				",
				newZonePk.ToString(),
				auZonePk.ToString(),
				newPivotPk.ToString());
			TestConnection.ExecuteNonQuery(sqlText);

			// RefZoneHeader
			AssertEquals("[BEFORE] New RefZoneHeader in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefZoneHeaderSchema.PK, newZonePk));
			AssertEquals("[BEFORE] Existing RefZoneHeader in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefZoneHeaderSchema.PK, auZonePk));
			// RefZonePivot
			AssertEquals("[BEFORE] New RefZonePivot in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefZonePivotSchema.PK, newPivotPk));

			RefZoneUpgradeTask testTask = new RefZoneUpgradeTask();
			testTask.Run();

			// Assert results

			// RefZoneHeader
			AssertEquals("New RefZoneHeader in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefZoneHeaderSchema.PK, newZonePk));
			AssertEquals("Update dbo.RefZoneHeader name not changed back", "Some~Desc~Not~To~Be~Changed", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, RefZoneHeaderSchema.PK, auZonePk, RefZoneHeaderSchema.FZ_Description));
			// RefZonePivot
			AssertEquals("New RefZonePivot in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, RefZonePivotSchema.PK, newPivotPk));
		}
	}
}
