using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class RepresentativeContactPersonProvider : IContactPerson
	{
		readonly OrgContact orgContact;
		readonly JobDocAddress jobDocAddress;

		public static RepresentativeContactPersonProvider NewOrNull(JobDocAddress jobDocAddress)
		{
			var orgContact = GetContact(jobDocAddress);
			return orgContact == null ? null : new RepresentativeContactPersonProvider(jobDocAddress, orgContact);
		}

		RepresentativeContactPersonProvider(JobDocAddress jobDocAddress, OrgContact orgContact)
		{
			this.jobDocAddress = jobDocAddress;
			this.orgContact = orgContact;
		}

		public string Name => orgContact.Name;

		public string PhoneNumber
		{
			get
			{
				var contactPhone = orgContact.OC_Phone;
				return contactPhone.IsEmpty ? jobDocAddress.E2_Phone : contactPhone;
			}
		}

		public string EMailAddress
		{
			get
			{
				var contactEmail = orgContact.Email;
				return contactEmail.IsEmpty ? jobDocAddress.E2_Email : contactEmail;
			}
		}

		static OrgContact GetContact(JobDocAddress jobDocAddress)
		{
			OrgContact result = null;
			if (jobDocAddress?.Parent is NctsDepartureMovementHeader movementHeader && movementHeader.Header is NctsHeader header)
			{
				var activeContacts = jobDocAddress.Organisation?.GetActiveContacts().Cast<OrgContact>().ToArray() ?? Array.Empty<OrgContact>();
				result = activeContacts.Cast<OrgContact>().FirstOrDefault(x => x.Allocations.Cast<OrgContactAllocation>().Any(allocation => allocation.PC_Type == OrgConstants.ContactAllocationType.CUS));
			}
			return result;
		}
	}
}
