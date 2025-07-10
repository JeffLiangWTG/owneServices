using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class IM2CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestDeclarationUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (IM2CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.OriginalDeclarationTabPage;
				AssertEquals("BaseDeclarationTabPage", typeof(CAJobDeclarationUserControl), brokerageControl.JobDeclarationUserControl.GetType());
				AssertEquals("Declaration", brokerageControl.OriginalDeclarationTabPage.Text);
			}
		}

		public void TestK84UserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (IM2CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.K84TabPage;
				AssertEquals("K84UserControl", typeof(K84UserControl), brokerageControl.K84UserControl.GetType());
				AssertEquals("Status", brokerageControl.K84TabPage.Text);
			}
		}
	}
}
