using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public class ReportItemAdditionalDocumentUcc6GridColumnLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
	IGridColumnLayout layout;

	IGridColumnLayout CreateLayout()
	{
		var commonColumnBag = AdditionalDocumentGridColumnsBag.Instance;
		var builder = GridColumnLayoutBuilder.Create();

		builder.AddColumn(commonColumnBag.ItemNumberCalcEditColumn);
		builder.AddColumn(commonColumnBag.CSI_SubTypeDropEditColumn);
		builder.AddColumn(commonColumnBag.CSI_CodeFindBoxColumn);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(AdditionalInfo.Schema.CSI_ReferenceNumber, 131);
		builder.AddColumn(commonColumnBag.StatusDropEditColumn);

		return builder.Build();
	}
}
