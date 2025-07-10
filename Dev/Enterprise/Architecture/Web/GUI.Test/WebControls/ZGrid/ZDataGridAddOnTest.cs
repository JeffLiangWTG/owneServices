using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public abstract class ZDataGridAddOnTest : WebControlTest
	{
		#region Test Cases

		public void TestGrid()
		{
			AssertNotNull(Control);
			ZDataGrid testGrid = new ZDataGrid();

			AssertNull(Control.Grid);
			Control.Grid = testGrid;
			AssertNotNull(Control.Grid);
			AssertEquals(testGrid, Control.Grid);
		}

		#endregion

		#region Implementation

		protected new ZDataGridAddOn Control
		{
			get
			{
				return (ZDataGridAddOn)base.Control;
			}
		}

		#endregion
	}
}
