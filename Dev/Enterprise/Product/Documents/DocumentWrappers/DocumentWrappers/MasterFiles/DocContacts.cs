using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocContacts : DocumentWrapper
	{
		protected DocContacts(IContactDetails source, BusinessObjectFactory factoryToWrap)
			: base(source, factoryToWrap)
		{
		}

		public static DocContacts New(OrgContact orgContact, BusinessObjectFactory factoryToWrap)
		{
			return (orgContact != null) ? new DocContacts(ContactOrgContactSource.New(orgContact, factoryToWrap), factoryToWrap) : null;
		}

		public static DocContacts New(JobDocAddress jobDocAddress, BusinessObjectFactory factoryForWrapper)
		{
			DocContacts docContacts = null;
			if (jobDocAddress != null)
			{
				if (jobDocAddress.Contact != null && !jobDocAddress.E2_AddressOverride)
				{
					docContacts = new DocContacts(ContactOrgContactSource.New(jobDocAddress.Contact, factoryForWrapper), factoryForWrapper);
				}
				else if (!jobDocAddress.E2_Contact.IsEmpty || jobDocAddress.E2_OA_Address.IsValid || jobDocAddress.E2_AddressOverride)
				{
					docContacts = new DocContacts(ContactAddressSource.New(jobDocAddress, factoryForWrapper), factoryForWrapper);
				}
			}
			return docContacts;
		}

		IContactDetails Source
		{
			get { return (IContactDetails)WrappedObject; }
		}

		#region Custom Fields

		public override string ToString()
		{
			return ContactName;
		}

		public ZString PostalAddress
		{
			get { return Source.PostalAddress; }
		}

		public ZString PostalAddressInEnglish
		{
			get { return Source.PostalAddressInEnglish; }
		}

		public ZString Name
		{
			get { return Source.Name; }
		}

		#endregion

		public ZString Code
		{
			get { return Source.Code; }
		}

		public ZString AttachmentType
		{
			get { return Source.AttachmentType; }
		}

		public ZDateTime Birthday
		{
			get { return Source.Birthday; }
		}

		public ZString ContactName
		{
			get { return Source.ContactName; }
		}

		public ZString Email
		{
			get { return Source.Email; }
		}

		public ZString Fax
		{
			get { return Source.Fax; }
		}

		public ZString HomePhone
		{
			get { return Source.HomePhone; }
		}

		public ZString Language
		{
			get { return Source.Language; }
		}

		public ZString Mobile
		{
			get { return Source.Mobile; }
		}

		public ZString NotifyMode
		{
			get { return Source.NotifyMode; }
		}

		public DocAddress OrgAddress
		{
			get { return Source.OrgAddress; }
		}

		public DocOrganisation Organisation
		{
			get { return Source.Organisation; }
		}

		public DocOrganisation AddressOverride
		{
			get { return Source.AddressOverride; }
		}

		public ZString OtherPhone
		{
			get { return Source.OtherPhone; }
		}

		public ZString Pager
		{
			get { return Source.Pager; }
		}

		public ZString Password
		{
			get { return Source.Password; }
		}

		public ZString PersonalInfo
		{
			get { return Source.PersonalInfo; }
		}

		public ZString Phone
		{
			get { return Source.Phone; }
		}

		public ZString Title
		{
			get { return Source.Title; }
		}
	}
}
