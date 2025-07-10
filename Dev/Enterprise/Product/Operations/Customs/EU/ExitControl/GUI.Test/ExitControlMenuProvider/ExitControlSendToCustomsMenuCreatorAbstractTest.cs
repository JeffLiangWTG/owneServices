using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

public abstract class ExitControlSendToCustomsMenuCreatorAbstractTest : TestCaseWithFactory
{
	public void TestCreate()
	{
		CombineAssertions(() =>
		{
			var sendToCustomsMenuItem = new ExitControlSendToCustomsMenuCreator(Factory.New<CusExitHeader>()).Create();
			AssertEquals("Send to Customs - Text", "Send to Customs", sendToCustomsMenuItem.Text);
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
			{
				var form = (ExitControlMessageSendingForm)obj;
				AssertType<ExitControlMessageSendingObjectParent>("MessageSendingObjectParent", form.MessageSendingObjectParent);
			});
			sendToCustomsMenuItem.PerformClick();
		});
	}
}
