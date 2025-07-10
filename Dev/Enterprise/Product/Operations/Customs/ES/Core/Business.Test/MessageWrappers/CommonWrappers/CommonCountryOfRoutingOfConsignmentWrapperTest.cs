using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CommonCountryOfRoutingOfConsignmentWrapperTest : WrapperHelperTest<CommonCountryOfRoutingOfConsignmentWrapper>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
		}

		public void TestCountryOfRouting()
		{
			AssertEquals("Expected filled CountryOfRouting", "ES", wrapper.CountryOfRouting);
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapper = new CommonCountryOfRoutingOfConsignmentWrapper(1, "ES");
		}

		CommonCountryOfRoutingOfConsignmentWrapper wrapper;

		protected override CommonCountryOfRoutingOfConsignmentWrapper GetProvider() => wrapper;
	}
}
