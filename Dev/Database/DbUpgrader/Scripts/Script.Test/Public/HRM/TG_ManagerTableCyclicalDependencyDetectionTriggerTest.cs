using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(TG_ManagerTableCyclicalDependencyDetectionTrigger))]
	class TG_ManagerTableCyclicalDependencyDetectionTriggerTest : DbCreateScriptTest
	{
		[UseSnapshotProtection(true)]
		public void TestChangeIsAborted()
		{
			var mg1 = Guid.NewGuid();
			var mg2 = Guid.NewGuid();
			var mg3 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{mg1}', 'MG1', 'MG1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg2}', 'MG2', 'MG2', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg3}', 'MG3', 'MG3', GETUTCDATE(), 'E', GETUTCDATE(), 'E')"));

			MakeManager(mg2, mg1);
			MakeManager(mg3, mg2);
			AssertExceptionThrown<SqlException>(() => MakeManager(mg1, mg3));

			Assert("When a cycle is detected, the row should be rejected", !TestConnection.Exists("FROM dbo.GlbStaffManager WHERE GSM_GS_Manager=@manager AND GSM_GS_Staff=@staff", p =>
			{
				p.AddParameterBasedOnDbColumn("@manager", mg1, GlbStaffSchema.PK);
				p.AddParameterBasedOnDbColumn("@staff", mg3, GlbStaffSchema.PK);
			}));
		}

		public void TestTriggerFormat()
		{
			var mg1 = Guid.NewGuid();
			var mg2 = Guid.NewGuid();
			var mg3 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{mg1}', 'MG1', 'MG1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg2}', 'MG2', 'MG2', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg3}', 'MG3', 'MG3', GETUTCDATE(), 'E', GETUTCDATE(), 'E')"));

			MakeManager(mg2, mg1);
			MakeManager(mg3, mg2);
			var ex = AssertExceptionThrown<SqlException>(() => MakeManager(mg1, mg3));

			AssertEquals("Expected precisely formatted message, for parsing from GLOW", "Management Cycle Detected: MG1,MG2,MG3,MG1", ex.Message);
		}

		public void TestTrigger_ExistingLoops()
		{
			var mg1 = Guid.NewGuid();
			var mg2 = Guid.NewGuid();
			var mg3 = Guid.NewGuid();
			var mg4 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{mg1}', 'MG1', 'MG1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg2}', 'MG2', 'MG2', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg3}', 'MG3', 'MG3', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg4}', 'MG4', 'MG4', GETUTCDATE(), 'E', GETUTCDATE(), 'E')"));

			TestConnection.ExecuteNonQuery($@"DISABLE TRIGGER [dbo].[{nameof(TG_ManagerTableCyclicalDependencyDetectionTrigger)}] ON [dbo].[{GlbStaffManagerSchema.Constants.TableName}]");

			MakeManager(mg1, mg2);
			MakeManager(mg2, mg3);
			MakeManager(mg3, mg1);

			TestConnection.ExecuteNonQuery($@"ENABLE TRIGGER [dbo].[{nameof(TG_ManagerTableCyclicalDependencyDetectionTrigger)}] ON [dbo].[{GlbStaffManagerSchema.Constants.TableName}]");

			var ex = AssertExceptionThrown<SqlException>(() => MakeManager(mg3, mg4));

			AssertEquals("Detected existing loop", "Management Cycle Detected: MG3,MG2,MG1,MG3", ex.Message);
		}

		public void TestTrigger_ExistingLoops_SelfManagement()
		{
			var mg1 = Guid.NewGuid();
			var mg2 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{mg1}', 'MG1', 'MG1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg2}', 'MG2', 'MG2', GETUTCDATE(), 'E', GETUTCDATE(), 'E')"));

			TestConnection.ExecuteNonQuery($@"DISABLE TRIGGER [dbo].[{nameof(TG_ManagerTableCyclicalDependencyDetectionTrigger)}] ON [dbo].[{GlbStaffManagerSchema.Constants.TableName}]");

			MakeManager(mg1, mg1);

			TestConnection.ExecuteNonQuery($@"ENABLE TRIGGER [dbo].[{nameof(TG_ManagerTableCyclicalDependencyDetectionTrigger)}] ON [dbo].[{GlbStaffManagerSchema.Constants.TableName}]");

			var ex = AssertExceptionThrown<SqlException>(() => MakeManager(mg1, mg2));

			AssertEquals("Detected existing self manage loop", "Management Cycle Detected: MG1,MG1", ex.Message);
		}

		public void TestTrigger_ExistingDeepLoops()
		{
			var mg1 = Guid.NewGuid();
			var mg2 = Guid.NewGuid();
			var mg3 = Guid.NewGuid();
			var mg4 = Guid.NewGuid();
			var mg5 = Guid.NewGuid();
			var mg6 = Guid.NewGuid();
			var mg7 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{mg1}', 'MG1', 'MG1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg2}', 'MG2', 'MG2', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg3}', 'MG3', 'MG3', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg4}', 'MG4', 'MG4', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg5}', 'MG5', 'MG5', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg6}', 'MG6', 'MG6', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg7}', 'MG7', 'MG7', GETUTCDATE(), 'E', GETUTCDATE(), 'E')"));

			TestConnection.ExecuteNonQuery($@"DISABLE TRIGGER [dbo].[{nameof(TG_ManagerTableCyclicalDependencyDetectionTrigger)}] ON [dbo].[{GlbStaffManagerSchema.Constants.TableName}]");

			MakeManager(mg1, mg2);
			MakeManager(mg2, mg3);
			MakeManager(mg3, mg2);
			MakeManager(mg3, mg4);
			MakeManager(mg4, mg5);
			MakeManager(mg5, mg6);

			TestConnection.ExecuteNonQuery($@"ENABLE TRIGGER [dbo].[{nameof(TG_ManagerTableCyclicalDependencyDetectionTrigger)}] ON [dbo].[{GlbStaffManagerSchema.Constants.TableName}]");

			var ex = AssertExceptionThrown<SqlException>(() => MakeManager(mg6, mg7));

			AssertEquals("Detected existing loop", "Management Cycle Detected: MG6,MG5,MG4,MG3,MG2,MG3", ex.Message);
		}

		public void TestInsertMultipleManagers_withNoCycles()
		{
			var loggedInStaff = Guid.NewGuid();
			var potentialManager1 = Guid.NewGuid();
			var potentialManager2 = Guid.NewGuid();
			var potentialManager3 = Guid.NewGuid();

			var setupData = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'NAH', 'NAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{potentialManager1}', 'NUH', 'NUH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{potentialManager2}', 'NEH', 'NEH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{potentialManager3}', 'NOH', 'NOH', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values 
					(newid(), 'PPL', '{potentialManager1}', '{loggedInStaff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E')
			");
			TestConnection.ExecuteNonQuery(setupData);

			var testQuery = FormattableString.Invariant($@"
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values
					(newid(), 'PPL', '{potentialManager2}', '{loggedInStaff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'HRM', '{potentialManager3}', '{loggedInStaff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'DRM', '{potentialManager1}', '{potentialManager2}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{potentialManager3}', '{potentialManager2}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'HRM', '{potentialManager3}', '{potentialManager1}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E')
			");

			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(testQuery));
		}

		public void TestUpdateMultipleManagers_withNoCycles()
		{
			var loggedInStaff = Guid.NewGuid();
			var potentialManager1 = Guid.NewGuid();
			var potentialManager2 = Guid.NewGuid();
			var potentialManager3 = Guid.NewGuid();

			var setupData = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'NAH', 'NAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{potentialManager1}', 'NUH', 'NUH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{potentialManager2}', 'NEH', 'NEH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{potentialManager3}', 'NOH', 'NOH', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values 
					(newid(), 'PPL', '{potentialManager1}', '{loggedInStaff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{potentialManager2}', '{loggedInStaff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{potentialManager3}', '{loggedInStaff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E')
			");
			TestConnection.ExecuteNonQuery(setupData);

			var testQuery = FormattableString.Invariant($@"
				update dbo.GlbStaffManager
				set GSM_ManagerType = 'DRM', GSM_SystemLastEditUser = 'E', GSM_SystemLastEditTimeUtc = GETDATE()
			");

			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(testQuery));
		}

		public void TestManySeperateInserts_WithNullEndDates()
		{
			var mg1 = Guid.NewGuid();
			var mg2 = Guid.NewGuid();
			var mg3 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{mg1}', 'MG1', 'MG1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg2}', 'MG2', 'MG2', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{mg3}', 'MG3', 'MG3', GETUTCDATE(), 'E', GETUTCDATE(), 'E')"));

			MakeManager(mg2, mg1);
			MakeManager(mg3, mg2);
			AssertExceptionThrown<SqlException>(() => MakeManager(mg1, mg3));
		}

		void MakeManager(Guid managerPk, Guid staffPk, string type = "PPL", DateTime? startDate = null, DateTime? endDate = null)
		{
			TestConnection.ExecuteNonQuery(@"
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values
					(NEWID(), @type, @manager, @staff, @effectiveDate, @endDate, GETDATE(), GETDATE(), 'E', 'E')", p =>
			{
				p.AddParameterBasedOnDbColumn("@type", type, GlbStaffManagerSchema.GSM_ManagerType);
				p.AddParameterBasedOnDbColumn("@manager", managerPk, GlbStaffManagerSchema.GSM_GS_Manager);
				p.AddParameterBasedOnDbColumn("@staff", staffPk, GlbStaffManagerSchema.GSM_GS_Staff);
				p.AddParameterBasedOnDbColumn("@effectiveDate", startDate ?? DateTime.Now, GlbStaffManagerSchema.GSM_EffectiveDate);
				p.AddParameterBasedOnDbColumn("@endDate", (object)endDate ?? DBNull.Value, GlbStaffManagerSchema.GSM_EndDate);
			});
		}

		public void TestInsertMultipleManagers_withDeepCycle()
		{
			var loggedInStaff = Guid.NewGuid();
			var potentialManager1 = Guid.NewGuid();
			var potentialManager2 = Guid.NewGuid();
			var potentialManager3 = Guid.NewGuid();
			var potentialManager4 = Guid.NewGuid();

			var setupData = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'NAH', 'NAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{potentialManager1}', 'NUH', 'NUH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{potentialManager2}', 'NEH', 'NEH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{potentialManager3}', 'NOH', 'NOH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{potentialManager4}', 'NOO', 'NOO', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values 
					(newid(), 'PPL', '{potentialManager1}', '{loggedInStaff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E')
			");
			TestConnection.ExecuteNonQuery(setupData);

			var testQuery = FormattableString.Invariant($@"
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values
					(newid(), 'PPL', '{potentialManager2}', '{potentialManager1}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{potentialManager3}', '{potentialManager2}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{potentialManager4}', '{potentialManager3}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{loggedInStaff}', '{potentialManager4}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E')
			");

			AssertExceptionThrown(typeof(SqlException), () => TestConnection.ExecuteNonQuery(testQuery));
		}

		public void TestUpdateMultipleManagers_withDeepCycle()
		{
			var nah = Guid.NewGuid();
			var nuh = Guid.NewGuid();
			var neh = Guid.NewGuid();
			var noh = Guid.NewGuid();
			var noo = Guid.NewGuid();

			var rowChangePk = Guid.NewGuid();

			var setupData = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{nah}', 'NAH', 'NAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{nuh}', 'NUH', 'NUH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{neh}', 'NEH', 'NEH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{noh}', 'NOH', 'NOH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{noo}', 'NOO', 'NOO', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values
					(newid(), 'PPL', '{nuh}', '{nah}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{neh}', '{nuh}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{noh}', '{neh}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					('{rowChangePk}', 'DRM', '{noo}', '{noh}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{nah}', '{noo}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E')
			");
			TestConnection.ExecuteNonQuery(setupData); // NUH, NEH, NOH, NEH, NOH   

			var testQuery = FormattableString.Invariant($@"
				update dbo.GlbStaffManager
				set GSM_ManagerType = 'PPL', GSM_SystemLastEditUser = 'E', GSM_SystemLastEditTimeUtc = GetDate()
				where GSM_PK = '{rowChangePk}'
			");

			AssertExceptionThrown(typeof(SqlException), () => TestConnection.ExecuteNonQuery(testQuery));
		}

		protected override void SetUp()
		{
			base.SetUp();
			ClearTable(GlbStaffManagerSchema.Constants.TableName);
			ClearTable(GlbStaffSchema.Constants.TableName);
		}

		static void ClearTable(string tableName)
			=> Db.Connection.ExecuteNonQuery("DELETE FROM " + tableName);
	}
}
