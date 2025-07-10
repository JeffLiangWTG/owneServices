using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CommonGNSSWrapperTest : WrapperHelperTest<CommonGNSSWrapper>
	{
		public void TestLatitude()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled Latitude", "1234.1234000", wrapper.Latitude);

				wrapper = new CommonGNSSWrapper(ZDecimal.Zero, -567.567m);
				AssertEquals("Expected empty Latitude when 0", ZString.Empty, wrapper.Latitude);
			});
		}

		public void TestLongitude()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled Longitude", "-567.5670000", wrapper.Longitude);

				wrapper = new CommonGNSSWrapper(1234.1234m, ZDecimal.Zero);
				AssertEquals("Expected empty Longitude when 0", ZString.Empty, wrapper.Longitude);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapper = new CommonGNSSWrapper(1234.1234m, -567.567m);
		}

		CommonGNSSWrapper wrapper;

		protected override CommonGNSSWrapper GetProvider() => wrapper;
	}
}
