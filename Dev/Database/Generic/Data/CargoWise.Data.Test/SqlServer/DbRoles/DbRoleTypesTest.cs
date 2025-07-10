using System.Linq;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbRoleTypesTest : TestCase
	{
		public void TestDbRoleTypes()
		{
			AssertEquals("db_owner", DbRoleTypes.DbOwnerRole);
			AssertEquals("db_datareader", DbRoleTypes.DbDataReaderRole);
			AssertEquals("cwReaderRole", DbRoleTypes.CwReaderRole);

			AssertEquals("db_backupoperator", DbRoleTypes.DbBackupOperatorRole);
			AssertEquals("db_datawriter", DbRoleTypes.DbDataWriterRole);

			AssertEquals("cwRestrictedReaderRole", DbRoleTypes.CwRestrictedReaderRole);
			AssertEquals("cwRestrictedWriterRole", DbRoleTypes.CwRestrictedWriterRole);
			AssertEquals("cwHRMStaffRole", DbRoleTypes.CwHRMStaffRole);
			AssertEquals("cwUnrestrictedWriterRole", DbRoleTypes.CwUnrestrictedWriterRole);
		}

		public void TestAllDbRoles()
		{
			var allRoles = DbRoleTypes.AllDbRoles.ToArray();

			AssertEquals(5, allRoles.Length);
			AssertType<CwReaderRole>(allRoles[0]);
			AssertType<CwRestrictedReaderRole>(allRoles[1]);
			AssertType<CwRestrictedWriterRole>(allRoles[2]);
			AssertType<CwHRMStaffRole>(allRoles[3]);
			AssertType<CwUnrestrictedWriterRole>(allRoles[4]);
		}

		public void TestCwMsdbAccessDeniedRole()
		{
			AssertType<CwMsdbAccessDeniedRole>(DbRoleTypes.CwMsdbAccessDeniedRole);
		}
	}
}
