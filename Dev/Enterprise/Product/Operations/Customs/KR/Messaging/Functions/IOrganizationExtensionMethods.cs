using CargoWise.Types;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	public static class IOrganizationExtensionMethods
	{
		public static ZString GetAddressDetails(this IOrganization organisation)
		{
			var strBuilder = new ZStringBuilder();
			if (!organisation.AddressLine1.IsEmpty)
			{
				strBuilder.Append(organisation.AddressLine1);
				if (!organisation.AddressLine2.IsEmpty)
				{
					strBuilder.Append(organisation.AddressLine2);
				}
			}
			return strBuilder.ToStringWithDelimiterBetweenAppends(" ");
		}

		public static ZString GetRegistrationNumberFormattedIfRequired(this IOrganization organisation, string type)
		{
			var result = organisation.GetRegistrationNumber(type);

			switch (type)
			{
				case IdentificationType.BusinessRegNo:
					result = MessageFunctions.GetFormattedNumber(result, new int[] { 0, 3, 5 });
					break;
				case IdentificationType.UnipassIDForOrganization:
					result = MessageFunctions.GetFormattedUnipassIDForOrganization(result);
					break;
				case IdentificationType.KoreanRegNoForResident:
				case IdentificationType.CorporationCode:
					result = MessageFunctions.GetFormattedNumber(result, new int[] { 0, 6 });
					break;
			}

			return result;
		}

		public static string[] GetBusinessOrIndividualRegistrationNumberTypes(bool isIndividual)
		{
			return isIndividual ?
				new string[] { IdentificationType.KoreanRegNoForResident, IdentificationType.PassportNo, IdentificationType.KoreanRegNoForForeigner, IdentificationType.UnipassIDForIndividual }
				: new string[] { IdentificationType.BusinessRegNo };
		}

		public static string GetRegistrationNumber(this IOrganization organisation, string type)
		{
			return organisation.GetFirstMatchedRegistrationNumber(new string[] { type });
		}

		public static IDNumberAndType GetRegistrationTypeAndNumber(this IOrganization organisation, string type)
		{
			return organisation.GetFirstMatchedRegistrationTypeNumber(new string[] { type });
		}

		public static ZString GetFirstMatchedRegistrationNumber(this IOrganization organisation, string[] types)
		{
			return organisation.GetFirstMatchedRegistrationTypeNumber(types)?.Number ?? ZString.Empty;
		}

		public static IDNumberAndType GetBusinessOrIndividualRegistrationNumber(this IOrganization organisation)
		{
			return organisation.GetFirstMatchedRegistrationTypeNumber(GetBusinessOrIndividualRegistrationNumberTypes(organisation.IsIndividual));
		}

		public static IDNumberAndType GetFirstMatchedRegistrationTypeNumber(this IOrganization organisation, string[] types)
		{
			var result = organisation.GetFirstMatchedRegistrationTypeNumberUnconverted(types);
			if (result != null)
			{
				if (result.Type == IdentificationType.PassportNo)
				{
					return new IDNumberAndType()
					{
						Type = PassportNoTypeForKRC,
						Number = GetModifiedPassportNumber(result),
						CountryOfIssue = result.CountryOfIssue
					};
				}
				else if (result.Type == IdentificationType.BusinessRegNo)
				{
					return new IDNumberAndType()
					{
						Type = BusinessNoTypeForKRC,
						Number = result.Number,
						CountryOfIssue = result.CountryOfIssue
					};
				}
				else if (result.Type == IdentificationType.CorporationCode)
				{
					return new IDNumberAndType()
					{
						Type = CorporationNoTypeForKRC,
						Number = result.Number,
						CountryOfIssue = result.CountryOfIssue
					};
				}
			}
			return result;
		}
		static IDNumberAndType GetFirstMatchedRegistrationTypeNumberUnconverted(this IOrganization organisation, string[] types)
		{
			var result = new IDNumberAndType();

			if (organisation != null)
			{
				foreach (var regoType in types)
				{
					result.Type = regoType;
					result.CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth;

					switch (regoType)//please keep them in an alphabetical order
					{
						case IdentificationType.BuildingNumber:
							result.Number = organisation.BuildingNumber;
							break;
						case IdentificationType.BusinessRegNo:
							result.Number = organisation.BusinessRegNo;
							break;
						case IdentificationType.ForeignCompanyID:
							result.Number = organisation.ForeignCompanyID;
							break;
						case IdentificationType.CertificateOfOriginExporterNumber:
							result.Number = organisation.CertificateOfOriginExporterNumber;
							break;
						case MasterFiles.Business.OrgCusCode.CodeTypes.CarrierCode:
							result.Number = organisation.CarrierCode;
							break;
						case IdentificationType.CorporationCode:
							result.Number = organisation.CorporationCode;
							break;
						case IdentificationType.KoreanRegNoForForeigner:
							result.Number = organisation.KoreanRegNoForForeigner;
							break;
						case IdentificationType.KoreanRegNoForResident:
							result.Number = organisation.KoreanRegNoForResident;
							break;
						case IdentificationType.OfficeID:
							result.Number = organisation.OfficeID;
							break;
						case IdentificationType.PassportNo:
							result.Number = organisation.PassportNo;
							break;
						case IdentificationType.RoadNameCode:
							result.Number = organisation.RoadNameCode;
							break;
						case IdentificationType.UnipassIDForIndividual:
							result.Number = organisation.UnipassIDForIndividual;
							break;
						case IdentificationType.UnipassIDForOrganization:
							result.Number = organisation.UnipassIDForOrganization;
							break;
						default:
							break;
					}

					if (!string.IsNullOrEmpty(result.Number))
					{
						break;
					}
				}
			}
			return result.Number.IsEmpty ? null : result;
		}

		public const string RegNoForResidentKRC = "01";
		public const string PassportNoTypeForKRC = "02";
		public const string BusinessNoTypeForKRC = "04";
		public const string CorporationNoTypeForKRC = "09";
		const string ForeignPassportInitial = "F";

		public static string GetModifiedPassportNumber(IDNumberAndType number)
		{
			var result = number.Number;

			if (number.CountryOfIssue != Core.Constants.CountryCodes.KoreaSouth && !number.Number.StartsWith(ForeignPassportInitial + number.CountryOfIssue))
			{
				result = ForeignPassportInitial + number.CountryOfIssue + number.Number;
			}

			return result;
		}
	}
}
