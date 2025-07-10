using System;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZCalcEditColumnEditItemTemplateTest : ZItemTemplateTest
	{
		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZCalcEditColumnEditItemTemplate); }
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZCalcEditColumn); }
		}

		#region Implementation

		public void TestRightAlignmentForNumberCells()
		{
			using (TableCell cell = new TableCell())
			{
				TestItemTemplate.InstantiateIn(cell);
				AssertNotNull("Precondition: Horizontal Align set", cell.HorizontalAlign);
				AssertEquals("Right alignment", HorizontalAlign.Right, cell.HorizontalAlign);
			}
		}

		#endregion
	}
}
