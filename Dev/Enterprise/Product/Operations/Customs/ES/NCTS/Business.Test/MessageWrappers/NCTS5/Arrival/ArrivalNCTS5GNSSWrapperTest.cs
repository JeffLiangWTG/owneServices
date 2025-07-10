using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalNCTS5GNSSWrapperTest : WrapperHelperTest<ArrivalNCTS5GNSSWrapper>
	{
		public void TestLatitude()
		{
			AssertEquals("Expected filled QualifierOfIdentification", "8.6600000", wrapper.Latitude);
		}

		public void TestLongitude()
		{
			AssertEquals("Expected filled Unlocode", "-9.4400000", wrapper.Longitude);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new ArrivalNCTS5GNSSWrapper(8.66, -9.44);
		}
		ArrivalNCTS5GNSSWrapper wrapper;

		protected override ArrivalNCTS5GNSSWrapper GetProvider() => wrapper;
	}
}
