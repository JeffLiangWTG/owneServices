using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class DocAddressValueObjectHelper
	{
		public DocAddressValueObjectHelper(string errorContext)
		{
			this.ErrorContext = errorContext;
		}

		public readonly string ErrorContext;

		#region Import

		public void ImportFromValueObjectCollection(Xsd.DocAddressCollection docAddressValueCollection, JobDocAddressDependentCollection docAddresses, IValueObjectImportContext context)
		{
			if (docAddressValueCollection.IsSpecified)
			{
				if (context != null && context.OrganisationMatching != null)
				{
					context.OrganisationMatching.OrgAddressSorter = new OrgAddressSorter(docAddressValueCollection);
				}

				ImportFromValueObjectCollectionCore(docAddressValueCollection, docAddresses, context);
			}
		}

		protected virtual void ImportFromValueObjectCollectionCore(Xsd.DocAddressCollection docAddressValueCollection, JobDocAddressDependentCollection docAddresses, IValueObjectImportContext context)
		{
			foreach (Xsd.DocAddress docAddressValue in docAddressValueCollection)
			{
				CreateOrUpdateFromValueObject(docAddresses, docAddressValue, context);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public void ImportFromValueObject(Xsd.DocAddress docAddressValue, JobDocAddress docAddressToUpdate, IValueObjectImportContext context, OrganisationTypes orgTypeForMatching = OrganisationTypes.None, string organisationSubType = "")
		{
			if (docAddressValue.AddressReference.IsSpecified)
			{
				AddressValueObjectHelper addressHelper = new AddressValueObjectHelper(ErrorContext);
				addressHelper.FromAddressReferenceSetupDocAddress(docAddressToUpdate, docAddressValue.AddressReference, context, orgTypeForMatching, organisationSubType);
			}
			else
			{
				docAddressToUpdate.E2_AddressOverride = true;
				context.SetPropertyInfoValue(docAddressToUpdate.E2_CompanyNameInfo, docAddressValue.CompanyName, docAddressValue.CompanyNameSpecified);
				context.SetPropertyInfoValue(docAddressToUpdate.E2_Address1Info, docAddressValue.AddressLine1, docAddressValue.AddressLine1Specified);
				context.SetPropertyInfoValue(docAddressToUpdate.E2_Address2Info, docAddressValue.AddressLine2, docAddressValue.AddressLine2Specified);
				context.SetPropertyInfoValue(docAddressToUpdate.E2_CityInfo, docAddressValue.CityOrSuburb, docAddressValue.CityOrSuburbSpecified);
				context.SetPropertyInfoValue(docAddressToUpdate.E2_PostcodeInfo, docAddressValue.PostCode, docAddressValue.PostCodeSpecified);
				context.SetPropertyInfoValue(docAddressToUpdate.E2_StateInfo, docAddressValue.StateOrProvince, docAddressValue.StateOrProvinceSpecified);
				context.SetPropertyInfoValue(docAddressToUpdate.E2_RN_NKCountryCodeInfo, docAddressValue.CountryCode, ForeignKeyType.CountryNK);
				docAddressToUpdate.E2_IsResidential = docAddressValue.IsResidential;

				if (docAddressValue.RegistrationNumberSpecified)
				{
					string numberType = OrgCusCodeXmlMappings.Instance.GetEnterpriseCode(docAddressValue.RegistrationNumber.NumberType, string.Format("DocAddress '{0}'", docAddressValue.CompanyName), context);
					context.SetPropertyInfoValue(docAddressToUpdate.E2_GovRegNumTypeInfo, numberType, docAddressValue.RegistrationNumber.NumberSpecified);
					context.SetPropertyInfoValue(docAddressToUpdate.E2_GovRegNumInfo, docAddressValue.RegistrationNumber.Number, docAddressValue.RegistrationNumber.NumberSpecified);
				}
			}

			context.SetPropertyInfoValue(docAddressToUpdate.E2_ContactInfo, docAddressValue.ContactName, docAddressValue.ContactNameSpecified);
			context.SetPropertyInfoValue(docAddressToUpdate.E2_EmailInfo, docAddressValue.Email, docAddressValue.EmailSpecified);

			if (docAddressValue.TelephoneNumbers.IsSpecified)
			{
				foreach (Xsd.TelephoneNumber number in docAddressValue.TelephoneNumbers)
				{
					if (number.NumberType == Xsd.TelephoneNumberNumberType.Business)
					{
						context.SetPropertyInfoValue(docAddressToUpdate.E2_PhoneInfo, number.Value, number.ValueSpecified);
					}
					else if (number.NumberType == Xsd.TelephoneNumberNumberType.Fax)
					{
						context.SetPropertyInfoValue(docAddressToUpdate.E2_FaxInfo, number.Value, number.ValueSpecified);
					}
				}
			}
		}

		public JobDocAddress CreateOrUpdateFromValueObject(JobDocAddressDependentCollection docAddresses, Xsd.DocAddress docAddressValue, IValueObjectImportContext context, OrganisationTypes orgTypeForMatching = OrganisationTypes.None, string organisationSubType = "")
		{
			DocAddressType addressType = DocAddressTypes.GetDocAddressTypeFromCode(context.Factory, docAddressValue.AddressType.ToString());
			JobDocAddress docAddressToUpdate = docAddresses.FindOrCreateWithDocAddressType(addressType);
			ImportFromValueObject(docAddressValue, docAddressToUpdate, context, orgTypeForMatching, organisationSubType);
			return docAddressToUpdate;
		}

		#endregion

		#region Export

		public void ExportToValueObjectCollection(JobDocAddressDependentCollection docAddresses, Xsd.DocAddressCollection docAddressValueCollection, IValueObjectExportContext exportContext)
		{
			JobDocAddressDependentCollection sortedAddresses = docAddresses;
			sortedAddresses.Sort(JobDocAddressSchema.E2_AddressType.Name);
			JobDocAddress[] addresses = (JobDocAddress[])sortedAddresses.ToArray(typeof(JobDocAddress));

			foreach (JobDocAddress docAddress in addresses)
			{
				if (docAddress.IsValidAddress && docAddress.DocAddressType != DocAddressType.None)
				{
					Xsd.DocAddress newAddress = ExportToValueObject(docAddress, exportContext);
					docAddressValueCollection.Add(newAddress);
				}
			}
		}

		public Xsd.DocAddress ExportToValueObject(JobDocAddress docAddress, IValueObjectExportContext exportContext)
		{
			return ExportToValueObject<Xsd.DocAddress>(docAddress, exportContext);
		}

		public T ExportToValueObject<T>(JobDocAddress docAddress, IValueObjectExportContext exportContext)
			where T : Xsd.DocAddress, new()
		{
			T result = new T();

			result.AddressType = DocAddressTypeXmlMappings.Instance.GetExternalCode(docAddress, ErrorContext, exportContext);
			result.AddressTypeSpecified = !docAddress.E2_AddressType.IsEmpty;

			if (docAddress.E2_AddressOverride)
			{
				if (!docAddress.E2_CompanyName.IsEmpty)
				{
					result.CompanyName = docAddress.E2_CompanyNameTruncated;
				}

				if (!docAddress.E2_Address1.IsEmpty)
				{
					result.AddressLine1 = docAddress.E2_Address1;
				}

				if (!docAddress.E2_Address2.IsEmpty)
				{
					result.AddressLine2 = docAddress.E2_Address2;
				}

				if (!docAddress.E2_City.IsEmpty)
				{
					result.CityOrSuburb = docAddress.E2_City;
				}

				if (!docAddress.E2_State.IsEmpty)
				{
					result.StateOrProvince = docAddress.E2_State;
				}

				if (!docAddress.E2_Postcode.IsEmpty)
				{
					result.PostCode = docAddress.E2_Postcode;
				}

				if (!docAddress.E2_RN_NKCountryCode.IsEmpty)
				{
					result.CountryCode = docAddress.E2_RN_NKCountryCode;
				}

				if (docAddress.E2_IsResidential)
				{
					result.IsResidential = true;
					result.IsResidentialSpecified = true;
				}

				if (!docAddress.E2_GovRegNum.IsEmpty)
				{
					result.RegistrationNumber.Number = docAddress.E2_GovRegNum;
					if (!docAddress.E2_GovRegNumType.IsEmpty)
					{
						result.RegistrationNumber.NumberType = OrgCusCodeXmlMappings.Instance.GetExternalCode(docAddress.E2_GovRegNumType, "DocAddress " + docAddress.E2_CompanyName, exportContext);
					}
					result.RegistrationNumber.CountryOfRegistration = result.CountryCode;
				}
			}

			if (!docAddress.E2_Contact.IsEmpty)
			{
				result.ContactName = docAddress.E2_Contact;
			}

			if (!docAddress.E2_Email.IsEmpty)
			{
				result.Email = docAddress.E2_Email;
			}

			result.TelephoneNumbers = new Xsd.TelephoneNumberCollection();

			if (!docAddress.E2_Phone.IsEmpty)
			{
				Xsd.TelephoneNumber businessNumber = result.TelephoneNumbers.AddNew();
				businessNumber.NumberType = Xsd.TelephoneNumberNumberType.Business;
				businessNumber.Value = docAddress.E2_Phone;
			}

			if (!docAddress.E2_Fax.IsEmpty)
			{
				Xsd.TelephoneNumber faxNumber = result.TelephoneNumbers.AddNew();
				faxNumber.NumberType = Xsd.TelephoneNumberNumberType.Fax;
				faxNumber.Value = docAddress.E2_Fax;
			}

			if (!docAddress.E2_AddressOverride)
			{
				result.AddressReference = new AddressValueObjectHelper(ErrorContext).ToAddressReference(docAddress.Address, exportContext);
			}

			return result;
		}

		#endregion

		#region ExportToOrganisationValueObject

		public Xsd.Organisation ExportToOrganisationValueObject(JobDocAddress jobDocAddress, IValueObjectExportContext exportContext, bool shouldExportContact = true)
		{
			Xsd.Organisation result = null;
			if (jobDocAddress != null)
			{
				if (!jobDocAddress.E2_AddressOverride && jobDocAddress.HasRealOrganisation)
				{
					result = new OrganisationValueObjectDataAdapter(jobDocAddress.Parent).ExportToValueObject(jobDocAddress.Organisation, exportContext);
				}
				else if (jobDocAddress.E2_AddressOverride)
				{
					result = ExportToOrganisationFromOverridenAddress(jobDocAddress, shouldExportContact);
				}
			}

			return result;
		}

		Xsd.Organisation ExportToOrganisationFromOverridenAddress(JobDocAddress jobDocAddress, bool shouldExportContact = true)
		{
			Xsd.Organisation organisation = new Xsd.Organisation();
			organisation.EDICode = "UNMATCHED";
			organisation.OwnerCode = "UNMATCHED";

			Xsd.OrganisationDetail organisationDetails = new Xsd.OrganisationDetail();
			organisationDetails.Name = jobDocAddress.E2_CompanyNameTruncated;
			organisation.OrganisationDetails = organisationDetails;

			Xsd.OrgAddress address = organisation.OrganisationDetails.Addresses.AddNew(Xsd.OrgAddressAddressType.MAIN);
			SetAddressCapabilities(address);

			address.Language = Constants.Languages.English;

			address.AddressLine1 = jobDocAddress.E2_Address1;
			address.AddressLine2 = jobDocAddress.E2_Address2;
			address.AddressCode = jobDocAddress.E2_Address1;

			address.CityOrSuburb = jobDocAddress.E2_City;
			address.StateOrProvince = jobDocAddress.E2_State;
			address.PostCode = jobDocAddress.E2_Postcode;
			address.Email = jobDocAddress.E2_Email;

			address.TelephoneNumbers = new Xsd.TelephoneNumberCollection();
			SetPhoneNumber(jobDocAddress.E2_Phone, address.TelephoneNumbers, Xsd.TelephoneNumberNumberType.Business);
			SetPhoneNumber(jobDocAddress.E2_Fax, address.TelephoneNumbers, Xsd.TelephoneNumberNumberType.Fax);

			if (!jobDocAddress.E2_RN_NKCountryCode.IsEmpty)
			{
				string dummyLocation = jobDocAddress.E2_RN_NKCountryCode + "ZZZ";
				address.Location.Value = dummyLocation;
				organisation.OrganisationDetails.Location.Value = dummyLocation;
			}

			if (!jobDocAddress.E2_Contact.IsEmpty && shouldExportContact)
			{
				organisation.OrganisationDetails.Contacts.Add(new Xsd.OrgContact()
				{
					Name = jobDocAddress.E2_Contact,
					EmailAddress = jobDocAddress.E2_Email,
					Phone = jobDocAddress.E2_Phone,
					Fax = jobDocAddress.E2_Fax,

					Language = Constants.Languages.English,
					NotifyMode = Constants.ContactNotifyModes.Email,
					AttachmentType = OrgConstants.AttachmentType.PDF
				});
			}

			return organisation;
		}

		void SetAddressCapabilities(Xsd.OrgAddress address)
		{
			address.AddressCapabilities = new Xsd.AddressCapabilityCollection();
			address.AddressCapabilities.Add(new Xsd.AddressCapability() { AddressType = Xsd.AddressCapabilityAddressType.MAIN });
			address.AddressCapabilities.Add(new Xsd.AddressCapability() { AddressType = Xsd.AddressCapabilityAddressType.OFC, IsMainAddress = Xsd.TrueFalse.@true });
		}

		void SetPhoneNumber(ZString phoneNumber, Xsd.TelephoneNumberCollection phoneNumbers, Xsd.TelephoneNumberNumberType numberType)
		{
			if (!phoneNumber.IsEmpty)
			{
				phoneNumbers.Add(new Xsd.TelephoneNumber() { Value = phoneNumber, NumberType = numberType });
			}
		}

		#endregion
	}
}
