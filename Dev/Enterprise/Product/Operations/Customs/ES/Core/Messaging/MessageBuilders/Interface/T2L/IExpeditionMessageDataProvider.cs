using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IExpeditionMessageDataProvider : IESEDIMessageCollectionProvider
	{
		IExpeditionHeader Header { get; }
		IReadOnlyCollection<IExpeditionLine> Lines { get; }
	}

	public interface IExpeditionHeader : IT2LHeaderCommon
	{
		ZString ExpeditionCustomsOffice { get; }
		ZString DestinationCountry { get; }
		ZString ConveyanceId { get; }
		IPartyProvider Sender { get; }
		IPartyProvider Consignee { get; }
		IT2LCommunicationsCommon Communications { get; }
	}

	public interface IExpeditionLine : IT2LLineCommon
	{
		IReadOnlyCollection<IExpeditionDocumentSubmitted> DocumentsSubmitted { get; }
	}

	public interface IExpeditionDocumentSubmitted
	{
		ZString Code { get; }
		ZString Number { get; }
		ZDateTime Date { get; }
	}
}
