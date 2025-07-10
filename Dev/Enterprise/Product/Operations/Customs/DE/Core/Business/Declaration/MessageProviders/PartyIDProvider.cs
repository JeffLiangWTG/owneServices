using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class PartyIDProvider : IPartyID
	{
		public static PartyIDProvider NewOrNull(OrgHeader org) => org != null ? new PartyIDProvider(org) : null;

		public static PartyIDProvider NewOrNull(OrgAddress address) => address != null ? new PartyIDProvider(address) : null;

		public static PartyIDProvider NewOrNull(JobDocAddress jobDocAddress) => jobDocAddress != null && jobDocAddress.IsValidAddress ? new PartyIDProvider(jobDocAddress.Address) : null;

		protected PartyIDProvider(OrgHeader org)
		{
			this.org = org;
		}

		protected PartyIDProvider(OrgAddress address) : this(address.Header)
		{
			this.address = address;
		}

		protected readonly OrgAddress address;
		protected readonly OrgHeader org;

		public string EoriNumber => org.GetEUEoriDetails().ValueOrNullIfEmpty();

		public string EoriBranchSuffix => address.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix).ValueOrNullIfEmpty();

		public string TCUNumber => org?.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU).ValueOrNullIfEmpty();
	}
}
