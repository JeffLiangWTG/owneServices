using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZColumnsCustomiseTester : ZColumnsCustomise
	{
		public ZColumnsCustomiseTester(IList<ICustomizableColumn> currentColumns, IList<ICustomizableColumn> defaultColumns = null, bool showLayoutsToolstrip = false, ZGridCustomiseBizObj customiseBizObj = null, ZGrid parentGrid = null)
			: base(currentColumns, defaultColumns, showLayoutsToolstrip, customiseBizObj, parentGrid) { }

		public void SearchColumnExposed(string text)
		{
			SetSearchText(text);
			ColumnSearchBox_TextChanged(null, null);
		}

		public void SaveColumnsExposed()
		{
			SaveColumns();
		}

		public void ResetColumnsExposed()
		{
			ResetColumns();
		}

		public void RemoveColumnsExposed()
		{
			RemoveColumns();
		}

		public void MoveColumnUpExposed()
		{
			MoveColumnUp();
		}

		public void MoveColumnDownExposed()
		{
			MoveColumnDown();
		}

		public void AddColumnExposed()
		{
			AddColumns();
		}

		public void SetCustomColumnsCheckbox(bool value)
		{
			CustomColumnsCheckBox.Checked = value;
		}

		public void UpdateCustomColumnsVisibilityExposed()
		{
			UpdateCustomColumnsVisibility();
		}

		public ListBox.ObjectCollection AvailableColumns
		{
			get { return AvailableColumnsListBox.Items; }
		}

		public ZListBox AvailableColumnsListBoxExposed
		{
			get { return AvailableColumnsListBox; }
		}

		public ListBox.ObjectCollection SelectedColumns
		{
			get { return CurrentColumnsListBox.Items; }
		}

		public ZListBox CurrentColumnsListBoxExposed
		{
			get { return CurrentColumnsListBox; }
		}

		public bool CustomColumnsCheckBoxCheckedExposed
		{
			get { return CustomColumnsCheckBox.Checked; }
		}

		protected override SaveLayoutBizO GetSaveLayoutBizOAfterQueryingUser(IModifyModuleAndGridLayout gridLayoutManageable)
		{
			return SaveLayoutBizObjExposed ?? base.GetSaveLayoutBizOAfterQueryingUser(gridLayoutManageable);
		}
		public SaveLayoutBizO SaveLayoutBizObjExposed;

		public ToolStripButton ToolStripManageLayoutsButtonExposed
		{
			get { return ToolStripManageLayoutsButton; }
		}

		public ToolStripButton ToolStripSaveLayoutButtonExposed
		{
			get { return ToolStripSaveLayoutButton; }
		}

		public IModifyModuleAndGridLayout GetGridLayoutManageableExposed()
		{
			return GetGridLayoutManageable();
		}

		public ZButton CancelButtonExposed
		{
			get { return Cancel_Button; }
		}

		public ZButton PostButtonExposed
		{
			get { return PostButton; }
		}

		public ZButton ResetButtonExposed
		{
			get { return ResetButton; }
		}

		public ZButton MoveUpButtonExposed
		{
			get { return MoveUpButton; }
		}

		public ZButton MoveDownButtonExposed
		{
			get { return MoveDownButton; }
		}

		public ZGrid ParentGridExposed
		{
			get { return ParentGrid; }
		}
	}
}
