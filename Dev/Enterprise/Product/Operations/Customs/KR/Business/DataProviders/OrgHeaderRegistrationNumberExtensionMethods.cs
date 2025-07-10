using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public static class OrganisationRegoNumberExtensionMethodsForDataProvider
	{
		public static IDNumberAndType[] GetRegistrationIDNumbers(this OrgHeader organisation, string[] types)
		{
			return GetIDNumbers(organisation.GetRegistrationNumbers(types));
		}
		public static IDNumberAndType[] GetRegistrationIDNumbers(this OrgAddress address, string[] types)
		{
			return GetIDNumbers(address.GetRegistrationNumbers(types));
		}
		public static IDNumberAndType[] GetRegistrationIDNumbersAndFirstMatchedBusinessOrIndividualID(this OrgHeader organisation, string[] types)
		{
			return GetIDNumbersAndFirstMatchedBusinessOrIndividualID(organisation.GetRegistrationNumbers(types), organisation);
		}
		public static IDNumberAndType[] GetRegistrationIDNumbersAndFirstMatchedBusinessOrIndividualID(this OrgAddress address, string[] types)
		{
			return GetIDNumbersAndFirstMatchedBusinessOrIndividualID(address.GetRegistrationNumbers(types), address.Header);
		}

		public static IDNumberAndType GetRegistrationFirstMatchedBusinessOrIndividualIDConverted(this OrgHeader organisation)
		{
			var regoTypes = IOrganizationExtensionMethods.GetBusinessOrIndividualRegistrationNumberTypes(organisation.GetIsIndividual());
			return FindFirstMatchedBusinessOrIndividualID(organisation.GetRegistrationNumbers(regoTypes));
		}

		public static IDNumberAndType GetRegistrationFirstMatchedBusinessOrIndividualIDConverted(this OrgAddress address)
		{
			var regoTypes = IOrganizationExtensionMethods.GetBusinessOrIndividualRegistrationNumberTypes(address.Header.GetIsIndividual());
			return FindFirstMatchedBusinessOrIndividualID(address.GetRegistrationNumbers(regoTypes));
		}

		static IDNumberAndType FindFirstMatchedBusinessOrIndividualID(OrgCusCode[] cusCodes)
		{
			var businessOrIndividualID = GetIDNumbers(cusCodes).FirstOrDefault();
			if (businessOrIndividualID != null && businessOrIndividualID.Type == IdentificationType.PassportNo)
			{
				businessOrIndividualID.Number = IOrganizationExtensionMethods.GetModifiedPassportNumber(businessOrIndividualID);
			}
			return businessOrIndividualID;
		}

		public static IDNumberAndType GetRegistrationIDNumber(this OrgHeader organisation, string type)
		{
			return organisation.GetRegistrationIDNumbers(new string[] { type }).FirstOrDefault();
		}

		public static IDNumberAndType GetRegistrationIDNumber(this OrgAddress address, string type)
		{
			return address.GetRegistrationIDNumbers(new string[] { type }).FirstOrDefault();
		}

		static IDNumberAndType[] GetIDNumbers(OrgCusCode[] cusCodes)
		{
			var result = new List<IDNumberAndType>();
			foreach (OrgCusCode cusCode in cusCodes)
			{
				result.Add(GetIDNumberAndType(cusCode));
			}
			return result.ToArray();
		}
		static IDNumberAndType[] GetIDNumbersAndFirstMatchedBusinessOrIndividualID(OrgCusCode[] cusCodes, OrgHeader organisation)
		{
			var result = new List<IDNumberAndType>();
			var idNumbers = GetIDNumbers(cusCodes);
			if (idNumbers.Length > 0)
			{
				result.AddRange(idNumbers);
			}

			var businessOrIndividualIDTypes = IOrganizationExtensionMethods.GetBusinessOrIndividualRegistrationNumberTypes(organisation.GetIsIndividual());
			var businessOrIndividualID = organisation.GetRegistrationNumbers(businessOrIndividualIDTypes).FirstOrDefault();
			if (businessOrIndividualID != null && !result.Any(x => x.Type == businessOrIndividualID.OK_CodeType))
			{
				result.Add(GetIDNumberAndType(businessOrIndividualID));
			}
			return result.ToArray();
		}

		static IDNumberAndType GetIDNumberAndType(OrgCusCode cusCode)
		{
			return new IDNumberAndType()
			{
				CountryOfIssue = cusCode.OK_RN_NKCodeCountry,
				Type = cusCode.OK_CodeType,
				Number = cusCode.OK_CustomsRegNo
			};
		}

		public static void SetRegistrationIDNumbers(this Organisation organisation, IDNumberAndType[] idNumbers)
		{
			var propertyInfos = typeof(Organisation).GetProperties().Where(x => x.GetCustomAttribute<OrganisationIdentificationTypeAttribute>() != null);

			foreach (var idNumber in idNumbers)
			{
				var propertyInfo = propertyInfos.FirstOrDefault(x => x.GetCustomAttribute<OrganisationIdentificationTypeAttribute>().IDNumberType == idNumber.Type);
				if (propertyInfo != null)
				{
					propertyInfo.SetValue(organisation, idNumber.Number.ToString());
				}
			}
		}
	}
}
