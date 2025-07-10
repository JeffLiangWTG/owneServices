using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing;

public class EcsMessageMenuProviderBaseOnlyTest : TestCaseWithFactory
{
	public void TestPreSaveBeforeSendingMessages()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
		exitHeader.CusExitDetails.AddNew().CED_Status = "XXX";

		using (var ecsMessage = new EcsMessagingMenu())
		using (var form = new ZForm(declaration))
		{
			form.Menu.MenuItems.Add(ecsMessage);
			ecsMessage.ExitHeader = exitHeader;

			var arrMessageMenu = ecsMessage.MenuItems.FindByText("Arrive at Exit Location");
			var depMessageMenu = ecsMessage.MenuItems.FindByText("Depart from Exit Location");

			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.No);
			arrMessageMenu.PerformClick();
			AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Declaration not saved", declaration.HasChanges);

			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
			depMessageMenu.PerformClick();
			AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Declaration saved", !declaration.HasChanges);
		}
	}
}
