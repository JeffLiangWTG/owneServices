using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class ClassificationWrapperTest : DataProviderTestCase<ClassificationWrapper>
{
	public void TestSequenceNo()
	{
		AssertEquals(3, wrapper.SequenceNo);
	}

	public void TestId()
	{
		AssertEquals("39269097", wrapper.Id);
	}

	public void TestIdentificationTypeCode()
	{
		AssertEquals("TSP", wrapper.IdentificationTypeCode);
	}

	public void TestCCQualifierCode()
	{
		AssertNull(wrapper.CCQualifierCode);
	}
	protected override ClassificationWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		wrapper = new ClassificationWrapper(3, "39269097", NLConstants.Classification.IdentificationTypeCodes.TSP);
	}
	ClassificationWrapper wrapper;
}
