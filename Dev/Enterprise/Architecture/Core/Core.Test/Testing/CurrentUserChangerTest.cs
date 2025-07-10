using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class CurrentUserChangerTest : TestCase
	{
		public void TestChangingUsers()
		{
			AssertEquals("Precondition: Initial user", User.SupportUserName, EnvProxy.Instance.CurrentUser.LoginName);

			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.WebUserName))
			{
				AssertEquals("EnvProxy.Instance.CurrentUser.LoginName", User.WebUserName, EnvProxy.Instance.CurrentUser.LoginName);

				using (EnvProxy.Instance.SuppressSwitchContextCheck())
				using (CurrentUserChanger.SwitchToNewUserTemporarily(User.PostMasterUserName))
				{
					AssertEquals("EnvProxy.Instance.CurrentUser.LoginName", User.PostMasterUserName, EnvProxy.Instance.CurrentUser.LoginName);
				}

				AssertEquals("EnvProxy.Instance.CurrentUser.LoginName", User.WebUserName, EnvProxy.Instance.CurrentUser.LoginName);
			}

			AssertEquals("EnvProxy.Instance.CurrentUser.LoginName", User.SupportUserName, EnvProxy.Instance.CurrentUser.LoginName);
		}

		public void TestChangingUsers_WithBranchAndDepartment()
		{
			Guid testBranchPK;
			Guid testDepartmentPK;

			using (DbCommand branchCommand = Db.Connection.Command("Select GB_PK from dbo.GlbBranch where GB_Code = @code"))
			{
				branchCommand.AddParameterBasedOnDbColumn("@code", "TES", GlbBranchSchema.GB_Code);
				testBranchPK = (Guid)branchCommand.ExecuteScalar();
			}

			using (DbCommand departmentCommand = Db.Connection.Command("Select GE_PK from dbo.GlbDepartment where GE_Code = @code"))
			{
				departmentCommand.AddParameterBasedOnDbColumn("@code", "TE", GlbDepartmentSchema.GE_Code);
				testDepartmentPK = (Guid)departmentCommand.ExecuteScalar();
			}

			AssertEquals("Precondition: Initial user.", User.SupportUserName, EnvProxy.Instance.CurrentUser.LoginName);

			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.WebUserName, testBranchPK, testDepartmentPK))
			{
				AssertEquals(testBranchPK, EnvProxy.Instance.CurrentBranch.PK);
				AssertEquals(testDepartmentPK, EnvProxy.Instance.CurrentDepartment.PK);
			}

			AssertEquals("EnvProxy.Instance.CurrentUser.LoginName", User.SupportUserName, EnvProxy.Instance.CurrentUser.LoginName);
		}
	}
}
