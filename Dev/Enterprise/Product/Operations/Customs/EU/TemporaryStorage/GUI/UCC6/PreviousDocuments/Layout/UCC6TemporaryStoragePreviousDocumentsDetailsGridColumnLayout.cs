using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI;

public sealed class UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnLayout : IGridColumnLayoutProvider
{
	public IGridColumnLayout Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	#region Implementation

	IGridColumnLayout CreateLayout()
	{
		var euGridColumnBag = UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(euGridColumnBag.TypeCodeFindBoxColumn);
		builder.AddColumn(euGridColumnBag.ReferenceNumberTextBoxColumn);
		builder.AddColumn(euGridColumnBag.GoodItemIdentifierCalcEditColumn);

		return builder.Build();
	}

	#endregion
}

