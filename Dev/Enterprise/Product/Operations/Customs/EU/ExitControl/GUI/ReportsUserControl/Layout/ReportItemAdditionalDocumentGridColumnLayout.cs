using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed class ReportItemAdditionalDocumentGridColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var columnBag = AdditionalDocumentGridColumnsBag.Instance;
			var gridColumnLayoutBuilder = GridColumnLayoutBuilder.Create();
			gridColumnLayoutBuilder.AddColumn(columnBag.CSI_SubTypeDropEditColumn);
			gridColumnLayoutBuilder.AddColumn(columnBag.CSI_CodeFindBoxColumn);
			gridColumnLayoutBuilder.AddColumn<ZTextBoxColumnStyleInfo>(AdditionalInfo.Schema.CSI_ReferenceNumber, 131);
			return gridColumnLayoutBuilder.Build();
		}
	}
}
