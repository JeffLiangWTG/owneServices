using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.GUI.TileBar;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Globals = Enterprise.ZArchitecture.Environment.Globals;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class StripControl : ZEmbeddedModuleControl
	{
		int recentItemsPanelLocationOffsetY;
		[DpiState(DpiState.Unscaled)]
		public int RecentItemsPanelLocationOffsetY
		{
			get
			{
				return recentItemsPanelLocationOffsetY;
			}
			set
			{
				if (recentItemsPanelLocationOffsetY != value)
				{
					recentItemsPanelLocationOffsetY = value;
					UpdateRecentItemsPanelLocation(this, null);
					UpdateToolStripLayout(Strips);
				}
			}
		}

		public StripControl()
		{
			InitializeComponent();
		}

		public StripControl(FilterStripBusinessObject filterStrip)
		{
			InitializeComponent();

			SetupToolBarImages();
			SetupToolBarFont();
			FilterBusinessObject = filterStrip;
			HookButtonEvents();
			UpdateToolStripLayout(Strips);
			if (FilterBusinessObject != null)
			{
				FilterBusinessObject.LayoutLoaded += LayoutLoaded;
			}

			RecentItemsPanel.AllowOverlap(this);
			CoveringLabel.AllowOverlap(ToolStripColourPicker);
			ToolStripHelp.AllowOverlap(AddStripButton);
			ToolStrip.AllowOverlap(ToolStripHelp);
			ToolStrip.AllowOverlap(ToolStripColourPicker);
			ToolStripRecordsFoundLabel.AllowOverlap(FilterStripsPanel);

			ToolStripRecordsFoundLabel.AllowOutsideOfParent();
			ToolStripHelp.AllowOutsideOfParent();
			FilterStripsPanel.AllowOutsideOfParent();
		}

		public virtual FilterStripBusinessObject FilterBusinessObject { get; private set; }

		protected IEnumerable<Control> FilterStripControls => new Control[] { FilterStripsPanel };
		protected IEnumerable<Control> ToolStripControls => new Control[] { ToolStrip, ToolStripHelp, CoveringLabel, ToolStripColourPicker, ToolStripRecordsFoundLabel, AddStripButton };

		#region Recent Items

		internal void SetRecentItemsVisibility(bool visible)
		{
			RecentItemsPanel.Visible = visible;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Control name")]
		protected void SetupRecentItemsControl()
		{
			RecentItemsControl = new RecentItemsControl();
			RecentItemsControl.FontFamily = new System.Windows.Media.FontFamily(OFont.NormalFontName);
			RecentItemsControl.PanelBackgroundColor = SystemDataRegistry.Instance.ColorTheme.NavBarRecentPanelBackground;
			RecentItemsControl.PanelBorderColor = SystemDataRegistry.Instance.ColorTheme.NavBarButtonColor1;
			HostControl.Child = RecentItemsControl;

			RecentItemsPanel.BackColor = SystemDataRegistry.Instance.ColorTheme.ToolbarColor;
			RecentItemsLabel.Text = Res.GetString("fccaad77-566a-4cf9-98d2-af9d3ffbf132", "Recent Items");

			ParentChanged += (s, e) =>
			{
				UpdateRecentItemsPanelLocation(s, e);

				if (!sizeChangedAttached && ParentForm != null)
				{
					var mainFormToolbarStrip = (ZToolStrip)(ParentForm.Find(x => x.Name == "MainFormToolbarStrip").FirstOrDefault() ?? ParentForm.Find(x => x.Name == "ToolBarPanel").FirstOrDefault()?.Find(x => x.Name == "Toolstrip").FirstOrDefault());
					if (mainFormToolbarStrip != null)
					{
						mainFormToolbarStrip.LayoutCompleted += UpdateRecentItemsPanelLocation;
						if (ToolStripRecordsFoundLabel != null)
						{
							ToolStripRecordsFoundLabel.LocationChanged += UpdateRecentItemsPanelLocation;
						}
						sizeChangedAttached = true;
					}
				}
			};
		}
		bool sizeChangedAttached;

		int GetToolStripButtonsWidth(ZToolStrip toolStrip)
		{
			var width = 0;
			if (toolStrip is { Items.Count: > 0 })
			{
				width += toolStrip.Items.Cast<ToolStripItem>().Sum(item => item.Width);
			}
			return width;
		}

		public void UpdateRecentItemsPanelLocation(object sender, EventArgs e)
		{
			var parentForm = ParentForm;
			if (parentForm == null)
			{
				return;
			}
			//On popup windows it has a different, more generic name, so try to not accidentally get other things called "Toolstrip" by mistake.
			var mainFormToolbarStrip = parentForm.Find(x => x.Name == "MainFormToolbarStrip").FirstOrDefault() ?? parentForm.Find(x => x.Name == "ToolBarPanel").FirstOrDefault()?.Find(x => x.Name == "Toolstrip").FirstOrDefault();
			var controlToAddTo = parentForm is Forms.ZMainForm z ? z.Workspace : (Control)parentForm;
			var moduleHeadingLabel = parentForm.Find(x => x.Name == "ModuleHeadingLabel").FirstOrDefault();
			if (!controlToAddTo.Controls.Contains(RecentItemsPanel))
			{ 
				controlToAddTo.Controls.Add(RecentItemsPanel);
			}

			var type = parentForm.GetType().Name;

			//Ensure the recent items panel does not cover the items found result label:
			var recentItemsPanelX =
				Math.Max(FilterStripsPanel.Right,
					(ToolStripRecordsFoundLabel?.Left ?? 0) + (ToolStripRecordsFoundLabel?.MaximumSize.Width ?? 0));

			//If the toolbar is so huge that it starts to overlap the Recent Items box, push it back until it no longer is:
			recentItemsPanelX = Math.Max(recentItemsPanelX, GetToolStripButtonsWidth((ZToolStrip)mainFormToolbarStrip) + (moduleHeadingLabel?.Right ?? 0));

			var scalePositionY = ControlDpiScalingHelper.ScaleToCurrentDpiY(RecentItemsPanelLocationOffsetY + 4);
			if (type.Contains("EmbeddedModulePopup")) // when opening in a popup window
			{
				ControlDpiScalingHelper.SetWidth(ref FilterStripsPanel, 700, true);
				RecentItemsPanel.Location = ControlDpiScalingHelper.NewScaledPoint(recentItemsPanelX, scalePositionY, false);
			}
			else
			{
				if (parentForm.Width >= parentForm.MinimumSize.Width)
				{
					RecentItemsPanel.Location = ControlDpiScalingHelper.NewScaledPoint(recentItemsPanelX, scalePositionY, false);
				}
				else if (!sizeChangedAttached)
				{
					// reduce the width of FilterStripsPanel when the resolution is lower than 1366 * 768
					// offset is set to 100 (800-100) when displayed under minimum resolution 1024 * 768
					var largestGap = parentForm.MinimumSize.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(1040);
					var offset = (parentForm.MinimumSize.Width - parentForm.Width) * 100 / largestGap;
					ControlDpiScalingHelper.SetWidth(ref FilterStripsPanel, ControlDpiScalingHelper.ScaleToCurrentDpiX(800) - offset, false);
					RecentItemsPanel.Location = ControlDpiScalingHelper.NewScaledPoint(recentItemsPanelX + ControlDpiScalingHelper.ScaleToCurrentDpiX(70), scalePositionY, false);
				}
			}
			ControlDpiScalingHelper.SetWidth(ref RecentItemsPanel, parentForm.Width - RecentItemsPanel.Location.X - ControlDpiScalingHelper.ScaleToCurrentDpiY(24), false);
			RecentItemsPanel.BringToFront();
		}

		public bool IsContainRecentItemPanel(Control panel)
		{
			return this.RecentItemsPanel != null && RecentItemsPanel == panel;
		}

		#endregion

		#region Clear

		protected virtual void PersistCurrentControlValue()
		{
			var currentControl = this.GetFrontMostActiveControl();
			if (currentControl != null)
			{
				FindForm().SelectNextControlNonTabStopNonReadOnly(currentControl, true, true, true);
				currentControl.Focus();
			}
		}

		protected virtual void ClearStrips()
		{
			Clear();
		}

#if DEBUG
		internal void ClearStrips_Exposed()
		{
			ClearStrips();
		}
#endif

		public event EventHandler FiltersCleared;

		void Clear()
		{
			foreach (var strip in Strips)
			{
				strip.Clear();
			}

			if (FiltersCleared != null)
			{
				FiltersCleared(this, EventArgs.Empty);
			}
		}

		#endregion

		#region FilterStrip

		protected internal virtual GroupStripControl NewGroupStripControl()
		{
			var groupControl = GetNewGroupStripControl();
			return groupControl;
		}

		protected virtual GroupStripControl GetNewGroupStripControl()
		{
			return new GroupStripControl();
		}

		bool suspendNewFilterStrips;

		protected void RebuildFilterStrips()
		{
			if (FilterBusinessObject.FilterStrips.Count == 0)
			{
				ResetFilterStrips(); // something went wrong deserializing, at least make one strip available
			}
			else
			{
				SuspendDrawing();
				SuspendLayout();
				foreach (var strip in Strips)
				{
					strip.SuspendLayout();
				}
				suspendNewFilterStrips = true;
				try
				{
					RemoveAllFilterStrips();
					RemoveAllGroupStripControls();
					GroupStrips.Clear();
					BuildStripGroupsIfNeeded();
					foreach (FilterStrip bizO in FilterBusinessObject.FilterStrips)
					{
						AddFilterStrip(bizO);
					}
				}
				finally
				{
					suspendNewFilterStrips = false;
					foreach (var strip in Strips)
					{
						strip.ResumeLayout();
					}
					ResumeLayout();
					ResumeDrawing();
				}
			}
		}

		protected void BuildStripGroupsIfNeeded()
		{
			var groupNames = FilterBusinessObject.FilterStrips.Select(strip => strip.GroupName).Distinct();
			if (groupNames.Count() > 1)
			{
				foreach (var name in groupNames)
				{
					CurrentGroupName = name;
				}
			}
		}

		#region Reset

		void MoveFocusToLastFilterStrip()
		{
			LastStrip?.Focus();
		}

		public Type LastFilterStripType
		{
			get { return LastStrip?.GetType(); }
		}

		void DeleteFilterStripWithoutRefreshingLayout(ZFilterStrip strip)
		{
			if (strip.CurrentDataItem != null)
			{
				if (strip.CurrentDataItem.ParentCollections.Count > 1)
				{
					throw new ArgumentException("A FilterStrip object exists in more than one collection.");
				}
				else if (strip.CurrentDataItem.ParentCollections.Count == 1)
				{
					strip.CurrentDataItem.ParentCollections.First().RemoveAndDelete(strip.CurrentDataItem);
					DeleteStripFromGroupStrips(strip);
				}
				else
				{
					strip.CurrentDataItem.Delete(); // why does this case exist?
				}
			}

			strip.SetDataBinding(null, "");
			Strips.Remove(strip);
			strip.Dispose();
		}

		internal void DeleteFilterStrip(ZFilterStrip strip)
		{
			var groupName = strip.CurrentDataItem?.GroupName ?? string.Empty;
			DeleteFilterStripWithoutRefreshingLayout(strip);

			if (GroupStrips.Count > 0)
			{
				RefreshStripLayoutWithinGroup(groupName);
				RefreshGroupLayout();
			}
			else
			{
				RefreshStripLayout();
			}

			OnFilterStripDeleted();
			OnFilterStripAddedOrRemoved();
		}

		protected virtual void OnFilterStripDeleted()
		{
		}

		internal void DeleteGroupFilterStrip(GroupStripControl group)
		{
			if (Strips.Count > 0 && GroupControls.Count > 1)
			{
				var strips = GetStripsForGroup(group.GroupName).ToList();

				while (strips.Count > 0)
				{
					DeleteFilterStripWithoutRefreshingLayout(strips[0]);
					strips.Remove(strips[0]);
				}
			}

			GroupStrips.Remove(group.GroupName);
			GroupControls.Remove(group);
			group.Dispose();

			RefreshGroupLayout();
			OnFilterStripAddedOrRemoved();
		}

		// this is called after the vertical scrollbar is displayed or hidden
		void OnFilterStripAddedOrRemoved()
		{
			if (FilterStripsPanel.VerticalScroll.Visible)
			{
				ControlDpiScalingHelper.SetLeft(ref ToolStripRecordsFoundLabel, FilterStripsPanel.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(2), false);
			}
			else
			{
				ControlDpiScalingHelper.SetLeft(ref ToolStripRecordsFoundLabel, FilterStripsPanel.Right - ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
			}
		}

		#region Add New Strip

		protected internal virtual ZFilterStrip NewZFilterStrip()
			=> new ZFilterStrip();

		protected internal virtual void AddStrip()
		{
			NewFilter = true;
			AddNewFilterStrip();
			MoveFocusToLastFilterStrip();
		}

		/// <summary>
		/// Adds a new ZFilterStrip into the user interface.
		/// 
		/// This is the control that has a dropdown on the left and data entry fields on the right.
		///
		/// To have this new ZFilterStrip have a pre-selected dropdown value, set its CurrentDataItem.FilterDescription
		/// to be the description of the ModuleFilter that you want it to have.
		///
		/// To give the new ZFilterStrip have a value in the data entry field, you have to set that via the
		/// respective ModuleFilter instance. You can fetch the ModuleFilter instance from the FilterStripBusinessObject[somecode]
		/// The above only works if you have 1x copy of that ModuleFilter.
		///
		/// If you have more than one copy of the ModuleFilter then you need to listen into the
		/// FilterStripBusinessObject's ModuleFilterIsActive event to configure it's default.
		/// </summary>
		public ZFilterStrip AddNewFilterStrip()
			=> AddFilterStrip(FilterBusinessObject.FilterStrips.AddNew());

		public ZFilterStrip AddFilterStrip(FilterStrip bizO)
		{
			var strip = NewZFilterStrip();
			if (suspendNewFilterStrips)
			{
				strip.SuspendLayout();
			}
			AddFilterStrip(strip, bizO);
			return strip;
		}

		protected virtual void AddFilterStrip(ZFilterStrip strip)
		{
			AddFilterStrip(strip, FilterBusinessObject.FilterStrips.AddNew());
		}

		protected void AddFilterStrip(ZFilterStrip strip, FilterStrip bizO)
		{
			GroupStripControl groupStripControl;
			if (NewFilter && GroupControls.Count > 0)
			{
				groupStripControl = GroupControls.Last();
				bizO.GroupName = groupStripControl.GroupName;
				bizO.GroupOrCategory = groupStripControl.GroupOrCategory;
			}
			else
			{
				groupStripControl = GroupControls.LastOrDefault(group => group.GroupName == bizO.GroupName);
			}

			AddFilterStrip(strip, bizO, groupStripControl);
		}

		protected internal void AddFilterStrip(ZFilterStrip strip, FilterStrip bizO, GroupStripControl groupStripControl)
		{
			OnFilterStripAdding(strip);

			if (Parent != null && !Parent.Visible)
			{
				Parent.Visible = true; // hack - controls won't show without this.
			}

			currentGroupName = groupStripControl?.GroupName ?? bizO.GroupName;
			AddFilterStripCore(strip, bizO, groupStripControl);
		}

		protected virtual void AddFilterStripCore(ZFilterStrip strip, FilterStrip bizO, GroupStripControl groupStripControl)
		{
			if (Parent != null && !Parent.Visible)
			{
				Parent.Visible = true; // hack - controls won't show without this.
			}

			strip.RelatedBizoPK = bizO.PK;

			strip.FilterEdited += delegate
			{ FilterStripEdited(); };
			strip.FilterOrCategoryChanged += delegate
			{ FilterStripEdited(); };

			strip.FilterEdited += delegate
			{ FilterEdited(); };
			strip.HeightChanged += delegate
			{ PerformLayout(); };
			strip.Location = NextStripLocation;

			Strips.Add(strip);
			UpdateGroupStrip(strip, CurrentGroupName);
			NewFilter = false;

			if (GroupControls.Count > 0)
			{
				if (groupStripControl != null)
				{
					if (bizO.GroupName.IsEmpty && !CurrentGroupName.IsEmpty)
					{
						bizO.GroupName = groupStripControl.GroupName;
						bizO.GroupOrCategory = groupStripControl.GroupOrCategory;
						groupStripControl.FilterStripGroupBox.Controls.Add(strip);
						RefreshStripLayoutWithinGroup(groupStripControl.GroupName);
					}
					else
					{
						RefreshStripLayoutWithinGroup(groupStripControl.GroupName);
						groupStripControl.FilterStripGroupBox.Controls.Add(strip);
						if (bizO.CurrentModuleFilter != null)
						{
							groupStripControl.GroupOrCategory = bizO.GroupOrCategory;
							groupStripControl.AdditionalGroupColourName = bizO.AdditionalGroupColourName;
						}
					}
					groupStripControl.UpdateFilterControlColorsForOrCategory();
				}
				RefreshGroupLayout(); // must do this again before adding the strip to the panel, otherwise groups will not be aligned properly after adding the last ZFilterStrip
			}
			else
			{
				RefreshStripLayout(); // must do this before adding the strip to the panel, otherwise the panel will attempt to show the scrollbar (flickers)
				FilterStripsPanel.Controls.Add(strip);
			}

			strip.SetDataBinding(bizO, "");
			OnFilterStripAddedOrRemoved();
		}

		void OnFilterStripAdding(ZFilterStrip strip)
		{
			FilterStripAdding?.Invoke(this, new ZFilterStripEventArgs(strip));
		}
		public event EventHandler<ZFilterStripEventArgs> FilterStripAdding;

		#endregion

		public void AddGroupFilterControl(string groupName = "")
		{
			SuspendLayout();

			try
			{
				if (Parent != null && !Parent.Visible)
				{
					Parent.Visible = true; // hack - controls won't show without this.
				}

				var groupControl = NewGroupStripControl();
				groupControl.GroupName = groupName;
				if (!string.IsNullOrEmpty(groupName))
				{
					groupControl.GroupNameLabel.Text = groupName;
				}
				groupControl.AutoSize = true;
				groupControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
				groupControl.FilterEdited += delegate
				{ FilterEdited(); };
				groupControl.Location = NextGroupControlLocation;
				groupControl.UpdateFilterControlColorsForOrCategory();

				GroupControls.Add(groupControl);
				UpdateGroupStrip(null, groupName);

				if (string.IsNullOrEmpty(groupName))
				{
					AddExistingFilterStripsToGroupStripControl();
				}
				else
				{
					RefreshGroupLayout();   //	must do this before adding the group control to the panel, otherwise the panel will attempt to show the scrollbar (flickers)
				}

				if (!FilterBusinessObject.FilterStrips.Any(strip => ((FilterStrip)strip).GroupName.IsEmpty) && !NewFilter && Strips.Count == 0 && string.IsNullOrEmpty(groupName))
				{
					DeleteGroupFilterStrip(groupControl);
					RefreshGroupLayout();
				}
				else
				{
					FilterStripsPanel.Controls.Add(groupControl);
				}

				OnFilterStripAddedOrRemoved();
			}
			finally
			{
				ResumeLayout();
			}
		}

		protected virtual void ResetFilterStrips()
		{
			SuspendDrawing();
			SuspendLayout();
			try
			{
				RemoveAllFilterStrips();
				RemoveAllGroupStripControls();
				AddAlwaysVisibleFilterStrips();
				if (ShouldAddEmptyFilterStripOnReset)
				{
					AddNewFilterStrip();
				}
				RefreshStripLayout();
			}
			finally
			{
				ResumeLayout();
				ResumeDrawing();
			}
		}

		protected virtual void AddAlwaysVisibleFilterStrips()
		{
		}

		protected virtual bool ShouldAddEmptyFilterStripOnReset
		{
			get { return true; }
		}

		void RemoveAllFilterStrips()
		{
			while (Strips.Count > 0)
			{
				DeleteFilterStripWithoutRefreshingLayout(Strips[0]);
			}
		}

		void AddExistingFilterStripsToGroupStripControl()
		{
			var originalHeight = FilterStripsPanel.Height;

			for (var i = 0; i < Strips.Count; i++)
			{
				Strips[i].Parent = (GroupControls[GroupControls.Count - 1]).FilterStripGroupBox;
				Strips[i].SetFilterDescriptionDropEditLocation();
				UpdateGroupStrip(Strips[i], "");
			}

			ControlDpiScalingHelper.SetHeight(ref FilterStripsPanel, originalHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(ToolStripGap), false);
		}

		void RemoveAllGroupStripControls()
		{
			while (GroupControls.Count > 0)
			{
				DeleteGroupFilterStrip(GroupControls[0]);
			}
		}

		#endregion

		internal
#if DEBUG
	protected virtual
#endif
	void InitializeStrips()
		{
			if (FilterBusinessObject != null)
			{
				var hasLayoutsOrLoadStripsWhenNoLayouts = (FilterBusinessObject.Layouts.Count > 0 || ShouldLoadStripsWhenNoLayoutsLoaded);

				if (ShouldInvokeFilterStrips && hasLayoutsOrLoadStripsWhenNoLayouts)
				{
					LoadFilterStrips(doInvoke: true);
				}
				else
				{
					ResetFilterStrips();
					UpdateToolStripLayout(Strips);

					if (!ShouldInvokeFilterStrips && hasLayoutsOrLoadStripsWhenNoLayouts)
					{
						LoadFilterStrips(doInvoke: false);
					}

					if (OnStripsInitialized != null)
					{
						OnStripsInitialized(this, null);
					}
				}
			}
		}

		internal bool ShouldLoadStripsWhenNoLayoutsLoaded { get; set; }

		protected virtual bool ShouldInvokeFilterStrips => true;

		#endregion

		#region Group Properties

		internal bool GroupExists(string grpName)
		{
			return GetStripsForGroup(grpName).Count > 0;
		}

		public ZString CurrentGroupName
		{
			get { return currentGroupName; }
			set
			{
				currentGroupName = value;

				if (GroupControls.Count == 0)
				{
					AddGroupFilterControl();
				}
				if (!string.IsNullOrEmpty(currentGroupName))
				{
					AddGroupFilterControl(currentGroupName);
				}
			}
		}

		ZString currentGroupName;

		#endregion

		#region Layout

		void LayoutLoaded(object sender, EventArgs e)
		{
			LayoutLoaded();
		}

		protected virtual void LayoutLoaded()
		{
			RebuildFilterStrips();
		}

		protected virtual int GetStripControlWidth(IStripControl control)
		{
			if (control is ZFilterStrip filter)
			{
				return filter.Width;
			}
			return Width;
		}

		protected virtual void UpdateAdditionalToolStripLayout()
		{
		}

		internal const int MaxWorkWidth = 619;
		internal const int PanelGapWidth = 20;

		void StripControl_Resize(object sender, EventArgs e)
		{
			StripControl_ResizeCore();
		}

		protected virtual void StripControl_ResizeCore()
		{
		}

		protected virtual void SaveLayout()
		{
		}

		protected virtual void ManageLayouts(bool shouldShowUserDefinedFilter)
		{
		}

		protected virtual void ResetLayout()
		{
			QueryUserAndResetStrips();
		}

		protected virtual void AddGroup()
		{
			AddNewGroup();
		}

		protected void HideAllButResetAndAddGroupToolStripButtons()
		{
			ToolStripHelp.Items.Remove(this.ToolStripHelpButton);
			ToolStripHelp.Items.Remove(this.ToolStripManageDropButton);
			ToolStripHelp.Items.Remove(this.ToolStripSaveLayoutButton);
		}

		#endregion

		#region Implementation

		protected virtual void UnhookEventsOnFilterBizO()
		{
			if (FilterBusinessObject != null)
			{
				FilterBusinessObject.LayoutLoaded -= LayoutLoaded;
			}
		}

		protected virtual void UnhookEventsOnParentForm()
		{
			if (!sizeChangedAttached || ParentForm == null)
			{
				return;
			}
			var mainFormToolbarStrip =
				(ZToolStrip)(ParentForm.Find(x => x.Name == "MainFormToolbarStrip").FirstOrDefault() ?? ParentForm
					.Find(x => x.Name == "ToolBarPanel").FirstOrDefault()?.Find(x => x.Name == "Toolstrip")
					.FirstOrDefault());
			if (mainFormToolbarStrip != null)
			{
				mainFormToolbarStrip.LayoutCompleted -= UpdateRecentItemsPanelLocation;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
				base.OnPaint(e);
			}
			catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
		}

		void RemoveRecentPanelControl()
		{
			if (RecentItemsPanel.Parent != null)
			{
				RecentItemsPanel.Parent.Controls.Remove(RecentItemsPanel);
			}

			RecentItemsPanel.Dispose();
		}

		protected ZString LastSavedFilterName;
		protected static string EmptyFindButtonText => Res.GetString("FilterStrip|ToolStrip|ButtonFind", "Find");
		protected static string NonEmptyFindButtonText => EmptyFindButtonText + " ";
		protected static string CancelSearchFindButtonText => Res.GetString("FilterStrip|ToolStrip|ButtonCancelSearch", "Cancel Search");

		public void OnLoad_Exposed()
		{
			OnLoad(new EventArgs());
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!Enterprise.ZArchitecture.Core.DesignModeFinder.IsDesigning)
			{
				Bind();

				if (!AlreadyLoaded)
				{
					AlreadyLoaded = true;
					InitializeStrips();
				}
			}
		}

		internal protected virtual void AddItemToFindDropList(StmModuleFilter filter)
		{
			ToolStripFindDropButton.DropDownItems.Remove(ToolStripNoLayoutsAddedMenuItem);
			EnsureMenuCategoryCreated(filter);
			var tree = FilterLayoutsHelper.InsertInSubTree(ToolStripFindDropButton.DropDownItems?.Cast<ZFilterToolStripMenuItem>().ToList(), filter, new Stack<string>(filter.S9_FilterName.ToString().Split('/').Reverse()), false, ItemClicked).ToArray();
			ToolStripFindDropButton.DropDownItems.AddRange(tree);
		}

		void EnsureMenuCategoryCreated(StmModuleFilter filter)
		{
			var firstLevelItems = ToolStripFindDropButton.DropDownItems.Cast<ToolStripItem>().ToList();
			if (filter.S9_IsPublished && !firstLevelItems.Any(x => x.Text.StartsWith(FilterLayoutsHelper.SharedFilterLayoutsText, StringComparison.CurrentCulture)))
			{
				ToolStripFindDropButton.DropDownItems.Add(new ZFilterToolStripMenuItem(FilterLayoutsHelper.SharedFilterLayoutsText, true) { IsCategoryName = true, Enabled = false });
			}
			else if (!filter.S9_IsPublished && !firstLevelItems.Any(x => x.Text.StartsWith(FilterLayoutsHelper.MyFilterLayoutsText, StringComparison.CurrentCulture)))
			{
				ToolStripFindDropButton.DropDownItems.Insert(0, new ZFilterToolStripMenuItem(FilterLayoutsHelper.MyFilterLayoutsText) { IsCategoryName = true, Enabled = false });
			}
		}

		#region For Test
#if DEBUG
		public ZToolStripSplitButton ToolStripFindDropButton_ForTest => ToolStripFindDropButton;
		internal ZToolStripButton ToolStripClearButton_ForTest => ToolStripClearButton;
		internal ZToolStripButton ToolStripSaveLayoutButton_ForTest => ToolStripSaveLayoutButton;
		internal ZToolStripSplitButton ToolStripManageDropButton_ForTest => ToolStripManageDropButton;
		public bool AlreadyLoadedForTest => AlreadyLoaded;
#endif
		#endregion

		protected event EventHandler OnStripsInitialized;
		internal event EventHandler OnFilterStripLoaded;
		protected bool AlreadyLoaded;
		protected readonly List<ZFilterStrip> Strips = new List<ZFilterStrip>();
		protected readonly List<GroupStripControl> GroupControls = new List<GroupStripControl>();
		protected readonly Dictionary<string, List<ZFilterStrip>> GroupStrips = new Dictionary<string, List<ZFilterStrip>>();
		protected bool NewFilter;

		protected virtual void Bind()
		{
		}

		protected virtual void UpdateFilteredGridRefreshWarning()
		{
		}

		protected virtual ZString UpdateNoteURL
		{
			get { return "http://www.cargowise.com/"; }
		}

		void LoadFilterStrips(bool doInvoke = true)
		{
			try
			{
				SuspendLayout();
				ToolStripFindDropButton.DropDown.SuspendDrawing();
				ToolStripFindDropButton.DropDown.SuspendLayout();

				LoadFindDropList();

				if (doInvoke)
				{
					OnFilterStripLoaded?.Invoke(this, null);
				}
			}
			finally
			{
				ToolStripFindDropButton.DropDown.ResumeLayout(false);
				ToolStripFindDropButton.DropDown.ResumeDrawing(false);
				ResumeLayout(true);
			}
		}

		void LoadFindDropList()
		{
			var favoriteFilters = FilterBusinessObject.FavoriteLayouts;
			var privateFilters = FilterBusinessObject.Layouts_UnpublishedOnly.Where(filter => !favoriteFilters.Contains(filter));
			var publishedFilters = FilterBusinessObject.Layouts_PublishedOnly.Where(filter => !favoriteFilters.Contains(filter));

			if (privateFilters.Any() || publishedFilters.Any() || favoriteFilters.Any())
			{
				ToolStripFindDropButton.DropDownItems.Remove(ToolStripNoLayoutsAddedMenuItem);
			}

			//Using AddRange instead of AddItemToDropList to prevent doing multiple insertion sorts
			if (favoriteFilters.Any())
			{
				var menuItemsFavorites = FilterLayoutsHelper.CalculateTree(favoriteFilters, true, ItemClicked).ToArray();
				ToolStripFindDropButton.DropDownItems.Add(new ZFilterToolStripMenuItem(FilterLayoutsHelper.FavoriteFilterLayoutsText) { IsCategoryName = true, Enabled = false });
				ToolStripFindDropButton.DropDownItems.AddRange(menuItemsFavorites);
			}
			if (privateFilters.Any())
			{
				var menuItemsPrivate = FilterLayoutsHelper.CalculateTree(privateFilters, false, ItemClicked).ToArray();
				ToolStripFindDropButton.DropDownItems.Add(new ZFilterToolStripMenuItem(FilterLayoutsHelper.MyFilterLayoutsText) { IsCategoryName = true, Enabled = false });
				ToolStripFindDropButton.DropDownItems.AddRange(menuItemsPrivate);
			}
			if (publishedFilters.Any())
			{
				var menuItemsPublished = FilterLayoutsHelper.CalculateTree(publishedFilters, false, ItemClicked).ToArray();
				ToolStripFindDropButton.DropDownItems.Add(new ZFilterToolStripMenuItem(FilterLayoutsHelper.SharedFilterLayoutsText, true) { IsCategoryName = true, Enabled = false });
				ToolStripFindDropButton.DropDownItems.AddRange(menuItemsPublished);
			}
		}

		protected void ReloadFindDropList()
		{
			((IFilterStripBusinessObjectInternals)FilterBusinessObject).ClearLayoutsCache();
			ToolStripFindDropButton.DropDownItems.Clear();

			LoadFindDropList();
		}

		protected virtual void ItemClicked(object sender, EventArgs e)
		{
		}

		void QueryUserAndResetStrips()
		{
			var messageText = Res.GetString("FilterStripControl|ThisWillCompletelyResetTheCurrentFilterLayout", "This will completely reset the current filter layout. Do you wish to continue?");
			if (Globals.Message.Show(messageText, Res.GetString("FilterStripControl|ResetLayout", "Reset Layout"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				ResetFilterStrips();
				FilterReset();
			}
		}

		protected virtual void FilterReset()
		{
		}

		void AddNewGroup()
		{
			using (var groupNameForm = new FilterGroupNameForm())
			{
				if (ZFormModaliser.ShowDialogAndDispose(groupNameForm) == DialogResult.OK)
				{
					var groupName = groupNameForm.SaveFiltersTextBox.Text;
					if (string.IsNullOrEmpty(groupName))
					{
						Globals.Message.ShowInformation(Res.GetString("dd994a72-145f-478d-8edd-afcd71c5251c", "You cannot add an empty group name."));
					}
					else if (!this.GroupExists(groupName))
					{
						this.CurrentGroupName = groupName;
						this.AddStrip();
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("5e29ec16-bd0d-4175-aa7b-b416fc29f7d2", "The group you are trying to add already exists."));
					}
				}
			}
		}

#if !WINZOR
		const int DISABLE_DRAWING = 0;
		const int ENABLE_DRAWING = 1;
		const int WM_SETREDRAW = 0xB;
#endif
		const int FirstStripLeft = 3;
		const int FirstStripTop = 3;
		const int ToolStripGap = 10;
		const int FirstGroupLeft = 3;
		const int FirstGroupTop = 3;
		const int AddButtonLeftAdjustment = 45;
		const int ToolStripLeftAdjustment = 8;
		const int RecentItemsPanelHeightAdjustment = 9;

		protected virtual int MinFilterStripPanelHeight
			=> 0;

		protected virtual int MaxFilterStripPanelHeight
			=> ControlDpiScalingHelper.ScaleToCurrentDpiY(365);

		protected virtual void HookButtonEvents()
		{
			ToolStripHelpButton.Click += delegate
			{ ShowHelp(); };
			ToolStripSaveLayoutButton.Click += delegate
			{ SaveLayout(); };
			ToolStripManageDropButton.ButtonClick += delegate
			{ ManageLayouts(false); };
			ToolStripManageLayoutsMenuItem.Click += delegate
			{ ManageLayouts(false); };
			ToolStripManageUsedDefinedMenuItem.Click += delegate
			{ ManageLayouts(true); };
			AddStripButton.Click += delegate
			{
				AddStrip();
				UpdateLayout();
			};
			ToolStripResetLayoutButton.Click += delegate
			{ ResetLayoutClicked(); };
			ToolStripAddGroupButton.Click += delegate
			{ AddGroup(); };
			ToolStripClearButton.Click += delegate
			{ ClearStrips(); };
			ToolStrip.ItemClicked += new ToolStripItemClickedEventHandler(ToolStrip_ItemClicked);
			ToolStripHelp.ItemClicked += new ToolStripItemClickedEventHandler(ToolStrip_ItemClicked);
		}

		void ResetLayoutClicked()
		{
			ResetLayout();
			FilterBusinessObject.ShouldSetDefaults = false;
		}

		void ToolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			PersistCurrentControlValue();
		}

		protected virtual void FilterEdited()
		{
		}

		protected virtual void FilterStripEdited()
		{
		}

		#region Layout

		protected override void OnLayout(LayoutEventArgs e)
		{
			if (FilterStripsPanel.Visible)
			{
				RefreshStripLayout();
			}

			base.OnLayout(e);
		}

		protected internal void RefreshStripLayout()
		{
			if (GroupStrips.Count > 0)
			{
				foreach (var group in GroupStrips)
				{
					RefreshStripLayoutWithinGroup(group.Key);
				}
			}
			else if (Strips.Count > 0)
			{
				var topStrip = Strips[0];
				if (topStrip.Top > ControlDpiScalingHelper.ScaleToCurrentDpiY(FirstStripTop)) // top can be negative if panel scrollbar is active
				{
					ControlDpiScalingHelper.SetTop(ref topStrip, FirstStripTop, true);
				}

				var previousStripBottom = topStrip.Bottom;
				for (var i = 1; i < Strips.Count; i++)
				{
					ControlDpiScalingHelper.SetTop(Strips[i], previousStripBottom, false);
					previousStripBottom = Strips[i].Bottom;
				}

				UpdateLayout();
			}
		}

		void RefreshStripLayoutWithinGroup(string grpName)
		{
			if (GroupStrips.Count <= 0)
			{
				throw new InvalidOperationException("No groups exist");
			}

			var strips = GetStripsForGroup(grpName);
			var topStrip = strips.FirstOrDefault();

			if (topStrip != null)
			{
				ControlDpiScalingHelper.SetTop(ref topStrip, FirstStripTop, true);
				var previousStripBottom = topStrip.Bottom;

				foreach (var strip in strips.Skip(1))
				{
					ControlDpiScalingHelper.SetTop(strip, previousStripBottom, false);
					previousStripBottom = strip.Bottom;
				}

				UpdateLayout();
			}
		}

		internal void RefreshGroupLayout()
		{
			if (GroupControls.Count > 0)
			{
				var topGroup = GroupControls[0];
				var maxTop = ControlDpiScalingHelper.ScaleToCurrentDpiY(FirstGroupTop);
				if (topGroup.Top > maxTop)  //	top can be negative if panel scrollbar is active
				{
					ControlDpiScalingHelper.SetTop(ref topGroup, maxTop, true);
				}

				var previousGroupBottom = topGroup.Bottom;
				for (var i = 1; i < GroupControls.Count; i++)
				{
					ControlDpiScalingHelper.SetTop(GroupControls[i], previousGroupBottom, false);
					previousGroupBottom = GroupControls[i].Bottom;
				}
			}

			UpdateLayout();
		}

		protected virtual void UpdateLayout()
		{
			if (!IsDisposed)
			{
				SuspendDrawing();
				SuspendLayout();

				try
				{
					if (GroupControls.Count <= 0)
					{
						UpdateToolStripButtonsState();

						ControlDpiScalingHelper.SetTop(AddStripButton, ToolStripTop, false);
						ControlDpiScalingHelper.SetTop(ToolStrip, ToolStripTop, false);
						ControlDpiScalingHelper.SetTop(ToolStripHelp, ToolStrip.Top, false);
						ControlDpiScalingHelper.SetTop(ToolStripColourPicker, ToolStrip.Top, false);
						// This one pixel adjustment is independent of the DPI, since it represents the height of a line that's 1 pixel weight
						ControlDpiScalingHelper.SetTop(CoveringLabel, ToolStrip.Bottom - ControlDpiScalingHelper.OnePixel, false); // label used to hide ugly lines under the toolstrip
						ControlDpiScalingHelper.SetWidth(CoveringLabel, ToolStrip.Left + ToolStrip.Width, false); // This prevents the label from cropping the'AddStripButton'
						UpdateFilteredGridRefreshWarning();
						ControlDpiScalingHelper.SetHeight(FilterStripsPanel, ToolStripTop - ControlDpiScalingHelper.ScaleToCurrentDpiY(ToolStripGap), false);
						UpdateToolStripLayout(Strips);
					}
					else
					{
						UpdateGroupToolStripButtonsState();

						ControlDpiScalingHelper.SetTop(AddStripButton, ToolStripGroupTop, false);
						ControlDpiScalingHelper.SetTop(ToolStrip, ToolStripGroupTop, false);
						ControlDpiScalingHelper.SetTop(ToolStripHelp, ToolStrip.Top, false);
						ControlDpiScalingHelper.SetTop(ToolStripColourPicker, ToolStrip.Top, false);
						// This one pixel adjustment is independent of the DPI, since it represents the height of a line that's 1 pixel weight
						ControlDpiScalingHelper.SetTop(CoveringLabel, ToolStrip.Bottom - ControlDpiScalingHelper.OnePixel, false); // label used to hide ugly lines under the toolstrip
						ControlDpiScalingHelper.SetWidth(CoveringLabel, ToolStrip.Left + ToolStrip.Width, false); // This prevents the label from cropping the'AddStripButton'
						UpdateFilteredGridRefreshWarning();
						ControlDpiScalingHelper.SetHeight(FilterStripsPanel, GroupControls.Count > 0 ? ToolStripGroupTop - ControlDpiScalingHelper.ScaleToCurrentDpiY(ToolStripGap) : ToolStripTop - ControlDpiScalingHelper.ScaleToCurrentDpiY(ToolStripGap), false);
						ControlDpiScalingHelper.SetWidth(FilterStripsPanel, GroupControls[0].Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(PanelGapWidth), false);
						UpdateToolStripLayout(GroupControls);
					}
				}
				finally
				{
					ResumeLayout();
					ResumeDrawing();
				}
			}
		}

		protected virtual void UpdateToolStripLayout<T>(List<T> controlList) where T : IStripControl
		{
			ToolStripManageDropButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
			ToolStripSaveLayoutButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;

			SuspendLayout();
			try
			{
				ControlDpiScalingHelper.SetLeft(AddStripButton, Math.Min(Width - AddStripButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(AddButtonLeftAdjustment), ControlDpiScalingHelper.ScaleToCurrentDpiX(MaxWorkWidth)), false);
				ControlDpiScalingHelper.SetLeft(ToolStrip, AddStripButton.Left - ToolStrip.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(ToolStripLeftAdjustment), false);
				ControlDpiScalingHelper.SetLeft(CoveringLabel, ToolStripHelp.Left, false);

				if (controlList != null)
				{
					foreach (var control in controlList)
					{
						control.UpdateLayout(GetStripControlWidth(control));
					}
				}

#if !WINZOR
				if (Parent != null && !(Parent is IZForm))
				{
					ControlDpiScalingHelper.SetHeight(RecentItemsPanel, ToolStrip.Top + ToolStrip.Height + Parent.Location.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(RecentItemsPanelHeightAdjustment + recentItemsPanelLocationOffsetY), false);
				}
				else
				{
					ControlDpiScalingHelper.SetHeight(RecentItemsPanel, ToolStrip.Top + ToolStrip.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(RecentItemsPanelHeightAdjustment + recentItemsPanelLocationOffsetY), false);
				}
#else
				if (Parent != null && !(Parent is IZForm))
				{
					ControlDpiScalingHelper.SetHeight(RecentItemsPanel, ToolStrip.Top + ToolStrip.Height + Parent.Location.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(RecentItemsPanelHeightAdjustment + RecentItemsPanelLocationOffsetY), false);
				}
				else
				{
					ControlDpiScalingHelper.SetHeight(RecentItemsPanel, ToolStrip.Top + ToolStrip.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(RecentItemsPanelHeightAdjustment + RecentItemsPanelLocationOffsetY), false);
				}
#endif
				UpdateAdditionalToolStripLayout();
			}
			finally
			{
				ResumeLayout();
			}
		}

#if !WINZOR
		int drawingSuspendCount;
#endif

		void SuspendDrawing()
		{
#if !WINZOR
			if (drawingSuspendCount == 0 && this.IsHandleCreated)
			{
				CargoWise.Interop.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), WM_SETREDRAW, DISABLE_DRAWING, 0);
			}
			drawingSuspendCount++;
#endif
		}

		void ResumeDrawing()
		{
#if !WINZOR
			--drawingSuspendCount;
			if (drawingSuspendCount == 0 && this.IsHandleCreated)
			{
				CargoWise.Interop.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), WM_SETREDRAW, ENABLE_DRAWING, 0);
				Invalidate(true);
			}
