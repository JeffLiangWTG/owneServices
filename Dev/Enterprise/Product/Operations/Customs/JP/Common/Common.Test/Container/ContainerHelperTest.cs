using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(ContainerHelper))]
public sealed class ContainerHelperTest : TestCaseWithFactory
{
	public void TestCombineList()
	{
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("ContainerLengthList", new[] { "1", "9" }, containerHelper.ContainerLengthList.Select(x => x.ZZD_Code));
			AssertContainsExactElementsInAnyOrder("ContainerHeightList", new[] { "2", "9" }, containerHelper.ContainerHeightList.Select(x => x.ZZD_Code));
			AssertContainsExactElementsInAnyOrder("ContainerTypeList", new[] { "GP", "SN" }, containerHelper.ContainerTypeList.Select(x => x.ZZD_Code));
		});
	}

	public void TestCombineCodes()
	{
		CombineAssertions(() =>
		{
			AssertEquals("OtherLengthCode", "9", containerHelper.OtherLengthCode);
			AssertEquals("OtherHeightCode", "9", containerHelper.OtherHeightCode);
			AssertEquals("OtherTypeCode", "SN", containerHelper.OtherTypeCode);
		});
	}

	public void TestMapContainerTypeToNaccsAcceptedValue()
	{
		AssertEquals("GP", containerHelper.MapContainerTypeToNaccsAcceptedValue("GP"));
		AssertEquals("SN", containerHelper.MapContainerTypeToNaccsAcceptedValue("XX"));
	}

	public void TestMapContainerSizeToNaccsAcceptedValue()
	{
		AssertEquals("12", containerHelper.MapContainerSizeToNaccsAcceptedValue("12"));
		AssertEquals("99", containerHelper.MapContainerSizeToNaccsAcceptedValue("XX"));
	}

	public void TestGetNACCSContainerType()
	{
		CombineAssertions(() =>
		{
			AssertEquals("JP Customs Container Code is adopted.", "GP", containerHelper.GetNACCSContainerType(Factory.Load<RefContainer>(containerSet.container1)));
			AssertEquals("ISO Code is adopted and mapped.", "SN", containerHelper.GetNACCSContainerType(Factory.Load<RefContainer>(containerSet.container2)));
			AssertEquals("RC_Code is adopted.", "GP", containerHelper.GetNACCSContainerType(Factory.Load<RefContainer>(containerSet.container3)));
			AssertEquals("RC_Code is adopted and mapped.", "SN", containerHelper.GetNACCSContainerType(Factory.Load<RefContainer>(containerSet.container4)));
		});
	}

	public void TestGetNACCSContainerSize()
	{
		CombineAssertions(() =>
		{
			AssertEquals("JP Customs Container Code is adopted.", "12", containerHelper.GetNACCSContainerSize(Factory.Load<RefContainer>(containerSet.container1)));
			AssertEquals("ISO Code is adopted", "12", containerHelper.GetNACCSContainerSize(Factory.Load<RefContainer>(containerSet.container2)));
			AssertEquals("RC_Code is adopted.", "12", containerHelper.GetNACCSContainerSize(Factory.Load<RefContainer>(containerSet.container3)));
			AssertEquals("RC_Code is adopted and mapped.", "99", containerHelper.GetNACCSContainerSize(Factory.Load<RefContainer>(containerSet.container4)));
		});
	}

	public static (ZGuid container1, ZGuid container2, ZGuid container3, ZGuid container4) CreateContainerTestData(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerHeight, "Container Height");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerLength, "Container Length");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerType, "Container Type");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerHeight, "2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerLength, "1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerType, "GP", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerHeight, "9", "≦4'（その他の高さについては本コードを使用）", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerLength, "9", "その他", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ContainerType, "SN", "その他のコンテナ Named cargo container", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

		factory.Save();

		var container1 = factory.New<RefContainer>();
		container1.RC_Code = "1111";
		var customsContainerCode = container1.CodeMapCollection.AddNew();
		customsContainerCode.RCM_RN_NKCountry = Core.Constants.CountryCodes.Japan;
		customsContainerCode.RCM_Code = "12GP";

		var container2 = factory.New<RefContainer>();
		container2.RC_Code = "2222";
		container2.RC_ISOType = "12R3";
		container2.RC_IsIso = true;

		var container3 = factory.New<RefContainer>();
		container3.RC_Code = "12GP";
		container3.RC_IsIso = false;

		var container4 = factory.New<RefContainer>();
		container4.RC_Code = "XXXX";
		container4.RC_IsIso = false;

		return (container1.PK, container2.PK, container3.PK, container4.PK);
	}

	protected override void SetUp()
	{
		base.SetUp();
		containerHelper = new ContainerHelper(Factory);
		containerSet = CreateContainerTestData(Factory);
	}

	ContainerHelper containerHelper;
	(ZGuid container1, ZGuid container2, ZGuid container3, ZGuid container4) containerSet;
}
