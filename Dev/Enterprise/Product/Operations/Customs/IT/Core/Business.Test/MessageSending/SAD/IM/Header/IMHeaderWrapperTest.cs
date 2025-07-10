using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class IMHeaderWrapperTest : SADHeaderCommonWrapperTest<IMHeaderWrapper>
{
	public abstract void TestDeliveryCosts();

	public abstract void TestProvinceOfDestination();

	public abstract void TestMeansOfTransportOnArrival();

	public abstract void TestMeansOfTransportCrossingBorder();

	public abstract void TestEntryCustomsOffice();

	public override void TestCountryOfDispatch()
	{
		var refUNLOCO = Factory.New<RefUNLOCO>();
		refUNLOCO.RL_Code = "DISP";
		refUNLOCO.RL_RN_NKCountryCode = "JP";

		AssertEquals(ZString.Empty, sadHeaderWrapper.CountryOfDispatch);
		jobDeclaration.JE_RL_NKOrigin = "DISP";
		AssertEquals(jobDeclaration.JE_GoodsOrigin, sadHeaderWrapper.CountryOfDispatch);
	}

	public void TestPreClearing()
	{
		jobDeclaration.ZG_PreClearing = ZBool.True;
		Assert("Pre Clearing should be true", sadHeaderWrapper.PreClearing);
		jobDeclaration.ZG_PreClearing = ZBool.False;
		Assert("Pre Clearing should be false", !sadHeaderWrapper.PreClearing);
	}

	public void TestCompanyRegister()
	{
		var companyRegisterWrapper = sadHeaderWrapper.CompanyRegister;
		AssertNotNull(companyRegisterWrapper);
		AssertType<IMHeaderCompanyRegisterWrapper>(companyRegisterWrapper);
		CombineAssertions(() =>
		{
			AssertEquals("Number", "", companyRegisterWrapper.Number);
			AssertEquals("Series", "", companyRegisterWrapper.Series);
			AssertEquals("Date", ZDate.Empty, companyRegisterWrapper.Date);
		});
	}

	public void TestLocationOfGoods()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(sadHeaderWrapper.LocationOfGoods);
			AssertType<IMHeaderLocationOfGoodWrapper>(sadHeaderWrapper.LocationOfGoods);
		});
	}

	public void TestDeclarationCore()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(sadHeaderWrapper.Declaration);
			AssertType<IMDeclarationWrapper>(sadHeaderWrapper.Declaration);
		});
	}

	public override void TestWarehouseIdentification()
	{
		CombineAssertions(() =>
		{
			var warehouseIdentification = sadHeaderWrapper.WarehouseIdentification;
			AssertNotNull(warehouseIdentification);
			AssertType<IMHeaderWarehouseIdentificationWrapper>(warehouseIdentification);
		});
	}

	protected void SetUpRefData()
	{
		var currentCountry = GlbCompany.CurrentCompany.Country.Code;
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure40And71ForCurrentCountry();
		helper.CreateRefCusProcedure(currentCountry, "A", "76", "11", "111", "", "IMP", group: "IFD", intoWarehouse: true);
		helper.CreateRefCusProcedure(currentCountry, "A", "77", "11", "111", "", "IMP", group: "IFD", intoWarehouse: true);
	}

	protected override Type GetExpectedDeclarationType() => typeof(IMDeclarationWrapper);

	protected override void SetUp()
	{
		base.SetUp();

		jobDeclaration.JE_MessageType = "IMP";
	}
}
