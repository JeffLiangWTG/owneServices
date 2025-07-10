using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class GEIDirectXTDelivery : GEIEServicesDelivery
	{
		protected override string InterchangeQueuedStatus => EDIInterchangeStatusList.Codes.Queued;

		protected override string TransportType => EDIInterchangeTransportTypeList.Codes.xT;

		protected override string GetRecipientID(IEDICommunicationsMode mode)
		{
			return mode.EK_Destination;
		}

		protected override ZString CreateHeaderText(IEDICommunicationsMode mode)
			// Writes to EI_HeaderText; XTI/XTO service tasks expect it to be JSON, but is XML by default.
			// Unused by Accounting
			=> ZString.Empty;
	}
}
