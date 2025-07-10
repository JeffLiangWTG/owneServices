using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Web.Business.FilterStrips;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	#region SuppressResourceStringsCheckRegion

	public partial class ZFilterStripControl : BaseUserControl, IContainResources
	{
		#region OnInit (Setup table formatting)

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			Table.CellPadding = 0;
			Table.CellSpacing = 0;
			Table.Attributes["class"] = "FilterStripControl";
		}

		#endregion

		#region Page_Load (Loads ViewState)

		protected void Page_Load(object sender, EventArgs e)
		{
			HookEventsOnFilterBizO();

			if (!Page.IsPostBack)
			{
				if (!ApplyFiltersFromURLParameters())
				{
					LoadFilterLayout(FilterStripBizO.LastUsedLayout ?? DefaultLayout);
				}
			}
			else
			{
				RebuildFilterStripRows(false);

				if (LayoutsHelper.IsCurrentLayoutDeleted)
				{
					LayoutsHelper.ResetIsCurrentLayoutDeleted();
					ResetFilterLayout();
				}
			}

			SetupAjaxControls();
		}

		public StmModuleFilter DefaultLayout
		{
			get { return FilterStripBizO.FindLayout(DefaultLayoutName, true); }
		}

		public ZString DefaultLayoutName
		{
			get;
			set;
		}

		void HookEventsOnFilterBizO()
		{
			// any events added here should be unhooked in Unload
			FilterStripBizO.LayoutLoaded += new EventHandler(FilterStripBizO_LayoutLoaded);
			LayoutsHelper.CurrentLayoutChanged += new EventHandler<FilterStripLayoutsHelperForWeb.CurrentLayoutEventArgs>(LayoutsHelper_CurrentLayoutChanged);
		}

		#endregion

		#region Page_PreRender (Saves ViewState)

		protected void Page_PreRender(object sender, EventArgs e)
		{
			if (FilterWasChanged)
			{
				UnsavedFilterRow.Visible = true;
				Footer.FilterLayoutsDropList.MakeCurrentFilterUnsaved();
			}
			else
			{
				UnsavedFilterRow.Visible = false;
			}

			if (!Page.SiteUser.IsLoggedIn)
			{
				UnsavedFilterRow.Visible = false;
			}

			Footer.ResetHasUserLoadedLayout();

			RegisterReturnKeyCapture();
		}

		#endregion

		#region AJAX

		void SetupAjaxControls()
		{
			//UpdatePanel FilterStripsUpdatePanel = new UpdatePanel();
			//FilterStripsUpdatePanel.ID = "FilterStripsUpdatePanel";
			//FilterStripsUpdatePanel.ContentTemplateContainer.Controls.Add(Table);

			//Controls.Add(FilterStripsUpdatePanel);
		}

		#endregion

		#region FilterStrip Rows

		protected List<ZFilterStripRow> FilterStripRows
		{
			get
			{
				List<ZFilterStripRow> result = new List<ZFilterStripRow>();

				for (int i = 0; i < Table.Rows.Count; i++)
				{
					ZFilterStripRow strip = Table.Rows[i] as ZFilterStripRow;
					if (strip != null)
					{
						result.Add(strip);
					}
				}

				return result;
			}
		}

		bool TableContainsFooterRow
		{
			get
			{
				foreach (HtmlTableRow row in Table.Rows)
				{
					if (row is ZFilterStripRowFooter)
					{
						return true;
					}
				}
				return false;
			}
		}

		bool TableContainsOnlyOneFilterStrip
		{
			get { return FilterStripRows.Count == 1; }
		}

		int InsertIndexOfNextFilterStripRow
		{
			get
			{
				int result = 0;

				if (Table.Rows.Count > 0)
				{
					for (int i = Table.Rows.Count - 1; i >= 0; i--)
					{
						if (Table.Rows[i] is ZFilterStripRow)
						{
							result = i + 1;
							break;
						}
					}
				}

				return result;
			}
		}

		protected void RegisterReturnKeyCapture()
		{
			foreach (ZFilterStripRow strip in FilterStripRows)
			{
				foreach (Control control in strip.ControlsToCaptureReturnKey)
				{
					ZPage.ClientFunctions.RegisterReturnKeyCapture(control, Footer.FindButton);
				}
			}
		}

		#endregion

		#region Adding Filter Strips

		void AddNewFilterStrip()
		{
			AddFilterStrip(FilterStripBizO.FilterStrips.AddNew());
		}

