using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CAeMHDocAddressRequirement : JobDocAddressRequirement
	{
		public CAeMHDocAddressRequirement(BusinessObjectFactory factory)
			: base()
		{
			this.factory = factory;
			Initialize();
		}
		readonly BusinessObjectFactory factory;

		public CAeMHDocAddressRequirement(BusinessObjectFactory factory, DocAddressType docAddresType)
			: base(docAddresType)
		{
			this.factory = factory;
			Initialize();
		}

		void Initialize()
		{
			this.GetRegistrationNumberResult = GetRegNumResult;
			this.LookupsGovRegNumTypes = GetGovRegTypes;
			this.ValidateCompanyName = ValidateE2_CompanyName;
			this.ValidateAddress1 = ValidateE2_Address1;
			this.ValidateAddress2 = ValidateE2_Address2;
			this.ValidatePostCode = ValidateE2_PostCode;
			this.ValidateCity = ValidateE2_City;
			this.ValidateContact = ValidateE2_Contact;
			this.ValidateCountry = ValidateE2_RN_NKCoutryCode;
			this.ValidateGovRegNumType = ValidateE2_GovRegNumType;
			this.ValidateGovRegNo = ValidateE2_GovRegNum;
			this.ValidateAddressType = ValidateE2_AddressType;
			this.ValidateOrganisationPK = ValidateOrganization;
		}

		#region Gov Reg Lookups and Default

		RegistrationNumberResult GetRegNumResult(JobDocAddress docAddress)
		{
			return new RegistrationNumberResult(docAddress.Factory, true, delegate
				{
					var result = new RegistrationNumber() { NumberType = GetRegNumType(docAddress.E2_AddressType) };

					if (!result.NumberType.IsEmpty)
					{
						var organisation = docAddress.Organisation;
						if (organisation != null)
						{
							result.Number = GetCustomsRegNoMatchingAddressAndCodes(organisation, docAddress.E2_OA_Address, result.NumberType);
						}
					}
					else
					{
						var primaryRegNum = docAddress.Address?.Header?.PrimaryRegistrationNumber;
						if (primaryRegNum != null)
						{
							result.NumberType = primaryRegNum.NumberType;
							result.Number = primaryRegNum.Number;
						}
					}
					return result;
				});
		}

		ZString GetRegNumType(ZString docAddressType)
		{
			switch (docAddressType)
			{
				case DocAddressTypes.Codes.ImportBroker:
					return OrgCusCode.CACodeTypes.AccountSecurityCode;
				case DocAddressTypes.Codes.ReceivingForwarderAddress:
				case DocAddressTypes.Codes.Carrier:
					return OrgCusCode.CodeTypes.CarrierCode;
				case DocAddressTypes.Codes.Warehouse:
					return OrgCusCode.CodeTypes.ControlledPremisesID;
				default:
					return ZString.Empty;
			}
		}

		ZString GetCustomsRegNoMatchingAddressAndCodes(OrgHeader organisation, ZGuid address, ZString codeType)
		{
			OrgCusCode result = null;
			ZQuery codeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Canada);
			var orgCusCodeList = organisation.CustomsCodes.Find(codeFilter).OfType<OrgCusCode>();
			result = orgCusCodeList.FirstOrDefault(orgCusCode => orgCusCode.OK_OA_PremisesAddress == address);
			if (result == null)
			{
				result = orgCusCodeList.FirstOrDefault(orgCusCode => orgCusCode.OK_OA_PremisesAddress.IsEmpty);
			}

			return result != null ? result.OK_CustomsRegNo : ZString.Empty;
		}

		CodeDescriptionPairList GetGovRegTypes(JobDocAddressLookups lookups)
		{
			var addressType = lookups.Parent.E2_AddressType;
			return factory.GetCachedValue("CAeMHDocAddressGovRegType" + addressType, delegate
			{
				var result = new CodeDescriptionPairList();
				var regNumType = GetRegNumType(addressType);
				if (!regNumType.IsEmpty)
				{
					result.Add(CACodeList[regNumType]);
				}
				return result;
			});
		}

		CodeDescriptionPairList CACodeList
		{
			get { return fCACodeList ?? (fCACodeList = new OrgCodeLists().CustomsCodes_List(factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Canada))); }
		}
		CodeDescriptionPairList fCACodeList;

		#endregion

		#region Validation

		void ValidateE2_AddressType(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			MandatoryValidation.CheckEntered(docAddress.E2_AddressTypeInfo);
			ListValidation.ErrorIfInvalidCode(docAddress.E2_AddressTypeInfo, CAeMHDocAddress.GetAddressTypeList(factory));
			CheckIfAddressTypeAreAllowedToBeMultiple(docAddress);
		}

		void CheckIfAddressTypeAreAllowedToBeMultiple(JobDocAddress docAddress)
		{
			if (!AllowMultipleAddresses(docAddress.E2_AddressType))
			{
				var house = docAddress.Parent;
				if (house != null && house.DocAddresses.Cast<JobDocAddress>().FirstOrDefault(x => x.E2_AddressType == docAddress.E2_AddressType
					&& x.PK != docAddress.PK) != null)
				{
					docAddress.E2_AddressTypeInfo.AddError(string.Format(MultipleAddressTypesAreNotAllowed, docAddress.AddressDescription));
				}
			}
		}
		internal const string MultipleAddressTypesAreNotAllowed = "There can be only one {0}";

		ZBool AllowMultipleAddresses(ZString addressType)
		{
			return addressType == DocAddressTypes.Codes.NotifyParty || addressType == DocAddressTypes.Codes.ConsigneePickupDeliveryAddress;
		}

		void ValidateE2_GovRegNumType(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			if (ShouldValidateGovRegNum(docAddress.E2_AddressType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(docAddress.E2_GovRegNumTypeInfo);
				ListValidation.MessageErrorIfInvalidCode(docAddress.E2_GovRegNumTypeInfo);
			}
		}

		void ValidateE2_GovRegNum(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			if (ShouldValidateGovRegNum(docAddress.E2_AddressType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(docAddress.E2_GovRegNumInfo);
			}
		}

		ZBool ShouldValidateGovRegNum(ZString docAddressType)
		{
			switch (docAddressType)
			{
				case DocAddressTypes.Codes.Warehouse:
				case DocAddressTypes.Codes.ImportBroker:
				case DocAddressTypes.Codes.Carrier:
				case DocAddressTypes.Codes.ReceivingForwarderAddress:
					return true;
				default:
					return false;
			}
		}

		void ValidateE2_CompanyName(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			if (ShouldValidateAddress(docAddress.E2_AddressType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(docAddress.E2_CompanyNameInfo);
				InformEnglishAddressCharactersIfNecessary(docAddress.E2_CompanyNameInfo);
			}
		}

		void ValidateE2_Address1(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			if (ShouldValidateAddress(docAddress.E2_AddressType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(docAddress.E2_Address1Info);
				InformEnglishAddressCharactersIfNecessary(docAddress.E2_Address1Info);
			}
		}

		void ValidateE2_Address2(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			if (ShouldValidateAddress(docAddress.E2_AddressType))
			{
				InformEnglishAddressCharactersIfNecessary(docAddress.E2_Address2Info);
			}
		}

		void ValidateE2_PostCode(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			if (ShouldValidateAddress(docAddress.E2_AddressType))
			{
				InformEnglishAddressCharactersIfNecessary(docAddress.E2_PostcodeInfo);
			}
		}

		void ValidateE2_City(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			if (ShouldValidateAddress(docAddress.E2_AddressType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(docAddress.E2_CityInfo);
				InformEnglishAddressCharactersIfNecessary(docAddress.E2_CityInfo);
			}
		}

		void ValidateE2_Contact(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			if (ShouldValidateAddress(docAddress.E2_AddressType))
			{
				InformEnglishAddressCharactersIfNecessary(docAddress.E2_ContactInfo);
			}
		}

		void ValidateE2_RN_NKCoutryCode(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			if (ShouldValidateAddress(docAddress.E2_AddressType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(docAddress.E2_RN_NKCountryCodeInfo);
			}
		}

		ZBool ShouldValidateAddress(ZString docAddressType)
		{
			switch (docAddressType)
			{
				case DocAddressTypes.Codes.PlaceOfConsolidation:
				case DocAddressTypes.Codes.Consolidator:
				case DocAddressTypes.Codes.ConsigneeDocumentaryAddress:
				case DocAddressTypes.Codes.ConsignorDocumentaryAddress:
				case DocAddressTypes.Codes.ConsigneePickupDeliveryAddress:
				case DocAddressTypes.Codes.NotifyParty:
				case DocAddressTypes.Codes.ImportBroker:
				case DocAddressTypes.Codes.ReceivingForwarderAddress:
				case DocAddressTypes.Codes.Carrier:
				case DocAddressTypes.Codes.Warehouse:
					return true;
				default:
					return false;
			}
		}

		void ValidateOrganization(JobDocAddressValidation validation)
		{
			var docAddress = validation.Parent;
			if (ShouldValidateAddress(docAddress.E2_AddressType))
			{
				InformEnglishAddressCharactersIfNecessary(docAddress.E2_CompanyNameInfo, docAddress.OrganisationPKInfo);
				InformEnglishAddressCharactersIfNecessary(docAddress.E2_Address1Info, docAddress.OrganisationPKInfo);
				InformEnglishAddressCharactersIfNecessary(docAddress.E2_Address2Info, docAddress.OrganisationPKInfo);
				InformEnglishAddressCharactersIfNecessary(docAddress.E2_CityInfo, docAddress.OrganisationPKInfo);
				InformEnglishAddressCharactersIfNecessary(docAddress.E2_PostcodeInfo, docAddress.OrganisationPKInfo);
				InformEnglishAddressCharactersIfNecessary(docAddress.E2_ContactInfo, docAddress.OrganisationPKInfo);
			}
		}

		void InformEnglishAddressCharactersIfNecessary(ZPropertyInfo validateInfo, IZPropertyInfo propertyInfo = null)
		{
			if (propertyInfo == null)
			{
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(validateInfo);
			}
			else
			{
				var stringValue = (ZString)validateInfo.Value.ToString();
				if (!stringValue.IsEnglishOnlyOrEmpty && !stringValue.RemoveDiacritics().IsEnglishOnlyOrEmpty)
				{
					propertyInfo.AddMessageError(EnglishAddressCharactersValidation.GetNotificationMessage(validateInfo));
				}
			}
		}

		#endregion
	}
}
