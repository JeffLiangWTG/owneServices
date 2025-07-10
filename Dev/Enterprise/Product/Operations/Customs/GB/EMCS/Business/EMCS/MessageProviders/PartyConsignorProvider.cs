using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class PartyConsignorProvider : PartyAddressProvider, IEMCSPartyConsignor
	{
		protected PartyConsignorProvider(JobDocAddress jobDocAddress) : base(jobDocAddress)
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

		public new static PartyConsignorProvider NewOrNull(JobDocAddress jobDocAddress) => jobDocAddress != null && jobDocAddress.IsValidAddress ? new PartyConsignorProvider(jobDocAddress) : null;

		public string TraderExciseNumber { get; }
	}
}