#if DEBUG
		protected
#endif
 void AddFilterStrip(ZGuid pk)
		{
			FilterStrip strip = GetFilterStripDataSource(pk);
			if (strip != null)
			{
				AddFilterStrip(strip);
			}
		}

		protected void AddFilterStrip(FilterStrip strip)
		{
			ZFilterStripRow stripRow = new ZFilterStripRow();
			stripRow.DeleteButtonClick += strip_DeleteButtonClick;
			stripRow.OrCategoryChanged += stripRow_OrCategoryChanged;
			stripRow.FilterStripDeleted += strip_FilterDeleted;
			stripRow.FilterDescriptionChanged += strip_FilterDescriptionChanged;
			stripRowList.Add(stripRow);

			Table.Rows.Insert(InsertIndexOfNextFilterStripRow, stripRow);
			stripRow.Bind(strip);

			UpdateLastDeleteButtonEnabledState();
		}

		readonly List<ZFilterStripRow> stripRowList = new List<ZFilterStripRow>();

		public override void Dispose()
		{
			UnsubscribeHandlers();
			base.Dispose();
		}

		void UnsubscribeHandlers()
		{
			foreach (ZFilterStripRow stripRow in stripRowList)
			{
				stripRow.DeleteButtonClick -= strip_DeleteButtonClick;
				stripRow.OrCategoryChanged -= stripRow_OrCategoryChanged;
				stripRow.FilterStripDeleted -= strip_FilterDeleted;
				stripRow.FilterDescriptionChanged -= strip_FilterDescriptionChanged;
			}

			if (fFooter != null)
			{
				fFooter.ClearFilters -= footer_Clear;
				fFooter.AddFilterStripButtonClick -= footer_AddFilterButtonClick;
				fFooter.LayoutChanged -= new EventHandler<LayoutEventArgs>(footer_LayoutChanged);
				fFooter.FindButtonClick -= footer_FindButtonClick;
				fFooter.SaveLayout -= new EventHandler<LayoutEventArgs>(footer_Save);
				fFooter.ResetLayout -= footer_ResetLayout;
			}
		}

#if DEBUG
		protected
#endif
 FilterStrip GetFilterStripDataSource(ZGuid pk)
		{
			FilterStrip result = (FilterStrip)FilterStripBizO.FilterStrips.FindByPK(pk);

			if (result == null)
			{
				string key = "No FilterStrip found with PK " + pk.ToString() + ".";
				Enterprise.ZArchitecture.Environment.Globals.Message.ShowDeveloperException(
					key,
					key + " Most likely this exception is generated, because pk extracted from ViewState doesn't point to any existing FilterStrip in collection."
					+ System.Environment.NewLine + "Stack trace is:" + System.Environment.NewLine + System.Environment.StackTrace,
					new InvalidOperationException("Result is null"));
			}

			return result;
		}

		public void UpdateLastDeleteButtonEnabledState()
		{
			if (TableContainsOnlyOneFilterStrip)
			{
				FilterStripRows[0].IsOnlyStrip = true;
			}
			else
			{
				foreach (ZFilterStripRow strip in FilterStripRows)
				{
					strip.IsOnlyStrip = false;
				}
			}
		}

		#endregion

		#region FilterStrip Events

		void strip_FilterDescriptionChanged(object sender, EventArgs e)
		{
			MakeCurrentFilterUnsaved();
		}

		void stripRow_OrCategoryChanged(object sender, EventArgs e)
		{
			MakeCurrentFilterUnsaved();
		}

		void strip_DeleteButtonClick(object sender, EventArgs e)
		{
			MakeCurrentFilterUnsaved();
		}

		void strip_FilterDeleted(object sender, EventArgs e)
		{
			UpdateLastDeleteButtonEnabledState();
		}

		void footer_AddFilterButtonClick(object sender, EventArgs e)
		{
			AddNewFilterStrip();
			MakeCurrentFilterUnsaved();
		}

		void MakeCurrentFilterUnsaved()
		{
			if (!LoadingLayoutSemaphore.IsSuspended)
			{
				CurrentFilterIsUnsaved = true;
			}
		}

		#endregion

		#region Footer Events

		void footer_LayoutChanged(object sender, LayoutEventArgs e)
		{
			using (new SemaphoreManager(LoadingLayoutSemaphore))
			{
				LoadFilterLayout(e.Layout);
			}
		}

		void footer_FindButtonClick(object sender, EventArgs e)
		{
			if (OnFind != null)
			{
				OnFind(sender, e);
			}
		}

		void footer_Save(object sender, LayoutEventArgs e)
		{
			SaveFilterLayout(e.Layout);
		}

		void footer_ResetLayout(object sender, EventArgs e)
		{
			ResetFilterLayout();
		}

		public event EventHandler OnFind;

		#endregion

		#region Semaphores / Flags

		public
