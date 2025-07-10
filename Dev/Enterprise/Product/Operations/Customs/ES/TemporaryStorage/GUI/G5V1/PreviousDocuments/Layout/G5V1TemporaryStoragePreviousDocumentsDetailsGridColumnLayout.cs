using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

public sealed class G5V1TemporaryStoragePreviousDocumentsDetailsGridColumnLayout : IGridColumnLayoutProvider
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
		builder.AddColumn(euGridColumnBag.ReferenceNumber2TextBoxColumn);

		return builder.Build();
	}

	#endregion
}
