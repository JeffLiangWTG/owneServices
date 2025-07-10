using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Balloons
{
	sealed class Balloon_Test : TestCase
	{
		public void TestShowCreatesCorrectBalloonDescriptor()
		{
			var control = new GenericExtendedControl();
			var extensions = new ControlExtensionCollection(control);

			control.Extensions = extensions;
			control.Height = 50;
			control.Width = 32;

			Form.Controls.Add(control);

			var notificationExtension = new NotificationExtension();
			control.Extensions.Add(notificationExtension);

			var hintExtension = new HintExtension();
			control.Extensions.Add(hintExtension);

			var notifications = new NotificationCollection();
			notifications.AddError("ERR");
			notifications.AddWarning("WARN");
			notifications.AddMessageError("MSG");

			notificationExtension.Notifications = notifications;
			hintExtension.Caption = "CAP";
			hintExtension.Description = "DESC";

			var descriptor =
				new BalloonDescriptor(control, control.ClientRectangle, "CAP", "DESC", notifications);
			descriptor.HideWhenMouseOverBalloon = false;

			var balloon = new Balloon();
			balloon.Show(control);

			var currentDescriptor = balloon.BalloonWindow.Descriptor;

			AssertEquals(descriptor.Description, currentDescriptor.Description);
			AssertEquals(descriptor.Caption, currentDescriptor.Caption);
			AssertEquals(descriptor.HideWhenMouseOverBalloon, currentDescriptor.HideWhenMouseOverBalloon);
			AssertEquals(descriptor.AnchorControl, currentDescriptor.AnchorControl);
			AssertEquals(descriptor.AnchorRectOnControl, currentDescriptor.AnchorRectOnControl);
			Assert(!balloon.BalloonWindow.TopMost);
			Assert(currentDescriptor.Notifications.HasErrors());
			Assert(currentDescriptor.Notifications.HasMessageErrors());
			Assert(currentDescriptor.Notifications.HasWarnings());

			balloon.Hide();

			Form.TopMost = true;

			var balloon2 = new Balloon();
			balloon2.Show(control);

			Assert(balloon2.BalloonWindow.TopMost);

			balloon2.Hide();
			extensions.Dispose();
		}

		public void TestShow_GetHintFromParentForZDropButton() => AssertGetHintFromParent<ZDropButton>();

		public void TestShow_GetHintFromParentForZButtonBare() => AssertGetHintFromParent<ZButton.Bare>();

		void AssertGetHintFromParent<TControl>()
			where TControl : Control, new()
		{
			var control = new GenericExtendedControl();
			var extensions = new ControlExtensionCollection(control);
			control.Extensions = extensions;
			control.Height = 50;
			control.Width = 32;

			var notificationExtension = new NotificationExtension();
			control.Extensions.Add(notificationExtension);
			var notifications = new NotificationCollection();
			notifications.AddError("ERR");
			notificationExtension.Notifications = notifications;

			var hintExtension = new HintExtension();
			hintExtension.Caption = "CAP";
			hintExtension.Description = "DESC";
			control.Extensions.Add(hintExtension);

			var dropButton = new TControl();
			control.Controls.Add(dropButton);
			Form.Controls.Add(control);

			var balloon = new Balloon();
			balloon.Show(notifications, dropButton, dropButton.ClientRectangle, false);

			var currentDescriptor = balloon.BalloonWindow.Descriptor;
			AssertEquals("CAP", currentDescriptor.Caption);

			balloon.Hide();
			extensions.Dispose();
		}

		public void TestInstance()
		{
			AssertEquals("Same singleton", Balloon.Instance, Balloon.Instance);
		}

		public void TestBalloonWindowInstance()
		{
			AssertEquals("BalloonWindow instance cached once created", Balloon.Instance.BalloonWindow, Balloon.Instance.BalloonWindow);
		}

		public void TestBalloonWindowDisposedOnHide()
		{
			var control = new GenericExtendedControl();
			Form.Controls.Add(control);
			var descriptor = new BalloonDescriptor(control, control.ClientRectangle, "CAP", "DESC", new NotificationCollection());
			Balloon.Instance.Show(descriptor);
			var window = Balloon.Instance.BalloonWindow;

			Balloon.Instance.Hide();
			AssertEquals("BalloonWindow is disposed once it has been hidden", true, window.IsDisposed);
			AssertEquals("BalloonWindow is re-created after the previous one was hidden", false, Balloon.Instance.BalloonWindow.IsDisposed);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestShowWithNull()
		{
			Balloon.Instance.Show((BalloonDescriptor)null);
		}

		public void LazyCreationOfWindow()
		{
			var controller = new Balloon();
			AssertNull(controller.balloonWindow);
		}

		public void TestHideOnEmptyDescriptor()
		{
			var label = new ZLabel();
			Form.Controls.Add(label);

			var descriptor = new BalloonDescriptor(label, "caption", "description", NotificationCollection.Empty);
			Balloon.Instance.Show(descriptor);
			AssertEquals("descriptor set, balloon showing", descriptor, Balloon.Instance.BalloonWindow.Descriptor);

			Balloon.Instance.Show(new BalloonDescriptor(label, "", "", null));
			AssertNull("descriptor", Balloon.Instance.BalloonWindow.Descriptor);
		}

		public void TestNoHideInTranslationFeedbackMode()
		{
			using (TranslationFeedbackManager.EnableTranslationFeedbackModeForTest())
			{
				var label = new ZLabel();
				Form.Controls.Add(label);
				var descriptor = new BalloonDescriptor(label, "caption", "description", NotificationCollection.Empty);
				Balloon.Instance.Show(descriptor);
				AssertEquals("descriptor set, balloon showing", descriptor, Balloon.Instance.BalloonWindow.Descriptor);
				Balloon.Instance.Hide();
				AssertEquals("descriptor set, balloon showing", descriptor, Balloon.Instance.BalloonWindow.Descriptor);
			}
		}

		public void TestBalloonStillShownDuringTestsWhenForced()
		{
			using (var label = new ZLabel())
			{
				Form.Controls.Add(label);
				var descriptor = new BalloonDescriptor(label, "caption", "description", NotificationCollection.Empty);
				Balloon.Instance.IsShownDuringTesting = true;
				Balloon.Instance.Show(descriptor);

				Assert("The balloon should be visible.", Balloon.Instance.IsVisible);
				Assert("The balloon should not be showing that it is visible because of simulation.", !Balloon.Instance.isSimulatedVisible);
				Assert("The BalloonWindow should actually be shown.", Balloon.Instance.BalloonWindow.Visible);
			}
		}

		public void TestShowBalloonWindowTwice_PreviousBalloonWindowIsDisposed()
		{
			var label = new ZLabel();
			Form.Controls.Add(label);
			var descriptor = new BalloonDescriptor(label, "caption", "description", NotificationCollection.Empty);
			Balloon.Instance.IsShownDuringTesting = true;
			Balloon.Instance.Show(descriptor);

			var balloonWindow1 = Balloon.Instance.BalloonWindow;

			var descriptor2 = new BalloonDescriptor(label, "another caption", "another description", NotificationCollection.Empty);
			Balloon.Instance.Show(descriptor2);

			var balloonWindow2 = Balloon.Instance.BalloonWindow;
			Assert("Previous balloon window should be disposed.", balloonWindow1.IsDisposed);
			Assert("Current balloon window should not be disposed.", !balloonWindow2.IsDisposed);
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			Balloon.Instance.Hide();
		}

		ZChildForm Form
		{
			get { return form ?? (form = new ZChildForm()); }
		}
		ZChildForm form;

		#endregion
	}
}
