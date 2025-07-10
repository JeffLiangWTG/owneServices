using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Summary description for ZCheckBox.
	/// </summary>
	[DefaultProperty("Text"), ToolboxData("<{0}:ZCheckBox runat=server></{0}:ZCheckBox>")]
	public class ZCheckBox : CheckBox, ISelfBindingPostbackWebControl, INotificationProvider
	{
		#region IBindTo

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}
		string fBindTo = "";

		#endregion

		#region Properties

		new ZPage Page
		{
			get
			{
				return (ZPage)base.Page;
			}
		}

		#endregion

		#region Overrides

		public override void RenderControl(HtmlTextWriter writer)
		{
			if (Page != null)
			{
				Page.RenderPageControlBeginTag(writer, this);
			}

			if ((Info != null && Info.ReadOnly))
			{
				Label textLabel = new Label();
				textLabel.Text = Text + ": ";
				textLabel.CssClass = CssClass;
				textLabel.Visible = Visible;
				textLabel.RenderControl(writer);

				Label valueLabel = new Label();
				valueLabel.Text = Checked ? "Y" : "N";
				valueLabel.Visible = Visible;
				valueLabel.RenderControl(writer);
			}
			else
			{
				base.RenderControl(writer);
			}

			if (Page != null)
			{
				Page.RenderPageControlEndTag(writer, this);
			}
		}

		#endregion

		#region ISelfBindingPostbackWebControl Members

		public bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(BindTo) && dataSource != null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Calculated property info name")]
		public void Bind(object dataSource)
		{
			this.BusinessEntity = dataSource;
			if (HasChanges)
			{
				fHasChanges = false;
				ZPropertyAccessor.Set(dataSource, BindTo, new ZBool(Checked));
			}
			else
			{
				Checked = (ZBool)ZPropertyAccessor.Get(dataSource, BindTo);
			}
			Info = (ZPropertyInfo)ZPropertyAccessor.Get(dataSource, BindTo + "Info");
		}
		internal ZPropertyInfo Info;

		public void UnBind()
		{
			BusinessEntity = null;
			fBindTo = "";
		}

		public bool HasChanges
		{
			get { return fHasChanges; }
			set { fHasChanges = value; }
		}
		internal bool fHasChanges;

		bool isPostDataLoaded;
		protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			if (!isPostDataLoaded)
			{
				fHasChanges = base.LoadPostData(postDataKey, postCollection);

				isPostDataLoaded = true;
			}

			return fHasChanges;
		}

		public event EventHandler PostDataChanged;

		protected override void RaisePostDataChangedEvent()
		{
			base.RaisePostDataChangedEvent();
			if (IsBindable(BusinessEntity))
			{
				Bind(BusinessEntity);
			}
			if (PostDataChanged != null)
			{
				PostDataChanged(this, new EventArgs());
			}
		}

		internal object BusinessEntity
		{
			get { return fBusinessEntity; }
			set { fBusinessEntity = value; }
		}

		object fBusinessEntity;

		#endregion

		#region INotificationProvider Members

		public IEnumerable<INotification> Notifications
		{
			get { return Info != null ? Info.Notifications : NotificationCollection.Empty; }
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
	}
}
