using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NCTSAuthorisedConsigneeProvider : INCTSPartyIDContact
	{
		public static NCTSAuthorisedConsigneeProvider NewOrNull(JobDocAddress jobDocAddress)
			=> jobDocAddress != null && jobDocAddress.IsValidAddress ? new NCTSAuthorisedConsigneeProvider(jobDocAddress.Address, jobDocAddress.Contact) : null;

		readonly OrgAddress address;
		readonly OrgContact contact;

		NCTSAuthorisedConsigneeProvider(OrgAddress address, OrgContact contact)
		{
			this.address = address;
			this.contact = contact;
		}

		IPartyID partyIDProvider;
		IPartyID PartyIDProvider => partyIDProvider ?? (partyIDProvider = DE.Business.PartyIDProvider.NewOrNull(address));

		public string EoriNumber => PartyIDProvider.EoriNumber.ValueOrNullIfEmpty();

		public string EoriBranchSuffix => PartyIDProvider.EoriBranchSuffix.ValueOrNullIfEmpty();

		public string Name => contact?.OC_ContactName.ValueOrNullIfEmpty();

		public string Position => contact?.OC_Title.ValueOrNullIfEmpty();

		public string PhoneNumber => contact?.OC_Phone.ValueOrNullIfEmpty();

		public string FacsimileNumber => contact?.OC_Fax.ValueOrNullIfEmpty();

		public string MailAddress => contact?.OC_Email.ValueOrNullIfEmpty();

		public string TCUNumber => null;
	}
}
