using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

public class CommonAdditionalCodeWrapperTest : WrapperHelperTest<CommonAdditionalCodeWrapper>
{
	public void TestSequenceNumber()
	{
		AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
	}

	public void TestCode()
	{
		AssertEquals("Expected filled Code", "code", wrapper.Code);
	}

	protected override void SetUp()
	{
		base.SetUp();

		wrapper = new CommonAdditionalCodeWrapper(1, "code");
	}

	CommonAdditionalCodeWrapper wrapper;

	protected override CommonAdditionalCodeWrapper GetProvider() => wrapper;
}
