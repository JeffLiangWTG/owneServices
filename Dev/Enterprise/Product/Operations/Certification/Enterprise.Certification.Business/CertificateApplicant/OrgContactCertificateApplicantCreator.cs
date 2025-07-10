using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Certification;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Certification.Business
{
	public class OrgContactCertificateApplicantCreator : IOrgContactCertificateApplicantCreator
	{
		public static CertificateApplicant LoadOrCreateFromContact(OrgContact contact)
		{
			Argument.NotNull(contact, "contact");
			Argument.NotNullOrEmpty(contact.OC_Email, "contact.OC_Email");

			var factory = contact.Factory;

			var result = LoadFromContact(contact);

			if (result == null)
			{
				result = factory.New<CertificateApplicant>();
				PopulateNewApplicant(result, contact);
			}

			bool shouldSaveFactory = !result.IsInDatabase;
			if (shouldSaveFactory)
			{
				factory.Save();
			}
			return result;
		}

		public static CertificateApplicant LoadFromContact(OrgContact contact)
		{
			Argument.NotNull(contact, "contact");
			var applicantWithMatchingEmail = !contact.OC_Email.IsEmpty
				? contact.Factory.LoadFromUniqueKey<CertificateApplicant>(HRJobApplicantSchema.HA_EmailAddress, contact.OC_Email)
				: null;

			return applicantWithMatchingEmail;
		}

		static void PopulateNewApplicant(CertificateApplicant applicant, OrgContact contact)
		{
			applicant.HA_PER = contact.OC_PER;

			applicant.RelatedOrgContactPK = contact.PK;
			applicant.HA_EmailAddress = contact.OC_Email;
			applicant.HA_MobilePhone = contact.OC_Mobile;
			applicant.HA_WorkPhone = contact.OC_Phone;
			applicant.HA_WorkExtension = contact.OC_PhoneExtension;
			applicant.HA_FaxNum = contact.OC_Fax;
		}

		OrgContact LoadContact(ZGuid contactPk)
		{
			var factory = new BusinessObjectFactory();
			return factory.Load<OrgContact>(contactPk);
		}
		ICertificateApplicant IOrgContactCertificateApplicantCreator.LoadOrCreateFromContact(ZGuid contactPk)
		{
			var contact = LoadContact(contactPk);
			return (contact == null || contact.OC_Email.IsEmpty) ? null : LoadOrCreateFromContact(contact);
		}

		ICertificateApplicant IOrgContactCertificateApplicantCreator.LoadFromContact(ZGuid contactPk)
		{
			var contact = LoadContact(contactPk);
			return LoadFromContact(contact);
		}
	}
}
