using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class ContactPersonProvider : IContactPerson
	{
		readonly JobDocAddress jobDocAddress;

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
	}
}
