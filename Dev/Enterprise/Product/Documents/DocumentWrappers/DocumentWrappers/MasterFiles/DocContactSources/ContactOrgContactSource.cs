using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class ContactOrgContactSource : DocumentWrapper, IContactDetails
	{
		ContactOrgContactSource(OrgContact orgContact, BusinessObjectFactory factoryToWrap)
			: base(orgContact, factoryToWrap)
		{
		}

		public static ContactOrgContactSource New(OrgContact orgContact, BusinessObjectFactory factoryToWrap)
		{
			return orgContact != null ? new ContactOrgContactSource(orgContact, factoryToWrap) : null;
		}

		OrgContact OrgContact
		{
			get { return (OrgContact)WrappedObject; }
		}

		#region Custom Fields

		public override string ToString()
		{
			return ContactName;
		}

		public ZString PostalAddress
		{
			get { return GetPostalAddress(); }
		}

		public ZString PostalAddressInEnglish
		{
			get { return GetPostalAddress(true); }
		}

		ZString GetPostalAddress(bool inEnglish = false)
		{
			var result = ZString.Empty;

			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var docCompany = DocCompany.New(currentCompany, Factory);

			if (docCompany != null && ContactDetails != null)
			{
				var addressFormatter = new WrapperPostalAddressFormatter(ContactDetails, docCompany);
				if (addressFormatter != null)
				{
					addressFormatter.EnglishOnly = inEnglish;
					result = addressFormatter.PostalAddress();
				}
			}

			return result;
		}

		protected DocDeliveryContact ContactDetails
		{
			get
			{
				return new DocAutoDelivery().GetDeliveryDetailsForContact(OrgContact);
			}
		}

		public ZString Name
		{
			get
			{
				ZString result = "";
				if (this.ContactDetails != null)
				{
					result = this.ContactDetails.Name + "\n" + this.ContactDetails.CompanyName;
				}

				return result;
			}
		}

		#endregion

		public ZString Code
		{
			get { return OrgContact.OC_ContactName; }
		}

		public ZString AttachmentType
		{
			get { return OrgContact.OC_AttachmentType; }
		}

		public ZDateTime Birthday
		{
			get { return OrgContact.OC_Birthday; }
		}

		public ZString ContactName
		{
			get { return OrgContact.OC_ContactName; }
		}

		public ZString Email
		{
			get { return OrgContact.OC_Email; }
		}

		public ZString Fax
		{
			get
			{
				ZString result = "";
				if (OrgContact.OC_Fax_Formatted.IsEmpty)
				{
					if (ContactDetails != null)
					{
						result = ContactDetails.Fax;
					}
				}
				else
				{
					result = OrgContact.OC_Fax_Formatted;
				}

				return result;
			}
		}

		public ZString HomePhone
		{
			get { return OrgContact.OC_HomePhone_Formatted; }
		}

		public ZString Language
		{
			get { return OrgContact.OC_Language; }
		}

		public ZString Mobile
		{
			get { return OrgContact.OC_Mobile_Formatted; }
		}

		public ZString NotifyMode
		{
			get { return OrgContact.OC_NotifyMode; }
		}

		public DocAddress OrgAddress
		{
			get { return DocAddress.New(OrgContact.OrgAddress, Factory); }
		}

		public DocOrganisation Organisation
		{
			get { return OrgContact.OC_OH.IsValid ? DocOrganisation.New(OrgContact.Factory, OrgContact.OC_OH) : null; }
		}

		public DocOrganisation AddressOverride
		{
			get { return DocOrganisation.New(OrgContact.AddressOverride, Factory); }
		}

		public ZString OtherPhone
		{
			get { return OrgContact.OC_OtherPhone_Formatted; }
		}

		public ZString Pager
		{
			get { return OrgContact.OC_Pager_Formatted; }
		}

		public ZString Password => ZString.Empty;

		public ZString PersonalInfo
		{
			get { return OrgContact.OC_PersonalInfo; }
		}

		public ZString Phone
		{
			get
			{
				ZString result = "";
				if (OrgContact.OC_Phone.IsEmpty)
				{
					if (this.ContactDetails != null)
					{
						result = this.ContactDetails.Phone;
					}
				}
				else
				{
					result = OrgContact.OC_Phone_Formatted;
				}

				return result;
			}
		}

		public ZString Title
		{
			get { return OrgContact.OC_Title; }
		}
	}
}
