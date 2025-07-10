using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NL.Business.Testing;

class GoodsLocationWrapperTest : DataProviderTestCase<GoodsLocationWrapper>
{
	public void TestConstructor()
	{
		var nullWrapper = GoodsLocationWrapper.New(null);
		AssertNull(nullWrapper);
	}

	public void TestTypeCode()
	{
		goodsLocation.CGL_Type = "A";
		AssertEquals("A", wrapper.TypeCode);
	}

	public void TestAddress()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.Address);
			AssertType<AddressLocationWrapper>(wrapper.Address);
		});
	}

	public void TestIdentificationTypeCode()
	{
		AssertEquals("T", wrapper.IdentificationTypeCode);
	}

	public void TestNullCases()
	{
		var nullWrapper = GoodsLocationWrapper.New(null);
		AssertNull(nullWrapper);
	}

	protected override GoodsLocationWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();

		goodsLocation = Factory.New<CusGoodsLocation>();
		wrapper = GoodsLocationWrapper.New(goodsLocation);
	}
	CusGoodsLocation goodsLocation;
	GoodsLocationWrapper wrapper;
}
