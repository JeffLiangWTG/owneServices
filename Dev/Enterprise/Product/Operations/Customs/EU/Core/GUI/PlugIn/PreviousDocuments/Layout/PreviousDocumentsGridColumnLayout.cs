
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public class PreviousDocumentsGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ??= CreateLayout();
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = PreviousDocumentsGridColumnBag.Instance;
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.CodeDropEditColumn);
			builder.AddColumn(euGridColumnBag.CodeDescriptionColumn);
			builder.AddColumn(euGridColumnBag.SubTypeColumn);
			builder.AddColumn(euGridColumnBag.ReferenceNumberColumn);
			builder.AddColumn(euGridColumnBag.DateOfIssueColumn);
			builder.AddColumn(euGridColumnBag.LineNoColumn);
			builder.AddColumn(euGridColumnBag.ProcedureColumn);

			builder.AddColumn(euGridColumnBag.ReferenceNumber2Column);
			builder.AddColumn(euGridColumnBag.CustomsOfficeColumn);
			builder.AddColumn(euGridColumnBag.QuantityColumn);
			builder.AddColumn(euGridColumnBag.UnitOfQuantityColumn);
			builder.AddColumn(euGridColumnBag.Quantity3Column);
			builder.AddColumn(euGridColumnBag.UnitOfQuantity3Column);
			builder.AddColumn(euGridColumnBag.PackQtyColumn);
			builder.AddColumn(euGridColumnBag.PackTypeColumn);
			builder.AddColumn(euGridColumnBag.ItemNumberColumn);
			builder.AddColumn(euGridColumnBag.Quantity2Column);
			builder.AddColumn(euGridColumnBag.UnitOfQuantity2Column);
			builder.AddColumn(euGridColumnBag.CountryCodeColumn);

			return builder.Build();
		}
	}
}
