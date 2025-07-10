using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IT.Business;
using Enterprise.Messaging.Business;
using IXmlCustomsResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class XmlResponseMessageCustomsStatusAnalyzer : ResponseMessageCustomsStatusAnalyzer<IXmlCustomsResponseMessage>
{
	public XmlResponseMessageCustomsStatusAnalyzer(NctsDepartureMovementHeader movementHeader) : base(movementHeader, EDIMessageTypeList.Codes.NewDeclaration)
	{
	}

	protected override IXmlCustomsResponseMessage GetResponseMessage(EDIMessage response) => (new NctsResponseMessage(response.EM_MessageText) as IResponseMessageWithWrapper).GetResponseMessageContents();

	protected override bool IsValidResponseMessage(EDIMessage response) => response.EM_MessageType == MessageProcessorConstants.InterchangeTypes.Ucc6ResponseMessageType;

	protected override ZString GetStatusFromResponseMessage(IXmlCustomsResponseMessage responseMessage)
	{
		var isReleased = !string.IsNullOrEmpty(responseMessage.ReleaseItems.HeaderItem?.ReferenceNumber);
		if (isReleased)
		{
			return NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
		}

		if (responseMessage.State == MessageProcessorConstants.AidaXmlResponseStatusNumbers.UnderControl)
		{
			return NCTS5DepartureCustomsStatusList.Codes.IntentionToControl;
		}

		if (!string.IsNullOrWhiteSpace(responseMessage.Mrn))
		{
			return NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
		}

		return ZString.Empty;
	}
}
