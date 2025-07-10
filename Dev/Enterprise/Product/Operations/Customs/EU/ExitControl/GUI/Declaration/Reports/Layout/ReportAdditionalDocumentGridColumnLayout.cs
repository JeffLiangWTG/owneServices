using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed class ReportAdditionalDocumentGridColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var columnBag = AdditionalDocumentGridColumnsBag.Instance;
			var gridColumnLayoutBuilder = GridColumnLayoutBuilder.Create();
			gridColumnLayoutBuilder.AddColumn(columnBag.CSI_SubTypeDropEditColumn);
			gridColumnLayoutBuilder.AddColumn(columnBag.CSI_CodeFindBoxColumn);
			gridColumnLayoutBuilder.AddColumn(columnBag.ReferenceNumberTextBoxColumn);
			return gridColumnLayoutBuilder.Build();
		}
	}
}
