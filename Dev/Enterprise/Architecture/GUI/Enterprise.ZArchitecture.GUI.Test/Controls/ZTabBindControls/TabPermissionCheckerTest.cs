using Enterprise.Core.Environment;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TabPermissionCheckerTest : TestCase
	{
		public void TestFindsCheckPointAndCheckIfItAllowedOrNot()
		{
			var security = CreateSecurityInstance();
			var root = CreateRootCheckPoint(security);

			new FormSecurityBuilder(root, security, true, true)
				.Form("Screen")
				.Tab("Tab1");

			var checker = new TabPermissionChecker("Screen", security);

			Assert(checker.IsEditAllowed("Tab1"));
			Assert(checker.IsViewAllowed("Tab1"));
			Assert(checker.IsEditAllowed("WorkflowTabPage"));
			Assert(checker.IsViewAllowed("WorkflowTabPage"));

			var notAllowedCheckPoint = security.FindCheckPoint("Screen.View.Tab1");
			notAllowedCheckPoint.IsAllowed = false;
			Assert(!checker.IsViewAllowed("Tab1"));
			Assert(checker.IsEditAllowed("WorkflowTabPage"));
			Assert(checker.IsViewAllowed("WorkflowTabPage"));
		}

		static SecurityCheckpoint CreateRootCheckPoint(IZSecurity security)
		{
			return new SecurityCheckpoint("Root", (NoResString)"Root", null, security);
		}

		static IZSecurity CreateSecurityInstance()
		{
			return new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
		}
	}
}
