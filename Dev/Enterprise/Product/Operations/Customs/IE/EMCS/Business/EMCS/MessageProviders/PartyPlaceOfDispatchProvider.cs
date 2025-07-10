using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class PartyPlaceOfDispatchProvider : IEMCSPartyPlaceOfDispatch
	{
		public static PartyPlaceOfDispatchProvider NewOrNull(JobDocAddress jobDocAddress)
			=> jobDocAddress != null && jobDocAddress.IsValidAddress ? new PartyPlaceOfDispatchProvider(jobDocAddress) : null;

		PartyPlaceOfDispatchProvider(JobDocAddress jobDocAddress)
		{
			var provider = PartyAddressProvider.NewOrNull(jobDocAddress);
			var submissionType = (jobDocAddress.Parent as EMCSJobDeclaration)?.ZG_SubmissionType ?? string.Empty;
			if (submissionType == EMCSSubmissionTypeList.Codes.SubmissionForDutyPaidB2B || jobDocAddress.E2_AddressOverride)
			{
				Language = provider.Language;
				Name = provider.Name;
				StreetAndNumber = provider.StreetAndNumber;
				City = provider.City;
				Postcode = provider.Postcode;
				Country = provider.Country;
			}
			else
			{
				Language = string.Empty;
				Name = string.Empty;
				StreetAndNumber = string.Empty;
				City = string.Empty;
				Postcode = string.Empty;
				Country = string.Empty;
			}

			if (submissionType == EMCSSubmissionTypeList.Codes.SubmissionForDutyPaidB2B)
			{
				ReferenceOfTaxWarehouse = string.Empty;
			}
			else
			{
				if (jobDocAddress.E2_AddressOverride)
				{
					ReferenceOfTaxWarehouse = jobDocAddress.E2_GovRegNumType == OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID ? jobDocAddress.E2_GovRegNum.ToString() : string.Empty;
				}
				else
				{
					ReferenceOfTaxWarehouse = jobDocAddress.Address.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID);
				}
			}
		}

		public string ReferenceOfTaxWarehouse { get; }

		public string Language { get; }

		public string Name { get; }

		public string StreetAndNumber { get; }

		public string City { get; }

		public string Postcode { get; }

		public string Country { get; }
	}
}
