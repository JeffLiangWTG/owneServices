using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Mail.Business
{
	public abstract class CustomerServiceEmailBaseWrapper : DocBaseWrapper
	{
		protected CustomerServiceEmailBaseWrapper(BusinessObject objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		[DocumentField("Current Date")]
		public ZString CurrentDate
		{
			get { return ZDateTime.Now.ToShortDateString(); }
		}

		[DocumentField("Contact and Client Name")]
		public ZString ContactAndClientName
		{
			get
			{
				ZString result = ZString.Empty;
				bool hasContact = (Contact != null);
				bool hasClient = (!ClientName.IsEmpty);

				if (hasContact || hasClient)
				{
					result = "<p><b>";
					if (hasContact)
					{
						result += Contact.ContactNameWithoutNumberSuffix.CapitaliseFirstLettersOfWords();
					}
					if (hasClient)
					{
						if (hasContact)
						{
							result += "<br />";
						}
						result += ClientName;
					}
					result += "</b></p>";
				}

				return result;
			}
		}

		protected abstract OrgContact Contact { get; }
		protected abstract ZString ClientName { get; }

		internal abstract void SetContact(OrgContact contact);
		internal abstract void SetClient(OrgHeader client);

		[DocumentField("Contact Salutation")]
		public ZString ContactSalutation
		{
			get
			{
				ZString result;
				OrgContact contact = Contact;
				if (contact == null)
				{
					result = "Dear Client";
				}
				else
				{
					result = contact.OC_Salutation.IsEmpty ? contact.ContactNameWithoutNumberSuffix : contact.OC_Salutation;
					result = result.CapitaliseFirstLettersOfWords();
				}
				return result;
			}
		}

		[DocumentField("Current Company OrgProxy Name")]
		public ZString CurrentCompanyOrgProxyName
		{
			get { return CurrentCompany.Organisation.Name; }
		}
	}
}

