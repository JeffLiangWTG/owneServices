using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class EMCSCustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestWorkflowTabInitialised()
		{
			using (var form = new EMCSDeclarationForm(declaration))
			{
				form.Show();
				var workflowTab = form.FindSingle<ZTemplateTabControl>("MainTabControl").GetTabPageByNameOrText("Workflow && Tracking");
				Assert(((IWorkflowTabPage)workflowTab).Initialized);
			}
		}

		public void TestSendUXMLMenu()
		{
			using (var form = new EMCSDeclarationForm(declaration))
			{
				form.Show();
				var actionMenu = form.Menu.MenuItems.FindByText("Actions");
				AssertNotNull(actionMenu.MenuItems.FindByText("Send Universal XML"));
			}
		}

		public void TestMessagesTabPageFound()
		{
			using (var form = new EMCSDeclarationForm(declaration))
			{
				form.Show();
				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertNotNull(tabControl.GetTabPageByNameOrText("Messages"));
			}
		}

		public void TestPackagesTabPageFound()
		{
			using (var form = new EMCSDeclarationForm(declaration))
			{
				form.Show();
				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertNotNull(tabControl.GetTabPageByNameOrText("Packing"));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
	}
}
