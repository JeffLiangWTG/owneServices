using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

public class CommonGoodsReferenceWrapperTest : WrapperHelperTest<CommonGoodsReferenceWrapper>
{
	public void TestSequenceNumber()
	{
		AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
	}
	public void TestGoodsItemNumber()
	{
		AssertEquals("Expected filled GoodsItemNumber", "2", wrapper.GoodsItemNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();

		wrapper = new CommonGoodsReferenceWrapper(1, 2);
	}

	CommonGoodsReferenceWrapper wrapper;

	protected override CommonGoodsReferenceWrapper GetProvider() => wrapper;
}
