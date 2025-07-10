using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Testing
{
	public class ZGridCustomiseTester : ZGridCustomise
	{
		public ZGridCustomiseTester(ZGridColumns currentColumns, ZGridColumns defaultColumns, ZGrid grid)
			: base(currentColumns, defaultColumns, true, GetCustomiseBizObj(grid)) { }

		public ZGridCustomiseTester(ZGridColumns currentColumns, ZGridColumns defaultColumns, ZGridCustomiseBizObj customiseBizO)
			: base(currentColumns, defaultColumns, true, customiseBizO) { }

		public new void SaveColumns()
		{
			base.SaveColumns();
		}

		static ZGridCustomiseBizObj GetCustomiseBizObj(ZGrid grid)
		{
			return new ZGridCustomiseBizObj(new string[] { "1" }, new string[] { "1" }, null, ZGuid.Empty);
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

		public ToolStripButton ManageLayoutButton
		{
			get { return ToolStripManageLayoutsButton; }
		}

		public ToolStripButton SaveLayoutButton
		{
			get { return ToolStripSaveLayoutButton; }
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

		public void CanAddColumnExposed(ICustomizableColumn column)
		{
			CanAddColumn(column);
		}

		public SaveLayoutBizO SaveLayoutBizObjExposed;

		protected override SaveLayoutBizO GetSaveLayoutBizOAfterQueryingUser(IModifyModuleAndGridLayout gridLayoutManageable)
		{
			return SaveLayoutBizObjExposed ?? base.GetSaveLayoutBizOAfterQueryingUser(gridLayoutManageable);
		}

		public IModifyModuleAndGridLayout GetGridLayoutManageableExposed()
		{
			return GetGridLayoutManageable();
		}

		public void SearchColumnExposed(string text)
		{
			SetSearchText(text);
			ColumnSearchBox_TextChanged(null, null);
		}

		public ZButton MoveUpButtonExposed => MoveUpButton;

		public ZButton MoveDownButtonExposed => MoveDownButton;

		public ZButton PostButtonExposed => PostButton;

		public ZButton CancelButtonExposed => Cancel_Button;

		public ZButton ResetButtonExposed => ResetButton;
	}
}
