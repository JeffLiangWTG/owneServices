using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.Testing
{
	public class JobManagementCRMSecurityProviderTest : CRMSecurityProviderTest<JobHeader>
	{
		protected override CRMSecurityProvider<JobHeader> GetNewProviderForTest() => new JobManagementCRMSecurityProvider();

		protected override IEnumerable<JobHeader> GetTestObjectWithoutStaffAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNewMainAddress();

			var obj = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			obj.JH_OA_LocalChargesAddr = address.PK;
			return new JobHeader[] { obj };
		}

		protected override IEnumerable<JobHeader> GetTestObjectWithOrgStaffAssignment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = NonOSMG.PK;
			var staffAssignments = org1.StaffAssignments.AddNew();
			staffAssignments.O8_Role = "SAL";
			staffAssignments.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			var address = org1.Addresses.AddNewMainAddress();

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_OA_LocalChargesAddr = address.PK;
			jobHeader1.JH_GS_NKRepSales = "U00";

			return new JobHeader[] { jobHeader1 };
		}

		protected override IEnumerable<JobHeader> GetTestObjectWithBizObjStaffAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNewMainAddress();

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeader1.JH_OA_LocalChargesAddr = address.PK;
			return new JobHeader[] { jobHeader1 };
		}

		protected override IEnumerable<JobHeader> GetTestObjectWithTaskAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNewMainAddress();

			var objs = base.GetTestObjectWithTaskAssignment();
			foreach (var obj in objs)
			{
				obj.JH_OA_LocalChargesAddr = address.PK;
			}

			return objs;
		}

		protected override IEnumerable<JobHeader> GetTestObjectsWithoutOrg()
		{
			var obj = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			return new JobHeader[] { obj };
		}

		protected override ZQuery SetupCRMSecurityFilterStripsQuery(ZQuery query)
		{
			var baseQuery = base.SetupCRMSecurityFilterStripsQuery(query);
			return baseQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
		}

		protected override IEnumerable<JobHeader> GetTestObjectWithOrgAssignedForOSMG(OrgHeader org)
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_OA_LocalChargesAddr = org.Addresses[0].PK;
			jobHeader1.JH_GS_NKRepSales = "U00";

			return new JobHeader[] { jobHeader1 };
		}

		protected override void AddStaffAssignmentForCompany(JobHeader obj, ZString staffCode, ZString role, ZGuid companyPk)
		{
			var assignment = obj.LocalChargesAddr.Header.StaffAssignments.AddNew();
			assignment.O8_Role = role;
			assignment.O8_GS_NKPersonResponsible = staffCode;
			assignment.O8_GC = companyPk;
		}
	}
}
