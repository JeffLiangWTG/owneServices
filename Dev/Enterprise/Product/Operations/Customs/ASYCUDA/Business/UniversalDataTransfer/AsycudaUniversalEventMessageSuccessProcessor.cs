using CargoWise.Types;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaUniversalEventMessageSuccessProcessor : AsycudaUniversalEventMessageProcessor
	{
		public AsycudaUniversalEventMessageSuccessProcessor(IXmlSessionTracker logger, Event universalEvent, AsycudaEDIMessage message, AsycudaManifestHeader manifestHeader)
			: base(logger, universalEvent, message, manifestHeader)
		{
		}

		protected override ZString GetSubject()
		{
			return Res.GetString("A0AF0848-1C9D-4F2F-B525-AB65298108CC", "Global Manifest Universal Event Response Received: Success");
		}

		protected override ZGuid GetGroupToSendCore() => ManifestCustomsDataRegistry.Instance.GroupToSendSucceedNotification.Value;

		protected override bool SendToGroup()
		{
			var regist = ManifestCustomsDataRegistry.Instance.SendSuccessNotifications.Value;
			return regist == Core.Constants.EmailTo.NominatedGroup
				|| regist == Core.Constants.EmailTo.StaffMemberAndNominatedGroup;
		}

		protected override bool SendToStaff()
		{
			var regist = ManifestCustomsDataRegistry.Instance.SendSuccessNotifications.Value;
			return regist == Core.Constants.EmailTo.StaffMemberAndNominatedGroup;
		}
	}
}
