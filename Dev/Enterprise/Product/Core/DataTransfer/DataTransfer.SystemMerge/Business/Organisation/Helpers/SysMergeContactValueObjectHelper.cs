using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	public class SysMergeContactValueObjectHelper
	{
		public SysMergeContactValueObjectHelper(string errorContext)
		{
			this.ErrorContext = errorContext;
		}

		public readonly string ErrorContext;

		#region Import

		public void ImportFromValueObjectCollection(Xsd.SysMergeOrgContactCollection contactValueCollection, OrgHeaderForDataTransfer organisation, IValueObjectImportContext context)
		{
			if (contactValueCollection.IsSpecified)
			{
				for (int i = 0; i < contactValueCollection.Count; i++)
				{
					Xsd.SysMergeOrgContact contactValue = contactValueCollection[i];
					OrgContact newContact = ImportFromValueObject(organisation, contactValue, context);
				}
			}
		}

		OrgContact ImportFromValueObject(OrgHeaderForDataTransfer organisation, Xsd.SysMergeOrgContact xsdContact, IValueObjectImportContext context)
		{
			OrgContact newContact = organisation.Factory.NewWithPrimaryKey<OrgContact>(new Guid(xsdContact.PK));
			(newContact as ISupportDataImporting).IsImportingData = true;
			newContact.OC_OH = organisation.PK;

			context.SetPropertyInfoValueIfValueNotEmpty(newContact.OC_ContactNameInfo, xsdContact.ContactName);
			SetContactDetails(newContact, xsdContact, context);

			return newContact;
		}

		void SetContactDetails(OrgContact contactToUpdate, Xsd.SysMergeOrgContact contactValue, IValueObjectImportContext context)
		{
			//import orgDocument
			foreach (Xsd.SysMergeOrgDocument xsdOrgDocument in contactValue.OrgDocuments)
			{
				ZGuid menuitem = ZGuid.Empty;
				ZGuid.TryParse(xsdOrgDocument.SU_MenuItemPK, out menuitem);

				bool shouldAdd = menuitem.IsEmpty ||
					contactToUpdate.Factory.Load<StmMenuItem>(menuitem) != null;

				if (shouldAdd)
				{
					OrgDocument orgDoc = contactToUpdate.Documents.AddNew();
					orgDoc.OD_OC = contactToUpdate.PK;

					context.SetPropertyInfoValueIfValueNotEmpty(orgDoc.OD_DocumentGroupInfo, xsdOrgDocument.DocumentGroup);
					context.SetPropertyInfoValueIfValueNotEmpty(orgDoc.OD_DeliverByInfo, xsdOrgDocument.DeliverBy);
					context.SetPropertyInfoValueIfValueNotEmpty(orgDoc.OD_AttachmentTypeInfo, xsdOrgDocument.AttachmentType);
					context.SetPropertyInfoValueIfValueNotEmpty(orgDoc.OD_DefaultContactInfo, xsdOrgDocument.DefaultContact.ToString());
					context.SetPropertyInfoValueIfValueNotEmpty(orgDoc.OD_FilterLocalPortInfo, xsdOrgDocument.FilterLocalPort);
					context.SetPropertyInfoValueIfValueNotEmpty(orgDoc.OD_FilterForeignPortInfo, xsdOrgDocument.FilterForeignPort);
					context.SetPropertyInfoValueIfValueNotEmpty(orgDoc.OD_FilterShipmentModeInfo, xsdOrgDocument.FilterShipmentMode);
					context.SetPropertyInfoValueIfValueNotEmpty(orgDoc.OD_FilterDirectionInfo, xsdOrgDocument.FilterDirection);

					if (!xsdOrgDocument.OH_RelatedFilterByPartyPK.IsEmpty)
					{
						ZGuid filterByPartyPk = new ZGuid(xsdOrgDocument.OH_RelatedFilterByPartyPK);
						OrgHeader filterByParty = contactToUpdate.Factory.Load<OrgHeader>(filterByPartyPk);

						if (filterByParty != null)
						{
							orgDoc.OD_OH_RelatedFilterByParty = filterByPartyPk;
						}
					}

					if (!menuitem.IsEmpty)
					{
						orgDoc.OD_SU_MenuItem = menuitem;
					}
				}
			}

			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_IsActiveInfo, contactValue.IsActive.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_SalutationInfo, contactValue.Salutation);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_LanguageInfo, contactValue.Language);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_NotifyModeInfo, contactValue.NotifyMode);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_TitleInfo, contactValue.JobTitle);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_JobCategoryInfo, contactValue.JobCategory);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_PhoneInfo, contactValue.Phone);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_PhoneExtensionInfo, contactValue.PhoneExtension);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_FaxInfo, contactValue.Fax);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_MobileInfo, contactValue.Mobile);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_HomePhoneInfo, contactValue.HomePhone);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_PagerInfo, contactValue.Pager);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_OtherPhoneInfo, contactValue.OtherPhone);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_EmailInfo, contactValue.Email);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_AttachmentTypeInfo, contactValue.AttachmentType);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_WebAccessEnabledInfo, contactValue.WebAccessEnabled.ToString());

			if (!contactValue.OrgHeaderPK.IsEmpty)
			{
				contactToUpdate.OC_OH = new ZGuid(contactValue.OrgHeaderPK);
			}

			if (!contactValue.OrgAddressPK.IsEmpty)
			{
				contactToUpdate.OC_OA_OrgAddress = new ZGuid(contactValue.OrgAddressPK);
			}

			if (contactValue.Birthday != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_BirthdayInfo, (ZDateTime)contactValue.Birthday);
			}

			if (contactValue.WebContractSignedDate != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_WebContractSignedDateInfo, (ZDateTime)contactValue.WebContractSignedDate);
			}

			if (contactValue.YearJoinedIndustry != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_YearJoinedIndustryInfo, (ZDateTime)contactValue.YearJoinedIndustry);
			}

			if (contactValue.YearJoinedCompany != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_YearJoinedCompanyInfo, (ZDateTime)contactValue.YearJoinedCompany);
			}

			if (contactValue.DetailsVerified != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_DetailsVerifiedInfo, (ZDateTime)contactValue.DetailsVerified);
			}

			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_PersonalInfoInfo, contactValue.PersonalInfo);
			context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_ContactSourceInfo, contactValue.ContactSource);
		}

		#endregion

		#region Export

		public void ExportToValueObjectCollection(OrgHeaderForDataTransfer org, Xsd.SysMergeOrgContactCollection contactValueCollection, INotifications notifications)
		{
			ZQuery query = new ZQuery(OrgContactSchema.OC_OH, org.PK);
			OrgContact[] contacts = org.Factory.Load<OrgContact>(query);

			for (int i = 0; i < contacts.Length; i++)
			{
				OrgContact contact = contacts[i];
				Xsd.SysMergeOrgContact xsdContact = ExportToValueObject(contact, notifications);
				contactValueCollection.Add(xsdContact);
			}
		}

		public Xsd.SysMergeOrgContact ExportToValueObject(OrgContact contact, INotifications notifications)
		{
			Xsd.SysMergeOrgContact result = new Xsd.SysMergeOrgContact();

			result.OrgDocuments = new Xsd.SysMergeOrgDocumentCollection();

			foreach (OrgDocument orgDocument in contact.Documents)
			{
				if (orgDocument.MenuItem != null && orgDocument.MenuItem.SU_IsSystemDefined || orgDocument.OD_SU_MenuItem.IsEmpty)
				{
					Xsd.SysMergeOrgDocument xsdOrgDoc = result.OrgDocuments.AddNew();

					xsdOrgDoc.DocumentGroup = orgDocument.OD_DocumentGroup;
					xsdOrgDoc.SU_MenuItemPK = orgDocument.OD_SU_MenuItem.ToString();
					xsdOrgDoc.DeliverBy = orgDocument.OD_DeliverBy;
					xsdOrgDoc.AttachmentType = orgDocument.OD_AttachmentType;
					if (orgDocument.OD_DefaultContact)
					{
						xsdOrgDoc.DefaultContact = orgDocument.OD_DefaultContact;
						xsdOrgDoc.DefaultContactSpecified = true;
					}
					xsdOrgDoc.FilterLocalPort = orgDocument.OD_FilterLocalPort;
					xsdOrgDoc.FilterForeignPort = orgDocument.OD_FilterForeignPort;
					xsdOrgDoc.FilterShipmentMode = orgDocument.OD_FilterShipmentMode;
					xsdOrgDoc.FilterDirection = orgDocument.OD_FilterDirection;
					xsdOrgDoc.OH_RelatedFilterByPartyPK = orgDocument.OD_OH_RelatedFilterByParty.ToString();
					xsdOrgDoc.OrgContactPK = orgDocument.OD_OC.ToString();
				}
			}

			result.PK = contact.PK.ToString();

			if (contact.OC_IsActive)
			{
				result.IsActive = contact.OC_IsActive;
				result.IsActiveSpecified = true;
			}
			result.ContactName = contact.OC_ContactName;
			result.Salutation = contact.OC_Salutation;
			result.Language = contact.OC_Language;
			result.NotifyMode = contact.OC_NotifyMode;
			result.JobTitle = contact.OC_Title;
			result.JobCategory = contact.OC_JobCategory;
			result.Phone = contact.OC_Phone;
			result.PhoneExtension = contact.OC_PhoneExtension;
			result.Fax = contact.OC_Fax;
			result.Mobile = contact.OC_Mobile;
			result.HomePhone = contact.OC_HomePhone;
			result.Pager = contact.OC_Pager;
			result.OtherPhone = contact.OC_OtherPhone;
			result.Email = contact.OC_Email;
			result.AttachmentType = contact.OC_AttachmentType;
			result.WebAccessEnabled = contact.OC_WebAccessEnabled;
			result.ContactSource = contact.OC_ContactSource;
			result.PersonalInfo = contact.OC_PersonalInfo;

			result.OrgHeaderPK = contact.OC_OH.ToString();
			result.OrgAddressPK = contact.OC_OA_OrgAddress.ToString();
			result.OrgHeaderAddressOverridePK = contact.OC_OH_AddressOverride.ToString();

			if (!contact.OC_Birthday.IsEmpty)
			{
				result.Birthday = contact.OC_Birthday.ToDateTime();
				result.BirthdaySpecified = true;
			}

			if (!contact.OC_WebContractSignedDate.IsEmpty)
			{
				result.WebContractSignedDate = contact.OC_WebContractSignedDate.ToDateTime();
				result.WebContractSignedDateSpecified = true;
			}

			if (!contact.OC_YearJoinedIndustry.IsEmpty)
			{
				result.YearJoinedIndustry = contact.OC_YearJoinedIndustry.ToDateTime();
				result.YearJoinedIndustrySpecified = true;
			}

			if (!contact.OC_YearJoinedCompany.IsEmpty)
			{
				result.YearJoinedCompany = contact.OC_YearJoinedCompany.ToDateTime();
				result.YearJoinedCompanySpecified = true;
			}

			if (!contact.OC_DetailsVerified.IsEmpty)
			{
				result.DetailsVerified = contact.OC_DetailsVerified.ToDateTime();
				result.DetailsVerifiedSpecified = true;
			}

			return result;
		}

		#endregion
	}
}
