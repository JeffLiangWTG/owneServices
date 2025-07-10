using System.Drawing;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Balloons
{
	sealed class BalloonDescriptor_Test : TestCase
	{
		public void TestIsEmpty()
		{
			Form.Controls.Add(Control);
			var descriptor = new BalloonDescriptor(Control, "caption", "description", Notifications);
			AssertEquals(false, descriptor.IsEmpty);

			descriptor = new BalloonDescriptor(Control, "", "", Notifications);
			AssertEquals(true, descriptor.IsEmpty);

			descriptor = new BalloonDescriptor(Control, null, null, Notifications);
			AssertEquals(true, descriptor.IsEmpty);

			Notifications = new NotificationCollection();
			Notifications.AddError("hi");
			descriptor = new BalloonDescriptor(Control, null, null, Notifications);
			AssertEquals(false, descriptor.IsEmpty);

			descriptor = new BalloonDescriptor(Control, "caption", null, Notifications);
			AssertEquals(false, descriptor.IsEmpty);

			descriptor = new BalloonDescriptor(Control, "", "desc", Notifications);
			AssertEquals(false, descriptor.IsEmpty);
		}

		public void TestAllUniqueNotifications()
		{
			var descriptor = new BalloonDescriptor(Control, "caption", "description", Notifications);
			Notifications.AddError("error");
			Notifications.AddError("error");
			Notifications.AddWarning("error");
			Notifications.AddWarning("warning");
			Notifications.AddWarning("warning");

			AssertEquals("Notifications.Count", 5, descriptor.Notifications.Count());
			AssertEquals("UniqueNotifications.Count", 3, descriptor.AllUniqueNotifications.Count());
		}

		public void TestUniqueNotifications()
		{
			var count = BalloonDescriptor.NotificationsCap * 2;
			var notifications = new INotification[count * 2];
			for (var i = 0; i < count; ++i)
			{
				notifications[i] = new Notification(CargoWise.EntityFramework.NotificationType.Warning, i.ToString());
				notifications[i + count] = new Notification(CargoWise.EntityFramework.NotificationType.Warning, i.ToString());
			}

			var descriptor = new BalloonDescriptor(Control, "caption", "description", notifications);

			AssertEquals("Notifications.Count", count * 2, descriptor.Notifications.Count());
			AssertEquals("AllUniqueNotifications.Count", count, descriptor.AllUniqueNotifications.Count());
			AssertEquals("UniqueNotifications(0).Count", BalloonDescriptor.NotificationsCap + 1, descriptor.UniqueNotifications(0).Count());
			AssertEquals("UniqueNotifications(1).Count", BalloonDescriptor.NotificationsCap + 2, descriptor.UniqueNotifications(1).Count());
			AssertEquals("UniqueNotifications(3).Count", BalloonDescriptor.NotificationsCap + 2, descriptor.UniqueNotifications(3).Count());
			AssertEquals("UniqueNotifications(count-notificationsCap).Count", BalloonDescriptor.NotificationsCap + 1, descriptor.UniqueNotifications(count - BalloonDescriptor.NotificationsCap).Count());
			//just to make sure no exceptions get thrown/nulls returned
			AssertNotEquals(null, descriptor.UniqueNotifications(BalloonDescriptor.NotificationsCap * 2 - 1));
			AssertNotEquals(null, descriptor.UniqueNotifications(BalloonDescriptor.NotificationsCap * 2));
			AssertNotEquals(null, descriptor.UniqueNotifications(BalloonDescriptor.NotificationsCap * 3));
			AssertNotEquals(null, descriptor.UniqueNotifications(BalloonDescriptor.NotificationsCap * 10));
			AssertNotEquals(null, descriptor.UniqueNotifications(-BalloonDescriptor.NotificationsCap));
		}

		public void TestConstructionWithNoAnchorRect()
		{
			Form.Controls.Add(Control);

			var descriptor = new BalloonDescriptor(Control, "caption", "description", Notifications);
			AssertEquals(Notifications, descriptor.Notifications);
			AssertEquals("caption", descriptor.Caption);
			AssertEquals("description", descriptor.Description);
			AssertEquals("form", Form, descriptor.ParentForm);
			AssertEquals("anchor control", Control, descriptor.AnchorControl);
			AssertEquals("anchor rect", new Rectangle(0, 0, Control.Width, Control.Height), descriptor.AnchorRectOnControl);
			AssertEquals("default HideWhenMouseOverBalloon", true, descriptor.HideWhenMouseOverBalloon);
		}

		public void TestConstructionWithAnchorRect()
		{
			Form.Controls.Add(Control);

			var rect = new Rectangle(10, 10, 20, 20);
			var descriptor = new BalloonDescriptor(Control, rect, "caption", "description", Notifications);
			AssertEquals(Notifications, descriptor.Notifications);
			AssertEquals("caption", descriptor.Caption);
			AssertEquals("description", descriptor.Description);
			AssertEquals("form", Form, descriptor.ParentForm);
			AssertEquals("anchor control", Control, descriptor.AnchorControl);
			AssertEquals("anchor rect", rect, descriptor.AnchorRectOnControl);
			AssertEquals("default HideWhenMouseOverBalloon", true, descriptor.HideWhenMouseOverBalloon);
		}

		#region Implementation

		ZChildForm Form
		{
			get { return form ?? (form = new ZChildForm()); }
		}
		ZChildForm form;

		ZLabel Control
		{
			get { return control ?? (control = new ZLabel()); }
		}
		ZLabel control;

		NotificationCollection Notifications = new NotificationCollection();

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (control != null)
			{
				control.Dispose();
			}
		}

		#endregion
	}
}
