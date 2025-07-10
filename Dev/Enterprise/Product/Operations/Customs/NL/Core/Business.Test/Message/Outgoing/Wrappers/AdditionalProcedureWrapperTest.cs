using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class AdditionalProcedureWrapperTest : DataProviderTestCase<AdditionalProcedureWrapper>
{
	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, wrapper.SequenceNumeric);
	}

	public void TestProcedureCode()
	{
		AssertEquals("ABC", wrapper.ProcedureCode);
		AssertEquals("", wrapperEmpty.ProcedureCode);
	}

	public void TestCCQualifierCode()
	{
		AssertNull(wrapper.CCQualifierCode);
		AssertNull(wrapperEmpty.CCQualifierCode);
	}

	protected override AdditionalProcedureWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();

		wrapper = new AdditionalProcedureWrapper("ADDPABCDEF", 1);
		wrapperEmpty = new AdditionalProcedureWrapper("ADD", 1);
	}
	AdditionalProcedureWrapper wrapper;
	AdditionalProcedureWrapper wrapperEmpty;
}
