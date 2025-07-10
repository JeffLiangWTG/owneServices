using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.AE.Business.Testing;

public class UAEUniversalReferenceTestHelper : UniversalReferenceTestDataHelper
{
	public UAEUniversalReferenceTestHelper(BusinessObjectFactory factory) : base(factory)
	{
	}

	public void InitialiseRefDataWithCusCodeType(ZString typeCode, ZString typeDescription)
	{
		TestCaseHelper.ClearTable(RefCusCodeListAttribute.Schema.TableName);
		TestCaseHelper.ClearTable(RefCusCodeList.Schema.TableName);
		TestCaseHelper.ClearTable(RefCusCodeType.Schema.TableName);

		CreateCusCodeType(typeCode, typeDescription, Core.Constants.CountryCodes.UnitedArabEmirates);
	}

	public void SetupServiceRequirements(string groupingCode)
	{
		var codeType = AEConstants.RefCusCodeList.CodeTypes.ServiceRequirement;
		var yesterday = ZDateTime.Today.AddDays(-1);
		var tomorrow = ZDateTime.Today.AddDays(1);
		CreateCusCodeType(codeType, "Serive Requirements", groupingCode);
		CreateCusCodeList(groupingCode, codeType, "ABC", yesterday, tomorrow);
		CreateCusCodeList(groupingCode, codeType, "LMN", yesterday, yesterday);
		CreateCusCodeList("IN", codeType, "XYZ", yesterday, tomorrow);

		factory.Save();
	}

	public void SetupCustomsStatuses(string groupingCode)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var yesterday = ZDateTime.Today.AddDays(-1);
		var tomorrow = ZDateTime.Today.AddDays(1);
		helper.CreateCusCodeType(RefCusCodeListTypes.CustomsManifestStatus, "Customs Status", groupingCode);
		helper.CreateCusCodeType(RefCusCodeListTypes.ErrorCode, "Error Codes", groupingCode);
		helper.CreateCusCodeList(groupingCode, RefCusCodeListTypes.CustomsManifestStatus, "ABC", yesterday, tomorrow);
		helper.CreateCusCodeList(groupingCode, RefCusCodeListTypes.CustomsManifestStatus, "LMN", yesterday, yesterday);
		helper.CreateCusCodeList("IN", RefCusCodeListTypes.CustomsManifestStatus, "XYZ", yesterday, tomorrow);
		helper.CreateCusCodeList(groupingCode, RefCusCodeListTypes.ErrorCode, "ERR", yesterday, tomorrow);
		helper.CreateCusCodeList(groupingCode, RefCusCodeListTypes.ErrorCode, "ACK", yesterday, tomorrow);

		factory.Save();
	}

	public void SetupUnitCodeMappings()
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var groupingCode = Core.Constants.CountryCodes.UnitedArabEmirates;
		var yesterday = ZDateTime.Today.AddDays(-1);
		var tomorrow = ZDateTime.Today.AddDays(1);
		helper.CreateCusMapType(RefCusMapTypeList.Codes.MUQCO, MapDirectionList.Codes.OUT, "AE Manifest Mass UQ Conversion Mapping", true);
		helper.CreateCusMapType(RefCusMapTypeList.Codes.VUQCO, MapDirectionList.Codes.OUT, "AE Manifest Volume UQ Conversion Mapping", true);
		helper.CreateCusMap(RefCusMapTypeList.Codes.MUQCO, "KG", "KGM", yesterday, tomorrow, groupingCode);
		helper.CreateCusMap(RefCusMapTypeList.Codes.MUQCO, "L", "LTR", yesterday, tomorrow, groupingCode);

		factory.Save();
	}
}
