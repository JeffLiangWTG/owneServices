using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(GlbStaffManager_WithIsNew))]
	class GlbStaffManager_WithIsNewTest : DbCreateScriptTest
	{
		public void TestStaffManagers_ReturnsEmpty()
		{
			var rows = RunStaffManagersQuery().Select();
			AssertEquals(0, rows.Length);
		}

		public void TestStaffManagers_ReturnsCorrectRows()
		{
			var staff1PK = TestDataCreator.CreateGlbStaff("EM1", "Employee 1");
			var staff2PK = TestDataCreator.CreateGlbStaff("EM2", "Employee 2");
			var manager1PK = TestDataCreator.CreateGlbStaff("MN1", "Manager 1");
			var manager2PK = TestDataCreator.CreateGlbStaff("MN2", "Manager 2");

			var staffManager1PK = AddStaffManager(staff1PK, manager1PK);
			var staffManager2PK = AddStaffManager(staff2PK, manager2PK);

			var rows = RunStaffManagersQuery().Select();
			AssertEquals(2, rows.Length);
			AssertStaffManagerRow(rows, staffManager1PK, staff1PK, manager1PK);
			AssertStaffManagerRow(rows, staffManager2PK, staff2PK, manager2PK);
		}

		Guid AddStaffManager(Guid staffPK, Guid managerPK)
		{
			var staffManagerPK = Guid.NewGuid();
			var staffManagerRecord = AddAuditValues("GSM", new Dictionary<string, string>()
			{
				{ "GSM_PK", "@staffManagerPK" },
				{ "GSM_GS_Staff", "@staffPK" },
				{ "GSM_ManagerType", "'PPL'" },
				{ "GSM_GS_Manager", "@managerPK" },
				{ "GSM_EffectiveDate", "@testDateUtc" },
				{ "GSM_GCR_ChangeRequest", "NULL" },
				{ "GSM_IsApproved", "0" },
			});
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.GlbStaffManager({0}) VALUES ({1})",
				string.Join(",", staffManagerRecord.Keys),
				string.Join(",", staffManagerRecord.Values)), (p) =>
				{
					p.AddParameter("@staffManagerPK", SqlDbType.UniqueIdentifier, staffManagerPK);
					p.AddParameter("@staffPK", SqlDbType.UniqueIdentifier, staffPK);
					p.AddParameter("@managerPK", SqlDbType.UniqueIdentifier, managerPK);
					p.AddParameter("@testDateUtc", SqlDbType.VarChar, TestDateUtc);
				});

			return staffManagerPK;
		}

		Dictionary<string, string> AddAuditValues(string prefix, Dictionary<string, string> recordValues)
		{
			recordValues.Add($"{prefix}_SystemCreateTimeUtc", "@testDateUtc");
			recordValues.Add($"{prefix}_SystemCreateUser", "'E'");
			recordValues.Add($"{prefix}_SystemLastEditTimeUtc", "@testDateUtc");
			recordValues.Add($"{prefix}_SystemLastEditUser", "'E'");
			return recordValues;
		}

		DataTable RunStaffManagersQuery()
		{
			using (var command = TestConnection.Command($"SELECT * FROM GlbStaffManager_WithIsNew()"))
			{
				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		void AssertStaffManagerRow(DataRow[] rows, Guid staffManagerPK, Guid staffPK, Guid managerPK)
		{
			var row = rows.Single(r => (Guid)r["GSM_PK"] == staffManagerPK);

			AssertEquals((Guid)row["GSM_PK"], staffManagerPK);
			AssertEquals((Guid)row["GSM_GS_Staff"], staffPK);
			AssertEquals((string)row["GSM_ManagerType"], "PPL");
			AssertEquals((Guid)row["GSM_GS_Manager"], managerPK);
			AssertEquals((DateTime)row["GSM_EffectiveDate"], TestDateUtc);
			AssertEquals(row["GSM_GCR_ChangeRequest"], DBNull.Value);
			AssertEquals((bool)row["GSM_IsApproved"], false);
			AssertEquals((string)row["GSM_SystemCreateUser"], "E");
			AssertEquals((DateTime)row["GSM_SystemCreateTimeUtc"], TestDateUtc);
			AssertEquals((string)row["GSM_SystemLastEditUser"], "E");
			AssertEquals((DateTime)row["GSM_SystemLastEditTimeUtc"], TestDateUtc);
		}

		DateTime TestDateUtc => new DateTime(2023, 1, 1, 8, 0, 0);
	}
}
