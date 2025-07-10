using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Drop down list on ZWeb forms
	/// </summary>
	public class ZDropDownList : DropDownList, IBindToList, ISelfBindingPostbackWebControl, INotificationProvider, IWebControlWithPostbackNewValue, ITextChangedEvent, IUINotiicationsProvider, IWebNotificationProvider
	{
		public ZDropDownList()
		{
			fDisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
		}

		#region Properties

		protected internal new ZPage Page
		{
			get { return base.Page as ZPage; }
		}

		public bool DisplayNotifications
		{
			get
			{
				return GetDisplayNotifications();
			}
		}

		protected virtual bool GetDisplayNotifications()
		{
			return true;
		}

		public OComboBoxDropDownStyle DisplayStyle
		{
			get { return fDisplayStyle; }
			set { fDisplayStyle = value; }
		}
		protected OComboBoxDropDownStyle fDisplayStyle;

		#endregion

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
			return dataSource != null && !string.IsNullOrEmpty(BindTo) && !string.IsNullOrEmpty(MetadataHelper.GetListMember(this, dataSource));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Calculated property info name")]
		public void Bind(object dataSource)
		{
			if (IsBindable(dataSource))
			{
				BusinessEntity = dataSource;

				string listMember = MetadataHelper.GetListMember(this, dataSource);
				object localDataSource = ZPropertyAccessor.Get(dataSource, listMember);

				IBusinessObjectCollection collection = localDataSource as IBusinessObjectCollection;
				if (collection != null)
				{
					BusinessObjectCollection legacyCollection = collection as BusinessObjectCollection;
					if (legacyCollection != null && !collection.IsLoaded && !(collection is INonPersistentBusinessObjectCollection))
					{
						legacyCollection.Load();
					}
					try
					{
						collection.ApplySort(new SortInfo(DataTextField, ListSortDirection.Ascending));
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
					}
				}
				PopulateItemsList(localDataSource as ICollection);

				if (HasChanges)
				{
					UpdateDataSource(GetSelectedValueForSavingIntoBizObject());
				}

				if (!string.IsNullOrEmpty(BindTo))
				{
					var selectedValue = DataSourceValue;
					if (!IsPostBack)
					{
						CachedSelectedValue = selectedValue;
					}
					var selectedItem = Items.FindByValue(selectedValue);
					if (selectedItem == null)
					{
						foreach (ListItem item in Items)
						{
							if (!string.IsNullOrEmpty(item.Value) && localDataSource is CodeDescriptionPairList)
							{
								if (!(((CodeDescriptionPairList)localDataSource)[item.Value] is CategoryCodeDescriptionPair))
								{
									selectedItem = item;
									HasChanges = true;
									UpdateDataSource(new ZString(selectedItem.Value));
									break;
								}
							}
						}
					}
					SelectedIndex = Items.IndexOf(selectedItem);
					Info = (ZPropertyInfo)ZPropertyAccessor.Get(dataSource, BindTo + "Info");
					Enabled = !Info.ReadOnly;
				}
			}
		}

		internal bool HasDataSource => BusinessEntity != null;

		string DataSourceValue => new StringConverter().ConvertToString(ZPropertyAccessor.Get(BusinessEntity, BindTo));

		bool IsPostBack => Page?.IsPostBack ?? false;

		void UpdateDataSource(object value)
		{
			if (HasChanges)
			{
				ZPropertyAccessor.Set(BusinessEntity, BindTo, value);
				HasChanges = false;
			}
		}

		protected internal object BusinessEntity
		{
			get { return fBusinessEntity; }
			private set { fBusinessEntity = value; }
		}

		object fBusinessEntity;
		protected ZPropertyInfo Info;

		protected virtual string EmptyItemValue
		{
			get { return ""; }
		}

		public bool ShowEmptyItem
		{
			get { return fShowEmptyItem; }
			set { fShowEmptyItem = value; }
		}
		bool fShowEmptyItem;

		public string EmptyItemText
		{
			get { return fEmptyItemText; }
			set { fEmptyItemText = value; }
		}
		string fEmptyItemText = "";

		[DefaultValue("")]
		public string CustomItemValue
		{
			get { return fCustomItemValue; }
			set { fCustomItemValue = value; }
		}
		string fCustomItemValue;

		[DefaultValue("")]
		public string CustomItemText
		{
			get { return fCustomItemText; }
			set { fCustomItemText = value; }
		}
		string fCustomItemText;

		[DefaultValue(false)]
		public bool ShowCustomItem
		{
			get { return fShowCustomItem; }
			set { fShowCustomItem = value; }
		}
		bool fShowCustomItem;

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
					if (ShowEmptyItem)
					{
						newCapacity++;
					}

					this.Items.Capacity = newCapacity;

					if (ShowEmptyItem)
					{
						this.Items.Add(new ListItem(EmptyItemText, EmptyItemValue));
					}

					if (ShowCustomItem)
					{
						this.Items.Capacity = ++newCapacity;
						this.Items.Add(new ListItem(CustomItemText, CustomItemValue));
					}
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
					var dataValueFieldForDisplay = obj1 is CodeDescriptionPair ? "MultilingualCode" : this.DataValueField;
					var dataTextField = GetDataTextField(obj1);

					var item1 = new ListItem();
					if (textAndValueFieldsSet)
					{
						if (this.DataTextField.Length > 0)
						{
							switch (fDisplayStyle)
							{
								case OComboBoxDropDownStyle.DescriptionOnly:
									{
										item1.Text = DataBinder.GetPropertyValue(obj1, dataTextField, this.DataTextFormatString);
										break;
									}
								case OComboBoxDropDownStyle.CodeAndDescription:
									{
										item1.Text = DataBinder.GetPropertyValue(obj1, dataValueFieldForDisplay, null) + " - " + DataBinder.GetPropertyValue(obj1, dataTextField, this.DataTextFormatString);
										break;
									}
								case OComboBoxDropDownStyle.CodeOnly:
									{
										item1.Text = DataBinder.GetPropertyValue(obj1, dataValueFieldForDisplay, null);
										break;
									}
								default:
									{
										item1.Text = DataBinder.GetPropertyValue(obj1, dataTextField, this.DataTextFormatString);
										break;
									}
							}
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

		protected virtual string GetDataTextField(object data)
		{
			return this.DataTextField;
		}

		protected internal string CachedSelectedValue
		{
			get
			{
				object valueInViewState = ViewState["CachedSelectedValue"];

				return valueInViewState?.ToString();
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

		public void EnableDuplicateValues(HiddenField hiddenField)
		{
			hiddenField.ID = $"{ID}_SelectedIndex"; // constant
			hiddenField.ValueChanged += SelectedIndexChangedEventHandler;

			if (!Page.ZClientScript.IsClientScriptBlockRegistered(typeof(ZDropDownList), StoreSelectedIndexScriptKey))
			{
				Page.ZClientScript.RegisterClientScriptBlock(typeof(ZDropDownList), StoreSelectedIndexScriptKey, StoreSelectedIndexScript, false);
			}

			Attributes["onchange"] += $"StoreSelectedIndex(this, \"{hiddenField.ClientID}\");"; // javascript
		}

		void SelectedIndexChangedEventHandler(object sender, EventArgs e)
		{
			var hf = (HiddenField)sender;
			if (int.TryParse(hf.Value, out var cachedSelectedIndex) && cachedSelectedIndex >= 0 && cachedSelectedIndex < Items.Count)
			{
				var value = Items[cachedSelectedIndex]?.Value;
				if (value == SelectedValue)
				{
					SelectedIndex = cachedSelectedIndex;
				}
			}
		}

		const string StoreSelectedIndexScript = @"
<script type=""text/javascript"">
function StoreSelectedIndex(select, hiddenFieldId) {
	document.getElementById(hiddenFieldId).value = select.selectedIndex;
}
</script>
";
		public const string StoreSelectedIndexScriptKey = "ZDropDownList_SelectedIndexCache";

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
			Enabled = false;
		}

		public bool HasChanges { get; set; }

		public void RaisePostDataChangedEventInternal()
		{
			RaisePostDataChangedEvent();
		}

		protected override void RaisePostDataChangedEvent()
		{
			this.OnSelectedIndexChanged(EventArgs.Empty);
			if (IsBindable(BusinessEntity))
			{
				Bind(BusinessEntity);
			}
			if (PostDataChanged != null)
			{
				PostDataChanged(this, new EventArgs());
			}
		}

		public event EventHandler PostDataChanged;

		protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			if (!HasDataSource)
			{
				base.LoadPostData(postDataKey, postCollection);
			}

			var value = postCollection[postDataKey];
			HasChanges = GetValueHasChanges(value);
			CachedSelectedValue = value;

			return HasChanges;
		}

		bool GetValueHasChanges(string value)
		{
			if (value == null)
			{
				return false;
			}

			if (!HasDataSource)
			{
				return !ValuesEqual(value, CachedSelectedValue ?? string.Empty);
			}

			return !ValuesEqual(value, DataSourceValue) && !ValuesEqual(value, CachedSelectedValue);
		}

		protected virtual bool ValuesEqual(string newValue, string oldValue)
		{
			return newValue.Equals(oldValue);
		}

		#endregion

		#region Control Overrides

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "javascript function text should not be translated")]
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			this.Attributes.Add("onchange", "ClearValidation('" + NotificationID + "')" + (this.Attributes["onchange"] != null ? ";" + this.Attributes["onchange"] : ""));
			if ((Info != null))
			{
				Enabled = !Info.ReadOnly;
			}
		}

		public override void RenderControl(HtmlTextWriter writer)
		{
			if ((Info != null && Info.ReadOnly))
			{
				Label readOnlyLabel = new Label();
				readOnlyLabel.Text = Text;
				readOnlyLabel.Visible = Visible;
				readOnlyLabel.RenderControl(writer);
			}
			else
			{
				base.RenderControl(writer);
			}
		}

		public override string Text
		{
			get { return SelectedItemText ?? base.Text; }
		}

		string SelectedItemText
		{
			get
			{
				string result = null;
				if (SelectedIndex >= 0 && Items.Count > SelectedIndex)
				{
					result = Items[SelectedIndex].Text;
				}
				return result;
			}
		}

		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			Page.RenderPageControlBeginTag(writer, this);
			base.RenderBeginTag(writer);
		}

		public override void RenderEndTag(HtmlTextWriter writer)
		{
			base.RenderEndTag(writer);
			Page.RenderPageControlEndTag(writer, this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Calculated field name")]
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Calculated field name")]
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

		#region IWebControlWithPostbackNewValue Members

		ZString IWebControlWithPostbackNewValue.NewValue
		{
			get { return CachedSelectedValue; }
		}

		#endregion

		#region IWebNotificationProvider Members

		public string NotificationID
		{
			get
			{
				return ClientID + "_NotificationID";
			}
		}

		#endregion

		#region Internal Properties

		internal bool LoadPostDataInternal(string postDataKey, NameValueCollection postCollection) => LoadPostData(postDataKey, postCollection);

		#endregion
	}
}
