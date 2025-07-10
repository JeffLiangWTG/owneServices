using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Notifications.Testing
{
	sealed class BackgroundAdornmentTest : TestCase
	{
		public void TestDoNotChangeControlBackgroundColorWhenNotificationsAreEmpty()
		{
			var control = new TextBox();
			var oldColor = control.BackColor;

			var adornment = new BackgroundAdornment();
			adornment.Initialize(control, Presenter);

			adornment.Notifications = NotificationCollection.Empty;
			Assert(control.BackColor == oldColor);
		}

		public void TestSetControlBackgroundColorWhenNotificationsAreNotEmpty()
		{
			var control = new TextBox();
			var oldColor = control.BackColor;

			var adornment = new BackgroundAdornment();
			adornment.Initialize(control, Presenter);

			var notifications = new NotificationCollection();
			notifications.AddError("ERR");

			adornment.Notifications = notifications;

			Assert(control.BackColor != oldColor);
			Assert(control.BackColor == NotificationColorScheme.GetColor(NotificationType.Error));
		}

		public void TestSetControlBackgroundColorToDefaultColorWhenNotificationsAreEmpty()
		{
			var control = new TextBox();
			var oldColor = control.BackColor;

			var adornment = new BackgroundAdornment();
			adornment.Initialize(control, Presenter);

			var notifications = new NotificationCollection();
			notifications.AddError("ERR");

			adornment.Notifications = notifications;

			Assert(control.BackColor != oldColor);
			Assert(control.BackColor == NotificationColorScheme.GetColor(NotificationType.Error));

			adornment.Notifications = NotificationCollection.Empty;

			Assert(control.BackColor == oldColor);
		}

		public void TestDoNotChangeControlColorIfControlIsReadOnly()
		{
			var control = new KTextBox();
			control.ReadOnly = true;

			var readOnlyColor = control.BackColor;

			var adornment = new BackgroundAdornment();
			adornment.Initialize(control, Presenter);

			var notifications = new NotificationCollection();
			notifications.AddError("ERR");

			adornment.Notifications = notifications;
			Assert(control.BackColor == readOnlyColor);

			adornment.Notifications = NotificationCollection.Empty;
			Assert(control.BackColor == readOnlyColor);
		}

		class FakePresenter : NotificationPresenter
		{
			protected override void Display() { }
			protected override void Clear() { }
		}
		FakePresenter Presenter
		{
			get { return presenter ?? (presenter = new FakePresenter()); }
		}
		FakePresenter presenter;

		protected override void TearDown()
		{
			base.TearDown();
			if (presenter != null)
			{
				presenter.Dispose();
			}
		}
	}
}
