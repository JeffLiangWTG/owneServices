using System.Windows.Forms;
#if WINZOR
	using WinzorFramework;
#endif
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	#region DummyZFilterStripBaseControl

	public interface IFilterStripBaseControlForTest
	{
		ZLabel AutoRefreshWarningLabelExposed { get; }
		void SetShouldPerformSearch(bool shouldPerformSearch);
		void AddOrUpdateExistingFindDropListItemExposed(StmModuleFilter filter);
		void HandleFindButtonDropDownItemClickExposed(ToolStripItem item);
		ToolStripSplitButton ToolStripFindDropButtonExposed { get; }
		ToolStripSplitButton FindButtonExposed { get; }
		ToolStripButton ToolStripSaveLayoutButtonExposed { get; }
		ToolStripMenuItem ToolStripManageLayoutsButtonExposed { get; }
		CargoWise.GUI.TileBar.RecentItemsControl RecentItemsControlExposed { get; }
		void SetCanSaveColumnLayouts(bool canSaveColumnLayouts);
		bool CanSaveColumnLayoutsExposed { get; }
		void SetCanSaveGridColours(bool canSaveGridColours);
		bool CanSaveGridColoursExposed { get; }
		KPanel FilterStripsPanelExposed { get; }
		ZToolStrip ToolStripHelpExposed { get; }
		ToolStripButton ToolStripClearButtonExposed { get; }
		bool ProcessDialogKeyExposed(Keys keyData);
		ZGrid RelatedGrid { get; }
	}

	public class DummyZFilterStripBaseControl : ZFilterStripBaseControl, IFilterStripBaseControlForTest
	{
		public DummyZFilterStripBaseControl(ZGrid grid, FilterStripBusinessObject filterBusinessObject)
			: base(grid, filterBusinessObject)
		{
			RelatedGrid.Columns.AddTextColumn("Z0_Code", 80, true, false);
			RelatedGrid.Columns.AddTextColumn("Z0_Description", 80, true, false);
			RelatedGrid.Columns.AddCalcEditColumn("Z0_Number", 80, true, false, 0);

			BindingSource.DataSourceType = typeof(DummyBusinessObject);
			BindingSource.SetBindingMember(RelatedGrid, "FilteredCollection");
		}

		#region Expose

		public CargoWise.GUI.TileBar.RecentItemsControl RecentItemsControlExposed
		{
			get { return RecentItemsControl; }
		}

		public ZLabel AutoRefreshWarningLabelExposed
		{
			get { return AutoRefreshWarningLabel; }
		}

		public void SetShouldPerformSearch(bool shouldPerformSearch)
		{
			this.shouldPerformSearch = shouldPerformSearch;
		}
		bool shouldPerformSearch = true;

		protected override ZBool ShouldPerformSearch()
		{
			return shouldPerformSearch;
		}

		public void AddOrUpdateExistingFindDropListItemExposed(StmModuleFilter filter)
		{
			AddOrUpdateExistingFindDropListItem(filter);
		}

		public void HandleFindButtonDropDownItemClickExposed(ToolStripItem item)
		{
			HandleFindButtonDropDownItemClick(item);
		}

		public ToolStripSplitButton ToolStripFindDropButtonExposed
		{
			get { return ToolStripFindDropButton; }
		}

		public ToolStripSplitButton FindButtonExposed
		{
			get { return base.ToolStripFindDropButton; }
		}

		public ToolStripButton ToolStripSaveLayoutButtonExposed
		{
			get { return base.ToolStripSaveLayoutButton; }
		}

		public ToolStripMenuItem ToolStripManageLayoutsButtonExposed
		{
			get { return base.ToolStripManageLayoutsMenuItem; }
		}

		public bool CanSaveColumnLayoutsExposed
		{
			get { return CanSaveColumnLayouts; }
		}

		public bool CanSaveGridColoursExposed
		{
			get { return CanSaveGridColours; }
		}

		public KPanel FilterStripsPanelExposed
		{
			get { return FilterStripsPanel; }
		}

		public ZToolStrip ToolStripHelpExposed
		{
			get { return ToolStripHelp; }
		}

		public ToolStripButton ToolStripClearButtonExposed
		{
			get { return ToolStripClearButton; }
		}

		public bool ProcessDialogKeyExposed(Keys keyData)
		{
			return ProcessDialogKey(keyData);
		}

		protected override bool CanSaveColumnLayouts
		{
			get { return canSaveColumnLayouts; }
		}

		bool canSaveColumnLayouts = true;

		public void SetCanSaveColumnLayouts(bool canSaveColumnLayouts)
		{
			this.canSaveColumnLayouts = canSaveColumnLayouts;
		}

		protected override bool CanSaveGridColours
		{
			get { return canSaveGridColours; }
		}

		bool canSaveGridColours = true;

		public void SetCanSaveGridColours(bool canSaveGridColours)
		{
			this.canSaveGridColours = canSaveGridColours;
		}

		#endregion
	}

	#endregion
}
