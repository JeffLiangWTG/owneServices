using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsSADHeaderAgreedLocationOfGoodsWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should be exception when nctsHeader is null", () => new NctsSADHeaderAgreedLocationOfGoodsWrapper(null));
		AssertExceptionThrown<ArgumentNullException>("Should be exception when nctsHeader.MovementHeader is null", () => new NctsSADHeaderAgreedLocationOfGoodsWrapper(Factory.New<NctsHeader>()));
	}

	public void TestAgreedLocationOfGoodsCode()
	{
		AssertEquals(nameof(wrapper.AgreedLocationOfGoodsCode), ZString.Empty, wrapper.AgreedLocationOfGoodsCode);
	}

	public void TestAgreedLocationOfGoodsDescription()
	{
		nctsHeader.Authorization = ZString.Empty;
		nctsHeader.MovementHeader.BM_LocationOfGoodsCode = "XYZ";
		AssertEquals($"When Authorization is empty, {nameof(wrapper.AgreedLocationOfGoodsDescription)}", "XYZ", wrapper.AgreedLocationOfGoodsDescription);

		nctsHeader.Authorization = "012345";
		AssertEquals($"When Authorization is not empty, {nameof(wrapper.AgreedLocationOfGoodsDescription)}", ZString.Empty, wrapper.AgreedLocationOfGoodsDescription);
	}

	public void TestAuthorizedLocationOfGoodsCodeAndCin()
	{
		nctsHeader.MovementHeader.UseElectronicFolder = false;
		nctsHeader.Authorization = ZString.Empty;
		nctsHeader.MovementHeader.BM_LocationOfGoodsCode = "XYZ";
		AssertEquals($"When Authorization is empty and not using electronic folder, {nameof(wrapper.AuthorizedLocationOfGoodsCodeAndCin)}", ZString.Empty, wrapper.AuthorizedLocationOfGoodsCodeAndCin);

		nctsHeader.Authorization = "012345";
		AssertEquals($"When Authorization is not empty and not using electronic folder, {nameof(wrapper.AuthorizedLocationOfGoodsCodeAndCin)}", "XYZ", wrapper.AuthorizedLocationOfGoodsCodeAndCin);

		nctsHeader.MovementHeader.UseElectronicFolder = true;
		nctsHeader.Authorization = ZString.Empty;
		nctsHeader.MovementHeader.BM_LocationOfGoodsCode = "XYZ";
		AssertEquals($"When Authorization is empty and using electronic folder, {nameof(wrapper.AuthorizedLocationOfGoodsCodeAndCin)}", "FE", wrapper.AuthorizedLocationOfGoodsCodeAndCin);

		nctsHeader.Authorization = "012345";
		AssertEquals($"When Authorization is not empty not using electronic folder, {nameof(wrapper.AuthorizedLocationOfGoodsCodeAndCin)}", "XYZ-FE", wrapper.AuthorizedLocationOfGoodsCodeAndCin);
	}

	public void TestCustomsSubPlace()
	{
		AssertEquals(nameof(wrapper.CustomsSubPlace), ZString.Empty, wrapper.CustomsSubPlace);

		nctsHeader.MovementHeader.BM_CustomsSubPlace = "D";
		AssertEquals(nameof(wrapper.CustomsSubPlace), "D", wrapper.CustomsSubPlace);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		wrapper = new NctsSADHeaderAgreedLocationOfGoodsWrapper(nctsHeader);
	}

	NctsHeader nctsHeader;
	NctsSADHeaderAgreedLocationOfGoodsWrapper wrapper;
}
