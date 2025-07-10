using CargoWise.Types;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.UniversalDataBuss.Integration;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaUniversalEventMessageFailureProcessor : AsycudaUniversalEventMessageProcessor
	{
		public AsycudaUniversalEventMessageFailureProcessor(IXmlSessionTracker logger, Event universalEvent, AsycudaEDIMessage message, AsycudaManifestHeader header)
			: base(logger, universalEvent, message, header)
		{
		}

		protected override ZString GetSubject()
		{
			return Res.GetString("2AC7F148-5891-4C21-8C55-0A5967427759", "Global Manifest Universal Event Response Received: Failed");
		}

		protected override ZGuid GetGroupToSendCore() => ManifestCustomsDataRegistry.Instance.GroupToSendErrorNotification.Value;

		protected override bool SendToGroup()
		{
			var regist = ManifestCustomsDataRegistry.Instance.SendErrorNotifications.Value;
			return regist == Core.Constants.EmailTo.NominatedGroup
				|| regist == Core.Constants.EmailTo.StaffMemberAndNominatedGroup;
		}

		protected override bool SendToStaff()
		{
			var regist = ManifestCustomsDataRegistry.Instance.SendErrorNotifications.Value;
			return regist == Core.Constants.EmailTo.StaffMemberAndNominatedGroup;
		}
	}
}
