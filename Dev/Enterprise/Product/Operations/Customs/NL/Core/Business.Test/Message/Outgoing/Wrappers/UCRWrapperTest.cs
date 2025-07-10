using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class UCRWrapperTest : DataProviderTestCase<UCRWrapper>
{
	public void TestTraderAssignedReferenceId()
	{
		AssertEquals("Trader reference", wrapper.TraderAssignedReferenceId);
	}
	protected override void SetUp()
	{
		base.SetUp();
		wrapper = new UCRWrapper("Trader reference");
	}
	UCRWrapper wrapper;

	protected override UCRWrapper GetProvider() => wrapper;
}
