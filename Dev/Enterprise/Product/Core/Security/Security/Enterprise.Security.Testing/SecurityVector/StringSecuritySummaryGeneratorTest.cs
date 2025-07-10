using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Security.Testing
{
	sealed class StringSecuritySummaryGeneratorTest : TestCaseWithFactory
	{
		public void TestSummary_WithNoGroups()
		{
			department1 = Factory.New<GlbDepartment>();
			department2 = Factory.New<GlbDepartment>();

			department1.GE_Code = "DD1";
			department2.GE_Code = "DD2";

			company1 = Factory.New<GlbCompany>();
			company1Branch1 = company1.Branches.AddNew();
			company1Branch2 = company1.Branches.AddNew();

			company2 = Factory.New<GlbCompany>();
			company2Branch1 = company2.Branches.AddNew();
			company2Branch2 = company2.Branches.AddNew();

			company1.GC_Code = "C1~";
			company2.GC_Code = "C2~";

			company1Branch1.GB_Code = "C11";
			company1Branch2.GB_Code = "C12";
			company2Branch1.GB_Code = "C21";
			company2Branch2.GB_Code = "C22";

			Staff.GS_IsActive = false;
			Staff.Groups.RemoveAll();

			var security_F = CreateSecurity(null, null, null, false);
			var securityC1_T = CreateSecurity(company1, null, null, true);
			var securityC2_F = CreateSecurity(company2, null, null, false);
			var securityC1D1_T = CreateSecurity(company1, null, department1, true); //when entering a department, must also enter a company or branch
			var securityC1D1_F = CreateSecurity(company1, null, department1, false); //when entering a department, must also enter a company or branch
			var securityC1D2_T = CreateSecurity(company1, null, department2, true); //when entering a department, must also enter a company or branch
			var securityC1D2_F = CreateSecurity(company1, null, department2, false); //when entering a department, must also enter a company or branch
			var securityD1B11_T = CreateSecurity(null, company1Branch1, department1, true);
			var securityD2B22_F = CreateSecurity(null, company2Branch2, department2, false);

			AssertSummary("Denied", null);
			AssertSummary("Denied", System.Array.Empty<GlbSecurity>());
			AssertSummary("Denied", security_F);

			AssertSummary("Granted only for logins to (*, C11 | *, C12)", security_F, securityC1_T);
			AssertSummary("Denied", security_F, securityC2_F);
			AssertSummary("Granted only for logins to (*, C11 | *, C12)", securityC1_T);
			AssertSummary("Denied", securityC2_F);

			AssertSummary("Granted only for logins to (DD1, C11 | DD1, C12)", security_F, securityC1D1_T);
			AssertSummary("Denied", security_F, securityC1D2_F);
			AssertSummary("Granted only for logins to (DD1, C11 | DD1, C12)", securityC1D1_T);
			AssertSummary("Denied", securityC1D2_F);

			AssertSummary("Granted only for logins to (*, C11 | *, C12)", security_F, securityC1_T, securityC1D1_T);
			AssertSummary("Denied", securityC2_F, securityC1D2_F);
			AssertSummary("Granted only for logins to (DD1, C11 | DD1, C12)", security_F, securityC1_T, securityC1D1_T, securityC2_F, securityC1D2_F);
			AssertSummary("Granted only for logins to (DD1, C11 | DD1, C12)", securityC1_T, securityC1D1_T, securityC2_F, securityC1D2_F);
			AssertSummary("Granted only for logins to (DD1, C11 | DD1, C12)", security_F, securityC1D1_T, securityC2_F);
			AssertSummary("Granted only for logins to (DD1, C11 | DD1, C12)", securityC1_T, securityC1D2_F);

			AssertSummary("Granted only for logins to (DD1, C11)", security_F, securityD1B11_T);
			AssertSummary("Denied", securityD2B22_F);

			AssertSummary("Granted only for logins to (DD1, C11 | DD2, C11 | DD2, C12)", security_F, securityD1B11_T, securityC1D2_T);
			AssertSummary("Denied", securityD2B22_F, securityC1D1_F);
		}

		public void TestSummary_WithGroupsImplicitlyGranting()
		{
			department1 = Factory.New<GlbDepartment>();
			department2 = Factory.New<GlbDepartment>();

			department1.GE_Code = "DD1";
			department2.GE_Code = "DD2";

			company1 = Factory.New<GlbCompany>();
			company1Branch1 = company1.Branches.AddNew();
			company1Branch2 = company1.Branches.AddNew();

			company2 = Factory.New<GlbCompany>();
			company2Branch1 = company2.Branches.AddNew();
			company2Branch2 = company2.Branches.AddNew();

			company1.GC_Code = "C1~";
			company2.GC_Code = "C2~";

			company1Branch1.GB_Code = "C11";
			company1Branch2.GB_Code = "C12";
			company2Branch1.GB_Code = "C21";
			company2Branch2.GB_Code = "C22";

			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			Staff.Groups.Add(group);

			var security_F = CreateSecurity(null, null, null, false);
			var securityC1_T = CreateSecurity(company1, null, null, true);
			var securityC2_F = CreateSecurity(company2, null, null, false);
			var securityC1D1_T = CreateSecurity(company1, null, department1, true); //when entering a department, must also enter a company or branch
			var securityC1D1_F = CreateSecurity(company1, null, department1, false); //when entering a department, must also enter a company or branch
			var securityC1D2_T = CreateSecurity(company1, null, department2, true); //when entering a department, must also enter a company or branch
			var securityC1D2_F = CreateSecurity(company1, null, department2, false); //when entering a department, must also enter a company or branch
			var securityD1B11_T = CreateSecurity(null, company1Branch1, department1, true);
			var securityD2B22_F = CreateSecurity(null, company2Branch2, department2, false);

			AssertSummary("Denied", null);
			AssertSummary("Granted", System.Array.Empty<GlbSecurity>());
			AssertSummary("Denied", security_F);

			AssertSummary("Granted only for logins to (*, C11 | *, C12)", security_F, securityC1_T);
			AssertSummary("Denied", security_F, securityC2_F);
			AssertSummary("Granted", securityC1_T);
			AssertSummary("Granted, except for logins to (*, C21 | *, C22)", securityC2_F);

			AssertSummary("Granted only for logins to (DD1, C11 | DD1, C12)", security_F, securityC1D1_T);
			AssertSummary("Denied", security_F, securityC1D2_F);
			AssertSummary("Granted", securityC1D1_T);
			AssertSummary("Granted, except for logins to (DD2, C11 | DD2, C12)", securityC1D2_F);

			AssertSummary("Granted only for logins to (*, C11 | *, C12)", security_F, securityC1_T, securityC1D1_T);
			AssertSummary("Granted, except for logins to (*, C21 | *, C22 | DD2, C11 | DD2, C12)", securityC2_F, securityC1D2_F);
			AssertSummary("Granted only for logins to (DD1, C11 | DD1, C12)", security_F, securityC1_T, securityC1D1_T, securityC2_F, securityC1D2_F);
			AssertSummary("Granted, except for logins to (*, C21 | *, C22 | DD2, C11 | DD2, C12)", securityC1_T, securityC1D1_T, securityC2_F, securityC1D2_F);
			AssertSummary("Granted only for logins to (DD1, C11 | DD1, C12)", security_F, securityC1D1_T, securityC2_F);
			AssertSummary("Granted, except for logins to (DD2, C11 | DD2, C12)", securityC1_T, securityC1D2_F);

			AssertSummary("Granted only for logins to (DD1, C11)", security_F, securityD1B11_T);
			AssertSummary("Granted, except for logins to (DD2, C22)", securityD2B22_F);

			AssertSummary("Granted only for logins to (DD1, C11 | DD2, C11 | DD2, C12)", security_F, securityD1B11_T, securityC1D2_T);
			AssertSummary("Granted, except for logins to (DD1, C11 | DD1, C12 | DD2, C22)", securityD2B22_F, securityC1D1_F);
		}

		public void TestGenerateLocalAdministratorSummary()
		{
			var group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "G~1";
			var group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "G~2";
			var staffA = Factory.New<GlbStaff>();
			staffA.GS_Code = "S~A";
			var staffB = Factory.New<GlbStaff>();
			staffB.GS_Code = "S~B";

			var security1 = Helper.CreateSecurity(Env.Security.FindOrCreateChangeGroupSecurityCheckpoint(group1.PK.ToGuid()), Staff, null, null, null, true);
			var security2 = Helper.CreateSecurity(Env.Security.FindOrCreateChangeGroupSecurityCheckpoint(group2.PK.ToGuid()), Staff, null, null, null, true);
			var security3 = Helper.CreateSecurity(Env.Security.FindOrCreateChangeStaffSecurityCheckpoint(staffA.PK.ToGuid()), Staff, null, null, null, true);
			var security4 = Helper.CreateSecurity(Env.Security.FindOrCreateChangeStaffSecurityCheckpoint(staffB.PK.ToGuid()), Staff, null, null, null, true);

			AssertLocalAdministratorSummary("Denied");
			AssertLocalAdministratorSummary("Granted for Group - G~1", security1);
			AssertLocalAdministratorSummary("Granted for Group - G~1, G~2", security1, security2);
			AssertLocalAdministratorSummary("Granted for Staff - S~A", security3);
			AssertLocalAdministratorSummary("Granted for Staff - S~A, S~B", security3, security4);
			AssertLocalAdministratorSummary("Granted for Group - G~1 | Staff - S~A", security1, security3);
			AssertLocalAdministratorSummary("Granted for Group - G~1, G~2 | Staff - S~A, S~B", security1, security2, security3, security4);
			AssertLocalAdministratorSummary("Granted for Group - G~2 | Staff - S~B", security2, security2, security4, security4);
		}

		void AssertSummary(string expectedSummary, params GlbSecurity[] securities)
		{
			string[] departments = { "", department1.GE_Code, department2.GE_Code };
			string[] branches = { "", company1Branch1.GB_Code, company1Branch2.GB_Code, company2Branch1.GB_Code, company2Branch2.GB_Code };
			ZGuid[] departmentsGuids = { ZGuid.Empty, department1.PK, department2.PK };
			ZGuid[] branchesGuids = { ZGuid.Empty, company1Branch1.PK, company1Branch2.PK, company2Branch1.PK, company2Branch2.PK };
			//use addrange with every passed security
			GlbSecurityCollection securityCollection = new GlbSecurityCollection(Factory);
			if (securities != null && securities.Length > 0)
			{
				securityCollection.AddRange(securities);
			}
			var securityCalculator = new SecurityCalculatorForStaff(Factory, securityCollection, new[] { Staff });
			var summary = new SecuritySummary<string>(Env.Security.Operations, Staff, Generator.GenerateSummary(Staff, departmentsGuids, branchesGuids, departments, branches, securities, securityCalculator, Env.Security.Operations, true, true), true);
			AssertEquals(expectedSummary, summary.Summary);
		}

		void AssertLocalAdministratorSummary(string expectedSummary, params GlbSecurity[] securities)
		{
			var summary = new SecuritySummary<string>(Env.Security.StaffLocalAdministratorPlaceholder, Staff, Generator.GenerateLocalAdministratorSummary(Staff, securities), true);
			AssertEquals(expectedSummary, summary.Summary);
		}

		GlbSecurity CreateSecurity(GlbCompany company, GlbBranch branch, GlbDepartment department, bool isAllowed)
		{
			return Helper.CreateSecurity(Env.Security.Operations, Staff, company, branch, department, isAllowed);
		}

		SecurityTestHelper Helper
		{
			get { return (helper = helper ?? new SecurityTestHelper(Factory)); }
		}
		SecurityTestHelper helper;

		StringSecuritySummaryGenerator Generator
		{
			get
			{
				if (generator == null)
				{
					generator = new StringSecuritySummaryGenerator();
				}

				return generator;
			}
		}
		StringSecuritySummaryGenerator generator;

		GlbStaff Staff
		{
			get
			{
				if (staff == null)
				{
					staff = Factory.New<GlbStaff>();
				}

				return staff;
			}
		}
		GlbStaff staff;

		GlbDepartment department1;
		GlbDepartment department2;
		GlbCompany company1;
		GlbCompany company2;
		GlbBranch company1Branch1;
		GlbBranch company1Branch2;
		GlbBranch company2Branch1;
		GlbBranch company2Branch2;
	}
}
