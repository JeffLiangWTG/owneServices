using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.Testing
{
	sealed class SecurityLocatorTest : TestCaseWithFactory
	{
		#region GetAllBranchesThatStaffHasPermissionsFor

		void TestGetAllBranchesThatStaffHasPermissionsFor(Guid itemGuid)
		{
			var nonOperationalUser = Factory.New<GlbStaff>();
			nonOperationalUser.GS_Code = "XYD";
			nonOperationalUser.GS_LoginName = "XYDLoginName";
			nonOperationalUser.GS_IsOperational = false;

			Db.Connection.ExecuteNonQuery("delete from dbo.GlbGroupRole");
			Db.Connection.ExecuteNonQuery("delete from dbo.GlbGroup");
			TestParameters parameters = new TestParameters();

			parameters.departments = new GlbDepartmentCollection(Factory);

			parameters.staff = Factory.New<GlbStaff>();

			parameters.company1 = Factory.New<GlbCompany>();
			parameters.company1.GC_Code = "C_1";
			parameters.company2 = Factory.New<GlbCompany>();
			parameters.company2.GC_Code = "C_2";

			parameters.department1 = parameters.departments.AddNew();
			parameters.department1.GE_Code = "D_1";
			parameters.department2 = parameters.departments.AddNew();
			parameters.department2.GE_Code = "D_2";

			parameters.company1Branch1 = parameters.company1.Branches.AddNew();
			parameters.company1Branch2 = parameters.company1.Branches.AddNew();
			parameters.company2Branch1 = parameters.company2.Branches.AddNew();
			parameters.company2Branch2 = parameters.company2.Branches.AddNew();

			parameters.company1Branch1.GB_Code = "C11";
			parameters.company1Branch2.GB_Code = "C12";
			parameters.company2Branch1.GB_Code = "C21";
			parameters.company2Branch2.GB_Code = "C22";

			const string parentCheckPointCode = "DUMMY_CHECK_POINT_PARENT";
			const string checkPointCode = "DUMMY_CHECKPOINT";

			Guid parentCheckPointItemGuid = itemGuid;
			Guid checkPointItemGuid = itemGuid;

			parameters.securities = new GlbSecurityCollection(Factory);
			ZSecurity security = (ZSecurity)new ZSecurityFactory().NewSecurityInstance(parameters.securities, GlbStaff.CurrentUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			SecurityCheckpoint parentCheckPoint = new SecurityCheckpoint(parentCheckPointCode, (NoResString)"", null, security, parentCheckPointItemGuid);
			parameters.checkPoint = new SecurityCheckpoint(checkPointCode, (NoResString)"", parentCheckPoint, security, checkPointItemGuid);

			parameters.staff.GS_IsController = true;
			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, parameters.AllBranches, parameters.NoBranches);

			parameters.staff.GS_IsController = false;
			parameters.staff.GS_LoginName = nonOperationalUser.GS_LoginName;
			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, parameters.NoBranches, parameters.NoBranches);

			parameters.staff.GS_LoginName = "bob";
			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, parameters.AllBranches, parameters.NoBranches);

			parameters.group1 = parameters.staff.Groups.AddNew();
			parameters.group2 = parameters.staff.Groups.AddNew();

			parameters.group1.SecurityPermissions.RemoveAndDeleteAll();
			parameters.group2.SecurityPermissions.RemoveAndDeleteAll();
			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, parameters.AllBranches, parameters.NoBranches);

			AddSecuritiesAndAssertResults(parameters, checkPointCode, true, checkPointItemGuid);
			AddSecuritiesAndAssertResults(parameters, parentCheckPointCode, false, parentCheckPointItemGuid);
			AddSecuritiesAndAssertResults(parameters, checkPointCode, false, checkPointItemGuid);

			GlbStaff anotherStaff = Factory.New<GlbStaff>();
			anotherStaff.Groups.RemoveAndDeleteAll();
			parameters.staff = anotherStaff;
			AssertGetAllBranchesThatStaffHasPermissionsFor(anotherStaff, parameters.checkPoint, parameters.AllBranches, parameters.NoBranches);
			parameters.group1 = parameters.staff.Groups.AddNew();
			parameters.group2 = parameters.staff.Groups.AddNew();
			parameters.group1.SecurityPermissions.RemoveAndDeleteAll();
			parameters.group2.SecurityPermissions.RemoveAndDeleteAll();
			AddSecuritiesAndAssertResults(parameters, checkPointCode, parentCheckPointCode, true, checkPointItemGuid);
			AddSecuritiesAndAssertResults(parameters, checkPointCode, parentCheckPointCode, false, checkPointItemGuid);
		}

		public void TestGetAllBranchesThatStaffHasPermissionsFor()
		{
			TestGetAllBranchesThatStaffHasPermissionsFor(Guid.NewGuid());
		}

		//For some weird reason, the below line can't be put in the above method (TestGetAllBranchesThatStaffHasPermissionsFor) 
		//- whichever order the two lines are put, the second one always fails.
		public void TestGetAllBranchesThatStaffHasPermissionsFor_WithEmptyGuid()
		{
			TestGetAllBranchesThatStaffHasPermissionsFor(Guid.Empty);
		}

		#endregion

		#region Implementation

		void AddSecuritiesAndAssertResults(TestParameters parameters, string securityCode, string parentSecurityCode, bool testGroupPermissions, Guid itemGuid)
		{
			string ownerPropertyName = testGroupPermissions ? GlbSecuritySchema.Constants.GU_GG : GlbSecuritySchema.Constants.GU_GS;
			ZGuid ownerPk = testGroupPermissions ? parameters.group1.PK : parameters.staff.PK;
			ZGuid alternateOwnerPk = testGroupPermissions ? parameters.group2.PK : parameters.staff.PK;

			AddSecurity(parameters.securities, ownerPropertyName, ownerPk, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, parentSecurityCode, false, itemGuid);
			if (testGroupPermissions)
			{
				AddSecurity(parameters.securities, ownerPropertyName, alternateOwnerPk, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, parentSecurityCode, false, itemGuid);
			}
			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, parameters.NoBranches, parameters.AllBranches);

			AddSecurity(parameters.securities, ownerPropertyName, ownerPk, ZGuid.Empty, ZGuid.Empty, parameters.company1.PK, parentSecurityCode, true, itemGuid);
			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, parameters.Company1Branches, parameters.Company2Branches);
			AddSecurity(parameters.securities, ownerPropertyName, ownerPk, parameters.department1.PK, ZGuid.Empty, parameters.company1.PK, securityCode, false, itemGuid);
			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, parameters.Company1Branches, parameters.Company2Branches);

			AddSecurity(parameters.securities, ownerPropertyName, ownerPk, ZGuid.Empty, ZGuid.Empty, parameters.company2.PK, parentSecurityCode, true, itemGuid);
			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, parameters.AllBranches, parameters.NoBranches);
		}

		void AddSecuritiesAndAssertResults(TestParameters parameters, string securityCode, bool testGroupPermissions, Guid itemGuid)
		{
			string ownerPropertyName = testGroupPermissions ? GlbSecuritySchema.Constants.GU_GG : GlbSecuritySchema.Constants.GU_GS;
			ZGuid ownerPk = testGroupPermissions ? parameters.group1.PK : parameters.staff.PK;
			ZGuid alternateOwnerPk = testGroupPermissions ? parameters.group2.PK : parameters.staff.PK;

			AddSecurity(parameters.securities, ownerPropertyName, ownerPk, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, securityCode, false, itemGuid);
			if (testGroupPermissions)
			{
				AddSecurity(parameters.securities, ownerPropertyName, alternateOwnerPk, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, securityCode, false, itemGuid);
			}
			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, parameters.NoBranches, parameters.AllBranches);

			AddSecurity(parameters.securities, ownerPropertyName, ownerPk, ZGuid.Empty, ZGuid.Empty, parameters.company1.PK, securityCode, true, itemGuid);
			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, parameters.Company1Branches, parameters.Company2Branches);

			AddSecurity(parameters.securities, ownerPropertyName, alternateOwnerPk, ZGuid.Empty, parameters.company2Branch1.PK, ZGuid.Empty, securityCode, true, itemGuid);
			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, new GlbBranch[] { parameters.company1Branch1, parameters.company1Branch2, parameters.company2Branch1 }, new GlbBranch[] { parameters.company2Branch2 });

			AddSecurity(parameters.securities, ownerPropertyName, ownerPk, parameters.department1.PK, ZGuid.Empty, parameters.company1.PK, securityCode, false, itemGuid);
			AddSecurity(parameters.securities, ownerPropertyName, ownerPk, parameters.department2.PK, ZGuid.Empty, parameters.company1.PK, securityCode, false, itemGuid);
			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, new GlbBranch[] { parameters.company1Branch1, parameters.company1Branch2, parameters.company2Branch1 }, new GlbBranch[] { parameters.company2Branch2 });

			foreach (GlbDepartment department in parameters.departments)
			{
				if (department != parameters.department1 && department != parameters.department2)
				{
					AddSecurity(parameters.securities, ownerPropertyName, ownerPk, department.PK, ZGuid.Empty, parameters.company1.PK, securityCode, false, itemGuid);
				}
			}

			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, new GlbBranch[] { parameters.company2Branch1 }, new GlbBranch[] { parameters.company1Branch1, parameters.company1Branch2, parameters.company2Branch2 });

			AddSecurity(parameters.securities, ownerPropertyName, ownerPk, parameters.department1.PK, parameters.company1Branch2.PK, ZGuid.Empty, securityCode, true, itemGuid);
			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, new GlbBranch[] { parameters.company1Branch2, parameters.company2Branch1 }, new GlbBranch[] { parameters.company1Branch1, parameters.company2Branch2 });

			AddSecurity(parameters.securities, ownerPropertyName, ownerPk, parameters.department2.PK, parameters.company2Branch1.PK, ZGuid.Empty, securityCode, false, itemGuid);
			AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, new GlbBranch[] { parameters.company1Branch2, parameters.company2Branch1 }, new GlbBranch[] { parameters.company1Branch1, parameters.company2Branch2 });

			if (testGroupPermissions)
			{
				parameters.group1.GG_IsActive = false;
				AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, new GlbBranch[] { parameters.company2Branch1 }, new GlbBranch[] { parameters.company1Branch1, parameters.company1Branch2, parameters.company2Branch2 });
				parameters.group2.GG_IsActive = false;
				AssertGetAllBranchesThatStaffHasPermissionsFor(parameters.staff, parameters.checkPoint, parameters.AllBranches, parameters.NoBranches);
				parameters.group1.GG_IsActive = true;
				parameters.group2.GG_IsActive = true;
			}
		}

		void AddSecurity(GlbSecurityCollection securities, string ownerPropertyName, ZGuid ownerPk, ZGuid departmentPk, ZGuid branchPk, ZGuid companyPk, string securityCode, bool isAllowed, Guid itemGuid)
		{
			GlbSecurity security = securities.AddNew();
			security[ownerPropertyName] = ownerPk;
			security.GU_GC = companyPk;
			security.GU_GB = branchPk;
			security.GU_GE = departmentPk;
			security.GU_SecurityRight = securityCode;
			security.GU_ItemGUID = itemGuid;
			security.GU_SecurityItemIsAllowed = isAllowed;
		}

		void AssertGetAllBranchesThatStaffHasPermissionsFor(GlbStaff staff, SecurityCheckpoint checkPoint, GlbBranch[] expectedGrantedBranches, GlbBranch[] expectedDeniedBranches)
		{
			SecurityLocator locator = new SecurityLocator(staff, Factory);
			GlbBranch[] grandedBranches = locator.GetAllBranchesThatStaffHasPermissionsFor(checkPoint);
			GlbBranch[] deniedBranches = locator.GetAllBranchesThatStaffHasPermissionsFor(checkPoint, false);

			foreach (GlbBranch branch in expectedGrantedBranches)
			{
				AssertCollectionContains("Branch with code '" + branch.GB_Code + "' should be granted.", branch, grandedBranches);
				AssertCollectionNotContains("Branch with code '" + branch.GB_Code + "' should not be denied.", branch, deniedBranches);
			}

			foreach (GlbBranch branch in expectedDeniedBranches)
			{
				AssertCollectionNotContains("Branch with code '" + branch.GB_Code + "' should not be granted.", branch, grandedBranches);
				AssertCollectionContains("Branch with code '" + branch.GB_Code + "' should be denied.", branch, deniedBranches);
			}

			AssertEquals(deniedBranches.Length == 0, locator.IsSecurityAllowedForAllBranches(checkPoint));
		}

		#region struct TestParameters

		struct TestParameters
		{
			public GlbBranch[] AllBranches
			{
				get { return new GlbBranch[] { company1Branch1, company1Branch2, company2Branch1, company2Branch2 }; }
			}

			public GlbBranch[] NoBranches
			{
				get { return Array.Empty<GlbBranch>(); }
			}

			public GlbBranch[] Company1Branches
			{
				get { return new GlbBranch[] { company1Branch1, company1Branch2 }; }
			}

			public GlbBranch[] Company2Branches
			{
				get { return new GlbBranch[] { company2Branch1, company2Branch2 }; }
			}

			public GlbStaff staff;
			public GlbGroup group1;
			public GlbGroup group2;
			public GlbCompany company1;
			public GlbCompany company2;
			public GlbBranch company1Branch1;
			public GlbBranch company1Branch2;
			public GlbBranch company2Branch1;
			public GlbBranch company2Branch2;
			public GlbDepartment department1;
			public GlbDepartment department2;
			public GlbDepartmentCollection departments;
			public SecurityCheckpoint checkPoint;
			public GlbSecurityCollection securities;
		}

		#endregion

		#endregion
	}
}
