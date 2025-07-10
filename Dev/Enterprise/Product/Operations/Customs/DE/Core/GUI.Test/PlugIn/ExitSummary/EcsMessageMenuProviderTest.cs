using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class EcsMessageMenuProviderTest : EU.GUI.PlugIn.Testing.EcsMessageMenuProviderTest
	{
		public override void TestCreateArrivalMessages()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ClickMenuEntry(ArriveCaption);
			AssertEquals(SentSuccessfullyMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCreateArrivalMessages_NoMovement()
		{
			exitHeader.CusExitDetails.RemoveAndDeleteAll();
			ClickMenuEntry(ArriveCaption);
			AssertEquals(NoMovementMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public override void TestCreateDepartureMessages()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ClickMenuEntry(DepartCaption);
			AssertEquals(SentSuccessfullyMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCreateDepartureMessages_NoMovement()
		{
			exitHeader.CusExitDetails.RemoveAndDeleteAll();
			ClickMenuEntry(DepartCaption);
			AssertEquals(NoMovementMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_ParentTableCode = "JE";
			var exitDetail1 = exitHeader.CusExitDetails.AddNew();
			exitDetail1.CED_Status = "XXX";
			var exitDetail2 = exitHeader.CusExitDetails.AddNew();
			exitDetail2.CED_Status = "XXX";
			var mnuProvider = new EcsMessageMenuProvider(exitHeader);
			menuItems = mnuProvider.CreateMenuItems().ToArray();
		}
		CusExitControlHeader exitHeader;
		ZMenuItem[] menuItems;

		void ClickMenuEntry(string menuCaption)
		{
			var arrMenuItem = menuItems.FindByText(menuCaption);
			arrMenuItem.PerformClick();
		}

		const string ArriveCaption = "Arrive at Exit Location";
		const string DepartCaption = "Depart from Exit Location";
		const string SentSuccessfullyMessage = "Message sent successfully";
		const string NoMovementMessage = "There is no movement to be sent.";
	}
}

