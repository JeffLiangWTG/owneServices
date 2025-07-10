using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class Phase5GoodsItemPackageGridLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = PackageGridControlBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.PackageTypeDropEdit);
			builder.AddColumn(euGridColumnBag.NumberOfPackagesCalcEdit);
			builder.AddColumn(euGridColumnBag.MarksAndNumbersTextBox);

			return builder.Build();
		}

		#endregion
	}
}
