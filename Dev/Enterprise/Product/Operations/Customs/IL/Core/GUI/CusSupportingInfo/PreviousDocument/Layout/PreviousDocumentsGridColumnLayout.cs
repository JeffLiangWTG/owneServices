using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public class PreviousDocumentsGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ??= CreateLayout();
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var ilGridColumnBag = PreviousDocumentsGridColumnBag.Instance;
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(ilGridColumnBag.CodeCodeFindBoxColumn);
			builder.AddColumn(ilGridColumnBag.ReferenceNumberColumn);
			return builder.Build();
		}
	}
}
