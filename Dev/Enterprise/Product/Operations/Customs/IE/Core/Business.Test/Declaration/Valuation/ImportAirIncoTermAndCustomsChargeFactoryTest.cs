using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ImportAirIncoTermAndCustomsChargeFactoryTest : ImportIncoTermAndCustomsChargeFactoryTest
	{
		public override void TestFactoryType()
		{
			AssertEquals(typeof(ImportAirIncoTermAndCustomsChargeFactory), incoTermAndChargeFactory.GetType());
		}

		protected override string GetCountryContext() => Core.Constants.CountryCodes.Ireland + IEJobMessageTypeList.Codes.Import + Core.Constants.TransportModes.Air;

		protected override string ExpectedConfigurationTextFileName => "ImportAirIncoTermAndCustomsChargeFactory.csv";
	}
}
