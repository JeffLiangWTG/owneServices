namespace Enterprise.Customs.IE.ExitControl.Business
{
	public interface IDocumentSendingMapper
	{
		void AddAdditionalInfomation(AdditionalInfoSendingObject sendingObject);
		void AddSupportingDocument(DocumentSendingObject sendingObject);
	}
}
