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
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// Summary description for ZRadioButton.
	/// </summary>
	[ToolboxData("<{0}:ZRadioButton runat=server></{0}:ZRadioButton>")]
	public class ZRadioButton : RadioButton, ISelfBindingPostbackWebControl, INotificationProvider
	{
		public ZRadioButton()
		{
			this.EnableViewState = true;
		}

		#region ISelfBindingPostbackWebControl Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}
		string fBindTo = "";

		public bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(BindTo) && dataSource != null;
		}

		public void Bind(object dataSource)
		{
			this.BusinessEntity = dataSource;
			if (HasChanges)
			{
				ZPropertyAccessor.Set(dataSource, BindTo, new ZBool(this.Checked));
				fHasChanges = false;
			}
			else
			{
				this.Checked = ((ZBool)ZPropertyAccessor.Get(dataSource, BindTo));
			}
			Info = (ZPropertyInfo)ZPropertyAccessor.Get(dataSource, BindTo + "Info");
		}
		internal ZPropertyInfo Info;

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

		public void UnBind()
		{
			BusinessEntity = null;
			BindTo = "";
		}

		public bool HasChanges
		{
			get { return fHasChanges; }
			set { fHasChanges = value; }
		}
		internal bool fHasChanges;

		public bool ProcessPostData(string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData(postDataKey, postCollection);
		}

		protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			string selectedButtonName = postCollection[this.UniqueGroupName];
			bool result = false;
			if ((selectedButtonName != null) && selectedButtonName.Equals(this.ValueAttribute))
			{
				if (!this.Checked)
				{
					this.Checked = true;
					result = true;
					fHasChanges = true;
				}
			}
			else if (this.Checked)
			{
				this.Checked = false;
				fHasChanges = true;
			}

			if (HasChanges && IsBindable(BusinessEntity))
			{
				Bind(BusinessEntity);
			}

			return result;
		}

		string ValueAttribute
		{
			get
			{
				string result = base.Attributes["value"];
				if (result != null)
				{
					return result;
				}
				if (this.ID != null)
				{
					return this.ID;
				}
				return this.UniqueID;
			}
		}

		internal string UniqueGroupName
		{
			get
			{
				if (fUniqueGroupName == null)
				{
					string text1 = this.GroupName;
					string text2 = this.UniqueID;
					if (text2 != null)
					{
						int num1 = text2.LastIndexOf(base.IdSeparator);
						if (num1 >= 0)
						{
							if (text1.Length > 0)
							{
								text1 = text2.Substring(0, num1 + 1) + text1;
							}
							else if (this.NamingContainer is RadioButtonList)
							{
								text1 = text2.Substring(0, num1);
							}
						}
						if (text1.Length == 0)
						{
							text1 = text2;
						}
					}
					fUniqueGroupName = text1;
				}
				return fUniqueGroupName;
			}
		}
		string fUniqueGroupName;

		internal object BusinessEntity
		{
			get { return fBusinessEntity; }
			set { fBusinessEntity = value; }
		}

		object fBusinessEntity;

		#endregion
	}

	#endregion
}
