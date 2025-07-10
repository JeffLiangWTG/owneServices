using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

/*
 * Wrapper control exists to allow adding to other controls via Designer. FilterRuleFilterStripControl doesn't appear.
 */
namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	public partial class FilterStripWrapperControl : ZUserControl
	{
		public FilterStripWrapperControl()
		{
			InitializeComponent();
		}

		public virtual string ControlIdentifier => "";

		protected FilterRuleFilterStripControl stripControl;

		public event EventHandler<ZFilterStripEventArgs> FilterStripAdding;

		public bool IsPreviewAllowed { get; set; }

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var filter = BoundFilter;
			if (filter != null && !filter.IsDeleted && stripControl == null)
			{
				SetupFilterStripControl(filter);
			}
		}

		protected StmModuleFilter BoundFilter => (StmModuleFilter)BindingSource.Current;

		protected ZString ModuleName => BoundFilter.S9_ModuleID;

		public int FilterStripsCount => stripControl?.FilterStripsCount ?? 0;

		void SetupFilterStripControl(StmModuleFilter filter)
		{
			stripControl = GetNewFilterStripControl();
			stripControl.Dock = DockStyle.Fill;

			RefreshPreviewButton(filter.S9_ModuleID);

			BindingSource.SetBindingMember(stripControl, ".");
			Controls.Add(stripControl);
			stripControl.SetDataBinding(filter, null);
			stripControl.FilterStripAdding += stripControl_FilterStripAdding;

			OnStripControlSetupComplete();
		}

		protected virtual CodeDescriptionPairList PreviewDropDownMenuItems => null;

		protected void RefreshPreviewButton(string moduleId)
		{
			if (IsPreviewAllowed)
			{
				stripControl.SetIsPreviewAllowed(moduleId, PreviewDropDownMenuItems);
			}
		}

		protected virtual FilterRuleFilterStripControl GetNewFilterStripControl()
		{
			return new FilterRuleFilterStripControl(GetModuleFilters(), NewFilterStripString, ControlIdentifier);
		}

		void stripControl_FilterStripAdding(object sender, ZFilterStripEventArgs e)
		{
			if (FilterStripAdding != null)
			{
				FilterStripAdding(sender, e);
			}
		}

		public event EventHandler StripControlSetupComplete;

		void OnStripControlSetupComplete()
		{
			StripControlSetupComplete?.Invoke(this, new EventArgs());
		}

		protected FilterStripBusinessObject GetModuleFilters()
		{
			var filter = BoundFilter;
			var filterBizo = RelatedModuleFiltersHelper.GetNewFilterBusinessObject(filter);
			((IFilterStripBusinessObjectInternals)filterBizo).LayoutContext = filter.S9_ModuleID;

			return (FilterStripBusinessObject)filterBizo;
		}

		protected virtual string NewFilterStripString
		{
			get { return string.Empty; }
		}

		public string FilterControlIdentifier { get; set; }

		public void RefreshStripControl()
		{
			stripControl.RefreshStripLayout();
			stripControl.RefreshGroupLayout();
		}
	}
}
