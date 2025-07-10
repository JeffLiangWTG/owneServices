using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Notifications.Testing
{
	sealed class NotificationAdornmentTest : TestCase
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestPresenterIsRequired()
		{
			var adornment = new TestAdornment();
			adornment.SetPresenter(null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestDoNotAllowToSetPresenterMoreThanOnce()
		{
			var adornment = new TestAdornment();
			adornment.SetPresenter(Presenter);
			adornment.SetPresenter(Presenter);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestControlIsRequired()
		{
			var adornment = new TestAdornment();
			adornment.SetControl(null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestDoNotAllowToSetControlMoreThanOnce()
		{
			var adornment = new TestAdornment();
			adornment.SetControl(new TextBox());
			adornment.SetControl(new TextBox());
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestNotificationsAreRequired()
		{
			var adornment = new TestAdornment();
			adornment.Notifications = null;
		}

		public void TestDefaultsToEmptyNotifications()
		{
			var adornment = new TestAdornment();
			AssertEquals(NotificationCollection.Empty, adornment.Notifications);
		}

		class TestAdornment : NotificationAdornment
		{
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
