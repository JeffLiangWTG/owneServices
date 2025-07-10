using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class Ucc6ExportEntryInstructionPreviousDocumentGridColumnLayout : IGridColumnLayoutProvider
{
	public IGridColumnLayout Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	IGridColumnLayout CreateLayout()
	{
		var euGridColumnBag = PreviousDocumentsGridColumnBag.Instance;
		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(euGridColumnBag.CodeDropEditColumn);
		builder.AddColumn(euGridColumnBag.ReferenceNumberColumn);

		return builder.Build();
	}
}
