using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	static class OrganisationValueObjectDataAdapterHelper
	{
		public static bool NumberIsAUGSTAndABNIsSpecified(Xsd.RegistrationNumber number, Xsd.RegistrationNumberCollection registrationNumbers)
		{
			return IsAUGST(number) && NumbersContainsAUABN(registrationNumbers);
		}

		public static bool IsUSDeprecatedSAN(Xsd.RegistrationNumber number, ZString orgCode, IValueObjectImportContext context)
		{
			var result = false;
			if (number.NumberType == Enterprise.DataTransfer.Xml.XsdVersion1.RegistrationNumberTypes.SAN && number.CountryOfRegistration == Core.Constants.CountryCodes.UnitedStates)
			{
				context.Add(new WarningNotification(WarningType.Warning, Res.GetString("316DC74A-F8BE-4895-9DC5-472E5AA3EB26",
						"Special Address Notification is obsolete. The number on {0} cannot be imported. Please use CBPF 4811 Notify Party or CBPF 4811 Notify Party ID on Organization -> Config -> US Defaults instead.", orgCode)));
				result = true;
			}
			return result;
		}

		static bool NumbersContainsAUABN(Xsd.RegistrationNumberCollection registrationNumbers)
		{
			return registrationNumbers.FindRegistrationNumber(Enterprise.DataTransfer.Xml.XsdVersion1.RegistrationNumberTypes.ABN, Constants.CountryCodes.Australia) != null;
		}

		public static bool IsAUGST(Xsd.RegistrationNumber number)
		{
			return number.CountryOfRegistration == Constants.CountryCodes.Australia && number.NumberType == Enterprise.DataTransfer.Xml.XsdVersion1.RegistrationNumberTypes.GST;
		}

		public static string GetNumberType(Xsd.RegistrationNumber number, string oh_code, IValueObjectImportContext context)
		{
			string numberType = OrgCusCodeXmlMappings.Instance.GetEnterpriseCode(number.NumberType.ToString(), Res.GetString("6c6ac08b-1f7a-47bd-9330-6c39e05fd186", "Organization with code '{0}'", oh_code), context);
			return IsAUGST(number) ? OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber : numberType;
		}
	}
}
