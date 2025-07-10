using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class GEIEAdaptorDelivery : GEIEServicesDelivery
	{
		protected override string GetRecipientID(IEDICommunicationsMode mode)
		{
			return mode.EK_Destination;
		}

		protected override string InterchangeQueuedStatus
		{
			get { return EDIInterchangeStatusList.Codes.eAdaptorQueued; }
		}

		protected override string TransportType
		{
			get { return EDIInterchangeTransportTypeList.Codes.eAdaptor; }
		}
	}
}
