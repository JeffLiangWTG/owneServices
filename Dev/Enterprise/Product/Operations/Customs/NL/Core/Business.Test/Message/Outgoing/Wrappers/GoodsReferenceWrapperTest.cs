using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NL.Business.Testing;

class GoodsReferenceWrapperTest : DataProviderTestCase<GoodsReferenceWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new GoodsReferenceWrapper(null, 2));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals(2, Provider.SequenceNumeric);
	}

	public void TestGoodsItemNumericValue()
	{
		AssertEquals(1, Provider.GoodsItemNumericValue);
	}

	protected override GoodsReferenceWrapper GetProvider() => new GoodsReferenceWrapper(containerInvLinePivot, 2);

	protected override void SetUp()
	{
		base.SetUp();
		containerInvLinePivot = (CusContainerInvoiceLinePivot)WrapperTestHelper.GetEntryHeaderForTest(Factory).InvoiceLines.First().ContainersPivot.First();
	}

	CusContainerInvoiceLinePivot containerInvLinePivot;
}
