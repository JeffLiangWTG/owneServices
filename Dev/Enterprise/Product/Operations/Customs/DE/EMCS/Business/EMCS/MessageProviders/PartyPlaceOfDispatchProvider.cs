using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class PartyPlaceOfDispatchProvider : IEMCSPartyPlaceOfDispatch
	{
		public static PartyPlaceOfDispatchProvider NewOrNull(JobDocAddress jobDocAddress)
			=> jobDocAddress != null && jobDocAddress.IsValidAddress ? new PartyPlaceOfDispatchProvider(jobDocAddress) : null;

		PartyPlaceOfDispatchProvider(JobDocAddress jobDocAddress)
		{
			var provider = PartyAddressProvider.NewOrNull(jobDocAddress);
			if (jobDocAddress.E2_AddressOverride)
			{
				ReferenceOfTaxWarehouse = jobDocAddress.E2_GovRegNumType == OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID ? jobDocAddress.E2_GovRegNum.ToString() : string.Empty;
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
				Language = string.Empty;
				Name = string.Empty;
				Address = string.Empty;
				City = string.Empty;
				Postcode = string.Empty;
				Country = string.Empty;
			}
		}

		public string ReferenceOfTaxWarehouse { get; }

		public string Language { get; }

		public string Name { get; }

		public string Address { get; }

		public string City { get; }

		public string Postcode { get; }

		public string Country { get; }
	}
}
