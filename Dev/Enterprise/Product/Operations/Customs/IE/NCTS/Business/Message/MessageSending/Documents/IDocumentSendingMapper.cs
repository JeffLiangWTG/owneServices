using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public interface IDocumentSendingMapper
	{
		void AddAdditionalInfomation(AdditionalInfoSendingObject sendingObject);
		void AddSupportingDocument(DocumentSendingObject sendingObject);
	}
}
