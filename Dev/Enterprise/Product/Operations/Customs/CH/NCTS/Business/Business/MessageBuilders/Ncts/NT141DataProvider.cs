using CargoWise.Customs.CH.MessageContracts;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT141DataProvider : BaseNctsMessageDataProvider<NctsHeaderDepartureMessageSendingObject>, INT141
{
	public NT141DataProvider(NctsHeaderDepartureMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	public IBaseTransitOperation TransitOperation => transitOperation ?? (transitOperation = BaseTransitOperationDataProvider.New(nctsHeader));
	IBaseTransitOperation transitOperation;

	public IEnquiry Enquiry => enquiry ?? (enquiry = EnquiryDataProvider.New(sendingObject));
	IEnquiry enquiry;

	public ICustomsOffice CustomsOfficeOfDestinationActual => customsOfficeOfDestinationActual ?? (customsOfficeOfDestinationActual = IsReasonTextEmpty ? null : CustomsOfficeReferenceDataProvider.New(sendingObject.ActualDestinationCustomsOffice));
	ICustomsOffice customsOfficeOfDestinationActual;

	public IConsignee ConsigneeActual => consigneeActual ?? (consigneeActual = IsReasonTextEmpty ? null : ConsigneeDataProvider.New(sendingObject.ActualConsignee));
	IConsignee consigneeActual;

	bool IsReasonTextEmpty => sendingObject.ReasonText.IsEmpty;

	protected override string GetCorrelationIdentifier()
	{
		string correlationIdentifier = null;

		var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, nctsHeader.MovementHeader.PK);
		query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CHCustomsPassar);
		query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIInterchange.Direction.Receive);
		query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeCodeList.Codes.MSG);
		query.AddToFilter(EDIMessageSchema.EM_MessageSubType, MessageSubTypeCodeList.Codes.PassarEnquiryOfNotArrivedTransit);
		query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.ProcessedOK);
		query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;

		var ediMessage = nctsHeader.Factory.LoadTop1<CHEDIMessage>(query);
		if (ediMessage?.MessageDetail is INT140ResponseDetail responseDetail)
		{
			correlationIdentifier = responseDetail.MessageIdentification;
		}

		return correlationIdentifier;
	}
}