#if DEBUG
 virtual
#endif
 bool FilterWasChanged
		{
			get { return CurrentFilterIsUnsaved && !Footer.HasUserLoadedLayout; }
		}

		bool CurrentFilterIsUnsaved
		{
			get { return ViewState["IsUnsaved"] != null && (bool)ViewState["IsUnsaved"]; }
			set { ViewState["IsUnsaved"] = value; }
		}

		Semaphore LoadingLayoutSemaphore
		{
			get { return fLoadingLayoutSemaphore ?? (fLoadingLayoutSemaphore = new Semaphore()); }
		}

		Semaphore AutoSavingLayoutSemaphore
		{
			get { return fAutoSavingLayoutSemaphore ?? (fAutoSavingLayoutSemaphore = new Semaphore()); }
		}

		Semaphore fLoadingLayoutSemaphore;
		Semaphore fAutoSavingLayoutSemaphore;

		#endregion

		#region Adding the Row Footer

		void AddRowFooter()
		{
			if (FilterStripRows.Count == 0)
			{
				AddNewFilterStrip();
			}

			if (!TableContainsFooterRow)
			{
				Table.Rows.Add(HrRow);
				Table.Rows.Add(Footer); // ASP .NET will restore the drop list view state here
				Table.Rows.Add(UnsavedFilterRow);

				// adding the footer to the parent will restore the drop list view state, so bind *after* this to ensure the values are correct
				Footer.Bind(FilterStripBizO);
			}
		}

		protected HtmlTableRow HrRow
		{
			get
			{
				if (fHrRow == null)
				{
					var hr = new HtmlGenericControl("hr");
					hr.Attributes.Add("style", "border:dotted 1px silver");

					var cell = new HtmlTableCell();
					cell.Attributes.Add("style", "padding-left:1px;");
					cell.Attributes.Add("style", "padding-right:2px;");
					cell.ColSpan = 3;
					cell.Controls.Add(hr);

					fHrRow = new HtmlTableRow();
					fHrRow.Cells.Add(cell);
					fHrRow.Cells.Add(new HtmlTableCell());
				}

				return fHrRow;
			}
		}
		HtmlTableRow fHrRow;

		protected ZFilterStripRowFooter Footer
		{
			get
			{
				if (fFooter == null)
				{
					fFooter = new ZFilterStripRowFooter();

					fFooter.ClearFilters += new EventHandler(footer_Clear);
					fFooter.AddFilterStripButtonClick += new EventHandler(footer_AddFilterButtonClick);
					fFooter.LayoutChanged += new EventHandler<LayoutEventArgs>(footer_LayoutChanged);
					fFooter.FindButtonClick += new EventHandler(footer_FindButtonClick);
					fFooter.SaveLayout += new EventHandler<LayoutEventArgs>(footer_Save);
					fFooter.ResetLayout += new EventHandler(footer_ResetLayout);
				}
				return fFooter;
			}
		}

		public Button FindButton
		{
			get { return Footer.FindButton; }
		}

