using CargoWise.ComponentModel;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class StatusbarTest : TestCase
	{
		public void TestAnswersCanNotUpdateIfStatusBarControlIsNull()
		{
			StatusBarExtension.Initialize((IUpdateStatusBar)null);
			AssertEquals(false, StatusBarExtension.CanUpdate());
		}

		public void TestAnswersCanNotUpdateIfIUpdateStatusBarIsNull()
		{
			StatusBarExtension.Initialize((IUpdateStatusBar)null);
			AssertEquals(false, StatusBarExtension.CanUpdate());
		}

		[ExpectNoExceptions]
		public void TestUpdateSimplyDelegatesToResourceStringParentControl()
		{
			statusBar.Setup(m => m.UpdateStatusBar("TEST", NotificationType.Error));

			StatusBarExtension.Update("TEST", NotificationType.Error);
		}

		#region Implementation
		Statusbar StatusBarExtension
		{
			get
			{
				if (statusBarExtension == null)
				{
					statusBarExtension = new Statusbar();
					statusBarExtension.Initialize(statusBar.Object);
				}
				return statusBarExtension;
			}
		}
		Statusbar statusBarExtension;
		readonly Mock<IUpdateStatusBar> statusBar = new Mock<IUpdateStatusBar>(MockBehavior.Strict);

		#endregion
	}
}
