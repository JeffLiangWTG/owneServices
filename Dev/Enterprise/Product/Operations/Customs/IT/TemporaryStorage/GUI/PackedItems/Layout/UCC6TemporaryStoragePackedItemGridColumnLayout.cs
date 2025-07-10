using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

public sealed class UCC6TemporaryStoragePackedItemGridColumnLayout : IGridColumnLayoutProvider
{
	public IGridColumnLayout Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	#region Implementation

	IGridColumnLayout CreateLayout()
	{
		var euGridColumnBag = EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackedItemGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(euGridColumnBag.LineNoCalcEditColumn);
		builder.AddColumn(euGridColumnBag.FormattedTariffColumn);
		builder.AddColumn(euGridColumnBag.GoodsDescriptionTextBoxColumn);
		builder.AddColumn(euGridColumnBag.GrossWeightCalcEditColumn);
		builder.AddColumn(euGridColumnBag.GrossWeightUnitDropEditColumn);
		builder.AddColumn(euGridColumnBag.NetWeightCalcEditColumn);
		builder.AddColumn(euGridColumnBag.NetWeightUnitDropEditColumn);
		builder.AddColumn(euGridColumnBag.CustomsQty2CalcEditColumn);
		builder.AddColumn(euGridColumnBag.CustomsUnit2DropEditColumn);
		builder.AddColumn(euGridColumnBag.ChemicalSubstanceCodeFindBoxColumn);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(TemporaryStoragePackedItem.RegistrationNo), 120, columnInfo => { columnInfo.IsReadOnly = true; });
		builder.AddColumn<ZDateEditColumnStyleInfo>(nameof(TemporaryStoragePackedItem.ReleaseDate), 120, columnInfo => { columnInfo.IsReadOnly = true; });
		return builder.Build();
	}

	#endregion
}
