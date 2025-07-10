using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.FilterStrips;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	public class ZFilterStripRowFooter : HtmlTableRow
	{
		#region Constants

		public const string HelpImageName = "help.png";
		public const string HelpDocumentURL = "https://myaccount-portal.cargowise.com/my-account/public/Documents/UpdateNotes/ediEnterpriseupdatenote20080522.pdf"; // Document URL

		#endregion

		#region Construction

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public ZFilterStripRowFooter()
		{
			EnableViewState = false;
			Cells.Add(GetCell1());
			Cells.Add(GetCell3());
			Cells.Add(GetCell4());
			VAlign = "top";

			Load += new EventHandler(ZFilterStripRowFooter_Load);
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			bool wasUserAuthenticated = ((ZPage)Page).SiteUser.IsLoggedIn;
			HiddenLayoutNameInput.Visible = wasUserAuthenticated;
			HelpHyperlink.Visible = wasUserAuthenticated;
			ManageLayoutsButton.Visible = wasUserAuthenticated;
			SaveLayoutButton.Visible = wasUserAuthenticated;
			FilterLayoutsDropList.Visible = wasUserAuthenticated;
			ResetLayoutButton.Visible = wasUserAuthenticated;
			SaveLayoutButton.Enabled = FilterStripBizO.ActiveModuleFilters.Count > 0;
		}

		void ZFilterStripRowFooter_Load(object sender, EventArgs e)
		{
			AssignClientEvents();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		void AssignClientEvents()
		{
			string setFilterIsNotUnsavedFlag =
				"document.getElementById('" + HiddenUserLoadedFilterLayoutInput.ClientID + "').value = '" + FilterIsNotUnsavedFlagText + "';";

			ResetLayoutButton.OnClientClick =
				setFilterIsNotUnsavedFlag +
				"document.getElementById('" + FilterLayoutsDropList.ClientID + "').options[0].selected = true;";

			FilterLayoutsDropList.OnClientChange = setFilterIsNotUnsavedFlag;
		}

		#endregion

		#region Cell1 - "Manage Layouts", "Save Layout", "Reset Layout", "Find"

		HtmlTableCell GetCell1()
		{
			HtmlTableCell result = new HtmlTableCell();
			result.ColSpan = 2;
			result.Controls.Add(HiddenLayoutNameInput);
			result.Controls.Add(HelpHyperlink);
			result.Controls.Add(ManageLayoutsButton);
			result.Controls.Add(SaveLayoutButton);
			result.Controls.Add(ResetLayoutButton);
			result.Controls.Add(FindButton);
			result.Width = (ZFilterStripConstants.Cells.FilterDescriptionWidth + ZFilterStripConstants.Cells.FilterClauseWidth).ToString();

			return result;
		}

		protected HtmlInputHidden HiddenLayoutNameInput
		{
			get
			{
				if (fHiddenLayoutNameInput == null)
				{
					fHiddenLayoutNameInput = new HtmlInputHidden();
					fHiddenLayoutNameInput.ID = "HiddenLayoutNameBox_FooterRow";
				}
				return fHiddenLayoutNameInput;
			}
		}
		HtmlInputHidden fHiddenLayoutNameInput;

		protected ZHyperlink HelpHyperlink
		{
			get
			{
				if (fHelpHyperlink == null)
				{
					fHelpHyperlink = new ZHyperlink();
					fHelpHyperlink.ID = "HelpHyperlink";
					fHelpHyperlink.ImageUrl = HelpImageName;
					fHelpHyperlink.NavigateUrl = HelpDocumentURL;
					fHelpHyperlink.ToolTip = Res.GetString("3fb0a312-65ee-43fb-9a8d-fa01166c939e", "Click to get instructions how to operate with filters and layouts");
				}
				return fHelpHyperlink;
			}
		}
		ZHyperlink fHelpHyperlink;

		protected ManageLayoutsPopup ManageLayoutsButton
		{
			get
			{
				if (fManageLayoutsButton == null)
				{
					fManageLayoutsButton = new ManageLayoutsPopup();
					fManageLayoutsButton.RenderContentsOnly = true;
				}
				return fManageLayoutsButton;
			}
		}
		ManageLayoutsPopup fManageLayoutsButton;

		protected SaveLayoutPopup SaveLayoutButton
		{
			get
			{
				if (fSaveLayoutButton == null)
				{
					fSaveLayoutButton = new SaveLayoutPopup();
					fSaveLayoutButton.RenderContentsOnly = true;
					fSaveLayoutButton.TextChanged += new EventHandler(fSaveLayoutButton_TextChanged);
				}
				return fSaveLayoutButton;
			}
		}

#if DEBUG
		public
#endif
 void fSaveLayoutButton_TextChanged(object sender, EventArgs e)
		{
			var layoutName = fSaveLayoutButton.Text;
			HiddenLayoutNameInput.Value = layoutName;
			var isPublished = SaveLayoutButton.IsPublishedTextBoxControl.Text == "Y";
			var filters = LayoutsHelper.Layouts.ToArray().Where(item => item.Description == layoutName).Select(item => (StmModuleFilter)item.PK);

			OnSaveLayout(filters.FirstOrDefault(item => item.S9_IsPublished == isPublished));
			fSaveLayoutButton.Text = string.Empty;
		}

		SaveLayoutPopup fSaveLayoutButton;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected ZButton ResetLayoutButton
		{
			get
			{
				if (fResetLayoutButton == null)
				{
					fResetLayoutButton = new ZButton();
					fResetLayoutButton.Click += new EventHandler(resetLayoutButton_Click);
					fResetLayoutButton.Height = ZFilterStripConstants.Controls.ButtonHeight;
					fResetLayoutButton.Width = 170;
					fResetLayoutButton.ID = "ResetLayoutButton";
					fResetLayoutButton.Text = Res.GetString("59145536-eac2-4c2a-86e2-f043366d480b", "Reset Layout");
					fResetLayoutButton.Attributes[nameof(HtmlTextWriterAttribute.Class)] = "Button";
				}
				return fResetLayoutButton;
			}
		}
		ZButton fResetLayoutButton;

		#endregion

		#region Cell3 - Find Droplist

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		HtmlTableCell GetCell3()
		{
			HtmlTableCell result = new HtmlTableCell();
			result.Align = "right";
			result.Width = ZFilterStripConstants.Cells.FilterControlsWidth.ToString();
			result.Controls.Add(HiddenUserLoadedFilterLayoutInput);
			result.Controls.Add(FilterLayoutsDropList);

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public ZButton FindButton
		{
			get
			{
				if (fFindButton == null)
				{
					fFindButton = new ZButton();
					fFindButton.Click += new EventHandler(findButton_Click);
					fFindButton.Font.Bold = true;
					fFindButton.Height = ZFilterStripConstants.Controls.ButtonHeight;
					fFindButton.ID = "FooterRow_FindButton";
					fFindButton.Style["float"] = "right";
					fFindButton.Text = " " + Res.GetString("72050d0c-44ef-4e1a-b821-9b47199d1883", "Find") + " ";
					fFindButton.Attributes[nameof(HtmlTextWriterAttribute.Class)] = "Button";
					fFindButton.OnClientClick = @"if (typeof(Sys) != ""undefined"") return !Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack(); else return true;";
				}
				return fFindButton;
			}
		}

		protected HtmlInputHidden HiddenUserLoadedFilterLayoutInput
		{
			get
			{
				if (fHiddenUserLoadedFilterLayoutInput == null)
				{
					fHiddenUserLoadedFilterLayoutInput = new HtmlInputHidden();
					fHiddenUserLoadedFilterLayoutInput.ID = "FooterRow_HiddenUserLoadedFilterLayoutInput";
				}
				return fHiddenUserLoadedFilterLayoutInput;
			}
		}

		public ZFilterLayoutDropDownList FilterLayoutsDropList
		{
			get
			{
				if (fFilterLayoutsDropList == null)
				{
					fFilterLayoutsDropList = new ZFilterLayoutDropDownList();
					fFilterLayoutsDropList.AutoPostBack = true;
					fFilterLayoutsDropList.BindTo = "LayoutsHelper." + FilterStripLayoutsHelperForWeb.Schema.CurrentLayoutName;
					fFilterLayoutsDropList.BindToList = "LayoutsHelper." + FilterStripLayoutsHelperForWeb.Schema.Layouts;
					fFilterLayoutsDropList.ID = "FooterRow_FilterLayoutsDropList";
					fFilterLayoutsDropList.DisplayStyle = OComboBoxDropDownStyle.CodeOnly;
					fFilterLayoutsDropList.Style["width"] = "100%";
				}
				return fFilterLayoutsDropList;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected ZButton ClearButton
		{
			get
			{
				if (fClearButton == null)
				{
					fClearButton = new ZButton();
					fClearButton.Click += new EventHandler(clearButton_Click);
					fClearButton.Height = ZFilterStripConstants.Controls.ButtonHeight;
					fClearButton.ID = "FooterRow_ClearButton";
					fClearButton.Text = Res.GetString("1afd8152-fc7a-48b5-ae36-d4e17c7c5ee6", "Clear");
					fClearButton.Attributes[nameof(HtmlTextWriterAttribute.Class)] = "Button";
				}
				return fClearButton;
			}
		}

		ZButton fFindButton;
		HtmlInputHidden fHiddenUserLoadedFilterLayoutInput;
		ZFilterLayoutDropDownList fFilterLayoutsDropList;
		ZButton fClearButton;

		#endregion

		#region Cell4 - "Clear", "[ + ]"

		HtmlTableCell GetCell4()
		{
			HtmlTableCell result = new HtmlTableCell();
			result.Width = ZFilterStripConstants.Cells.FilterAddButtonWidth.ToString();
			result.Style["padding-left"] = "10px";
			result.Controls.Add(ClearButton);
			result.Controls.Add(AddButton);

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected ZButton AddButton
		{
			get
			{
				if (fAddButton == null)
				{
					fAddButton = new ZButton();
					fAddButton.Click += new EventHandler(addButton_Click);
					fAddButton.Font.Bold = true;
					fAddButton.Height = ZFilterStripConstants.Controls.ButtonHeight;
					fAddButton.ID = "FooterRow_AddButton";
					fAddButton.Text = "+";
					fAddButton.Width = ZFilterStripConstants.Controls.AddButtonWidth;
					fAddButton.Attributes[nameof(HtmlTextWriterAttribute.Class)] = "Button";
				}
				return fAddButton;
			}
		}

		ZButton fAddButton;

		#endregion

		#region Bind()

		public void Bind(FilterStripBusinessObject dataSource)
		{
			FilterStripBizO = dataSource;

			FilterLayoutsDropList.Bind(dataSource);
			LayoutsHelper.CurrentLayoutChanged += CurrentLayoutNameInfo_ValueChanged;
		}

		FilterStripLayoutsHelperForWeb LayoutsHelper
		{
			get { return (FilterStripLayoutsHelperForWeb)FilterStripBizO.LayoutsHelper; }
		}

		FilterStripBusinessObject FilterStripBizO;

		#endregion

		#region Dispose

		public override void Dispose()
		{
			base.Dispose();
			if (FilterStripBizO != null && LayoutsHelper != null)
			{
				LayoutsHelper.CurrentLayoutChanged -= CurrentLayoutNameInfo_ValueChanged;
			}
		}

		#endregion

		#region HasUserLoadedLayout

		public bool HasUserLoadedLayout
		{
			get { return HiddenUserLoadedFilterLayoutInput.Value == FilterIsNotUnsavedFlagText; }
		}

		public void ResetHasUserLoadedLayout()
		{
			HiddenUserLoadedFilterLayoutInput.Value = "";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected const string FilterIsNotUnsavedFlagText = "moo";

		#endregion

		#region Save Layout Button Click

		void OnSaveLayout(StmModuleFilter layout)
		{
			if (SaveLayout != null)
			{
				SaveLayout(this, new LayoutEventArgs(layout));
			}
		}

		public event EventHandler<LayoutEventArgs> SaveLayout;

		#endregion

		#region Reset Layout Button Click

		void resetLayoutButton_Click(object sender, EventArgs e)
		{
			OnResetLayout();
		}

		void OnResetLayout()
		{
			if (ResetLayout != null)
			{
				ResetLayout(this, EventArgs.Empty);
			}
		}

		public event EventHandler ResetLayout;

		#endregion

		#region Find Button Click

		void findButton_Click(object sender, EventArgs e)
		{
			OnFindButtonClick();
		}

		void OnFindButtonClick()
		{
			if (FindButtonClick != null)
			{
				FindButtonClick(this, EventArgs.Empty);
			}
		}

		public event EventHandler FindButtonClick;

		#endregion

		#region Selecting a Layout from the DropList

		void CurrentLayoutNameInfo_ValueChanged(object sender, EventArgs e)
		{
			AssignClientEvents(); // because the item text has now changed
			OnLayoutChanged();
		}

		void OnLayoutChanged()
		{
			if (LayoutChanged != null)
			{
				LayoutChanged(this, new LayoutEventArgs(LayoutsHelper.CurrentLayout));
			}
		}

		public event EventHandler<LayoutEventArgs> LayoutChanged;

		#endregion

		#region Clear Button Click

		void clearButton_Click(object sender, EventArgs e)
		{
			OnClearFilters();
		}

		void OnClearFilters()
		{
			if (ClearFilters != null)
			{
				ClearFilters(this, EventArgs.Empty);
			}
		}

		public event EventHandler ClearFilters;

		#endregion

		#region [ + ] Button Click

		void addButton_Click(object sender, EventArgs e)
		{
			OnAddFilterStripButtonClick();
		}

		void OnAddFilterStripButtonClick()
		{
			if (AddFilterStripButtonClick != null)
			{
				AddFilterStripButtonClick(this, EventArgs.Empty);
			}
		}

		public event EventHandler AddFilterStripButtonClick;

		#endregion
	}
}
