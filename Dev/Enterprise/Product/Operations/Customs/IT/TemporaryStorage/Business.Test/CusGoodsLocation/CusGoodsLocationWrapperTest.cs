using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class CusGoodsLocationWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CusGoodsLocationWrapper(null));
	}

	public void TestAddress()
	{
		AssertType<CusGoodsLocationAddress>(goodsLocationWrapper.Address);
		AssertSame(goodsLocation.Address, goodsLocationWrapper.Address);
	}

	public void TestGetCustomOfficeCodes()
	{
		header.PresentationCustomsOffice = ZString.Empty;
		var customsOfficeCodes = goodsLocationWrapper.GetCustomOfficeCodes();
		AssertEquals("When PresentationCustomsOffice is empty", "", customsOfficeCodes.JoinAsString());

		header.PresentationCustomsOffice = "IT279100";
		customsOfficeCodes = goodsLocationWrapper.GetCustomOfficeCodes();
		AssertEquals("When PresentationCustomsOffice has value", "IT279100", customsOfficeCodes.JoinAsString());
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<TemporaryStorageHeader>();
		goodsLocation = header.GoodsLocation;
		goodsLocationWrapper = new CusGoodsLocationWrapper(goodsLocation);
	}

	TemporaryStorageHeader header;
	CusGoodsLocation goodsLocation;
	ICusGoodsLocationWrapper goodsLocationWrapper;
}
