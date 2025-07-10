using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Grid.Testing
{
	[SuppressFormDesignerAnalysis]
	sealed class TestFormForGridRowFinder : ZForm
	{
		public TestFormForGridRowFinder(DummyBusinessObject bizo)
			: base(bizo)
		{
			Grid = new ZGrid();
			ZTextBoxColumnStyleInfo info1 = new ZTextBoxColumnStyleInfo();
			info1.Caption = "Test";
			info1.ColumnName = DummyBizoSchema.Z0_VarCharMax.Name;

			ZTextBoxColumnStyleInfo info2 = new ZTextBoxColumnStyleInfo();
			info2.Caption = "Test2";
			info2.ColumnName = DummyBizoSchema.Z0_Description.Name;

			ZTextBoxColumnStyleInfo infoMultilingual = new ZTextBoxColumnStyleInfo();
			infoMultilingual.Caption = "MultilingualTest";
			infoMultilingual.ColumnName = "MultilingualTest";

			var guidInfo = new ZGuidDropEditColumnStyleInfo { ColumnName = "Z0_Guid", Caption = "LOL" };

			Grid.ColumnStyles.Add(info1);
			Grid.ColumnStyles.Add(info2);
			Grid.ColumnStyles.Add(infoMultilingual);
			Grid.ColumnStyles.Add(guidInfo);

			Controls.Add(Grid);
			Grid.BindTo = "Collection";
			Grid.SetDataBinding(bizo, "Collection");
		}

		public readonly ZGrid Grid;

		protected override void Dispose(bool disposing)
		{
			if (Grid != null)
			{
				Grid.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
