using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public class ReportAdditionalDocumentUcc6GridColumnLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??=CreateLayout();
	IGridColumnLayout layout;

	IGridColumnLayout CreateLayout()
	{
		var commonColumnBag = AdditionalDocumentGridColumnsBag.Instance;
		var builder = GridColumnLayoutBuilder.Create();

		builder.AddColumn(commonColumnBag.ItemNumberCalcEditColumn);
		builder.AddColumn(commonColumnBag.CSI_SubTypeDropEditColumn);
		builder.AddColumn(commonColumnBag.CSI_CodeFindBoxColumn);
		builder.AddColumn(commonColumnBag.ReferenceNumberTextBoxColumn);
		builder.AddColumn(commonColumnBag.StatusDropEditColumn);

		return builder.Build();
	}
}