#endif
		}

		#endregion

		void UpdateToolStripButtonsState()
		{
			if (Strips.Count == 1 + FilterBusinessObject.AlwaysVisibleModuleFilters.Count)
			{
				var lastStrip = LastStrip;
				if (lastStrip != null)
				{
					lastStrip.IsDeleteButtonEnabled = false;
				}
			}
			else
			{
				foreach (var strip in Strips)
				{
					strip.IsDeleteButtonEnabled = !strip.ReadOnly;
				}
			}
		}

		internal void UpdateGroupToolStripButtonsState()
		{
			if (GroupControls.Count > 0)
			{
				foreach (var groupStripControl in GroupControls)
				{
					var strips = GetStripsForGroup(groupStripControl.GroupName);
					if (strips.All(s => !s.DeleteStripButton.Visible))
					{
						groupStripControl.SetButtonEnabledState(false);
					}
					else
					{
						groupStripControl.SetButtonEnabledState(!this.GetReadOnly() && !ShouldGroupStripsBeReadOnly);
					}

					groupStripControl.IsFilterCategoriesToolStripDropDownVisible = !strips.Any(s => (s?.CurrentDataItem?.CurrentModuleFilter?.IsGroupOrCategoryReadOnly).GetValueOrDefault(false));
				}
			}
		}

		protected virtual bool ShouldGroupStripsBeReadOnly => false;

		#region Strips

		Point NextStripLocation
		{
			get
			{
				var lastStrip = this.LastStrip;
				return (lastStrip != null) ?
					ControlDpiScalingHelper.NewScaledPoint(lastStrip.Left, lastStrip.Bottom, false) :
					ControlDpiScalingHelper.NewScaledPoint(FirstStripLeft, FirstStripTop);
			}
		}

		[return: DpiState(DpiState.ScaledVariant)]
		internal Point NextStripWithinGroupLocation(string groupName)
		{
			if (LastStripWithinGroup(groupName) != null)
			{
				return ControlDpiScalingHelper.NewScaledPoint(LastStripWithinGroup(groupName).Left, LastStripWithinGroup(groupName).Bottom, false);
			}

			return ControlDpiScalingHelper.NewScaledPoint(FirstStripLeft, FirstStripTop, true);
		}

		Point NextGroupControlLocation
		{
			get
			{
				return (LastGroupControl != null) ?
					ControlDpiScalingHelper.NewScaledPoint(LastGroupControl.Left, LastGroupControl.Bottom, false) :
					ControlDpiScalingHelper.NewScaledPoint(FirstGroupLeft, FirstGroupTop);
			}
		}

		[DpiState(DpiState.ScaleY)]
		protected int LastFilterStripBottom
		{
			get
			{
				int result;

				if (LastStrip == null)
				{
					result = FirstStripTop;
				}
				else
				{
					var stripsHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(ToolStripGap + FirstStripTop);

					foreach (var strip in Strips) // some strips take up multiple rows
					{
						stripsHeight += strip.Height;
					}

					result = (stripsHeight < MaxFilterStripPanelHeight) ? stripsHeight : MaxFilterStripPanelHeight;
				}

				if (result < MinFilterStripPanelHeight)
				{
					result = MinFilterStripPanelHeight;
				}

				return result;
			}
		}

		[DpiState(DpiState.ScaleY)]
		protected virtual int ToolStripTop => LastFilterStripBottom;

		protected int LastGroupControlBottomPlusPadding
		{
			get
			{
				int result;

				if (LastGroupControl == null)
				{
					result = FirstGroupTop;
				}
				else
				{
					var groupsHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(ToolStripGap + FirstGroupTop);

					foreach (var group in GroupControls)  //	some groups take up multiple rows
					{
						groupsHeight += group.Height;
					}

					result = (groupsHeight < MaxFilterStripPanelHeight) ? groupsHeight : MaxFilterStripPanelHeight;
				}

				if (result < MinFilterStripPanelHeight)
				{
					result = MinFilterStripPanelHeight;
				}

				return result + ControlDpiScalingHelper.ScaleToCurrentDpiY(15);
			}
		}

		protected virtual int ToolStripGroupTop => LastGroupControlBottomPlusPadding;

		ZFilterStrip LastStrip
			=> LastStripWithinGroup(currentGroupName) ?? Strips.LastOrDefault();

		ZFilterStrip LastStripWithinGroup(string groupName)
			=> GetStripsForGroup(groupName).LastOrDefault();

		GroupStripControl LastGroupControl
		{
			get { return GroupControls.Count > 0 ? GroupControls[GroupControls.Count - 1] : null; }
		}

		#endregion //Strips

		void ShowHelp()
		{
			WebUrlLauncher.Launch(UpdateNoteURL);
		}

		void SetupToolBarImages()
		{
			SuspendLayout();
			try
			{
				SetupToolBarButton(ToolStripFindDropButton, IconTypes.FindButtonRest);
				SetupToolBarButton(ToolStripPreviewDropButton, IconTypes.FindButtonRest);
				SetupToolBarButton(ToolStripHelpButton, IconTypes.HelpButtonRest);
				SetupToolBarButton(ToolStripSaveLayoutButton, IconTypes.SaveButtonRest);
				SetupToolBarButton(ToolStripManageDropButton, IconTypes.ManageButtonRest);
				SetupToolBarButton(ToolStripResetLayoutButton, IconTypes.ResetButtonRest);
				SetupToolBarButton(ToolStripAddGroupButton, IconTypes.AddButtonRest);
				SetupToolBarButton(ToolStripClearButton, IconTypes.ClearButtonRest);
			}
			finally
			{
				ResumeLayout();
			}
		}

		void SetupToolBarFont()
		{
			var font = new Font(OFont.NormalFontName, 8.25F, FontStyle.Bold);

			ToolStripFindDropButton.Font = font;
			ToolStripPreviewDropButton.Font = font;
			ToolStripHelpButton.Font = font;
			ToolStripSaveLayoutButton.Font = font;
			ToolStripManageDropButton.Font = font;
			ToolStripManageLayoutsMenuItem.Font = font;
			ToolStripManageUsedDefinedMenuItem.Font = font;
			ToolStripResetLayoutButton.Font = font;
			ToolStripAddGroupButton.Font = font;
			ToolStripClearButton.Font = font;
		}

		protected virtual void SetupToolBarButton(ToolStripItem button, IconTypes icon)
			=> button.Image = Icons.GetImage(icon);

		void UpdateGroupStrip(ZFilterStrip strip, string groupName)
		{
			List<ZFilterStrip> strips;
			if (!GroupStrips.TryGetValue(groupName, out strips))
			{
				if (strip == null)
				{
					strips = new List<ZFilterStrip>();
					GroupStrips.Add(groupName, strips);
				}
			}
			else
			{
				strips.Add(strip);
			}
		}

		internal List<ZFilterStrip> GetStripsForGroup(string groupName)
			=> GroupStrips.TryGetValue(groupName, out var strips) ? strips : new List<ZFilterStrip>();

		internal void MoveStripBetweenGroups(string fromKey, string toKey, ZFilterStrip filterStrip)
		{
			List<ZFilterStrip> strips;
			var stripsToDelete = new List<ZFilterStrip>();

			if (GroupStrips.TryGetValue(fromKey, out strips))
			{
				foreach (var strip in strips)
				{
					if (strip == filterStrip)
					{
						stripsToDelete.Add(strip);
						break;
					}
				}
			}

			if (stripsToDelete.Count > 0)
			{
				strips.Remove(stripsToDelete[0]);

				if (GroupStrips.TryGetValue(toKey, out strips))
				{
					strips.Add(stripsToDelete[0]);
				}
			}

			stripsToDelete.RemoveRange(0, stripsToDelete.Count);
		}

		internal void RemoveEmptyGroupStripControl(string grpName)
		{
			var groupControl = FindGroupControlFromGroupName(grpName);
			if (groupControl != null && groupControl.FilterStripGroupBox.Controls.Count == 1)
			{
				DeleteGroupFilterStrip(groupControl);
			}

			RemoveIfOnlyGroup();
		}

		void RemoveIfOnlyGroup()
		{
			if (GroupControls.Count == 1)
			{
				foreach (var currentStrip in Strips)
				{
					currentStrip.Parent = FilterStripsPanel;
					currentStrip.SetFilterDescriptionDropEditLocation();
					ResetCurrentFilterStrip(currentStrip);
				}

				DeleteGroupFilterStrip(GroupControls[0]);
				RefreshStripLayout();
			}
		}

		void ResetCurrentFilterStrip(ZFilterStrip strip)
		{
			if (strip.CurrentDataItem != null)
			{
				strip.CurrentDataItem.GroupName = "";
				strip.CurrentDataItem.GroupOrCategory = FilterOrCategory.None;
				strip.CurrentDataItem.AdditionalColourName = "";
				strip.CurrentDataItem.AdditionalGroupColourName = "";
			}
		}

		void DeleteStripFromGroupStrips(ZFilterStrip strip)
		{
			List<ZFilterStrip> strips;
			var stripToDelete = new List<ZFilterStrip>();
			var grpControl = GroupControls.LastOrDefault(group => group.GroupName == strip.CurrentDataItem.GroupName);

			SuspendLayout();
			try
			{
				if (GroupStrips.TryGetValue(strip.CurrentDataItem.GroupName, out strips))
				{
					strips.Remove(strip);
					Strips.Remove(strip);
				}

				if (strips != null && strips.Count == 0)
				{
					strip.CurrentDataItem.FilterDescription = ""; //just to make 100% sure we blank out the filter if the group gets disposed
					DeleteGroupFilterStrip(grpControl);
				}

				RemoveIfOnlyGroup();
			}
			finally
			{
				ResumeLayout();
			}
		}

		GroupStripControl FindGroupControlFromGroupName(string grpName)
		{
			return GroupControls.LastOrDefault(s => s.GroupName == grpName);
		}

		protected IEnumerable<int> GetExistingAdditionalColor<T>(List<T> controlList) where T : IStripControl
		{
			var strips = controlList.Where(s => s.OrCategory > FilterOrCategory.Grey);
			foreach (IStripControl strip in strips)
			{
				yield return ColorTranslator.ToWin32(Color.FromName(strip.OrCategory.ToString()));
			}
		}

		internal int[] ExistingAdditionalColorForZFilterStrip
		{
			get
			{
				return GetExistingAdditionalColor(Strips).Cast<int>().Distinct().ToArray();
			}
		}

		internal int[] ExistingAdditionalColorForGroupStripControl
		{
			get
			{
				return GetExistingAdditionalColor(GroupControls).Cast<int>().Distinct().ToArray();
			}
		}

		internal void SetFilterStripPanelControlHeight(int height)
		{
			ControlDpiScalingHelper.SetHeight(FilterStripsPanel, FilterStripsPanel.Height + height, false);
		}

		internal bool FilterStripDragDropReorder(ZFilterStrip filterStrip, Point? localpoint)
		{
			var result = false;
			if (localpoint.HasValue)
			{
				if (GroupStrips.Count > 0)
				{
					var filters = GroupStrips[filterStrip.CurrentDataItem.GroupName];
					if (filters != null)
					{
						result = FilterStripDragDropReorderCore(filters, filterStrip, localpoint.Value, filterStrip.CurrentDataItem.GroupName);
					}
				}
				else
				{
					result = FilterStripDragDropReorderCore(Strips, filterStrip, localpoint.Value, string.Empty);
				}
			}
			return result;
		}

		bool FilterStripDragDropReorderCore(List<ZFilterStrip> strips, ZFilterStrip filterStrip, Point localpoint, string groupName)
		{
			var result = false;
			if (strips.Count > 1)
			{
				var newPosition = 0;
				if (localpoint.Y > 0)
				{
					for (var i = 0; i < strips.Count; i++)
					{
						if (localpoint.Y > (strips[i].Location.Y + strips[i].Height))
						{
							newPosition = i + 1;
						}
						else
						{
							break;
						}
					}
				}

				var indexCurrent = strips.IndexOf(filterStrip);
				if (indexCurrent >= 0)
				{
					if (indexCurrent != newPosition) // When dropping a filter in a different group, it's added in the end, so we don't need to reorder it
					{
						var item = strips[indexCurrent];
						strips.RemoveAt(indexCurrent);

						if (newPosition >= strips.Count)
						{
							strips.Add(item);
						}
						else
						{
							strips.Insert(newPosition, item);
						}
						RefreshStripLayout();
						RefreshGroupLayout();
					}

					ReorderFilterBusinessObject(filterStrip, groupName, newPosition);
					result = true;
				}
			}
			return result;
		}

		void ReorderFilterBusinessObject(ZFilterStrip filterStrip, string groupName, int newPositionGroup)
		{
			// FilterBusinessObject has all items in one single list, when we change a filter to another group we just change the property GroupName and don't reorder this list,
			// this way if we save the layout, the filters will be in a different order
			var bizoFilterList = (IList)FilterBusinessObject.FilterStrips;
			var currentIndex = -1;
			var newPosition = -1;

			for (var i = 0; i < bizoFilterList.Count; i++)
			{
				var item = ((FilterStrip)bizoFilterList[i]);
				if (item.GroupName == groupName)
				{
					if (item.PK == filterStrip.RelatedBizoPK)
					{
						currentIndex = i;
					}

					if (newPositionGroup == 0)
					{
						newPosition = i;
					}
					newPositionGroup--;
				}
			}

			if (currentIndex >= 0 && newPosition >= 0)
			{
				var currentItem = bizoFilterList[currentIndex];
				bizoFilterList.RemoveAt(currentIndex);

				if (newPosition >= bizoFilterList.Count)
				{
					bizoFilterList.Add(currentItem);
				}
				else
				{
					bizoFilterList.Insert(newPosition, currentItem);
				}
			}
		}

		#endregion

		[SuppressFormsLocalizedTest]
		public class ZFilterToolStripMenuItem : ZToolStripMenuItem
		{
			[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
			public ZFilterToolStripMenuItem(string text, bool isPublished = false, bool isFavorite = false, bool isAction = false)
			{
				Text = text;
				IsPublished = isPublished;
				IsCategoryName = false;
				IsFavorite = isFavorite;

#if WINZOR
				if (isAction)
				{
					Image = IsFavorite ? FilterLayoutsHelper.FavoriteSelected : FilterLayoutsHelper.FavoriteUnselected;
					HoverImage = IsFavorite ? FilterLayoutsHelper.FavoriteUnselected : FilterLayoutsHelper.FavoriteSelected;
				}
#endif
			}

			public bool IsPublished { get; }

			public bool IsCategoryName { get; set; }

			public bool IsFavorite { get; }
#if WINZOR
			protected override void OnImageMouseEnter()
			{
				IsOnImage = true;
			}

			protected override void OnImageMouseLeave()
			{
				IsOnImage = false;
			}

			public bool IsOnImage;
#else
			public bool IsOnImage
				=> IsPointOnImage(Owner.PointToClient(Cursor.Position));
			
			protected virtual bool IsPointOnImage(Point p)
				=> Bounds.Contains(p) && p.X < ContentRectangle.X + Image.Width;

			protected override void OnMouseMove(MouseEventArgs mea)
			{
				base.OnMouseMove(mea);
				if (Tag != null)
				{
					Image = IsFavorite == IsOnImage ? FilterLayoutsHelper.FavoriteUnselected : FilterLayoutsHelper.FavoriteSelected;
					var addRemove = IsFavorite ? Res.GetString("C5FAD632-293A-4DF9-AAF8-329DE2EAE231", "Remove from Favorites") : Res.GetString("2387B1BA-816E-4DBD-A73D-B4F3FE292314", "Add to Favorites");

					ToolTipText = IsOnImage ? addRemove : FilterLayoutsHelper.GetToolTipText(this, (StmModuleFilter)Tag);
				}
			}

			protected override void OnMouseLeave(EventArgs e)
			{
				base.OnMouseLeave(e);
				if (Tag != null)
				{
					Image = IsFavorite ? FilterLayoutsHelper.FavoriteSelected : FilterLayoutsHelper.FavoriteUnselected;
				}
			}
#endif
		}
	}
}
