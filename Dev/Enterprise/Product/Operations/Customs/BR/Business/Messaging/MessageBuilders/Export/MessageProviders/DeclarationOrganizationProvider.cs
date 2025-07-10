using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationOrganizationProvider : IDeclarationOrganization
	{
		public DeclarationOrganizationProvider(OrgHeader organization)
		{
			this.organization = organization;
		}

		readonly OrgHeader organization;

		public string ID => organization.GetCNPJOrCPF();

		public string Name => organization?.OH_FullName ?? string.Empty;

		public string CountryCode => organization?.MainAddress.OA_RN_NKCountryCode ?? string.Empty;

		public string CountryState => organization?.MainAddress.OA_State ?? string.Empty;

		public string AddressLine
		{
			get
			{
				if (organization != null)
				{
					return organization.MainAddress.Address1 + " " + organization.MainAddress.Address2;
				}
				return string.Empty;
			}
		}
	}
}
