using System.Collections.Generic;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.GB.ICS.Messaging.IE313
{
	[CodeAlive("Will be used in subsequent WI.")]
	public interface IDeclaration
	{
		ZString MessageSender { get; }
		ZString MessageRecipient { get; }
		ZString DateOfPreparation { get; }
		ZString TimeOfPreparation { get; }
		ZString Priority { get; }
		ZBool TestIndicator { get; }
		ZString MessageIdentification { get; }
		ZString MessageType { get; }
		ZString CorrelationIdentifier { get; }

		IHeader Header { get; }

		ITrader Consignor { get; }
		ITrader Consignee { get; }
		ITrader NotifyParty { get; }

		IEnumerable<IGoodsItem> GoodsItems { get; }

		IEnumerable<IItinerary> Itineraries { get; }

		ITrader Representative { get; }
		IPerson LodgingSummaryDeclarationPerson { get; }
		IEnumerable<ISealsID> SealsIds { get; }
		IFirstEntryCustomsOffice FirstEntryCustomsOffice { get; }
		IEnumerable<ICustomsOffice> SubsequentEntryCustomsOffices { get; }
		ITrader EntryCarrier { get; }
	}
}
