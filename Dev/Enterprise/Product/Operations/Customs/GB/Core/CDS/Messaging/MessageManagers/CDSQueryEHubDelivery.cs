using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageManagers
{
	public class CDSQueryEHubDelivery : EServicesDelivery
	{
		public CDSQueryEHubDelivery(UniversalEvent universalEvent)
		{
			universalEventCore = universalEvent;
		}

		readonly UniversalEvent universalEventCore;

		protected override string InterchangeQueuedStatus => EDIInterchangeStatusList.Codes.eHubQueued;

		protected override string TransportType => EDIInterchangeTransportTypeList.Codes.eHub;

		protected override string GetRecipientID(IEDICommunicationsMode mode) => mode.EK_Destination;
		protected override string GetInterchangeTo(IEDICommunicationsMode mode)
		{
			return mode.EK_Destination.SubstringSafe(0, mode.EK_Destination.IndexOf(':'));
		}
		protected override MessageDataLogLinker GetMessageDataLogLinker(BusinessObjectFactory factory, DeliveryContext context)
		{
			return new MessageDataLogLinker(Events.ServiceRequested, factory, universalEventCore?.EventReference ?? ZString.Empty);
		}
	}
}
