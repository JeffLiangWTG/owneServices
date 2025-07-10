using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaUniversalEventMessagePinProcessor : AsycudaUniversalEventMessageProcessor
	{
		public AsycudaUniversalEventMessagePinProcessor(IXmlSessionTracker logger, Event universalEvent, AsycudaEDIMessage message, AsycudaManifestHeader manifestHeader)
			: base(logger, universalEvent, message, manifestHeader)
		{
		}

		protected override ZGuid GetGroupToSendCore() => ZGuid.Empty;

		protected override ZString GetSubject() => ZString.Empty;

		protected override bool SendToGroup() => false;

		protected override bool SendToStaff() => false;
	}
}
