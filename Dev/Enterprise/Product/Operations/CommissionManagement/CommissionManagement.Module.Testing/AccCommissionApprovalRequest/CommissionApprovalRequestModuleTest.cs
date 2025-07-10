using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Module.Testing
{
	[TestedType(typeof(CommissionApprovalRequestModule))]
	public class CommissionApprovalRequestModuleTest : ZModuleBasherTest
	{
		#region ID

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CommissionApprovalRequest;
		}

		#endregion

		#region Allowed Actions

		public void TestAllowDelete()
		{
			using (var module = new CommissionApprovalRequestModule())
			{
				AssertEquals(false, module.AllowDelete);
			}
		}

		public void TestAllowEdit()
		{
			using (var module = new CommissionApprovalRequestModule())
			{
				AssertEquals(true, module.AllowEdit);
			}
		}

		public void TestAllowNew()
		{
			using (var module = new CommissionApprovalRequestModule())
			{
				AssertEquals(false, module.AllowNew);
			}
		}

		public void TestAllowView()
		{
			using (var module = new CommissionApprovalRequestModule())
			{
				AssertEquals(true, module.AllowView);
			}
		}

		#endregion

		#region Security

		public void TestSecurity()
		{
			using (var module = new CommissionApprovalRequestModule())
			{
				AssertEquals(Env.Security.CommissionApprovalRequest, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region Licence

		public void TestLicenceCheckPoint()
		{
			using (var module = new CommissionManagementModule())
			{
				AssertEquals(Env.Licence.CommissionManager, module.LicenceCheckPoint);
			}
		}

		#endregion
	}
}
