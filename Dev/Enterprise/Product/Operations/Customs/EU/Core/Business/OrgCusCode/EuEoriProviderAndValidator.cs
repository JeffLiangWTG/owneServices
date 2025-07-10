using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public static class EuEoriProviderAndValidator
	{
		static IEuOrgCusCodeHelper GetEuOrgCusCodeHelper(BusinessObjectFactory factory)
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "EuOrgCusCodeHelper_{0}", countryCode), () =>
			{
				IEuOrgCusCodeHelper result = null;
				var key = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
				var types = ObjectFactory.Get<Hashtable>("IEuOrgCusCodeHelper");

				if (!string.IsNullOrEmpty(key))
				{
					var objectHandle = (ObjectHandle)types[key];
					result = (IEuOrgCusCodeHelper)objectHandle?.GetObject();
				}

				if (result == null)
				{
					var objectHandle = (ObjectHandle)types[Core.Constants.CountryCodes.EuropeanUnion];
					result = (IEuOrgCusCodeHelper)objectHandle?.GetObject();
				}
				return result;
			});
		}

		public static ZString GetEuIdentificationNumber(this JobDocAddress docAddress)
		{
			var result = ZString.Empty;

			if (docAddress != null)
			{
				result = GetEuOrgCusCodeHelper(docAddress.Factory).GetEuIdentificationNumber(docAddress);
			}

			return result;
		}

		public static ZString GetEuIdentificationNumber(this JobDocAddress docAddress, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false)
		{
			var result = ZString.Empty;

			if (docAddress != null)
			{
				result = GetEuOrgCusCodeHelper(docAddress.Factory).GetEuIdentificationNumber(docAddress, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched);
			}

			return result;
		}

		public static ZString GetEuIdentificationNumber(this OrgAddress address, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false)
		{
			var result = ZString.Empty;

			if (address != null)
			{
				result = GetEuOrgCusCodeHelper(address.Factory).GetEuIdentificationNumber(address, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched);
			}

			return result;
		}

		public static ZString GetEuIdentificationNumber(this OrgHeader organisation, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false)
		{
			var result = ZString.Empty;

			if (organisation != null)
			{
				result = GetEuOrgCusCodeHelper(organisation.Factory).GetEuIdentificationNumber(organisation, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched);
			}

			return result;
		}

		public static ZString GetEORI(this OrgAddress address, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false)
		{
			var result = ZString.Empty;

			if (address != null)
			{
				result = GetEuOrgCusCodeHelper(address.Factory).GetEORI(address, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched);
			}

			return result;
		}

		public static ZString GetEORI(this OrgHeader organisation, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false)
		{
			var result = ZString.Empty;

			if (organisation != null)
			{
				result = GetEuOrgCusCodeHelper(organisation.Factory).GetEORI(organisation, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched);
			}

			return result;
		}

		public static ZString GetUnprefixedEORI(this OrgHeader organisation, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = true)
		{
			var result = ZString.Empty;

			if (organisation != null)
			{
				result = GetEuOrgCusCodeHelper(organisation.Factory).GetUnprefixedEORI(organisation, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched);
			}

			return result;
		}

		public static ZString GetUnprefixedEORI(this OrgAddress address, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = true)
		{
			var result = ZString.Empty;

			if (address != null)
			{
				result = GetEuOrgCusCodeHelper(address.Factory).GetUnprefixedEORI(address, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched);
			}

			return result;
		}

		public static ZString PrivateEoriReg(BusinessObjectFactory factory) => GetEuOrgCusCodeHelper(factory).PrivateEoriReg;

		public static ZString UnregForEori(BusinessObjectFactory factory) => GetEuOrgCusCodeHelper(factory).UnregForEori;

		public static bool ValidEORIorTCUIFormat(ZString eoriOrTCUI, BusinessObjectFactory factory)
		{
			return GetEuOrgCusCodeHelper(factory).ValidEORIorTCUIFormat(eoriOrTCUI, factory);
		}

		public static ZString GetRegoCodeOfThisOrg(this OrgHeader organisation, string typeOfCodeEoriTurnEtc, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false)
		{
			var result = ZString.Empty;

			if (organisation != null)
			{
				result = GetEuOrgCusCodeHelper(organisation.Factory).GetRegoCodeOfThisOrg(organisation, typeOfCodeEoriTurnEtc, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched);
			}

			return result;
		}

		public static ZString GetRegoCodeOfThisAddress(this OrgAddress address, string typeOfCodeEoriTurnEtc, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false)
		{
			var result = ZString.Empty;

			if (address != null)
			{
				result = GetEuOrgCusCodeHelper(address.Factory).GetRegoCodeOfThisAddress(address, typeOfCodeEoriTurnEtc, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched);
			}

			return result;
		}

		public static (ZString CountryCode, ZString RegistrationNumber) GetEuIdentificationNumberComponents(this OrgHeader organisation)
		{
			var result = (ZString.Empty, ZString.Empty);

			if (organisation != null)
			{
				result = GetEuOrgCusCodeHelper(organisation.Factory).GetEuIdentificationNumberComponents(organisation);
			}

			return result;
		}

		public static ZString GetEUVATCodeOfThisOrg(this OrgHeader organisation)
		{
			var result = ZString.Empty;

			if (organisation != null)
			{
				result = GetEuOrgCusCodeHelper(organisation.Factory).GetEUVATCodeOfThisOrg(organisation);
			}

			return result;
		}

		public static IEnumerable<ZString> GetEUVATCodesOfThisOrg(this OrgHeader organisation)
		{
			var result = Enumerable.Empty<ZString>();

			if (organisation != null)
			{
				result = GetEuOrgCusCodeHelper(organisation.Factory).GetEUVATCodesOfThisOrg(organisation);
			}

			return result;
		}

		public static ZString GetBranchSuffixesForBox44(CusEntryHeader entryHeader)
		{
			var result = ZString.Empty;

			if (entryHeader != null)
			{
				result = GetEuOrgCusCodeHelper(entryHeader.Factory).GetBranchSuffixesForBox44(entryHeader);
			}

			return result;
		}

		/// <summary>
		/// Gets a list looking like (123, AG),(456, BR),(789, BR)
		/// </summary>
		public static List<EoriSuffixAndType> GetBranchSuffixesListForBox44(CusEntryHeader entryHeader)
		{
			var result = new List<EoriSuffixAndType>();

			if (entryHeader != null)
			{
				result = GetEuOrgCusCodeHelper(entryHeader.Factory).GetBranchSuffixesListForBox44(entryHeader);
			}

			return result;
		}
	}
}
