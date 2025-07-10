using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions.Testing
{
	sealed class StatusbarExtensionTest : BaseExtensionTest<StatusbarExtension>
	{
		GenericExtendedControl control;

		Mock<INotificationExtension> notificationExtensionMock;
		Mock<IHintExtension> hintExtensionMock;
		INotificationExtension notificationExtension;
		IHintExtension hintExtension;
		Mock<Statusbar> statusBarMock;
		Statusbar statusBar;

		TestStatusbarExtension statusbarExtension;

		protected override void SetUp()
		{
			base.SetUp();

			control = new GenericExtendedControl();

			notificationExtensionMock = new Mock<INotificationExtension>(MockBehavior.Strict);
			notificationExtension = notificationExtensionMock.Object;
			hintExtensionMock = new Mock<IHintExtension>(MockBehavior.Strict);
			hintExtension = hintExtensionMock.Object;
			statusBarMock = new Mock<Statusbar>(MockBehavior.Strict) { CallBase = true };
			statusBar = statusBarMock.Object;

			statusBarMock.Setup(m => m.Initialize(control));

			statusbarExtension = new TestStatusbarExtension(statusBar);
			statusbarExtension.HintExtension = hintExtension;
			statusbarExtension.NotificationExtension = notificationExtension;
			statusbarExtension.Initialize(control);
		}

		[ExpectNoExceptions]
		public void TestDoNotTryToDisplayHintsIfCantUpdateStatusbar()
		{
			statusBarMock.Setup(m => m.CanUpdate()).Returns(false);

			statusbarExtension.Display();
		}

		[ExpectNoExceptions]
		public void TestDoNothingIfBothHintAndNotificationExtensionAreNotAvailable()
		{
			statusbarExtension.NotificationExtension = null;
			statusbarExtension.HintExtension = null;

			statusBarMock.Setup(m => m.CanUpdate()).Returns(true);

			statusbarExtension.Display();
		}

		[ExpectNoExceptions]
		public void TestDoNotDisplayHintsINotificationTypeIsNone()
		{
			statusbarExtension.HintExtension = null;

			statusBarMock.Setup(m => m.CanUpdate()).Returns(true);
			notificationExtensionMock.Setup(m => m.Notifications).Returns(NotificationCollection.Empty);

			statusbarExtension.Display();
		}

		[ExpectNoExceptions]
		public void TestDisplaysHighestPriorityNotification()
		{
			statusbarExtension.HintExtension = null;

			var notifications = new NotificationCollection();
			notifications.AddError("ERR");
			notifications.AddMessageError("MESSERR");
			notifications.AddWarning("WARN");

			statusBarMock.Setup(m => m.CanUpdate()).Returns(true);
			notificationExtensionMock.Setup(m => m.Notifications).Returns(notifications);

			var errorMessage = string.Empty;
			var typeOutput = string.Empty;
			statusBarMock.Setup(m => m.Update(It.IsAny<string>(), It.IsAny<INotificationType>()))
				.Callback((string s, INotificationType n) =>
				{
					errorMessage = s;
					typeOutput = $"IsFatal:{n.IsFatal}|Severity:{n.Severity}|Name:{n.EnumValueName}";
				});

			statusbarExtension.Display();

			AssertEquals(
				message: "The passed in message should have been that with the highest priority.",
				expected: "ERR",
				actual: errorMessage);
			AssertEquals(
				message: "The passed in notification type should have been that with the highest priority.",
				expected: "IsFatal:True|Severity:100|Name:Error",
				actual: typeOutput);
		}

		[ExpectNoExceptions]
		public void TestFallbackToShowHintsIfNoNotifications()
		{
			statusbarExtension.NotificationExtension = null;

			statusBarMock.Setup(m => m.CanUpdate()).Returns(true);
			hintExtensionMock.Setup(m => m.Description).Returns("TEST");
			statusBarMock.Setup(m => m.Update("TEST", null));

			statusbarExtension.Display();
		}

		[ExpectNoExceptions]
		public void TestShowCaptionIfNullDescription()
		{
			AssertShowCaptionIfBlankDescription(null);
		}

		[ExpectNoExceptions]
		public void TestShowCaptionIfEmptyDescription()
		{
			AssertShowCaptionIfBlankDescription("");
		}

		[ExpectNoExceptions]
		public void TestShowCaptionIfBlankDescription()
		{
			AssertShowCaptionIfBlankDescription(" ");
		}

		void AssertShowCaptionIfBlankDescription(string expectedDescription)
		{
			statusbarExtension.NotificationExtension = null;

			statusBarMock.Setup(m => m.CanUpdate()).Returns(true);
			hintExtensionMock.Setup(m => m.Description).Returns(expectedDescription);
			hintExtensionMock.Setup(m => m.Caption).Returns("TEST CAPTION");
			statusBarMock.Setup(m => m.Update("TEST CAPTION", null));

			statusbarExtension.Display();
		}

		[ExpectNoExceptions]
		public void TestDoNotUpdateStatusbarOnClearIfCantUpdate()
		{
			statusBarMock.Setup(m => m.CanUpdate()).Returns(false);

			statusbarExtension.Clear();
		}

		[ExpectNoExceptions]
		public void TestClearStatusbarIfCanUpdate()
		{
			statusBarMock.Setup(m => m.CanUpdate()).Returns(true);
			statusBarMock.Setup(m => m.Update(string.Empty, null));

			statusbarExtension.Clear();
		}

		#region Support

		class TestStatusbarExtension : StatusbarExtension
		{
			public IHintExtension HintExtension;
			public INotificationExtension NotificationExtension;

			public TestStatusbarExtension(Statusbar statusbar)
				: base(statusbar)
			{ }

			protected override IHintExtension GetHintExtension()
			{
				return HintExtension;
			}

			protected override INotificationExtension GetNotificationExtension()
			{
				return NotificationExtension;
			}
		}

		#endregion
	}
}
