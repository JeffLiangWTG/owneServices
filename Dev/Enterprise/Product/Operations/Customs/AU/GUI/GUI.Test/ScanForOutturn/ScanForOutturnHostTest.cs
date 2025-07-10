using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI.Testing
{
	abstract class ScanForOutturnHostTest : TestCaseWithFactory
	{
		public void TestLicenceError()
		{
			using (var form = new ZForm())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var manager = GetNewScanForOutturnHost(form);
				manager.ScanForOutturnClick(null, null);
				AssertEquals("License error", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("To start scanning for outturn you need to obtain HVLVClearance license.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected abstract ScanForOutturnHost GetNewScanForOutturnHost(ZForm form);
	}
}
