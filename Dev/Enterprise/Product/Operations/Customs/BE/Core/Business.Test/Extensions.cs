using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.BE.Business.Testing;

public static class Extensions
{
	public static RefCusCodeList Create_CusCodeList_RefData(this UniversalReferenceTestDataHelper helper, string countryCode, string refDataType, string code)
	{
		var dateMIN = ZDateTime.Today.AddMonths(-1);
		var dateMAX = ZDateTime.Today.AddMonths(3);

		return helper.CreateCusCodeList(countryCode, refDataType, code, dateMIN, dateMAX);
	}

	public static RefCusCodeList CreateNewOrGetExistingCusCodeList(this UniversalReferenceTestDataHelper helper, ZString dataGroupingCode, ZString codeType, ZString code, ZString description)
	{
		return helper.CreateNewOrGetExistingCusCodeList(dataGroupingCode, codeType, code, description, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
	}

	public static RefCusCodeList CreateCusCodeList(this UniversalReferenceTestDataHelper helper, ZString dataGroupingCode, ZString codeType, ZString code, ZString description)
	{
		return helper.CreateCusCodeList(dataGroupingCode, codeType, code, description, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
	}

	public static void SetImport(this JobDeclaration jobDeclaration) => jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;

	public static void SetExport(this JobDeclaration jobDeclaration) => jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;

	public static void SetMisc(this JobDeclaration jobDeclaration) => jobDeclaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
}
