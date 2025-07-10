using System.Text;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public static class AdjustmentDocHelper
	{
		public static ZString AddressForImporterFormatted(OrgAddress orgAddress)
		{
			if (orgAddress == null)
			{
				return ZString.Empty;
			}
			return AddressForImporterFormatted(orgAddress.CompanyName, orgAddress.Address1, orgAddress.Address2, orgAddress.City, orgAddress.StateCode, orgAddress.Postcode, orgAddress.CountryName);
		}

		public static ZString AddressForImporterFormatted(JobDocAddress docAddress)
		{
			return AddressForImporterFormatted(docAddress.CompanyName, docAddress.Address1, docAddress.Address2, docAddress.City, docAddress.StateCode, docAddress.Postcode, docAddress.Country?.Description ?? ZString.Empty);
		}

		static ZString AddressForImporterFormatted(ZString companyName, ZString address1, ZString address2, ZString cityName, ZString stateCode, ZString postCode, ZString countryName)
		{
			var result = new StringBuilder();
			result.AppendLine(companyName.SubstringSafe(0, Constant.TruncatedInfoOnOneLine).TrimEnd());
			var address = ((ZString)(address1 + " " + address2)).SubstringSafe(0, Constant.TruncatedInfoOnOneLine).Trim();
			if (address.Length > 0)
			{
				result.AppendLine(address);
			}
			var cityStatePostCode = ZString.Empty;
			cityStatePostCode = cityName.SubstringSafe(0, Constant.CityName_MaxLength).Trim();
			cityStatePostCode += stateCode.Length > 0 ? (ZString)(" " + (stateCode.SubstringSafe(0, Constant.StateName_MaxLength)).Trim()) : ZString.Empty;
			cityStatePostCode += postCode.Length > 0 ? (ZString)(" " + postCode) : ZString.Empty;
			result.AppendLine(cityStatePostCode);
			result.AppendLine(countryName);
			return result.ToString().Trim();
		}

		public static MasterFiles.Integration.IDocAddress AddressForImporter(OrgHeader orgHeader)
		{
			if (orgHeader != null)
			{
				return orgHeader.CustomsAddress ?? orgHeader.MainAddress;
			}
			return null;
		}

		public static ZString AddressForMailToWithFullAddress(OrgAddress docAddress)
		{
			var result = new StringBuilder();
			result.AppendLine(docAddress.CompanyName.SubstringSafe(0, Constant.TruncatedInfoOnOneLine).TrimEnd());
			var address = ((ZString)(docAddress.Address1.Trim() + " " + docAddress.Address2.Trim())).SubstringSafe(0, Constant.TruncatedInfoOnOneLine).Trim();
			if (address.Length > 0)
			{
				result.AppendLine(address);
			}
			var cityStatePostCode = ZString.Empty;
			cityStatePostCode = docAddress.City.SubstringSafe(0, Constant.CityName_MaxLength).Trim();
			cityStatePostCode += docAddress.StateCode.Length > 0 ? (ZString)(" " + docAddress.StateCode.SubstringSafe(0, Constant.StateName_MaxLength)) : ZString.Empty;
			cityStatePostCode += docAddress.Postcode.Length > 0 ? (ZString)(" " + docAddress.Postcode) : ZString.Empty;
			result.AppendLine(cityStatePostCode);
			result.AppendLine(docAddress.CountryName);

			return result.ToString().Trim();
		}

		public static ZString AddressForVendorFormatted(OrgAddress orgAddress)
		{
			return AddressForVendorFormatted(orgAddress.CompanyName, orgAddress.Address1, orgAddress.Address2, orgAddress.City, orgAddress.StateCode, orgAddress.Postcode, orgAddress.CountryName, orgAddress.Country?.Code ?? ZString.Empty);
		}

		public static ZString AddressForVendorFormatted(JobDocAddress docAddress)
		{
			return AddressForVendorFormatted(docAddress.CompanyName, docAddress.Address1, docAddress.Address2, docAddress.City, docAddress.StateCode, docAddress.Postcode, docAddress.Country?.Description ?? ZString.Empty, docAddress.Country?.Code ?? ZString.Empty);
		}

		static ZString AddressForVendorFormatted(ZString companyName, ZString address1, ZString address2, ZString cityName, ZString stateCode, ZString postCode, ZString countryName, ZString countryCode)
		{
			var result = new StringBuilder();
			result.AppendLine(companyName.SubstringSafe(0, Constant.TruncatedInfoOnOneLine).TrimEnd());

			var cityStatePostCode = ZString.Empty;
			if (countryCode == Core.Constants.CountryCodes.UnitedStates)
			{
				cityStatePostCode += stateCode.Length > 0 ? (stateCode.SubstringSafe(0, Constant.StateName_MaxLength)).Trim() : ZString.Empty;
				cityStatePostCode += postCode.Length > 0 ? (ZString)(" " + postCode) : ZString.Empty;
				result.AppendLine(cityStatePostCode);
			}
			else
			{
				cityStatePostCode = cityName.SubstringSafe(0, Constant.CityName_MaxLength).Trim();
				cityStatePostCode += stateCode.Length > 0 ? (ZString)(" " + (stateCode.SubstringSafe(0, Constant.StateName_MaxLength)).Trim()) : ZString.Empty;
				cityStatePostCode += postCode.Length > 0 ? (ZString)(" " + postCode) : ZString.Empty;
				cityStatePostCode += postCode.Length > 0 ? (ZString)(" " + countryName) : ZString.Empty;
				result.AppendLine(cityStatePostCode);
			}

			return result.ToString().Trim();
		}

		public static class Constant
		{
			public const int TruncatedInfoOnOneLine = 30;
			public const int CityName_MaxLength = 13;
			public const int StateName_MaxLength = 13;
		}
	}
}
