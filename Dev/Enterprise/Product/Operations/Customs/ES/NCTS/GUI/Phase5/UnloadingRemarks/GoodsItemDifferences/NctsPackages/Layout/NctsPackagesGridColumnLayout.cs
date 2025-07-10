using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	sealed class NctsPackagesGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = NctsPackagesGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.SequenceNumberCalcEditColumn);
			builder.AddColumn(euGridColumnBag.TypeOfDifferenceDropEditColumn);
			builder.AddColumn(euGridColumnBag.UnitCountCalcEditColumn);
			builder.AddColumn(euGridColumnBag.UnitTypeDropEditColumn);
			builder.AddColumn(euGridColumnBag.MarksAndNumbersTextBoxColumn);
			builder.AddColumn(euGridColumnBag.PackageIDTextBoxColumn);
			builder.AddColumn(euGridColumnBag.BrandTextBoxColumn);
			builder.AddColumn(euGridColumnBag.ModelTextBoxColumn);
			builder.AddColumn(euGridColumnBag.GrossWeightCalcEditColumn);
			builder.AddColumn(euGridColumnBag.GrossWeightUQDropEditColumn);

			return builder.Build();
		}

		#endregion
	}
}
