using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class ColorSelectorColumnStyleTest : TestCaseWithFactory
	{
		[GuiTest]
		public void TestPaintMethodDoesntThrowInvalidCastException()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var newDummy = dummy.Collection.AddNew();
			newDummy.Z0_VarCharMax = "12,23,34";

			using (var form = new ZForm(dummy))
			{
				var grid = new ZGrid { BindTo = "Collection" };
				grid.Columns.Add(new ColorSelectorColumnStyleInfo { ColumnName = "Z0_VarCharMax" });

				Assert("Ensure we have the right column", grid.Columns[0].ColumnStyle is ColorSelectorColumnStyle);

				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);

				form.Show();

				//Exception will be thrown and caught by the ErrorReporter, so the test will still fail if there is any problems.
				Application.DoEvents();
			}
		}
	}
}
