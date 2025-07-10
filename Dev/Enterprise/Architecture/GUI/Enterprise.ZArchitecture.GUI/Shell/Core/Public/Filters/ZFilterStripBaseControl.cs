using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ZFilterStripBaseControl : StripControl, IResultCountHandler
	{
		protected ZFilterStripBaseControl()
		{
			finishedConstruction = true;
		}

		public ZFilterStripBaseControl(FilterStripBusinessObject filterBusinessObject) : base(filterBusinessObject)
		{
			HookEventsOnFilterBizO();
			SetBackgroundColor();

			OnFilterStripLoaded += delegate
			{ FilterStripLoaded(); };
			OnStripsInitialized += delegate
			{ StripsInitialized(); };

			ToolStripColourPicker.Visible = false;
			finishedConstruction = true;

			RegisterHotkeys();

			CoveringLabel.AllowOverlap(ToolStrip);
			CoveringLabel.AllowOverlap(ToolStripHelp);
		}

		public ZFilterStripBaseControl(ZGrid grid, FilterStripBusinessObject filterBusinessObject) : this(filterBusinessObject)
		{
			this.grid = grid;
		}

		bool finishedConstruction { get; }

		public override FilterStripBusinessObject FilterBusinessObject
		{
			get
			{
				if (finishedConstruction && queryObjectTypeNotSet && base.FilterBusinessObject != null)
				{
					if (base.FilterBusinessObject.QueryObjectType == null)
					{
						if (RelatedGrid != null && RelatedGrid.BindingContextInitialized)
						{
							var list = RelatedGrid.List as IBusinessObjectCollection;

							if (list != null)
							{
								base.FilterBusinessObject.QueryObjectType = list.TypeOfElements;
								queryObjectTypeNotSet = false;
							}
						}
					}
					else
					{
						queryObjectTypeNotSet = false;
					}
				}

				return base.FilterBusinessObject;
			}
		}

		bool queryObjectTypeNotSet = true;

		#region Grid

		readonly ZGrid grid;

		public virtual ZGrid RelatedGrid { get { return grid; } }

		protected virtual Control ControlForLayout
		{
			get { return RelatedGrid; }
		}

		#endregion

		#region GridCollection

		public virtual IBusinessObjectCollection GridCollection
		{
			get
			{
				return RelatedGrid.List as IBusinessObjectCollection ?? throw new InvalidOperationException("Should never be null");
			}
		}

		#endregion

		#region Initialisation

		public ModuleIdentifier ParentModuleID { get; set; } = ModuleIDs.NotAssigned;

		protected virtual bool GetDefaultShouldRunSearchOnStripsInitialized() => true;

		public bool ShouldRunSearchOnStripsInitialized
		{
			get => shouldRunSearchOnStripsInitialized ?? (bool)(shouldRunSearchOnStripsInitialized = GetDefaultShouldRunSearchOnStripsInitialized());
			set => shouldRunSearchOnStripsInitialized = value;
		}
		bool? shouldRunSearchOnStripsInitialized;

		void StripsInitialized()
		{
			if (ShouldRunSearchOnStripsInitialized)
			{
				Find(isManualSearch: false);
			}
		}

		public void FilterStripLoaded()
		{
			var clearCollectionResults = !ShouldRunSearchOnStripsInitialized;
			var layoutToUse = LayoutToLoad ?? FilterBusinessObject.LastUsedLayout;

			if (layoutToUse != null && (ShouldLoadLayoutEvenWhenUnsaved || GetItemFromFindDropButton(ToolStripFindDropButton.DropDownItems, layoutToUse.PK) != null))
			{
				SelectUserFilter(layoutToUse, clearCollectionResults, isManualSearch: false);
			}
			else
			{
				// can happen if a saved filter name no longer exists
				SelectUserFilter((StmModuleFilter)ToolStripFindDropButton.DropDownItems.Cast<ToolStripItem>().Last().Tag, clearCollectionResults, isManualSearch: false);
			}

			if (FilterBusinessObject.ParentModule?.LimitedColumns != null && !(FilterBusinessObject.ParentModule.LimitedColumns as ZLimitedColumnsProvider).LimitColumnExists)
			{
				allowPerformFind = false;
				IsFilterReadonly = true;
				var moduleAccess = Env.Security.FindOrCreateAccessModuleCheckPoint((FilterBusinessObject.ParentModule as ZModule).SecurityCheckpoint);
				moduleAccess.ShowError();
			}
		}

		bool allowPerformFind = true;

		protected override void HookButtonEvents()
		{
			base.HookButtonEvents();
			ToolStripFindDropButton.ButtonClick += ToolStripFindDropButton_ButtonClick;
			ToolStripFindDropButton.DropDownItemClicked += ToolStripFindDropButton_DropDownItemClicked;
			ToolStripFindDropButton.TextChanged += ToolStripFindDropButton_TextChanged;
		}

		#endregion

		#region Visual Styles

		void SetBackgroundColor()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				BackColor = ObjectFactory.Get<ISystemDataRegistry>().ColorTheme.FilterBackgroundColor;
			}
		}

		#endregion

		#region Events

		void HookEventsOnFilterBizO()
		{
			if (FilterBusinessObject != null)
			{
				GridModuleFilterLayoutDeletedEvent.AddLayoutDeletedEventHandler(((IModifyModuleAndGridLayout)FilterBusinessObject).Factory, RemoveDeletedLayout);
				GridModuleFilterSavedLayoutUpdatedEvent.AddLayoutUpdatedEventHandler(((IModifyModuleAndGridLayout)FilterBusinessObject).Factory, UpdateFilterDescription);
			}
		}

		protected override void UnhookEventsOnFilterBizO()
		{
			if (FilterBusinessObject != null)
			{
				GridModuleFilterLayoutDeletedEvent.RemoveLayoutDeletedEventHandler(((IModifyModuleAndGridLayout)FilterBusinessObject).Factory, RemoveDeletedLayout);
				GridModuleFilterSavedLayoutUpdatedEvent.RemoveLayoutUpdatedEventHandler(((IModifyModuleAndGridLayout)FilterBusinessObject).Factory, UpdateFilterDescription);
			}
			base.UnhookEventsOnFilterBizO();
		}

		#endregion

		#region Filter Visibility

		public bool IsFilterVisible
		{
			get => FilterStripsPanel.Visible;

			set
			{
				if (value != IsFilterVisible)
				{
					var controlForLayout = ControlForLayout;
					if (controlForLayout != null)
					{
						controlForLayout.Dock = value ? DockStyle.None : DockStyle.Fill;
					}
					SetFilterControlVisibility(value);
					controlForLayout?.BringToFront();
				}
			}
		}

		bool isFilterReadonly;
		public bool IsFilterReadonly
		{
			get { return isFilterReadonly; }
			set
			{
				if (isFilterReadonly != value)
				{
					isFilterReadonly = value;

					if (value)
					{
						FilterBusinessObject.LayoutLoaded += (s, e) => MakeFilterStripsReadOnly();
					}

					ToolStripHelp.Enabled = !value;
					ToolStripClearButton.Enabled = !value;
					ToolStripFindDropButton.Enabled = !value;
					AddStripButton.Enabled = !value;
				}
			}
		}

		protected override bool ShouldGroupStripsBeReadOnly => IsFilterReadonly;

		void MakeFilterStripsReadOnly()
		{
			foreach (var strip in FilterStripsPanel.FindAll<ZFilterStrip>())
			{
				strip.ReadOnly = true;
			}
		}

		void SetFilterControlVisibility(bool visible)
		{
			FilterStripsPanel.Visible = visible;
			ToolStripHelp.Visible = visible;
			ToolStrip.Visible = visible;
			AddStripButton.Visible = visible;
			ToolStripRecordsFoundLabel.Visible = visible;
			ToolStripColourPicker.Visible = visible;
			SetRecentItemsVisibility(visible);
			UpdateFilteredGridRefreshWarning();
		}

		#endregion

		#region Key Handler

		public override string TypeNameForDisplay => Res.GetString("d2deeeee-e269-4cf0-a9ca-85e3e6437624", "Filter Strip");

		void RegisterHotkeys()
		{
			Hotkeys.RegisterHotKey(Keys.Control | Keys.Enter, OnControlEnterKeyPress, Res.GetString("209ed0af-7ed1-4e59-9314-f245332bceea", "Find"));
			Hotkeys.RegisterHotKey(Keys.Alt | Keys.Enter, OnAltEnterKeyPress, Res.GetString("7ea021d9-dc05-467a-92b9-d1c099586142", "Clear"));
			Hotkeys.RegisterHotKey(Keys.Control | Keys.E, AddStrip, Res.GetString("d6bcf5cf-6f20-490e-a2ea-2cfc81c19373", "Add New Strip"));

			Hotkeys.RegisterHotKey(Keys.Control | Keys.Alt | Keys.D, ControlAltD);
			Hotkeys.RegisterHotKey(Keys.Control | Keys.Shift | Keys.D, ControlAltD);
			void ControlAltD()
			{
				var frontMostActiveControl = ActiveControl.GetFrontMostActiveControl();
				CommitReadOnlyBindings(frontMostActiveControl);
				HandleCtrlAltDAndCtrlShiftDKey();
			}
		}

		protected void AdjustFocus(Control frontMostActiveControl)
		{
			if (RelatedGrid != null && RelatedGrid.List != null && RelatedGrid.List.Count > 0 && RelatedGrid.Visible && RelatedGrid.Enabled)
			{
				try
				{
					ActiveControl = RelatedGrid;
					RelatedGrid.Select(0);
				}
				catch (ArgumentException) { } //Invisible or disabled control cannot be activated
			}
			else if (frontMostActiveControl != null && frontMostActiveControl.Visible && frontMostActiveControl.Enabled)
			{
				try
				{
					ActiveControl = frontMostActiveControl;
				}
				catch (ArgumentException) { } //Invisible or disabled control cannot be activated
			}
		}

		protected void OnAltEnterKeyPress()
		{
			var frontMostActiveControl = ActiveControl.GetFrontMostActiveControl();
			PersistCurrentControlValue();
			ClearStrips();
			AdjustFocus(frontMostActiveControl);
		}

		protected virtual void OnControlEnterKeyPress()
		{
			var frontMostActiveControl = ActiveControl.GetFrontMostActiveControl();
			PersistCurrentControlValue();
			Find();
			AdjustFocus(frontMostActiveControl);
		}

		#region Developer Information

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static bool DevloperHasLoggedInOnce;

		void HandleCtrlAltDAndCtrlShiftDKey()
		{
			if (DeveloperLoginForm.TryAuthenticate())
			{
				ShowDiagnosticsForm();
			}
		}

		void ShowDiagnosticsForm()
		{
			if (RelatedGrid != null && RelatedGrid.List is IBusinessObjectCollection && FilterBusinessObject != null)
			{
				var diagnosticsForm = new ZDeveloperFilterDiagnosticsForm(FilterBusinessObject, GridCollection);
				diagnosticsForm.Show();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("FilterStripControl|ShowDiagnosticsFormError", "Could not show Diagnostics Form because Grid, Grid Collection or Filter Business Object is null."));
			}
		}

		#endregion

		#endregion

		#region Search

		[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		public void FirePerformSearch(bool showError = true, bool isManualSearch = true)
		{
			var form = FindForm();

			using (PerformanceStatisticsCollector.StartMonitoring("PerformSearch", FilterBusinessObject != null ? FilterBusinessObject.ToString() : null))
			{
				if (!ShouldPerformSearch() || !allowPerformFind)
				{
					return;
				}

				FilterBusinessObject?.RunPreSaveValidation();

				if (FilterBusinessObject != null && FilterBusinessObject.HasErrors)
				{
					Globals.Message.Show(Res.GetString("FilterStripControl|ThereAreErrors", "There are errors. Please correct these before searching."), Res.GetString("FilterStripControl|Errors", "Errors..."), MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK, form);
				}
				else
				{
					try
					{
						if (PerformSearchAsync != null)
						{
							using (new ZWaitCursorChanger(form))
							{
								PerformSearchAsync(this, new PerformSearchAsyncEventArgs(AfterSearch, OnBeginPerformSearchAsync, OnAfterPerformSearchAsync));
							}
						}
						else if (PerformSearch != null)
						{
							using (new ZWaitCursorChanger(form))
							{
								PerformSearch(this, new PerformSearchEventArgs());
								AfterSearch();
							}
						}
						else
						{
							AfterSearch(); // For all of those tests that never set PerformSearch but still expect search to have occurred.
						}
					}
					catch (System.Data.Common.DbException ex)
					{
						var customSqlFilters = FilterBusinessObject?.ActiveModuleFilters.OfType<ModuleSQLFilter>().ToArray();
						var userDefinedFilters = FilterBusinessObject?.ActiveModuleFilters.OfType<ModuleUserDefinedFilter>().ToArray();
						if (!customSqlFilters.IsNullOrEmpty() || !userDefinedFilters.IsNullOrEmpty())
						{
							DisplayCustomSqlError(ex.Message, customSqlFilters, userDefinedFilters);
						}
						else
						{
							throw;
						}
					}
				}
			}

			void AfterSearch()
			{
				OnAfterPerformSearch?.Invoke(this, EventArgs.Empty);
				OnSearchPerformed(showError, true, isManualSearch, form);
			}
		}

		void OnBeginPerformSearchAsync()
		{
			FilterStripsPanel.SetReadOnly(true);
			lastSearchText = ToolStripFindDropButton.Text;
			lastSearchImage = ToolStripFindDropButton.Image;
			ToolStripFindDropButton.Text = CancelSearchFindButtonText;
			ToolStripFindDropButton.Image = Icons.GetImage(IconTypes.BlackWhite_Cross);
			RefreshGroupLayout();
		}
		string lastSearchText;

		Image lastSearchImage;

		void OnAfterPerformSearchAsync()
		{
			ToolStripFindDropButton.Text = lastSearchText;
			ToolStripFindDropButton.Image = lastSearchImage;
			FilterStripsPanel.SetReadOnly(false);
			RefreshGroupLayout();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		static void DisplayCustomSqlError(string error, IEnumerable<ModuleSQLFilter> sqlFilters, IEnumerable<ModuleUserDefinedFilter> userDefinedFilters)
		{
			var sqlFilterErrorMessage = string.Empty;
			var userDefinedFilterErrorMessage = string.Empty;
			var custom = false;

			if (!sqlFilters.IsNullOrEmpty())
			{
				custom = true;
				sqlFilterErrorMessage = System.Environment.NewLine + string.Join(System.Environment.NewLine, sqlFilters.Select(f => "   " + f.Property1.ToString()));
			}

			if (!userDefinedFilters.IsNullOrEmpty())
			{
				custom = true;
				userDefinedFilterErrorMessage = System.Environment.NewLine + string.Join(System.Environment.NewLine, userDefinedFilters.Select(f => "   " + f.LayoutName));
			}

			if (custom && error.Contains("conver", StringComparison.OrdinalIgnoreCase))
			{
				error = (NoResString)"Conversion failed."; //NoResString because it's just truncating an English language SQL error message
			}

			var message = Res.GetString("AE6F15A0-8556-4A86-A8C3-81C266D94741",
@"The search had an error that may be caused by a Custom SQL Filter. Custom SQL Filters may also be part of a User-Defined Filter.
Please remove Custom SQL Filters from the search or from any User-Defined Filters and try again before reporting an incident.

Error message: {0}

Custom SQL Filters should be of the form [Column 1] = 'Value 1' AND [Column 2] = 'Value 2'.

Your Custom SQL Filter(s):{1}
Your User-Defined Filter(s):{2}", error, sqlFilterErrorMessage, userDefinedFilterErrorMessage);

			Globals.Message.ShowError(message);
		}

		public bool ShowSearchResultsMessageBox { get; set; }

		protected virtual void OnSearchPerformed(bool showError, bool didSearch, bool isManualSearch, Form form)
		{
			if (ShowSearchResultsMessageBox && showError && didSearch && ShouldShowNumberLoadedMessageBox && FilterBusinessObject != null && !FilterBusinessObject.HasErrors)
			{
				var popup = form as EmbeddedModulePopup;

				if (popup == null || isManualSearch)
				{
					ShowFindResultsError();
				}
				else
				{
					form.Shown += (s, e) =>
					{
						ShowFindResultsError();

						if (IsFilterReadonly)
						{
							popup.DialogResult = DialogResult.Abort; // also closes the form when the user clicks OK
						}
					};
				}
			}
		}

		void ShowFindResultsError()
		{
			Globals.Message.Show(NumberRecordsLoadedLabelText, Res.GetString("FilterStripControl|SearchResults", "Search Results"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		protected virtual ZBool ShouldPerformSearch()
		{
			return true;
		}

		public void DisableSearch()
		{
			AlreadyLoaded = true;
			foreach (Control control in Controls)
			{
				if (!(control is ZGrid))
				{
					control.Visible = false;
				}
			}
		}

		#endregion

		#region Auto Focus

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible && IsAutoFocus)
			{
				AutoFocus();
			}
		}

		void AutoFocus()
		{
			SelectNextControl(RelatedGrid, true, true, true, true);
		}

		// don't auto focus if no strips added, (so that the droplist will show the "please select filter" text)
		bool IsAutoFocus
		{
			get { return DesignModeFinder.IsDesigning || FilterBusinessObject.Layouts.Count > 0; }
		}

		#endregion

		#region Auto-Refresh

		public AutoRefreshWarningType AutoRefreshWarning
		{
			get { return fAutoRefreshWarning; }
			set
			{
				if (fAutoRefreshWarning != value)
				{
					fAutoRefreshWarning = value;
					SetAutoRefreshWarningLabelMessage();
					UpdateLayout();
				}
			}
		}

		AutoRefreshWarningType fAutoRefreshWarning;

		internal string AutoRefreshSlowQueryWarningMessage => (Res.GetString("8f519cf0-4f2a-4f2c-bf81-a2631274d2a8", @"Auto-Refresh has been temporarily disabled because the search was too slow on 2 successive retrieves.
Try optimizing the filter, reducing the record set and removing any 'contains' filters or any of the more complex filters."));

		internal string AutoRefreshQueryQueryErrorMessage => (Res.GetString("6f83204d-6f16-4a23-9bcb-7ad99149d298", @"Auto-Refresh has been disabled because the search query contains errors."));

		void SetAutoRefreshWarningLabelMessage()
		{
			string message;
			switch (AutoRefreshWarning)
			{
				case AutoRefreshWarningType.SlowQuery:
					message = AutoRefreshSlowQueryWarningMessage;
					break;
				case AutoRefreshWarningType.ErrorQuery:
					message = AutoRefreshQueryQueryErrorMessage;
					break;
				default:
					message = "";
					break;
			}

			AutoRefreshWarningLabel.Text = message;
			AutoRefreshWarningLabel.Visible = (AutoRefreshWarning != AutoRefreshWarningType.None);
		}

		protected override void UpdateFilteredGridRefreshWarning()
		{
			if (ControlForLayout != null)
			{
				ControlDpiScalingHelper.SetTop(ControlForLayout, AddStripButton.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(4), false);

				if (AutoRefreshWarningLabel.Visible)
				{
					if (IsFilterVisible)
					{
						ControlDpiScalingHelper.SetTop(AutoRefreshWarningLabel, ControlForLayout.Top, false);
						ControlDpiScalingHelper.SetTop(ControlForLayout, ControlForLayout.Top + AutoRefreshWarningLabel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
					}
					else
					{
						if (ControlForLayout.Dock == DockStyle.Fill)
						{
							ControlForLayout.Dock = DockStyle.None;
						}
						ControlDpiScalingHelper.SetTop(AutoRefreshWarningLabel, 0, false);
						ControlDpiScalingHelper.SetTop(ControlForLayout, AutoRefreshWarningLabel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
					}
				}
				else
				{
					if (!IsFilterVisible)
					{
						if (ControlForLayout.Dock == DockStyle.None)
						{
							ControlForLayout.Dock = DockStyle.Fill;
						}
					}
				}

				UpdateToolStripRecordsFoundLabelTop();
			}
		}

		protected ZLabel AutoRefreshWarningLabel
		{
			get
			{
				if (fAutoRefreshWarningLabel == null)
				{
					fAutoRefreshWarningLabel = new ZLabel();
					fAutoRefreshWarningLabel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
					fAutoRefreshWarningLabel.AutoSize = true;
					fAutoRefreshWarningLabel.Font = new Font(OFont.NormalFontName, 8.25F, FontStyle.Bold);
					fAutoRefreshWarningLabel.ForeColor = Color.Red;
					ControlDpiScalingHelper.SetLeft(ref fAutoRefreshWarningLabel, 3, true);
					fAutoRefreshWarningLabel.Visible = false;

					Controls.Add(fAutoRefreshWarningLabel);
					fAutoRefreshWarningLabel.BringToFront();
				}
				return fAutoRefreshWarningLabel;
			}
		}

		ZLabel fAutoRefreshWarningLabel;

		#endregion

		#region Rename / Delete

		void UpdateFilterDescription(GridModuleFilterSavedLayoutUpdatedEventArgs e)
		{
			var itemHasChangedColumnSavingAbility = (e.saveColumnLayoutOption != SaveColumnLayout.Ignore);
			if (RelatedGrid != null && RelatedGrid.Visible && itemHasChangedColumnSavingAbility)
			{
				RelatedGrid.SaveLayoutEvenIfColumnsAreUnchanged();
				RelatedGrid.LoadUserLayoutSettings();
			}

			ReloadFindDropList();

			if (IsLayoutCurrentlySelectedInFindList(e.filter.PK))
			{
				UpdateFindButtonText(e.filter);
			}
			else
			{
				UpdateDropDownStyle(e.filter);
			}
		}

		bool IsLayoutCurrentlySelectedInFindList(ZGuid layoutPk)
		{
			return (CurrentLayout != null && CurrentLayout.PK == layoutPk);
		}

		StmModuleFilter CurrentLayout
		{
			get { return (StmModuleFilter)ToolStripFindDropButton.Tag; }
		}

		ToolStripDropDownItem GetTopItem(ToolStripMenuItem item)
		{
			var topItemToDelete = item as ToolStripDropDownItem;
			while (topItemToDelete.OwnerItem != null && topItemToDelete.OwnerItem.Tag == null && ((ToolStripDropDownItem)topItemToDelete.OwnerItem).DropDownItems.Count == 1)
			{
				topItemToDelete = topItemToDelete.OwnerItem as ToolStripDropDownItem;
			}
			return topItemToDelete;
		}

		void RemoveDeletedLayout(GridModuleFilterLayoutDeletedEventArgs e)
		{
			var item = GetItemFromFindDropButton(ToolStripFindDropButton.DropDownItems, e.layoutPk);

			if (item != null)
			{
				if (item.DropDownItems.Count == 0)
				{
					var topItemToDelete = GetTopItem(item);

					if (topItemToDelete.OwnerItem != null)
					{
						var owner = topItemToDelete.OwnerItem as ToolStripDropDownItem;
						owner.DropDownItems.Remove(topItemToDelete);
					}
					else
					{
						topItemToDelete.DropDownItems.Clear();
					}
				}
				else
				{
					item.ToolTipText = string.Empty;
					item.Tag = null;
					item.Click -= new EventHandler(ItemClicked);
				}

				if (IsLayoutCurrentlySelectedInFindList(e.layoutPk))
				{
					ResetFindButton();
					Find();
				}
			}
		}

		void RemoveFilterCollectionFindBoxForDeletedFilters()
		{
			var stripsToBeUpdated = Strips.Where(strip => strip.CurrentDataItem != null && strip.CurrentDataItem.CurrentModuleFilter == null);

			foreach (var strip in stripsToBeUpdated)
			{
				var itemToRemove = strip.Controls.OfType<ZFilterCollectionFindBox>().FirstOrDefault();

				if (itemToRemove != null)
				{
					strip.Controls.Remove(itemToRemove);
				}
			}
		}

		#endregion

		#region Adding / Deleting Filter Strips

		protected override void AddAlwaysVisibleFilterStrips()
		{
			if (FilterBusinessObject.SearchType == SearchType.Index)
			{
				FilterBusinessObject.AddAlwaysVisibleFilters();
				foreach (FilterStrip filterStrip in FilterBusinessObject.FilterStrips)
				{
					AddFilterStrip(NewZFilterStrip(), filterStrip);
				}
			}
			else
			{
				foreach (var moduleFilter in FilterBusinessObject.AlwaysVisibleModuleFilters)
				{
					var newStrip = FilterBusinessObject.FilterStrips.AddNew(moduleFilter);
					AddFilterStrip(NewZFilterStrip(), newStrip);
				}
			}
		}

		protected void UpdateToolStripRecordsFoundLabelTop()
		{
			ControlDpiScalingHelper.SetTop(ref ToolStripRecordsFoundLabel, ToolStrip.Bottom - ToolStripRecordsFoundLabel.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(4), false);
		}

		#endregion

		#region Buttons

		#region Help

		protected override ZString UpdateNoteURL
		{
			get { return "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20061128b.pdf"; }
		}

		#endregion

		#region Save

		protected override void SaveLayout()
		{
			var form = TopLevelControl?.FindForm();

			if (form is Enterprise.Core.Modules.IMainForm mainForm
				&& mainForm.CurrentModule.SecurityCheckpoint != null
				&& !mainForm.CurrentModule.SecurityCheckpoint.IsAllowed
#if DEBUG
				&& (!NUnit.Framework.TestingState.IsRunningTests || !BypassSecruityCheckForTest)
#endif
				)
			{
				mainForm.CurrentModule.SecurityCheckpoint.ShowError();
				return;
			}

			var saveLayoutBizObj = GetQueryHander().QueryUser(
				layoutManageable: FilterBusinessObject,
				shouldShowSaveColumnCheckBox: CanSaveColumnLayouts,
				shouldShowSaveAsUserDefinedFilter: true,
				defaultValues: GetDefaultValuesForSaveLayoutFunc?.Invoke());

			if (saveLayoutBizObj != null)
			{
				try
				{
					var filter = DataGridLayoutManager.SaveLayout(saveLayoutBizObj, FilterBusinessObject, CanSaveColumnLayouts);

					if (RelatedGrid != null && RelatedGrid.Visible)
					{
						if (filter.S9_SaveGridColourLayout && filter.S9_GridColourLayoutID.IsEmpty)
						{
							var colourScheme = RelatedGrid.GetLastUsedColourSchemeForCurrentUser;
							if (colourScheme != null)
							{
								filter.S9_GridColourLayoutID = colourScheme.PK;
							}
						}

						RelatedGrid.CurrentColumnLayout = filter;
						RelatedGrid.SaveLayoutEvenIfColumnsAreUnchanged();
					}

					if (saveLayoutBizObj.IsUserDefinedFilter)
					{
						UpdateUserDefinedFilters();
					}
					else
					{
						AddOrUpdateExistingFindDropListItem(filter);
						UpdateFindButtonText(filter, allowLoad: false);
						FilterBusinessObject.SaveLastUsedLayout(filter.PK);
						RefreshStripLayout(); //to re-align Find/Clear/Add buttons
					}

					LayoutSaved?.Invoke(this, new LayoutSavedEventArgs(filter));
				}
				catch (ZSaveConcurrencyException ex)
				{
					Globals.Message.ShowError(new ConcurrencyExceptionHandler(ex).UserFriendlyMessage);
				}
				catch (ZSaveException ex)
				{
					var uniqueIndexName = ex.IndexNameIfUniqueIndexViolation;

					if (!uniqueIndexName.IsEmpty)
					{
						foreach (IBusinessObjectInternals bizObj in ex.BusinessObjects)
						{
							var handler = bizObj.UniqueIndexFailureHandlers.FirstOrDefault(h => h.HandledUniqueIndexNames.Contains(uniqueIndexName.ToString()));

							if (handler != null)
							{
								handler.NotifyUserAndAttemptToResolve(NotificationHandler.Instance, uniqueIndexName);
							}
							else
							{
								throw;
							}
						}
					}
					else
					{
						throw;
					}
				}
			}
		}

#if DEBUG
		public bool BypassSecruityCheckForTest = true;
#endif

		internal event EventHandler<LayoutSavedEventArgs> LayoutSaved;

		internal class LayoutSavedEventArgs : EventArgs
		{
			public StmModuleFilter SavedLayout { get; }

			public LayoutSavedEventArgs(StmModuleFilter savedLayout)
			{
				SavedLayout = savedLayout;
			}
		}

		internal Func<SaveLayoutBizO> GetDefaultValuesForSaveLayoutFunc { get; set; }

		/// <summary>
		/// Overriding this property will disable the "Save column with this layout" option on
		/// the Save Layouts and Manage Layouts forms and prevent layouts being saved.
		/// </summary>
		protected virtual bool CanSaveColumnLayouts
		{
			get { return true; }
		}

		/// <summary>
		/// Overriding this property will disable the "Save grid colour with this layout" option on
		/// the Save Layouts and Manage Layouts forms and prevent grid colours being saved.
		/// </summary>
		protected virtual bool CanSaveGridColours
		{
			get { return true; }
		}

		protected virtual SaveLayoutUserQueryHandler GetQueryHander()
		{
			return new SaveLayoutUserQueryHandler();
		}

		#region Adding Filter to Drop List

		protected void AddOrUpdateExistingFindDropListItem(StmModuleFilter filter)
		{
			var existingItem = GetItemFromFindDropButton(ToolStripFindDropButton.DropDownItems, filter.PK);
			if (existingItem != null)
			{
				existingItem.Text = filter.DisplayName.ToString().Split('/').Last();
			}
			else
			{
				AddItemToFindDropList(filter);
			}
		}

		ToolStripMenuItem GetItemFromFindDropButton(ToolStripItemCollection dropDownItems, ZGuid pk)
		{
			foreach (ToolStripMenuItem item in dropDownItems)
			{
				var itemFilter = item.Tag as StmModuleFilter;
				if (itemFilter != null && itemFilter.PK == pk)
				{
					return item;
				}
				if (item.DropDownItems.Count > 0)
				{
					var result = GetItemFromFindDropButton(item.DropDownItems, pk);
					if (result != null)
					{
						return result;
					}
				}
			}

			return null;
		}

		public event EventHandler<ModuleFilterToApplyEventArgs> FindButtonTextChanging;

		void OnFindButtonTextChanging(StmModuleFilter filter)
		{
			FindButtonTextChanging?.Invoke(null, new ModuleFilterToApplyEventArgs(filter));
		}

		protected void UpdateFindButtonText(StmModuleFilter filter, bool allowLoad = true)
		{
			OnFindButtonTextChanging(filter);

			ToolStripFindDropButton.Tag = filter;

			var filterName = filter != null && !filter.S9_FilterNameMultilingual.IsEmpty ?
				string.Format(CultureInfo.InvariantCulture, "{0}({1})", NonEmptyFindButtonText, filter.S9_FilterNameMultilingual)
				: EmptyFindButtonText;

			TrimLongFilterName(filterName);

			UpdateDropDownStyle(filter);

			if (allowLoad)
			{
				if (RelatedGrid != null)
				{
					if (filter != null)
					{
						RelatedGrid.CurrentColumnLayout = filter.S9_SaveColumnLayout ? filter : null;
						RelatedGrid.LoadUserLayoutSettings();
					}
					else
					{
						RelatedGrid.CurrentColumnLayout = null;
					}
				}

				UpdateToolStripLayout(Strips);
			}
		}

		void UpdateDropDownStyle(StmModuleFilter filter)
		{
			if (ToolStripFindDropButton.DropDownItems.Count == 0)
			{
				return;
			}

			var owner = ToolStripFindDropButton.DropDownItems[0].Owner;

			try
			{
				owner.SuspendLayout();

				foreach (ToolStripItem item in ToolStripFindDropButton.DropDownItems)
				{
					if (item is ZFilterToolStripMenuItem filterToolStripMenuItem && filterToolStripMenuItem.IsCategoryName)
					{
						item.Text = item.Text.PadRight(item.Text.Trim().Length + 20); // just padding in order to have the menu drop down a little wider
						item.Enabled = false;
						item.Font = CategoryNameFindListItemFont;
					}
					else
					{
						item.Text = item.Text.PadLeft(item.Text.Trim().Length + 4); // some padding here in order to slightly indent the text to the right at the first level
						item.Enabled = true;
						var candidateFont = IsThisItemOrOneOfItsChildrenSelected(filter, item) ? SelectedFindListItemFont : UnselectedFindListItemFont;
						if (candidateFont != item.Font)
						{
							item.Font = candidateFont;
						}
					}
				}
			}
			finally
			{
				owner.ResumeLayout();
			}
		}

		bool IsThisItemOrOneOfItsChildrenSelected(StmModuleFilter filter, ToolStripItem item)
		{
			var result = false;

			item.Font = UnselectedFindListItemFont;

			if (filter != null && item.Tag != null && ((StmModuleFilter)item.Tag).PK == filter.PK)
			{
				item.Font = SelectedFindListItemFont;
				result = true;
			}

			foreach (ToolStripItem dropDownItem in ((ZToolStripMenuItem)item).DropDownItems)
			{
				if (IsThisItemOrOneOfItsChildrenSelected(filter, dropDownItem))
				{
					dropDownItem.Font = SelectedFindListItemFont;
					result = true;
				}
			}

			return result;
		}

		void TrimLongFilterName(ZString filterName)
		{
			var intersected = false;
			const string continuation = "...";
			ToolStripFindDropButton.Text = filterName;

			while (ToolStripFindDropButton.Bounds.IntersectsWith(ToolStripAddGroupButton.Bounds))
			{
				intersected = true;
				if (filterName.Length <= 1)
				{
					break;
				}
				filterName = filterName.Substring(0, filterName.Length - 1);
				ToolStripFindDropButton.Text = filterName;
			}

			if (intersected)
			{
				ToolStripFindDropButton.Text = filterName.Substring(0, Math.Max(0, filterName.Length - 4)) + continuation;
			}
		}

		public static Font UnselectedFindListItemFont
		{
			get { return fUnselectedFindListItemFont ?? (fUnselectedFindListItemFont = new Font(OFont.NormalFontName, 8.25F)); }
		}

		Font SelectedFindListItemFont
		{
			get { return fSelectedFindListItemFont ?? (fSelectedFindListItemFont = new Font(OFont.NormalFontName, 8.25F, FontStyle.Bold)); }
		}

		[ThreadStatic]
		static Font fUnselectedFindListItemFont;
		[ThreadStatic]
		static Font fSelectedFindListItemFont;
		Font CategoryNameFindListItemFont
		{
			get { return fCategoryNameFindListItemFont ?? (fCategoryNameFindListItemFont = new Font(ToolStripFindDropButton.Font.FontFamily, ToolStripFindDropButton.Font.Size + 1, FontStyle.Bold)); }
		}
		Font fCategoryNameFindListItemFont;

		void ToolStripFindDropButton_TextChanged(object sender, EventArgs e)
		{
			ResumeLayout(true); // force resizing of the toolstrip control (ToolStrip.ResumeLayout(true) doesn't work)
		}

		#endregion

		#endregion

		#region Reset

		[SuppressMessage("CargoWiseOne", "CW1084:DoNotUseNewVirtual", Justification = "Baseline")]
		public new virtual void ResetFilterStrips()
		{
			ResetFindButton(); // this must be before SuspendLayout because we want the width updated immediately
			base.ResetFilterStrips();
		}

		#endregion

		#region Manage

		protected override void ManageLayouts(bool shouldShowUserDefinedFilter)
		{
			var form = GetManageLayoutsForm(shouldShowUserDefinedFilter);
			form.UserDefinedFiltersChanged += OnUserDefinedFiltersChanged;
			ZFormModaliser.Show(form, FindForm());
		}

		void OnUserDefinedFiltersChanged(object sender, EventArgs e)
		{
			((IFilterStripBusinessObjectInternals)FilterBusinessObject).ClearLayoutsCache();
			UpdateUserDefinedFilters();

			foreach (var strip in FilterBusinessObject.FilterStrips.Cast<FilterStrip>())
			{
				strip.Validation.ValidateFilterDescription();
			}

			if (ToolStripFindDropButton.DropDownItems.Cast<ToolStripItem>().All(d => d is ZFilterToolStripMenuItem item && item.IsCategoryName))
			{
				ToolStripFindDropButton.DropDownItems.Clear();
				ToolStripFindDropButton.DropDownItems.AddRange(new ToolStripItem[] { ToolStripNoLayoutsAddedMenuItem });
			}

			RemoveFilterCollectionFindBoxForDeletedFilters();
		}

		void UpdateUserDefinedFilters()
		{
			FilterBusinessObject.RefreshUserDefinedFilters();
			FilterBusinessObject.ModuleFilters.ResetFilterList();
			foreach (var strip in FilterStripsPanel.FindAll<ZFilterStrip>())
			{
				strip.FilterDescriptionDropEdit.InvalidateList();
			}
		}

		protected ManageLayoutsForm GetManageLayoutsForm(bool shouldShowUserDefinedFilter)
		{
			return GetManageLayoutsForm(FilterBusinessObject, CanSaveColumnLayouts, CanSaveGridColours, shouldShowUserDefinedFilter, RelatedGrid?.GetLastUsedColourSchemeForCurrentUser);
		}

		protected virtual ManageLayoutsForm GetManageLayoutsForm(FilterStripBusinessObject filterBusinessObject, bool canSaveColumnLayouts, bool canSaveGridColours, bool shouldShowUserDefinedFilter, GridColourScheme colourScheme = null)
		{
			return new ManageLayoutsForm(filterBusinessObject, canSaveColumnLayouts, canSaveGridColours, shouldShowUserDefinedFilter, colourScheme);
		}

		#endregion

		#region Find

		void ToolStripFindDropButton_ButtonClick(object sender, EventArgs e)
		{
			Find();
		}

		public virtual void Find(bool isManualSearch = true)
		{
			try
			{
				FirePerformSearch(Visible, isManualSearch);
			}
			finally
			{
				ToolStrip.Enabled = true;
			}
		}

		public virtual void ToolStripFindDropButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
		}

		protected override void ItemClicked(object sender, EventArgs e)
		{
			if (!(sender is ZFilterToolStripMenuItem clickedItem && clickedItem.Owner != null && clickedItem.IsOnImage))
			{
				HandleFindButtonDropDownItemClick((ToolStripItem)sender);
				OnFireValidationRequest();
			}
			else
			{
				try
				{
					FilterBusinessObject.AddOrRemoveFavoriteFilter((StmModuleFilter)clickedItem.Tag);
					ReloadFindDropList();
					UpdateDropDownStyle((StmModuleFilter)ToolStripFindDropButton.Tag); // need to put the currently selected filter here not the one being added/removed to/from favorites
				}
				catch (InvalidOperationException ex)
				{
					Globals.Message.ShowWarning(ex.Message);
				}
				ToolStripFindDropButton.ShowDropDown();
			}
		}

		protected void HandleFindButtonDropDownItemClick(ToolStripItem item)
		{
			ToolStripFindDropButton.HideDropDown();
			ParentForm?.Refresh(); // force drop menu to go away while we do the Find operation..

			if (ShouldPerformSearch())
			{
				SaveCurrentGridLayoutIfRequired();
				SelectUserFilter((StmModuleFilter)item.Tag, clearCollectionResults: ShouldClearCollectionResultsOnFindButtonDropDownItemClicked, isManualSearch: true);
			}
		}

		protected virtual bool ShouldClearCollectionResultsOnFindButtonDropDownItemClicked { get { return false; } }

		public event EventHandler FireValidationRequest;
		protected void OnFireValidationRequest()
		{
			FireValidationRequest?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// If a current layout is stored in StmData, but users have chosen a filter layout with Save Column Layout ticked
		/// then the current layout should be saved in StmData if changed.
		/// </summary>
		void SaveCurrentGridLayoutIfRequired()
		{
			if (RelatedGrid != null && RelatedGrid.Visible && RelatedGrid.CurrentColumnLayout != null && !RelatedGrid.CurrentColumnLayout.IsDeleted)
			{
				if (StmDataGridLayoutStorage.IsDefaultLayout(RelatedGrid.CurrentColumnLayout.ColumnLayoutName))
				{
					RelatedGrid.SaveUserLayoutSettings();
				}
			}
		}

		bool FirstLayoutLoad
		{
			get
			{
				var result = firstLayoutLoad;
				firstLayoutLoad = false;
				return result;
			}
		}
		bool firstLayoutLoad = true;

		void SelectUserFilter(StmModuleFilter filter, bool clearCollectionResults, bool isManualSearch)
		{
			if (FilterBusinessObject.LoadLayout(filter, FirstLayoutLoad))
			{
				UpdateFindButtonText(filter);
			}
			else
			{
				UpdateFindButtonText(null);
			}

			if (clearCollectionResults)
			{
				ClearGridResults();
			}
			else
			{
				Find(isManualSearch);
			}
		}

		protected void ClearGridResults()
		{
			if (RelatedGrid != null)
			{
				var legacyCollection = GridCollection as BusinessObjectCollection;
				legacyCollection?.RemoveAll();

				var activeCollection = GridCollection as IActiveBusinessObjectCollection;
				if (activeCollection != null)
				{
					activeCollection.AdditionalFilter = ZQuery.NoResultQuery;
				}
			}
		}

		protected override void LayoutLoaded()
		{
			RebuildFilterStrips();
		}

		protected override void FilterEdited()
		{
			ResetFindButton();
		}

		internal void ResetFindButton()
		{
			UpdateFindButtonText(null);
		}

		protected internal override void AddStrip()
		{
			base.AddStrip();
			ResetFindButton();
		}

		public bool IsCurrentFilterUserDefined
		{
			get { return CurrentLayout != null; }
		}

		public StmModuleFilter LayoutToLoad { get; set; }
		internal bool ShouldLoadLayoutEvenWhenUnsaved { get; set; }

		#endregion

		#endregion

		#region IResultCountHandler

		void IResultCountHandler.UpdateNumberLoadedMessage(ZString message, int numberOfRecordsFound, bool shouldShowNumberLoadedMessageBox)
		{
			UpdateNumberLoadedMessage(message, numberOfRecordsFound, shouldShowNumberLoadedMessageBox);
		}

		protected void UpdateNumberLoadedMessage(ZString message, int numberOfRecordsFound, bool shouldShowNumberLoadedMessageBox)
		{
			UpdateNumberLoadedMessageCore(message, numberOfRecordsFound, shouldShowNumberLoadedMessageBox);
		}

		protected virtual void UpdateNumberLoadedMessageCore(ZString message, int numberOfRecordsFound, bool shouldShowNumberLoadedMessageBox)
		{
			ShouldShowNumberLoadedMessageBox = shouldShowNumberLoadedMessageBox;
			NumberRecordsLoadedLabelText = message;

			SuspendLayout();
			try
			{
				ToolStripRecordsFoundLabel.ForeColor = Color.Blue;

				if (numberOfRecordsFound == 0)
				{
					ToolStripRecordsFoundLabel.Text = "    " + Res.GetString("FilterStripControl|FoundNoRecords", "Found\r\n  no records");
				}
				else if (numberOfRecordsFound == 1)
				{
					ToolStripRecordsFoundLabel.Text = "    " + Res.GetString("FilterStripControl|Found1Record", "Found\r\n  1 record");
				}
				else if (numberOfRecordsFound > MaximumAllowableQueriesPerSqlStatement)
				{
					ToolStripRecordsFoundLabel.ForeColor = Color.Red;
					ToolStripRecordsFoundLabel.Text = "     " + Res.GetString("FilterStripControl|FoundTooManyRecords", "Found\r\n  too many\r\n   records");
				}
				else
				{
					ToolStripRecordsFoundLabel.Text = "    " + Res.GetString("FilterStripControl|FoundGRecords", "Found\r\n{0:G} records", numberOfRecordsFound);
				}

				ToolStripRecordsFoundLabel.Text = ToolStripRecordsFoundLabel.Text.Replace("\\n", System.Environment.NewLine);

				UpdateToolStripRecordsFoundLabelTop();
			}
			finally
			{
				ResumeLayout();
			}
		}

		protected string NumberRecordsLoadedLabelText;
		protected bool ShouldShowNumberLoadedMessageBox;

		protected virtual int MaximumAllowableQueriesPerSqlStatement => SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;

		#endregion

		public event EventHandler<PerformSearchEventArgs> PerformSearch;
		public event EventHandler<PerformSearchAsyncEventArgs> PerformSearchAsync;
		public event EventHandler OnAfterPerformSearch;

		#region ModuleFilterToApplyEventArgs

		public class ModuleFilterToApplyEventArgs : EventArgs
		{
			public ModuleFilterToApplyEventArgs(StmModuleFilter moduleFilterToApply)
			{
				ModuleFilterToApply = moduleFilterToApply;
			}

			public readonly StmModuleFilter ModuleFilterToApply;
		}

		#endregion
	}
}
