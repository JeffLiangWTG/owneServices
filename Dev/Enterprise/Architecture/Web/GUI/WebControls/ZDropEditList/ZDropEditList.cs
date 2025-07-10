using System;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI.WebControls;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// Summary description for ZDropEditList.
	/// </summary>
	public class ZDropEditList : ZTextBoxButton, IBindToList, IContainResources, ISelfBindingWebControl
	{
		#region Control Overrides

		#region Selected value

		protected override IZType GetSelectedValue()
		{
			return (ZString)TextBoxControl.Text.Trim(Whitespace);
		}

		protected override string GetTextFromValue(IZType newValue)
		{
			return (newValue is ZString) ? ((ZString)newValue).Trim(Whitespace) : null;
		}

		#endregion

		protected override Unit MinWidth
		{
			get { return Unit.Pixel(40); }
		}

		protected override string ButtonBackgroundStyle
		{
			get { return string.Empty; }
		}

		#endregion Control Overrides

		#region Rendering and Controls Creation

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			RegisterPopupScript();
			ListBoxControl.Attributes.Add("onclick", SelectItemHandler);
			ListBoxControl.Attributes.Add("onblur", OnBLurHandler);
			ListBoxControl.Attributes.Add("onkeydown", KeysHandler);
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			CreatePanelControls();
			Controls.Add(DivPanel);
		}

		protected virtual void CreatePanelControls()
		{
			DivPanel.ID = "Popup";
			DivPanel.Style.Add("z-index", "999");
			DivPanel.Style.Add("visibility", "hidden");
			DivPanel.Style.Add("position", "absolute");
			DivPanel.Style.Add("left", "0px");
			DivPanel.Style.Add("top", TextBoxControl.Height.ToString());

			fListBoxControl = new ZMultiTextListBox();
			DivPanel.Controls.Add(fListBoxControl);
			fListBoxControl.Rows = Rows;
		}

		protected Panel fDivPanel;

		protected virtual Panel DivPanel
		{
			get
			{
				if (fDivPanel == null)
				{
					fDivPanel = new Panel();
				}
				return fDivPanel;
			}
		}

		void RegisterPopupScript()
		{
			if (Page != null && !Page.ZClientScript.IsClientScriptIncludeRegistered("ZDropEditList_ClientScriptBlock"))
			{
				Page.ZClientScript.RegisterClientScriptInclude("ZDropEditList_ClientScriptBlock", ScriptFile.FileName);
			}
		}

		#endregion Rendering

		#region Div Controls

		ZMultiTextListBox fListBoxControl;

		protected internal ZMultiTextListBox ListBoxControl
		{
			get
			{
				EnsureChildControls();
				return fListBoxControl;
			}
		}

		#endregion

		#region IBindToList Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToList))]
		public string BindToList
		{
			get { return fBindToList; }
			set { fBindToList = value; }
		}
		string fBindToList = "";

		#endregion

		#region Properties

		#region Appearance

		#region PopupHeight

		[Browsable(true)]
		[Category("Appearance")]
		[DefaultValue(300)]
		public Unit PopupHeight
		{
			get { return fPopupHeight; }
			set
			{
				if (value.Value < 300)
				{
					fPopupHeight = Unit.Pixel(300);
				}
				else
				{
					fPopupHeight = value;
				}
				EnsureChildControls();
				ListBoxControl.Height = fPopupHeight;
			}
		}
		Unit fPopupHeight;

		#endregion PopupHeight

		#region PopupWidth

		[Browsable(true)]
		[Category("Appearance")]
		[DefaultValue(0)]
		public Unit PopupWidth
		{
			get { return (fPopupWidth.Value < Width.Value) ? Width : fPopupWidth; }
			set
			{
				if (value.Value < Width.Value)
				{
					fPopupWidth = Width;
				}
				else
				{
					fPopupWidth = value;
				}
				EnsureChildControls();
				ListBoxControl.Width = fPopupWidth;
			}
		}
		Unit fPopupWidth;

		#endregion PopupWidth

		#region Rows

		[Browsable(true)]
		[Category("Appearance")]
		[DefaultValue(10)]
		public int Rows
		{
			get
			{
				if (fRows < 1)
				{
					fRows = 10;
				}
				return fRows;
			}
			set { fRows = value; ListBoxControl.Rows = value; }
		}
		int fRows;

		#endregion Rows

		#region MaxLength

		[Browsable(true)]
		[Category("Appearance")]
		public new int MaxLength
		{
			get
			{
				EnsureChildControls();
				return TextBoxControl.MaxLength;
			}
			set
			{
				EnsureChildControls();
				TextBoxControl.MaxLength = value;
			}
		}

		#endregion MaxLength

		#endregion Appearance

		#region Control Properties

		protected override string ButtonClickHandler
		{
			get
			{
				EnsureChildControls();
				return string.Format("ZDropEditList_ShowPopup('{0}'{1})", PopupID, !RelatedControlID.IsEmpty ? ",'" + RelatedControlID + "'" : "");
			}
		}

		protected virtual ZString RelatedControlID
		{
			get
			{
				return string.Empty;
			}
		}

		protected virtual string SelectItemHandler
		{
			get
			{
				EnsureChildControls();
				return string.Format("ZDropEditList_SelectItem('{0}')", TextBoxControl.ClientID);
			}
		}

		protected virtual string KeysHandler
		{
			get
			{
				EnsureChildControls();
				return string.Format("return ZDropEditList_KeyPress('{0}')", TextBoxControl.ClientID);
			}
		}

		protected string OnBLurHandler
		{
			get
			{
				EnsureChildControls();
				return string.Format("ZDropEditList_HidePopup('{0}')", PopupID);
			}
		}

		protected string PopupID
		{
			get
			{
				EnsureChildControls();
				return DivPanel.ClientID;
			}
		}

		protected virtual Color PopupBackgroundColor
		{
			get { return Color.Empty; }
		}

		string fDisplayStyle;
		protected virtual string DisplayStyle
		{
			get
			{
				if (fDisplayStyle == null)
				{
					if (Page is ZPage && Page.BrowserType == BrowserType.Mozilla)
					{
						fDisplayStyle = "VISIBILITY:hidden";
					}
					else
					{
						fDisplayStyle = "DISPLAY:none";
					}
				}
				return fDisplayStyle;
			}
		}

		ZPage BasePage
		{
			get { return Page; }
		}

		[Browsable(true), Category("Appearance"), DefaultValue(true)]
		public bool ShowDescription
		{
			get
			{
				object obj1 = this.ViewState["ShowDescription"];
				if (obj1 != null)
				{
					return (bool)obj1;
				}
				return true;
			}
			set
			{
				this.ViewState["ShowDescription"] = value;
			}
		}

		#endregion Control Properties

		#endregion Properties

		#region IContainResources Members

		public override ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = base.Resources;
				result.Add(ScriptFile);
				return result;
			}
		}

		protected ZWebResource ScriptFile
		{
			get
			{
				if (fScriptFile == null)
				{
					fScriptFile = GetScriptFileResource();
				}

				return fScriptFile;
			}
		}
		ZWebResource fScriptFile;

		protected ZWebResource GetScriptFileResource()
		{
			return new ZWebResource(typeof(ZDropEditList), ScriptFileName, BasePage);
		}

		protected string ScriptFileName
		{
			get { return "ZDropEditScriptBlock.js"; }
		}
		#endregion IContainResources Members

		#region ISelfBindingWebControl Members

		public override bool IsBindable(object dataSource)
		{
			return base.IsBindable(dataSource) && (AllowBindWithoutListMember() || !string.IsNullOrEmpty(MetadataHelper.GetListMember(this, dataSource)));
		}

		protected virtual bool AllowBindWithoutListMember()
		{
			return false;
		}

		protected override void BindCore(object dataSource)
		{
			BindListControl(dataSource);
			base.BindCore(dataSource);
		}

		protected virtual void BindListControl(object dataSource)
		{
			var listMember = MetadataHelper.GetListMember(this, dataSource);
			ListBoxControl.DataSource = ZPropertyAccessor.Get(dataSource, listMember);

			var collection = ListBoxControl.DataSource as IBusinessObjectCollection;

			if (collection != null)
			{
				var orderBySchemaColumn = GetOrderByFilterColumn(collection.TypeOfElements);
				var orderBy = orderBySchemaColumn != null ? orderBySchemaColumn.Name : "";

				var legacyCollection = collection as BusinessObjectCollection;
				if (legacyCollection != null && !(legacyCollection is INonPersistentBusinessObjectCollection))
				{
					var filter = new ZQuery(((ILegacyBusinessObjectCollectionInternals)legacyCollection).AdditionalFilter);
					filter.OrderBy = orderBy;
					legacyCollection.Load(filter);
				}
				collection.ApplySort(new SortInfo(orderBy, ListSortDirection.Ascending));
			}

			ListBoxControl.ShowDescription = ShowDescription;
			ListBoxControl.DataBind();
		}

		SchemaColumn GetOrderByFilterColumn(Type type)
		{
			SchemaColumn result = null;
			try
			{
				string tableName = BusinessObjectFactory.GetTableNameFromType(type);
				string propertyName = CodePropertyAttribute.CodePropertyNameFromType(type);
				result = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(propertyName, tableName) as SchemaStringColumn;
			}
			catch (NoCodePropertyException)
			{
			}
			return result;
		}

		public new void UnBind()
		{
			BindTo = "";
			BindToList = "";
			TextBoxControl.Text = "";
			ListBoxControl.DataSource = null;
		}

		#endregion
	}

	#endregion
}
