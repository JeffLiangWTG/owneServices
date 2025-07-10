using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Registry;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class UrlDecider
	{
		public UrlDecider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		readonly NctsHeader nctsHeader;

		public ZString GetUrlNcts()
		{
			if (nctsHeader.CanLaunchNctsUrl())
			{
				return ComposeUrl(ESCustomsDataRegistry.Instance.NctsTransitStatusQueryUrl.Value, nctsHeader.MovementReferenceNumber);
			}
			else
			{
				return ZString.Empty;
			}
		}

		static string ComposeUrl(ZString url, ZString mrn)
			=> url.Replace(CustomsWebsiteUrlCodes.MRNinRegistryUrl, mrn);
	}
}
