namespace Enterprise.Customs.IE.Business
{
	public interface IDocumentSendingMapper
	{
		void AddAdditionalInfomation(AdditionalInfoSendingObject sendingObject);
		void AddSupportingDocument(DocumentSendingObject sendingObject);
	}
}
