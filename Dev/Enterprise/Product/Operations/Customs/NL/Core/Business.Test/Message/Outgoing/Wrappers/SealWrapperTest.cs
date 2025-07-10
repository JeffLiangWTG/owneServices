using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class SealWrapperTest : DataProviderTestCase<SealWrapper>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>(() => new SealWrapper(null as CusSeal, 1));
		AssertExceptionThrown<ArgumentNullException>(() => new SealWrapper(null as string, 1));
	});

	public void TestId()
	{
		seal.BK_SealNumber = "Test123";
		AssertEquals("ID", "Test123", new SealWrapper(seal, 1).Id);
	}

	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, wrapper.SequenceNumeric);
	}

	protected override void SetUp()
	{
		base.SetUp();
		seal = Factory.New<CusSeal>();
		wrapper = new SealWrapper(seal, 1);
	}
	CusSeal seal;
	SealWrapper wrapper;

	protected override SealWrapper GetProvider() => wrapper;
}
