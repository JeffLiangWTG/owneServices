using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class AgentDirectWrapperTest : DataProviderTestCase<AgentDirectWrapper>
{
	public void TestName()
	{
		AssertEquals(string.Empty, wrapper.Name);
	}

	public void TestId()
	{
		AssertEquals("NL78787878", wrapper.Id);
	}

	public void TestAddress()
	{
		AssertNull(wrapper.Address);
	}

	public void TestContact()
	{
		AssertNull(wrapper.Contact);
	}

	public void TestFunctionCode()
	{
		AssertEquals("2", wrapper.FunctionCode);
	}

	protected override AgentDirectWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		var orgHeaderRepresentative = WrapperTestHelper.CreateOrgHeader(Factory, "Representative Full Name", "", "78787878");
		wrapper = new AgentDirectWrapper(orgHeaderRepresentative);
	}
	AgentDirectWrapper wrapper;
}
