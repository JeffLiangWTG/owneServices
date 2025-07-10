
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Notifications.Testing
{
	sealed class NotificationAdornmentPresenterTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestPropogateNotificationsOnAdornment()
		{
			var notifications = new NotificationCollection();
			notifications.AddError("ERR");
			var adornment = new Mock<NotificationAdornment>();
			var control = new TextBox();
			using (var presenter = new TestNotificationAdornmentPresenter(adornment.Object))
			{
				presenter.Initialize(control);

				presenter.SetNotifications(notifications);

				adornment.Object.Initialize(control, presenter);

				adornment.Object.Attach();
				adornment.Object.Notifications = notifications;

				adornment.Object.Detach();
				adornment.Object.Notifications = NotificationCollection.Empty;

				presenter.FireDisplay();
				presenter.FireClear();
			}
		}

		#region Test Classes

		class TestNotificationAdornmentPresenter : NotificationAdornmentPresenter
		{
			readonly NotificationAdornment adornment;

			public TestNotificationAdornmentPresenter(NotificationAdornment adornment)
			{
				this.adornment = adornment;
			}

			public void FireDisplay()
			{
				Display();
			}

			public void FireClear()
			{
				Clear();
			}

			protected override NotificationAdornment CreateAdornment()
			{
				return adornment;
			}

			public override IEnumerable<INotification> Notifications
			{
				get { return notifications; }
				set { base.Notifications = value; }
			}

			public void SetNotifications(NotificationCollection notifications)
			{
				this.notifications = notifications;
			}

			NotificationCollection notifications;
		}

		#endregion

		#region Implementation

		#endregion
	}
}
