using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[ToolboxItem(false)]
	[SuppressBindingMemberBashingTest]
	public partial class ZFilterStrip : ZUserControl, IStripControl, IReadOnlyToggleControl
	{
		internal ZGuid RelatedBizoPK { get; set; }
		ImageList imageListCache;

		#region Construction

		public ZFilterStrip()
		{
			InitializeComponent();
#if WINZOR
			GripHolderPanel.AllowItemDrag = true;
			GripHolderPanel.DragTargetHidden = false;
			var commandKeyHandler = new ZFilterStripDropEdit.CommandKeyHandler(FilterDescriptionDropEdit, this);
			FilterDescriptionDropEdit.SetCommandKeyHandler(commandKeyHandler);
#endif
			FilterCategoriesToolStrip.Renderer = new CargowiseToolStripRenderer();
			InitializeFilterStrip();
			InitializeFilterCategoryMenuItems();

			DeleteStripButton.BackgroundImageLayout = ImageLayout.Stretch;
			DeleteStripButton.FlatStyle = FlatStyle.Flat;
			DeleteStripButton.FlatAppearance.BorderSize = 0;
			DeleteStripButton.BackColor = Color.Transparent;
			DeleteStripButton.BackgroundImage = Icons.GetImage(IconTypes.MinusButtonRest);
			DeleteStripButton.MouseEnter += delegate { DeleteStripButton.BackgroundImage = Icons.GetImage(IsDeleteButtonEnabled ? IconTypes.MinusButtonActive : IconTypes.MinusButtonDisabled); };
			DeleteStripButton.MouseLeave += delegate { DeleteStripButton.BackgroundImage = Icons.GetImage(IsDeleteButtonEnabled ? IconTypes.MinusButtonRest : IconTypes.MinusButtonDisabled); };
			DeleteStripButton.EnabledChanged += OnDeleteStripButtonEnabledChanged;

			FilterPropertyLockButton.BackgroundImageLayout = ImageLayout.Stretch;
			FilterPropertyLockButton.FlatStyle = FlatStyle.Flat;
			FilterPropertyLockButton.FlatAppearance.BorderSize = 0;
			FilterPropertyLockButton.BackColor = Color.Transparent;
			FilterPropertyLockButton.BackgroundImage = Icons.GetImage(IconTypes.BlackWhite_Unlock);

			BindingSource.DefaultBindingMembersEnabled = false;

			GripHolderPanel.BackgroundImage = Icons.GetImage(IconTypes.GripHolder);
			GripHolderPanel.MouseEnter += delegate { GripHolderPanel.Cursor = System.Windows.Forms.Cursors.SizeAll; };
			GripHolderPanel.MouseMove += (o, e) => { if (e.Button == System.Windows.Forms.MouseButtons.Left) { DoDragDrop(this, DragDropEffects.Move); } };

			MissingResourceStringChecker.ExcludeFromTest(DeleteStripButton);

			FilterCategoriesToolStrip.AllowOutsideOfParent();
			FilterPropertyLockButton.AllowOutsideOfParent();
		}

		void InitializeFilterStrip()
		{
			CurrentFilterControls = new List<Control>();
			InitialHeight = Height;

			// We want to have the same space between GripHolderPanel image and FilterDescriptionDropEdit, but it isn't safe to set a non DPI aware point on design because if another developer
			// open up form designer, change anything and save it will change back to DPI aware point.
			// The safest way to do this is to set the non DPI aware point after designer has run
			SetFilterDescriptionDropEditLocation();
		}

		void InitializeFilterCategoryMenuItems()
		{
			imageListCache = ZFilterControlImages.FilterImageList;

			if (imageListCache != null)
			{
				filterCategoriesToolStripMenuItem.Image = imageListCache.Images["FilterCategories"];
			}

			InitializeFilterCategoryMenuItem(noCategoryToolStripMenuItem, "", Color.Transparent, FilterOrCategory.None);
			InitializeFilterCategoryMenuItem(redToolStripMenuItem, "FilterCategoryRed", Color.FromArgb(242, 177, 178), FilterOrCategory.Red);
			InitializeFilterCategoryMenuItem(greenToolStripMenuItem, "FilterCategoryGreen", Color.FromArgb(196, 232, 190), FilterOrCategory.Green);
			InitializeFilterCategoryMenuItem(blueToolStripMenuItem, "FilterCategoryBlue", Color.FromArgb(183, 201, 238), FilterOrCategory.Blue);
			InitializeFilterCategoryMenuItem(brownToolStripMenuItem, "FilterCategoryBrown", Color.FromArgb(226, 202, 176), FilterOrCategory.Brown);
			InitializeFilterCategoryMenuItem(greyToolStripMenuItem, "FilterCategoryGrey", Color.FromArgb(201, 201, 201), FilterOrCategory.Grey);
			InitializeFilterCategoryMenuItem(additionalToolStripMenuItem, "FilterCategories", Color.Empty, FilterOrCategory.Others);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
		void InitializeFilterCategoryMenuItem(ZFilterStripToolStripMenuItem menuItem, string imageKey, Color color, FilterOrCategory orCategory)
		{
			try
			{
				if (imageListCache != null)
				{
					menuItem.Image = imageListCache.Images[imageKey];
				}
			}
			catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (System.Runtime.InteropServices.ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			menuItem.Color = color;
			menuItem.OrCategory = orCategory;
			menuItem.Click += new EventHandler(toolStripMenuItem_Click);
		}

		#endregion

		#region Drag and drop

		protected override void OnDragOver(DragEventArgs drgevent)
		{
			base.OnDragOver(drgevent);
			if (StripToDrop(drgevent) != null)
			{
				AddDragAndDropEffect();
				drgevent.Effect = DragDropEffects.Move;
			}
			else
			{
				drgevent.Effect = DragDropEffects.None;
			}
		}

		protected override void OnDragLeave(EventArgs e)
		{
			base.OnDragLeave(e);
			RemoveDragAndDropEffect();
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Point not used for rendering")]
		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			base.OnDragDrop(drgevent);
			RemoveDragAndDropEffect();

			var strip = StripToDrop(drgevent);

			if (strip != null && drgevent.Effect == DragDropEffects.Move)
			{
				if (ParentGroupFilterControl != null && strip.CurrentDataItem.GroupName != ParentGroupFilterControl.GroupName)
				{
					ParentGroupFilterControl.FilterStripGroupBox_DragDrop(ParentGroupFilterControl.FilterStripGroupBox, drgevent);
				}

				var point = new Point(drgevent.X, drgevent.Y);
				var pointToClient = Parent?.PointToClient(point);
#if DEBUG
				if (Globals.IsTest)
				{
					pointToClient = point;
				}
#endif
				var hasReorder = ParentFilterControl?.FilterStripDragDropReorder(strip, pointToClient);
				if (hasReorder.HasValue && hasReorder.Value)
				{
					ParentFilterStripCommonControl?.ResetFindButton();
				}
			}
		}

		ZFilterStrip StripToDrop(DragEventArgs drgevent)
		{
			var formats = drgevent.Data.GetFormats();
			if (formats.Length > 0 && drgevent.Data.GetData(formats[0]) is ZFilterStrip strip)
			{
				return strip;
			}

			return null;
		}

		internal void UserControl_DragDrop(object sender, DragEventArgs e)
		{
			OnDragDrop(e);
		}

		internal void UserControl_DragLeave(object sender, EventArgs e)
		{
			OnDragLeave(e);
		}

		internal void UserControl_DragOver(object sender, DragEventArgs e)
		{
			OnDragOver(e);
		}

		void AddDragAndDropEffect()
		{
			if (BackColor != Color.LightGray)
			{
				BackColor = Color.LightGray;
			}
		}

		void RemoveDragAndDropEffect()
		{
			if (OrCategory != FilterOrCategory.None)
			{
				BackColor = FilterCategoriesToolStrip.BackColor;
			}
			else
			{
				BackColor = default;
			}
		}

		ZFilterStripBaseControl ParentFilterStripCommonControl
		{
			get
			{
				var parent = base.Parent;
				while (parent != null)
				{
					if (parent is ZFilterStripBaseControl parentFilterControl)
					{
						return parentFilterControl;
					}
					parent = parent.Parent;
				}

				return null;
			}
		}

		#endregion

		#region IsDeleteButtonDisabled

		public bool IsDeleteButtonEnabled
		{
			get { return DeleteStripButton.Enabled; }
			set
			{
				DeleteStripButton.Enabled = value;
			}
		}

		void OnDeleteStripButtonEnabledChanged(object sender, EventArgs e)
		{
			DeleteStripButton.BackgroundImage = Icons.GetImage(DeleteStripButton.Enabled ? IconTypes.MinusButtonRest : IconTypes.MinusButtonDisabled);
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);

			IsDeleteButtonEnabled = Enabled;
		}

		#endregion

		#region IsDeleteButtonDisabled

		public bool IsFilterCategoriesEnabled
		{
			get { return FilterCategoriesToolStripDropDown.Visible; }
			set { FilterCategoriesToolStripDropDown.Visible = value; }
		}

		#endregion

		#region Clicking a Filter Category MenuItem

		void toolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (((ZFilterStripToolStripMenuItem)sender).OrCategory > FilterOrCategory.Grey)
			{
				ChangeAdditionalFilterOrCategory(((ZFilterStripToolStripMenuItem)sender).OrCategory);
			}
			else
			{
				ChangeFilterOrCategory(((ZFilterStripToolStripMenuItem)sender).OrCategory);
			}
		}

		void ChangeAdditionalFilterOrCategory(FilterOrCategory filterOrCategory)
		{
			var additionalOrCategory = GetAdditionalColors();
			ChangeFilterOrCategory(additionalOrCategory);
		}

		FilterOrCategory GetAdditionalColors()
		{
			var cp = new ColourProperties();

			using (var colorDialog = new ColorDialog())
			{
				colorDialog.CustomColors = ParentFilterControl.ExistingAdditionalColorForZFilterStrip;

				if (colorDialog.ShowDialog() == DialogResult.OK)
				{
					cp.ToKnownColour(colorDialog.Color);
					CurrentDataItem.AdditionalColourName = cp.ColourName;
					CurrentDataItem.OrCategory = (FilterOrCategory)Enum.Parse(typeof(FilterOrCategory), cp.ColourName);
				}
			}

			return CurrentDataItem.OrCategory;
		}

		void ChangeFilterOrCategory(FilterOrCategory orCategory)
		{
			FilterCategoriesToolStripDropDown.SelectCategory(orCategory);

			// colours
			var color = (int)orCategory > (int)FilterOrCategory.Grey ? Color.FromName(Enum.GetName(typeof(FilterOrCategory), orCategory)) : FilterCategoriesToolStripDropDown.CurrentCategoryColor;
			BackColor = color;
			FilterCategoriesToolStrip.BackColor = color;

			ChangeControlColorToMatchCategoryColor(CurrentFilterControls, color);

			if (orCategory != FilterOrCategory.Others)
			{
				CurrentDataItem.OrCategory = orCategory;
			}
		}

		void ChangeControlColorToMatchCategoryColor(List<Control> controls, Color color)
		{
			foreach (var control in controls)
			{
				if (control is CheckBox || control is RadioButton)
				{
					control.BackColor = color;
				}
				ChangeControlColorToMatchCategoryColor(control.Controls, color);
			}
		}

		void ChangeControlColorToMatchCategoryColor(Control.ControlCollection controls, Color color)
		{
			if (controls != null)
			{
				foreach (Control control in controls)
				{
					if (control is CheckBox || control is RadioButton)
					{
						control.BackColor = color;
					}
					ChangeControlColorToMatchCategoryColor(control.Controls, color);
				}
			}
		}

		void BizO_OrCategoryChanged(object sender, EventArgs e)
		{
			UpdateFilterControlColorsForOrCategory();
			OnFilterOrCategoryChanged();
		}

		#endregion

		#region Changing FilterDescription

		void FilterDescriptionInfo_ValueChanged(object sender, EventArgs e)
		{
			OnFilterEdited();
			RebuildAndBindFilterControls();
		}

		#endregion

		#region Building & Binding up the Module Filter Controls

		void RebuildAndBindFilterControls()
		{
			SuspendLayout();
			try
			{
				PreferredHeight = InitialHeight;
				var currentControls = GetCurrentFilterControls(CurrentDataItem.CurrentModuleFilter);

				FilterControlBindingSource.SetDataBinding(null, "");

				if (currentControls != null)
				{
					SetCurrentFilterControls(currentControls);
					FilterControlBindingSource.SetDataBinding(CurrentDataItem.CurrentModuleFilter, "");
					VerifyAllControlsBound();
					HookValueChangedFromCurrentModule();
					SetModuleFilterGroupCategory();
					currentControls.ForEach(GripHolderPanel.AllowOverlap);
				}
				else
				{
					UnHookValueChangedEvents();
				}
				FilterCategoriesToolStripDropDown.Visible = !(CurrentDataItem.CurrentModuleFilter?.IsOrCategoryReadOnly ?? false);
				UpdateFilterControlFonts();
				UpdateFilterControlColorsForOrCategory();
				FireHeightChangedIfNecessary();
			}
			catch (NullReferenceException ex)
			{
				ErrorReporter.ReportOnce("RebuildAndBindFilterControls_Catch", BuildMessage(ex.Message));
			}
			finally
			{
				ResumeLayout();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting")]
		void SetModuleFilterGroupCategory()
		{
			if (CurrentDataItem.CurrentModuleFilter != null)
			{
				CurrentDataItem.CurrentModuleFilter.GroupName = CurrentDataItem.GroupName;
				CurrentDataItem.CurrentModuleFilter.GroupOrCategory = CurrentDataItem.GroupOrCategory;
			}
			else
			{
				ErrorReporter.ReportOnce("SetModuleFilterGroupCategory", BuildMessage("CurrentDataItem.CurrentModuleFilter is null"));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal log message")]
		string BuildMessage(string initialMessage)
		{
			var message = new ZStringBuilder();
			message.AppendLine(string.Format(CultureInfo.InvariantCulture, initialMessage));
			message.AppendLine(string.Format(CultureInfo.InvariantCulture, "ParentFilterControl: {0} ({1})", ParentFilterControl?.Name, ParentFilterControl?.GetType()));
			message.AppendLine(string.Format(CultureInfo.InvariantCulture, "ParentFilterStripCommonControl: {0} ({1})", ParentFilterStripCommonControl?.Name, ParentFilterStripCommonControl?.GetType()));
			message.AppendLine("CurrentDataItem Type: " + CurrentDataItem.GetType());
			message.AppendLine("CurrentDataItem.CurrentModuleFilter.Description: " + CurrentDataItem?.CurrentModuleFilter?.Description);
			message.AppendLine("CurrentDataItem.CurrentModuleFilter.TableName: " + CurrentDataItem?.CurrentModuleFilter?.TableName);
			message.AppendLine("CurrentDataItem CurrentModuleFilterDescription: " + CurrentDataItem.CurrentModuleFilterDescription);
			message.AppendLine(string.Format(CultureInfo.InvariantCulture, "GetCurrentFilterControls: {0}", GetCurrentFilterControls(CurrentDataItem.CurrentModuleFilter)?.Length));
			message.AppendLine(string.Format(CultureInfo.InvariantCulture, "FilterControlBindingSource.DataSource: {0}", FilterControlBindingSource?.DataSource));

			return message.ToString();
		}

		void HookValueChangedFromCurrentModule()
		{
			var currentModule = CurrentDataItem != null ? CurrentDataItem.CurrentModuleFilter : null;

			try
			{
				if (currentModule != moduleFilterHookedForValueChangedEvents)
				{
					if (moduleFilterHookedForValueChangedEvents != null)
					{
						HookOrUnHookValueChangedEvents(false, moduleFilterHookedForValueChangedEvents);
					}

					if (currentModule != null)
					{
						HookOrUnHookValueChangedEvents(true, currentModule);
					}
				}
			}
			finally
			{
				moduleFilterHookedForValueChangedEvents = currentModule;
			}
		}

		void UnHookValueChangedEvents()
		{
			try
			{
				if (moduleFilterHookedForValueChangedEvents != null)
				{
					HookOrUnHookValueChangedEvents(false, moduleFilterHookedForValueChangedEvents);
				}
			}
			finally
			{
				moduleFilterHookedForValueChangedEvents = null;
			}
		}

		ModuleFilter moduleFilterHookedForValueChangedEvents;

		void HookOrUnHookValueChangedEvents(bool hook, ModuleFilter moduleFilter)
		{
			var infos = moduleFilter.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

			foreach (var info in infos)
			{
				if (IsZPropertyInfoType(info))
				{
					var zPropertyInfo = (ZPropertyInfo)info.GetValue(moduleFilter, Array.Empty<object>());
					if (hook)
					{
						zPropertyInfo.ValueChanged += ZPropertyInfoValueChangedEventHandler;
					}
					else
					{
						zPropertyInfo.ValueChanged -= ZPropertyInfoValueChangedEventHandler;
					}
				}
			}
		}

		protected virtual bool IsZPropertyInfoType(PropertyInfo info)
		{
			return info.PropertyType == typeof(ZPropertyInfo);
		}

		void ZPropertyInfoValueChangedEventHandler(object sender, EventArgs e)
		{
			OnFilterEdited();
		}

		protected ZBindingSource FilterControlBindingSource
		{
			get { return filterControlBindingSource ?? (filterControlBindingSource = new ZBindingSource(this) { DataSourceType = typeof(object) }); }
		}
		ZBindingSource filterControlBindingSource;

		void ResetFilterControlBindingSource()
		{
			if (filterControlBindingSource != null)
			{
				filterControlBindingSource.Dispose();
				filterControlBindingSource = null;
			}
		}

		protected virtual Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result = null;
			if (TryGetCustomFilterControls(currentModuleFilter, out result))
			{
				return result;
			}

			var textFilter = CurrentDataItem.CurrentModuleFilter as ModuleTextFilter;

			if (textFilter != null && textFilter.List != null)
			{
				result = GetTextWithListFilterControls(textFilter.HasComparisonOperator, textFilter.ShowDescription);
			}
			else if (CurrentDataItem.IsNkFilter)
			{
				result = GetControlsForFilterWithSelectedFilters();
			}
			else if (CurrentDataItem.IsTextAndNkFilter)
			{
				result = GetControlsForFilterWithSelectedFilters();
			}
			else if (CurrentDataItem.IsPeriodFilter)
			{
				result = GetPeriodFilterControls();
			}
			else if (CurrentDataItem.IsNumberFilter || CurrentDataItem.IsTextFilter)
			{
				result = GetTextOrNumberFilterControls();
			}
			else if (CurrentDataItem.IsDateFilter)
			{
				result = GetDateFilterControls();
			}
			else if (CurrentDataItem.IsTimeFilter)
			{
				result = GetTimeFilterControls();
			}
			else if (CurrentDataItem.IsSingleDateFilter)
			{
				result = GetSingleDateFilterControls();
			}
			else if (CurrentDataItem.IsGuidFilter)
			{
				result = GetControlsForFilterWithSelectedFilters();
			}
			else if (CurrentDataItem.IsGuidsFilter)
			{
				result = GetGuidsFilterControls();
			}
			else if (CurrentDataItem.IsLocationFilter)
			{
				result = GetLocationsFilterControls();
			}
			else if (CurrentDataItem.IsFlagsFilter)
			{
				result = GetFlagsFilterControls();
			}
			else if (CurrentDataItem.IsNumberRangeFilter)
			{
				result = GetNumberRangeControls();
			}
			else if (CurrentDataItem.IsTextRangeFilter)
			{
				result = GetTextRangeControls();
			}
			else if (CurrentDataItem.IsSqlFilter)
			{
				result = GetSqlFilterControls();
			}
			else
			{
				RemoveCurrentFilterControls(); // user typed crap
			}

			if (!CurrentDataItem.IsNumberRangeFilter && !CurrentDataItem.IsTextRangeFilter && !CurrentDataItem.IsLocationFilter && !CurrentDataItem.IsGuidsFilter && result != null)
			{
				foreach (var createdControl in result)
				{
					this.LabelCaptionRenderProvider.SetLabelCaptionVisible(createdControl, false);
				}
			}

			return result;
		}

		#region CustomFilterControlsBuilder

		public void AddCustomFilterControlsBuilder(CustomFilterControlsBuilder builder)
		{
			customFilterControlsBuilders.Add(builder);
		}

		bool TryGetCustomFilterControls(ModuleFilter moduleFilter, out Control[] controls)
		{
			foreach (var builder in CustomFilterControlsBuilders)
			{
				if (builder.Handles(moduleFilter))
				{
					controls = builder.GetFilterControls(this, moduleFilter, FilterControlBindingSource);
					return true;
				}
			}

			controls = null;
			return false;
		}

		public IEnumerable<CustomFilterControlsBuilder> CustomFilterControlsBuilders
		{
			get { return customFilterControlsBuilders; }
		}
		readonly IList<CustomFilterControlsBuilder> customFilterControlsBuilders = new List<CustomFilterControlsBuilder>();

		public abstract class CustomFilterControlsBuilder
		{
			public abstract bool Handles(ModuleFilter moduleFilter);
			public abstract Control[] GetFilterControls(ZFilterStrip parentStrip, ModuleFilter moduleFilter, ZBindingSource bindingSource);
		}

		#endregion

		void UpdateFilterControlFonts()
		{
			if (CurrentDataItem.IsFilterDescriptionEmpty)
			{
				FilterDescriptionDropEdit.CodeBox.Font = StandardItemFont;
				FilterDescriptionDropEdit.ForeColor = SystemColors.GrayText;
			}
			else if (CurrentDataItem.CurrentModuleFilter != null && CurrentDataItem.CurrentModuleFilter.IsExclusiveHelper)
			{
				FilterDescriptionDropEdit.CodeBox.Font = ExclusiveItemFont;
				FilterDescriptionDropEdit.ForeColor = Color.Blue;

				foreach (var control in CurrentFilterControls)
				{
					control.ForeColor = Color.Blue;
				}
			}
			else
			{
				FilterDescriptionDropEdit.CodeBox.Font = StandardItemFont;
				FilterDescriptionDropEdit.ForeColor = SystemColors.WindowText;
			}
		}

		void UpdateFilterControlColorsForOrCategory()
		{
			ChangeFilterOrCategory(CurrentDataItem.OrCategory);
		}

		void FireHeightChangedIfNecessary()
		{
			if (Height != PreferredHeight && ParentFilterControl != null)
			{
				ParentFilterControl.SetFilterStripPanelControlHeight(Height < PreferredHeight ? PreferredHeight : -PreferredHeight);
				ControlDpiScalingHelper.SetHeight(this, PreferredHeight, false);
				OnHeightChanged();
				ParentFilterControl.RefreshGroupLayout();
			}
		}

		void OnHeightChanged()
		{
			if (HeightChanged != null)
			{
				HeightChanged(this, EventArgs.Empty);
			}
		}

		public void SetHeight(int height)
		{
			PreferredHeight = height;
			FireHeightChangedIfNecessary();
		}

		public event EventHandler HeightChanged;

		protected int PreferredHeight;
		int InitialHeight;

		#region Control Location and Size Values

		public int LabelTop { get { return FilterDescriptionDropEdit.Top + ControlDpiScalingHelper.ScaleToCurrentDpiY(2); } }
		public int FilterControlTop { get { return FilterDescriptionDropEdit.Top; } }
		public int AddOrControlTop { get { return FilterControlTop + FilterDescriptionDropEdit.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(5); } }
		public int AddOrControlLeft { get { return FilterDescriptionDropEdit.Left + FilterDescriptionDropEdit.Width / 2 - ControlDpiScalingHelper.ScaleToCurrentDpiX(35); } }

		[DpiState(DpiState.Unscaled)]
		public int FilterControlsStart { get { return ControlDpiScalingHelper.UnscaleFromCurrentDpiX(FilterDescriptionDropEdit.Right) + SpaceForErrorProviderAndFirstLabel; } }

		[DpiState(DpiState.Unscaled)]
		public int FilterControlsBox1Start
		{
			get { return FilterControlsStart + SpaceForFirstLabel; }
		}

		[return: DpiState(DpiState.Unscaled)]
		public int FilterControlsBox2Start(Control control)
		{
			return FilterControlsBox1Start + FilterControlBoxWidth - ControlDpiScalingHelper.UnscaleFromCurrentDpiX(control.Width);
		}

		public int Label1Start(ZLabel label)
		{
			return ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterControlsBox1Start - SpaceBetweenLabelAndControl) - label.Width;
		}

		public int Label2Start(ZLabel label, Control controlAfterLabel)
		{
			return ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterControlsBox2Start(controlAfterLabel) - SpaceBetweenLabelAndControl) - label.Width;
		}

		public int FilterControlBoxHeight => ZTextBox.DefaultHeight;

		public int FilterControlBoxWidthSmall => ZFilterStripDateEdit.DefaultWidth;

		protected int FilterErrorProviderWidth
		{
			get
			{
				return DeleteStripButton.Left - ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterControlsBox1Start) - ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterControlBoxWidth);
			}
		}

		[DpiState(DpiState.Unscaled)]
		public int FilterControlBoxWidth
		{
			get { return Math.Min(filterControlBoxOverriddenWidth, FilterControlBoxMaxWidth); }
		}
		int filterControlBoxOverriddenWidth = FilterControlBoxMaxWidth;

		internal const int FilterControlBoxMaxWidth = 300;
		public const int DropListCodeBoxWidth = 70;
		public const int DropListDateBoxWidth = 110;
		protected const int SpaceForErrorProviderAndFirstLabel = 28;
		protected const int SpaceForFirstLabel = 65;
		public const int SpaceBetweenLabelAndControl = 4;
		const int FilterDescriptionMaxWidth = 201;
		internal const int MaxFilterStripWidth = 680;
		internal const int MinFilterStripWidth = 450;
		internal const int DeleteButtonDefaultLeft = 615;
		internal const int FilterButtonDefaultLeft = 658;

		internal void UpdateLayout(int width)
		{
			SuspendLayout();
			try
			{
				if (width < ControlDpiScalingHelper.ScaleToCurrentDpiX(MinFilterStripWidth))
				{
					width = ControlDpiScalingHelper.ScaleToCurrentDpiX(MinFilterStripWidth);
				}
				if (width > ControlDpiScalingHelper.ScaleToCurrentDpiX(MaxFilterStripWidth))
				{
					width = ControlDpiScalingHelper.ScaleToCurrentDpiX(MaxFilterStripWidth);
				}

				var widthChanged = width != Width;

				if (widthChanged && FilterDescriptionDropEdit != null)
				{
					var shift = ControlDpiScalingHelper.ScaleToCurrentDpiX(MaxFilterStripWidth) - width;

					FilterDescriptionDropEdit.SetControlWidth(ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterDescriptionMaxWidth + SpaceBetweenLabelAndControl) - shift / 3);
					filterControlBoxOverriddenWidth = FilterControlBoxMaxWidth + FilterDescriptionMaxWidth - ControlDpiScalingHelper.UnscaleFromCurrentDpiX(shift + FilterDescriptionDropEdit.Width);
					ControlDpiScalingHelper.SetLeft(DeleteStripButton, ControlDpiScalingHelper.ScaleToCurrentDpiX(DeleteButtonDefaultLeft) - shift, false);
					ControlDpiScalingHelper.SetLeft(FilterCategoriesToolStrip, ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterButtonDefaultLeft) - shift, false);

					ControlDpiScalingHelper.SetWidth(this, width, false);
				}
				if (widthChanged && CurrentDataItem != null)
				{
					RebuildAndBindFilterControls();
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "We want to keep always the same space between GripHolderPanel image and FilterDescriptionDropEdit")]
		public void SetFilterDescriptionDropEditLocation()
		{
			FilterDescriptionDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(14, 1);
		}

		#endregion

		#region Text & Number Filter Controls

		protected Control[] GetTextWithListFilterControls(bool createComparisonOperator, bool showDescription)
		{
			var result = new List<Control>();
			if (createComparisonOperator)
			{
				var operatorDropEdit = CreateComparisonOperatorDropList();
				operatorDropEdit.TabIndex = 1;
				result.Add(operatorDropEdit);
			}
			var dropEdit = new ZDropEditWithFixedWidth();
			dropEdit.CharacterCasing = CharacterCasing.Normal;
			ControlDpiScalingHelper.SetTop(ref dropEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref dropEdit, FilterControlsBox1Start, true);
			ControlDpiScalingHelper.SetWidth(ref dropEdit, FilterControlBoxWidth, true);
			ControlDpiScalingHelper.SetWidth(dropEdit.CodeBox, showDescription ? DropListCodeBoxWidth : FilterControlBoxWidth - 20, true);
			dropEdit.ShowInDropDown = showDescription ? ZDropEdit.ShowInDropDownList.ShowCodeAndDescription : ZDropEdit.ShowInDropDownList.OnlyShowCode;
			dropEdit.ShowDescriptionBox = showDescription;
			dropEdit.TabIndex = 1;
			dropEdit.BindTo = "Property";
			dropEdit.BindToList = "List";
			FilterControlBindingSource.SetBindingMember(dropEdit, dropEdit.BindTo);
			MissingResourceStringChecker.ExcludeFromTest(dropEdit);
			result.Add(dropEdit);

			return result.ToArray();
		}

