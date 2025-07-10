using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

sealed class DfiaExportItemDetailsGridColumnsLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();
	IGridColumnLayout CreateLayout()
	{
		var gridColumnBag = DfiaExportItemDetailsGridColumnsBag.Instance;
		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(gridColumnBag.SerialNoTextBoxColumn);
		builder.AddColumn(gridColumnBag.LicenseNoTextBoxColumn);
		builder.AddColumn(gridColumnBag.LicenseDateEditColumn);
		builder.AddColumn(gridColumnBag.LicenseExportItemSerialNoTextBoxColumn);
		builder.AddColumn(gridColumnBag.LicenseExportQuantityCalcEditColumn);
		builder.AddColumn(gridColumnBag.LicenseExportQuantityUnitDropEditColumn);
		return builder.Build();
	}
	IGridColumnLayout layout;
}
