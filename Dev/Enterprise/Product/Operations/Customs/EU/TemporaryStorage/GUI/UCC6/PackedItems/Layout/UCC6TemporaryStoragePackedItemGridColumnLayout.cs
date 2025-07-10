using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI;

public sealed class UCC6TemporaryStoragePackedItemGridColumnLayout : IGridColumnLayoutProvider
{
	public IGridColumnLayout Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	#region Implementation

	IGridColumnLayout CreateLayout()
	{
		var euGridColumnBag = UCC6TemporaryStoragePackedItemGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(euGridColumnBag.LineNoCalcEditColumn);
		builder.AddColumn(euGridColumnBag.FormattedTariffColumn);
		builder.AddColumn(euGridColumnBag.GoodsDescriptionTextBoxColumn);
		builder.AddColumn(euGridColumnBag.GrossWeightCalcEditColumn);
		builder.AddColumn(euGridColumnBag.GrossWeightUnitDropEditColumn);
		builder.AddColumn(euGridColumnBag.ChemicalSubstanceCodeFindBoxColumn);

		return builder.Build();
	}

	#endregion
}
