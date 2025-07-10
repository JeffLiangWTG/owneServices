using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI.Testing
{
	internal class UPECurrentQueueUserControlTest : TestCaseWithFactory
	{
		public void TestControlVisibility()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Callout callout = Factory.New<Callout>();
			using (var uPECurrentQueueUserControl = new FormForUPECurrentQueueUserControl(callout))
			{
				uPECurrentQueueUserControl.Show();
				AssertEquals(true, uPECurrentQueueUserControl.UserControl.EIRRaisedDateDateEdit.Visible);
				AssertEquals(true, uPECurrentQueueUserControl.UserControl.EIRRaisedDateLabel.Visible);
				AssertEquals(true, uPECurrentQueueUserControl.UserControl.AccountNumberTextBox.Visible);
				AssertEquals(true, uPECurrentQueueUserControl.UserControl.AccountNumberLabel.Visible);
			}

			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			using (var uPECurrentQueueUserControl = new FormForUPECurrentQueueUserControl(uPECusHAWB))
			{
				uPECurrentQueueUserControl.Show();
				AssertEquals(true, uPECurrentQueueUserControl.UserControl.EIRRaisedDateDateEdit.Visible);
				AssertEquals(true, uPECurrentQueueUserControl.UserControl.EIRRaisedDateLabel.Visible);
				AssertEquals(false, uPECurrentQueueUserControl.UserControl.AccountNumberTextBox.Visible);
				AssertEquals(false, uPECurrentQueueUserControl.UserControl.AccountNumberLabel.Visible);
			}

			UPEJobDeclaration uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			using (var uPECurrentQueueUserControl = new FormForUPECurrentQueueUserControl(uPEJobDeclaration))
			{
				uPECurrentQueueUserControl.Show();
				AssertEquals(true, uPECurrentQueueUserControl.UserControl.EIRRaisedDateDateEdit.Visible);
				AssertEquals(true, uPECurrentQueueUserControl.UserControl.EIRRaisedDateLabel.Visible);
				AssertEquals(false, uPECurrentQueueUserControl.UserControl.AccountNumberTextBox.Visible);
				AssertEquals(false, uPECurrentQueueUserControl.UserControl.AccountNumberLabel.Visible);
			}
		}
	}
}
