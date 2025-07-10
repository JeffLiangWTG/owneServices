using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class DocumentSendingObject : SupportingDocSendingObject, ISupportingDocumentMessageDataProvider
	{
		public DocumentSendingObject(ISupportingDocObject nctsHeader) : base(nctsHeader)
		{
		}

		protected override void SetDefaultValues()
		{
			ShouldSend = true;
		}

		public static DocumentSendingObject New(ISupportingDocObject nctsHeader) => (DocumentSendingObject)nctsHeader.GetSupportingDocSendingObject();

		public new ISupportingDocObject SupportingDocObject => base.SupportingDocObject;

		protected override SupportingDocSendingObjectValidation GetNewValidation() => new DocumentSendingObjectValidation(this);

		public override bool ShouldCheckSizeInEdocField => false;

		protected override void GetExtraDocsIfNoneAvailable(System.Guid edocKey)
		{
		}

		CusEntryHeader ISupportingDocumentMessageDataProvider.Header => null;

		ForwardingShipment ISupportingDocumentMessageDataProvider.Shipment => null;

		EnterpriseBusinessObject ISupportingDocumentMessageDataProvider.BusinessObject => SupportingDocObject as EnterpriseBusinessObject;

		ZString ISupportingDocumentMessageDataProvider.CaseNumber => CaseNumber;

		ZString ISupportingDocumentMessageDataProvider.ContextReference => null;

		DataContextType ISupportingDocumentMessageDataProvider.ContextType => DataContextType.NctsHeader;

		IeDoc ISupportingDocumentMessageDataProvider.Document => Document;

		ZString ISupportingDocumentMessageDataProvider.LocalReferenceNumber => LocalReferenceNumber;

		ZString ISupportingDocumentMessageDataProvider.DocumentType => DocumentType;

		ZString ISupportingDocumentMessageDataProvider.CountryCode => SupportingDocObject.CountryCode;
	}
}
