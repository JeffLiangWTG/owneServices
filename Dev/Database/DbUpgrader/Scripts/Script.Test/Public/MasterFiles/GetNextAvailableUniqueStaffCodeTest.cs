using System;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Testing
{
	[TestedType(typeof(GetNextAvailableUniqueStaffCode))]
	class GetNextAvailableUniqueStaffCodeTest : DbCreateScriptTest
	{
		public void TestGetNextAvailableUniqueStaffCode()
		{
			var personPk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($"insert into dbo.GlbPerson (PER_PK, PER_FullName) values ('{personPk}', 'name')");

			TestConnection.Command(string.Format(@"
insert into dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
values(NEWID(), 'AAA', '1', '{0}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
insert into dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
values(NEWID(), 'AAB', '2', '{0}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
insert into dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
values(NEWID(), 'ABA', '3', '{0}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
insert into dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
values(NEWID(), 'ABB', '4', '{0}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
insert into dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
values(NEWID(), 'BAA', '5', '{0}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')", personPk)).
				ExecuteNonQuery();

			AssertEquals("BAB", TestConnection.ExecuteScalar("select dbo.GetNextAvailableUniqueStaffCode('AB', '')").ToString());
			AssertEquals("BAB", TestConnection.ExecuteScalar("select dbo.GetNextAvailableUniqueStaffCode('AB', 'AA')").ToString());
			AssertEquals("BAB", TestConnection.ExecuteScalar("select dbo.GetNextAvailableUniqueStaffCode('AB', 'AAA')").ToString());
			AssertEquals("BAB", TestConnection.ExecuteScalar("select dbo.GetNextAvailableUniqueStaffCode('AB', 'BA')").ToString());
			AssertEquals("BAB", TestConnection.ExecuteScalar("select dbo.GetNextAvailableUniqueStaffCode('AB', 'BAA')").ToString());
			AssertEquals("BBA", TestConnection.ExecuteScalar("select dbo.GetNextAvailableUniqueStaffCode('AB', 'BAB')").ToString());

			TestConnection.Command(string.Format(@"
insert into dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
values(NEWID(), 'BAB', '6', '{0}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
insert into dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
values(NEWID(), 'BBA', '7', '{0}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
insert into dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
values(NEWID(), 'BBB', '8', '{0}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')", personPk)).
				ExecuteNonQuery();

			Assert(string.IsNullOrEmpty(TestConnection.ExecuteScalar("select dbo.GetNextAvailableUniqueStaffCode('AB', '')").ToString().Trim()));
			Assert(string.IsNullOrEmpty(TestConnection.ExecuteScalar("select dbo.GetNextAvailableUniqueStaffCode('AB', 'AAA')").ToString().Trim()));
		}
	}
}

