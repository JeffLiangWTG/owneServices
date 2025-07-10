using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.CDS
{
	class GBConsolCustomsRequestDataProvider : GBCustomsRequestDataProviderBase
	{
		readonly ForwardingConsol consol;
		readonly CredentialsSetting credentialsSetting;
		readonly CustomsExportConsolIntegrationWrapper consolWrapper;

		public GBConsolCustomsRequestDataProvider(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));

			consolWrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
			credentialsSetting = consolWrapper.GetCredentialForPima();
		}

		protected override ZString JobNumberCore => consol.JK_UniqueConsignRef;

		protected override ZString GatewayCore => credentialsSetting?.CSP ?? ZString.Empty;

		protected override ZString GetCredentialsKey()
		{
			var provider = GBCustomsRequestFactory.GetProviderType(GatewayCore);
			var badge = provider == ProviderType.Direct ? consolWrapper.MawbExportHelper.ME_Profile : credentialsSetting?.BadgeCode ?? ZString.Empty;
			return consol.GetCredentialsKey(badge);
		}

		protected override CredentialsSetting GetCredentialsSetting() => credentialsSetting;
	}
}
