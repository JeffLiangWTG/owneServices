using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class ContactPersonProvider : IContactPerson
	{
		public static ContactPersonProvider NewOrNull(JobDocAddress jobDocAddress)
		{
			return jobDocAddress != null && (!jobDocAddress.E2_Contact.IsEmpty || !jobDocAddress.E2_Phone.IsEmpty || !jobDocAddress.E2_Email.IsEmpty)
				? new ContactPersonProvider(jobDocAddress)
				: null;
		}

		ContactPersonProvider(JobDocAddress jobDocAddress)
		{
			this.jobDocAddress = jobDocAddress;
		}

		public string Name => jobDocAddress.E2_Contact;

		public string PhoneNumber => jobDocAddress.E2_Phone;

		public string EMailAddress => jobDocAddress.E2_Email;

		readonly JobDocAddress jobDocAddress;
	}
}
