using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.WebControls.ZGridInternals;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	[DefaultProperty("Text"),
	ToolboxData("<{0}:ZGrid runat=server></{0}:ZGrid>")]
	public class ZGrid : ZDataGrid, IContainResources
	{
		public static class Constants
		{
			public const string ExcelButtonID = "_ExcelButton"; // Constant for id
			public const string CustomizeColumnsButtonID = "_CustomizeButton"; // Constant for id
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant for id")]
			public const string CustomizeControlID = "Customize";
			public const string DownloadEDocsButtonID = "_DownloadEDocsButton"; // Constant for id
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant for id")]
			public const string ControllerID = "Controller";
		}

		WebControl exportToExcelButton;
		WebControl customizeColumnsButton;
		WebControl downloadEDocsButton;
		ZOnDemandLoadControl gridController;
		ZTextLabel collapsedMessageLabel;

		#region Properties

		#region Collapsed Message Label

		[DefaultValue(false)]
		public bool ShowCollapsedMessage { get; set; }

		public ZTextLabel CollapsedMessageLabel
		{
			get
			{
				if (collapsedMessageLabel == null)
				{
					collapsedMessageLabel = new ZTextLabel();
					collapsedMessageLabel.CssClass = "CollapsedMessage";
				}
				return collapsedMessageLabel;
			}
		}

		#endregion

		#region ExportToExcel

		public bool ShowExportToExcelButton
		{
			get { return showExportToExcelButton; }
			set
			{
				if (!showExportToExcelButton && value && !GridController.MenuItems.Controls.Contains(ExportToExcelButton))
				{
					GridController.MenuItems.Controls.Add(ExportToExcelButton);
				}
				showExportToExcelButton = value;
			}
		}
		bool showExportToExcelButton = true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "css class")]
		public WebControl ExportToExcelButton
		{
			get
			{
				if (exportToExcelButton == null)
				{
					if (HideButtonsToMenu)
					{
						exportToExcelButton = new LinkButton();
						exportToExcelButton.Attributes.Add("class", "OnDemandMenuButton");
						((LinkButton)exportToExcelButton).Text = Res.GetString("899a1d3e-92bb-4d78-9ba7-028f5d175655", "Export to Excel");
						((LinkButton)exportToExcelButton).Click += new EventHandler(ExportToExcelButton_Click);
					}
					else
					{
						exportToExcelButton = new Button();
						((Button)exportToExcelButton).Text = Res.GetString("899a1d3e-92bb-4d78-9ba7-028f5d175655", "Export to Excel");
						((Button)exportToExcelButton).Click += new EventHandler(ExportToExcelButton_Click);
					}
					exportToExcelButton.ID = this.ID + Constants.ExcelButtonID;
				}
				return exportToExcelButton;
			}
		}

		public void SetButtonText(WebControl button, string text)
		{
			if (button is Button)
			{
				((Button)button).Text = text;
			}
			if (button is LinkButton)
			{
				((LinkButton)button).Text = text;
			}
		}

		void ExportToExcelButton_Click(object sender, EventArgs e)
		{
			isMarkedForExcelExportAfterBound = true;
			if (Page != null)
			{
				((ZPage)Page).SuspendValidationErrorMessageForCurrentLoad();
			}
		}
		bool isMarkedForExcelExportAfterBound;

		#endregion

		#region CustomizeColumns

		public bool ShowCustomizeColumnsButton
		{
			get { return showCustomizeColumnsButton; }
			set
			{
				if (!showCustomizeColumnsButton && value && !GridController.MenuItems.Controls.Contains(CustomizeColumnsButton))
				{
					GridController.MenuItems.Controls.Add(CustomizeColumnsButton);
				}
				showCustomizeColumnsButton = value;
			}
		}
		bool showCustomizeColumnsButton = true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html class")]
		public WebControl CustomizeColumnsButton
		{
			get
			{
				if (customizeColumnsButton == null)
				{
					if (HideButtonsToMenu)
					{
						customizeColumnsButton = new LinkButton();
						customizeColumnsButton.Attributes.Add("class", "OnDemandMenuButton");
						((LinkButton)customizeColumnsButton).Text = Res.GetString("e67e3370-737f-4f6d-810e-5ae6a9a1c1ed", "Customize Columns");
						((LinkButton)customizeColumnsButton).Click += new EventHandler(CustomizeColumnsButton_Click);
					}
					else
					{
						customizeColumnsButton = new Button();
						((Button)customizeColumnsButton).Text = Res.GetString("e67e3370-737f-4f6d-810e-5ae6a9a1c1ed", "Customize Columns");
						((Button)customizeColumnsButton).Click += new EventHandler(CustomizeColumnsButton_Click);
					}
					customizeColumnsButton.ID = this.ID + Constants.CustomizeColumnsButtonID;
				}
				return customizeColumnsButton;
			}
		}

		void CustomizeColumnsButton_Click(object sender, EventArgs e)
		{
			isMarkedForColumnCustomization = true;
			if (Page != null)
			{
				((ZPage)Page).SuspendValidationErrorMessageForCurrentLoad();
			}
		}
		bool isMarkedForColumnCustomization;

		#endregion

		#region Download eDocs

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "http header should not be translated")]
		public void DownloadEDocs()
		{
			if (ZGridModuleEDocsHelper.TryGetZippedEDocs(this, HttpContext.Current.Response.OutputStream))
			{
				HttpContext.Current.Response.ContentType = DataContentTypes.Zip;
				HttpContext.Current.Response.AppendHeader("Content-Disposition", string.Format("attachment; filename=\"eDocs_{0}.zip\"", ZDateTime.Now.ToString("yyyyMMdd-HHmmss-fff")));
				HttpContext.Current.Response.End();
			}
			else
			{
				if (Page is ZPage page)
				{
					string message;
					if (Collection.Count < 1)
					{
						message = Res.GetString("66cfc2e3-902c-4b96-9e14-eb3b2d319fd6", "Your search filters returned no results. Please modify your search before trying to download eDocs.");
					}
					else if (SelectedCellsValues.Count < 1)
					{
						message = Res.GetString("f2de715a-5f17-48b1-8cc4-94a6c0fc1ef4", "Please select the document types you want to download.");
					}
					else
					{
						message = Res.GetString("05ee89d4-d8ef-4636-995e-477fb7b672e9", "There are no documents of the selected types to download.");
					}
					page.ShowClientSideAlert(message);
				}
			}
		}

		public bool ShowDownloadEDocsButton
		{
			get { return showDownloadEDocsButton; }
			set
			{
				if (!showDownloadEDocsButton && value && !GridController.MenuItems.Controls.Contains(DownloadEDocsButton))
				{
					GridController.MenuItems.Controls.Add(DownloadEDocsButton);
				}
				showDownloadEDocsButton = value;
			}
		}
		bool showDownloadEDocsButton;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html class")]
		public WebControl DownloadEDocsButton
		{
			get
			{
				if (downloadEDocsButton == null)
				{
					if (HideButtonsToMenu)
					{
						downloadEDocsButton = new LinkButton();
						downloadEDocsButton.Attributes.Add("class", "OnDemandMenuButton");
						((LinkButton)downloadEDocsButton).Text = Res.GetString("d148668a-6193-4146-bd08-4987b3609b70", "Download eDocs");
						((LinkButton)downloadEDocsButton).Click += new EventHandler(DownloadEDocsButton_Click);
					}
					else
					{
						downloadEDocsButton = new Button();
						((Button)downloadEDocsButton).Text = Res.GetString("d148668a-6193-4146-bd08-4987b3609b70", "Download eDocs");
						((Button)downloadEDocsButton).Click += new EventHandler(DownloadEDocsButton_Click);
					}
					downloadEDocsButton.ID = this.ID + Constants.DownloadEDocsButtonID;
				}
				return downloadEDocsButton;
			}
		}

		void DownloadEDocsButton_Click(object sender, EventArgs e)
		{
			isMarkedForDownloadEDocs = true;
			if (Page != null)
			{
				((ZPage)Page).SuspendValidationErrorMessageForCurrentLoad();
			}
		}
		bool isMarkedForDownloadEDocs;

		#endregion

		#region GridController

		public string ControllerUniqueID
		{
			get { return GridController.UniqueID; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html class constant")]
		ZOnDemandLoadControl GridController
		{
			get
			{
				if (gridController == null)
				{
					gridController = new ZOnDemandLoadControl() { ParentControlID = ID };
					gridController.ID = ID + Constants.ControllerID;
					gridController.Attributes.Add("class", "DataGridController");
				}
				return gridController;
			}
		}

		#endregion

		public bool OldStyle
		{
			set
			{
				DisableCollapsing = value;
				ShowMenu = !value;
			}
		}

		public bool ShowMenu
		{
			get { return GridController.MenuItems.Visible; }
			set { GridController.MenuItems.Visible = value; }
		}

		public HtmlGenericControl Header
		{
			get { return GridController.Header; }
		}

		public HtmlGenericControl Menu
		{
			get { return GridController.MenuItems; }
		}

		public HtmlGenericControl Container
		{
			get { return GridController.Container; }
		}

		public bool HideButtonsToMenu
		{
			get { return GridController.HideButtonsToMenu; }
			set { GridController.HideButtonsToMenu = value; }
		}

		public override string Caption
		{
			get
			{
				return string.Empty;
			}
			set
			{
				GridController.ControlButtonText = value;
			}
		}

		public string Label
		{
			get { return GridController.ControlButtonText; }
			set
			{
				GridController.ControlButtonText = value;
			}
		}

		public bool DisableCollapsing
		{
			get { return GridController.IsCollapsingDisabled; }
			set { GridController.IsCollapsingDisabled = value; }
		}

		public bool ShouldShowControl
		{
			get { return GridController.ShouldShowControl; }
			set { GridController.ShouldShowControl = value; }
		}

		public bool Collapsed
		{
			get { return GridController.IsCollapsed; }
			set { GridController.IsCollapsed = value; }
		}

		public UpdatePanelUpdateMode UpdateMode
		{
			get { return GridController.UpdateMode; }
			set { GridController.UpdateMode = value; }
		}

		public UpdatePanelTriggerCollection Triggers
		{
			get { return GridController.Triggers; }
		}

		public bool AllowCollapseOfAlwaysVisibleHolder
		{
			get { return GridController.AllowCollapseOfAlwaysVisibleHolder; }
			set { GridController.AllowCollapseOfAlwaysVisibleHolder = value; }
		}

		#endregion

		#region Overrides

		public void Bind(object dataSource, bool bindController)
		{
			if (bindController)
			{
				GridController.Bind(dataSource);
			}
			else
			{
				Bind(dataSource);
			}
		}

		public override bool Visible
		{
			get
			{
				return GridController.Visible;
			}
			set
			{
				GridController.Visible = value;
			}
		}

		public void RegisterPostBackControl(Control control)
		{
			if (!PostBackControls.Contains(control))
			{
				PostBackControls.Add(control);
			}
		}

		protected List<Control> PostBackControls
		{
			get
			{
				if (postBackControls == null)
				{
					postBackControls = new List<Control>();
				}
				return postBackControls;
			}
		}

		List<Control> postBackControls;

		protected override void OnPreRender(EventArgs e)
		{
			var ajaxPage = Page as ZAjaxPage;
			if (ajaxPage != null)
			{
				if (GridController.UseUpdatePanel)
				{
					if (GridController.AllowCollapseOfAlwaysVisibleHolder)
					{
						ajaxPage.AJAX.ScriptManager.RegisterPostBackControl(ExportToExcelButton);
						ajaxPage.AJAX.ScriptManager.RegisterPostBackControl(DownloadEDocsButton);
					}
					foreach (var control in PostBackControls)
					{
						ajaxPage.AJAX.ScriptManager.RegisterPostBackControl(control);
					}
				}
			}
			HandleCustomClicks();
			base.OnPreRender(e);
			var page = Page as ZPage;
			if (page != null)
			{
				if (!page.ZClientScript.IsClientScriptIncludeRegistered(ZGridCustomizerScriptKey))
				{
					page.ZClientScript.RegisterClientScriptInclude(ZGridCustomizerScriptKey, ZGridCustomizerScript.FileName);
				}
			}
			CustomizeColumnsButton.Visible = !GridManager.ColumnProvider.IsEmpty;
			CollapsedMessageLabel.Visible = ShowCollapsedMessage && Collapsed;
		}

		protected override void OnInit(EventArgs e)
		{
			this.Page.InitComplete += new EventHandler(Page_InitComplete);

			if (ShowExportToExcelButton)
			{
				GridController.MenuItems.Controls.Add(ExportToExcelButton);
			}
			if (ShowCustomizeColumnsButton)
			{
				GridController.MenuItems.Controls.Add(CustomizeColumnsButton);
			}
			if (ShowDownloadEDocsButton)
			{
				GridController.MenuItems.Controls.Add(DownloadEDocsButton);
			}

			base.OnInit(e);
		}

#if DEBUG
		public void InitGridControllerForTesting()
		{
			GridController.OnInitForTesting();
			InitGridController();
		}
#endif

		void InitGridController()
		{
			var parent = Parent;
			var index = parent.Controls.IndexOf(this);

			parent.Controls.AddAt(index, GridController);
			GridController.ControlsToRender.Add(CollapsedMessageLabel);
			GridController.ControlsToRender.Add(this);
		}

		bool initCompleteHookExecuted;
		public void InitCompleteHook()
		{
			if (!initCompleteHookExecuted)
			{
				initCompleteHookExecuted = true;
				InitGridController();
				string acceptButtonID = (this.NamingContainer != Page ? this.NamingContainer.ClientID : GridController.ID) + "_" + CustomizeColumnsControl.Constants.AcceptButtonID;
				string cancelButtonID = (this.NamingContainer != Page ? this.NamingContainer.ClientID : GridController.ID) + "_" + CustomizeColumnsControl.Constants.CancelButtonID;

				string postBackControlName = WebEnv.AppInstance.Request.Params.Get("__EVENTTARGET");

				if (postBackControlName == acceptButtonID || postBackControlName == cancelButtonID)
				{
					if (Page != null && Page is ZPage)
					{
						((ZPage)Page).SuspendValidationErrorMessageForCurrentLoad();
					}
				}
			}
		}

		void Page_InitComplete(object sender, EventArgs e)
		{
			InitCompleteHook();
		}

		protected override void CreateChildControls()
		{
			if (!ColumnProvider.IsEmpty)
			{
				base.CreateChildControls();
			}
		}

		protected override ArrayList CreateColumnSet(PagedDataSource dataSource, bool useDataSource)
		{
			RepopulateColumns();
			ColumnIndexToInsert = 0;
			return CreateColumnsCore(dataSource, useDataSource);
		}

		public void HandleCustomClicks()
		{
			string layoutPosterID = this.NamingContainer != Page ? this.NamingContainer.ClientID : GridController.ID;
			layoutPosterID += "_" + CustomizeColumnsControl.Constants.AcceptButtonID;
			string postBackControlName = WebEnv.AppInstance.Request.Params.Get("__EVENTTARGET");

			if (postBackControlName == layoutPosterID)
			{
				if (Page != null)
				{
					((ZPage)Page).SuspendValidationErrorMessageForCurrentLoad();
				}
				string passedArgument = WebEnv.AppInstance.Request.Params.Get("__EVENTARGUMENT");
				GridManager.GridLayout = passedArgument;
				Rebind();
			}

			if (isMarkedForExcelExportAfterBound)
			{
				if (DataSource != null)
				{
					ExportIntoExcel();
					isMarkedForExcelExportAfterBound = false;
				}
			}
			if (isMarkedForColumnCustomization)
			{
				if (!GridManager.ColumnProvider.IsEmpty)
				{
					CustomizeColumnsControl customizeControl = new CustomizeColumnsControl() { ID = Constants.CustomizeControlID };
					foreach (var key in GridManager.ColumnProvider.Keys)
					{
						customizeControl.AvailableColumns.Add(key, GridManager.ColumnProvider[key].HeaderText);
					}
					int[] keys = GridManager.SelectedColumnsKeys ?? GridManager.ColumnProvider.DefaultColumns.ToArray();
					customizeControl.SelectedColumns.AddRange(keys);
					customizeControl.RequiredColumns.AddRange(GridManager.ColumnProvider.RequiredColumns.ToArray());

					GridController.AlwaysVisibleHolder.Controls.Add(customizeControl);
				}
				isMarkedForColumnCustomization = false;
			}
			if (isMarkedForDownloadEDocs)
			{
				DownloadEDocs();
				isMarkedForDownloadEDocs = false;
			}
		}

		#endregion

		#region IContainResources Members

		public ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = new ZWebResourceCollection();
				result.Add(ZGridCustomizerScript);
				return result;
			}
		}

		protected ZWebResource ZGridCustomizerScript
		{
			get
			{
				return fzGridCustomizerScript ?? (fzGridCustomizerScript = new ZWebResource(typeof(ZGrid), "ZGridCustomizer.js", (ZPage)Page, "Enterprise.ZArchitecture.Web.GUI.Ajax.WebControls.ZGrid"));
			}
		}

		ZWebResource fzGridCustomizerScript;

		readonly string ZGridCustomizerScriptKey = "ZGridCustomizerScriptKey";

		#endregion

		#region Grid Layout

		#region Repopulate Columns

		public string LayoutNameToUse
		{
			get { return layoutNameToUse; }
			set { layoutNameToUse = value; }
		}
		string layoutNameToUse = string.Empty;

		public void RepopulateColumns()
		{
			SetLayoutAndRepopulateColumns(LayoutNameToUse);
		}

		public void SetLayoutAndRepopulateColumns(string layoutName)
		{
			GridManager.Layout = layoutName;
			GridManager.PopulateColumns();
		}

		public virtual void SaveAsLayoutColumns(string layoutName, bool isPublishedForOrganisation, bool isPublishedForCompany)
		{
			GridManager.SaveAsGridLayout(layoutName, isPublishedForOrganisation, isPublishedForCompany);
		}

		#endregion

		public GridColumnProvider ColumnProvider
		{
			get { return GridManager.ColumnProvider; }
			set
			{
				if (value.UniqueColumnsCount == 0)
				{
					value.CustomizeDictionary();
				}

				if (Columns.Count > 0)
				{
					Columns.Clear();
				}
				AutoGenerateColumns = false;
				GridManager.ColumnProvider = value;
				RepopulateColumns();
			}
		}

		protected ZGridLayoutManager GridManager
		{
			get { return gridManager ?? (gridManager = new ZGridLayoutManager(this)); }
		}
		ZGridLayoutManager gridManager;

		#endregion
	}
}