#if DEBUG
		protected
#endif
 HtmlTableRow UnsavedFilterRow
		{
			get
			{
				if (fUnsavedFilterRow == null)
				{
					ZTextLabel label = new ZTextLabel(Res.GetString("e98f2567-6aa2-46db-9c66-3e44bfd035b0", "*The current filter layout is unsaved"));
					label.Font.Bold = true;
					label.Font.Size = 7;
					label.ForeColor = Color.Red;

					HtmlTableCell cell = new HtmlTableCell();
					cell.Align = "right";
					cell.Attributes["style"] = "padding-top:2px; padding-right:2px;";
					cell.ColSpan = 3;
					cell.Controls.Add(label);

					fUnsavedFilterRow = new HtmlTableRow();
					fUnsavedFilterRow.Cells.Add(cell);
					fUnsavedFilterRow.Cells.Add(new HtmlTableCell());
				}

				return fUnsavedFilterRow;
			}
		}

		ZFilterStripRowFooter fFooter;
		HtmlTableRow fUnsavedFilterRow;

		#endregion

		#region Load Filter Layout

		void LoadFilterLayout(StmModuleFilter layout)
		{
			using (new SemaphoreManager(LoadingLayoutSemaphore))
			{
				try
				{
					FilterStripBizO.LoadLayout(layout);
					if (Page.IsPostBack) // no need to save it on first load
					{
						FilterStripBizO.SaveLastUsedLayout();
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					if (DefaultLayout != null && (layout == null || layout.PK != DefaultLayout.PK))
					{
						LoadFilterLayout(DefaultLayout);
					}
					else
					{
						RebuildFilterStripRows(true);
					}
				}
			}
		}

		void FilterStripBizO_LayoutLoaded(object sender, EventArgs e)
		{
			RebuildFilterStripRows(true);
		}

		void RebuildFilterStripRows(bool changeCurrentFilterIsUnsavedState)
		{
			ClearFilterStripRows();

			for (int i = 0; i < FilterStripBizO.FilterStrips.Count; i++)
			{
				FilterStrip bizO = FilterStripBizO.FilterStrips[i];
				if (bizO.IsFilterDescriptionEmpty && FilterStripBizO.FilterStrips.Count > 1) // don't want to add empty filters until its the only one
				{
					FilterStripBizO.FilterStrips.RemoveAndDelete(bizO);
					i--;
				}
				else
				{
					AddFilterStrip(bizO);
				}
			}

			AddRowFooter();
			if (changeCurrentFilterIsUnsavedState)
			{
				CurrentFilterIsUnsaved = false;
			}
		}

		void ClearFilterStripRows()
		{
			for (int i = FilterStripRows.Count - 1; i >= 0; i--)
			{
				FilterStripRows[i].Delete();
			}
		}

		#endregion

		#region Save Filter Layout

		void SaveFilterLayout(StmModuleFilter layout)
		{
			if (!FilterWasSaved && layout != null)
			{
				FilterStripBizO_LayoutSaved(layout);
				CurrentFilterIsUnsaved = false;
			}
		}

		void LayoutsHelper_CurrentLayoutChanged(object sender, FilterStripLayoutsHelperForWeb.CurrentLayoutEventArgs e)
		{
		}

		void FilterStripBizO_LayoutSaved(StmModuleFilter layout)
		{
			FilterWasSaved = true;

			if (!AutoSavingLayoutSemaphore.IsSuspended)
			{
				RaiseBeforeSaveLayout(layout);
				LayoutsHelper.CurrentLayout = layout;
				Footer.FilterLayoutsDropList.Rebind(); // rebind the droplist so that it shows the newly-saved filter
			}
		}

		void RaiseBeforeSaveLayout(StmModuleFilter layout)
		{
			if (BeforeSaveLayout != null)
			{
				BeforeSaveLayout(this, new LayoutEventArgs(layout));
			}
		}

		public event EventHandler<LayoutEventArgs> BeforeSaveLayout;

		bool FilterWasSaved;

		#endregion

		#region Reset Filter Layout

		void ResetFilterLayout()
		{
			LoadFilterLayout(FilterStripBizO.LastUsedLayout ?? DefaultLayout);
		}

		#endregion

		#region Clear Values

		public event EventHandler OnClear;

		void footer_Clear(object sender, EventArgs e)
		{
			ClearValues();
			if (OnClear != null)
			{
				OnClear(sender, e);
			}
		}

		void ClearValues()
		{
			foreach (ZFilterStripRow strip in FilterStripRows)
			{
				strip.Clear();
			}
		}

		#endregion

		#region ApplyFiltersFromURLParameters

		bool ApplyFiltersFromURLParameters()
		{
			bool validFilterParameterFound = false;

			if (HttpContext.Current.Request.QueryString.Count > 0)
			{
				FilterStripBizO.FilterStrips.RemoveAll();

				foreach (string filterName in HttpContext.Current.Request.QueryString.Keys)
				{
					ModuleFilter filter = FilterStripBizO[filterName];
					if (filter != null)
					{
						AddNewFilterStrip();
						FilterStripBizO.FilterStrips[InsertIndexOfNextFilterStripRow - 1].FilterDescription = filterName;
						validFilterParameterFound = true;
					}
				}

				AddRowFooter();

				FilterStripURLParameterHelper parameterHelper = new FilterStripURLParameterHelper();
				parameterHelper.SetupFilterStripsFromQueryString(FilterStripBizO, HttpContext.Current.Request.QueryString.ToString());

				for (int i = 0; i < Table.Rows.Count; i++)
				{
					ZFilterStripRow row = Table.Rows[i] as ZFilterStripRow;
					if (row != null && i < FilterStripBizO.FilterStrips.Count)
					{
						row.Bind(FilterStripBizO.FilterStrips[i]);
					}
				}
			}

			return validFilterParameterFound;
		}

		#endregion

		#region Unload

		protected override void OnUnload(EventArgs e)
		{
			UnhookEventsOnFilterBizO();
			base.OnUnload(e);
		}

		void UnhookEventsOnFilterBizO()
		{
			if (FilterStripBizO != null)
			{
				FilterStripBizO.LayoutLoaded -= new EventHandler(FilterStripBizO_LayoutLoaded);
			}

			if (LayoutsHelper != null)
			{
				LayoutsHelper.CurrentLayoutChanged -= new EventHandler<FilterStripLayoutsHelperForWeb.CurrentLayoutEventArgs>(LayoutsHelper_CurrentLayoutChanged);
			}
		}

		#endregion

		#region FilterStripBizO

		public FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				return filterStripBizO ?? Page.DataSource as FilterStripBusinessObject;
			}
			set
			{
				filterStripBizO = value;
			}
		}

		FilterStripBusinessObject filterStripBizO;

		#endregion

		FilterStripLayoutsHelperForWeb LayoutsHelper
		{
			get { return (FilterStripBizO != null) ? FilterStripBizO.LayoutsHelper as FilterStripLayoutsHelperForWeb : null; }
		}

		#region IContainResources Members

		public ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection fResources = new ZWebResourceCollection();
				fResources.Add(HelpImageResource);
				return fResources;
			}
		}

		#region HelpImageResource

		public ZWebResource HelpImageResource
		{
			get
			{
				if (fHelpImageResource == null)
				{
					fHelpImageResource = new ZWebResource(typeof(ZFilterStripGridModule), helpImageResourceFileName, Page, "Enterprise.ZArchitecture.Web.GUI.FilterStrips");
				}
				return fHelpImageResource;
			}
		}
		ZWebResource fHelpImageResource;
		const string helpImageResourceFileName = "help.png";

		#endregion

		#endregion
	}

	#region class LayoutNameEventArgs

	public class LayoutEventArgs : EventArgs
	{
		public LayoutEventArgs(StmModuleFilter layout)
		{
			Layout = layout;
		}

		public readonly StmModuleFilter Layout;
	}

	#endregion

	#endregion
}
