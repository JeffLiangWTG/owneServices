using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing;

[TestedType(typeof(CusGoodsLocationAddress))]
public class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
{
	public void TestReadOnlyAuthorisationNumber()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		var goodsLocation = entryInstruction.GoodsLocation;
		var locationAddress = goodsLocation.Address;

		CombineAssertions(() =>
		{
			goodsLocation.CGL_Qualifier = "A";
			AssertEquals("AuthorisationNumber is not editable", true, locationAddress.AuthorisationNumberInfo.ReadOnly);

			goodsLocation.CGL_Qualifier = "Y";
			AssertEquals("AuthorisationNumber is editable", false, locationAddress.AuthorisationNumberInfo.ReadOnly);

			goodsLocation.CGL_Qualifier = "A";
			AssertEquals("AuthorisationNumber is not editable", true, locationAddress.AuthorisationNumberInfo.ReadOnly);
		});
	}

	public void TestAuthorisationNumber_MaxLegnth()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		var goodsLocation = entryInstruction.GoodsLocation;
		var locationAddress = goodsLocation.Address;

		AssertEquals(17, locationAddress.AuthorisationNumberInfo.MaxLength);
	}

	public void TestLookups()
	{
		AssertType<CusGoodsLocationAddressLookups>(Factory.New<CusGoodsLocationAddress>().Lookups);
	}

	public void TestValidation()
	{
		AssertType<CusGoodsLocationAddressValidation>(Factory.New<CusGoodsLocationAddress>().Validation);
	}
}
