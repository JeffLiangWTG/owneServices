using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

public sealed class UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnLayout : IGridColumnLayoutProvider
{
	public IGridColumnLayout Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	#region Implementation

	IGridColumnLayout CreateLayout()
	{
		var euGridColumnBag = EU.TemporaryStorage.GUI.UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(euGridColumnBag.TypeCodeFindBoxColumn);
		builder.AddColumn(euGridColumnBag.ReferenceNumberTextBoxColumn);
		builder.AddColumn(euGridColumnBag.GoodItemIdentifierCalcEditColumn);
		builder.AddColumn(euGridColumnBag.PackTypeDropEditColumn);
		builder.AddColumn(euGridColumnBag.PackQtyCalcEditColumn);
		builder.AddColumn(euGridColumnBag.QuantityCalcEditColumn);
		builder.AddColumn(euGridColumnBag.QuantityUnitDropEditColumn);

		return builder.Build();
	}

	#endregion
}
