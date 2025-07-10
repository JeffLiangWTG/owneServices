using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class ImportInvoiceLinePreviousDocumentGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ??= CreateLayout();
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = PreviousDocumentsGridColumnBag.Instance;
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.CodeDropEditColumn);
			builder.AddColumn(euGridColumnBag.SubTypeColumn);
			builder.AddColumn(euGridColumnBag.ReferenceNumberColumn);
			builder.AddColumn(euGridColumnBag.DateOfIssueColumn);
			builder.AddColumn(euGridColumnBag.LineNoColumn);
			builder.AddColumn(euGridColumnBag.PackQtyColumn);
			builder.AddColumn(euGridColumnBag.PackTypeColumn);
			builder.AddColumn(euGridColumnBag.QuantityColumn);
			builder.AddColumn(euGridColumnBag.UnitOfQuantityColumn);
			builder.AddColumn(euGridColumnBag.ItemNumberColumn);

			return builder.Build();
		}
	}
}
