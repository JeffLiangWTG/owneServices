
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public class NonUCC6PreviousDocumentGridColumnLayout : IGridColumnLayoutProvider
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

			return builder.Build();
		}
	}
}
