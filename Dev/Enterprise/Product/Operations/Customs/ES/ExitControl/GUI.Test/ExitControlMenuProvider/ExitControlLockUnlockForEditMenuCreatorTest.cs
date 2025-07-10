using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ES.ExitControl.GUI.Testing
{
	class ExitControlLockUnlockForEditMenuCreatorTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var exitControlLockForEditMenuItem = new ExitControlLockUnlockForEditMenuCreator(Factory.New<CusExitHeader>()).Create();
			AssertEquals("Lock/Unlock Exit Control - Text", "Lock/Unlock Exit Control", exitControlLockForEditMenuItem.Text);
		}

		public void TestLockUnlockExitReport_Click()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();

			exitHeader.Logs.RemoveAndDeleteAll();
			Factory.Save();

			var exitControlLockForEditMenuItem = new ExitControlLockUnlockForEditMenuCreator(exitHeader).Create();

			CombineAssertions(() =>
			{
				exitControlLockForEditMenuItem.PerformClick();
				AssertEquals("Message informing lock is done", "This tab page has been locked for edit\r\nExit Control\r\n\r\nYou can click the Exit Control - Unlock Exit Control to unlock it.", UnitTestUserNotification.Instance.LastMessage.Text);

				var log = exitHeader.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.LockForEditCode).First();
				Assert("Should contains the active LCK event.", !log.IsCancelled);

				exitControlLockForEditMenuItem.PerformClick();
				AssertEquals("Message informing unlock is done", "The Exit Control is unlocked.", UnitTestUserNotification.Instance.LastMessage.Text);

				log = exitHeader.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.UnlockForEditCode).First();
				Assert("Should contains the active UCK event.", !log.IsCancelled);
			});
		}
	}
}
