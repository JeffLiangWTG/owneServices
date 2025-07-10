using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Testing;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	sealed class KNotificationProviderTest : TestCase
	{
		public void TestBrowsableProperties()
		{ BrowsableChecker.CheckBrowsableProperties(typeof(KNotificationProvider), "BindingSource", "NotificationRenderer", "RenderNotificationsAfterFocus"); }

		#region Binding Notifications

		[RequiresSTA]
		public void TestNotificationsOnMetaData()
		{
			SetupForNotifications();
			NotificationProvider.RenderNotificationsAfterFocus = false;

			notificationsEntity.Text = "";
			ErrorProvider emptyError1 = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNotifications);
			ErrorProvider emptyError2 = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNoNotifications);
			AssertNull("No notifications yet", emptyError1);
			AssertNull("No notifications yet", emptyError2);

			notificationsEntity.Text = "error";
			ErrorProvider error1 = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNotifications);
			ErrorProvider error2 = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNoNotifications);
			AssertEquals("Should be a notification now", "error", error1.GetError(controlWithNotifications.TextBox));
			AssertNull("This control doesn't support our notification rendering", error2);
		}

		[RequiresSTA]
		public void TestNotificationsOnMetaData_WhenNotificationsAccessorPropertyCalculatedFromOtherPropertyAndValueChanged()
		{
			SetupForNotifications();
			NotificationProvider.RenderNotificationsAfterFocus = false;

			notificationsEntity.PropertyNotificationIsCalculatedFrom = "error_from_unrelated_prop";
			ErrorProvider error = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNotifications);
			AssertEquals("Should be a notification now", "error_from_unrelated_prop", error.GetError(controlWithNotifications.TextBox));
		}

		[RequiresSTA]
		public void TestNotificationsOnMetaData_RenderOnlyAfterFocused()
		{
			SetupForNotifications();
			NotificationProvider.RenderNotificationsAfterFocus = true;

			notificationsEntity.Text = "";
			ErrorProvider emptyError = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNotifications);
			AssertNull("No notifications yet", emptyError);

			notificationsEntity.Text = "error";
			emptyError = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNotifications);
			AssertNull("Should not be a notification yet", emptyError);

			controlWithNotifications.TextBox.Focus();
			controlWithNotifications.TextBox.Text = "";
			controlWithNotifications.TextBox.Text = "error";
			emptyError = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNotifications);
			AssertNull("Should not be a notification yet", emptyError);

			controlWithNoNotifications.Focus();
			notificationsEntity.Text = "";
			notificationsEntity.Text = "error";
			ErrorProvider error = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNotifications);
			AssertEquals("Should be a notification now the control has been focused", "error", error.GetError(controlWithNotifications.TextBox));
		}

		[RequiresSTA]
		public void TestNotificationsOnObject()
		{
			SetupForNotifications();
			NotificationProvider.RenderNotificationsAfterFocus = false;
			BindingSource.SetBindingMember(controlWithNotifications, ".");

			notificationsEntity.SetNotifications(new NotificationCollection(new Notification(NotificationType.Warning, "object_error")));
			ErrorProvider errorOnObject = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNotifications);
			AssertEquals("Should have an error on the object", "object_error", errorOnObject.GetError(controlWithNotifications.TextBox));

			notificationsEntity.SetNotifications(new NotificationCollection(new Notification(NotificationType.Error, "object_error_changed")));
			errorOnObject = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNotifications);
			AssertEquals("Should have an error on the object", "object_error_changed", errorOnObject.GetError(controlWithNotifications.TextBox));

			AssertEquals(
				"Should have NotificationsChanged events hooked for test",
				1, notificationsEntity.NotificationsChangedHookCount);
			BindingSource.SetBindingMember(controlWithNotifications, "");
			AssertEquals(
				"Should no longer have any NotificationsChanged events hooked",
				0, notificationsEntity.NotificationsChangedHookCount);
		}

		[RequiresSTA]
		public void TestNotificationPriority()
		{
			SetupForNotifications();
			NotificationProvider.RenderNotificationsAfterFocus = false;

			BindingSource.SetBindingMember(controlWithNotifications, ".");
			Icon errorIcon = NotificationIconScheme.Instance.GetIcon(NotificationType.Error);
			Icon warningIcon = NotificationIconScheme.Instance.GetIcon(NotificationType.Warning);

			notificationsEntity.SetNotifications(new NotificationCollection(
				new Notification(NotificationType.Warning, "warning"),
				new Notification(NotificationType.Warning, "warning")));
			ErrorProvider errorOnObject = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNotifications);
			AssertEquals("Should have warning icon", warningIcon, errorOnObject.Icon);

			notificationsEntity.SetNotifications(new NotificationCollection(
				new Notification(NotificationType.Warning, "warning"),
				new Notification(NotificationType.Error, "error")));
			errorOnObject = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNotifications);
			AssertEquals("Should have error icon", errorIcon, errorOnObject.Icon);

			notificationsEntity.SetNotifications(new NotificationCollection(
				new Notification(NotificationType.Error, "error"),
				new Notification(NotificationType.Warning, "warning")));
			errorOnObject = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNotifications);
			AssertEquals("Should have error icon", errorIcon, errorOnObject.Icon);

			notificationsEntity.SetNotifications(new NotificationCollection(
				new Notification(NotificationType.Error, "error"),
				new Notification(NotificationType.Error, "error")));
			errorOnObject = NotificationRenderer.GetActiveErrorProviderForTargetControl(controlWithNotifications);
			AssertEquals("Should have error icon", errorIcon, errorOnObject.Icon);
		}

		[RequiresSTA]
		public void TestExposeAllNotifications()
		{
			SetupForNotifications();
			NotificationProvider.RenderNotificationsAfterFocus = true;
			BindingSource.SetBindingMember(controlWithNotifications, ".");
			Form.Show();

			AssertEquals(
				"Shouldn't render notification when control not focused",
				false,
				NotificationProvider.ShouldRenderNotification(controlWithNotifications, notificationsEntity, "Text"));
			AssertEquals(
				"Shouldn't render notification when control not focused",
				false,
				NotificationProvider.ShouldRenderNotification(controlWithNotifications, notificationsEntity, ""));
			AssertEquals(
				"Shouldn't render notification when control not focused",
				false,
				NotificationProvider.ShouldRenderNotification(controlWithNotifications, controlWithNoNotifications.MyRenderedEntity, ""));

			NotificationProvider.ExposeAllNotifications();
			AssertEquals(
				"Should render notifications on entity now they're all exposed",
				true,
				NotificationProvider.ShouldRenderNotification(controlWithNotifications, notificationsEntity, "Text"));
			AssertEquals(
				"Should render notifications on entity now they're all exposed",
				true,
				NotificationProvider.ShouldRenderNotification(controlWithNotifications, notificationsEntity, ""));
			AssertEquals(
				"Should render notifications on entity for self-rendered control",
				true,
				NotificationProvider.ShouldRenderNotification(controlWithNotifications, controlWithNoNotifications.MyRenderedEntity, ""));
			AssertEquals(
				"Should render notifications on entity for self-rendered control",
				true,
				NotificationProvider.ShouldRenderNotification(controlWithNotifications, controlWithNoNotifications.MyRenderedEntity, "xxx"));
		}

		#endregion

		#region INotificationRenderContext

		[RequiresSTA]
		public void TestShouldAlwaysRenderAllNotifications()
		{
			SetupForNotifications();
			INotificationRenderContext context = NotificationProvider;

			NotificationProvider.RenderNotificationsAfterFocus = false;
			AssertEquals("ShouldAlwaysRenderAllNotifications true", true, context.ShouldAlwaysRenderAllNotifications());
			NotificationProvider.RenderNotificationsAfterFocus = true;
			AssertEquals("ShouldAlwaysRenderAllNotifications false", false, context.ShouldAlwaysRenderAllNotifications());
		}

		[RequiresSTA]
		public void TestShouldRenderNotification()
		{
			SetupForNotifications();
			TextBox ctrl = new TextBox();

			INotificationRenderContext context = NotificationProvider;
			NotificationProvider.RenderNotificationsAfterFocus = true;
			BindingSource.SetDataBinding(notificationsEntity, "");

			AssertEquals(false, context.ShouldRenderNotification(ctrl, notificationsEntity, ""));
			context.NotifyDataControlFocused(ctrl, notificationsEntity, "");
			AssertEquals(true, context.ShouldRenderNotification(ctrl, notificationsEntity, ""));

			AssertEquals(false, context.ShouldRenderNotification(ctrl, notificationsEntity, "Text"));
			context.NotifyDataControlFocused(ctrl, notificationsEntity, "Text");
			AssertEquals(true, context.ShouldRenderNotification(ctrl, notificationsEntity, "Text"));
			AssertEquals(true, context.ShouldRenderNotification(ctrl, notificationsEntity, ""));

			BindingSource.DataSource = null;
			AssertEquals(false, context.ShouldRenderNotification(ctrl, notificationsEntity, "Text"));
			AssertEquals(false, context.ShouldRenderNotification(ctrl, notificationsEntity, ""));
		}

		[RequiresSTA]
		public void TestShouldRenderNotification_UsingNotifyDataItemDeleted()
		{
			SetupForNotifications();
			TextBox ctrl = new TextBox();
			TextBox ctrl2 = new TextBox();

			INotificationRenderContext context = NotificationProvider;
			NotificationProvider.RenderNotificationsAfterFocus = true;
			BindingSource.SetDataBinding(notificationsEntity, "");

			context.NotifyDataControlFocused(ctrl, notificationsEntity, "");
			context.NotifyDataControlFocused(ctrl, notificationsEntity, "Text");
			context.NotifyDataControlFocused(ctrl2, notificationsEntity, "");
			context.NotifyDataControlFocused(ctrl2, notificationsEntity, "Text");
			AssertEquals(true, context.ShouldRenderNotification(ctrl, notificationsEntity, ""));
			AssertEquals(true, context.ShouldRenderNotification(ctrl, notificationsEntity, "Text"));

			context.NotifyDataItemDeleted(ctrl, notificationsEntity);
			AssertEquals("There is another control displaying notifications", true, context.ShouldRenderNotification(ctrl, notificationsEntity, ""));
			AssertEquals("There is another control displaying notifications", true, context.ShouldRenderNotification(ctrl, notificationsEntity, "Text"));

			context.NotifyDataItemDeleted(ctrl2, notificationsEntity);
			AssertEquals("Both controls displaying notifications gone", false, context.ShouldRenderNotification(ctrl, notificationsEntity, ""));
			AssertEquals("Both controls displaying notifications gone", false, context.ShouldRenderNotification(ctrl, notificationsEntity, "Text"));

			AssertEquals("Both controls displaying notifications gone", false, context.ShouldRenderNotification(ctrl, notificationsEntity, ""));
			AssertEquals("Both controls displaying notifications gone", false, context.ShouldRenderNotification(ctrl, notificationsEntity, "Text"));
		}

		[RequiresSTA]
		public void TestShouldRenderAnyNotificationsFrom()
		{
			SetupForNotifications();
			TextBox ctrl = new TextBox();

			INotificationRenderContext context = NotificationProvider;
			NotificationProvider.RenderNotificationsAfterFocus = true;
			BindingSource.SetDataBinding(notificationsEntity, "");
			AssertEquals(false, context.ShouldRenderAnyNotificationsFrom(ctrl, notificationsEntity));

			context.NotifyDataControlFocused(ctrl, notificationsEntity, "");
			AssertEquals(true, context.ShouldRenderAnyNotificationsFrom(ctrl, notificationsEntity));

			object dataSource = BindingSource.DataSource;
			BindingSource.DataSource = null;
			BindingSource.DataSource = dataSource;
			AssertEquals(false, context.ShouldRenderAnyNotificationsFrom(ctrl, notificationsEntity));
			context.NotifyDataControlFocused(ctrl, notificationsEntity, "error");
			AssertEquals(true, context.ShouldRenderAnyNotificationsFrom(ctrl, notificationsEntity));

			BindingSource.DataSource = null;
			AssertEquals(false, context.ShouldRenderAnyNotificationsFrom(ctrl, notificationsEntity));
		}

		#endregion

		#region Test Classes

		class TestBindingSource : KBindingSource
		{
			public TestBindingSource()
			{ DataSourceType = typeof(object); }

			public TestBindingSource(IContainer container)
				: base(container)
			{ DataSourceType = typeof(object); }

			public TestBindingSource(ContainerControl containerControl)
				: base(containerControl)
			{ DataSourceType = typeof(object); }
		}

		class TestNotificationRenderer : NotificationRenderer
		{
			public ErrorProvider GetActiveErrorProviderForTargetControl(Control control)
			{
				ISelfNotificationRendering selfNotificationRendering = control as ISelfNotificationRendering;
				ErrorProvider result = null;
				if (selfNotificationRendering == null || selfNotificationRendering.DefaultRenderingEnabled)
				{
					while (control is INotificationRenderTarget && control != null)
					{
						control = (control as INotificationRenderTarget).ControlToRenderNotificationOn;
					}

					if (control != null)
					{
						result = GetActiveErrorProviderForControl(control);
					}
				}
				return result;
			}
		}

		[DefaultBindingProperty("BoundValue")]
		[DesignTimeVisible(false)]
		[ToolboxItem(false)]
		public class TestControlWithNotifications : KPanel, INotificationRenderTarget
		{
			public TestControlWithNotifications()
			{
				this.TextBox = new KTextBox();
				Controls.Add(this.TextBox);
			}

			public KTextBox TextBox { get; private set; }

			public object BoundValue
			{
				get { return "ignore"; }
				set { }
			}

			#region INotificationRenderTarget Members

			public Control ControlToRenderNotificationOn
			{ get { return this.TextBox; } }

			#endregion
		}

		[DefaultBindingProperty("BoundValue")]
		[DesignTimeVisible(false)]
		[ToolboxItem(false)]
		public class TestControlSelfNotificationRendering : UserControl, ISelfNotificationRendering
		{
			public object BoundValue
			{
				get { return "ignore"; }
				set { }
			}

			public object MyRenderedEntity = new object();

			#region ISelfNotificationRendering Members

			public void SetNotificationRenderContext(INotificationRenderContext info)
			{
			}

			public object[] GetCurrentlyFocusableDataItems()
			{ return new object[] { this.MyRenderedEntity }; }

			public bool DefaultRenderingEnabled
			{ get { return false; } }

			public void ExposeAllNotifications()
			{
			}

			#endregion
		}

		public class TestEntityWithNotifications : KComponentWithPropertyChange, INotificationSource
		{
			public IEnumerable<INotification> Notifications
			{ get { return notifications; } }
			IEnumerable<INotification> notifications;

			public void SetNotifications(IEnumerable<INotification> value)
			{
				notifications = value;
				if (notificationsChanged != null)
				{
					notificationsChanged(this, EventArgs.Empty);
				}
			}

			public event EventHandler NotificationsChanged
			{
				add
				{
					NotificationsChangedHookCount++;
					notificationsChanged += value;
				}
				remove
				{
					NotificationsChangedHookCount--;
					notificationsChanged -= value;
				}
			}
			event EventHandler notificationsChanged;
			public int NotificationsChangedHookCount;

			[DefaultValue("")]
			protected string TextInternal { get; set; }

			[CalculatedFrom("TextInternal")]
			[NotificationsMember("TextNotifications")]
			public string Text
			{
				get { return TextInternal; }
				set
				{
					if (value != "ignore")
					{
						TextInternal = value;
						FirePropertyChanged(nameof(TextNotifications));
					}
				}
			}

			public string PropertyNotificationIsCalculatedFrom
			{
				get { return propertyNotificationIsCalculatedFrom; }
				set
				{
					if (propertyNotificationIsCalculatedFrom != value)
					{
						propertyNotificationIsCalculatedFrom = value;
						FirePropertyChanged(nameof(PropertyNotificationIsCalculatedFrom));
						FirePropertyChanged(nameof(TextNotifications));
					}
				}
			}
			string propertyNotificationIsCalculatedFrom;

			public NotificationCollection TextNotifications
			{
				get
				{
					NotificationCollection result = new NotificationCollection();
					if (Text == "error")
					{
						result.AddError("error");
					}
					if (PropertyNotificationIsCalculatedFrom == "error_from_unrelated_prop")
					{
						result.AddError("error_from_unrelated_prop");
					}
					return result;
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		class TestForm : KForm
		{
			public TestForm()
			{
				BindingSource.DataSourceType = typeof(object);
			}
		}

		class MockSite : ISite
		{
			readonly IContainer container;

			public MockSite(IContainer container)
			{ this.container = container; }

			#region ISite Members

			public IComponent Component
			{ get { return null; } }

			public IContainer Container
			{ get { return container; } }

			public bool DesignMode
			{ get { return false; } }

			public string Name
			{
				get { return ""; }
				set { }
			}

			#endregion

			#region IServiceProvider Members

			public object GetService(Type serviceType)
			{ return null; }

			#endregion
		}

		#endregion

		#region Implementation

		TestControlWithNotifications controlWithNotifications;
		TestControlSelfNotificationRendering controlWithNoNotifications;
		TestEntityWithNotifications notificationsEntity;

		void SetupForNotifications()
		{
			controlWithNotifications = new TestControlWithNotifications();
			controlWithNoNotifications = new TestControlSelfNotificationRendering();
			Form.Controls.Add(controlWithNotifications);
			Form.Controls.Add(controlWithNoNotifications);

			object notificationsActive = NotificationProvider;
			BindingSource.SetBindingMember(controlWithNotifications, "Text");
			BindingSource.SetBindingMember(controlWithNoNotifications, "Text");
			notificationsEntity = new TestEntityWithNotifications();

			BindingSource.SetDataBinding(notificationsEntity, "");
			Form.Show();
		}

		TestBindingSource BindingSource
		{
			get
			{
				if (bindingSource == null)
				{
					bindingSource = new TestBindingSource(Form);
				}
				return bindingSource;
			}
		}
		TestBindingSource bindingSource;

		KNotificationProviderForTest NotificationProvider
		{
			get
			{
				if (notificationProvider == null)
				{
					notificationProvider = new KNotificationProviderForTest(Form);
					notificationProvider.NotificationRenderer = NotificationRenderer;
					notificationProvider.BindingSource = BindingSource;
				}
				return notificationProvider;
			}
		}
		KNotificationProviderForTest notificationProvider;

		TestNotificationRenderer NotificationRenderer
		{ get { return notificationRenderer ?? (notificationRenderer = new TestNotificationRenderer()); } }
		TestNotificationRenderer notificationRenderer;

		class KNotificationProviderForTest : KNotificationProvider
		{
			internal KNotificationProviderForTest(ContainerControl containerControl) : base(containerControl) { }

			internal new bool ShouldRenderNotification(Control control, object item, string propertyName) =>
				base.ShouldRenderNotification(control, item, propertyName);
		}

		TestForm Form
		{
			get
			{
				if (form == null)
				{
					form = new TestForm();
					form.Site = Site;
				}
				return form;
			}
		}
		TestForm form;

		readonly Container Container = new Container();

		MockSite Site
		{
			get
			{
				if (site == null)
				{
					site = new MockSite(Container);
				}
				return site;
			}
		}
		MockSite site;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (notificationRenderer != null)
			{
				notificationRenderer.Dispose();
			}
			if (notificationProvider != null)
			{
				notificationProvider.Dispose();
			}
			if (bindingSource != null)
			{
				bindingSource.Dispose();
			}
		}

		#endregion
	}
}
