using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

sealed class DfiaImportItemDetailsGridColumnsLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();

	IGridColumnLayout CreateLayout()
	{
		var gridColumnBag = DfiaImportItemDetailsGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(gridColumnBag.SerialNoTextBoxColumn);
		builder.AddColumn(gridColumnBag.LicenseImportItemSerialNoTextBoxColumn);
		builder.AddColumn(gridColumnBag.LicenseImportQuantityCalcEditColumn);
		builder.AddColumn(gridColumnBag.LicenseImportQuantityUnitDropEditColumn);
		builder.AddColumn(gridColumnBag.ItemTypeDropEditColumn);

		return builder.Build();
	}

	IGridColumnLayout layout;
}
