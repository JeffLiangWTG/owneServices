using CargoWise.Customs.DE.MessageContracts;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class PartyContactPersonJobDocAddressProvider : IAESPartyContactPerson
	{
		readonly JobDocAddress jobDocAddress;

		public static PartyContactPersonJobDocAddressProvider NewOrNull(JobDocAddress jobDocAddress) => jobDocAddress == null ? null : new PartyContactPersonJobDocAddressProvider(jobDocAddress);

		PartyContactPersonJobDocAddressProvider(JobDocAddress jobDocAddress)
		{
			this.jobDocAddress = jobDocAddress;
		}

		public string Position => string.Empty;

		public string PersonName => jobDocAddress.E2_Contact;

		public string PhoneNumber => jobDocAddress.E2_Phone;

		public string FacsimileNumber => jobDocAddress.E2_Fax;

		public string MailAddress => jobDocAddress.E2_Email;
	}
}
