using System;
using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZSelectionColumnItemTemplateTest : ZItemTemplateTest
	{
		#region Overrides

		public void TestInstantiateIn()
		{
			Control container = new Control();
			TestItemTemplate.InstantiateIn(container);
			AssertEquals("Should be one control added", 1, container.Controls.Count);
			ZSelectionCheckBox checkBox = container.Controls[0] as ZSelectionCheckBox;
			AssertNotNull("Should be a ZSelectionCheckBox", checkBox);
			AssertSelectAllStatus(checkBox);
		}

		protected virtual void AssertSelectAllStatus(ZSelectionCheckBox checkBox)
		{
			AssertEquals("Should be not SelectAll", "Select/Deselect", checkBox.ToolTip);
		}

		public void TestGetControlDoesNotThrowException()
		{
			AssertNull(TestItemTemplate.GetControl());
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZSelectionColumn); }
		}

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZSelectionColumnItemTemplate); }
		}

		public new ZSelectionColumnItemTemplate TestItemTemplate
		{
			get { return base.TestItemTemplate as ZSelectionColumnItemTemplate; }
		}

		public new ZSelectionColumn TestColumn
		{
			get { return base.TestColumn as ZSelectionColumn; }
		}

		protected override ZTemplateColumn GetNewColumn(string header, string bindTo)
		{
			ZDataGrid grid = new ZDataGrid();
			return new ZSelectionColumn(grid);
		}

		#endregion
	}
}
