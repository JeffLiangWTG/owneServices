using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.GUI
{
	public class SupportingDocSendingForm : Customs.GUI.SupportingDocSendingForm
	{
		public SupportingDocSendingForm(JobDeclarationSupportingDocSendingObjectParent declarationWrapper)
			: base(declarationWrapper)
		{
			UseTextBoxColumnForDocumentType();
		}

		void UseTextBoxColumnForDocumentType()
		{
			var documentTypeDropEditColumnStyleInfo = MessageSendingObjectsGrid.GetColumnStyle(SupportingDocSendingObject.Schema.DocumentType);
			var indexOfDocumentTypeDropEditColumnStyleInfo = MessageSendingObjectsGrid.ColumnStyles.IndexOf(documentTypeDropEditColumnStyleInfo);
			var documentTypeTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = SupportingDocSendingObject.Schema.DocumentType,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133),
				IsMandatory = true
			};

			MessageSendingObjectsGrid.ColumnStyles.RemoveAt(indexOfDocumentTypeDropEditColumnStyleInfo);
			MessageSendingObjectsGrid.ColumnStyles.Insert(indexOfDocumentTypeDropEditColumnStyleInfo, documentTypeTextBoxColumnStyleInfo);
		}
	}
}
