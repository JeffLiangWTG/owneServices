using System;
using System.Collections;
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

	public class ZListBox : ListBox, ISelfBindingWebControl, INotificationProvider
	{
		#region IBindTo Members

		string fBindTo = "";
		string fBindToList = "";

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public virtual string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindToList
		{
			get { return fBindToList; }
			set { fBindToList = value; }
		}
		#endregion

		#region ISelfBindingPostbackWebControl Members

		public bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(BindTo) && !string.IsNullOrEmpty(BindToList) && dataSource != null;
		}

		public void Bind(object dataSource)
		{
			this.BusinessEntity = dataSource;
			object localDataSource = ZPropertyAccessor.Get(dataSource, BindToList);

			if (localDataSource is IBusinessObjectCollection)
			{
				IBusinessObjectCollection collection = localDataSource as IBusinessObjectCollection;
				BusinessObjectCollection legacyCollection = collection as BusinessObjectCollection;
				if (legacyCollection != null && !(collection is INonPersistentBusinessObjectCollection) && (!collection.IsLoaded
#if DEBUG
 || Enterprise.ZArchitecture.Environment.Globals.IsTest
#endif
					))
				{
					legacyCollection.Load();
				}

				if (AllowSorting)
				{
					collection.ApplySort(new SortInfo(DataTextField, ListSortDirection.Ascending));
				}
			}
			PopulateItemsList(localDataSource as ICollection);

			if (HasChanges)
			{
				ZPropertyAccessor.Set(dataSource, BindTo, GetSelectedValueForSavingIntoBizObject());
				fHasChanges = false;
			}

			if (!string.IsNullOrEmpty(BindTo))
			{
				CachedSelectedValue = new StringConverter().ConvertToString(ZPropertyAccessor.Get(dataSource, BindTo));
				SelectedIndex = Items.IndexOf(Items.FindByValue(CachedSelectedValue));
			}
			Info = (ZPropertyInfo)ZPropertyAccessor.Get(dataSource, BindTo + "Info");
		}

		internal object BusinessEntity
		{
			get { return fBusinessEntity; }
			set { fBusinessEntity = value; }
		}

		object fBusinessEntity;
		ZPropertyInfo Info;

		void PopulateItemsList(ICollection collection)
		{
			if (collection != null)
			{
				bool textAndValueFieldsSet = false;
				bool formattingIsSet = false;

				this.Items.Clear();
				if (collection != null)
				{
					int newCapacity = collection.Count;
					this.Items.Capacity = newCapacity;
				}
				if ((this.DataTextField.Length != 0) || (this.DataValueField.Length != 0))
				{
					textAndValueFieldsSet = true;
				}
				if (this.DataTextFormatString.Length != 0)
				{
					formattingIsSet = true;
				}
				foreach (object obj1 in collection)
				{
					ListItem item1 = new ListItem();
					if (textAndValueFieldsSet)
					{
						if (this.DataTextField.Length > 0)
						{
							item1.Text = DataBinder.GetPropertyValue(obj1, this.DataTextField, this.DataTextFormatString);
						}
						if (this.DataValueField.Length > 0)
						{
							item1.Value = DataBinder.GetPropertyValue(obj1, this.DataValueField, null);
						}
					}
					else
					{
						if (formattingIsSet)
						{
							item1.Text = string.Format(this.DataTextFormatString, obj1);
						}
						else
						{
							item1.Text = obj1.ToString();
						}
						item1.Value = obj1.ToString();
					}
					this.Items.Add(item1);
				}
			}
		}

		protected string CachedSelectedValue
		{
			get
			{
				object valueInViewState = ViewState["CachedSelectedValue"];
				return valueInViewState == null ? "" : valueInViewState.ToString();
			}
			set
			{
				ViewState["CachedSelectedValue"] = value;
			}
		}

		protected virtual object GetSelectedValueForSavingIntoBizObject()
		{
			return new ZString(CachedSelectedValue);
		}

		#region INotificationProvider

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

		public void UnBind()
		{
			this.BusinessEntity = null;
			this.Items.Clear();
			OnSelectedIndexChanged(EventArgs.Empty);
		}

		public bool HasChanges
		{
			get { return fHasChanges; }
			set { fHasChanges = value; }
		}
		internal bool fHasChanges;

		public bool AllowSorting
		{
			get { return fAllowSorting; }
			set { fAllowSorting = value; }
		}
		bool fAllowSorting = true;

		protected override void RaisePostDataChangedEvent()
		{
			this.OnSelectedIndexChanged(EventArgs.Empty);
			if (IsBindable(BusinessEntity))
			{
				Bind(BusinessEntity);
			}
		}

		protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			string value = postCollection[postDataKey];

			if (value != null)
			{
				// If there is a change, raise a change event.
				fHasChanges = (!value.Equals(CachedSelectedValue));
				CachedSelectedValue = value;
				return HasChanges;
			}
			return false;
		}

		#endregion

		#region Control Overrides

		public override string DataTextField
		{
			get
			{
				if (string.IsNullOrEmpty(base.DataTextField))
				{
					return "Description";
				}
				return base.DataTextField;
			}
			set
			{
				base.DataTextField = value;
			}
		}

		public override string DataValueField
		{
			get
			{
				if (string.IsNullOrEmpty(base.DataValueField))
				{
					return "Code";
				}
				return base.DataValueField;
			}
			set
			{
				base.DataValueField = value;
			}
		}

		[Obsolete("Should not be using .NET DataSource because we're doing our own binding using BusinessEntity", true)]
		public new object DataSource
		{
			get { return base.DataSource; }
			set { base.DataSource = value; }
		}

		#endregion Control Overrides
	}

	#endregion
}
