using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class AddressValueObjectHelper
	{
		public AddressValueObjectHelper(string errorContext)
		{
			this.ErrorContext = errorContext;
		}

		public readonly string ErrorContext;

		#region Import

		public IMatchingAddress FromAddressReferenceSetupDocAddress(JobDocAddress docAddress, Xsd.AddressReference addressReference, IValueObjectImportContext context, OrganisationTypes orgTypeForMatching, string organisationSubType = "")
		{
			return FromAddressReference(docAddress, addressReference, context, orgTypeForMatching, organisationSubType);
		}

		public IMatchingAddress FromAddressReference(Xsd.AddressReference addressReference, IValueObjectImportContext context, OrganisationTypes orgTypeForMatching, string organisationSubType = "")
		{
			return FromAddressReference(null, addressReference, context, orgTypeForMatching, organisationSubType);
		}

		IMatchingAddress FromAddressReference(JobDocAddress docAddress, Xsd.AddressReference addressReference, IValueObjectImportContext context, OrganisationTypes orgTypeForMatching, ZString organisationSubType)
		{
			IMatchingAddress address = null;
			IOrgHeaderForMatching organisation = null;

			if (IsValidAddressContainer(addressReference))
			{
				// set the org type to be used later if org is not matched
				var orgType = orgTypeForMatching;
				var docAddressType = string.Empty;
				if (orgType == OrganisationTypes.None && docAddress != null)
				{
					if (docAddress.E2_AddressType == DocAddressTypes.Codes.ConsigneeDocumentaryAddress ||
							docAddress.E2_AddressType == DocAddressTypes.Codes.ImporterDocumentaryAddress ||
							docAddress.E2_AddressType == DocAddressTypes.Codes.BuyerDocumentaryAddress ||
							docAddress.E2_AddressType == DocAddressTypes.Codes.ConsigneeAddress)
					{
						orgType = OrganisationTypes.Consignee;
					}

					if (docAddress.E2_AddressType == DocAddressTypes.Codes.ConsignorDocumentaryAddress ||
							docAddress.E2_AddressType == DocAddressTypes.Codes.SupplierDocumentaryAddress)
					{
						orgType = OrganisationTypes.Consignor;
					}

					docAddressType = docAddress.E2_AddressType;
				}

				var orgSubType = organisationSubType.IsEmpty ? orgType.ToString() : organisationSubType.ToString();
				var unmatchOrgRecordCriteria = new UnmatchOrgRecordCriteria { OrganisationSubType = orgSubType, DocAddressType = docAddressType };
				organisation = context.FindOrCreateTempOrganisation(addressReference.Organisation, GetBusinessObjectFromJobDocAddress(docAddress), orgType, unmatchOrgRecordCriteria);
				if (organisation != null)
				{
					Xsd.OrgAddress addressValue = GetAddressValue(addressReference);
					if (addressValue != null)
					{
						address = FindOrgAddress(organisation, addressReference.Organisation.OrganisationDetails.Addresses, addressValue, context);
						if (address == null)
						{
							if (ShouldCreateNewAddress(context))
							{
								address = CreateOrUpdateFromValueObject(organisation, addressReference.Organisation.OrganisationDetails.Addresses, addressValue, context);
								EnsureAddressShortCodeIsUnique(address);
							}
							else if (addressValue == addressReference.Organisation.OrganisationDetails.Addresses.GetMainOrFirstAddress())
							{
								address = organisation.MainAddress;
							}
						}
					}
				}
			}

			if (docAddress != null) //update docAddress - use OrgAddress - fallback to org
			{
				if (address != null)
				{
					docAddress.E2_OA_Address = address.PK;
				}
				else if (organisation != null)
				{
					docAddress.OrganisationPK = organisation.PK;
				}
			}

			return address;
		}

		static void EnsureAddressShortCodeIsUnique(IMatchingAddress address)
		{
			OrgAddress orgAddress = address as OrgAddress;
			if (orgAddress != null)
			{
				var codes = orgAddress.GetUsageComments().ToArray();
				if (orgAddress.OA_Code.IsEmpty || !orgAddress.IsUniqueUsageComment(orgAddress.OA_Code, codes))
				{
					orgAddress.SetDefaultUsageComment(codes);
				}
			}
		}

		static bool ShouldCreateNewAddress(IValueObjectImportContext context)
		{
			var iAutoCreateAddressOnUnmatch = context.ImportingJob as IAutoCreateAddressOnUnmatch;
			return iAutoCreateAddressOnUnmatch != null && iAutoCreateAddressOnUnmatch.CreateOrgAddressOnUnmatch;
		}

		public ZGuid FromAddressReferenceGetAddressPK(Xsd.AddressReference addressReference, IValueObjectImportContext context)
		{
			var address = FromAddressReference(addressReference, context, OrganisationTypes.None);
			return address == null ? ZGuid.Empty : address.PK;
		}

		public ZString FromAddressReferenceGetAddressCode(Xsd.AddressReference addressReference, IValueObjectImportContext context)
		{
			var address = FromAddressReference(addressReference, context, OrganisationTypes.None);
			return address == null ? ZString.Empty : address.OA_Code;
		}

		public ZString FromAddressReferenceGetAddressCode(Xsd.AddressReference addressReference, IOrgHeaderForMatching organisation, IValueObjectImportContext context)
		{
			if (IsValidAddressContainer(addressReference) && (organisation != null))
			{
				Xsd.OrgAddress addressValue = GetAddressValue(addressReference);
				if (addressValue != null)
				{
					var address = MatchAddressFromOrganisation(organisation, addressValue, context);
					if (address != null)
					{
						return address.OA_Code;
					}
				}
			}
			return ZString.Empty;
		}

		public void ImportFromValueObjectCollection(Xsd.OrgAddressCollection addressValueCollection, IOrgHeaderForMatching organisation, IValueObjectImportContext context)
		{
			if (addressValueCollection.IsSpecified)
			{
				for (int i = 0; i < addressValueCollection.Count; i++)
				{
					Xsd.OrgAddress addressValue = addressValueCollection[i];
					CreateOrUpdateFromValueObject(organisation, addressValueCollection, addressValue, context);
				}
			}
			else
			{
				context.Notify(new ErrorNotification(ErrorType.RequiredFieldEmpty, Res.GetString("ee18df16-a60b-4ba0-b34d-bfcf29dace06", "Address on organization {0}", ErrorContext)));
			}
		}

		public IMatchingAddress CreateOrUpdateFromValueObject(IOrgHeaderForMatching organisation, Xsd.OrgAddressCollection allAddressValues, Xsd.OrgAddress addressValue, IValueObjectImportContext context)
		{
			if (organisation.Factory != context.Factory && organisation.Factory != null)
			{
				throw new InvalidOperationException("Factory for the Organisation should be the same as the Context's Factory");
			}

			var iAddressToUpdate = GetMatchingOrNewAddress(organisation, allAddressValues, addressValue, context);
			var orgAddress = iAddressToUpdate as OrgAddress;
			var addressToUpdate = orgAddress == null ? iAddressToUpdate as OrgAddressForMatching : null;
			if (iAddressToUpdate != null)
			{
				if (addressValue != allAddressValues.GetMainOrFirstAddress() && !addressValue.AddressCapabilityTypeSpecified && orgAddress != null)
				{
					orgAddress.AddressCapability.DisableAllCapabilities();
				}

				if (orgAddress != null)
				{
					context.SetPropertyInfoValue(orgAddress.OA_CompanyNameOverrideInfo, addressValue.CompanyName, addressValue.CompanyNameSpecified);
					context.SetPropertyInfoValue(orgAddress.OA_Address1Info, addressValue.AddressLine1, addressValue.AddressLine1Specified);
					context.SetPropertyInfoValue(orgAddress.OA_Address2Info, addressValue.AddressLine2, addressValue.AddressLine2Specified);
					context.SetPropertyInfoValue(orgAddress.OA_CityInfo, addressValue.CityOrSuburb, addressValue.CityOrSuburbSpecified);
					context.SetPropertyInfoValue(orgAddress.OA_EmailInfo, addressValue.Email, addressValue.EmailSpecified);
					context.SetPropertyInfoValue(orgAddress.OA_LanguageInfo, addressValue.Language, addressValue.LanguageSpecified);
					context.SetPropertyInfoValue(orgAddress.OA_PostCodeInfo, addressValue.PostCode, addressValue.PostCodeSpecified);
					context.SetPropertyInfoValue(orgAddress.OA_StateInfo, addressValue.StateOrProvince, addressValue.StateOrProvinceSpecified);
				}
				else
				{
					addressToUpdate.OA_CompanyNameOverride = addressValue.CompanyNameSpecified ?
							addressValue.CompanyName.Left(OrgAddress.Schema.OA_CompanyNameOverrideMaxLength) :
							addressToUpdate.OA_CompanyNameOverride.Left(OrgAddress.Schema.OA_CompanyNameOverrideMaxLength);
					addressToUpdate.OA_Address1 = addressValue.AddressLine1Specified ?
							addressValue.AddressLine1.Left(OrgAddress.Schema.OA_Address1MaxLength) :
							addressToUpdate.OA_Address1.Left(OrgAddress.Schema.OA_Address1MaxLength);
					addressToUpdate.OA_Address2 = addressValue.AddressLine2Specified ?
							addressValue.AddressLine2.Left(OrgAddress.Schema.OA_Address2MaxLength) :
							addressToUpdate.OA_Address2.Left(OrgAddress.Schema.OA_Address2MaxLength);
					addressToUpdate.OA_City = addressValue.CityOrSuburbSpecified ?
							addressValue.CityOrSuburb.Left(OrgAddress.Schema.OA_CityMaxLength) :
							addressToUpdate.OA_City.Left(OrgAddress.Schema.OA_CityMaxLength);
					addressToUpdate.OA_Email = addressValue.EmailSpecified ?
							addressValue.Email.Left(OrgAddress.Schema.OA_EmailMaxLength) :
							addressToUpdate.OA_Email.Left(OrgAddress.Schema.OA_EmailMaxLength);
					addressToUpdate.OA_Language = addressValue.LanguageSpecified ?
							addressValue.Language.Left(OrgAddress.Schema.OA_LanguageMaxLength) :
							addressToUpdate.OA_Language.Left(OrgAddress.Schema.OA_LanguageMaxLength);
					addressToUpdate.OA_PostCode = addressValue.PostCodeSpecified ?
							addressValue.PostCode.Left(OrgAddress.Schema.OA_PostCodeMaxLength) :
							addressToUpdate.OA_PostCode.Left(OrgAddress.Schema.OA_PostCodeMaxLength);
					addressToUpdate.OA_State = addressValue.StateOrProvinceSpecified ?
							addressValue.StateOrProvince.Left(OrgAddress.Schema.OA_StateMaxLength) :
							addressToUpdate.OA_State.Left(OrgAddress.Schema.OA_StateMaxLength);
				}

				if (addressValue.Location.IsSpecified ||
						(!addressValue.Location.IsSpecified && organisation.OH_RL_NKClosestPort.IsEmpty))
				{
					if (orgAddress != null)
					{
						context.SetPropertyInfoValue(orgAddress.OA_RL_NKRelatedPortCodeInfo, addressValue.Location.Value, ForeignKeyType.PortNK);
					}
					else
					{
						addressToUpdate.OA_RL_NKRelatedPortCode = addressValue.Location.Value.Left(OrgAddress.Schema.OA_RL_NKRelatedPortCodeMaxLength);
						addressToUpdate.PortName = addressValue.Location.City;
						addressToUpdate.CountryName = (NoResString)addressValue.Location.Country;
					}
				}

				if (addressValue.TelephoneNumbers.IsSpecified)
				{
					foreach (Xsd.TelephoneNumber number in addressValue.TelephoneNumbers)
					{
						if (number.NumberType == Xsd.TelephoneNumberNumberType.Business)
						{
							if (orgAddress != null)
							{
								context.SetPropertyInfoValue(orgAddress.OA_PhoneInfo, number.Value, number.ValueSpecified);
							}
							else
							{
								addressToUpdate.OA_Phone = number.ValueSpecified ?
										number.Value.Left(OrgAddress.Schema.OA_PhoneMaxLength) :
										addressToUpdate.OA_Phone.Left(OrgAddress.Schema.OA_PhoneMaxLength);
							}
						}
						else if (number.NumberType == Xsd.TelephoneNumberNumberType.Fax)
						{
							if (orgAddress != null)
							{
								context.SetPropertyInfoValue(orgAddress.OA_FaxInfo, number.Value, number.ValueSpecified);
							}
							else
							{
								addressToUpdate.OA_Fax = number.ValueSpecified ?
										number.Value.Left(OrgAddress.Schema.OA_FaxMaxLength) :
										addressToUpdate.OA_Fax.Left(OrgAddress.Schema.OA_FaxMaxLength);
							}
						}
						else if (number.NumberType == Xsd.TelephoneNumberNumberType.Mobile)
						{
							if (orgAddress != null)
							{
								context.SetPropertyInfoValue(orgAddress.OA_MobileInfo, number.Value, number.ValueSpecified);
							}
							else
							{
								addressToUpdate.OA_Mobile = number.ValueSpecified ?
										number.Value.Left(OrgAddress.Schema.OA_MobileMaxLength) :
										addressToUpdate.OA_Mobile.Left(OrgAddress.Schema.OA_MobileMaxLength);
							}
						}
					}
				}

				if (orgAddress != null)
				{
					SetCapabilities(context, orgAddress, addressValue);
					context.SetPropertyInfoValue(orgAddress.OA_CodeInfo, addressValue.AddressCode, addressValue.AddressCodeSpecified);
				}
				else
				{
					foreach (Xsd.AddressCapability cap in addressValue.AddressCapabilities)
					{
						if (cap.AddressType == Enterprise.DataTransfer.Xml.XsdVersion1.AddressCapabilityAddressType.MAIN)
						{
							addressToUpdate.SetMainAddress();
							break;
						}
					}
					addressToUpdate.OA_Code = addressValue.AddressCodeSpecified ?
							addressValue.AddressCode.Left(OrgAddress.Schema.OA_CodeMaxLength) :
							addressToUpdate.OA_Code.Left(OrgAddress.Schema.OA_CodeMaxLength);
				}
			}
			else
			{
				iAddressToUpdate = organisation.MainAddress;
			}

			return iAddressToUpdate;
		}

		IMatchingAddress GetMatchingOrNewAddress(IOrgHeaderForMatching organisation, Xsd.OrgAddressCollection allAddressValues, Xsd.OrgAddress addressValue, IValueObjectImportContext context)
		{
			IMatchingAddress result = null;

			var orgHeader = organisation as OrgHeader;
			if (orgHeader == null || !orgHeader.IsSystemDefinedOrganisation)
			{
				if (addressValue.AddressLine1.IsEmpty)
				{
					if (addressValue.AddressLine2.IsEmpty)
					{
						result = FindOrgAddress(organisation, allAddressValues, addressValue, context);
						if (result == null)
						{
							var warningMessage = GetInvalidAddressWarning(organisation, context);
							context.Notify(new WarningNotification(WarningType.Warning, warningMessage));
						}
					}
					else
					{
						addressValue.AddressLine1 = addressValue.AddressLine2;
						addressValue.AddressLine2 = "";
						result = FindOrCreateOrgAddress(organisation, allAddressValues, addressValue, context);
					}
				}
				else
				{
					result = FindOrCreateOrgAddress(organisation, allAddressValues, addressValue, context);
				}
			}

			return result;
		}

		static string GetInvalidAddressWarning(IOrgHeaderForMatching organisation, IValueObjectImportContext context)
		{
			string warningMessage;
			if (context.ImportingJob == null)
			{
				warningMessage = Res.GetString("b9731ec7-beeb-458e-bd06-1f9b2d869871", "Address Line 1 is required on Organization {0}", organisation.OH_Code);
			}
			else
			{
				var importingJobDescription = context.ImportingJob.IsInDatabase
						? " " + Res.GetString("28309f80-5a22-4b4d-a35c-32b3044b77f0", "on transaction {0}", context.ImportingJob.HumanReadableName)
						: "";

				warningMessage = Res.GetString("d6691571-10be-4f12-ad52-64b000e8da48",
						"Attempted to import an invalid Address (Missing Address Line 1) for Organization {0} ({1}){2}. Address set to Main Address for this Organization.",
						organisation.OH_Code, organisation.OH_FullName, importingJobDescription);
			}

			return warningMessage;
		}

		protected void SetCapabilities(IValueObjectImportContext context, OrgAddress addressToUpdate, Xsd.OrgAddress addressValue)
		{
			if (addressValue.AddressCapabilities.IsSpecified)
			{
				foreach (Xsd.AddressCapability addressCapability in addressValue.AddressCapabilities)
				{
					addressToUpdate.AddressCapability.SetCapabilityEnabled(addressCapability.AddressType.ToString());
				}
			}

			if (addressToUpdate.AddressCapability.IsEmpty)
			{
				addressToUpdate.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Miscellaneous);
			}
		}

		#endregion

		#region Export

		public Xsd.AddressReference ToAddressReference(OrgAddress selectedAddress, IValueObjectExportContext exportContext)
		{
			if (selectedAddress == null)
			{
				return null;
			}

			var result = new Xsd.AddressReference();
			var orgHeader = selectedAddress.Header;
			var organisationValueObjectDataAdapter = new OrganisationValueObjectDataAdapter(new OrgAddressesExporter(selectedAddress));
			result.Organisation = organisationValueObjectDataAdapter.ExportToValueObject(orgHeader, exportContext);
			result.Organisation = organisationValueObjectDataAdapter.ExportToValueObject(orgHeader, exportContext);

			foreach (Xml.XsdVersion1.OrgAddress xsdAddress in result.Organisation.OrganisationDetails.Addresses)
			{
				if (xsdAddress.AddressCode == selectedAddress.OA_Code)
				{
					result.AddressSequenceRef = xsdAddress.Sequence;
					break;
				}
			}

			if (result.Organisation.OrganisationDetails.Addresses.Count > 0 && result.AddressSequenceRef < 1)
			{
				throw new InvalidOperationException("Should have found a sequence");
			}

			return result;
		}

		public void ExportToValueObjectCollection(OrgAddress address, Xsd.OrgAddressCollection addressValueCollection, INotifications notifications)
		{
			addressValueCollection.Add(ExportToValueObject(address, notifications));
		}

		public void ExportToValueObjectCollection(OrgAddressDependentCollection addresses, Xsd.OrgAddressCollection addressValueCollection, INotifications notifications)
		{
			ExportToValueObjectCollection(null, addresses, null, addressValueCollection, notifications);
		}

		public void ExportToValueObjectCollection(OrgAddress selectedAddress, OrgAddressDependentCollection addresses, IDocAddresses parentDocAddressConsumer, Xsd.OrgAddressCollection addressValueCollection, INotifications notifications)
		{
			var addressesInExportOrder = GetExportAddresses(selectedAddress, addresses, parentDocAddressConsumer);

			foreach (var address in addressesInExportOrder)
			{
				var newAddress = ExportToValueObject(address, notifications);
				addressValueCollection.Add(newAddress);
			}
		}

		OrgAddress[] GetExportAddresses(OrgAddress selectedAddress, OrgAddressDependentCollection addresses, IDocAddresses parentDocAddressConsumer)
		{
			var result = new List<OrgAddress>();
			if (addresses.MainAddress != null)
			{
				result.Add(addresses.MainAddress);
			}

			if (selectedAddress != null && !result.Contains(selectedAddress))
			{
				result.Add(selectedAddress);
			}

			var filtedAddresses = addresses.OfType<OrgAddress>()
					.Where(a => a != addresses.MainAddress)
					.Where(a => a != selectedAddress)
					.Where(a => AddressMatchesDocAddress(a, parentDocAddressConsumer));

			result.AddRange(filtedAddresses);

			return result.ToArray();
		}

		bool AddressMatchesDocAddress(OrgAddress address, IDocAddresses parentDocAddressConsumer)
		{
			bool result = false;

			if (parentDocAddressConsumer != null && parentDocAddressConsumer.DocAddresses.Count > 0)
			{
				foreach (JobDocAddress docAddress in parentDocAddressConsumer.DocAddresses)
				{
					if (docAddress.E2_OA_Address == address.PK)
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

		public Xsd.OrgAddress ExportToValueObject(OrgAddress address, INotifications notifications)
		{
			Xsd.OrgAddress result = new Xsd.OrgAddress();
			result.AddressCapabilities = new Xsd.AddressCapabilityCollection();

			if (address.IsMainAddress)
			{
				result.AddressType = Xsd.OrgAddressAddressType.MAIN;
				result.AddressTypeSpecified = true;
			}

			SetAddressCapabilities(result.AddressCapabilities, address, notifications);

			if (!address.OA_CompanyNameOverride.IsEmpty)
			{
				result.CompanyName = address.OA_CompanyNameOverrideTruncated;
			}

			if (!address.OA_Address1.IsEmpty)
			{
				result.AddressLine1 = address.OA_Address1;
			}

			if (!address.OA_Address2.IsEmpty)
			{
				result.AddressLine2 = address.OA_Address2;
			}

			if (!address.OA_City.IsEmpty)
			{
				result.CityOrSuburb = address.OA_City;
			}

			if (!address.OA_State.IsEmpty)
			{
				result.StateOrProvince = address.OA_State;
			}

			if (!address.OA_PostCode.IsEmpty)
			{
				result.PostCode = address.OA_PostCode;
			}

			if (!address.OA_Email.IsEmpty)
			{
				result.Email = address.OA_Email;
			}

			if (!address.OA_Language.IsEmpty)
			{
				result.Language = address.OA_Language;
			}

			if (!address.OA_Code.IsEmpty)
			{
				result.AddressCode = address.OA_Code;
			}

			if (!address.OA_RL_NKRelatedPortCode.IsEmpty &&
					((address.OA_RL_NKRelatedPortCode != address.Header.OH_RL_NKClosestPort) || address.IsMainAddress))
			{
				result.Location.Value = address.OA_RL_NKRelatedPortCode;
			}

			result.TelephoneNumbers = new Xsd.TelephoneNumberCollection();
			SetPhoneNumber(address.OA_Phone, result.TelephoneNumbers, Xsd.TelephoneNumberNumberType.Business);
			SetPhoneNumber(address.OA_Mobile, result.TelephoneNumbers, Xsd.TelephoneNumberNumberType.Mobile);
			SetPhoneNumber(address.OA_Fax, result.TelephoneNumbers, Xsd.TelephoneNumberNumberType.Fax);

			return result;
		}

		void SetAddressCapabilities(Xsd.AddressCapabilityCollection addressCapabilities, OrgAddress address, INotifications notifications)
		{
			if (address.IsMainAddress)
			{
				Xsd.AddressCapability xsdCapability = addressCapabilities.AddNew();
				xsdCapability.AddressType = Xsd.AddressCapabilityAddressType.MAIN;
				xsdCapability.AddressTypeSpecified = true;
			}
			foreach (OrgAddressCapabilityWrapper capability in address.AddressCapability)
			{
				if (capability.Enabled)
				{
					Xsd.AddressCapability xsdCapability = addressCapabilities.AddNew();
					xsdCapability.AddressType = AddressTypeXmlMappings.Instance.GetExternalCode(capability, ErrorContext, notifications);
					xsdCapability.IsMainAddress = (capability.Main) ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
					xsdCapability.AddressTypeSpecified = true;
					xsdCapability.IsMainAddressSpecified = true;
				}
			}
		}

		void SetPhoneNumber(ZString phoneNumber, Xsd.TelephoneNumberCollection telephoneNumberCollection, Xsd.TelephoneNumberNumberType numberType)
		{
			if (!phoneNumber.IsEmpty)
			{
				Xsd.TelephoneNumber xSDPhoneNumber = telephoneNumberCollection.AddNew();
				xSDPhoneNumber.Value = phoneNumber;
				xSDPhoneNumber.NumberType = numberType;
			}
		}

		#endregion

		#region Implementation

		protected IMatchingAddress FindOrgAddress(IOrgHeaderForMatching organisation, Xsd.OrgAddressCollection allAddressValues, Xsd.OrgAddress addressValue, IValueObjectImportContext context)
		{
			IMatchingAddress result = null;

			if (IsValidAddressContainer(addressValue))
			{
				result = MatchAddressFromOrganisation(organisation, addressValue, context);
				if (result == null)
				{
					var addressToCompareTo = !organisation.IsInDatabase && organisation.OH_IsTempAccount
							? allAddressValues.GetMainOrFirstAddress()
							: allAddressValues.GetAddressWithMainCapability();
					if (addressValue == addressToCompareTo)
					{
						result = organisation.MainAddress;
					}
				}
			}

			return result;
		}

		protected IMatchingAddress FindOrCreateOrgAddress(IOrgHeaderForMatching organisation, Xsd.OrgAddressCollection allAddressValues, Xsd.OrgAddress addressValue, IValueObjectImportContext context)
		{
			var result = FindOrgAddress(organisation, allAddressValues, addressValue, context);
			if (result == null && IsValidAddressContainer(addressValue))
			{
				result = organisation.AddNewAddress();
			}

			return result;
		}

		protected IMatchingAddress MatchAddressFromOrganisation(IOrgHeaderForMatching organisation, Xsd.OrgAddress addressValue, IValueObjectImportContext context)
		{
			IMatchingAddress result = null;
			if (IsValidAddressContainer(addressValue))
			{
				foreach (var currentAddress in organisation.Addresses)
				{
					if (GetIsMatchingAddress(currentAddress, addressValue, organisation, context))
					{
						result = currentAddress;
						break;
					}
				}

				if (result == null)
				{
					foreach (var currentAddress in organisation.Addresses)
					{
						if (GetIsFuzzyMatching(currentAddress, addressValue))
						{
							result = currentAddress;
							break;
						}
					}
				}
			}
			return result;
		}

		public Xsd.OrgAddress GetAddressValue(Xsd.AddressReference addressReference)
		{
			foreach (Xsd.OrgAddress addressValue in addressReference.Organisation.OrganisationDetails.Addresses)
			{
				if (addressValue.Sequence == addressReference.AddressSequenceRef)
				{
					return addressValue;
				}
			}
			return null;
		}

		protected virtual bool GetIsMatchingAddress(IMatchingAddress currentAddress, Xsd.OrgAddress addressValue, IOrgHeaderForMatching organisation, IValueObjectImportContext context)
		{
			return addressValue.AddressLine1.EqualsIgnoringCase(currentAddress.OA_Address1) && addressValue.AddressLine2.EqualsIgnoringCase(currentAddress.OA_Address2) ||
					addressValue.AddressLine1.EqualsIgnoringCase(currentAddress.OA_Address1) && addressValue.AddressCode.EqualsIgnoringCase(currentAddress.OA_Code) &&
					(string.IsNullOrEmpty(currentAddress.OA_Address2) || string.IsNullOrEmpty(addressValue.AddressLine2));
		}

		protected virtual bool GetIsFuzzyMatching(IMatchingAddress currentAddress, Xsd.OrgAddress addressValue)
		{
			string xsdAddressLine1 = AddressLineFuzzyMatch.GetStringForFuzzyComparing(addressValue.AddressLine1);
			string orgAddressLine1 = AddressLineFuzzyMatch.GetStringForFuzzyComparing(currentAddress.OA_Address1);

			string xsdAddressLine2 = AddressLineFuzzyMatch.GetStringForFuzzyComparing(addressValue.AddressLine2);
			string orgAddressLine2 = AddressLineFuzzyMatch.GetStringForFuzzyComparing(currentAddress.OA_Address2);

			return xsdAddressLine1.CompareTo(orgAddressLine1) == 0 && xsdAddressLine2.CompareTo(orgAddressLine2) == 0;
		}

		protected bool IsValidAddressContainer(IValueObject addressContainer)
		{
			return (addressContainer != null) && addressContainer.IsSpecified;
		}

		BusinessObject GetBusinessObjectFromJobDocAddress(JobDocAddress docAddress)
		{
			if (docAddress != null && docAddress.Parent != null && docAddress.Parent.DocAddresses != null)
			{
				return docAddress.Parent.DocAddresses.Master;
			}
			return null;
		}

		#endregion
	}
}
