using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public sealed class KRAddressFormatter : AddressFormatter
	{
		public KRAddressFormatter(BusinessObjectFactory factory, OrgAddress address)
			: base(factory, address)
		{
			this.address = address;
		}

		readonly OrgAddress address;

		string representativeName;
		string buyerID;
		string businessRegNo;
		string unipassID;

		protected override void BuildAddress()
		{
			representativeName = address?.Header.GetRepresentativeName() ?? string.Empty;
			businessRegNo = GetRegistrationNumberAlongWithType(Constants.IdentificationType.BusinessRegNo);
			unipassID = GetRegistrationNumberAlongWithType(Constants.IdentificationType.UnipassIDForOrganization);
			buyerID = GetRegistrationNumberAlongWithType(Constants.IdentificationType.ForeignCompanyID);
			FormattedAddress = RemoveMultipleSpacesAndTrim(FormatInKoreaSpecificWay());

			if (!Env.Registry.OrgAllowMixedCase)
			{
				FormattedAddress = FormattedAddress.ToUpper();
			}
		}

		string FormatInKoreaSpecificWay()
		{
			string result = "";
			if (CountryCode == Core.Constants.CountryCodes.KoreaSouth)
			{
				result = FormatForKoreanOrganization();
			}
			else
			{
				result = FormatOther();
			}
			return result;
		}

		string GetRegistrationNumberAlongWithType(ZString type)
		{
			var idNumber = address?.Header.GetRegistrationNumber(type) ?? ZString.Empty;
			return idNumber.IsEmpty ? string.Empty : $"({type}){idNumber}";
		}

		string BuildIDNumbersAndTypesString()
		{
			var strBuilder = new ZStringBuilder();
			if (!string.IsNullOrEmpty(businessRegNo))
			{
				strBuilder.Append(businessRegNo);
			}
			if (!string.IsNullOrEmpty(unipassID))
			{
				strBuilder.Append(unipassID);
			}
			return strBuilder.ToStringWithDelimiterBetweenAppends(",");
		}

		string FormatForKoreanOrganization()
		{
			var strBuilder = new ZStringBuilder();
			strBuilder.Append(representativeName);
			strBuilder.Append(BuildIDNumbersAndTypesString());
			strBuilder.Append(Address1);
			strBuilder.Append(Address2);
			strBuilder.Append(PostCode);
			return strBuilder.ToStringWithDelimiterBetweenAppends(newLine);
		}

		string FormatOther()
		{
			var strBuilder = new ZStringBuilder();
			strBuilder.Append(buyerID);
			strBuilder.Append(Format());
			return strBuilder.ToStringWithDelimiterBetweenAppends(newLine);
		}
	}
}
