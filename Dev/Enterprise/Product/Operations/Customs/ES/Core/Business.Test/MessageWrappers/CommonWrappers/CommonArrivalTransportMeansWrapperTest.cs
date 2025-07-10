using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

public class CommonArrivalTransportMeansWrapperTest : WrapperHelperTest<CommonArrivalTransportMeansWrapper>
{
	public void TestType()
	{
		AssertEquals("Expected filled Type", "10", wrapper.Type);
	}

	public void TestId()
	{
		AssertEquals("Expected filled Id", "identification", wrapper.Id);
	}

	protected override void SetUp()
	{
		base.SetUp();

		wrapper = new CommonArrivalTransportMeansWrapper("10", "identification");
	}

	CommonArrivalTransportMeansWrapper wrapper;

	protected override CommonArrivalTransportMeansWrapper GetProvider() => wrapper;
}
