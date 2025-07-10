
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	sealed class CopyDocumentsLineGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ??= CreateLayout();
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = CopyDocumentsLineGridColumnBag.Instance;
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.TypeColumn);
			builder.AddColumn(euGridColumnBag.SubTypeColumn);
			builder.AddColumn(euGridColumnBag.CodeColumn);
			builder.AddColumn(euGridColumnBag.ReferenceNumberColumn);
			builder.AddColumn(euGridColumnBag.ReferenceNumber2Column);
			builder.AddColumn(euGridColumnBag.DescriptionColumn);
			builder.AddColumn(euGridColumnBag.IsSelectedColumn);

			return builder.Build();
		}
	}
}
