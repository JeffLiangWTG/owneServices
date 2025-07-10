using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

public static class PreviousDocumentLookupsHelperTest
{
	public static void TestCustomsOfficeList(BusinessObjectFactory factory, IPreviousDocumentLookupsForTesting lookups)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Enterprise.MasterFiles.Business.EconomicGroupList.Codes.EuropeanUnion);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eun);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "BankCode");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "01234", "Test 0", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "12345", "Test 1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "23456", "Test 2", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "34567", "Test 3", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "45678", "Test 4", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(-1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "56789", "Test 5", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(-1));
		factory.Save();
		var query = lookups.CustomsOfficeList.CompleteFilter;
		var data = factory.Load<ZZRefCusCodeListCombined>(query);
		Assertion.Assert(data.Any(x => x.ZZD_Code == "01234"));
		Assertion.Assert(data.Any(x => x.ZZD_Code == "12345"));
		Assertion.Assert(!data.Any(x => x.ZZD_Code == "23456"));
		Assertion.Assert(!data.Any(x => x.ZZD_Code == "34567"));
		Assertion.Assert(!data.Any(x => x.ZZD_Code == "45678"));
		Assertion.Assert(!data.Any(x => x.ZZD_Code == "56789"));
	}
}
public interface IPreviousDocumentLookupsForTesting
{
	CodeDescriptionPairList ProcedureList { get; }
	CodeDescriptionPairList CodeList { get; }
	CodeDescriptionPairList SubTypeList { get; }
	CustomsOfficeCodeCollection CustomsOfficeList { get; }
	EU.Business.Declaration.MultiLineAddInfos.PreviousDocument PreviousDocument { get; }
}
