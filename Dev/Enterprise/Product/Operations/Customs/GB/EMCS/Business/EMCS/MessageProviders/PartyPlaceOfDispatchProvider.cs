using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class PartyPlaceOfDispatchProvider : IEMCSPartyPlaceOfDispatch
	{
		PartyPlaceOfDispatchProvider(JobDocAddress jobDocAddress)
		{
			var provider = PartyAddressProvider.NewOrNull(jobDocAddress);
			if (jobDocAddress.E2_AddressOverride)
			{
				ReferenceOfTaxWarehouse = jobDocAddress.E2_GovRegNumType == OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID ? jobDocAddress.E2_GovRegNum.ToString() : string.Empty;
				EORINumber = jobDocAddress.E2_GovRegNumType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori ? jobDocAddress.E2_GovRegNum.ToString() : string.Empty;
				Language = provider.Language;
				Name = provider.Name;
				Address = provider.Address;
				City = provider.City;
				Postcode = provider.Postcode;
				Country = provider.Country;
			}
			else
			{
				ReferenceOfTaxWarehouse = jobDocAddress.Address.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID);
				EORINumber = jobDocAddress.Organisation.GetEoriDetails();
				Language = provider.Language;
				Name = provider.Name;
				Address = provider.Address;
				City = string.Empty;
				Postcode = string.Empty;
				Country = string.Empty;
			}
		}

		public static PartyPlaceOfDispatchProvider NewOrNull(JobDocAddress jobDocAddress) => jobDocAddress != null && jobDocAddress.IsValidAddress ? new PartyPlaceOfDispatchProvider(jobDocAddress) : null;

		public string ReferenceOfTaxWarehouse { get; }

		public string EORINumber { get; }

		public string Language { get; }

		public string Name { get; }

		public string City { get; }

		public string Country { get; }

		public string Address { get; }

		public string Postcode { get; }
	}
}
