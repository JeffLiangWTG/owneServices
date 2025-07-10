using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class InvoiceLineImportPreviousDocumentGridColumnLayout : IGridColumnLayoutProvider
{
	public IGridColumnLayout Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	IGridColumnLayout CreateLayout()
	{
		var euGridColumnBag = PreviousDocumentsGridColumnBag.Instance;
		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(euGridColumnBag.ProcedureColumn);
		builder.AddColumn(euGridColumnBag.CodeDropEditColumn);
		builder.AddColumn(euGridColumnBag.ReferenceNumberColumn);
		builder.AddColumn(euGridColumnBag.ReferenceNumber2Column);
		builder.AddColumn(euGridColumnBag.DateOfIssueColumn);
		builder.AddColumn(euGridColumnBag.LineNoColumn);
		builder.AddColumn(euGridColumnBag.CustomsOfficeColumn);
		builder.AddColumn(euGridColumnBag.QuantityColumn);
		builder.AddColumn(euGridColumnBag.UnitOfQuantityColumn);
		builder.AddColumn(euGridColumnBag.PackQtyColumn);
		builder.AddColumn(euGridColumnBag.PackTypeColumn);

		return builder.Build();
	}
}
