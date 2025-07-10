using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class B3XMessagesUserControlTest : TestCaseWithFactory
	{
		public void TestStatusesErrorsTabHasUserControl()
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
	}
}
