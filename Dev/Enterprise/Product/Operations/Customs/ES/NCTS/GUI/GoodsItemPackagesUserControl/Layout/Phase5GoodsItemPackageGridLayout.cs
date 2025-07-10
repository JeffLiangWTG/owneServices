using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public class Phase5GoodsItemPackageGridLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var gridColumnBag = PackageGridControlBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(gridColumnBag.SequenceNumberTextBox);
			builder.AddColumn(gridColumnBag.PackageTypeDropEdit);
			builder.AddColumn(gridColumnBag.NumberOfPackagesCalcEdit);
			builder.AddColumn(gridColumnBag.MarksAndNumbersTextBox);
			builder.AddColumn(gridColumnBag.PackageIDTextBoxColumn);
			builder.AddColumn(gridColumnBag.BrandTextBoxColumn);
			builder.AddColumn(gridColumnBag.ModelTextBoxColumn);

			return builder.Build();
		}

		#endregion
	}
}
