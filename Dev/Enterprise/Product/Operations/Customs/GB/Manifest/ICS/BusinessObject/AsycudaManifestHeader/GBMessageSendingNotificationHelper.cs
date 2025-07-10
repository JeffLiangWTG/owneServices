using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class GBMessageSendingNotificationHelper : MessageSendingNotificationHelper
	{
		public GBMessageSendingNotificationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override ZString GetExtraMessageSendingNotificationCore()
		{
			var result = CheckForICSManifestSending();
			if (result.IsEmpty)
			{
				result = base.GetExtraMessageSendingNotificationCore();
			}
			return result;
		}

		ZString CheckForICSManifestSending()
		{
			ZString result = ZString.Empty;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.UnitedKingdom)
			{
				result = ResString.GetMultilingualString("30F9C6DE-71F9-4084-9D28-B94319BD12E2", "To create a message for UK you must be logged-in under a UK company.");
			}
			else if (string.IsNullOrWhiteSpace(GBCustomsDataRegistry.Instance.ICSUsername.Value) || string.IsNullOrWhiteSpace(GBCustomsDataRegistry.Instance.ICSPasssword.Value))
			{
				result = ResString.GetMultilingualString("824E4D68-9A21-4613-BCD0-D99A01B2C689", "ICS Username and Password must be entered. \r\nSee System -> Registry -> Customs -> Country or Region Specific -> United Kingdom -> ICS.");
			}
			else if (GlbBranch.CurrentBranch.OrgProxy == null || GlbBranch.CurrentBranch.OrgProxy.GetEoriNumber(Core.Constants.CountryCodes.UnitedKingdom).IsEmpty)
			{
				result = ResString.GetMultilingualString("E7A10C06-8FA4-4364-B2CE-94A6D6937577", "EORI must exist under the currently logged in branch's Organization Proxy. \r\nConfig code: EOR.");
			}

			return result;
		}
	}
}
