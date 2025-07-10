using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// Base ZLabel for Web forms. 
	/// You need to subclass it in order to create your concrete implementation of label
	/// </summary>
	public abstract class ZLabelBase : System.Web.UI.WebControls.Label, ISelfBindingWebControl, INotificationProvider, IHtmlEncodableLabelControl
	{
		public ZLabelBase()
		{
			EnableHtmlEncoding = true;
		}

		#region ZPage

		protected new ZPage Page
		{
			get { return base.Page as ZPage; }
		}

		#endregion

		#region IBindTo

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public virtual string BindTo
		{
			get { return bindTo; }
			set { bindTo = value; }
		}
		string bindTo = "";

		#endregion

		#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(BindTo) && dataSource != null;
		}

		public void UnBind()
		{
			BindTo = null;
			Info = null;
			ClearText();
		}

		public virtual void Bind(object dataSource)
		{
			try
			{
				SetTextProperty(dataSource);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ClearText();
			}

			try
			{
				Info = (ZPropertyInfo)ZPropertyAccessor.GetWithEncode(dataSource, BindTo + "Info");
			}
			catch (ArgumentException)
			{
				//ignore it for labels (required if we need to bind to items from DocWrappers
			}
		}
		ZPropertyInfo Info
		{
			get { return info; }
			set { info = value; }
		}
		ZPropertyInfo info;

		public IEnumerable<INotification> Notifications
		{
			get
			{
				if (Info != null)
				{
					if (!(Info is ZWrappedPropertyInfo) || (((ZWrappedPropertyInfo)Info).InnerInfo != null))
					{
						return Info.Notifications;
					}
				}
				return NotificationCollection.Empty;
			}
		}

		bool INotificationProvider.HasNotifications()
		{
			return Info.HasNotifications();
		}

		bool INotificationProvider.HasNotifications(INotificationType type)
		{
			return Notifications.HasNotifications(type);
		}

		INotificationType INotificationProvider.GetHighestSeverityNotificationType()
		{
			return Info.GetHighestSeverityNotificationType();
		}

		#endregion

		#region IHtmlEncodableLabelControl Members

		public bool EnableHtmlEncoding
		{
			get;
			set;
		}

		#endregion

		#region Text handling routines

		protected virtual void SetTextProperty(object dataSource)
		{
			IZType propertyValue = GetPropertyValueObject(dataSource);
			Text = GetText(propertyValue);
			ToolTip = GetToolTip(propertyValue);
		}

		void ClearText()
		{
			Text = "";
			ToolTip = "";
		}

		public override string Text
		{
			get
			{
				return this.GetHtmlEncodableLabelContent(base.Text);
			}
			set
			{
				base.Text = value;
			}
		}

		protected virtual IZType GetPropertyValueObject(object dataSource)
		{
			return (IZType)ZPropertyAccessor.Get(dataSource, BindTo);
		}

		protected internal virtual string GetText(IZType value)
		{
			string stringValue;

			if (value != null && value.Equals(SuppressUtil.SuppressedDateTime))
			{
				stringValue = SuppressUtil.SuppressedText;
			}
			else
			{
				stringValue = new StringConverter().ConvertToString(value);
			}

			return stringValue;
		}

		protected virtual string GetToolTip(IZType value)
		{
			return GetText(value);
		}

		#endregion

		[DefaultValue(false)]
		public bool ShowNotifications { get; set; }

		public bool HideIfBlank { get; set; }

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			if (this.Visible && HideIfBlank && string.IsNullOrEmpty(Text))
			{
				this.Visible = false;
			}
		}

		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			if (ShowNotifications && Page != null)
			{
				Page.RenderPageControlBeginTag(writer, this);
			}
			base.RenderBeginTag(writer);
		}

		public override void RenderEndTag(HtmlTextWriter writer)
		{
			base.RenderEndTag(writer);
			if (ShowNotifications && Page != null)
			{
				Page.RenderPageControlEndTag(writer, this);
			}
		}
	}

	#endregion
}
