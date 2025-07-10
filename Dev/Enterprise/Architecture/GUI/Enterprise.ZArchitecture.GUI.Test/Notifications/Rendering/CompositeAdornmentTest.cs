using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Notifications.Testing
{
	sealed class CompositeAdornmentTest : TestCase
	{
		List<Mock<NotificationAdornment>> children;
		TextBox control;
		INotificationPresenter presenter;

		CompositeAdornment composite;

		class FakePresenter : NotificationPresenter
		{
			protected override void Display() { }
			protected override void Clear() { }
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TextBox();

			presenter = new Mock<INotificationPresenter>().Object;

			var mockRepo = new MockRepository(MockBehavior.Strict);
			children = new List<Mock<NotificationAdornment>>();
			children.Add(mockRepo.Create<NotificationAdornment>());
			children.Add(mockRepo.Create<NotificationAdornment>());

			composite = new TestCompositeAdornment(control);
			composite.Add(children[0].Object);
			composite.Add(children[1].Object);

			foreach (var child in children)
			{
				child.SetupAllProperties();
				child.Setup(m => m.Attach()).CallBase();
				child.Setup(m => m.Detach()).CallBase();
			}

			Assert(composite[0] == children[0].Object);
			Assert(composite[1] == children[1].Object);
		}

		[ExpectNoExceptions]
		public void TestDetach_WithDetachedAdornment()
		{
			using (var fakePresenter = new FakePresenter())
			{
				var testAdornment = new IconAdornment();
				testAdornment.Initialize(control, fakePresenter);
				testAdornment.Attach();
				composite.Add(testAdornment);
				testAdornment.Detach();
				composite.Detach();
			}
		}

		[ExpectNoExceptions]
		public void TestSetNotificationsOnEachChild()
		{
			var notifications = NotificationCollection.Empty;

			foreach (var child in children)
			{
				child.Object.Notifications = notifications;
			}

			composite.Notifications = notifications;
		}

		[ExpectNoExceptions]
		public void TestSetControlOnEachChildIfControlIsNull()
		{
			foreach (var child in children)
			{
				child.Setup(m => m.Control).Returns((Control)null);
				child.Setup(m => m.SetControl(control));
			}

			composite.SetControl(control);
		}

		[ExpectNoExceptions]
		public void TestDoNotSetControlOnEachChildIfControlIsNotNull()
		{
			foreach (var child in children)
			{
				child.Setup(m => m.Control).Returns(control);
			}

			composite.SetControl(control);

			foreach (var child in children)
			{
				child.Verify(m => m.SetControl(null), Times.Never);
			}
		}

		[ExpectNoExceptions]
		public void TestSetPresenterOnEachChildIfPresenterIsNull()
		{
			foreach (var child in children)
			{
				child.Setup(m => m.Presenter).Returns((INotificationPresenter)null);
				child.Setup(m => m.SetPresenter(presenter));
			}

			composite.SetPresenter(presenter);
		}

		[ExpectNoExceptions]
		public void TestDoNotSetPresenterOnEachChildIfPresenterIsNotNull()
		{
			foreach (var child in children)
			{
				child.Setup(m => m.Presenter).Returns(presenter);
			}

			composite.SetPresenter(presenter);

			foreach (var child in children)
			{
				child.Verify(m => m.SetPresenter(null), Times.Never);
			}
		}

		[ExpectNoExceptions]
		public void TestCallAttachOnEachChild()
		{
			foreach (var child in children)
			{
				child.Object.Attach();
			}

			composite.Attach();
		}

		[ExpectNoExceptions]
		public void TestCallDetachOnEachChild()
		{
			foreach (var child in children)
			{
				child.Object.Detach();
			}

			composite.Detach();
		}
	}
}
