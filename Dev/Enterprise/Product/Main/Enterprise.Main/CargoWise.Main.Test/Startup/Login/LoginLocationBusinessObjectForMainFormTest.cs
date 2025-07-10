using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(LoginLocationBusinessObjectForMainForm))]
	sealed class LoginLocationBusinessObjectForMainFormTest : LoginLocationBusinessObjectTest
	{
		public void TestDefaults()
		{
			var bizo = CreateNewLoginObject();
			AssertEquals(Env.CurrentCompany.Code, bizo.CompanyCode);
			AssertEquals(Env.CurrentBranch.Code, bizo.BranchCode);
			AssertEquals(Env.CurrentDepartment.Code, bizo.DepartmentCode);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var security1 = Factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = staff.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			using (Env.SetTemporaryUserContext(null))
			using (LoginDirector.UseTestInstance())
			{
				bizo = CreateNewLoginObject();
				AssertNotEquals("", bizo.CompanyCode);
				AssertNotEquals("", bizo.BranchCode);
				AssertNotEquals("", bizo.DepartmentCode);

				LoginDirector.Instance.AuthenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(staff);
				bizo = CreateNewLoginObject();
				AssertNotEquals("", bizo.CompanyCode);
				AssertNotEquals("", bizo.BranchCode);
				AssertNotEquals("", bizo.DepartmentCode);

				staff.GS_GB_HomeBranch = branch1.PK;
				staff.GS_GE_HomeDepartment = department1.PK;
				bizo = CreateNewLoginObject();
				AssertEquals(company1.GC_Code, bizo.CompanyCode);
				AssertEquals(branch1.GB_Code, bizo.BranchCode);
				AssertEquals(department1.GE_Code, bizo.DepartmentCode);

				staff.GS_GB_LastLogonBranch = branch2.PK;
				staff.GS_GE_LastLogonDepartment = department2.PK;
				bizo = CreateNewLoginObject();
				AssertEquals(company2.GC_Code, bizo.CompanyCode);
				AssertEquals(branch2.GB_Code, bizo.BranchCode);
				AssertEquals(department2.GE_Code, bizo.DepartmentCode);
			}
		}

		public void TestSelectingCompanySelectsBranchIfOnlyOneBranch()
		{
			var loginObject = CreateNewLoginObject();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var security1 = Factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = staff.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;
			branch3.GB_GC = company2.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(null))
			using (LoginDirector.UseTestInstance())
			{
				LoginDirector.Instance.AuthenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(staff);

				loginObject.BranchCode = ZString.Empty;
				loginObject.CompanyCode = company1.GC_Code;
				AssertEquals(branch1.GB_Code, loginObject.BranchCode);

				loginObject.CompanyCode = company2.GC_Code;
				AssertEquals(ZString.Empty, loginObject.BranchCode);

				loginObject.BranchCode = branch2.GB_Code;
				loginObject.CompanyCode = company2.GC_Code;
				AssertEquals(branch2.GB_Code, loginObject.BranchCode);

				loginObject.CompanyCode = company1.GC_Code;
				AssertEquals(branch1.GB_Code, loginObject.BranchCode);
			}
		}

		public override void TestAllowedBranchList()
		{
			DataRegistry.Instance.ShowAvailableBranchesOnly = true;
			base.TestAllowedBranchList();
		}

		protected override void AssertDepartmentsForUser(string userLogin, params string[] expectedDepartments)
		{
			SetupUser(Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, userLogin)), LoginObj);
			var departments = Factory.Load<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, new string[] { "D1", "D2", "D3" })); // will always show all departments
			AssertContainsExactElementsInAnyOrder(userLogin + " departments",
				departments.Select(d => d.GE_Code),
				LoginObj.Departments.Select(d => d.GE_Code));
		}

		[StressTest]
		public void TestAllowedBranchList_ShowAllBranches()
		{
			PrepareTestData();
			DataRegistry.Instance.ShowAvailableBranchesOnly = false;
			AssertBranchesForUser("User 3 (granted group)", "Branch 1", "Branch 2", "Branch 3");
			AssertBranchesForUser("User 4 (denied group)", "Branch 1", "Branch 2", "Branch 3");
			AssertBranchesForUser("User 9 (1+ 2= 3-)", "Branch 1", "Branch 2", "Branch 3");
			AssertBranchesForUser("User 11 (all denied)", "Branch 1", "Branch 2", "Branch 3");
			AssertBranchesForUser("User 12 (1+)", "Branch 1", "Branch 2", "Branch 3");
			AssertBranchesForUser("adminXXX", "Branch 1", "Branch 2", "Branch 3");
		}

		public void TestShowAvailableDepartmentsOnly()
		{
			PrepareTestData();

			base.LoginObj.BranchCode = "B1";
			base.LoginObj.DepartmentCode = "D1";
			DataRegistry.Instance.ShowAvailableDepartmentsOnly = true;
			base.AssertDepartmentsForUser("User 15 (D1 denied)", "D2", "D3");

			DataRegistry.Instance.ShowAvailableDepartmentsOnly = false;
			base.AssertDepartmentsForUser("User 15 (D1 denied)", "D1", "D2", "D3");

			DataRegistry.Instance.ShowAvailableDepartmentsOnly = true;
			base.AssertDepartmentsForUser("User 16 (D1 denied, but I'm a controller)", "D1", "D2", "D3");
		}

		protected override void SetUp()
		{
			loginDirectorTestInstance = LoginDirector.UseTestInstance();
			base.SetUp();
		}

		protected override void TearDown()
		{
			loginDirectorTestInstance.Dispose();
			base.TearDown();
		}

		IDisposable loginDirectorTestInstance;

		protected override LoginLocationBusinessObject CreateNewLoginObject()
		{
			return new LoginLocationBusinessObjectForMainForm(Factory);
		}

		protected override void SetupUser(GlbStaff user, LoginLocationBusinessObject loginObj)
		{
			LoginDirector.Instance.AuthenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(user);
		}

		protected override void ResetLoginUser()
		{
			LoginDirector.Instance.AuthenticatedUser = null;
		}
	}
}
