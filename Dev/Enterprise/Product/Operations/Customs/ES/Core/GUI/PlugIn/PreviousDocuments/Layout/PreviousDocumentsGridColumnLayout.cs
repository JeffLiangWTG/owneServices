using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public class PreviousDocumentsGridColumnLayout : IGridColumnLayoutProvider
{
	public IGridColumnLayout Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	IGridColumnLayout CreateLayout()
	{
		var esGridColumnBag = PreviousDocumentsGridColumnBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(esGridColumnBag.CodeDropEditColumn);
		builder.AddColumn(esGridColumnBag.SubTypeDropEditColumn);
		builder.AddColumn(esGridColumnBag.ReferenceNumberColumn);
		builder.AddColumn(esGridColumnBag.CountryCodeColumn);

		return builder.Build();
	}
}
