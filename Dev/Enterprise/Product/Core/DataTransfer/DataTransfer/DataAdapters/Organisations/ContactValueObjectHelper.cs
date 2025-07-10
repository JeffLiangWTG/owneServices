using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class ContactValueObjectHelper
	{
		public ContactValueObjectHelper(string errorContext)
		{
			this.ErrorContext = errorContext;
		}

		public ContactValueObjectHelper(string errorContext, IDocAddresses parent)
		{
			this.ErrorContext = errorContext;
			ParentDocAddressConsumer = parent;
		}

		public readonly string ErrorContext;

		#region Import

		public OrgContact FromContactReference(Xsd.ContactReference contactReference, IValueObjectImportContext context)
		{
			OrgContact result = null;
			if (contactReference != null && contactReference.IsSpecified)
			{
				OrgHeader organisation = context.FindOrCreateTempOrganisation(contactReference.Organisation, null, OrganisationTypes.None) as OrgHeader;
				if (organisation != null)
				{
					foreach (Xsd.OrgContact contactValue in contactReference.Organisation.OrganisationDetails.Contacts)
					{
						if (contactValue.Sequence == contactReference.ContactSequenceRef)
						{
							result = ImportFromValueObject(organisation, contactValue, context);
							break;
						}
					}
				}
			}
			return result;
		}

		public ZGuid FromContactReferenceGetContactPK(Xsd.ContactReference contactReference, IValueObjectImportContext context)
		{
			OrgContact contact = FromContactReference(contactReference, context);
			return contact == null ? ZGuid.Empty : contact.PK;
		}

		public ZString FromContactReferenceGetContactName(Xsd.ContactReference contactReference, IValueObjectImportContext context)
		{
			OrgContact contact = FromContactReference(contactReference, context);
			return contact == null ? ZString.Empty : contact.OC_ContactName;
		}

		public void ImportFromValueObjectCollection(Xsd.OrgContactCollection contactValueCollection,
			OrgHeader organisation,
			IValueObjectImportContext context,
			bool convertInvalidToEmpty = false)
		{
			if (contactValueCollection.IsSpecified)
			{
				using (organisation.ToggleShowSystemGeneratedContactsFlagTemporarily(true))
				{
					Dictionary<ZGuid, OrgContact> updatedContacts = new Dictionary<ZGuid, OrgContact>(contactValueCollection.Count);
					Dictionary<string, string> updatedContactEmails = new Dictionary<string, string>(contactValueCollection.Count);
					var initialContacts = organisation.Contacts.ToArray();

					for (int i = 0; i < contactValueCollection.Count; i++)
					{
						Xsd.OrgContact contactValue = contactValueCollection[i];
						OrgContact updatedContact = ImportFromValueObject(organisation, contactValue, context, convertInvalidToEmpty);

						if (updatedContact != null && !updatedContacts.ContainsKey(updatedContact.PK))
						{
							updatedContacts.Add(updatedContact.PK, updatedContact);
							if (!updatedContact.OC_Email.IsEmpty)
							{
								updatedContactEmails[updatedContact.OC_Email.Trim()] = null;
							}
						}
					}

					if (DeactivateExistingContact)
					{
						foreach (OrgContact contact in initialContacts)
						{
							// Note: contact.IsSystemGenerated is expensive since it hits the DB for the creation log - calling it last
							if (contact.OC_IsActive
								&& !updatedContacts.ContainsKey(contact.PK)
								&& (contact.OC_Email.IsEmpty || !updatedContactEmails.ContainsKey(contact.OC_Email.Trim())) // If there are duplicate existing contacts with same email address, keep them all rather than deactivate some.
								&& contact.IsSystemGenerated)
							{
								contact.OC_IsActive = false;
							}
						}
					}
				}
			}
		}

		protected virtual bool DeactivateExistingContact
		{
			get { return true; }
		}

		public OrgContact ImportFromValueObject(OrgHeader organisation, Xsd.OrgContact contactValue, IValueObjectImportContext context, bool convertInvalidToEmpty = false, bool activeOnly = false)
		{
			return ImportOne(organisation, organisation.Contacts, contactValue, context, convertInvalidToEmpty, activeOnly);
		}

		public OrgContact ImportOneWithoutLoadingOtherContacts(OrgHeader organisation, Xsd.OrgContact contactValue, IValueObjectImportContext context, bool convertInvalidToEmpty = false, bool activeOnly = false)
		{
			return ImportOne(organisation, null, contactValue, context, convertInvalidToEmpty, activeOnly);
		}

		public OrgContact ImportOneWithoutFindExisting(OrgHeader organisation, Xsd.OrgContact contactValue, IValueObjectImportContext context, bool convertInvalidToEmpty = false, bool activeOnly = false)
		{
			return ImportOne(organisation, null, contactValue, context, convertInvalidToEmpty, activeOnly, findExisting: false);
		}

		/// <summary>
		/// If contacts collection is given, use it for finding matches, otherwise use db queries.
		/// </summary>
		OrgContact ImportOne(OrgHeader organisation, OrgContactDependentCollection contacts, Xsd.OrgContact contactValue, IValueObjectImportContext context, bool convertInvalidToEmpty = false, bool activeOnly = false, bool findExisting = true)
		{
			OrgContact contactToUpdate = null;

			if (findExisting)
			{
				contactToUpdate = FindOne(organisation, contacts, contactValue, activeOnly);
			}

			if (contactToUpdate == null)
			{
				contactToUpdate = organisation.Factory.New<OrgContact>();
				var proposedContactName = contactValue.Name.SubstringSafe(0, Enterprise.ZArchitecture.Schema.OrgContactSchema.OC_ContactName.MaxLength).Trim();
				var contactName = OrgContactUniqueNameHelper.GenerateUniqueContactName(organisation, contacts, proposedContactName);

				// Set name and email before setting the Header to bypass expensive uniqueness check, etc.
				// We already know it is unique.
				context.SetPropertyInfoValueIfValueNotEmpty(contactToUpdate.OC_ContactNameInfo, contactName);
				SetProperty(contactToUpdate.OC_EmailInfo, contactValue.EmailAddress.Trim(), context, convertInvalidToEmpty);

				contactToUpdate.OC_OH = organisation.PK;
				contacts?.Add(contactToUpdate);
			}

			SetContactDetails(contactToUpdate, organisation, contactValue, context, convertInvalidToEmpty);

			return contactToUpdate;
		}

		public OrgContact FindOne(OrgHeader organisation, Xsd.OrgContact contactValue, bool activeOnly = false)
		{
			return FindOne(organisation, organisation.Contacts, contactValue, activeOnly);
		}

		public OrgContact FindOneWithoutLoadingOtherContacts(OrgHeader organisation, Xsd.OrgContact contactValue, bool activeOnly = false)
		{
			return FindOne(organisation, null, contactValue, activeOnly);
		}

		OrgContact FindOne(OrgHeader organisation, OrgContactDependentCollection contacts, Xsd.OrgContact contactValue, bool activeOnly = false)
		{
			OrgContact result = null;
			OrgContact[] foundContacts = System.Array.Empty<OrgContact>();
			ZString contactName = contactValue.Name.SubstringSafe(0, Enterprise.ZArchitecture.Schema.OrgContactSchema.OC_ContactName.MaxLength).Trim();

			if (contactValue.EmailAddressSpecified)
			{
				foundContacts = FindByContactEmail(organisation, contacts, contactValue.EmailAddress, activeOnly);
				if (foundContacts.Length == 1)
				{
					result = foundContacts[0];
				}
				else if (foundContacts.Length == 0)
				{
					foundContacts = FindByContactNameWithBlankEmailAddress(organisation, contacts, contactName, activeOnly);
					if (foundContacts.Length > 0)
					{
						result = foundContacts[0];
					}
				}
				else if (foundContacts.Length > 1)
				{
					if (contactValue.WebAccessEnable)
					{
						// Only one contact email can be active and have web access
						result = foundContacts.FirstOrDefault(x => x.OC_IsActive && x.OC_WebAccessEnabled);
					}

					if (result == null)
					{
						// prefer same name
						result = foundContacts.FirstOrDefault(x => x.OC_ContactName == contactName);
					}
				}
			}
			else
			{
				foundContacts = FindByContactName(organisation, contacts, contactName, activeOnly);
				if (foundContacts.Length > 0)
				{
					result = foundContacts[0];
				}
			}

			return result;
		}

		OrgContact[] Find(OrgHeader organisation, OrgContactDependentCollection contacts, ZQuery query)
		{
			if (contacts != null)
			{
				return (OrgContact[])contacts.Find(query);
			}
			else
			{
				query.AddToFilter(OrgContactSchema.OC_OH, organisation.PK);
				return organisation.Factory.Load<OrgContact>(query);
			}
		}

		OrgContact[] FindByContactEmail(OrgHeader organisation, OrgContactDependentCollection contacts, string contactEmail, bool activeOnly)
		{
			var query = new ZQuery(OrgContactSchema.OC_Email, contactEmail.Trim());
			if (activeOnly)
			{
				query.AddToFilter(OrgContactSchema.OC_IsActive, activeOnly);
			}
			return Find(organisation, contacts, query);
		}

		OrgContact[] FindByContactName(OrgHeader organisation, OrgContactDependentCollection contacts, string contactName, bool activeOnly)
		{
			var query = new ZQuery(OrgContactSchema.OC_ContactName, contactName);
			if (activeOnly)
			{
				query.AddToFilter(OrgContactSchema.OC_IsActive, activeOnly);
			}

			return Find(organisation, contacts, query);
		}

		OrgContact[] FindByContactNameWithBlankEmailAddress(OrgHeader organisation, OrgContactDependentCollection contacts, string contactName, bool activeOnly)
		{
			var query = new ZQuery(OrgContactSchema.OC_ContactName, contactName);
			query.AddToFilter(OrgContactSchema.OC_Email, "");
			if (activeOnly)
			{
				query.AddToFilter(OrgContactSchema.OC_IsActive, activeOnly);
			}

			return Find(organisation, contacts, query);
		}

		public void SetContactDetails(OrgContact contactToUpdate, OrgHeader organisation, Xsd.OrgContact contactValue, IValueObjectImportContext context, bool convertInvalidToEmpty)
		{
			SetProperty(contactToUpdate.OC_NotifyModeInfo, contactValue.NotifyMode, context, convertInvalidToEmpty);
			SetProperty(contactToUpdate.OC_AttachmentTypeInfo, contactValue.AttachmentType, context, convertInvalidToEmpty);
			SetProperty(contactToUpdate.OC_BirthdayInfo, contactValue.Birthday, context, convertInvalidToEmpty);
			SetProperty(contactToUpdate.OC_EmailInfo, contactValue.EmailAddress.Trim(), context, convertInvalidToEmpty);
			SetProperty(contactToUpdate.OC_Fax_FormattedInfo, contactValue.Fax, context, convertInvalidToEmpty);
			SetProperty(contactToUpdate.OC_HomePhone_FormattedInfo, GetNormalizedPhone(contactToUpdate, organisation, contactValue.HomePhone), context, convertInvalidToEmpty);
			SetProperty(contactToUpdate.OC_TitleInfo, contactValue.JobTitle, context, convertInvalidToEmpty);
			SetProperty(contactToUpdate.OC_LanguageInfo, contactValue.Language, context, convertInvalidToEmpty);
			SetProperty(contactToUpdate.OC_Mobile_FormattedInfo, GetNormalizedPhone(contactToUpdate, organisation, contactValue.Mobile), context, convertInvalidToEmpty);
			SetProperty(contactToUpdate.OC_OtherPhone_FormattedInfo, GetNormalizedPhone(contactToUpdate, organisation, contactValue.OtherPhone), context, convertInvalidToEmpty);
			SetProperty(contactToUpdate.OC_Pager_FormattedInfo, GetNormalizedPhone(contactToUpdate, organisation, contactValue.Pager), context, convertInvalidToEmpty);
			SetProperty(contactToUpdate.OC_Phone_FormattedInfo, GetNormalizedPhone(contactToUpdate, organisation, contactValue.Phone), context, convertInvalidToEmpty);
			SetProperty(contactToUpdate.OC_PhoneExtensionInfo, contactValue.PhoneExtension, context, convertInvalidToEmpty);
			SetProperty(contactToUpdate.OC_SalutationInfo, contactValue.Salutation, context, convertInvalidToEmpty);
			if (contactValue.WebAccessEnable && !contactToUpdate.OC_WebAccessEnabled && !contactToUpdate.OC_Email.IsEmpty)
			{
				// Already known to be valid
				using (contactToUpdate.GetValidationSuspender())
				{
					contactToUpdate.OC_WebAccessEnabled = contactValue.WebAccessEnable;
				}
			}

			SetProperty(contactToUpdate.OC_WebContractSignedDateInfo, contactValue.WebContractSignDate, context, convertInvalidToEmpty);
		}

		void SetProperty(ZPropertyInfo propertyInfo, string valueAsString, IValueObjectImportContext context, bool convertInvalidToEmpty)
		{
			context.SetPropertyInfoValueIfValueNotEmpty(propertyInfo, valueAsString);
			if (convertInvalidToEmpty && propertyInfo.HasErrors() && (!propertyInfo.BizObj.IsInDatabase || propertyInfo.HasChanges))
			{
				context.SetPropertyInfoValue(propertyInfo, string.Empty);
			}
		}

		void SetProperty(ZPropertyInfo propertyInfo, ZDateTime val, IValueObjectImportContext context, bool convertInvalidToEmpty)
		{
			context.SetPropertyInfoValueIfValueNotEmpty(propertyInfo, val);
			if (convertInvalidToEmpty && propertyInfo.HasErrors() && (!propertyInfo.BizObj.IsInDatabase || propertyInfo.HasChanges))
			{
				propertyInfo.Value = ZDateTime.Empty;
			}
		}

		static string GetNormalizedPhone(OrgContact contact, OrgHeader organisation, string phone)
		{
			string normalizedPhone = phone;

			if (!string.IsNullOrEmpty(phone))
			{
				string countryDialingCode = null;

				if (contact.BranchAddress != null && contact.BranchAddress.RelatedCountry != null)
				{
					countryDialingCode = contact.BranchAddress.RelatedCountry.RN_CountryDialingCode;
				}
				else if (organisation.ClosestPort != null && organisation.ClosestPort.Country != null)
				{
					countryDialingCode = organisation.ClosestPort.Country.RN_CountryDialingCode;
				}

				if (!string.IsNullOrEmpty(countryDialingCode))
				{
					if (!normalizedPhone.StartsWith("+") && !normalizedPhone.StartsWith(countryDialingCode))
					{
						normalizedPhone = countryDialingCode + normalizedPhone;
					}
				}

				if (!normalizedPhone.StartsWith("+"))
				{
					normalizedPhone = "+" + normalizedPhone;
				}
			}

			return normalizedPhone;
		}

		#endregion

		#region Export

		public IDocAddresses ParentDocAddressConsumer { get; set; }

		public Xsd.ContactReference ToContactReference(OrgContact contact, IValueObjectExportContext context)
		{
			Xsd.ContactReference result = null;
			if (contact != null)
			{
				result = new Xsd.ContactReference();
				result.Organisation = new OrganisationValueObjectDataAdapter(ParentDocAddressConsumer).ExportToValueObject(contact.Header, context);
				for (int i = 0; i < contact.Header.Contacts.Count; i++)
				{
					OrgContact currentContact = contact.Header.Contacts[i];
					if (currentContact.PK == contact.PK)
					{
						result.ContactSequenceRef = i + 1;
						break;
					}
				}
			}
			return result;
		}

		public void ExportToValueObjectCollection(OrgContactDependentCollection contacts, Xsd.OrgContactCollection contactValueCollection, IValueObjectExportContext context)
		{
			for (int i = 0; i < contacts.Count; i++)
			{
				OrgContact contact = contacts[i];
				if (!contact.OC_ContactName.IsEmpty && ContactMatchesDocAddress(contact))
				{
					Xsd.OrgContact newContact = ExportToValueObject(contact, context);
					contactValueCollection.Add(newContact);
				}
			}
		}

		protected virtual bool ContactMatchesDocAddress(OrgContact contact)
		{
			bool result = false;

			if (SystemDataRegistry.Instance.SimpleXMLExportFormat.Value &&
				ParentDocAddressConsumer != null && ParentDocAddressConsumer.DocAddresses.Count > 0)
			{
				foreach (JobDocAddress docAddress in ParentDocAddressConsumer.DocAddresses)
				{
					if (docAddress.ContactPK == contact.PK)
					{
						result = true;
						break;
					}
				}
			}
			else
			{
				result = true;
			}

			return result;
		}

		public Xsd.OrgContact ExportToValueObject(OrgContact contact, INotifications notifications)
		{
			Xsd.OrgContact result = new Xsd.OrgContact();
			result.Name = contact.OC_ContactName.Trim();
			result.Fax = contact.OC_Fax;
			result.WebContractSignDate = contact.OC_WebContractSignedDate;
			result.AttachmentType = contact.OC_AttachmentType;
			result.Phone = contact.OC_Phone;
			result.JobTitle = contact.OC_Title;
			result.PhoneExtension = contact.OC_PhoneExtension;
			result.NotifyMode = contact.OC_NotifyMode;
			result.Mobile = contact.OC_Mobile;
			result.Language = contact.OC_Language;
			result.EmailAddress = contact.OC_Email.Trim();
			result.Salutation = contact.OC_Salutation;
			result.WebAccessEnable = contact.OC_WebAccessEnabled;
			result.Birthday = contact.OC_Birthday;
			result.OtherPhone = contact.OC_OtherPhone;
			result.Pager = contact.OC_Pager;
			result.HomePhone = contact.OC_HomePhone;

			return result;
		}

		#endregion
	}
}
