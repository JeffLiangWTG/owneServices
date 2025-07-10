using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusGoodsLocationWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CusGoodsLocationWrapper(goodsLocation: null));
		AssertNoExceptionThrown(() => new CusGoodsLocationWrapper(goodsLocation));
	}

	public void TestAddress()
	{
		AssertType<CusGoodsLocationAddress>(goodsLocationWrapper.Address);
		AssertSame("Address", goodsLocation.Address, goodsLocationWrapper.Address);
	}

	public void TestGetCustomOfficeCodes()
	{
		declaration.JE_CustomsOffice = ZString.Empty;
		var customsOfficeCodes = goodsLocationWrapper.GetCustomOfficeCodes();
		AssertNotNull(customsOfficeCodes);
		AssertEquals("When CustomsOffice is empty", 0, customsOfficeCodes.Count);

		declaration.JE_CustomsOffice = "IT1234";
		customsOfficeCodes = goodsLocationWrapper.GetCustomOfficeCodes();
		AssertNotNull(customsOfficeCodes);
		AssertEquals("When CustomsOffice is IT1234", 1, customsOfficeCodes.Count);
		AssertEquals("Customs Office Code", "IT1234", customsOfficeCodes.First());
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		goodsLocation = (CusGoodsLocation)declaration.GoodsLocation;
		goodsLocationWrapper = new CusGoodsLocationWrapper(goodsLocation);
	}

	JobDeclaration declaration;
	CusGoodsLocation goodsLocation;
	ICusGoodsLocationWrapper goodsLocationWrapper;
}
