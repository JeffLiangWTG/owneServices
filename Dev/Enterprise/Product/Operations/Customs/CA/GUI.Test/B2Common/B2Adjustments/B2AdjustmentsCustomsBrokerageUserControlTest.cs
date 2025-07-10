using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class B2AdjustmentsCustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestMessageUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (B2AdjustmentsCustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				AssertEquals("B3XMessagesUserControl", typeof(B3XMessagesUserControl), brokerageControl.MessageUserControl.GetType());
			}
		}

		public void TestRemovingRedundantTabs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (B2AdjustmentsCustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				Assert(brokerageControl.InvoiceLinesTabPage.IsDisposed);
				Assert(brokerageControl.InvoicesTabPage.IsDisposed);
				Assert(brokerageControl.PackingTabPage.IsDisposed);
				Assert(brokerageControl.InvoiceGroupingTabPage.IsDisposed);
				Assert(brokerageControl.MiscOptionsTabPage.IsDisposed);
				Assert(brokerageControl.ContainerTabPage.IsDisposed);
				Assert(!brokerageControl.MessagesTabPage.IsDisposed);
			}
		}

		public void TestK84UserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (B2AdjustmentsCustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.K84TabPage;
				AssertEquals("K84UserControl", typeof(K84UserControl), brokerageControl.K84UserControl.GetType());
				AssertEquals("Status", brokerageControl.K84TabPage.Text);
			}
		}

		public void TestTabVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (B2AdjustmentsCustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				Assert("Message Tab invisible for B2", !brokerageControl.MessagesTabPage.TabVisible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (B2AdjustmentsCustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				Assert("Message Tab visible for B3X", brokerageControl.MessagesTabPage.TabVisible);
			}
		}
	}
}
