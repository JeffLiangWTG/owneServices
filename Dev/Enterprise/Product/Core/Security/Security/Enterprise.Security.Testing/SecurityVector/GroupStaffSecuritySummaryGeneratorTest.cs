using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Security.Testing
{
	sealed class GroupStaffSecuritySummaryGeneratorTest : TestCaseWithFactory
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
			var securityD1B11_T = CreateSecurity(null, company1Branch1, department1, true);
			var securityD2B22_F = CreateSecurity(null, company2Branch2, department2, false);

			AssertSummary(string.Empty, null);
			AssertSummary(string.Empty, System.Array.Empty<GlbSecurity>());
			AssertSummary(string.Empty, security_F);

			AssertSummary("*StaffProfile", security_F, securityC1_T);
			AssertSummary(string.Empty, security_F, securityC2_F);
			AssertSummary("*StaffProfile", securityC1_T);
			AssertSummary(string.Empty, securityC2_F);

			AssertSummary("*StaffProfile", security_F, securityC1D1_T);
			AssertSummary("*StaffProfile", securityC1D1_T);

			AssertSummary("*StaffProfile", security_F, securityC1_T, securityC1D1_T);
			AssertSummary("*StaffProfile", security_F, securityC1D1_T, securityC2_F);

			AssertSummary("*StaffProfile", security_F, securityD1B11_T);
			AssertSummary(string.Empty, securityD2B22_F);

			AssertSummary(string.Empty, securityD2B22_F, securityC1D1_F);
		}

		public void TestSummary_WithGroupsImplicitlyGranting()
		{
			department1 = Factory.New<GlbDepartment>();
			department2 = Factory.New<GlbDepartment>();

			department1.GE_Code = "DD1";

			company1 = Factory.New<GlbCompany>();
			company1Branch1 = company1.Branches.AddNew();
			company1Branch2 = company1.Branches.AddNew();

			company2 = Factory.New<GlbCompany>();
			company2Branch1 = company2.Branches.AddNew();
			company2Branch2 = company2.Branches.AddNew();

			company1.GC_Code = "C1~";
			company2.GC_Code = "C2~";

			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			Staff.Groups.Add(group);
			//second, non-security group
			GlbGroup group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_IsSales = true;
			Staff.Groups.Add(group2);

			var security_F = CreateSecurity(null, null, null, false);
			var securityC1_T = CreateSecurity(company1, null, null, true);
			var securityC2_F = CreateSecurity(company2, null, null, false);
			var securityC1D2_T = CreateSecurityForGroup(group, company1, null, department1, true); //when entering a department, must also enter a company or branch
			var securityC1D2_F = CreateSecurityForGroup(group, null, null, null, false); //when entering a department, must also enter a company or branch
			CreateSecurityForGroup(group2, company1, null, department1, false); //when entering a department, must also enter a company or branch
			CreateSecurityForGroup(group2, null, null, null, true); //when entering a department, must also enter a company or branch

			var summaryResultWithoutStaff = group.GG_Code + ",ALL";
			var summaryResultWithoutGroup = "*StaffProfile,ALL";
			var summaryResult = "*StaffProfile," + group.GG_Code + ",ALL";

			AssertSummary(string.Empty, null);
			AssertSummary(summaryResultWithoutStaff, System.Array.Empty<GlbSecurity>());
			AssertSummary(string.Empty, security_F);

			AssertSummary(summaryResult, security_F, securityC1_T);
			AssertSummary(summaryResult, securityC1_T);
			AssertSummary(summaryResultWithoutStaff, securityC2_F);

			AssertSummary(string.Empty, security_F, securityC1D2_F);
			AssertSummary("ALL", securityC1D2_F);

			AssertSummary("ALL", securityC2_F, securityC1D2_F);
			AssertSummary(summaryResultWithoutGroup, securityC1_T, securityC1D2_F);
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

		GlbSecurity CreateSecurity(GlbCompany company, GlbBranch branch, GlbDepartment department, bool isAllowed)
		{
			return Helper.CreateSecurity(Env.Security.Operations, Staff, company, branch, department, isAllowed);
		}

		GlbSecurity CreateSecurityForGroup(GlbGroup group, GlbCompany company, GlbBranch branch, GlbDepartment department, bool isAllowed)
		{
			return Helper.CreateSecurity(Env.Security.Operations, group, company, branch, department, isAllowed);
		}

		SecurityTestHelper Helper
		{
			get { return (helper = helper ?? new SecurityTestHelper(Factory)); }
		}
		SecurityTestHelper helper;

		GroupStaffSecuritySummaryGenerator Generator
		{
			get
			{
				if (generator == null)
				{
					generator = new GroupStaffSecuritySummaryGenerator();
				}

				return generator;
			}
		}
		GroupStaffSecuritySummaryGenerator generator;

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
