using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Testing
{
	sealed class SecurityMapTest : TestCaseWithFactory
	{
		public void TestSecurityMapWithGroupsAndDefaultRights()
		{
			GlbGroup group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "G1";
			group1.SecurityPermissions.Add(Helper.CreateSecurity(Env.Security.Organisation, group1, null, null, null, true));
			group1.SecurityPermissions.Add(Helper.CreateSecurity(Env.Security.Countries, group1, null, null, null, false));

			GlbGroup group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "G2";
			group2.SecurityPermissions.RemoveAndDeleteAll();

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.Groups.Add(group1);
			staff.Groups.Add(group2);
			staff.StaffSecurityPermissionsCollection.Add(Helper.CreateSecurity(Env.Security.Forwarding, staff, null, null, null, false));

			SecurityMap map = new SecurityMap();
			GlbStaff staffToChange = Factory.New<GlbStaff>();

			AssertHasAccess(map, staff, staffToChange, Env.Security.Organisation, null, null, null, true); // Organisation right is set on Group 1
			AssertHasAccess(map, staff, staffToChange, Env.Security.OrganisationDelete, null, null, null, true);
			AssertHasAccess(map, staff, staffToChange, Env.Security.Countries, null, null, null, true); // Countries right is defaulted from Group 2
			AssertHasAccess(map, staff, staffToChange, Env.Security.CountriesNew, null, null, null, true);
			AssertHasAccess(map, staff, staffToChange, Env.Security.Forwarding, null, null, null, false); // Forwarding right is explicitly denied on Staff
			AssertHasAccess(map, staff, staffToChange, Env.Security.RoadDistanceCalculationServiceForwarding, null, null, null, false);
		}

		public void TestSecurityMap()
		{
			var department1 = Factory.New<GlbDepartment>();
			var department2 = Factory.New<GlbDepartment>();

			department1.GE_Code = "DD1";
			department2.GE_Code = "DD2";

			var company1 = Factory.New<GlbCompany>();
			var company1Branch1 = company1.Branches.AddNew();
			var company1Branch2 = company1.Branches.AddNew();

			var company2 = Factory.New<GlbCompany>();
			var company2Branch1 = company2.Branches.AddNew();
			var company2Branch2 = company2.Branches.AddNew();

			company1.GC_Code = "C1~";
			company2.GC_Code = "C2~";

			company1Branch1.GB_Code = "C11";
			company1Branch2.GB_Code = "C12";
			company2Branch1.GB_Code = "C21";
			company2Branch2.GB_Code = "C22";

			var company2Branch3 = company2.Branches.AddNew();
			company2Branch3.GB_Code = "C23";
			company2Branch3.GB_IsActive = false; // Inactive branch should not cause problems

			GlbStaff currentUser = Factory.New<GlbStaff>();
			currentUser.StaffSecurityPermissionsCollection.AddRange(
				Helper.CreateSecurity(Env.Security.Warehouse, currentUser, null, null, null, true),
				Helper.CreateSecurity(Env.Security.WhsInventory, currentUser, company1, null, null, false),
				Helper.CreateSecurity(Env.Security.WhsReceive, currentUser, null, company2Branch1, null, false),
				Helper.CreateSecurity(Env.Security.WhsRelease, currentUser, null, company1Branch2, department1, false), //when entering a department, must also enter a company or branch
				Helper.CreateSecurity(Env.Security.WhsStocktake, currentUser, company2, null, department2, false),
				Helper.CreateSecurity(Env.Security.CFSCTO, currentUser, null, null, null, false),
				Helper.CreateSecurity(Env.Security.CFSGatePass, currentUser, null, company1Branch1, null, true) //cannot have a security rule with both company and branch at the same time; it is redundant
				);
			GlbStaff staffToChange = Factory.New<GlbStaff>();
			currentUser.GS_LoginName = "~Test User~";
			currentUser.GS_Code = "~T~";
			staffToChange.GS_LoginName = "~Test User 2~";
			staffToChange.GS_Code = "~2~";

			Factory.Save();

			SecurityMap map = new SecurityMap();

			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsAdjustment, null, null, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsAdjustment, company1, null, department1, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsAdjustment, null, company1Branch1, department1, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsAdjustment, company2, null, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsAdjustment, null, company2Branch2, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsAdjustment, null, null, department2, true);

			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsAdjustmentView, null, null, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsAdjustmentView, company1, null, department1, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsAdjustmentView, null, company1Branch1, department1, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsAdjustmentView, company2, null, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsAdjustmentView, null, company2Branch2, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsAdjustmentView, null, null, department2, true);

			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventory, null, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventory, company1, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventory, company2, null, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventory, null, company1Branch1, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventory, null, company2Branch2, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventory, null, null, department1, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventory, null, company1Branch2, department1, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventory, null, company2Branch1, department2, true);

			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventoryView, null, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventoryView, company1, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventoryView, company2, null, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventoryView, null, company1Branch1, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventoryView, null, company2Branch2, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventoryView, null, null, department1, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventoryView, null, company1Branch2, department1, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsInventoryView, null, company2Branch1, department2, true);

			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceive, null, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceive, company1, null, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceive, company2, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceive, null, company1Branch2, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceive, null, company2Branch1, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceive, null, company2Branch2, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceive, null, null, department1, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceive, null, company1Branch2, department2, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceive, null, company2Branch1, department2, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceive, null, company2Branch2, department2, true);

			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceiveView, null, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceiveView, company1, null, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceiveView, company2, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceiveView, null, company1Branch2, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceiveView, null, company2Branch1, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceiveView, null, company2Branch2, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceiveView, null, null, department1, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceiveView, null, company1Branch2, department2, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceiveView, null, company2Branch1, department2, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReceiveView, null, company2Branch2, department2, true);

			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsRelease, null, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsRelease, company1, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsRelease, null, company2Branch1, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsRelease, null, null, department1, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsRelease, null, null, department2, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsRelease, company1, null, department2, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsRelease, null, company2Branch1, department2, true);

			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReleaseView, null, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReleaseView, company1, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReleaseView, null, company2Branch1, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReleaseView, null, null, department1, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReleaseView, null, null, department2, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReleaseView, company1, null, department2, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsReleaseView, null, company2Branch1, department2, true);

			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktake, null, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktake, company1, null, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktake, company2, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktake, null, company1Branch2, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktake, null, company2Branch1, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktake, null, null, department1, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktake, null, null, department2, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktake, company1, null, department1, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktake, company1, null, department2, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktake, company1, null, department2, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktake, company2, null, department2, false);

			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktakeView, null, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktakeView, company1, null, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktakeView, company2, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktakeView, null, company1Branch2, null, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktakeView, null, company2Branch1, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktakeView, null, null, department1, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktakeView, null, null, department2, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktakeView, company1, null, department1, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktakeView, company1, null, department2, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktakeView, company1, null, department2, true);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.WhsStocktakeView, company2, null, department2, false);

			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSCTO, null, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSCTO, company1, null, department1, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSCTO, null, company1Branch1, department1, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSCTO, company2, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSCTO, null, company2Branch2, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSCTO, null, null, department2, false);

			AssertHasAccess(map, currentUser, staffToChange, Env.Security.RoadDistanceCalculationServiceCFS, null, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.RoadDistanceCalculationServiceCFS, company1, null, department1, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.RoadDistanceCalculationServiceCFS, null, company1Branch1, department1, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.RoadDistanceCalculationServiceCFS, company2, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.RoadDistanceCalculationServiceCFS, null, company2Branch2, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.RoadDistanceCalculationServiceCFS, null, null, department2, false);

			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePass, null, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePass, company1, null, null, false); //not true for company1Branch2
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePass, company2, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePass, null, null, department1, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePass, null, null, department2, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePass, null, company1Branch2, null, false); //not true for company1Branch2
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePass, company1, null, department2, false); //not true for company1Branch2
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePass, null, company1Branch1, null, true);

			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePassModify, null, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePassModify, company1, null, null, false); //not true for company1Branch2
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePassModify, company2, null, null, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePassModify, null, null, department1, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePassModify, null, null, department2, false);
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePassModify, null, company1Branch2, null, false); //not true for company1Branch2
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePassModify, company1, null, department2, false); //not true for company1Branch2
			AssertHasAccess(map, currentUser, staffToChange, Env.Security.CFSGatePassModify, null, company1Branch1, null, true);
		}

		public void TestSecurityMap2()
		{
			var company1 = Factory.New<GlbCompany>();
			var company1Branch1 = company1.Branches.AddNew();
			var company1Branch2 = company1.Branches.AddNew();
			company1Branch1.GB_Code = "C11";
			company1Branch2.GB_Code = "C12";

			GlbStaff currentUser = Factory.New<GlbStaff>();
			currentUser.GS_LoginName = "~Test User~";
			currentUser.GS_Code = "~T~";

			Helper.CreateSecurity(Env.Security.Operations, currentUser, null, null, null, false);
			Helper.CreateSecurity(Env.Security.Operations, currentUser, null, company1Branch1, null, true);
			Helper.CreateSecurity(Env.Security.Operations, currentUser, null, company1Branch2, null, true);
			Helper.CreateSecurity(Env.Security.Forwarding, currentUser, null, null, null, false);

			Factory.Save();

			string[] operations = null;
			string[] forwarding = null;

			SecurityMap map = new SecurityMap();
			foreach (var ss in map.GetStaffSecurity(currentUser))
			{
				if (ss[0] == "Operate")
				{
					operations = ss;
				}
				else if (ss[0] == "Operate -> Forwarding")
				{
					forwarding = ss;
					break;
				}
			}

			AssertEquals("Granted only for logins to (*, C11 | *, C12)", operations[1]);
			AssertEquals("Denied", forwarding[1]);
		}

		void AssertHasAccess(SecurityMap map, GlbStaff currentUser, GlbStaff staffToChange,
			ISecurityCheckpoint checkpoint, GlbCompany company, GlbBranch branch, GlbDepartment department, bool expected)
		{
			AssertEquals(expected, map.HasAccessForAllBranchesAndDepartments(Factory, currentUser.StaffSecurityPermissionsCollection, Env.Security, currentUser,
				Helper.CreateSecurity(checkpoint, staffToChange, company, branch, department, true)));
		}

		SecurityTestHelper Helper
		{
			get { return (helper = helper ?? new SecurityTestHelper(Factory)); }
		}
		SecurityTestHelper helper;
	}
}
