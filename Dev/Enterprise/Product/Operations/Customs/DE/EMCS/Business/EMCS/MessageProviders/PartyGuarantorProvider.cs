using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class PartyGuarantorProvider : PartyAddressProvider, IEMCSPartyGuarantor
	{
		public new static PartyGuarantorProvider NewOrNull(JobDocAddress jobDocAddress)
			=> jobDocAddress != null && jobDocAddress.IsValidAddress ? new PartyGuarantorProvider(jobDocAddress) : null;

		PartyGuarantorProvider(JobDocAddress jobDocAddress)
			: base(jobDocAddress)
		{
			var countryCode = jobDocAddress.E2_RN_NKCountryCode;
			if (jobDocAddress.E2_AddressOverride)
			{
				TraderExciseNumber = jobDocAddress.E2_GovRegNumType == OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber ? jobDocAddress.E2_GovRegNum.ToString() : string.Empty;
				VatNumber = jobDocAddress.E2_GovRegNumType == countryCode.GetVATCodeType() ? jobDocAddress.E2_GovRegNum.ToString() : string.Empty;
			}
			else
			{
				var org = jobDocAddress.Organisation;
				TraderExciseNumber = org.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber);
				VatNumber = org.GetVATRegistrationNumber(countryCode);
			}
		}

		public string TraderExciseNumber { get; }

		public string VatNumber { get; }
	}
}
