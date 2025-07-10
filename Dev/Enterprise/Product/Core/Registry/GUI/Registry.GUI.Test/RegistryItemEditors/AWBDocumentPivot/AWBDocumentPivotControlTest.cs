using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class AWBDocumentPivotControlTest : TestCaseWithFactory
	{
		public void TestTitleColumnIsReadOnly()
		{
			using (var titleColumnIsNotReadOnlyContorl = new AWBDocumentPivotControl(false))
			{
				var grid = titleColumnIsNotReadOnlyContorl.Controls.Find("HAWBDocumentPivotGrid", true)[0] as ZGrid;
				Assert(!grid.GetColumnStyle("Title").IsReadOnly);
			}

			using (var titleColumnIsReadOnlyContorl = new AWBDocumentPivotControl(true))
			{
				var grid = titleColumnIsReadOnlyContorl.Controls.Find("HAWBDocumentPivotGrid", true)[0] as ZGrid;
				Assert(grid.GetColumnStyle("Title").IsReadOnly);
			}
		}
	}
}
