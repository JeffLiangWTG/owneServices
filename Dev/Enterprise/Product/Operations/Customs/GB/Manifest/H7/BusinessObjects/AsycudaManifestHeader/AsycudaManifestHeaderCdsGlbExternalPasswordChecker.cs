using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.H7.Business
{
	public class AsycudaManifestHeaderCdsGlbExternalPasswordChecker : AbstractCdsGlbExternalPasswordChecker<AsycudaManifestHeader>
	{
		public AsycudaManifestHeaderCdsGlbExternalPasswordChecker(AsycudaManifestHeader businessObject, MessageSendingNotificationCollection notifications) : base(businessObject)
		{
			this.notifications = notifications;
		}

		readonly MessageSendingNotificationCollection notifications;

		protected override ZString GetEori()
		{
			return businessObject?.Branch?.OrgProxy?.GetEuIdentificationNumber() ?? ZString.Empty;
		}

		protected override ZString GetBadge()
		{
			return businessObject.AMA_CustomsProfile;
		}

		protected override void ShowError(ZString error)
		{
			notifications.AddError(error);
		}

		protected override void ShowWarning(ZString warning)
		{
			notifications.AddWarning(warning);
		}
	}
}
