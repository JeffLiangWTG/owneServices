using System.Linq;
using Enterprise.Core.Environment;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Security.Core.Testing
{
	sealed class CRMSecurityTest : TransactionedTestCase
	{
		public void TestCRMSecurity()
		{
			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			var viewParent = new SecurityCheckpoint("viewParent", (NoResString)"viewParent", null, security);
			var editParent = new SecurityCheckpoint("editParent", (NoResString)"editParent", null, security);
			var osmgParent = new SecurityCheckpoint("osmgParent", (NoResString)"editParent", null, security);
			var taskASNParent = new SecurityCheckpoint("taskASNParent", (NoResString)"taskASNParent", null, security);

			var crmSecurity1 = new CRMSecurity(security);
			crmSecurity1.CreateViewCheckpoint(viewParent);
			crmSecurity1.CreateEditCheckpoints(editParent);
			crmSecurity1.CreateOSMGCheckpoint(osmgParent);
			crmSecurity1.CreateTaskAssignmentCheckpoint(taskASNParent);
			AssertCRMSecurity(security, viewParent, editParent, crmSecurity1);
			AssertOSMGSecurity(security, osmgParent, crmSecurity1);
			AssertTaskAssignmentSecurity(security, taskASNParent, crmSecurity1);

			var viewParent2 = new SecurityCheckpoint("viewParent2", (NoResString)"viewParent2", null, security);
			var crmSecurity2 = new CRMSecurity(security);
			crmSecurity2.CreateViewCheckpoint(viewParent2);
			AssertCRMSecurity(security, viewParent2, null, crmSecurity2);
		}

		void AssertCRMSecurity(IZSecurity security, SecurityCheckpoint viewParent, SecurityCheckpoint editParent, CRMSecurity crmSecurity)
		{
			var view = security.FindCheckPoint($"{viewParent.Code}.ViewBySNA");

			AssertEquals(1, viewParent.ChildCheckPoints.Count());
			AssertEquals(viewParent.ChildCheckPoints.First(), view);
			AssertEquals(0, view.ChildCheckPoints.Count());

			if (crmSecurity.IsViewOnly)
			{
				AssertNull(crmSecurity.EditByStaffNotAssigned);
				AssertNull(crmSecurity.EditByStaffRoleAssignedLookup);
				AssertNull(editParent);
			}
			else
			{
				var edit = security.FindCheckPoint($"{editParent.Code}.EditBySNA");
				var codes = DataRegistry.Instance.OrgStaffMemberAssignmentRoles.OfType<CodeDescriptionPair>().Where(x => !string.IsNullOrWhiteSpace(x.Code)).ToArray();

				AssertEquals(1 + codes.Length, editParent.ChildCheckPoints.Count());
				AssertEquals(editParent.ChildCheckPoints.First(), edit);
				AssertEquals(0, edit.ChildCheckPoints.Count());

				foreach (var code in codes)
				{
					var roleCheckpoint = security.FindCheckPoint($"{editParent.Code}.EditBySRA.{code}");
					AssertNotNull(roleCheckpoint);
					Assert(editParent.ChildCheckPoints.Any(x => x.Code == roleCheckpoint.Code));
				}
			}
		}

		void AssertOSMGSecurity(IZSecurity security, SecurityCheckpoint osmgParent, CRMSecurity crmSecurity)
		{
			var osmg = security.FindCheckPoint($"{osmgParent.Code}.IgnoreOSMG");

			AssertEquals(1, osmgParent.ChildCheckPoints.Count());
			AssertEquals(osmgParent.ChildCheckPoints.First(), osmg);
			AssertEquals(0, osmg.ChildCheckPoints.Count());
		}

		void AssertTaskAssignmentSecurity(IZSecurity security, SecurityCheckpoint taskAssignmentParent, CRMSecurity crmSecurity)
		{
			var osmg = security.FindCheckPoint($"{taskAssignmentParent.Code}.IgnoreTaskASN");

			AssertEquals(1, taskAssignmentParent.ChildCheckPoints.Count());
			AssertEquals(taskAssignmentParent.ChildCheckPoints.First(), osmg);
			AssertEquals(0, osmg.ChildCheckPoints.Count());
		}
	}
}
