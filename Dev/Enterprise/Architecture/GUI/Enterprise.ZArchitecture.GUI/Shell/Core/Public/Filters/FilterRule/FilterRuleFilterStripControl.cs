using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class FilterRuleFilterStripControl : StripControl
	{
		public FilterRuleFilterStripControl(FilterStripBusinessObject filterStrip, string newFilterStripString, string filterControlIdentifier)
			: base(filterStrip)
		{
			InitializeComponent();
			HideControlsWeDontNeed();
			NewFilterStripString = newFilterStripString;
			MoveControlsToSplitContainer();
			FilterStripsPanel.Dock = DockStyle.Fill;
			FilterControlIdentifier = filterControlIdentifier;
			ToolStrip.AllowOverlap(CoveringLabel);
			ToolStripHelp.AllowOverlap(CoveringLabel);
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);

			ReadOnly = !Enabled;
		}

		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				readOnly = value;

				SetToolStripControlEnabledProperty(value);
				SetAllSubFilterStripsEnabledProperty(value);
				UpdateLayout();
			}
		}

		bool readOnly;

		void SetToolStripControlEnabledProperty(bool newReadOnlyValue)
		{
			foreach (var control in ToolStripControls.Where(x => x != ToolStrip))
			{
				control.Enabled = !newReadOnlyValue;
			}

			foreach (ToolStripItem item in ToolStrip.Items)
			{
				item.Enabled = CalculateToolStripItemsEnabledValue(newReadOnlyValue, item);
			}
		}

		bool CalculateToolStripItemsEnabledValue(bool newReadOnlyValue, ToolStripItem item)
		{
			return item == ToolStripPreviewDropButton || !newReadOnlyValue;
		}

		void SetAllSubFilterStripsEnabledProperty(bool newReadOnlyValue)
		{
			SetFirstLayerFilterStripsEnabledProperty(newReadOnlyValue);
			SetGroupFilterStripsEnabledProperty(newReadOnlyValue);
		}

		void SetFirstLayerFilterStripsEnabledProperty(bool newReadOnlyValue)
		{
			var strips = FilterStripControls.SelectMany(c => c.Controls.OfType<ZFilterStrip>());
			SetFilterStripsEnabledProperty(newReadOnlyValue, strips);
		}

		void SetGroupFilterStripsEnabledProperty(bool newReadOnlyValue)
		{
			var groupStrips = FilterStripControls.SelectMany(c => c.Controls.OfType<GroupStripControl>());
			var subStrips = groupStrips.SelectMany(c => c.FilterStripGroupBox.Controls.OfType<ZFilterStrip>());
			SetFilterStripsEnabledProperty(newReadOnlyValue, subStrips);
		}

		void SetFilterStripsEnabledProperty(bool newReadOnlyValue, IEnumerable<ZFilterStrip> strips)
		{
			foreach (var strip in strips)
			{
				if (strip.ReadOnly != newReadOnlyValue)
				{
					strip.ReadOnly = newReadOnlyValue;
				}
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			UpdateLayout(); // in case ReadOnly and Visible are changed together.
		}

		void MoveControlsToSplitContainer()
		{
			foreach (var control in FilterStripControls)
			{
				Controls.Remove(control);
				SplitContainer.Panel1.Controls.Add(control);
			}

			foreach (var control in ToolStripControls)
			{
				Controls.Remove(control);
				SplitContainer.Panel2.Controls.Add(control);
				ControlDpiScalingHelper.SetTop(control, 0, false);
			}
		}

		protected KSplitContainer SplitContainer;
		readonly string NewFilterStripString;
		readonly string FilterControlIdentifier;

		protected override int ToolStripTop => 0;

		protected override int ToolStripGroupTop => 0;

		protected override int GetStripControlWidth(IStripControl control)
		{
			return Width - (control is GroupStripControl ? 0 : ControlDpiScalingHelper.ScaleToCurrentDpiX(PanelGapWidth));
		}

		protected override GroupStripControl GetNewGroupStripControl()
		{
			return new NarrowParentGroupStripControl();
		}

		protected override void UpdateAdditionalToolStripLayout()
		{
			base.UpdateAdditionalToolStripLayout();

			if (SplitContainer != null)
			{
				var min = SplitContainer.Panel1MinSize;
				var max = SplitContainer.Height - SplitContainer.Panel2MinSize;
				var distance = GroupControls != null && GroupControls.Any() ? LastGroupControlBottomPlusPadding : LastFilterStripBottom;
				distance = Math.Max(distance, min);
				distance = Math.Min(distance, max);

				if (distance >= min && distance <= max)
				{
					SetSplitterDistance(distance);
				}
			}
		}

		protected virtual void SetSplitterDistance(int distance)
		{
			SplitContainer.SplitterDistance = distance;
		}

		public int FilterStripsCount
		{
			get { return this.FilterBusinessObject.FilterStrips.Count; }
		}

		#region Control Customisation

		void HideControlsWeDontNeed()
		{
			ToolStripSaveLayoutButton.Visible = false;
			ToolStripManageDropButton.Visible = false;
			ToolStripFindDropButton.Visible = false;

			HideAllButResetAndAddGroupToolStripButtons();
		}

		protected internal override ZFilterStrip NewZFilterStrip()
		{
			if (NewFilterStripString.IsNullOrEmpty())
			{
				return base.NewZFilterStrip();
			}
			else
			{
				return (ZFilterStrip)ObjectFactory.Get(NewFilterStripString);
			}
		}

		protected override int MaxFilterStripPanelHeight
		{
			get
			{
				var baseHeight = base.MaxFilterStripPanelHeight;
				var gap = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(60);

				if (Height > baseHeight && Height > gap ||
					Height > CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(174) && Height < baseHeight)
				{
					return Height - gap;
				}
				return baseHeight;
			}
		}

		protected override void StripControl_ResizeCore()
		{
			if (readOnly)
			{
				this.SetReadOnly(true);
			}
			else
			{
				UpdateLayout();
			}
		}

		#endregion

		#region Binding

		public StmModuleFilter Source
		{
			get { return BindingSource.Current as StmModuleFilter; }
		}

		ZString previewModuleId;

		public void SetIsPreviewAllowed(ZString moduleId, CodeDescriptionPairList dropDownItems = null)
		{
			previewModuleId = moduleId;
			ToolStripPreviewDropButton.Visible = true;
			ToolStripPreviewDropButton.Click -= ToolStripPreviewDropButton_ButtonClick;
			ToolStripPreviewDropButton.DropDownItems.Clear();

			if (dropDownItems == null)
			{
				ToolStripPreviewDropButton.Click += ToolStripPreviewDropButton_ButtonClick;
				ToolStripPreviewDropButton.ShowDropDownArrow = false;
			}
			else
			{
				ToolStripPreviewDropButton.ShowDropDownArrow = true;

				foreach (CodeDescriptionPair itemDefinition in dropDownItems)
				{
					var item = new ZToolStripMenuItem(itemDefinition.Description, PreviewDropDownItem_Click)
					{
						Image = Icons.GetImage(IconTypes.FindButtonRest),
						Tag = itemDefinition.Code,
					};
					ToolStripPreviewDropButton.DropDownItems.Add(item);
				}
			}
		}

		void ToolStripPreviewDropButton_ButtonClick(object sender, EventArgs e)
		{
			if (!ToolStripPreviewDropButton.ShowDropDownArrow)
			{
				OnPreviewClicked();
			}
		}

		void PreviewDropDownItem_Click(object sender, EventArgs e)
		{
			var control = (ToolStripMenuItem)sender;
			OnPreviewClicked((string)control.Tag);
		}

		protected virtual void OnPreviewClicked(string dropDownCode = null)
		{
			var moduleID = ModuleIDs.AllExcludingClientModules.FirstOrDefault(m => m.Name.Equals(previewModuleId));

			if (moduleID != null)
			{
				SaveLayout();

				var loadFilterStrips = (Parent as FilterStripWrapperControl)?.FilterControlIdentifier != FilterControlIdentifier;
				BusinessObjectModulePicker.ShowModuleScreen(moduleID,
					module => SetAdditionalPreviewFilter(module, dropDownCode),
					BusinessObjectModulePicker.FilterLayoutStrategy.TransientLayoutStrategy(GetCurrentLayout, loadFilterStrips),
					isSelectionMandatory: false);
			}
		}

		void SetAdditionalPreviewFilter(ZFilterModule module, string dropDownCode)
		{
			var zForm = FindForm() as ZForm;
			BusinessObject dataSource = null;
			if (zForm != null)
			{
				var withSubObject = zForm as IFilterPreviewableWithSubObject;
				dataSource = withSubObject == null ? zForm.DataSource as BusinessObject : withSubObject.GetObjectForPreview(((FilterStripWrapperControl)Parent).FilterControlIdentifier);
			}

			var filterBizo = module.FilterBusinessObject;
			filterBizo.SearchType = FilterBusinessObject.SearchType;
			var additionalQuery = filterBizo.GetAdditionalPreviewFilter(dataSource, module.ID.Name, dropDownCode);

			module.AddAdditionalDisplayFilter = (query) =>
			{
				if (additionalQuery != null)
				{
					query.AddToFilter(additionalQuery);
				}
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Temporary layout just to pass values to the module form")]
		StmModuleFilter GetCurrentLayout()
		{
			var result = new BusinessObjectFactory().New<StmModuleFilter>();
			result.S9_FilterName = "Current Selection Layout - " + ZDateTime.Now.Ticks;
			result.S9_ModuleID = previewModuleId;

			SerialiseFilterCustomisation(result, new FilterStripLayoutsHelper());
			return result;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			BindFilters();
			SetReadOnlyIfRequired();
			UpdateToolStripLayout(Strips);
		}

		void SetReadOnlyIfRequired()
		{
			var parent = ParentForm as IZForm ?? FindForm() as IZForm ?? this.GetRootContainer() as IZForm;

			if (parent != null && parent.DisplayMode == ODisplayMode.ReadOnly)
			{
				ReadOnly = true;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			BindFilters();
		}

		protected override void FilterEdited()
		{
			base.FilterEdited();

			if (boundFilter != null)
			{
				boundFilter.HasChanges = true;
				SerialiseFilterCustomisation();
			}
		}

		protected override void OnFilterStripDeleted()
		{
			FilterEdited();
		}

		protected virtual void BindFilters()
		{
			if (this.AlreadyLoaded && boundFilter != Source)
			{
				if (boundFilter != null)
				{
					SerialiseFilterCustomisation();
				}

				boundFilter = Source;

				LoadFilterRuleLayout();
				FilterBusinessObject.ResetLastUsedLayout(); // Never save the last used layout.

				if (boundFilter != null && boundFilter.ReadOnly)
				{
					this.SetReadOnly(true);
				}
			}
		}

		StmModuleFilter boundFilter;

		void LoadFilterRuleLayout()
		{
			var layout = FilterBusinessObject as IRelatedModuleFilterBusinessObject;
			if (layout != null)
			{
				layout.LoadFilterRuleLayout(boundFilter);
			}
			else
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unknown Filter Business Object Type. Please ensure your FilterStripBusienssObject implements {0}.", nameof(IRelatedModuleFilterBusinessObject)));
			}
		}

		protected override void FilterReset()
		{
			FilterEdited();
		}

		#endregion

		#region Serialisation

		public void SerialiseFilterCustomisation()
		{
			SerialiseFilterCustomisation(boundFilter);
		}

		void SerialiseFilterCustomisation(StmModuleFilter source, FilterStripLayoutsHelper helperOverride = null)
		{
			if (source != null && !source.IsDeleted)
			{
				var helper = FilterBusinessObject.LayoutsHelper;

				try
				{
					if (helperOverride != null)
					{
						FilterBusinessObject.LayoutsHelper = helperOverride;
					}

					((IModifyModuleAndGridLayout)FilterBusinessObject).SerialiseLayoutAndWriteTo(source);
				}
				finally
				{
					if (helperOverride != null)
					{
						FilterBusinessObject.LayoutsHelper = helper;
					}
				}
			}
		}

		#endregion
	}
}
