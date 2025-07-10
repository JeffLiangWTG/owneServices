using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class PartyTransporterProvider : PartyAddressProvider, IEMCSPartyTransporter
	{
		public new static PartyTransporterProvider NewOrNull(JobDocAddress jobDocAddress)
			=> jobDocAddress != null && jobDocAddress.IsValidAddress ? new PartyTransporterProvider(jobDocAddress) : null;

		PartyTransporterProvider(JobDocAddress jobDocAddress)
			: base(jobDocAddress)
		{
			var countryCode = jobDocAddress.E2_RN_NKCountryCode;
			if (jobDocAddress.E2_AddressOverride)
			{
				VatNumber = jobDocAddress.E2_GovRegNumType == countryCode.GetVATCodeType() ? jobDocAddress.E2_GovRegNum.ToString() : string.Empty;
			}
			else
			{
				VatNumber = jobDocAddress.Organisation.GetVATRegistrationNumber(countryCode);
			}
		}

		public string VatNumber { get; }
	}
}
