using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class COLSResponsiblePartyAddressRequirement : JobDocAddressRequirement
	{
		public COLSResponsiblePartyAddressRequirement(QuarantineColsHeader addressParent) : base(DocAddressType.COLSResponsibleParty)
		{
			Initialize();
		}

		void Initialize()
		{
			IsMandatory = true;
			ValidateContact = ValidateE2_Contact;
			ValidateEmail = ValidateE2_Email;
			ValidatePhoneFormatted = ValidatePhoneNumber;
			ValidateMobileFormatted = ValidateMobilePhoneNumber;
			ValidateOrganisationPKUponMandatoryRequirement = AddressMandatoryCheck;
		}

		void ValidateE2_Contact(JobDocAddressValidation validation)
		{
			var address = validation.Parent;
			if (address.E2_Contact.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(address.E2_ContactInfo);
			}
			else
			{
				if (address.Contact is OrgContact contact)
				{
					var orgAddress = address.Address;
					var contactDetails = new ContactDetailsGUIFormatter(contact, orgAddress, orgAddress.Header).GetContactDetailsForGUI();

					if (contactDetails.Email == ContactDetailsGUIFormatter.OrganisationMessages.NoEmailFoundOnFile)
					{
						address.E2_ContactInfo.AddMessageError("Email must be specified.");
					}

					if (contactDetails.Phone == ContactDetailsGUIFormatter.OrganisationMessages.NoPhoneFoundOnFile)
					{
						address.E2_ContactInfo.AddMessageError("Phone or Mobile must be specified.");
					}
				}
				else if (!address.E2_AddressOverride)
				{
					address.E2_ContactInfo.AddMessageError("Need to select a Contact.");
				}
			}
		}

		void ValidateE2_Email(JobDocAddressValidation validation)
		{
			var address = validation.Parent;
			MandatoryValidation.MessageErrorIfNotEntered(address.E2_EmailInfo);
		}

		void ValidatePhoneNumber(JobDocAddressValidation validation)
		{
			var address = validation.Parent;
			if (address.E2_Mobile.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(address.E2_Phone_FormattedInfo);
			}
		}

		void ValidateMobilePhoneNumber(JobDocAddressValidation validation)
		{
			var address = validation.Parent;
			if (address.E2_Phone.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(address.E2_Mobile_FormattedInfo);
			}
		}

		void AddressMandatoryCheck(JobDocAddressValidation validation)
		{
			var address = validation.Parent;
			if (address.OrganisationPK.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(address.OrganisationPKInfo);
			}
		}
	}
}
