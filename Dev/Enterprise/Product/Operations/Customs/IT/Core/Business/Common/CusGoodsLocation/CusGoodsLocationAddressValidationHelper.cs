using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business;

public static class CusGoodsLocationAddressValidationHelper
{
	public static void ValidateE2_Email(EU.Business.CusGoodsLocationAddress cusGoodsLocationAddress)
	{
		var emailAddress = cusGoodsLocationAddress.E2_Email;
		if (emailAddress.IsEmpty || EmailAddressValidation.IsEmailAddressValid(emailAddress))
		{
			return;
		}

		cusGoodsLocationAddress.E2_EmailInfo.AddMessageError(GetInvalidEmailAddressMessageError());
	}

	public static void ValidateE2_Address1AndE2_Address2(EU.Business.CusGoodsLocationAddress cusGoodsLocationAddress)
	{
		DoMandatoryValidationForQualifierZ(cusGoodsLocationAddress, cusGoodsLocationAddress.E2_Address1AndE2_Address2Info);
	}

	public static void ValidateE2_RN_NKCountryCode(EU.Business.CusGoodsLocationAddress cusGoodsLocationAddress, bool validationCondition)
	{
		var countryCodeInfo = cusGoodsLocationAddress.E2_RN_NKCountryCodeInfo;
		if (validationCondition)
		{
			DoMandatoryValidationForQualifierZ(cusGoodsLocationAddress, countryCodeInfo);
		}
		ListValidation.MessageErrorIfInvalidCode(countryCodeInfo);
	}

	public static void ValidateE2_City(EU.Business.CusGoodsLocationAddress cusGoodsLocationAddress)
	{
		DoMandatoryValidationForQualifierZ(cusGoodsLocationAddress, cusGoodsLocationAddress.E2_CityInfo);
	}

	public static void ValidateE2_Contact(EU.Business.CusGoodsLocationAddress cusGoodsLocationAddress)
	{
		if (HasQualifierZ(cusGoodsLocationAddress.GoodsLocation) && cusGoodsLocationAddress.E2_Contact.IsEmpty && !cusGoodsLocationAddress.E2_Phone.IsEmpty)
		{
			cusGoodsLocationAddress.E2_ContactInfo.AddMessageError(GetNameAndPhoneAreRequiredMessageError());
		}
	}

	public static void ValidateE2_Phone(EU.Business.CusGoodsLocationAddress cusGoodsLocationAddress)
	{
		if (HasQualifierZ(cusGoodsLocationAddress.GoodsLocation) && cusGoodsLocationAddress.E2_Phone.IsEmpty && !cusGoodsLocationAddress.E2_Contact.IsEmpty)
		{
			cusGoodsLocationAddress.E2_PhoneInfo.AddMessageError(GetNameAndPhoneAreRequiredMessageError());
		}
	}

	#region Implementation

	static void DoMandatoryValidationForQualifierZ(EU.Business.CusGoodsLocationAddress cusGoodsLocationAddress, ZPropertyInfo propertyInfo)
	{
		var goodsLocation = cusGoodsLocationAddress.GoodsLocation;
		if (HasQualifierZ(goodsLocation))
		{
			MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
		}
	}

	static bool HasQualifierZ(CusGoodsLocation goodsLocation)
	{
		var qualifier = goodsLocation?.CGL_Qualifier ?? ZString.Empty;
		return qualifier == CusGoodsLocationQualifierList.Codes.Address;
	}

	static string GetNameAndPhoneAreRequiredMessageError()
	{
		return Res.GetString("7c22f927-b5ca-4ac5-9d2e-c8a7b75b93af", "Name and Phone Number must be both filled or both empty");
	}

	static string GetInvalidEmailAddressMessageError()
	{
		return Res.GetString("a5efbca4-19f2-409c-9b72-671d0f075c47", "Email Address is not valid");
	}

	#endregion
}
