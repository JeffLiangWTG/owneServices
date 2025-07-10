using System.Windows.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ExitNotificationMessageSendingForm))]
	class ExitNotificationMessageSendingFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				AssertEquals("Send Exit Notification", form.Text);
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override Form GetFormToBashCore()
		{
			var exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CusExitDetails.AddNew();
			exitHeader.CusExitDetails.AddNew();

			var parent = new ExitNotificationMessageSendingActionParent(exitHeader);
			return new ExitNotificationMessageSendingForm(parent);
		}
	}
}
