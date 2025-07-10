using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(ManagerTableCyclicalDependencyDetection))]
	class ManagerTableCyclicalDependencyDetectionTest : DbCreateScriptTest
	{
		DataTable Execute(Guid loggedInStaff, DateTime startTime, DateTime endTime, string managerType = "PPL")
		{
			using (var command = TestConnection.Command($@"SELECT * FROM ManagerTableCyclicalDependencyDetection(@loggedInStaff, @startTime, @endTime, @managerType)"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, loggedInStaff);
				command.AddParameterBasedOnDbColumn("@startTime", startTime, GlbStaffManagerSchema.GSM_EffectiveDate);
				command.AddParameterBasedOnDbColumn("@endTime", endTime, GlbStaffManagerSchema.GSM_EndDate);
				command.AddParameterBasedOnDbColumn("@managerType", managerType, GlbStaffManagerSchema.GSM_ManagerType);

				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		string[] GrabProps(DataTable results)
		{
			var staffCodes = results
				.Select()
				.SelectMany(x => x[0].ToString().Split(','))
				.ToArray();

			return staffCodes;
		}

		public void TestSimpleCycleDetection()
		{
			var loggedInStaff = Guid.NewGuid();
			var otherStaffMember = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'YAH', 'YAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{otherStaffMember}', 'YEH', 'YEH', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values 
					(newid(), 'PPL', '{loggedInStaff}', '{otherStaffMember}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{otherStaffMember}', '{loggedInStaff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E')
			");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff, new DateTime(2021, 07, 01), new DateTime(2022, 1, 1));
			var actual = GrabProps(results);
			AssertContainsExactElementsInExactOrder(new[] { "YEH", "YAH", "YEH" }, actual);
		}

		public void TestDateNoOverlapOnFirstStep()
		{
			var loggedInStaff = Guid.NewGuid();
			var otherStaffMember = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'NAH', 'NAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{otherStaffMember}', 'NOO', 'NOO', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values 
					(newid(), 'PPL', '{loggedInStaff}', '{otherStaffMember}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{otherStaffMember}', '{loggedInStaff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E')
			");

			TestConnection.ExecuteNonQuery(commandText);

			var results = Execute(loggedInStaff, new DateTime(2010, 07, 01), new DateTime(2012, 01, 01));
			AssertEmpty(results);

			results = Execute(loggedInStaff, new DateTime(2030, 07, 01), new DateTime(2032, 01, 01));
			AssertEmpty(results);
		}

		public void TestDateNoOverlapOnSubsequentRecursionStep_BothNulls()
		{
			var loggedInStaff = Guid.NewGuid();
			var otherStaffMember = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'YAH', 'YAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{otherStaffMember}', 'YEH', 'YEH', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values 
					(newid(), 'PPL', '{loggedInStaff}', '{otherStaffMember}', '2021-12-30', null, GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{otherStaffMember}', '{loggedInStaff}', '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E')
				");
			TestConnection.ExecuteNonQuery(commandText);

			var results = Execute(loggedInStaff, new DateTime(2021, 01, 01), new DateTime(2030, 01, 01));
			var actual = GrabProps(results);
			AssertContainsExactElementsInExactOrder(new[] { "YEH", "YAH", "YEH" }, actual);
		}

		public void TestDateNoOverlapOnSubsequentRecursionStep_OneNull()
		{
			var loggedInStaff = Guid.NewGuid();
			var otherStaffMember = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'NAH', 'NAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{otherStaffMember}', 'NOO', 'NOO', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values 
					(newid(), 'PPL', '{loggedInStaff}', '{otherStaffMember}', '2021-12-30', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{otherStaffMember}', '{loggedInStaff}', '2022-01-01', null, GETDATE(), GETDATE(), 'E', 'E')
				");
			TestConnection.ExecuteNonQuery(commandText);

			var results = Execute(loggedInStaff, new DateTime(2021, 01, 01), new DateTime(2030, 01, 01));
			AssertEmpty(results);
		}

		public void TestDateNoOverlapOnSubsequentRecursionStep_NoNulls()
		{
			var loggedInStaff1 = Guid.NewGuid();
			var otherStaffMember1 = Guid.NewGuid();
			var loggedInStaff2 = Guid.NewGuid();
			var otherStaffMember2 = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff1}', 'NAH', 'NAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{otherStaffMember1}', 'NOO', 'NOO', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{loggedInStaff2}', 'NEH', 'NEH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{otherStaffMember2}', 'NUH', 'NUH', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values 
					(newid(), 'PPL', '{loggedInStaff1}', '{otherStaffMember1}', '2021-12-30', '2022-01-15', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{otherStaffMember1}', '{loggedInStaff1}', '2020-01-01', '2021-01-01', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{loggedInStaff2}', '{otherStaffMember2}', '2021-12-30', '2022-01-15', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{otherStaffMember2}', '{loggedInStaff2}', '2022-01-16', '2022-12-31', GETDATE(), GETDATE(), 'E', 'E')
				");
			TestConnection.ExecuteNonQuery(commandText);

			var results = Execute(loggedInStaff1, new DateTime(2010, 01, 01), new DateTime(2030, 01, 01));
			AssertEmpty(results);

			results = Execute(loggedInStaff2, new DateTime(2010, 01, 01), new DateTime(2030, 01, 01));
			AssertEmpty(results);
		}

		public void TestDeepHierarchyCycleWithDateConstraints()
		{
			var loggedInStaff = Guid.NewGuid();
			var layer1staff = Guid.NewGuid();
			var layer2staff = Guid.NewGuid();
			var layer3staff = Guid.NewGuid();
			var layer4staff = Guid.NewGuid();
			var layer5staff = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'YAH', 'YAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer1staff}', 'YEH', 'YEH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer2staff}', 'YUH', 'YUH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer3staff}', 'YEA', 'YEA', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer4staff}', 'YES', 'YES', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer5staff}', 'YEP', 'YEP', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values 
					(newid(), 'PPL', '{loggedInStaff}', '{layer1staff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{layer1staff}', '{layer2staff}', '2021-08-10', '2021-12-15', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{layer2staff}', '{layer3staff}', '2021-07-12', '2022-01-10', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{layer3staff}', '{layer4staff}', '2021-09-05', '2021-09-20', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{layer4staff}', '{layer5staff}', '2021-08-22', '2021-10-21', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{layer5staff}', '{loggedInStaff}', '2021-09-01', '2021-09-18', GETDATE(), GETDATE(), 'E', 'E')
			");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff, new DateTime(2021, 07, 01), new DateTime(2022, 01, 01));
			var actual = GrabProps(results);
			AssertContainsExactElementsInExactOrder(new[] { "YEP", "YES", "YEA", "YUH", "YEH", "YAH", "YEP" }, actual);
		}

		public void TestManagerTypeRecognition()
		{
			var loggedInStaff = Guid.NewGuid();
			var layer1staff = Guid.NewGuid();
			var layer2staff = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'NAH', 'NAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer1staff}', 'NOO', 'NOO', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer2staff}', 'NUH', 'NUH', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values 
					(newid(), 'PPL', '{loggedInStaff}', '{layer1staff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'DRM', '{layer1staff}', '{layer2staff}', '2021-08-10', '2021-12-15', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{layer2staff}', '{loggedInStaff}', '2021-07-12', '2022-01-10', GETDATE(), GETDATE(), 'E', 'E')
			");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff, new DateTime(2021, 07, 01), new DateTime(2022, 01, 01));
			AssertEmpty(results);
		}

		public void TestHandleNullEndDates()
		{
			var loggedInStaff = Guid.NewGuid();
			var layer1staff = Guid.NewGuid();
			var layer2staff = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'YES', 'YES', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer1staff}', 'YEA', 'YEA', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer2staff}', 'YEH', 'YEH', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values 
					(newid(), 'PPL', '{loggedInStaff}', '{layer1staff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{layer1staff}', '{layer2staff}', '2021-08-10', null, GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{layer2staff}', '{loggedInStaff}', '2021-07-12', null, GETDATE(), GETDATE(), 'E', 'E')
			");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff, new DateTime(2021, 07, 01), new DateTime(2022, 01, 01));
			var actual = GrabProps(results);
			AssertContainsExactElementsInExactOrder(new[] { "YEH", "YEA", "YES", "YEH" }, actual);
		}

		public void TestHandleCEOStaff()
		{
			var loggedInStaff = Guid.NewGuid();
			var layer1staff = Guid.NewGuid();
			var layer2staff = Guid.NewGuid();
			var layer3staff = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'NAH', 'NAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer1staff}', 'NOO', 'NOO', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer2staff}', 'NUH', 'NUH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer3staff}', 'CEO', 'CEO', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values 
					(newid(), 'PPL', '{layer3staff}', '{loggedInStaff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{layer3staff}', '{layer1staff}', '2021-08-10', null, GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{layer3staff}', '{layer2staff}', '2021-07-12', null, GETDATE(), GETDATE(), 'E', 'E')
			");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff, new DateTime(2021, 07, 01), new DateTime(2022, 01, 01));
			AssertEmpty(results);
		}

		public void TestMultipleParentsWithCycle()
		{
			var loggedInStaff = Guid.NewGuid();
			var layer1staff = Guid.NewGuid();
			var layer2staff = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'YEA', 'YEA', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer1staff}', 'YES', 'YES', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer2staff}', 'NAH', 'NAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values 
					(newid(), 'PPL', '{layer1staff}', '{loggedInStaff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{layer2staff}', '{loggedInStaff}', '2021-08-10', null, GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{loggedInStaff}', '{layer1staff}', '2021-12-30', null, GETDATE(), GETDATE(), 'E', 'E')
			");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff, new DateTime(2021, 07, 01), new DateTime(2022, 01, 01));
			var actual = GrabProps(results);
			AssertContainsExactElementsInExactOrder(new[] { "YES", "YEA", "YES" }, actual);
		}

		public void TestHierarchySwitch()
		{
			var loggedInStaff = Guid.NewGuid();
			var layer1staff = Guid.NewGuid();
			var layer2staff = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'NAH', 'NAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer1staff}', 'NOO', 'NOO', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{layer2staff}', 'NUH', 'NUH', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
				values 
					(newid(), 'PPL', '{loggedInStaff}', '{layer1staff}', '2021-07-28', '2021-08-31', GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{layer1staff}', '{loggedInStaff}', '2021-09-01', null, GETDATE(), GETDATE(), 'E', 'E'),
					(newid(), 'PPL', '{layer2staff}', '{loggedInStaff}', '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E')
			");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff, new DateTime(2021, 07, 01), new DateTime(2022, 01, 01));
			AssertEmpty(results);
		}

		public void TestSimpleCycleDetection_IsApproved_False_DirectManager()
		{
			var loggedInStaff = Guid.NewGuid();
			var manager = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'YAH', 'YAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{manager}', 'YEH', 'YEH', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser, GSM_IsApproved)
				values 
					(newid(), 'PPL', '{loggedInStaff}', '{manager}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E', 0),
					(newid(), 'PPL', '{manager}', '{loggedInStaff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E', 1)
			");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff, new DateTime(2021, 07, 01), new DateTime(2022, 1, 1));
			AssertEmpty(results);
		}

		public void TestSimpleCycleDetection_IsApproved_False_IndirectManager()
		{
			var loggedInStaff = Guid.NewGuid();
			var manager1 = Guid.NewGuid();
			var manager2 = Guid.NewGuid();
			var manager3 = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
				insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				values
					('{loggedInStaff}', 'YAH', 'YAH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{manager1}', 'YBH', 'YBH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{manager2}', 'YCH', 'YCH', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
					('{manager3}', 'YDH', 'YDH', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
				insert into dbo.GlbStaffManager
					(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser, GSM_IsApproved)
				values 
					(newid(), 'PPL', '{manager1}', '{loggedInStaff}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E', 1),
					(newid(), 'PPL', '{manager2}', '{manager1}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E', 1),
					(newid(), 'PPL', '{manager3}', '{manager2}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E', 1),
					(newid(), 'PPL', '{loggedInStaff}', '{manager3}', '2021-07-28', '2021-12-31', GETDATE(), GETDATE(), 'E', 'E', 0)
			");

			TestConnection.ExecuteNonQuery(commandText);
			var results = Execute(loggedInStaff, new DateTime(2021, 07, 01), new DateTime(2022, 1, 1));
			AssertEmpty(results);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ClearTable(GlbStaffManagerSchema.Constants.TableName);
			ClearTable(GlbStaffSchema.Constants.TableName);
			TestConnection.ExecuteNonQuery($@"DISABLE TRIGGER [dbo].[{nameof(TG_ManagerTableCyclicalDependencyDetectionTrigger)}] ON [dbo].[{GlbStaffManagerSchema.Constants.TableName}]");
		}

		static void ClearTable(string tableName)
			=> Db.Connection.ExecuteNonQuery("DELETE FROM " + tableName);

		void AssertEmpty(DataTable table)
		{
			var errorMsg = string.Join(Environment.NewLine, table
				.Select()
				.Select((row, index) => $"{index}: {string.Join(", ", row.ItemArray.Select(UsefulName))}"));

			Assert(errorMsg, string.IsNullOrEmpty(errorMsg));

			string UsefulName(object o)
			{
				if (o is null || o is DBNull)
				{
					return "NULL";
				}

				if (o is string s)
				{
					return $"'{s}'";
				}

				return o.ToString();
			}
		}
	}
}
