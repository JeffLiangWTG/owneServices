using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.MasterFiles.Business.OrgConstants;

namespace Enterprise.Customs.KR.Business
{
	public static class OrgHeaderExtensionMethods
	{
		public static ZString GetRepresentativeName(this OrgHeader organisation)
		{
			return organisation.GetRepresentativeContact()?.OC_ContactName ?? ZString.Empty;
		}

		public static ZString GetExtensionNumber(this OrgHeader organisation)
		{
			return organisation.GetRepresentativeContact()?.OC_PhoneExtension ?? ZString.Empty;
		}

		static OrgContact GetRepresentativeContact(this OrgHeader organisation)
		{
			return organisation.Contacts.Cast<OrgContact>().FirstOrDefault(x => x.Allocations.Cast<OrgContactAllocation>().FirstOrDefault(s => s.PC_Type == ContactAllocationType.CEOForKRCustoms) != null);
		}

		public static ZBool GetIsIndividual(this OrgHeader organisation)
		{
			return organisation.OH_Category == OrgConstants.Category.NaturalPersonIndividual;
		}

		public static ZString GetPhoneNumber(this OrgHeader organisation)
		{
			return organisation.MainAddress.OA_Phone;
		}

		public static ZString GetAddressDetails(this OrgAddress organisation)
		{
			var strBuilder = new ZStringBuilder();
			if (!organisation.Address1.IsEmpty)
			{
				strBuilder.Append(organisation.Address1);
				if (!organisation.Address2.IsEmpty)
				{
					strBuilder.Append(organisation.Address2);
				}
			}
			return strBuilder.ToStringWithDelimiterBetweenAppends(" ");
		}

		public static ZString GetFormattedPostcode(this OrgAddress address)
		{
			return MessageFunctions.GetFormattedNumber(address.Postcode, new int[] { 0, 3 });
		}

		public static ZString GetRegistrationNumber(this OrgHeader organisation, string type)
		{
			var result = GetRegistrationNumberObject(organisation, type);
			return result?.OK_CustomsRegNo ?? ZString.Empty;
		}

		public static ZString GetRegistrationNumber(this OrgAddress address, string type)
		{
			var result = GetRegistrationNumberObject(address, type);
			return result?.OK_CustomsRegNo ?? ZString.Empty;
		}

		public static OrgCusCode GetRegistrationNumberObject(this OrgHeader organisation, string type)
		{
			var result = GetCustomsCodes(organisation, new string[] { type });
			return result.Length > 0 ? result[0] : null;
		}

		public static OrgCusCode GetRegistrationNumberObject(this OrgAddress address, string type)
		{
			var result = GetCustomsCodes(address, new string[] { type });
			return result.Length > 0 ? result[0] : null;
		}

		public static OrgCusCode[] GetRegistrationNumbers(this OrgHeader organisation, string[] types)
		{
			return GetCustomsCodes(organisation, types);
		}

		public static OrgCusCode[] GetRegistrationNumbers(this OrgAddress address, string[] types)
		{
			return GetCustomsCodes(address, types);
		}

		public static ZString GetRoadNameCode(this OrgAddress address)
		{
			return address.GetRegistrationNumber(MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.RoadNameCode);
		}

		public static ZString GetBuildingNumber(this OrgAddress address)
		{
			return address.GetRegistrationNumber(MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.BuildingNumber);
		}

		static OrgCusCode[] GetCustomsCodes(OrgHeader organisation, string[] types)
		{
			var result = new List<OrgCusCode>();
			var list = GetCustomsCodes(organisation);
			foreach (string regoNumberType in types)
			{
				var match = list.FirstOrDefault(x => x.OK_CodeType == regoNumberType);
				if (match != null)
				{
					result.Add(match);
				}
			}
			return result.ToArray();
		}

		static OrgCusCode[] GetCustomsCodes(OrgAddress address, string[] types)
		{
			var result = new List<OrgCusCode>();
			var list = GetCustomsCodes(address);
			foreach (string regoNumberType in types)
			{
				var match = list.FirstOrDefault(x => x.OK_CodeType == regoNumberType);
				if (match != null)
				{
					result.Add(match);
				}
			}
			return result.ToArray();
		}

		static OrgCusCode[] GetCustomsCodes(OrgHeader organisation)
		{
			var query = new ZQuery(OrgCusCodeSchema.OK_OH, organisation.PK);
			query.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, null);
			query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.KoreaSouth);
			return organisation.Factory.Load<OrgCusCode>(query);
		}

		static OrgCusCode[] GetCustomsCodes(this OrgAddress address)
		{
			var query = new ZQuery(OrgCusCodeSchema.OK_OH, address.OA_OH);

			var premiseAddressQuery = new ZQuery(OrgCusCodeSchema.OK_OA_PremisesAddress, null);
			premiseAddressQuery.AddToFilter(JoinCondition.Or, OrgCusCodeSchema.OK_OA_PremisesAddress, address.PK);

			query.AddToFilter(premiseAddressQuery);
			return address.Factory.Load<OrgCusCode>(query);
		}

		public static ZString GetRegistrationFirstMatchedBusinessOrIndividualID(this OrgHeader organisation)
		{
			var regoTypes = IOrganizationExtensionMethods.GetBusinessOrIndividualRegistrationNumberTypes(organisation.GetIsIndividual());
			var result = organisation.GetRegistrationNumbers(regoTypes);
			return result.Length > 0 ? result[0].OK_CustomsRegNo : ZString.Empty;
		}
		public static ZString GetRegistrationNumberFormattedIfRequired(this OrgAddress address, string type)
		{
			return FormatRegistrationIDNumber(address.GetRegistrationNumber(type), type);
		}

		public static ZString GetRegistrationNumberFormattedIfRequired(this OrgHeader organisation, string type)
		{
			return FormatRegistrationIDNumber(organisation.GetRegistrationNumber(type), type);
		}

		public static ZString FormatRegistrationIDNumber(string idNumber, string type)
		{
			switch (type)
			{
				case IdentificationType.BusinessRegNo:
					idNumber = MessageFunctions.GetFormattedNumber(idNumber, new int[] { 0, 3, 5 });
					break;
				case IdentificationType.UnipassIDForOrganization:
					idNumber = MessageFunctions.GetFormattedUnipassIDForOrganization(idNumber);
					break;
				case IdentificationType.KoreanRegNoForResident:
				case IdentificationType.CorporationCode:
					idNumber = MessageFunctions.GetFormattedNumber(idNumber, new int[] { 0, 6 });
					break;
			}
			return idNumber;
		}

		public static OrgAddress GetCustomsAddressThenMainAddress(this OrgHeader orgHeader)
		{
			return orgHeader.CustomsAddress ?? orgHeader.MainAddress;
		}
	}
}
