using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class PreValidateTraderEHubDelivery : EServicesDelivery
	{
		public PreValidateTraderEHubDelivery(UniversalEvent universalEvent) => universalEventCore = universalEvent;

		protected override string InterchangeQueuedStatus => EDIInterchangeStatusList.Codes.eHubQueued;

		protected override string TransportType => EDIInterchangeTransportTypeList.Codes.eHub;

		protected override string GetRecipientID(IEDICommunicationsMode mode) => mode.EK_Destination;

		protected override string GetInterchangeTo(IEDICommunicationsMode mode) => mode.EK_Destination.SubstringSafe(0, mode.EK_Destination.IndexOf(':'));

		protected override MessageDataLogLinker GetMessageDataLogLinker(BusinessObjectFactory factory, DeliveryContext context) => new (Events.ServiceRequested, factory, universalEventCore?.EventReference ?? ZString.Empty);

		readonly UniversalEvent universalEventCore;
	}
}
