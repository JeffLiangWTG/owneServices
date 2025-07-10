using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class PartyConsignorProvider : PartyAddressProvider, IEMCSPartyConsignor
	{
		public new static PartyConsignorProvider NewOrNull(JobDocAddress jobDocAddress)
			=> jobDocAddress != null && jobDocAddress.IsValidAddress ? new PartyConsignorProvider(jobDocAddress) : null;

		PartyConsignorProvider(JobDocAddress jobDocAddress)
			: base(jobDocAddress)
		{
			if (jobDocAddress.E2_AddressOverride)
			{
				TraderExciseNumber = jobDocAddress.E2_GovRegNumType == OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber ? jobDocAddress.E2_GovRegNum.ToString() : string.Empty;
			}
			else
			{
				TraderExciseNumber = jobDocAddress.Organisation.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber);
			}
		}

		public string TraderExciseNumber { get; }
	}
}
