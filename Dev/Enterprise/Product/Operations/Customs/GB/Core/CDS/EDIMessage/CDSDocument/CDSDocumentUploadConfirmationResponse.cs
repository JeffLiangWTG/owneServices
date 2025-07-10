using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSDocumentUploadConfirmationResponse : CDSEDIMessage<DocumentUploadConfirmationResponse>
	{
		public CDSDocumentUploadConfirmationResponse(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.DocumentUploadConfirmation;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}

		public new DocumentUploadConfirmationResponse MessageDataObject => messageDataObject ?? (messageDataObject = new DocumentUploadConfirmationResponse(EM_MessageText));
		DocumentUploadConfirmationResponse messageDataObject;
	}
}
