using System;
using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class GlbDepartmentUpgradeTaskTest : TransactionedTestCase
	{
		public void TestDoNotUpdateGE_IsActive()
		{
			//Deactivate system defined Department
			string updateSql = @"UPDATE dbo.GlbDepartment SET GE_IsActive = 0, GE_SystemLastEditUser = 'E', GE_SystemLastEditTimeUtc = GetDate() WHERE GE_PK = 'f5c72696-19ad-4759-879f-89c8532ff238'";
			Db.Connection.ExecuteNonQuery(updateSql);

			// Inserts 3 Departments
			// 111 - System Department
			// 222 - System Department
			// 333 - User Department, referencing 222
			string insertSql = @"INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_IsActive, GE_Desc, GE_SystemCode) VALUES ('23F14091-7FEA-4D96-A16C-1A1C3741C75D', '222', 1, 'Test System', 1)
				INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_IsActive, GE_Desc, GE_SystemCode, GE_GE) VALUES ('161C7962-6886-4FF4-9E49-3F2494A093B9', '333', 1, 'Test User', 0, '23F14091-7FEA-4D96-A16C-1A1C3741C75D')";
			Db.Connection.ExecuteNonQuery(insertSql);

			//Run the upgrade task
			GlbDepartmentUpgradeTask task = new GlbDepartmentUpgradeTask();
			task.Run();

			//Get the decativated system department
			string resultSql = @"SELECT GE_IsActive FROM dbo.GlbDepartment WHERE GE_PK = 'f5c72696-19ad-4759-879f-89c8532ff238'";
			object result = Db.Connection.ExecuteScalar(resultSql);

			AssertEquals("Deparment should remain deactivated", false, result);
		}

		public void TestDoesNotInsertIfUserDepartmentWithSamePkExists()
		{
			string sqlText = "SELECT GE_PK FROM dbo.GlbDepartment WHERE GE_Code = 'BRN'";
			Guid systemBrnPk = (Guid)TestConnection.ExecuteScalar(sqlText);
			Guid user222Pk = Guid.NewGuid();

			// Updates BRN to be a user department
			// Inserts 1 new user department
			sqlText = String.Format(@"
				UPDATE dbo.GlbDepartment SET GE_Code = '111', GE_SystemCode = 0, GE_SystemLastEditUser = 'E', GE_SystemLastEditTimeUtc = GetDate() WHERE GE_PK = '{0}'
				INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_IsActive, GE_Desc, GE_SystemCode) VALUES ('{1}', '222', 1, 'Test User', 0)",
				systemBrnPk.ToString(), user222Pk.ToString());
			TestConnection.ExecuteNonQuery(sqlText);

			AssertEquals("[before upgrade] 111 user department (former BRN) is in the database?", true, IsDepartmentInDatabase(systemBrnPk, "111", false));
			AssertEquals("[before upgrade] 222 user department is in the database?", true, IsDepartmentInDatabase(user222Pk, "222", false));

			GlbDepartmentUpgradeTask task = new GlbDepartmentUpgradeTask();
			task.Run();

			AssertEquals("[after upgrade] 111 user department (former BRN) is in the database?", true, IsDepartmentInDatabase(systemBrnPk, "111", false));
			AssertEquals("[after upgrade] 222 user department is in the database?", true, IsDepartmentInDatabase(user222Pk, "222", false));
		}

		public void TestHandlesDuplicateCodeOnUpdate()
		{
			string sqlText = "SELECT GE_PK FROM dbo.GlbDepartment WHERE GE_Code = 'BRN'";
			Guid systemBrnPk = (Guid)TestConnection.ExecuteScalar(sqlText);
			Guid userBrnPk = Guid.NewGuid();

			// Updates existing BRN code to 111
			// Inserts 1 new user department with code = BRN
			sqlText = String.Format(@"
				UPDATE dbo.GlbDepartment SET GE_Code = '111', GE_SystemLastEditUser = 'E', GE_SystemLastEditTimeUtc = GetDate() WHERE GE_PK = '{0}'
				INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_IsActive, GE_Desc, GE_SystemCode) VALUES ('{1}', 'BRN', 1, 'Test User', 0)",
				systemBrnPk.ToString(), userBrnPk.ToString());
			TestConnection.ExecuteNonQuery(sqlText);

			AssertEquals("[before upgrade] 111 system department (former BRN) is in the database?", true, IsDepartmentInDatabase(systemBrnPk, "111", true));
			AssertEquals("[before upgrade] BRN user department (BRN) is in the database?", true, IsDepartmentInDatabase(userBrnPk, "BRN", false));

			GlbDepartmentUpgradeTask task = new GlbDepartmentUpgradeTask();
			task.Run();

			AssertEquals("[after upgrade] BRN system department (BRN->111->BRN) is in the database?", true, IsDepartmentInDatabase(systemBrnPk, "BRN", true));
			AssertEquals("[after upgrade] BRN user department (new) is in the database?", false, IsDepartmentInDatabase(userBrnPk, "BRN", false));
		}

		public void TestDeleteOrInactivateUnmatchingSystemRows()
		{
			// Inserts 3 Departments
			// 111 - System Department
			// 222 - System Department
			// 333 - User Department, referencing 222
			string insertSql = @"
				INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_IsActive, GE_Desc, GE_SystemCode) VALUES ('3D4877DB-C899-4282-97AD-9E7F3DC42E98', '111', 1, 'Test System', 1)
				INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_IsActive, GE_Desc, GE_SystemCode) VALUES ('23F14091-7FEA-4D96-A16C-1A1C3741C75D', '222', 1, 'Test System', 1)
				INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_IsActive, GE_Desc, GE_SystemCode, GE_GE) VALUES ('161C7962-6886-4FF4-9E49-3F2494A093B9', '333', 1, 'Test User', 0, '23F14091-7FEA-4D96-A16C-1A1C3741C75D')";
			Db.Connection.ExecuteNonQuery(insertSql);

			GlbDepartmentUpgradeTask task = new GlbDepartmentUpgradeTask();
			task.Run();

			// Doesn't use a GlbDepartmentDataFile as it doesn't load non-system GlbDepartments
			DataFile tempFile = new EmbeddedDataFile("", "GlbDepartment");
			var data = tempFile.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);
			AssertEquals("Table Count", "GlbDepartment", data.Tables[0].TableName);

			DataRow[] unmatchingSystemRows = data.Tables["GlbDepartment"].Select("GE_Desc = 'Test System'");
			AssertEquals("Unmatching System Departments Count", 1, unmatchingSystemRows.Length);
			AssertEquals("FK-Referenced Department should have been preserved", new Guid("23F14091-7FEA-4D96-A16C-1A1C3741C75D"), unmatchingSystemRows[0][GlbDepartmentSchema.Constants.PK]);
			AssertEquals("FK-Referenced Department should have been inactivated", false, unmatchingSystemRows[0][GlbDepartmentSchema.Constants.GE_IsActive]);
			Assert("FK-Referenced Department code should have been changed", unmatchingSystemRows[0][GlbDepartmentSchema.Constants.GE_Code].ToString() != "222");

			DataRow[] userRows = data.Tables["GlbDepartment"].Select("GE_Desc = 'Test User'");
			AssertEquals("User Department should NOT have been deleted", 1, userRows.Length);
			AssertEquals("User Department should NOT have been inactivated", true, userRows[0][GlbDepartmentSchema.Constants.GE_IsActive]);
		}

		public void TestDuplicatedUnmatchingNonFkReferencedSystemDepartmentIsDeletedToAvoidConflicts()
		{
			// Changes the PK of Department BRN to force unmatching
			string sqlText = "UPDATE dbo.GlbDepartment SET GE_PK = '23F14091-7FEA-4D96-A16C-1A1C3741C75D', GE_SystemLastEditUser = 'E', GE_SystemLastEditTimeUtc = GetDate() WHERE GE_Code = 'MSJ'";
			Db.Connection.ExecuteNonQuery(sqlText);

			GlbDepartmentUpgradeTask task = new GlbDepartmentUpgradeTask();
			task.Run();

			sqlText = "SELECT count(*) FROM dbo.GlbDepartment WHERE GE_PK = '23F14091-7FEA-4D96-A16C-1A1C3741C75D'";
			int rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Duplicated Unmatching Non-FK-Referenced System Department should have been deleted", 0, rowCount);
		}

		public void TestDuplicatedUnmatchingNonReferencedUserDepartmentIsDeletedToAvoidConflicts()
		{
			// Changes the PK of Department BRN to force unmatching; and SystemCode to 0 to become a User Department
			string sqlText = "UPDATE dbo.GlbDepartment SET GE_PK = '23F14091-7FEA-4D96-A16C-1A1C3741C75D', GE_SystemCode = 0, GE_SystemLastEditUser = 'E', GE_SystemLastEditTimeUtc = GetDate() WHERE GE_Code = 'MSJ'";
			Db.Connection.ExecuteNonQuery(sqlText);

			GlbDepartmentUpgradeTask task = new GlbDepartmentUpgradeTask();
			task.Run();

			sqlText = "SELECT count(*) FROM dbo.GlbDepartment WHERE GE_PK = '23F14091-7FEA-4D96-A16C-1A1C3741C75D'";
			int rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Duplicated Unmatching Non-FK-Referenced User Department should have been deleted", 0, rowCount);
		}

		public void TestDuplicatedUnmatchingReferencedSystemDepartmentIsInactivatedToAvoidConflicts()
		{
			// Changes the PK of Department MSJ to force unmatching
			// Inserts a User Department, referencing the new PK
			string sqlText = @"
				UPDATE dbo.GlbDepartment SET GE_PK = '23F14091-7FEA-4D96-A16C-1A1C3741C75D', GE_SystemLastEditUser = 'E', GE_SystemLastEditTimeUtc = GetDate() WHERE GE_Code = 'MSJ'
				INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_IsActive, GE_Desc, GE_SystemCode, GE_GE) VALUES ('161C7962-6886-4FF4-9E49-3F2494A093B9', '111', 1, 'Test User', 0, '23F14091-7FEA-4D96-A16C-1A1C3741C75D')";
			Db.Connection.ExecuteNonQuery(sqlText);

			GlbDepartmentUpgradeTask task = new GlbDepartmentUpgradeTask();
			task.Run();

			sqlText = "SELECT GE_IsActive FROM dbo.GlbDepartment WHERE GE_PK = '23F14091-7FEA-4D96-A16C-1A1C3741C75D'";
			var isActive = (bool)Db.Connection.ExecuteScalar(sqlText);
			AssertEquals("Duplicated Unmatching FK-Referenced System Department should have been inactivated", false, isActive);

			sqlText = "SELECT GE_Code FROM dbo.GlbDepartment WHERE GE_PK = '23F14091-7FEA-4D96-A16C-1A1C3741C75D'";
			string code = Db.Connection.ExecuteScalar(sqlText).ToString();
			Assert("Duplicated Unmatching FK-Referenced System Department code should have been changed", code != "MSJ");
		}

		public void TestDuplicatedUnmatchingReferencedUserDepartmentStaysActivateAndNewSystemOneIsDeactivated()
		{
			// Changes the PK of Department BRN to force unmatching; and SystemCode to 0 to become a User Department
			// Inserts a User Department, referencing the new PK
			string sqlText = @"
				UPDATE dbo.GlbDepartment SET GE_PK = '23F14091-7FEA-4D96-A16C-1A1C3741C75D', GE_SystemCode = 0, GE_SystemLastEditUser = 'E', GE_SystemLastEditTimeUtc = GetDate() WHERE GE_Code = 'MSJ'
				INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_IsActive, GE_Desc, GE_SystemCode, GE_GE) VALUES ('161C7962-6886-4FF4-9E49-3F2494A093B9', '111', 1, 'Test User', 0, '23F14091-7FEA-4D96-A16C-1A1C3741C75D')";
			Db.Connection.ExecuteNonQuery(sqlText);

			GlbDepartmentUpgradeTask task = new GlbDepartmentUpgradeTask();
			task.Run();

			sqlText = "SELECT GE_IsActive FROM dbo.GlbDepartment WHERE GE_PK = '23F14091-7FEA-4D96-A16C-1A1C3741C75D'";
			bool isActive = (bool)Db.Connection.ExecuteScalar(sqlText);
			AssertEquals("Duplicated Unmatching User FK-Referenced Department should stay active", true, isActive);

			sqlText = "SELECT GE_Code FROM dbo.GlbDepartment WHERE GE_PK = '23F14091-7FEA-4D96-A16C-1A1C3741C75D'";
			string code = Db.Connection.ExecuteScalar(sqlText).ToString();
			Assert("Duplicated Unmatching FK-Referenced User Department code should have been changed", code != "MSJ");

			sqlText = "SELECT GE_IsActive FROM dbo.GlbDepartment WHERE GE_PK = '66fb3c22-9e61-43ae-a9ca-17a2df581049'";
			bool isActiveDepartment = (bool)Db.Connection.ExecuteScalar(sqlText);
			AssertEquals("Added System Department with Duplicated Code should have been inactivated", false, isActiveDepartment);
		}

		public void TestDuplicatedUnmatchingReferencedInactiveUserDepartmentStaysInactivateAndNewSystemOneAddedAsActive()
		{
			// Changes the PK of Department MSJ to force unmatching; and SystemCode to 0 to become a User Department
			// Inserts a User Department, referencing the new PK
			string sqlText = @"
				UPDATE dbo.GlbDepartment SET GE_PK = '23F14091-7FEA-4D96-A16C-1A1C3741C75D', GE_SystemCode = 0, GE_IsActive=0, GE_SystemLastEditUser = 'E', GE_SystemLastEditTimeUtc = GetDate() WHERE GE_Code = 'MSJ'
				INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_IsActive, GE_Desc, GE_SystemCode, GE_GE) VALUES ('161C7962-6886-4FF4-9E49-3F2494A093B9', '111', 1, 'Test User', 0, '23F14091-7FEA-4D96-A16C-1A1C3741C75D')";
			Db.Connection.ExecuteNonQuery(sqlText);

			GlbDepartmentUpgradeTask task = new GlbDepartmentUpgradeTask();
			task.Run();

			sqlText = "SELECT GE_IsActive FROM dbo.GlbDepartment WHERE GE_PK = '23F14091-7FEA-4D96-A16C-1A1C3741C75D'";
			bool isActive = (bool)Db.Connection.ExecuteScalar(sqlText);
			AssertEquals("Duplicated Matching User FK-Referenced Department should stay inactive", false, isActive);

			sqlText = "SELECT GE_Code FROM dbo.GlbDepartment WHERE GE_PK = '23F14091-7FEA-4D96-A16C-1A1C3741C75D'";
			string code = Db.Connection.ExecuteScalar(sqlText).ToString();
			Assert("Duplicated Matching FK-Referenced User Department code should have been changed", code != "MSJ");

			sqlText = "SELECT GE_IsActive FROM dbo.GlbDepartment WHERE GE_PK = '66fb3c22-9e61-43ae-a9ca-17a2df581049'";
			bool isActiveDepartment = (bool)Db.Connection.ExecuteScalar(sqlText);
			AssertEquals("Added System Department with Duplicated Code should be active", true, isActiveDepartment);
		}

		/// <summary>
		/// Department codes starting with Tildo are reserved to resolve unique code conflicts between system and user departments
		/// during a data upgrade.
		/// </summary>
		public void TestNoBaseDataDepartmentCodeStartsWithTildo()
		{
			GlbDepartmentUpgradeTask task = new GlbDepartmentUpgradeTask();
			task.RunForSetup();

			string sqlText = "SELECT count(*) FROM dbo.GlbDepartment WHERE GE_Code like '~%'";
			int rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("There should be no 'base data departments' which code starts with '~'", 0, rowCount);
		}

		bool IsDepartmentInDatabase(Guid pk, string code, bool isSystem)
		{
			string sqlText = String.Format(
				"SELECT count(*) FROM dbo.GlbDepartment WHERE GE_PK = '{0}' AND GE_Code = '{1}' AND GE_SystemCode = {2}",
				pk.ToString(), code, isSystem ? "1" : "0");
			int rowCount = (int)TestConnection.ExecuteScalar(sqlText);
			return (rowCount == 1);
		}
	}
}
