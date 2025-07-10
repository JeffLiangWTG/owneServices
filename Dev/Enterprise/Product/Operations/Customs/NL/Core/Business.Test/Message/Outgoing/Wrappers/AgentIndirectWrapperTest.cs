using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class AgentIndirectWrapperTest : DataProviderTestCase<AgentIndirectWrapper>
{
	public void TestName()
	{
		AssertEquals(string.Empty, GetProvider().Name);
	}

	public void TestId()
	{
		AssertEquals(string.Empty, GetProvider().Id);
	}

	public void TestAddress()
	{
		AssertNull(GetProvider().Address);
	}

	public void TestContact()
	{
		AssertNull(GetProvider().Contact);
	}

	public void TestFunctionCode()
	{
		AssertEquals("3", GetProvider().FunctionCode);
	}

	protected override AgentIndirectWrapper GetProvider() => new AgentIndirectWrapper();
}

