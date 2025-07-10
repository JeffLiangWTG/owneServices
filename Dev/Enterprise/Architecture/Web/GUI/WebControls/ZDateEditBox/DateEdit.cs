using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZDateEdit : System.Web.UI.WebControls.WebControl, INotificationProvider, ISelfBindingPostbackWebControl, IDateInputControl, INamingContainer, IFocusCompositeControl, ITextChangedEvent, IContainResources
	{
		public event EventHandler TextChanged;

		protected void OnTextChanged(object sender, EventArgs e)
		{
			if (TextChanged != null)
			{
				TextChanged(this, e);
			}
		}

		public bool CanBeEnabledByClient
		{
			get
			{
				return DateBox.CanBeEnabledByClient;
			}
			set
			{
				DateBox.CanBeEnabledByClient = value;
			}
		}

		public string Text
		{
			get
			{
				if (DateTimeFormat != ZDateTimePickerFormat.Short)
				{
					if (DateTimeFormat == ZDateTimePickerFormat.IncludingTimeZone)
					{
						return DateBox.Text + " " + TimeBox.Text + " " + TimezoneBox.Text;
					}

					return DateBox.Text + " " + TimeBox.Text;
				}
				return DateBox.Text;
			}
		}

		public ZTextBox TextBoxControl
		{
			get { return DateBox.TextBoxControl; }
		}

		public bool ReadOnly
		{
			get { return DateBox.ReadOnly; }
			set { DateBox.ReadOnly = value; }
		}

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(ZDateTimePickerFormat.Short)]
		public ZDateTimePickerFormat DateTimeFormat
		{
			get { return fDateTimeFormat; }
			set
			{
				if (fDateTimeFormat != value)
				{
					fDateTimeFormat = value;
					DateBox.DateTimeFormat = value;
				}
				TimeBox.Visible = fDateTimeFormat != ZDateTimePickerFormat.Short;
				TimezoneBox.Visible = fDateTimeFormat == ZDateTimePickerFormat.IncludingTimeZone;
			}
		}
		ZDateTimePickerFormat fDateTimeFormat = ZDateTimePickerFormat.Short;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "css style value should not be translated")]
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			Style[HtmlTextWriterStyle.Display] = "inline";

			if (CanBeEnabledByClient)
			{
				Attributes.Add("enabled-display", Style[HtmlTextWriterStyle.Display]);
				if (DateBox.DataReadOnly)
				{
					Style[HtmlTextWriterStyle.Display] = "none";
				}
			}
		}

		public override void RenderControl(HtmlTextWriter writer)
		{
			if (ReadOnly)
			{
				DateBox.RenderControl(writer);
			}
			else
			{
				base.RenderControl(writer);
			}
		}

		#region ChildControls

		internal ZDateEditBox DateBox
		{
			get
			{
				if (dateBox == null)
				{
					dateBox = new ZDateEditBox { DateTimeFormat = DateTimeFormat };
					dateBox.TextChanged += OnTextChanged;
				}
				return dateBox;
			}
		}
		ZDateEditBox dateBox;

		internal ZTimeEditList TimeBox
		{
			get
			{
				if (timeBox == null)
				{
					timeBox = new ZTimeEditList(DateBox) { Visible = DateTimeFormat != ZDateTimePickerFormat.Short };
					timeBox.TextChanged += OnTextChanged;
				}
				return timeBox;
			}
		}
		ZTimeEditList timeBox;

		internal ZTimezoneEditList TimezoneBox
		{
			get
			{
				if (timezoneBox == null)
				{
					timezoneBox = new ZTimezoneEditList(DateBox) { Visible = DateTimeFormat == ZDateTimePickerFormat.IncludingTimeZone };
					timezoneBox.TextChanged += OnTextChanged;
				}
				return timezoneBox;
			}
		}
		ZTimezoneEditList timezoneBox;

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			Controls.Add(DateBox);
			Controls.Add(TimeBox);
			Controls.Add(TimezoneBox);
		}

		#endregion

		#region ISelfBindingWebControl Members

		public bool HasChanges
		{
			get { return DateBox.HasChanges || TimeBox.HasChanges || TimezoneBox.HasChanges; }
			set
			{
				DateBox.HasChanges = value;
				TimeBox.HasChanges = value;
				TimezoneBox.HasChanges = value;
			}
		}

		public bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(BindTo) && dataSource != null && dataSource is BusinessObject;
		}

		public void Bind(object dataSource)
		{
			EnsureChildControls();
			DateBox.Bind(dataSource);
			TimeBox.Bind(dataSource);
			TimezoneBox.Bind(dataSource);
		}

		public void UnBind()
		{
			EnsureChildControls();
			DateBox.UnBind();
			TimeBox.UnBind();
			TimezoneBox.UnBind();
		}

		#endregion

		#region IBindTo Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return bindTo; }
			set
			{
				bindTo = value;
				DateBox.BindTo = value;
				TimeBox.BindTo = value;
				TimezoneBox.BindTo = value;
			}
		}
		string bindTo = string.Empty;

		public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			return DateBox.LoadPostData(postDataKey, postCollection) || TimeBox.LoadPostData(postDataKey, postCollection) || TimezoneBox.LoadPostData(postDataKey, postCollection);
		}

		public void RaisePostDataChangedEvent()
		{
			DateBox.RaisePostDataChangedEvent();
			TimeBox.RaisePostDataChangedEvent();
			TimezoneBox.RaisePostDataChangedEvent();
		}

		#endregion

		#region IDateInputControl Members

		public int AutoCompleteMonthThreshold
		{
			get { return DateBox.AutoCompleteMonthThreshold; }
		}

		public bool AutoCompleteYear
		{
			get { return DateBox.AutoCompleteYear; }
		}

		public string FormatString
		{
			get { return DateBox.FormatString; }
		}

		#endregion

		#region IFocusCompositeControl Members

		public string ChildControlIDForFocus
		{
			get { return DateBox.ID; }
		}

		#endregion

		#region INotificationProvider Members

		public INotificationType GetHighestSeverityNotificationType()
		{
			return Notifications.GetHighestSeverityNotificationType();
		}

		public bool HasNotifications(INotificationType type)
		{
			return ((INotificationProvider)DateBox).HasNotifications(type) || ((INotificationProvider)TimeBox).HasNotifications(type) || ((INotificationProvider)TimezoneBox).HasNotifications(type);
		}

		public bool HasNotifications()
		{
			return ((INotificationProvider)DateBox).HasNotifications() || ((INotificationProvider)TimeBox).HasNotifications() || ((INotificationProvider)TimezoneBox).HasNotifications();
		}

		public IEnumerable<INotification> Notifications
		{
			get
			{
				foreach (INotification notification in DateBox.Notifications)
				{
					yield return notification;
				}
				foreach (INotification notification in TimeBox.Notifications)
				{
					yield return notification;
				}
				foreach (INotification notification in TimezoneBox.Notifications)
				{
					yield return notification;
				}
			}
		}

		#endregion

		#region IContainResources Members

		public ZWebResourceCollection Resources
		{
			get
			{
				EnsureChildControls();
				ZWebResourceCollection result = DateBox.Resources;
				foreach (ZWebResource resource in TimeBox.Resources)
				{
					result.Add(resource);
				}
				foreach (ZWebResource resource in TimezoneBox.Resources)
				{
					result.Add(resource);
				}
				return result;
			}
		}

		#endregion
	}
}
