using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class DocumentSendingSupportingDocumentGridColumnLayout : IGridColumnLayoutProvider
	{
		#region IGridColumnLayoutProvider

		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		#endregion

		IGridColumnLayout CreateLayout()
		{
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn<ZGuidDropEditColumnStyleInfo>(DocumentSendingObject.Schema.EDoc, 200);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(DocumentSendingObject.FileName), 100);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(DocumentSendingObject.FileSizeInKB), 80);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(nameof(DocumentSendingObject.FileDescription), 200);
			return builder.Build();
		}
	}
}