#if DEBUG
		internal
#endif
		Control[] GetTextOrNumberFilterControls()
		{
			var result = new List<Control>();
			var currentTabIndex = 1;

			if (((IModuleFilterWithSelectedFilters)CurrentDataItem.CurrentModuleFilter).HasComparisonOperator)
			{
				var operatorDropEdit = CreateComparisonOperatorDropList();
				operatorDropEdit.Name = "OperatorDropEdit";
				operatorDropEdit.TabIndex = currentTabIndex++;
				result.Add(operatorDropEdit);
			}

			var textBox = new ZTextBox();
			textBox.IsOnFilterStrip = true;
			ControlDpiScalingHelper.SetTop(ref textBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref textBox, FilterControlsBox1Start, true);
			ControlDpiScalingHelper.SetWidth(ref textBox, FilterControlBoxWidth, true);
			textBox.TabIndex = currentTabIndex;

			if (CurrentDataItem.IsTextFilter && CurrentDataItem.CurrentModuleFilter != null)
			{
				textBox.MaxLength = CurrentDataItem.CurrentModuleFilter.MaxLength;
			}

			FilterControlBindingSource.SetBindingMember(textBox, "Property");
			result.Add(textBox);

			return result.ToArray();
		}

		protected ZDropEdit CreateComparisonOperatorDropList(string bindTo = ComparisonOperatorBindToIdentifier, string bindToList = ComparisonOperatorBindToListIdentifier)
		{
			var result = new ZDropEdit();
			result.CharacterCasing = CharacterCasing.Lower;
			result.CodeBox.TextAlign = HorizontalAlignment.Center;
			result.CodeBox.Font = ComparisonOperatorFont;
			result.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref result, FilterControlTop, false);
			result.PreBoundMaxLength = FilterComparisonOperatorBoxPreBoundMaxLength;
			ControlDpiScalingHelper.SetLeft(ref result, ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterControlsBox1Start - SpaceBetweenLabelAndControl) - result.Width, false);
			result.BindTo = bindTo;
			result.BindToList = bindToList;
			FilterControlBindingSource.SetBindingMember(result, result.BindTo);

			return result;
		}

		public const string ComparisonOperatorBindToIdentifier = "ComparisonOperator";
		public const string ComparisonOperatorBindToListIdentifier = "ComparisonOperator_List";

		public int FilterComparisonOperatorBoxStart
		{
			get
			{
				if (fFilterComparisonOperatorBoxStart == -1)
				{
					using (var dropEdit = CreateComparisonOperatorDropList())
					{
						fFilterComparisonOperatorBoxStart = dropEdit.Left;
					}
				}
				return fFilterComparisonOperatorBoxStart;
			}
		}

		public const int FilterComparisonOperatorBoxPreBoundMaxLength = 7;
		int fFilterComparisonOperatorBoxStart = -1;

		#endregion

		#region Period Filter Controls

		Control[] GetPeriodFilterControls()
		{
			var operatorDropEdit = CreateComparisonOperatorDropList();
			operatorDropEdit.TabIndex = 1;

			var textBox = new ZPeriodEdit();
			ControlDpiScalingHelper.SetTop(ref textBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref textBox, FilterControlsBox1Start, true);
			ControlDpiScalingHelper.SetWidth(ref textBox, FilterControlBoxWidthSmall, false);
			textBox.TabIndex = 2;
			//textBox.BindTo = "Property";
			FilterControlBindingSource.SetBindingMember(textBox, "Property");

			return new Control[] { operatorDropEdit, textBox };
		}

		#endregion

		#region Date Filter Controls

		Control[] GetDateFilterControls()
		{
			var dateControl = new ZDateRangeControl(this) { TabIndex = 1 };
			FilterDescriptionDropEdit.AllowOverlap(dateControl);
			DeleteStripButton.AllowOverlap(dateControl);
			dateControl.AllowOutsideOfParent();
			return GetControlsHelper(dateControl);
		}

		Control[] GetSingleDateFilterControls()
		{
			var singleDateFilter = (ModuleSingleDateFilter)CurrentDataItem.CurrentModuleFilter;
			var singleDateControl = new ZSingleDateControl(this, singleDateFilter.DateTimeFormat) { TabIndex = 1 };
			FilterCategoriesToolStrip.AllowOverlap(singleDateControl);
			FilterDescriptionDropEdit.AllowOverlap(singleDateControl);
			FilterPropertyLockButton.AllowOverlap(singleDateControl);
			singleDateControl.AllowOutsideOfParent();
			return GetControlsHelper(singleDateControl);
		}

		#endregion

		#region Time Filter Controls

		Control[] GetTimeFilterControls()
		{
			var dateControl = new ZTimeRangeControl(this) { TabIndex = 1 };
			FilterDescriptionDropEdit.AllowOverlap(dateControl);
			GripHolderPanel.AllowOverlap(dateControl);
			dateControl.AllowOutsideOfParent();
			return GetControlsHelper(dateControl);
		}

		#endregion

		#region Filters Match Supporting Controls (Guid, NK, TextAndNk)

		const int DefaultControlPaddingUnscaled = 1;
		int DefaultControlPadding { get; } = ControlDpiScalingHelper.ScaleToCurrentDpiX(DefaultControlPaddingUnscaled);

		Control[] GetControlsForFilterWithSelectedFilters()
		{
			var moduleFilter = (IModuleFilterWithSelectedFilters)CurrentDataItem.CurrentModuleFilter;

			var result = new List<Control>();
			ZDropEdit operatorDropEdit = null;
			var currentTabIndex = 1;

			if (moduleFilter.HasComparisonOperator && !moduleFilter.IsComparisonOperatorAutomaticallySelected)
			{
				operatorDropEdit = CreateComparisonOperatorDropList();
				operatorDropEdit.TabIndex = currentTabIndex++;
				operatorDropEdit.Name = "OperatorDropEdit";
				operatorDropEdit.CodeBox.Text = moduleFilter.ComparisonOperator;
				result.Add(operatorDropEdit);
			}

			var moduleSpecifiedFilter = moduleFilter as ModuleGuidModuleSpecifiedFilter;
			var isModuleSpecifiedFilter = moduleSpecifiedFilter != null;
			ZDropEdit moduleDropEdit = null;

			if (isModuleSpecifiedFilter)
			{
				moduleDropEdit = CreateModuleDropEdit();
				moduleDropEdit.TabIndex = currentTabIndex++;
				result.Add(moduleDropEdit);
			}

			var findBox = CreateFindBox(moduleFilter, moduleDropEdit);
			findBox.TabIndex = currentTabIndex++;
			result.Add(findBox);

			ZTextBox textAndNkTextBox = null;

			var moduleTextAndNkFilter = moduleFilter as ModuleTextAndNkFilter;
			if (moduleTextAndNkFilter != null)
			{
				textAndNkTextBox = new ZTextBox();
				textAndNkTextBox.IsOnFilterStrip = true;
				ControlDpiScalingHelper.SetTop(ref textAndNkTextBox, FilterControlTop, false);
				ControlDpiScalingHelper.SetLeft(ref textAndNkTextBox, findBox.Right + DefaultControlPadding, false);
				ControlDpiScalingHelper.SetWidth(ref textAndNkTextBox, ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterControlBoxWidth) - findBox.Width, false);
				textAndNkTextBox.Name = "TextAndNkTextBox";
				textAndNkTextBox.TabIndex = currentTabIndex++;
				findBox.CodeBox.MaxLength = moduleTextAndNkFilter.NkMaxLength;
				FilterControlBindingSource.SetBindingMember(textAndNkTextBox, "Property");
				result.Add(textAndNkTextBox);

				LabelCaptionRenderProvider.SetLabelCaptionVisible(operatorDropEdit, false);
				LabelCaptionRenderProvider.SetLabelCaptionVisible(textAndNkTextBox, false);
				LabelCaptionRenderProvider.SetLabelCaptionVisible(findBox, false);
			}

			ZFilterCollectionFindBox filterFindBox = null;

			if (operatorDropEdit != null || moduleFilter.IsFilterCollectionComparisonOperatorSelected())
			{
				filterFindBox = CreateFilterCollectionFindBox(moduleFilter, findBox, textAndNkTextBox, canModuleChange: isModuleSpecifiedFilter);
				filterFindBox.TabIndex = currentTabIndex++;
				result.Add(filterFindBox);
			}

			if (isModuleSpecifiedFilter)
			{
				void ModuleSelectedHandler(object s, ModuleGuidModuleSpecifiedFilter.ModuleSelectedEventArgs e)
				{
					var oldFindBoxes = Controls.Find("PropertyFindBox", true).Where(b => b as ZCodeFindBox != null).Select(b => b as ZCodeFindBox);
					if (oldFindBoxes.Count() > 1)
					{
						var lines = oldFindBoxes.Select(b => "Parent=" + b.ParentForm + ",Module=" + b.ModuleID + ",Code=" + b.CurrentCode + ",Disposing=" + b.Disposing + ",Disposed=" + b.IsDisposed);
						ErrorReporter.ReportOnce(string.Join("|", lines));
					}

					var oldFindBox = oldFindBoxes.FirstOrDefault();
					if (oldFindBox != null)
					{
						if (ShouldSwapPropertyFindBox(e.OldModule, e.NewModule))
						{
							SwapPropertyFindBoxes(oldFindBox, moduleSpecifiedFilter);
						}
						else
						{
							oldFindBox.ModuleID = moduleSpecifiedFilter.ModuleId;
							oldFindBox.SetPopupButtonReadOnlyForNewlySelectedModule(moduleSpecifiedFilter);
						}
					}

					if (filterFindBox != null)
					{
						filterFindBox.ModuleID = moduleSpecifiedFilter.ModuleId;
					}
				}

				moduleSpecifiedFilter.ModuleSelected += ModuleSelectedHandler;
				Disposed += (s, e) => moduleSpecifiedFilter.ModuleSelected -= ModuleSelectedHandler;
			}

			var moduleFilterWithDropDown = moduleFilter as ModuleGuidInSubCollectionFilter;
			if (moduleFilterWithDropDown != null && moduleFilterWithDropDown.DropDownTypeNameList != null)
			{
				var dropDown = CreateGuidFilterDropDown();
				dropDown.TabIndex = currentTabIndex++;
				result.Add(dropDown);

				ControlDpiScalingHelper.SetLeft(result[1], result[0].Left, false);
				ControlDpiScalingHelper.SetWidth(result[1], 215, true);
				ControlDpiScalingHelper.SetLeft(result[0], result[1].Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(SpaceBetweenLabelAndControl), false);
				ControlDpiScalingHelper.SetLeft(ref dropDown, result[0].Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(SpaceBetweenLabelAndControl), false);

				var findBoxIndex = operatorDropEdit.TabIndex;

				findBox.TabIndex = findBoxIndex;
				filterFindBox.TabIndex = findBoxIndex + 1;
				operatorDropEdit.TabIndex = findBoxIndex + 2;
			}

			return result.ToArray();
		}

		static bool ShouldSwapPropertyFindBox(ModuleIdentifier oldModule, ModuleIdentifier newModule)
		{
			return (Equals(oldModule, ModuleIDs.Organisation) && !Equals(newModule, ModuleIDs.Organisation)) || (!Equals(oldModule, ModuleIDs.Organisation) && Equals(newModule, ModuleIDs.Organisation));
		}

		void SwapPropertyFindBoxes(ZCodeFindBox oldFindBox, ModuleGuidModuleSpecifiedFilter moduleSpecifiedFilter)
		{
			var parent = oldFindBox.Parent;
			var newFindBox = CreateFindBox(moduleSpecifiedFilter, oldFindBox.Left, oldFindBox.Width);
			newFindBox.Visible = oldFindBox.Visible;
			newFindBox.TabIndex = oldFindBox.TabIndex;
			FilterControlBindingSource.SetBindingMember(oldFindBox, null);
			parent.Controls.Remove(oldFindBox);
			parent.Controls.Add(newFindBox);
			oldFindBox.Dispose();
		}

		ZDropEdit CreateModuleDropEdit()
		{
			var drop = new ZDropEdit();
			drop.ShowDescriptionBox = false;
			drop.CodeBox.CharacterCasing = CharacterCasing.Normal;
			ControlDpiScalingHelper.SetTop(ref drop, FilterControlTop, false);
			drop.PreBoundMaxLength = FilterComparisonOperatorBoxPreBoundMaxLength;
			ControlDpiScalingHelper.SetLeft(ref drop, FilterControlsBox1Start + DefaultControlPaddingUnscaled, true);
			drop.Name = "ModuleDropEdit";
			drop.BindTo = "SelectedModule";
			drop.BindToList = "ModuleOptions";
			FilterControlBindingSource.SetBindingMember(drop, drop.BindTo);

			return drop;
		}

		ZCodeFindBox CreateFindBox(IModuleFilterWithSelectedFilters moduleFilter, Control lastAddedControl)
		{
			var left = lastAddedControl == null ? ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterControlsBox1Start) : lastAddedControl.Right + DefaultControlPadding;
			var width = ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterControlBoxWidth) - (lastAddedControl == null ? 0 : lastAddedControl.Width + (DefaultControlPadding * 2));

			return CreateFindBox(moduleFilter, left, width);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "There are property names for binding")]
		ZCodeFindBox CreateFindBox(IModuleFilterWithSelectedFilters moduleFilter, int left, int width)
		{
			var findBox = CreateFindBox(moduleFilter);
			var isTextAndNk = moduleFilter is ModuleTextAndNkFilter;
			var propertyToBind = isTextAndNk ? "NkProperty" : "Property";
			findBox.ShowDescriptionBox = !isTextAndNk;
			findBox.ShouldResize = !isTextAndNk;
			ControlDpiScalingHelper.SetTop(ref findBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref findBox, left, false);
			findBox.BindTo = propertyToBind;
			findBox.BindToList = "List";
			findBox.Name = "PropertyFindBox";
			ControlDpiScalingHelper.SetWidth(ref findBox, width, false);
			FilterControlBindingSource.SetBindingMember(findBox, propertyToBind);

			if (moduleFilter is ModuleGuidFilter guidFilter)
			{
				findBox.CodeBox.TextChanged += delegate { guidFilter.PropertyCode = findBox.CodeBox.Text; };
			}

			return findBox;
		}

		ZFilterCollectionFindBox CreateFilterCollectionFindBox(IModuleFilterWithSelectedFilters moduleFilter, ZCodeFindBox findBox, ZTextBox textAndNkTextBox, bool canModuleChange)
		{
			var filterFindBox = canModuleChange
				? new ZFilterCollectionFindBoxForVariableModuleFilter(moduleFilter, isPopupButtonEnabledWhenReadOnly: true, parentType: GetParentType(), parentModuleID: GetParentModuleID())
				: new ZFilterCollectionFindBox(moduleFilter, isPopupButtonEnabledWhenReadOnly: true, parentType: GetParentType(), parentModuleID: GetParentModuleID());
			filterFindBox.ShowDescriptionBox = true;
			ControlDpiScalingHelper.SetTop(ref filterFindBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref filterFindBox, findBox.Left, false);
			filterFindBox.BindTo = nameof(moduleFilter.SelectedFilters);
			filterFindBox.BindToList = "List";
			filterFindBox.BindToForDescription = nameof(moduleFilter.SelectedFiltersDescription);
			filterFindBox.Name = FilterCollectionFindBoxName;
			ControlDpiScalingHelper.SetWidth(ref filterFindBox, findBox.Width, false);
			FilterControlBindingSource.SetBindingMember(filterFindBox, nameof(moduleFilter.SelectedFilters));

			void UpdateSelectedFilterDescription()
			{
				filterFindBox.DescriptionBox.Text = moduleFilter.SelectedFiltersDescription;
			}

			void OperatorChangedHandler(object s, EventArgs e)
			{
				var findBoxToSet = this.FindSingleOrDefault<ZCodeFindBox>(x => x.Name == "PropertyFindBox");

				if (findBoxToSet != null)
				{
					var textAndNkTextBoxToSet = this.FindSingleOrDefault<ZTextBox>(x => x.Name == "TextAndNkTextBox");
					SetGuidFindBoxVisibilities(new Control[] { findBoxToSet, textAndNkTextBoxToSet }, filterFindBox, moduleFilter);
					UpdateSelectedFilterDescription();
				}
			}

			moduleFilter.ComparisonOperatorChanged += OperatorChangedHandler;
			filterFindBox.Disposed += (s, e) => moduleFilter.ComparisonOperatorChanged -= OperatorChangedHandler;

			void SelectedFiltersChangedHandler(object s, EventArgs e)
			{
				if (!IsDisposing)
				{
					using (filterFindBox.SuspendSelectedFiltersChangedFiring())
					{
						UpdateSelectedFilterDescription();
					}
				}
			}

			moduleFilter.SelectedFiltersChanged += SelectedFiltersChangedHandler;
			filterFindBox.Disposed += (s, e) => moduleFilter.SelectedFiltersChanged -= SelectedFiltersChangedHandler;

			SetGuidFindBoxVisibilities(new Control[] { findBox, textAndNkTextBox }, filterFindBox, moduleFilter);

			return filterFindBox;
		}

		const string FilterCollectionFindBoxName = "FilterCollectionFindBox";

		static void SetGuidFindBoxVisibilities(IEnumerable<Control> nonFiltersMatchControls, Control filterFindBox, IModuleFilterWithSelectedFilters moduleFilter)
		{
			var useFilterMatchMode = moduleFilter.IsFilterCollectionComparisonOperatorSelected();
			filterFindBox.Visible = useFilterMatchMode;

			foreach (var control in nonFiltersMatchControls.WhereNotNull())
			{
				control.Visible = !useFilterMatchMode;
			}
		}

		ZDropEdit CreateGuidFilterDropDown()
		{
			var dropDown = new ZDropEdit();
			dropDown.CharacterCasing = CharacterCasing.Normal;
			dropDown.CodeBox.TextAlign = HorizontalAlignment.Left;
			dropDown.ShowDescriptionBox = false;
			dropDown.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			ControlDpiScalingHelper.SetTop(ref dropDown, FilterControlTop, false);
			dropDown.PreBoundMaxLength = FilterComparisonOperatorBoxPreBoundMaxLength;
			dropDown.BindTo = ModuleGuidInSubCollectionFilter.Schema.DropDownTypeName;
			dropDown.BindToList = ModuleGuidInSubCollectionFilter.Schema.DropDownTypeNameList;
			dropDown.Name = "ModuleGuidInSubCollectionFilter_GuidFilterDropDown";
			FilterControlBindingSource.SetBindingMember(dropDown, dropDown.BindTo);

			return dropDown;
		}

		Control[] GetGuidsFilterControls()
		{
			var moduleFilter = (ModuleGuidsFilter)CurrentDataItem.CurrentModuleFilter;
			var label1 = moduleFilter.ItemDescription1 ?? Res.GetData("229BC68D-B98F-4000-93C8-7CFB976A61C6", "#1");
			var label2 = moduleFilter.ItemDescription2 ?? Res.GetData("B0BCFF0F-A3A4-4A39-AC85-AB6941B0A2CE", "#2");

			var findBox1 = CreateFindBox(moduleFilter);
			var findBox2 = CreateFindBox(moduleFilter);

			var width = CalculateNewWidthForCodeFindBox(MeasureLabel(label2.Caption));
			findBox1.SetWidth(width);
			findBox2.SetWidth(width);

			findBox1.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref findBox1, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref findBox1, FilterControlsBox1Start, true);
			findBox1.BindTo = "Property1";
			findBox1.BindToList = "List1";
			findBox1.CaptionResourceString = label1;
			findBox1.TabIndex = 2;
			findBox1.CodeBox.TextChanged += delegate { moduleFilter.PropertyCode1 = findBox1.CodeBox.Text; };
			FilterControlBindingSource.SetBindingMember(findBox1, findBox1.BindTo);

			findBox2.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref findBox2, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref findBox2, FilterControlsBox2Start(findBox2), true);
			findBox2.BindTo = "Property2";
			findBox2.BindToList = "List2";
			findBox2.CaptionResourceString = label2;
			findBox2.TabIndex = 4;
			findBox2.CodeBox.TextChanged += delegate { moduleFilter.PropertyCode2 = findBox2.CodeBox.Text; };
			FilterControlBindingSource.SetBindingMember(findBox2, findBox2.BindTo);

			return new Control[] { findBox1, findBox2 };
		}

		protected virtual ZCodeFindBox CreateFindBox(IModuleFilterWithModuleID moduleFilter)
		{
			ZCodeFindBox result;

			if (moduleFilter is ModuleNkFilter || moduleFilter is ModuleTextAndNkFilter)
			{
				result = new ZCodeFindBox { ModuleID = moduleFilter.ModuleId, ParentModuleID = GetParentModuleID(), ParentType = GetParentType() };
			}
			else if (Equals(moduleFilter.ModuleId, ModuleIDs.Organisation))
			{
				result = (ZGuidFindBox)Activator.CreateInstance(ObjectFactory.GetType("ZOrganisationFindBox"));
			}
			else
			{
				result = new ZGuidFindBox { ModuleID = moduleFilter.ModuleId, ParentModuleID = GetParentModuleID(), ParentType = GetParentType() };
			}

			return result;
		}

		public Type GetParentType()
		{
			var current = GetParentOfType<ZFilterStripCommonControl>(Parent);
			if (current != null)
			{
				return current.Grid?.ElementType;
			}
			return null;
		}

		public ModuleIdentifier GetParentModuleID()
		{
			var current = GetParentOfType<ZFilterStripCommonControl>(Parent);
			if (current != null)
			{
				return current.ParentModuleID;
			}
			return null;
		}

		public T GetParentOfType<T>(Control parent) where T : Control
		{
			var current = GetParentControlOfType<T>(parent);
			if (current is T)
			{
				return (T)current;
			}
			return null;
		}

		public Control GetParentControlOfType<T>(Control parent = null)
		{
			if (parent != null)
			{
				if (parent is T)
				{
					return parent;
				}
				return GetParentControlOfType<T>(parent.Parent);
			}
			return null;
		}

		#endregion

		#region Location Filter Controls

		Control[] GetLocationsFilterControls()
		{
			var moduleFilter = (ModuleFilterWithSubDescriptions)CurrentDataItem.CurrentModuleFilter;
			var label1 = moduleFilter.ItemDescription1 ?? Res.GetData("FF6E3F6E-5B16-453D-A499-B0124BA1F542", "Location #1");
			var label2 = moduleFilter.ItemDescription2 ?? Res.GetData("213AB122-3E5B-49CF-9CBA-7246AF3E4C37", "Location #2");

			var location1FindBox = new ZCodeFindBox();
			var location2FindBox = new ZCodeFindBox();

			var width = CalculateNewWidthForCodeFindBox(MeasureLabel(label2.Caption));
			location1FindBox.SetWidth(width);
			location2FindBox.SetWidth(width);

			location1FindBox.ModuleID = Modules.ModuleIDs.Location;
			location1FindBox.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref location1FindBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref location1FindBox, FilterControlsBox1Start, true);
			location1FindBox.BindTo = "Property1";
			location1FindBox.BindToList = "List1";
			location1FindBox.TabIndex = 2;
			location1FindBox.CaptionResourceString = label1;
			FilterControlBindingSource.SetBindingMember(location1FindBox, location1FindBox.BindTo);

			location2FindBox.ModuleID = Modules.ModuleIDs.Location;
			location2FindBox.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref location2FindBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref location2FindBox, FilterControlsBox2Start(location2FindBox), true);
			location2FindBox.BindTo = "Property2";
			location2FindBox.BindToList = "List2";
			location2FindBox.TabIndex = 4;
			location2FindBox.CaptionResourceString = label2;
			FilterControlBindingSource.SetBindingMember(location2FindBox, location2FindBox.BindTo);

			return new Control[] { location1FindBox, location2FindBox };
		}

		#endregion

		#region Flags Filter Controls

		Control[] GetFlagsFilterControls()
		{
			List<Control> result = null;
			const int checkBoxGap = 5;

			var moduleFilter = CurrentDataItem.CurrentModuleFilter as ModuleFlagsFilter;
			if (moduleFilter != null)
			{
				result = new List<Control>();

				var checkBoxes = new ZCheckBox[moduleFilter.FlagNames.Length];

				var nextLeft = ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterControlsBox1Start);
				var nextTop = LabelTop - ControlDpiScalingHelper.ScaleToCurrentDpiY(1);

				for (var i = 0; i < checkBoxes.Length; i++)
				{
					checkBoxes[i] = new ZCheckBox();
					var box = checkBoxes[i];
					box.UseCompatibleTextRendering = true;
					box.Font = Font;

					box.Text = moduleFilter.FlagNames[i];
					ControlDpiScalingHelper.SetWidth(ref box, box.PreferredSize.Width, false);
					ControlDpiScalingHelper.SetHeight(ref box, box.PreferredSize.Height, false);
					ControlDpiScalingHelper.SetTop(ref box, nextTop, false);
					ControlDpiScalingHelper.SetLeft(ref box, nextLeft, false);
					box.BindTo = "Property" + i;
					box.TabIndex = 1 + i;

					if (box.Right >= DeleteStripButton.Left)
					{
						var boxHeight = box.Height;

						nextTop += boxHeight;
						nextLeft = ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterControlsBox1Start);

						ControlDpiScalingHelper.SetTop(ref box, nextTop, false);
						ControlDpiScalingHelper.SetLeft(ref box, nextLeft, false);

						PreferredHeight += boxHeight;
					}
					nextLeft += box.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(checkBoxGap);
					FilterControlBindingSource.SetBindingMember(box, box.BindTo);
				}

				result.AddRange(checkBoxes);

				if (moduleFilter.ShowAddOrRadioBox)
				{
					AddAndOrRadioButtions(result, checkBoxes.Length);
				}
			}

			return result == null ? null : result.ToArray();
		}

		void AddAndOrRadioButtions(List<Control> controls, int index)
		{
			var minHeight = FilterControlBoxHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(25);
			if (PreferredHeight < minHeight)
			{
				PreferredHeight = minHeight;
			}

			var andRadioButton = new ZRadioButton
			{
				AutoCheck = false,
				AutoSize = true,
				BindTo = "AndJoinCondition",
				CaptionResourceString = Res.GetData("CAFBD2BB-A9C8-4471-9968-410C1EB1A640", "And"),
				FlatStyle = FlatStyle.System,
				Location = ControlDpiScalingHelper.NewScaledPoint(AddOrControlLeft, AddOrControlTop, false),
				Name = "ANDRadioButton",
				Size = ControlDpiScalingHelper.NewScaledSize(36, 17, true),
				TabIndex = index,
				TabStop = false,
				UseVisualStyleBackColor = true
			};
			FilterControlBindingSource.SetBindingMember(andRadioButton, andRadioButton.BindTo);
			controls.Add(andRadioButton);

			var orRadioButton = new ZRadioButton
			{
				AutoCheck = false,
				AutoSize = true,
				BindTo = "OrJoinCondition",
				CaptionResourceString = Res.GetData("75FF2A76-F16E-405A-A4FF-E0E6DD69D107", "Or"),
				FlatStyle = FlatStyle.System,
				Location = ControlDpiScalingHelper.NewScaledPoint(AddOrControlLeft + ControlDpiScalingHelper.ScaleToCurrentDpiX(45), AddOrControlTop, false),
				Name = "ORRadioButton",
				Size = ControlDpiScalingHelper.NewScaledSize(36, 17, true),
				TabIndex = index + 1,
				TabStop = false,
				UseVisualStyleBackColor = true
			};
			FilterControlBindingSource.SetBindingMember(orRadioButton, orRadioButton.BindTo);
			controls.Add(orRadioButton);
		}

		#endregion

		#region Get Controls Helper

		Control[] GetNumberRangeControls()
		{
			var numberRangeControl = new ZNumberRangeControl(this);
			DeleteStripButton.AllowOverlap(numberRangeControl);
			FilterPropertyLockButton.AllowOverlap(numberRangeControl);
			return GetControlsHelper(numberRangeControl);
		}

		#endregion

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		Control[] GetControlsHelper(Control control)
		{
			try
			{
				FilterControlBindingSource.SetBindingMember(control, ".");
				return new Control[] { control };
			}
			catch
			{
				try
				{
					control.Dispose();
				}
				catch { }
				throw;
			}
		}

		#region Text Range Controls

		Control[] GetTextRangeControls()
		{
			Control[] result = null;
			var moduleFilter = CurrentDataItem.CurrentModuleFilter as ModuleTextRangeFilter;

			if (moduleFilter != null)
			{
				var fromLabel = new ZLabel();
				var toLabel = new ZLabel();
				var fromEdit = new ZTextBox();
				fromEdit.IsOnFilterStrip = true;
				var toEdit = new ZTextBox();
				toEdit.IsOnFilterStrip = true;

				fromEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZFilterStrip|25D5821D-CD99-447d-83E9-BFED9B15E368", "From");
				ControlDpiScalingHelper.SetTop(ref fromEdit, FilterControlTop, false);
				ControlDpiScalingHelper.SetLeft(ref fromEdit, FilterControlsBox1Start, true);
				FilterControlBindingSource.SetBindingMember(fromEdit, "Property1");
				fromEdit.TabIndex = 2;
				ControlDpiScalingHelper.SetWidth(ref fromEdit, FilterControlBoxWidthSmall - ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStripDateEdit.CalendarButtonWidthZ), false);

				ControlDpiScalingHelper.SetWidth(ref toEdit, FilterControlBoxWidthSmall, false); // must be before calculating Label2Start() below..

				toEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZFilterStrip|B1DA7C5C-C69D-47e1-9718-9499AA8598EC", "To");
				ControlDpiScalingHelper.SetTop(ref toEdit, FilterControlTop, false);
				ControlDpiScalingHelper.SetLeft(ref toEdit, FilterControlsBox2Start(toEdit), true);
				FilterControlBindingSource.SetBindingMember(toEdit, "Property2");
				toEdit.TabIndex = 4;
				ControlDpiScalingHelper.SetWidth(toEdit, toEdit.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStripDateEdit.CalendarButtonWidthZ), false); // must be after calculating FilterControlsBox2Start()

				result = new Control[] { fromLabel, fromEdit, toLabel, toEdit };
			}

			return result;
		}

		#endregion

		#region SQL Filter Controls

		Control[] GetSqlFilterControls()
		{
			Control[] result = null;
			var moduleFilter = CurrentDataItem.CurrentModuleFilter as ModuleSQLFilter;

			if (moduleFilter != null)
			{
				var sqlEdit = new ZTextBox()
				{
					CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZFilterStrip|52665de2-bb78-4c00-998a-7ac187652160", "SQL Text"), // Sql is a real word it is not incorrectly spelt
					IsDynamicMultiline = true,
					CharacterCasing = CharacterCasing.Normal,
					AcceptsTab = true,
					TabIndex = 2,
					ReadOnly = this.ReadOnly,
					IsOnFilterStrip = true,
				};
				ControlDpiScalingHelper.SetTop(sqlEdit, FilterControlTop, false);
				ControlDpiScalingHelper.SetLeft(sqlEdit, FilterControlsBox1Start, true);
				ControlDpiScalingHelper.SetWidth(sqlEdit, FilterControlBoxWidth, true);
				FilterControlBindingSource.SetBindingMember(sqlEdit, "Property1");

				result = new Control[] { sqlEdit };
			}
			return result;
		}

		#endregion

		#region CodeFindBox calculations

		protected int CalculateNewWidthForCodeFindBox(int labelWidth)
		{
			return (ControlDpiScalingHelper.ScaleToCurrentDpiX(FilterControlBoxWidth) - (ControlDpiScalingHelper.ScaleToCurrentDpiX(2 * SpaceBetweenLabelAndControl) + labelWidth + FilterErrorProviderWidth)) / 2;
		}

		protected int MeasureLabel(string label)
		{
			SizeF size;
			bool truncated;
			StringRenderingHelper.MeasureBestFit(new string[] { label }, 1000, ZCodeFindBox.DefaultFont, ZCodeFindBox.DefaultFont.Height, StringRenderingOptions.Default, out size, out truncated);
			return (int)size.Width;
		}

		#endregion

		#region Adding / Removing Current Filter Controls

		void SetCurrentFilterControls(params Control[] filterControls)
		{
			RemoveCurrentFilterControls(filterControls);
			CurrentFilterControls.AddRange(filterControls);
			Controls.AddRange(filterControls);
			var parentFilterControl = ParentFilterControl;
			if (parentFilterControl != null && parentFilterControl.Parent != null)
			{
				parentFilterControl.Parent.Visible = true; // without this hack the controls are not visible. dunno why - Geoff & Clint
			}
		}

		void RemoveCurrentFilterControls()
		{
			RemoveCurrentFilterControls(Array.Empty<Control>());
		}

		void RemoveCurrentFilterControls(ICollection<Control> newControls)
		{
			foreach (var control in CurrentFilterControls)
			{
				// to repro a bug that will occuw without this begin invoke:
				// 1. type in a valid filter into the drop list, tab off (should show controls)
				// 2. click back on the drop list, erase the text - dont tab off, instead click one of the filter controls
				// 3. thread exception will occur because the Dispose() method is called but control focus/mouse events are not finished
				FilterControlBindingSource.SetBindingMember(control, "");
				if (!newControls.Contains(control))
				{
#if WINZOR
					ControlDispose(control);
#else
					System.Threading.SynchronizationContext.Current.Post(new System.Threading.SendOrPostCallback(ControlDispose), control);
#endif
				}
			}

			CurrentFilterControls.Clear();
		}

		public List<Control> CurrentFilterControls { get; private set; }

		void ControlDispose(object control)
		{
			((Control)control).Dispose();
		}

		#endregion

		#region Fonts

		Font StandardItemFont
		{
			get { return fStandardItemFont ?? (fStandardItemFont = new Font(OFont.NormalFontName, 8.0f)); }
		}

		Font ExclusiveItemFont
		{
			get { return fExclusiveItemFont ?? (fExclusiveItemFont = new Font(OFont.NormalFontName, 8.0f, FontStyle.Bold)); }
		}

		public Font ComparisonOperatorFont
		{
			get { return fComparisonOperatorFont ?? (fComparisonOperatorFont = new Font(OFont.NormalFontName, 8.0f, FontStyle.Italic)); }
		}

		Font fStandardItemFont;
		Font fExclusiveItemFont;
		Font fComparisonOperatorFont;

		#endregion

		#endregion

		#region 'X' button

		void DeleteStripButton_Click(object sender, EventArgs e)
		{
			OnFilterEdited();
			ParentFilterControl.DeleteFilterStrip(this);
		}

		internal StripControl ParentFilterControl
		{
			get
			{
				var parent = base.Parent;

				while (parent != null)
				{
					var parentFilterControl = parent as StripControl;
					if (parentFilterControl != null)
					{
						return parentFilterControl;
					}
					parent = parent.Parent;
				}

				return null;
			}
		}

		GroupStripControl ParentGroupFilterControl
		{
			get
			{
				var parent = base.Parent;
				while (parent != null)
				{
					var parentGroupFilterControl = parent as GroupStripControl;
					if (parentGroupFilterControl != null)
					{
						return parentGroupFilterControl;
					}
					parent = parent.Parent;
				}

				return null;
			}
		}

		#endregion

		#region Reset & Clear

		void FilterPropertyLockButtonClick(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.FilterPropertyLockStatus = !CurrentDataItem.FilterPropertyLockStatus;
				UpdateFilterPropertyLockIcon();
			}
		}

		public void ResetFilter()
		{
			CurrentDataItem.ResetFilter();
			if (ParentFilterControl is ZFilterStripControl)
			{
				((ZFilterStripControl)ParentFilterControl).Find();
			}
		}

		public void Clear()
		{
			if (CurrentDataItem != null && CurrentDataItem.CurrentModuleFilter != null && !CurrentDataItem.FilterPropertyLockStatus)
			{
				CurrentDataItem.CurrentModuleFilter.Clear();
			}
		}

		void UpdateFilterPropertyLockIcon()
		{
			FilterPropertyLockButton.BackgroundImage = Icons.GetImage(CurrentDataItem.FilterPropertyLockStatus ? IconTypes.BlackWhite_Lock : IconTypes.BlackWhite_Unlock);
			FilterPropertyLockButton.Size = ControlDpiScalingHelper.NewScaledSize(CurrentDataItem.FilterPropertyLockStatus ? 10 : 14, 18);
		}

		#endregion

		#region FilterEdited event

		public event EventHandler FilterEdited;

		protected void OnFilterEdited()
		{
			if (FilterEdited != null && !IsCurrentDataItemChanging)
			{
				FilterEdited(this, EventArgs.Empty);
			}
		}

		public event EventHandler FilterOrCategoryChanged;

		void OnFilterOrCategoryChanged()
		{
			if (FilterOrCategoryChanged != null)
			{
				FilterOrCategoryChanged(this, EventArgs.Empty);
			}
			OnFilterEdited();
		}

		#endregion

		#region IDataBoundControl Members

		public new FilterStrip CurrentDataItem
		{
			get { return (FilterStrip)base.CurrentDataItem; }
		}

		[DefaultValue(false)]
		bool IsCurrentDataItemChanging { get; set; }

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			IsCurrentDataItemChanging = true;
			try
			{
				base.OnCurrentDataItemChanging(e);
				if (CurrentDataItem != null)
				{
					CurrentDataItem.FilterDescriptionInfo.ValueChanged -= new EventHandler(FilterDescriptionInfo_ValueChanged);
					CurrentDataItem.OrCategoryChanged -= new EventHandler(BizO_OrCategoryChanged);
					UnHookValueChangedEvents();

					FilterControlBindingSource.SetDataBinding(null, "");
					if (CurrentDataItem != null)
					{
						CurrentDataItem.FilterDescription = ""; // ensures IsActive state is correct
					}
				}
			}
			finally
			{
				IsCurrentDataItemChanging = false;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.FilterDescriptionInfo.ValueChanged += new EventHandler(FilterDescriptionInfo_ValueChanged);
				CurrentDataItem.OrCategoryChanged += new EventHandler(BizO_OrCategoryChanged);

				DeleteStripButton.Visible = GripHolderPanel.Visible = !(CurrentDataItem.CurrentModuleFilter != null && CurrentDataItem.CurrentModuleFilter.Visibility == FilterVisibility.AlwaysVisible);
				UpdateFilterPropertyLockIcon();

				RebuildAndBindFilterControls();
			}
		}

		void VerifyAllControlsBound()
		{
			foreach (var control in CurrentFilterControls)
			{
				var dataBoundControl = control as IDataBoundControl;
				if (dataBoundControl != null &&
					string.IsNullOrEmpty(FilterControlBindingSource.GetBindingMember(control)) &&
					TypeDescriptor.GetAttributes(control)[typeof(SuppressFilterStripControlBoundCheckAttribute)] == null &&
					dataBoundControl.DataSource == null)
				{
					var controlName = !string.IsNullOrEmpty(control.Name) ? control.Name : control.GetType().Name;
					ErrorReporter.ReportOnce("FilterStripControlNotBound_" + controlName, "Filter strip control '" + controlName + "' was not bound.");
				}
			}
		}

		#endregion

		#region IStripControl Members

		void IStripControl.UpdateLayout(int width)
		{
			UpdateLayout(width);
		}

		public FilterOrCategory OrCategory
		{
			get { return this.CurrentDataItem.OrCategory; }
		}

		#endregion

		#region IReadOnlyToggleControl Members

		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				readOnly = value;

				IsDeleteButtonEnabled = !value;

				if (CurrentDataItem != null)
				{
					var isSqlFilter = CurrentDataItem.IsSqlFilter;
					var isUserDefinedFilter = CurrentDataItem.IsUserDefinedFilter;

					foreach (Control control in Controls)
					{
						SetControlEnabled(control, value, isSqlFilter, isUserDefinedFilter);
					}
				}
			}
		}

		bool readOnly;

		void SetControlEnabled(Control control, bool newReadOnlyValue, bool isSqlFilter, bool isUserDefinedFilter)
		{
			var findBox = control as ZFilterCollectionFindBox;

			if (isSqlFilter && control is ZTextBox textBox && textBox.IsDynamicMultiline)
			{
				// customSQL textBoxes should always able to be previewed
				textBox.ReadOnly = newReadOnlyValue;
				control.Enabled = true;
			}
			else if (isUserDefinedFilter && findBox != null)
			{
				// userdefined filters should be always able to be previewed, but don't need to be directly editable
				findBox.Enabled = true;
				findBox.ReadOnly = false;
			}
			else if (control.Name == FilterCollectionFindBoxName)
			{
				findBox?.SetReadOnlyWithEnabledButton();
			}
			else
			{
				control.Enabled = !newReadOnlyValue;
			}
		}

		#endregion
	}

	public class ColourProperties
	{
		public Color Colour;
		public string ColourName;
		public int Deviation;
	}

	public static class ColourConverter
	{
		public static ColourProperties ToKnownColour(this ColourProperties colourProperties, Color fromColour)
		{
			colourProperties.ColourName = "";
			foreach (var name in Enum.GetNames(typeof(KnownColor)).Where(x => Enum.GetNames(typeof(FilterOrCategory)).Contains(x)))
			{
				var knownColour = Color.FromName(name);
				var deviation = Math.Abs(fromColour.R - knownColour.R) + Math.Abs(fromColour.G - knownColour.G) + Math.Abs(fromColour.B - knownColour.B);

				if (string.IsNullOrEmpty(colourProperties.ColourName) || deviation < colourProperties.Deviation)
				{
					colourProperties.Colour = knownColour;
					colourProperties.ColourName = name;
					colourProperties.Deviation = deviation;
				}
			}

			return colourProperties;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public sealed class SuppressFilterStripControlBoundCheckAttribute : Attribute
	{
	}

	#region class ZFilterStripToolStripMenuItem

	public class ZFilterStripToolStripMenuItem : ZToolStripMenuItem
	{
		public bool IsChecked
		{
			get { return Font == SelectedFilterCategoryMenuItemFont; }
			internal set { Font = value ? SelectedFilterCategoryMenuItemFont : UnselectedFilterCategoryMenuItemFont; }
		}

		public Color Color
		{
			get { return fColor; }
			set { fColor = value; }
		}

		public FilterOrCategory OrCategory
		{
			get { return fOrCategory; }
			set { fOrCategory = value; }
		}

		Font UnselectedFilterCategoryMenuItemFont
		{
			get { return fUnselectedFilterCategoryMenuItemFont ?? (fUnselectedFilterCategoryMenuItemFont = new Font(OFont.NormalFontName, 8.25F)); }
		}

		Font SelectedFilterCategoryMenuItemFont
		{
			get { return fSelectedFilterCategoryMenuItemFont ?? (fSelectedFilterCategoryMenuItemFont = new Font(OFont.NormalFontName, 8.25F, FontStyle.Bold)); }
		}

		Font fUnselectedFilterCategoryMenuItemFont;
		Font fSelectedFilterCategoryMenuItemFont;
		Color fColor;
		FilterOrCategory fOrCategory;
	}

	#endregion

	#region Class ZFilterStripEventArgs

	public class ZFilterStripEventArgs : EventArgs
	{
		public ZFilterStripEventArgs(ZFilterStrip strip)
		{
			Strip = strip;
		}

		public readonly ZFilterStrip Strip;
	}

	#endregion
}
