using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	class ExitControlSendToCustomsMenuCreatorTest : TestCaseWithFactory
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
					var form = (EU.ExitControl.GUI.ExitControlMessageSendingForm)obj;
					AssertType<ExitControlMessageSendingObjectParent>("MessageSendingObjectParent", form.MessageSendingObjectParent);
				});
				sendToCustomsMenuItem.PerformClick();
			});
		}
	}
}
