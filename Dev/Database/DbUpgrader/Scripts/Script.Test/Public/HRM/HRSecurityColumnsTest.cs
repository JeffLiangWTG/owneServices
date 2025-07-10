using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(HRSecurityColumns))]
	class HRSecurityColumnsTest : DbCreateScriptTest
	{
		public void TestGetColumnNames()
		{
			AssertSequencesEqual(HRSecurityColumns.GetColumnNames(), new string[] { "IsSelf", "IsManaged1", "IsManaged2", "IsManaged3" });
		}

		public void TestExecute_PathFromStaff0()
		{
			var actualResults = Execute(staff0, "HRM,PPL,DRM", DateTimeOffset.Now);
			var expectedResultArray = new object[][] {
				new object[] { staff0, true, false, false, false },
				new object[] { staff1, false, false, true, false },
				new object[] { staff2, false, true, false, false },
				new object[] { staff3, false, false, false, true },
				new object[] { staff4, false, true, false, false },
				new object[] { staff7, false, false, false, true },
				new object[] { staff8, false, true, false, false },
				new object[] { staff9, false, false, false, true },
			};

			AssertResult("The Staff0 manages all children except Staff5 and Staff6.", expectedResultArray, actualResults);
		}

		public void TestExecute_PathFromStaff2()
		{
			var actualResults = Execute(staff2, "HRM,PPL,DRM", DateTimeOffset.Now);
			var expectedResultArray = new object[][] {
					new object[] { staff2, true, false, false, false },
					new object[] { staff4, false, true, false, false },
					new object[] { staff5, false, false, false, true },
					new object[] { staff8, false, true, false, false },
				};

			AssertResult("The Staff2 manages all it's children.", expectedResultArray, actualResults);
		}

		public void TestExecute_PathFromStaff3()
		{
			var actualResults = Execute(staff3, "HRM,PPL,DRM", DateTimeOffset.Now);
			var expectedResultArray = new object[][] {
					new object[] { staff3, true, false, false, false },
					new object[] { staff6, false, true, false, false },
					new object[] { staff7, false, false, false, true },
					new object[] { staff9, false, false, false, true },
				};

			AssertResult("The Staff3 manages all it's children.", expectedResultArray, actualResults);
		}

		public void TestExecute_HRMPathFromStaff2()
		{
			var actualResults = Execute(staff2, "HRM", DateTimeOffset.Now);
			var expectedResultArray = new object[][] {
					new object[] { staff2, true, false, false, false },
					new object[] { staff4, false, true, false, false },
					new object[] { staff8, false, true, false, false },
				};

			AssertResult("The Staff2 is the HRM manager of Staff4 and Staff8.", expectedResultArray, actualResults);
		}

		public void TestExecute_DRMPathFromStaff2()
		{
			var actualResults = Execute(staff2, "DRM", DateTimeOffset.Now);
			var expectedResultArray = new object[][] {
					new object[] { staff2, true, false, false, false },
					new object[] { staff5, false, true, false, false },
				};

			AssertResult("The Staff2 is the DRM manager of Staff5.", expectedResultArray, actualResults);
		}

		public void TestExecute_PPLPathFromStaff2()
		{
			var actualResults = Execute(staff2, "PPL", DateTimeOffset.Now);
			var expectedResultArray = new object[][] {
					new object[] { staff2, true, false, false, false },
				};

			AssertResult("The Staff2 is not the PPL manager of any of it's children.", expectedResultArray, actualResults);
		}

		public void TestExecute_PPLPathFromStaff0()
		{
			var actualResults = Execute(staff0, "PPL", DateTimeOffset.Now);
			var expectedResultArray = new object[][] {
					new object[] { staff0, true, false, false, false },
					new object[] { staff1, false, true, false, false },
				};

			AssertResult("The Staff0 is the PPL manager of Staff1.", expectedResultArray, actualResults);
		}

		static void AssertResult(string message, object[][] expectedResultArray, DataTable actualResults)
		{
			AssertEquals(5, actualResults.Columns.Count);
			AssertEquals("StaffPK", actualResults.Columns[0].ColumnName);
			AssertEquals(typeof(Guid), actualResults.Columns[0].DataType);
			AssertEquals("IsSelf", actualResults.Columns[1].ColumnName);
			AssertEquals(typeof(bool), actualResults.Columns[1].DataType);
			AssertEquals("IsManaged1", actualResults.Columns[2].ColumnName);
			AssertEquals(typeof(bool), actualResults.Columns[2].DataType);
			AssertEquals("IsManaged2", actualResults.Columns[3].ColumnName);
			AssertEquals(typeof(bool), actualResults.Columns[3].DataType);
			AssertEquals("IsManaged3", actualResults.Columns[4].ColumnName);
			AssertEquals(typeof(bool), actualResults.Columns[4].DataType);

			AssertEquals(expectedResultArray.Length, actualResults.Rows.Count);

			for (int i = 0; i < actualResults.Rows.Count; i++)
			{
				var acctualRowValue = actualResults.Rows[i].ItemArray;
				var expectedRowValue = expectedResultArray.First(row => row[0].Equals(acctualRowValue[0]));

				AssertEquals(5, acctualRowValue.Length);
				AssertSequencesEqual(message, expectedRowValue, acctualRowValue);
			}
		}

		DataTable Execute(Guid loggedInStaff, string managerTypes, DateTimeOffset effectiveAsAt)
		{
			using (var command = TestConnection.Command($"SELECT * FROM HRSecurityColumns(@loggedInStaff, @managerTypes, @effectiveAsAt)"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, loggedInStaff);
				command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);
				command.AddParameter("@effectiveAsAt", SqlDbType.DateTimeOffset, effectiveAsAt);

				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			staff0 = Guid.NewGuid();
			staff1 = Guid.NewGuid();
			staff2 = Guid.NewGuid();
			staff3 = Guid.NewGuid();
			staff4 = Guid.NewGuid();
			staff5 = Guid.NewGuid();
			staff6 = Guid.NewGuid();
			staff7 = Guid.NewGuid();
			staff8 = Guid.NewGuid();
			staff9 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES
	(@staff0, 'S00', 'Staff00', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff1, 'S01', 'Staff01', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff2, 'S02', 'Staff02', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff3, 'S03', 'Staff03', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff4, 'S04', 'Staff04', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff5, 'S05', 'Staff05', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff6, 'S06', 'Staff06', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff7, 'S07', 'Staff07', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff8, 'S08', 'Staff08', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff9, 'S09', 'Staff09', GETUTCDATE(), 'E', GETUTCDATE(), 'E');


INSERT INTO dbo.GlbStaffManager
	(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
VALUES
	(newid(), 'PPL', @staff0, @staff1, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(newid(), 'HRM', @staff0, @staff2, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(newid(), 'DRM', @staff0, @staff3, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),

	(newid(), 'HRM', @staff2, @staff4, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(newid(), 'DRM', @staff2, @staff5, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),

	(newid(), 'HRM', @staff3, @staff6, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(newid(), 'DRM', @staff3, @staff7, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),

	(newid(), 'HRM', @staff4, @staff8, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),

	(newid(), 'DRM', @staff7, @staff9, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E');

			", p =>
			{
				p.AddParameter("@staff0", SqlDbType.UniqueIdentifier, staff0);
				p.AddParameter("@staff1", SqlDbType.UniqueIdentifier, staff1);
				p.AddParameter("@staff2", SqlDbType.UniqueIdentifier, staff2);
				p.AddParameter("@staff3", SqlDbType.UniqueIdentifier, staff3);
				p.AddParameter("@staff4", SqlDbType.UniqueIdentifier, staff4);
				p.AddParameter("@staff5", SqlDbType.UniqueIdentifier, staff5);
				p.AddParameter("@staff6", SqlDbType.UniqueIdentifier, staff6);
				p.AddParameter("@staff7", SqlDbType.UniqueIdentifier, staff7);
				p.AddParameter("@staff8", SqlDbType.UniqueIdentifier, staff8);
				p.AddParameter("@staff9", SqlDbType.UniqueIdentifier, staff9);
			});
		}

		Guid staff0;
		Guid staff1;
		Guid staff2;
		Guid staff3;
		Guid staff4;
		Guid staff5;
		Guid staff6;
		Guid staff7;
		Guid staff8;
		Guid staff9;
	}
}

