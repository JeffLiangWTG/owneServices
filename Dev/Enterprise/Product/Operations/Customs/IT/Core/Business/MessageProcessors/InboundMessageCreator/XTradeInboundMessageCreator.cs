using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business;

sealed class XTradeInboundMessageCreator : IInboundMessageCreator
{
	void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange incomingInterchange)
	{
		Argument.NotNull(incomingInterchange, nameof(incomingInterchange));

		var existingInterchangePk = GetExistingReceivedInterchangePk(incomingInterchange);
		if (!existingInterchangePk.IsEmpty)
		{
			throw new CustomsMessageProcessorException(GetErrorMessage(existingInterchangePk));
		}

		var interchangeProcessingStrategy = GetInterchangeProcessingStrategy(incomingInterchange.EI_InterchangeType);
		interchangeProcessingStrategy.ProcessInterchange(incomingInterchange);
	}

	#region Implementation

	ZGuid GetExistingReceivedInterchangePk(EDIInterchange incomingInterchange)
	{
		var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ITCustomsXTrade)
			.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, incomingInterchange.EI_SessionGUID)
			.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, incomingInterchange.EI_ReceiveTransmit)
			.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Received)
			.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, incomingInterchange.EI_InterchangeType)
			.AddToFilter(EDIInterchangeSchema.EI_From, incomingInterchange.EI_From)
			.AddToFilter(EDIInterchangeSchema.EI_To, incomingInterchange.EI_To)
			.AddToFilter(EDIInterchangeSchema.EI_BodyText, incomingInterchange.EI_BodyText);

		query.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name;

		return incomingInterchange.Factory.LoadTop1<EDIInterchange>(query)?.PK ?? ZGuid.Empty;
	}

	ZString GetErrorMessage(ZGuid pk) => FormattableString.Invariant($"Duplicated message of EI_PK = '{pk}'");

	IInboundInterchangeProcessingStrategy GetInterchangeProcessingStrategy(string interchangeType) => interchangeType switch
	{
		EDIMessageTypeList.Codes.IvistoResponse => new IvistoInboundInterchangeProcessingStrategy(),
		EDIMessageTypeList.Codes.IrildesResponse => new IrildesInboundInterchangeProcessingStrategy(),
		EDIMessageTypeList.Codes.ElectronicFolderQuery => new ElectronicFolderInboundInterchangeProcessingStrategy(),
		EDIMessageTypeList.Codes.XtCustomsError => new XtCustomsErrorInterchangeProcessingStrategy(),
		_ => new DefaultInboundInterchangeProcessingStrategy()
	};

	#endregion
}
