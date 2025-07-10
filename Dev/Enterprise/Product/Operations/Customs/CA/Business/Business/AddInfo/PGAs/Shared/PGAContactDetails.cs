using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class PGAContactDetails : IPGAContactDetails
	{
		public PGAContactDetails(OrgHeader organisation)
			: this(organisation, GetPGAContact(organisation))
		{
		}

		public PGAContactDetails(OrgHeader organisation, GlbStaff staff)
			: this(organisation)
		{
			this.emailAddress = staff?.GS_EmailAddress ?? ZString.Empty;
			this.phoneNumber = staff?.GS_WorkPhone ?? ZString.Empty;
			this.contactName = staff?.GS_FullName ?? ZString.Empty;
		}

		PGAContactDetails(OrgHeader organisation, OrgContact contact)
		{
			this.organisation = organisation;
			this.contact = contact;
		}

		readonly OrgHeader organisation;
		readonly OrgContact contact;
		readonly ZString emailAddress;
		readonly ZString phoneNumber;
		readonly ZString contactName;

		const string AllocationType = OrgConstants.ContactAllocationType.CAPGA;

		public ZString EmailAddress
		{
			get
			{
				var result = emailAddress;

				if (result.IsEmpty)
				{
					result = contact?.OC_Email ?? ZString.Empty;
				}

				if (result.IsEmpty)
				{
					result = organisation?.MainAddress?.OA_Email ?? ZString.Empty;
				}

				return result;
			}
		}

		public ZString PhoneNumber
		{
			get
			{
				var result = phoneNumber;

				if (result.IsEmpty)
				{
					result = contact?.OC_Phone ?? ZString.Empty;
				}

				if (result.IsEmpty)
				{
					result = organisation?.MainAddress?.OA_Phone ?? ZString.Empty;
				}

				return result;
			}
		}

		public ZString ContactName
		{
			get
			{
				var result = contactName;

				if (result.IsEmpty)
				{
					result = contact?.OC_ContactName ?? ZString.Empty;
				}

				return result;
			}
		}
		static OrgContact GetPGAContact(OrgHeader organisation)
		{
			return organisation?.Contacts.GetContactForAllocation(AllocationType);
		}
	}
}
