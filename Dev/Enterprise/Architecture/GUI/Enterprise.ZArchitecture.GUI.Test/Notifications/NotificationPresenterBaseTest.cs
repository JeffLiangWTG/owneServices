using System.Windows.Forms;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Notifications.Testing
{
	sealed class NotificationPresenterBaseTest : TestCase
	{
		public void TestShowNotificationsIfNotificationsAreNotEmpty()
		{
			var notifications = new NotificationCollection();
			notifications.AddError("Err");

			presenter.Notifications = notifications;
			Assert(presenter.DisplayWasCalled);
		}

		public void TestClearsNotificationsIfNotificationsAreEmpty()
		{
			var notifications = new NotificationCollection();
			notifications.AddError("Err");

			presenter.Notifications = notifications;
			Assert(presenter.DisplayWasCalled);

			presenter.Notifications = NotificationCollection.Empty;
			Assert(presenter.ClearWasCalled);
		}

		#region Test Classes

		class TestNotificationPresenterBase : NotificationPresenter
		{
			public bool DisplayWasCalled;
			public bool ClearWasCalled;

			protected override void Display()
			{
				DisplayWasCalled = true;
			}

			protected override void Clear()
			{
				ClearWasCalled = true;
			}
		}

		#endregion

		#region Implementation

		TextBox control;
		TestNotificationPresenterBase presenter;

		protected override void SetUp()
		{
			control = new TextBox();

			presenter = new TestNotificationPresenterBase();
			presenter.Initialize(control);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (presenter != null)
			{
				presenter.Dispose();
			}
		}

		#endregion
	}
}
