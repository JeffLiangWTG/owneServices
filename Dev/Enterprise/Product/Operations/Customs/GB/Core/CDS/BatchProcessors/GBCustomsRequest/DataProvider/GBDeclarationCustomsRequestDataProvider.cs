using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.CDS
{
	class GBDeclarationCustomsRequestDataProvider : GBCustomsRequestDataProviderBase
	{
		readonly JobDeclaration declaration;

		public GBDeclarationCustomsRequestDataProvider(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		protected override ZString JobNumberCore => declaration.JE_DeclarationReference;

		protected override ZString GatewayCore => declaration.ZG_Gateway;

		protected override ZString GetCredentialsKey() => declaration.GetCredentialsKey();

		protected override CredentialsSetting GetCredentialsSetting() => declaration.GetCredentialsSettingByBadgeCode();

		protected override ZString GetCredentialPartyID()
		{
			return Gateway == GatewayList.Codes.Pentant
			? declaration.DunsForBranchOrgProxy
			: ZString.Empty;
		}
	}
}
