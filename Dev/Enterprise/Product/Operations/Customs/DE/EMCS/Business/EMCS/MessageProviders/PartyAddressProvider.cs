using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class PartyAddressProvider : IEMCSPartyAddress
	{
		public static PartyAddressProvider NewOrNull(JobDocAddress jobDocAddress)
			=> jobDocAddress != null && jobDocAddress.IsValidAddress ? new PartyAddressProvider(jobDocAddress) : null;

		protected PartyAddressProvider(JobDocAddress jobDocAddress)
		{
			if (!jobDocAddress.E2_AddressOverride)
			{
				var address = jobDocAddress.Address;
				Language = jobDocAddress.Organisation.OH_Language.ToLower();
				City = address.OA_City;
				Address = address.OA_Address1 + address.OA_Address2;
				Postcode = address.OA_PostCode;
				Name = jobDocAddress.Organisation.OH_FullName;
			}
			else
			{
				Language = GlbBranch.CurrentBranch.OrgProxy?.OH_Language.ToLower() ?? string.Empty;
				City = jobDocAddress.E2_City;
				Address = jobDocAddress.E2_Address1 + jobDocAddress.E2_Address2;
				Postcode = jobDocAddress.E2_Postcode;
				Name = jobDocAddress.E2_CompanyName;
			}
		}

		public string Language { get; }

		public string Name { get; }

		public string City { get; }

		public string Address { get; }

		public string Postcode { get; }

		public string Country => string.Empty;
	}
}
