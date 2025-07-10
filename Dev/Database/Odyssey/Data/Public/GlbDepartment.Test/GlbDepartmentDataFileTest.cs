using System;
using System.IO;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class GlbDepartmentDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new GlbDepartmentDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestGlbDepartmentDataFile()
		{
			string insertSql = @"
				INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_IsActive, GE_Desc, GE_SystemCode) VALUES ('3D4877DB-C899-4282-97AD-9E7F3DC42E98', '111', 1, 'Department 1', 1)
				INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_IsActive, GE_Desc, GE_SystemCode) VALUES ('23F14091-7FEA-4D96-A16C-1A1C3741C75D', '222', 1, 'Department 2', 1)";
			Db.Connection.ExecuteNonQuery(insertSql);

			GlbDepartmentDataFile file = new GlbDepartmentDataFile();
			var data = file.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);
			Assert("GlbDepartment row count", data.Tables["GlbDepartment"].Rows.Count >= 2);
			Assert("Contains Department 1", data.Tables["GlbDepartment"].Rows.Contains(new Guid("3D4877DB-C899-4282-97AD-9E7F3DC42E98")));
			Assert("Contains Department 2", data.Tables["GlbDepartment"].Rows.Contains(new Guid("23F14091-7FEA-4D96-A16C-1A1C3741C75D")));
		}

		public void TestUserRowsAreNotLoaded()
		{
			string insertSql = @"
				INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_IsActive, GE_Desc, GE_SystemCode) VALUES ('3D4877DB-C899-4282-97AD-9E7F3DC42E98', '111', 1, 'System Department', 1)
				INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_IsActive, GE_Desc, GE_SystemCode) VALUES ('23F14091-7FEA-4D96-A16C-1A1C3741C75D', '222', 1, 'User Department',   0)";

			Db.Connection.ExecuteNonQuery(insertSql);

			GlbDepartmentDataFile file = new GlbDepartmentDataFile();
			var data = file.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);
			Assert("System Department should have been loaded", data.Tables["GlbDepartment"].Rows.Contains(new Guid("3D4877DB-C899-4282-97AD-9E7F3DC42E98")));
			Assert("User Department should NOT have been loaded", !data.Tables["GlbDepartment"].Rows.Contains(new Guid("23F14091-7FEA-4D96-A16C-1A1C3741C75D")));
		}
	}
}
