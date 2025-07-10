using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed public class EnRouteIncidentConfigurationTest : TestCaseWithFactory
	{
		public void TestGetGoodsLocationValidationDecider()
		{
			AssertType<ArrivalPhase5CusGoodsLocationValidationDecider>(configuration.GetGoodsLocationValidationDecider());
		}

		public void TestGetEnRouteIncidentValidationDecider()
		{
			AssertType<EnRouteIncidentValidationDecider>(configuration.GetEnRouteIncidentValidationDecider());
		}

		protected override void SetUp()
		{
			base.SetUp();
			configuration = new EnRouteIncidentConfiguration();
		}
		EnRouteIncidentConfiguration configuration;
	}
}
