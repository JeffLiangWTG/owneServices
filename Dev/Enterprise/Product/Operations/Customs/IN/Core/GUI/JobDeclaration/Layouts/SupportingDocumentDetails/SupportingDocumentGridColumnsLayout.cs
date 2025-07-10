using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class SupportingDocumentGridColumnsLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();

	#region Implementation

	IGridColumnLayout CreateLayout()
	{
		var gridColumnBag = SupportingDocumentsGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(gridColumnBag.LineNoTextBoxColumn);
		builder.AddColumn(gridColumnBag.ImageReferenceNumberTextBoxColumn);
		builder.AddColumn(gridColumnBag.DocumentTypeCodeFindBoxColumn);
		builder.AddColumn(gridColumnBag.OrganisationGuidFindBoxColumn);
		builder.AddColumn(gridColumnBag.CodeDropEditColumn);

		return builder.Build();
	}

	IGridColumnLayout layout;

	#endregion
}
