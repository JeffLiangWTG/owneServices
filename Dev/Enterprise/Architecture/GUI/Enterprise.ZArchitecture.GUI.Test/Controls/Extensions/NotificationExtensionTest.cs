using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions.Testing
{
	sealed class NotificationExtensionTest : BaseExtensionTest<NotificationExtension>
	{
		#region Binding
		[ExpectNoExceptions]
		public void TestDisposedExtension()
		{
			var extension = CreateExtension();

			PresenterMoq.Setup(m => m.Dispose());
			extension.Dispose();

			extension.SetDataBinding(new object(), "");
			Assert(GetBindings(extension).Count == 0);
		}

		public void TestDoNotBindIfDataSourceIsNull()
		{
			var control = new GenericExtendedDataBoundControl_WithNotificationDataMembers(Array.Empty<string>(), null, "");
			var extension = CreateExtension(control);

			extension.SetDataBinding(null, "");
			Assert(GetBindings(extension).Count == 0);
		}

		public void TestDoNotBindIfDataMemberIsEmpty()
		{
			var control = new GenericExtendedDataBoundControl_WithNotificationDataMembers(Array.Empty<string>(), new object(), "");
			var extension = CreateExtension(control);

			extension.SetDataBinding(new object(), "");
			Assert(GetBindings(extension).Count == 0);
		}

		public void TestThrowExceptionIfNumberOfPropertyMembersIsMoreThanSupported()
		{
			var props = new List<string>();

			for (var i = 0; i <= NotificationExtension.SupportedNumberOfBoundProperties; i++)
			{
				props.Add("prop" + i);
			}

			var control = new GenericExtendedDataBoundControl_WithNotificationDataMembers(props.ToArray(), new object(), "prop");
			var extension = CreateExtension(control);

			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{ extension.SetDataBinding(new object(), "prop"); });
			Assert(GetBindings(extension).Count == 0);
		}

		public void TestThrowExceptionIfPropertyMembersHasDuplicateEntries()
		{
			var control = new GenericExtendedDataBoundControl_WithNotificationDataMembers(new string[] { "prop", "prop" }, new object(), "prop");
			var extension = CreateExtension(control);

			AssertExceptionThrown(typeof(NotificationExtension.DuplicatePropertyMemberException), delegate
			{ extension.SetDataBinding(new object(), "prop"); });
			Assert(GetBindings(extension).Count == 0);
		}

		public void TestDoNotAddBindingsIfPropertyMemberIsEmptyString()
		{
			var control = new GenericExtendedDataBoundControl_WithNotificationDataMembers(new string[] { "" }, new object(), "prop");
			var extension = CreateExtension(control);

			extension.SetDataBinding(new object(), "prop");
			Assert(GetBindings(extension).Count == 0);
		}

		public void TestDoNotAddBindingsIfNoCorrespondingPropertyInfo()
		{
			var extension = CreateExtension();
			var control = (GenericExtendedDataBoundControl_WithNotificationDataMembers)extension.Owner;
			Form.Controls.Add(control);

			foreach (var member in control.NotificationDataMembers)
			{
				extension.IsPropertyExistsResult = false;
			}

			extension.SetDataBinding(control.DataSource, ""); // data members from INotificationDataMembers will be used

			Assert(GetBindings(extension).Count == 0);
		}

		public void TestAddAndForceBindings()
		{
			var extension = CreateExtension();
			var control = (GenericExtendedDataBoundControl_WithNotificationDataMembers)extension.Owner;

			foreach (var member in control.NotificationDataMembers)
			{
				extension.IsPropertyExistsResult = true;
			}

			extension.SetDataBinding(control.DataSource, ""); // data members from INotificationDataMembers will be used

			var bindings = GetBindings(extension);

			Assert(bindings.Count == control.NotificationDataMembers.Length);

			for (var i = 0; i < bindings.Count; i++)
			{
				var binding = (KBinding)bindings[i];
				var notificationsPropertyPath = control.NotificationDataMembers[i] +
												   BindingHelper.InfoSuffix + "+Notifications";

				Assert(binding.DataSource == control.DataSource);
				Assert(binding.BindingMemberInfo.BindingField == notificationsPropertyPath);
				Assert(binding.PropertyName == "Notifications" + (i + 1));
				Assert(extension.ForceWasCalled);
				Assert(extension.BindingPassedToForceBinding.PropertyName.StartsWith("Notifications"));
				Assert(extension.ControlPassedToForceBinding == extension.Owner.Host);
			}
		}

		public void TestUnbindRemovesBindings()
		{
			var extension = CreateExtension();
			var control = (GenericExtendedDataBoundControl_WithNotificationDataMembers)extension.Owner;

			foreach (var member in control.NotificationDataMembers)
			{
				extension.IsPropertyExistsResult = true;
			}

			extension.SetDataBinding(control.DataSource, ""); // data members from INotificationDataMembers will be used
			Assert(GetBindings(extension).Count == control.NotificationDataMembers.Length);

			extension.SetDataBinding(null, "");
			Assert(GetBindings(extension).Count == 0);
		}

		static ControlBindingsCollection GetBindings(IBindableComponent extension)
		{
			return extension.DataBindings;
		}

		#endregion

		[ExpectNoExceptions]
		public void TestHoldOnIfNewINotificationTypeIsNoneAndCurrentINotificationTypeIsNoneToo()
		{
			var extension = CreateExtension();

			extension.Validating = false;
			extension.Notifications = NotificationCollection.Empty;
		}

		[ExpectNoExceptions]
		public void TestHoldOnIfValidationIsNotFinished()
		{
			var extension = CreateExtension();

			var notifications = new NotificationCollection();
			notifications.AddError("TEST");

			extension.Validating = true;
			extension.Notifications = notifications;
		}

		[ExpectNoExceptions]
		public void TestUpdatesNotificationsOnPresenterAndBroadcastINotificationTypeChanges()
		{
			var extension = CreateExtension();
			var control = extension.Owner;

			var notifications = new NotificationCollection();
			notifications.AddError("TEST");

			NotifierMoq.Object.BroadcastNotificationsChange(notifications.GetHighestSeverityNotificationType(), control.Host);
			PresenterMoq.Object.Notifications = notifications;

			extension.Validating = false;
			extension.Notifications = notifications;
		}

		[ExpectNoExceptions]
		public void TestUpdatesNotificationsOnPresenterButDoNotBroadcastINotificationTypeChangesIfStateIsTheSame()
		{
			var extension = CreateExtension();
			var control = extension.Owner;

			var notifications = new NotificationCollection();
			notifications.AddError("TEST");

			NotifierMoq.Object.BroadcastNotificationsChange(notifications.GetHighestSeverityNotificationType(), control.Host);
			PresenterMoq.Object.Notifications = notifications;

			extension.Validating = false;
			extension.Notifications = notifications;

			extension.Validating = false;
			extension.Notifications = notifications;
		}

		[ExpectNoExceptions]
		public void TestBoundNotificationPropertiesCombinesNotifications()
		{
			var extension = CreateExtension();
			var control = extension.Owner;

			var notifications1 = new NotificationCollection();
			notifications1.AddWarning("WARN");

			var notifications2 = new NotificationCollection();
			notifications2.AddError("ERR");

			var combined1 = new NotificationCollection();
			combined1.AddRange(notifications1);
			combined1.AddRange(NotificationCollection.Empty);

			var combined2 = new NotificationCollection();
			combined2.AddRange(notifications1);
			combined2.AddRange(notifications2);

			NotifierMoq.Object.BroadcastNotificationsChange(combined1.GetHighestSeverityNotificationType(), control.Host);
			PresenterMoq.Object.Notifications = null;

			NotifierMoq.Object.BroadcastNotificationsChange(combined2.GetHighestSeverityNotificationType(), control.Host);
			PresenterMoq.Object.Notifications = null;

			extension.Validating = false;
			extension.Notifications1 = notifications1;
			extension.Notifications2 = notifications2;

			AssertEquals("Two notifications need to be present.", 2, extension.Notifications.Count());
			foreach (var notification in extension.Notifications)
			{
				if (!notifications1.Contains(notification) && !notifications2.Contains(notification))
				{
					var type = notification.Type.NotificationTypeName(CargoWise.ResourceStrings.Grammar.PluralState.Plural);
					var message = notification.Message;
					Assert($"Both notifications must be present.\r\nNotification {type}:{message} was not found within either notification collections.", false);
				}
			}
		}

		public void TestIsValidatingReturnsCurrentValidationStateByUsingValidationExtensionIfItsSupported()
		{
			var validationExtension = new Mock<IValidationExtension>(MockBehavior.Strict);
			var extensions = new Mock<IControlExtensionCollection>(MockBehavior.Strict);

			IExtendedControl control = new GenericExtendedControl(extensions.Object);
			var extension = CreateExtension(control);

			extensions.Setup(m => m.Supports<IValidationExtension>()).Returns(true);
			extensions.Setup(m => m.Get<IValidationExtension>()).Returns(validationExtension.Object);
			validationExtension.Setup(m => m.IsValidating).Returns(true);

			extension.ShouldCallOriginalMethod = true;
			Assert(extension.CallIsValidating());
		}

		public void TestIsValidatingReturnsFalseWhenValidationExtensionIsNotSupported()
		{
			var extensions = new Mock<IControlExtensionCollection>(MockBehavior.Strict);

			IExtendedControl control = new GenericExtendedControl(extensions.Object);
			var extension = CreateExtension(control);

			extensions.Setup(m => m.Supports<IValidationExtension>()).Returns(false);

			extension.ShouldCallOriginalMethod = true;
			Assert(!extension.CallIsValidating());
		}

		public void TestNotificationChangedEventsExistsForBoundProperties_IfNotMemoryLeakWillOccur_DueToBugInDotNet()
		{
			AssertNotNull("Notifications1Changed event must exist on the class to prevent binding's PropertyDescriptor.AddValueChanged causing a memory leak", typeof(NotificationExtension).GetEvent("Notifications1Changed"));
			AssertNotNull("Notifications2Changed event must exist on the class to prevent binding's PropertyDescriptor.AddValueChanged causing a memory leak", typeof(NotificationExtension).GetEvent("Notifications2Changed"));
		}

		#region Support

		TestNotificationExtension CreateExtension()
		{
			var dataSource = new FakeBusinessObject();
			var control = new GenericExtendedDataBoundControl_WithNotificationDataMembers(new string[] { "property1", "property2" }, dataSource, "property1");
			return CreateExtension(control);
		}

		TestNotificationExtension CreateExtension(IExtendedControl control)
		{
			var result = new TestNotificationExtension(NotifierMoq.Object);
			PresenterMoq.Setup(m => m.Initialize(control.Host));

			result.Initialize(control, PresenterMoq.Object);

			PresenterMoq.Verify(m => m.Initialize(control.Host));

			return result;
		}

		class TestNotificationExtension : NotificationExtension
		{
			public bool Validating;
			public bool ShouldCallOriginalMethod;

			public TestNotificationExtension(NotificationBroadcaster notifier)
				: base(notifier)
			{
			}

			protected override bool IsValidating()
			{
				return ShouldCallOriginalMethod ? base.IsValidating() : Validating;
			}

			public bool CallIsValidating()
			{
				return IsValidating();
			}

			protected override void ForceBinding(Binding binding, Control control)
			{
				this.ForceWasCalled = true;
				this.BindingPassedToForceBinding = binding;
				this.ControlPassedToForceBinding = control;
			}

			public bool IsPropertyExistsResult;

			protected override bool IsPropertyExists(BindingContext bindingContext, object dataSource, string dataMember)
			{
				return IsPropertyExistsResult;
			}

			public bool ForceWasCalled;
			public Binding BindingPassedToForceBinding;
			public Control ControlPassedToForceBinding;
		}

		#endregion

		#region Implementation
		Mock<INotificationPresenter> PresenterMoq => presenterMoq ?? (presenterMoq = new Mock<INotificationPresenter>(MockBehavior.Strict) { CallBase = true });
		Mock<INotificationPresenter> presenterMoq;

		Mock<NotificationBroadcaster> NotifierMoq => notifierMoq ?? (notifierMoq = new Mock<NotificationBroadcaster>(MockBehavior.Default) { CallBase = true });
		Mock<NotificationBroadcaster> notifierMoq;

		ZForm Form => form ?? (form = new ZForm());
		ZForm form;

		protected override void SetUp()
		{
			base.SetUp();
			PresenterMoq.SetupAllProperties();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
