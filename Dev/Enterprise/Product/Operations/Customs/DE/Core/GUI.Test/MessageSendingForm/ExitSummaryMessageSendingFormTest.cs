using System.Windows.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ExitSummaryMessageSendingForm))]
	class ExitSummaryMessageSendingFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				AssertEquals("Send Exit Summary", form.Text);
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override Form GetFormToBashCore()
		{
			var exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CusExitDetails.AddNew();
			exitHeader.CusExitDetails.AddNew();

			var parent = new ExitSummaryMessageSendingActionParent(exitHeader);
			return new ExitSummaryMessageSendingForm(parent);
		}
	}
}
