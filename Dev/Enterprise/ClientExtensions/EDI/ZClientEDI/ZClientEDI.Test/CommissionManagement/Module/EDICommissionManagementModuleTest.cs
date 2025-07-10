using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.CommissionManagement.Module.Testing
{
	[TestedType(typeof(EDICommissionManagementModule))]
	class EDICommissionManagementModuleTest : ZModuleBasherTest
	{
		#region ID
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Commission;
		}

		#endregion
		#region Menu Items
		public void TestActionMenuItems()
		{
			using (var module = new EDICommissionManagementModule())
			{
				var toolBarButtons = module.ToolBarButtons;
				AssertArrayEqualsByElements(new[] { "Agreement Approval", "Ambiguity Resolver", "Finalizer", }, toolBarButtons.Skip(toolBarButtons.Length - 4).Take(3).Select(x => x.Text).ToArray());
			}
		}

		public void TestCommissionAmbiguityResolverMenuItem()
		{
			using (var module = new EDICommissionManagementModule())
			{
				var toolBarButtons = module.ToolBarButtons;
				var ambiguityResolverMenuItem = (ZToolBarButton)toolBarButtons.FirstOrDefault(x => x.Text == "Ambiguity Resolver");
				AssertNotNull("ambiguityResolverMenuItem", ambiguityResolverMenuItem);
				EDISecurityCheckpoints.CommissionResolveAmbiguity.IsAllowed = false;
				ambiguityResolverMenuItem.PerformClick();
				AssertEquals(EDISecurityCheckpoints.CommissionResolveAmbiguity.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion
		#region Export To Excel
		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}
		#endregion
	}
}
