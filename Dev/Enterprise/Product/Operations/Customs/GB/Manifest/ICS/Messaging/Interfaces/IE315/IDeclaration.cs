using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging.IE315
{
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
		ICustomsOffice LodgementCustomsOffice { get; }
		ITrader Representative { get; }
		ITrader LodgingSummaryDeclarationPerson { get; }
		IEnumerable<ISealsID> SealsIds { get; }
		IFirstEntryCustomsOffice FirstEntry { get; }
		IEnumerable<ICustomsOffice> SubsequentEntries { get; }
		ITrader EntryCarrier { get; }
	}
}
