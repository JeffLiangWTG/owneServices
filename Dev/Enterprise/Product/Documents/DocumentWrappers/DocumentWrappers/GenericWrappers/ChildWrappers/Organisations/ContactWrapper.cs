using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("FullName")]
	public class ContactWrapper : GenericWrapper
	{
		public ContactWrapper(OrgContact contact, BusinessObjectFactory factory)
			: this(contact, (contact != null ? contact.Header : null), factory)
		{
		}

		public ContactWrapper(OrgContact contact, OrgAddress selectedAddress, BusinessObjectFactory factory)
			: this(contact, selectedAddress != null ? selectedAddress.Header : factory.GetNull<OrgHeader>(), factory)
		{
			SelectedAddressBO = selectedAddress;
		}

		public ContactWrapper(OrgContact contact, OrgHeader organisation, BusinessObjectFactory factory)
			: base(contact, factory)
		{
			if (contact == null)
			{
				ContactBO = factory.GetNull<OrgContact>();
			}
			else
			{
				ContactBO = contact;
				AddressBO = contact.EffectiveContactAddress;
				if (AddressBO == null && organisation != null)
				{
					AddressBO = organisation.MainAddress;
				}
			}
		}

		public ContactWrapper(JobDocAddress docAddress, BusinessObjectFactory factory)
			: base(docAddress, factory)
		{
			if (docAddress == null || docAddress.IsEmpty)
			{
				ContactBO = factory.GetNull<OrgContact>();
			}
			else
			{
				if (docAddress.E2_AddressOverride)
				{
					ContactBO = factory.GetNull<OrgContact>();
					DocAddress = docAddress;
				}
				else
				{
					ContactBO = docAddress.Contact;
					AddressBO = docAddress.Address;
					if (ContactBO == null)
					{
						ContactBO = factory.GetNull<OrgContact>();
					}
				}
			}
		}

		public AddressWrapper Address
		{
			get
			{
				if (fAddress == null)
				{
					if (SelectedAddressBO != null)
					{
						fAddress = new AddressWrapper(SelectedAddressBO, FullName, Factory);
					}
					else if (DocAddress != null)
					{
						fAddress = new AddressWrapper(DocAddress, Factory);
					}
					else
					{
						fAddress = new AddressWrapper(AddressBO, FullName, Factory);
					}
				}
				return fAddress;
			}
		}

		public CodeAndDescriptionWrapper AttachmentType
		{
			get { return new CodeAndDescriptionWrapper(ContactBO.OC_AttachmentType, ContactBO.OC_AttachmentType_List, Factory); }
		}

		public CodeAndDescriptionWrapper Language
		{
			get { return new CodeAndDescriptionWrapper(ContactBO.OC_Language, ContactBO.Languages, Factory); }
		}

		public CodeAndDescriptionWrapper NotifyMode
		{
			get { return new CodeAndDescriptionWrapper(ContactBO.OC_NotifyMode, ContactBO.OC_NotifyMode_List, Factory); }
		}

		public ZString FullName
		{
			get { return DocAddress != null ? DocAddress.E2_Contact : ContactBO.OC_ContactName; }
		}

		public ZString Salutation
		{
			get { return ContactBO.OC_Salutation; }
		}

		public ZString JobTitle
		{
			get { return ContactBO.OC_Title; }
		}

		public ZString JobCategory
		{
			get { return ContactBO.OC_JobCategory; }
		}

		public ZString Phone
		{
			get { return GetBestValueWithFallback(ContactBO.OC_Phone_Formatted, Address.Phone); }
		}

		public ZString Extension
		{
			get { return ContactBO.OC_PhoneExtension; }
		}

		public ZString Fax
		{
			get { return GetBestValueWithFallback(ContactBO.OC_Fax_Formatted, Address.Fax); }
		}

		public ZString Mobile
		{
			get { return GetBestValueWithFallback(ContactBO.OC_Mobile_Formatted, Address.Mobile); }
		}

		public ZString Email
		{
			get { return GetBestValueWithFallback(ContactBO.OC_Email, Address.Email); }
		}

		public ZString Pager
		{
			get { return ContactBO.OC_Pager_Formatted; }
		}

		public ZString HomePhone
		{
			get { return ContactBO.OC_HomePhone_Formatted; }
		}

		public ZString OtherPhone
		{
			get { return ContactBO.OC_OtherPhone_Formatted; }
		}

		public ZBool IsActive => ContactBO.IsNull ? false : ContactBO.OC_IsActive;

		#region Implementation
#if DEBUG
		internal
#endif
		readonly OrgContact ContactBO;
		readonly OrgAddress AddressBO;
		readonly OrgAddress SelectedAddressBO;
		readonly JobDocAddress DocAddress;
		AddressWrapper fAddress;
		#endregion
	}
}
