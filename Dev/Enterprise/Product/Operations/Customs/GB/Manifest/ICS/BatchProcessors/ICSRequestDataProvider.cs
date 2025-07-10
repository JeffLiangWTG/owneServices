using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.ICS
{
	public class ICSRequestDataProvider : GBCustomsRequestDataProviderBase
	{
		public ICSRequestDataProvider(AsycudaManifestHeaderBase manifest)
		{
			this.manifestHeader = Argument.NotNull(manifest, nameof(manifest));
		}

		public ZString ServiceReference => manifestHeader.RegistrationNumber;

		protected override ZString JobNumberCore => manifestHeader.AMA_JobReference;

		protected override ZString GatewayCore => "ICS";

		protected override ZString GetCredentialsKey()
		{
			var credentialKey = ".NOVALIDTOKENFOUND";
			var enterpriseCode = GBExtensions.GetEnterpriseCode();
			var eori = manifestHeader.Branch?.OrgProxy.GetEuIdentificationNumber() ?? ZString.Empty;

			var validPassword = !eori.IsEmpty ? GBCustomsCDSXmlCredentialConfigurationHandler.FindAndLoadGlbExternalPassword(eori, manifestHeader.Factory) : null;

			if (validPassword != null)
			{
				credentialKey = FormattableString.Invariant($"{enterpriseCode}.{validPassword.GP_UserID}");
			}

			return credentialKey;
		}

		protected override CredentialsSetting GetCredentialsSetting() => null;

		readonly AsycudaManifestHeaderBase manifestHeader;
	}
}
