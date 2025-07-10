using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.Testing
{
	sealed class SecurityCalculatorForStaffTest : TestCaseWithFactory
	{
		public void TestDoesNotUnnecessaryData()
		{
			var staffs = new List<GlbStaff>(10);
			for (var i = 10; i < 21; i++)
			{
				var staff = Factory.New<GlbStaff>();
				staff.FillWithValidTestData();
				var code = i.ToString();
				staff.GS_Code = code;
				staff.GS_LoginName = "a" + code;
				staff.StaffPlainTextPassword = "password";
				staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
				var se = Factory.New<GlbSecurity>();
				se.GU_SecurityRight = Env.Security.AllowMessageErrors.Code;
				se.GU_SecurityItemIsAllowed = true;
				se.GU_GS = staff.PK;
				se.GU_GB = GlbBranch.CurrentBranch.PK;
				staffs.Add(staff);
				staff.GS_IsActive = i % 2 == 0;
			}
			var groups = new List<GlbGroup>(10);
			for (var i = 10; i < 21; i++)
			{
				var code = i.ToString();
				var group = Factory.New<GlbGroup>();
				group.GG_Code = "A" + code;
				staffs.ForEach(staff =>
				{
					if (staff.GS_Code < code)
					{
						group.Staff.Add(staff);
					}
				});
				groups.Add(group);
				group.GG_IsActive = i % 2 == 3;
			}
			Factory.Save();
			var staffPKs = staffs.Select(x => x.PK).ToArray();
			var checkpoint = Env.Security.FindCheckPoint(Env.Security.AllowMessageErrors.Code);
			var branchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			var departmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid();
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var tablesToIgnore = new List<string>();
			using (AssertDbHitsForAllFactories("Security Check", new Dictionary<string, int>(),
				hitTolerance: 5,
				stackTraceToIgnore: null,
				tablesToCollectQueriesFor: null,
				useOnlyNewFactories: true,
				tablesToIgnore: tablesToIgnore,
				acceptableVariance: 1))
			{
				var newFactory = new BusinessObjectFactory();
				var collection = new GlbSecurityCollection(newFactory);
				collection.Load(new ZQuery(GlbSecuritySchema.GU_GS, staffPKs));
				var securityCalculatorForStaff = new SecurityCalculatorForStaff(newFactory, collection, staffs);
				staffs.ForEach(staff => securityCalculatorForStaff.IsRightAllowed(checkpoint, staff.PK.ToGuid(), branchPK, departmentPK, companyPK));
			}
		}
	}
}
