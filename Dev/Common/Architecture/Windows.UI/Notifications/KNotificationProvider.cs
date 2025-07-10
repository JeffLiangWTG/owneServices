using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Provides rendered notifications on the controls managed by an
	/// ICompositeControlBindingSource (such as KBindingSource).
	/// </summary>
	public class KNotificationProvider : Component, INotificationRenderContext
	{
		#region Constructors

		public KNotificationProvider()
		{
		}

		public KNotificationProvider(IContainer container)
		{ container.Add(this); }

		public KNotificationProvider(ContainerControl containerControl)
		{ this.ContainerControl = containerControl; }

		#endregion

		#region BindingSource

		/// <summary>
		/// Get or set the binding source whose data source and controls will be used to
		/// render the notifications.
		/// </summary>
		[Category(DesignerConstants.Category)]
		public ICompositeControlBindingSource BindingSource
		{
			get { return bindingSource; }
			set
			{
				if (BindingSource != value)
				{
					if (BindingSource != null)
					{
						BindingSource.DataSourceChanged -= BindingSource_DataSourceChanged;
						BindingSource.FullBindingMemberChanged -= BindingSource_FullBindingMemberChanged;
						IComponent bindingSourceComponent = BindingSource as IComponent;
						if (bindingSourceComponent != null)
						{
							bindingSourceComponent.Disposed -= BindingSource_Disposed;
						}
						DataSource = null;
						RecreateRenderers();
					}
					this.bindingSource = value;
					if (BindingSource != null)
					{
						BindingSource.DataSourceChanged += BindingSource_DataSourceChanged;
						BindingSource.FullBindingMemberChanged += BindingSource_FullBindingMemberChanged;
						IComponent bindingSourceComponent = BindingSource as IComponent;
						if (bindingSourceComponent != null)
						{
							bindingSourceComponent.Disposed += BindingSource_Disposed;
						}
						DataSource = BindingSource.DataSource;
						RecreateRenderers();
					}
				}
			}
		}
		ICompositeControlBindingSource bindingSource;

		#endregion

		#region ContainerControl

		[Browsable(false)]
		public ContainerControl ContainerControl { get; set; }

		#endregion

		#region Site / IsDesignMode

		public override ISite Site
		{
			get { return base.Site; }
			set
			{
				base.Site = value;
				if (value != null)
				{
					IDesignerHost host = value.GetService(typeof(IDesignerHost)) as IDesignerHost;
					if (host != null)
					{
						ContainerControl root = host.RootComponent as ContainerControl;
						if (root != null)
						{
							ContainerControl = root;
						}
					}

					if (BindingSource == null)
					{
						ICompositeControlBindingSource newBindingSource = null;
						foreach (IComponent component in value.Container.Components)
						{
							newBindingSource = component as ICompositeControlBindingSource;
							if (newBindingSource != null)
							{
								break;
							}
						}
						this.BindingSource = newBindingSource;
					}
				}
				isDesignModePopulated = false;
			}
		}

		internal bool IsDesignMode
		{
			get
			{
				if (!isDesignModePopulated)
				{
					isDesignMode = this.IsDesignMode();
					isDesignModePopulated = true;
				}
				return isDesignMode;
			}
		}
		bool isDesignMode;
		bool isDesignModePopulated;

		#endregion

		#region NotificationRenderer

		/// <summary>
		/// Get or set the component that renders the notifications to the target control.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Renderer")]
		[Category(DesignerConstants.Category)]
		public INotificationRenderer NotificationRenderer
		{
			get
			{
				if (!notificationRendererSet && notificationRenderer == null)
				{
					notificationRenderer = NotificationRenderer = new NotificationRenderer();
				}
				return notificationRenderer;
			}
			set
			{
				if (notificationRenderer != value)
				{
					IComponent notificationRendererComponent = notificationRenderer as IComponent;
					if (notificationRendererComponent != null)
					{
						notificationRendererComponent.Disposed -= new EventHandler(NotificationRenderer_Disposed);
					}
					notificationRenderer = value;
					notificationRendererSet = true;
					notificationRendererComponent = notificationRenderer as IComponent;
					if (notificationRendererComponent != null)
					{
						notificationRendererComponent.Disposed += new EventHandler(NotificationRenderer_Disposed);
					}
				}
			}
		}
		INotificationRenderer notificationRenderer;
		bool notificationRendererSet;

		void NotificationRenderer_Disposed(object sender, EventArgs e)
		{ NotificationRenderer = null; }

		#endregion

		#region BindingContext

		[Browsable(false)]
		public BindingContext BindingContext
		{
			get
			{
				if (bindingContext == null && DataSource != null)
				{
					bindingContext = ContainerControl == null ? new BindingContext() : ContainerControl.BindingContext;
				}
				return bindingContext;
			}
		}
		BindingContext bindingContext;

		#endregion

		#region RenderNotificationsAfterFocus

		[DefaultValue(true)]
		public bool RenderNotificationsAfterFocus
		{
			get { return renderNotificationsAfterFocus; }
			set
			{
				renderNotificationsAfterFocus = value;
				dataItemsFocused.Clear();
			}
		}
		bool renderNotificationsAfterFocus = true;

		#endregion

		#region SetControlNotifications

		internal void SetControlNotifications(ControlNotificationProvider controlRenderer, NotificationCollection notifications)
		{
			Control notificationTarget = controlRenderer.NotificationTarget;
			if (notificationTarget != null && notifications != null)
			{
				NotificationRenderer.SetNotifications(notificationTarget, notifications);
			}
		}

		#endregion

		#region ExposeAllNotifications

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule")]
		public void ExposeAllNotifications()
		{
			INotificationRenderContext renderContext = this;
			foreach (ControlNotificationProvider renderer in this.controlNotifications.Values)
			{
				Control control = renderer.Control;
				while (control is INotificationRenderTarget && control != null)
				{
					control = (control as INotificationRenderTarget).ControlToRenderNotificationOn;
				}

				ISelfNotificationRendering selfRenderingNotifications = control as ISelfNotificationRendering;
				if (selfRenderingNotifications != null)
				{
					object[] items = selfRenderingNotifications.GetCurrentlyFocusableDataItems();
					if (items != null)
					{
						foreach (object item in items)
						{
							renderContext.NotifyDataControlFocusedForAllProperties(control, item);
						}
					}
					selfRenderingNotifications.ExposeAllNotifications();
				}
				if (selfRenderingNotifications == null ||
						selfRenderingNotifications.DefaultRenderingEnabled)
				{
					IDataBoundControl boundControl = DataBoundControl.Get(renderer.Control);
					if (boundControl.DataSource != null)
					{
						BindingManagerBase bm = BindingContext[boundControl.DataSource, boundControl.DataMember];
						CurrencyManager cm = bm as CurrencyManager;
						if (cm != null)
						{
							BindingMemberInfo member = new BindingMemberInfo(boundControl.DataMember);
							BindingManagerBase parentBM = BindingContext[boundControl.DataSource, member.BindingPath];
							renderContext.NotifyDataControlFocused(renderer.Control, parentBM, AllPropertiesDummyPropertyName);
							foreach (object item in cm.List)
							{
								renderContext.NotifyDataControlFocusedForAllProperties(renderer.Control, item);
							}
						}
						else if (bm.Current != null)
						{
							renderContext.NotifyDataControlFocusedForAllProperties(renderer.Control, bm.Current);
						}
						renderer.UpdateNotifications();
					}
				}
				control.Invalidate();
			}
		}

		#endregion

		#region BindingSource events

		void BindingSource_FullBindingMemberChanged(object sender, ControlBindingMemberChangedEventArgs e)
		{
			if (e.Control == null)
			{
				RecreateRenderers();
			}
			else
			{
				ControlNotificationProvider controlNotification = null;
				if (controlNotifications.TryGetValue(e.Control, out controlNotification))
				{
					controlNotification.Dispose();
				}
				if (string.IsNullOrEmpty(e.BindingMember))
				{
					controlNotifications.Remove(e.Control);
				}
				else
				{
					controlNotification = new ControlNotificationProvider(this, e.Control, BindingSource.DataSource, e.BindingMember);
					controlNotification.Enabled = true;
					controlNotifications[e.Control] = controlNotification;
				}
			}
		}

		void BindingSource_DataSourceChanged(object sender, EventArgs e)
		{
			DataSource = BindingSource.DataSource;
			dataItemsFocused.Clear();
		}

		void BindingSource_Disposed(object sender, EventArgs e)
		{ BindingSource = null; }

		#endregion

		#region INotificationRenderContext

		void INotificationRenderContext.NotifyDataControlFocused(Control control, object item, string propertyName)
		{ NotifyDataControlFocused(control, item, propertyName); }

		void INotificationRenderContext.NotifyDataControlFocusedForAllProperties(Control control, object item)
		{ NotifyDataControlFocusedForAllProperties(control, item); }

		protected void NotifyDataControlFocusedForAllProperties(Control control, object item)
		{ NotifyDataControlFocused(control, item, AllPropertiesDummyPropertyName); }

		protected void NotifyDataControlFocused(Control control, object item, string propertyName)
		{
			if (this.RenderNotificationsAfterFocus)
			{
				FocusedDataEntry entry = null;
				dataItemsFocused.TryGetValue(item, out entry);
				if (entry == null)
				{
					entry = new FocusedDataEntry(control, propertyName);
					dataItemsFocused.Add(item, entry);
				}
				else
				{
					entry.AddPropertyName(propertyName);
					entry.AddControl(control);
				}
			}
		}

		void INotificationRenderContext.NotifyDataItemDeleted(Control control, object item)
		{ NotifyDataItemDeleted(control, item); }

		protected void NotifyDataItemDeleted(Control control, object item)
		{
			if (this.RenderNotificationsAfterFocus)
			{
				FocusedDataEntry list = null;
				dataItemsFocused.TryGetValue(item, out list);
				if (list != null)
				{
					list.RemoveControl(control);
					if (list.IsEmpty)
					{
						dataItemsFocused.Remove(item);
					}
				}
			}
		}

		bool INotificationRenderContext.ShouldAlwaysRenderAllNotifications()
		{ return ShouldAlwaysRenderAllNotifications(); }

		protected bool ShouldAlwaysRenderAllNotifications()
		{ return !this.RenderNotificationsAfterFocus; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "control")]
		bool INotificationRenderContext.ShouldRenderNotification(Control control, object item, string propertyName)
		{ return ShouldRenderNotification(control, item, propertyName); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "control")]
		protected bool ShouldRenderNotification(Control control, object item, string propertyName)
		{
			bool result = true;
			if (this.RenderNotificationsAfterFocus)
			{
				FocusedDataEntry entry = null;
				dataItemsFocused.TryGetValue(item, out entry);
				result = entry != null && entry.ContainsPropertyName(propertyName);
			}
			return result;
		}

		bool INotificationRenderContext.ShouldRenderAnyNotificationsFrom(Control control, object item)
		{ return ShouldRenderAnyNotificationsFrom(control, item); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "control")]
		protected bool ShouldRenderAnyNotificationsFrom(Control control, object item)
		{ return !RenderNotificationsAfterFocus || dataItemsFocused.ContainsKey(item); }

		sealed class FocusedDataEntry
		{
			public FocusedDataEntry(Control control, string propertyName)
			{
				propertyNames.Add(propertyName);
				if (propertyName == AllPropertiesDummyPropertyName)
				{
					this.allPropertiesExposed = true;
				}
				controls.Add(control);
			}

			public void AddPropertyName(string propertyName)
			{
				if (!propertyNames.Contains(propertyName))
				{
					propertyNames.Add(propertyName);
				}
				if (propertyName == AllPropertiesDummyPropertyName)
				{
					this.allPropertiesExposed = true;
				}
			}

			public void AddControl(Control control)
			{
				if (!controls.Contains(control))
				{
					controls.Add(control);
					controls.TrimExcess();
				}
			}

			public void RemoveControl(Control control)
			{ this.controls.Remove(control); }

			public bool ContainsPropertyName(string propertyName)
			{ return this.allPropertiesExposed || propertyNames.Contains(propertyName); }

			public bool IsEmpty
			{ get { return this.controls.Count == 0 || this.propertyNames.Count == 0; } }

			bool allPropertiesExposed;
			readonly StringCollection propertyNames = new StringCollection();
			readonly List<Control> controls = new List<Control>(1);
		}

		#endregion

		#region IDisposable

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ContainerControl = null;
				BindingSource = null;
				IDisposable renderer = NotificationRenderer as IDisposable;
				if (renderer != null)
				{
					renderer.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Implementation

		const string AllPropertiesDummyPropertyName = "all_properties_dummy_property_name";

		readonly Dictionary<Control, ControlNotificationProvider> controlNotifications = new Dictionary<Control, ControlNotificationProvider>();
		readonly Dictionary<object, FocusedDataEntry> dataItemsFocused = new Dictionary<object, FocusedDataEntry>();

		object DataSource
		{
			get { return dataSource; }
			set
			{
				if (DataSource != value)
				{
					if (DataSource != null)
					{
						IDataSourceEvents oldEntity = DataSource as IDataSourceEvents;
						if (oldEntity != null)
						{
							oldEntity.DataSourcePosting -= new EventHandler(DataSource_Posting);
						}
					}
					this.dataSource = value;
					if (DataSource != null)
					{
						IDataSourceEvents newEntity = DataSource as IDataSourceEvents;
						if (newEntity != null)
						{
							newEntity.DataSourcePosting += new EventHandler(DataSource_Posting);
						}
					}
					RecreateRenderers();
				}
			}
		}
		object dataSource;

		void RecreateRenderers()
		{
			foreach (ControlNotificationProvider controlNotification in controlNotifications.Values)
			{
				controlNotification.Dispose();
			}
			controlNotifications.Clear();
			if (BindingSource != null && BindingSource.DataSource != null)
			{
				foreach (KeyValuePair<Control, string> member in BindingSource.FullBindingMembers)
				{
					ControlNotificationProvider renderer = new ControlNotificationProvider(this, member.Key, BindingSource.DataSource, member.Value);
					renderer.Enabled = true;
					controlNotifications.Add(member.Key, renderer);
				}
			}
		}

		void DataSource_Posting(object sender, EventArgs e)
		{
			if (RenderNotificationsAfterFocus)
			{
				ExposeAllNotifications();
			}
		}

		#endregion
	}
}
