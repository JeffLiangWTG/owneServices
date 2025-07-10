using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Registry;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class UrlDecider
	{
		public UrlDecider(TemporaryStorageHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		readonly TemporaryStorageHeader header;

		public ZString GetUrl()
		{
			var mrn = header.MRN;

			if (!mrn.IsEmpty)
			{
				return ComposeUrl(ESCustomsDataRegistry.Instance.G5V1StatusQueryUrl.Value, mrn);
			}

			return ZString.Empty;
		}

		static string ComposeUrl(ZString url, ZString mrn)
			=> url.Replace(CustomsWebsiteUrlCodes.MRNinRegistryUrl, mrn);
	}
}
