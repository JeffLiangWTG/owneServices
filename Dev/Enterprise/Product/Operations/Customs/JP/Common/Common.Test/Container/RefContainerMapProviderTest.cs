using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(RefContainerMapProvider))]
sealed class RefContainerMapProviderTest : TestCaseWithFactory
{
	public void TestIsUsageNeeded()
	{
		AssertEquals(UsageRequirement.NotRequire, containerCodeMap.ContainerMapProvider.IsUsageNeeded);
	}

	public void TestUsageList()
	{
		Assert(containerCodeMap.Lookups.UsageList.Count == 0);
	}

	public void TestCodeList()
	{
		AssertEquals(1, containerCodeMap.Lookups.CodeList.Count);
		Assert(containerCodeMap.Lookups.CodeList.ContainsCode("12GP"));
		AssertEquals("ドライコンテナ General purpose container - 長さ10' - 高さ8'6\"", containerCodeMap.Lookups.CodeList.GetDescriptionFromCode("12GP"));
	}

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerHeight, "Container Height");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerLength, "Container Length");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerType, "Container Type");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerHeight, "2", "8'6\"", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerLength, "1", "10'", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerType, "GP", "ドライコンテナ General purpose container", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

		Factory.Save();

		containerCodeMap = Factory.NewWithValidTestData<RefContainerCodeMap>();
		containerCodeMap.RCM_RN_NKCountry = Core.Constants.CountryCodes.Japan;
		containerCodeMap.RCM_Code = "12GP";
	}

	RefContainerCodeMap containerCodeMap;
}
