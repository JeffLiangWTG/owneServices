using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ConfirmationDialogForm))]
	sealed class ConfirmationDialogFormTest : ZFormBasherTest
	{
		public void TestNoNotifications_ShouldThrowException()
		{
			AssertExceptionThrown<ArgumentException>("There are no notifications to confirm so why show the form?", () => new ConfirmationDialogForm(new ConfirmationDialogDescriptor("No notifications", "Should explode")));
		}

		public void TestWithWarning_CanBypass()
		{
			var notification = new ConfirmationNotification(NotificationTypes.Warning, "4 minute");
			var descriptor = new ConfirmationDialogDescriptor("Bypassable", "", notification);

			using (var form = new ConfirmationDialogForm(descriptor))
			{
				form.Show();

				form.OKDialogButton.PerformClick();

				AssertEquals("You must correct the notifications or choose to ignore them.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, notification.IsIgnored);
				AssertEquals(DialogResult.None, form.DialogResult);

				var notificationControl = form.NotificationsPanel.Controls.Cast<ConfirmationNotificationUserControl>().Single();
				notificationControl.IgnoredCheckBox.Checked = true;

				AssertEquals(true, notification.IsIgnored);

				form.OKDialogButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertEquals(ZDialogResult.OK, descriptor.Result);
			}
		}

		public void TestWithWarning_CancelButtonPressed()
		{
			var notification = new ConfirmationNotification(NotificationTypes.Warning, "4 minute");
			var descriptor = new ConfirmationDialogDescriptor("Bypassable", "", notification);

			using (var form = new ConfirmationDialogForm(descriptor))
			{
				form.Show();

				var notificationControl = form.NotificationsPanel.Controls.Cast<ConfirmationNotificationUserControl>().Single();
				notificationControl.IgnoredCheckBox.Checked = true;

				AssertEquals(true, notification.IsIgnored);

				form.CancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
				AssertEquals(ZDialogResult.Cancel, descriptor.Result);
			}
		}

		public void TestWithWarningAndError_CannotBypass()
		{
			var notification1 = new ConfirmationNotification(NotificationTypes.Warning, "4 minute");
			var notification2 = new ConfirmationNotification(NotificationTypes.Error, "Reckoner");
			var descriptor = new ConfirmationDialogDescriptor("Not Bypassable", "", notification1, notification2);

			using (var form = new ConfirmationDialogForm(descriptor))
			{
				form.Show();

				form.OKDialogButton.PerformClick();

				AssertEquals("You must correct the errors before proceeding.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, notification1.IsIgnored);
				AssertEquals(DialogResult.None, form.DialogResult);

				var notificationControls = form.NotificationsPanel.Controls.Cast<ConfirmationNotificationUserControl>().ToArray();

				AssertEquals(2, notificationControls.Length);
				AssertEquals("Warning control background colour", Color.LightYellow, notificationControls[0].BackColor);
				AssertEquals("Error control background colour", Color.Pink, notificationControls[1].BackColor);
				AssertEquals("Warnings can be overridden", true, notificationControls[0].IgnoredCheckBox.Visible);
				AssertEquals("Errors cannot be overridden", false, notificationControls[1].IgnoredCheckBox.Visible);

				notificationControls[0].IgnoredCheckBox.Checked = true;

				AssertEquals(true, notification1.IsIgnored);

				form.OKDialogButton.PerformClick();

				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals(ZDialogResult.None, descriptor.Result);

				form.CancelButton.PerformClick();

				AssertEquals(DialogResult.Cancel, form.DialogResult);
				AssertEquals(ZDialogResult.Cancel, descriptor.Result);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var descriptor = new ConfirmationDialogDescriptor("Friendship is magic", "Is it?",
				new ConfirmationNotification(NotificationTypes.Error, "It is"),
				new ConfirmationNotification(NotificationTypes.Warning, "It isn't")
				);

			return new ConfirmationDialogForm(descriptor);
		}

		#endregion
	}
}
