using System;
using CargoWise.Customs.CH.MessageContracts;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class EnquiryDataProvider : IEnquiry
{
	public static EnquiryDataProvider New(NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		return sendingObject == null ? null : new EnquiryDataProvider(sendingObject);
	}

	EnquiryDataProvider(NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		this.sendingObject = sendingObject;
	}
	readonly NctsHeaderDepartureMessageSendingObject sendingObject;

	public string Reason => sendingObject.ReasonCode;

	public string Text => sendingObject.ReasonText.ReturnNullIfEmpty();

	public string MRNDoubleEntry => sendingObject.DoubleEntryMRN.ReturnNullIfEmpty();

	public DateTime? TC11DeliveryDate => sendingObject.TC11DeliveryDate.ToOptionalDateTime();
}
