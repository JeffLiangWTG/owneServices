using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Internal;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:ZOrgAddressControl runat=server></{0}:ZOrgAddressControl>")]
	public class ZOrgAddressControl : CompositeControl, ISelfBindingPostbackWebControl
	{
		#region Implementation

		Label CaptionLabel;
		Label OrganisationLabel;
		Label AddressLabel;

		ZAutoCompleteTextBoxWithButton OrganisationTextBox;
		protected ZAutoCompleteTextBoxWithButton AddressTextBox;
		ZTextLabel FormattedAddressLabel;

#endregion

#region Properties

#region CSS Classes

#region CssClass

		[Category("Appearance"), DefaultValue("ResultsTable"), Browsable(true)]
		public override string CssClass
		{
			get
			{
				return fCssClass;
			}
			set
			{
				fCssClass = value;
			}
		}
		string fCssClass = "ResultsTable";

#endregion

#region CaptionCssClass

		[Category("Appearance"), DefaultValue("SectionTitle"), Browsable(true)]
		public string CaptionCssClass
		{
			get
			{
				return fCaptionCssClass;
			}
			set
			{
				fCaptionCssClass = value;
			}
		}
		string fCaptionCssClass = "SectionTitle";

#endregion

#region LabelsCssClass

		[Category("Appearance"), DefaultValue("Label"), Browsable(true)]
		public string LabelsCssClass
		{
			get
			{
				return fLabelsCssClass;
			}
			set
			{
				fLabelsCssClass = value;
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		string fLabelsCssClass = "Label";

#endregion

#region OrganisationCaptionCellCss

		[Category("Appearance"), Browsable(true)]
		public string OrganisationCaptionCellCss
		{
			get
			{
				return fOrganisationCaptionCss;
			}
			set
			{
				fOrganisationCaptionCss = value;
			}
		}
		string fOrganisationCaptionCss = "ZOrgAddressOrganisationCaption"; // Hard-coded constant

#endregion

#region OrganisationTextBoxCellCss

		[Category("Appearance"), Browsable(true)]
		public string OrganisationTextBoxCellCss
		{
			get
			{
				return fOrganisationTextBoxCellCss;
			}
			set
			{
				fOrganisationTextBoxCellCss = value;
			}
		}
		string fOrganisationTextBoxCellCss = "ZOrgAddressOrganisation"; // Hard-coded constant

#endregion

#region AddressCaptionCellCss

		[Category("Appearance"), Browsable(true)]
		public string AddressCaptionCellCss
		{
			get
			{
				return fAddressCaptionCellCss;
			}
			set
			{
				fAddressCaptionCellCss = value;
			}
		}
		string fAddressCaptionCellCss = "ZOrgAddressAddressCaption"; // Hard-coded constant

#endregion

#region AddressTextBoxCellCss

		[Category("Appearance"), Browsable(true)]
		public string AddressTextBoxCellCss
		{
			get
			{
				return fAddressTextBoxCellCss;
			}
			set
			{
				fAddressTextBoxCellCss = value;
			}
		}
		string fAddressTextBoxCellCss = "ZOrgAddressAddressCaption"; // Hard-coded constant

#endregion

#region OrganisationTextBoxCss

		[Category("Appearance"), Browsable(true)]
		public string OrganisationTextBoxCss
		{
			get
			{
				return fOrganisationTextBoxCss;
			}
			set
			{
				fOrganisationTextBoxCss = value;
			}
		}
		string fOrganisationTextBoxCss = "ZOrgAddressOrganisationText"; // Hard-coded constant

#endregion

#region AddressTextBoxCss

		[Category("Appearance"), Browsable(true)]
		public string AddressTextBoxCss
		{
			get
			{
				return fAddressTextBoxCss;
			}
			set
			{
				fAddressTextBoxCss = value;
			}
		}
		string fAddressTextBoxCss = "ZOrgAddressAddressText"; // Hard-coded constant

#endregion

#endregion

#region Captions

#region Control Header Caption

		[Category("Appearance"), Browsable(true)]
		public string Caption
		{
			get
			{
				return fCaption;
			}
			set
			{
				fCaption = value;
			}
		}
		string fCaption = string.Empty;

#endregion

#region OrganisationCaption

		[Category("Appearance"), Browsable(true)]
		public string OrganisationCaption
		{
			get
			{
				return fOrganisationCaption;
			}
			set
			{
				fOrganisationCaption = value;
			}
		}
		string fOrganisationCaption = Res.GetString("60A6F0FF-7950-4f7f-93AB-5E10E7C8F4DB", "Company");

#endregion

#region AddressCapation

		[Category("Appearance"), Browsable(true)]
		public string AddressCaption
		{
			get
			{
				return fAddressCaption;
			}
			set
			{
				fAddressCaption = value;
			}
		}
		string fAddressCaption = Res.GetString("B2BE2EA6-7175-447d-9E9F-10923875AAA5", "Address");

#endregion

#endregion

		[Category("Behaviour"), Browsable(true), DefaultValue(false)]
		public bool UseVerticalLayout
		{
			get { return fUseVerticalLayout; }
			set { fUseVerticalLayout = value; }
		}
		bool fUseVerticalLayout;

		[Category("Behaviour"), Browsable(true), DefaultValue(false)]
		public bool ReadOnly
		{
			get
			{
				bool infoReadonly = Info != null && Info.ReadOnly;
				return fReadOnly || infoReadonly;
			}
			set { fReadOnly = value; }
		}
		bool fReadOnly;

		BusinessObjectFactory Factory
		{
			get { return ((ZPage)Page).Factory; }
		}

		[Category("Behaviour"), Browsable(true)]
		[Editor(typeof(WebModuleIDEditor), typeof(UITypeEditor))]
		public WebModuleID ModuleID
		{
			get { return fModuleID; }
			set { fModuleID = value; }
		}
		WebModuleID fModuleID = WebModuleIDs.OrganisationTracking;

		[Category("Behavior"), Browsable(true)]
		[Editor(typeof(WebModuleIDEditor), typeof(UITypeEditor))]
		public WebModuleID AddressModuleID
		{
			get { return fAddressModuleID; }
			set { fAddressModuleID = value; }
		}
		WebModuleID fAddressModuleID = WebModuleIDs.OrgAddress;

		#endregion

		#region Control Building

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			CaptionLabel = new Label();
			OrganisationLabel = new Label();
			AddressLabel = new Label();

			OrganisationLabel.CssClass = LabelsCssClass;
			AddressLabel.CssClass = LabelsCssClass;

			OrganisationLabel = new Label();
			OrganisationTextBox = new ZAutoCompleteTextBoxWithButton();
			OrganisationTextBox.ModuleID = ModuleID;
			OrganisationTextBox.BindTo = "OrganisationPK";
			OrganisationTextBox.CssClass = OrganisationTextBoxCss;
			OrganisationTextBox.AutoPostBack = true;
			OrganisationTextBox.ID = "Org";

			AddressTextBox = new ZAutoCompleteTextBoxWithButton();
			AddressTextBox.Helper = GetOrgAddressHelper();
			AddressTextBox.ModuleID = AddressModuleID;
			AddressTextBox.BindTo = "AddressPK";
			AddressTextBox.CssClass = AddressTextBoxCss;
			AddressTextBox.AutoPostBack = true;
			AddressTextBox.ID = "Addr1";

			FormattedAddressLabel = new ZTextLabel();
			FormattedAddressLabel.BindTo = "FormattedAddressSummary";

			SetupEvents();
		}

		AutoCompleteHelper GetOrgAddressHelper()
		{
			switch (AddressModuleID.ID)
			{
				case WebModuleId.OrgAddressReceivablesTracking:
					return new OrgAddressReceivablesAutoCompleteHelper(Factory);
				default:
					return new OrgAddressAutoCompleteHelper(Factory);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		Panel PutInNoWrapPanel(WebControl control)
		{
			Panel result = new Panel();
			result.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");
			result.Controls.Add(control);
			return result;
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			UpdatePanel controlUpdatePanel = new UpdatePanel();
			controlUpdatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;

			Table controlTable = new Table();
			controlTable.CssClass = CssClass;

#region Caption

			CaptionLabel.CssClass = CaptionCssClass;
			CaptionLabel.Text = Caption;

			TableCell captionCell = new TableCell();
			captionCell.Controls.Add(PutInNoWrapPanel(CaptionLabel));

#endregion

#region Organisation

			OrganisationLabel.Text = OrganisationCaption + ":";

			TableCell orgCaptionCell = new TableCell();
			orgCaptionCell.CssClass = OrganisationCaptionCellCss;
			orgCaptionCell.Controls.Add(PutInNoWrapPanel(OrganisationLabel));

			TableCell orgCell = new TableCell();
			orgCell.CssClass = OrganisationTextBoxCellCss;
			orgCell.Controls.Add(OrganisationTextBox);

#endregion

#region Address

			AddressLabel.Text = AddressCaption + ":";

			TableCell addrCaptionCell = new TableCell();
			addrCaptionCell.CssClass = AddressCaptionCellCss;
			addrCaptionCell.Controls.Add(PutInNoWrapPanel(AddressLabel));

			TableCell addrCell = new TableCell();
			addrCell.CssClass = AddressTextBoxCellCss;
			addrCell.Controls.Add(AddressTextBox);

#endregion

			captionCell.ColumnSpan = 4;

			TableRow captionRow = new TableRow();
			captionRow.Cells.Add(captionCell);
			controlTable.Rows.Add(captionRow);
			captionRow.Visible = !string.IsNullOrEmpty(CaptionLabel.Text);

			if (ReadOnly)
			{
				TableRow summaryRow = new TableRow();
				TableCell formattedAddressCell = new TableCell();
				formattedAddressCell.ColumnSpan = 4;
				formattedAddressCell.Controls.Add(PutInNoWrapPanel(FormattedAddressLabel));
				summaryRow.Cells.Add(formattedAddressCell);
				controlTable.Rows.Add(summaryRow);
			}
			else
			{
				orgCell.ColumnSpan = 3;
				addrCell.ColumnSpan = 3;

				TableRow orgRow = new TableRow();
				orgRow.Cells.Add(orgCaptionCell);
				orgRow.Cells.Add(orgCell);

				if (UseVerticalLayout)
				{
					TableRow addrRow = new TableRow();
					addrRow.Cells.Add(addrCaptionCell);
					addrRow.Cells.Add(addrCell);

					controlTable.Rows.Add(orgRow);
					controlTable.Rows.Add(addrRow);
				}
				else
				{
					orgRow.Cells.Add(addrCaptionCell);
					orgRow.Cells.Add(addrCell);

					controlTable.Rows.Add(orgRow);
				}
			}

			controlUpdatePanel.ContentTemplateContainer.Controls.Add(controlTable);
			Controls.Add(controlUpdatePanel);
		}

#endregion

#region ISelfBindingPostbackWebControl Members

		public bool HasChanges
		{
			get;
			set;
		}

#endregion

#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			return dataSource != null;
		}

		object BusinessEntity;
		ZPropertyInfo Info;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant to get info because arch team too lazy to implement get info in zproperty accessor")]
		public void Bind(object dataSource)
		{
			BusinessEntity = dataSource;
			ZGuid guid = (ZGuid)ZPropertyAccessor.Get(dataSource, BindTo);
			Info = (ZPropertyInfo)ZPropertyAccessor.Get(dataSource, string.Format("{0}Info", BindTo));
			WebOrgAddress.AddressPK = guid;
			DataBind();
		}

		WebOrgAddress WebOrgAddress
		{
			get
			{
				if (fWebOrgAddress == null)
				{
					fWebOrgAddress = new WebOrgAddress(Factory, GetOrgAddressType());
				}
				return fWebOrgAddress;
			}
		}
		WebOrgAddress fWebOrgAddress;

		OrgAddressType GetOrgAddressType()
		{
			switch (AddressModuleID.ID)
			{
				case WebModuleId.OrgAddressReceivablesTracking:
					return OrgAddressType.Receivables;
				default:
					return null;
			}
		}

		public void UnBind()
		{
			BusinessEntity = null;
		}

#endregion

#region IBindTo Members

		public string BindTo
		{
			get;
			set;
		}

#endregion

		protected virtual void SetupEvents()
		{
			WebOrgAddress.AddressChanged += new EventHandler(webOrgAddress_AddressChanged);
		}

		void webOrgAddress_AddressChanged(object sender, EventArgs e)
		{
			if (BusinessEntity != null && (ZGuid)ZPropertyAccessor.Get(BusinessEntity, BindTo) != WebOrgAddress.AddressPK)
			{
				ZPropertyAccessor.Set(BusinessEntity, BindTo, WebOrgAddress.AddressPK);
				AddressTextBox.HasChanges = false;
			}
		}

		public override void DataBind()
		{
			base.DataBind();
			BindChildControls();
		}

		void BindChildControls()
		{
			if (AddressTextBox.HasChanges)
			{
				AddressTextBox.Bind(WebOrgAddress);
				OrganisationTextBox.Bind(WebOrgAddress);
			}
			else
			{
				OrganisationTextBox.Bind(WebOrgAddress);
				AddressTextBox.Bind(WebOrgAddress);
			}

			((OrgAddressAutoCompleteHelper)AddressTextBox.Helper).ParentPK = ParentOrgPK;
			AddressTextBox.PopupEnabled = !ParentOrgPK.IsEmpty;

			FormattedAddressLabel.Bind(WebOrgAddress);
		}

		protected ZGuid ParentOrgPK
		{
			get
			{
				return WebOrgAddress.OrganisationPK;
			}
		}

#region IPostBackDataHandler Members

		protected void OnPostDataChanged(object sender, EventArgs e)
		{
			RaisePostDataChangedEvent();
		}

		public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			if (!isPostDataLoaded)
			{
				HasChanges = AddressTextBox.LoadPostData(postDataKey, postCollection) ||
					OrganisationTextBox.LoadPostData(postDataKey, postCollection);

				isPostDataLoaded = true;
			}

			return HasChanges;
		}
		bool isPostDataLoaded;

		public void RaisePostDataChangedEvent()
		{
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

#endregion
	}
}
