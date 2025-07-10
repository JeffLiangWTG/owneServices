using CargoWise.EntityFramework;
using Enterprise.Rating.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class FreightChargeStrategyForTest : IChargeStrategyForTest
	{
		public FreightChargeStrategyForTest(TestHelper testHelper, BusinessObjectFactory factory)
		{
			TestHelper = testHelper;
			Factory = factory;
		}
		string IChargeStrategyForTest.Charge0 => "FRT";
		string IChargeStrategyForTest.Charge1 => "BAF";
		string IChargeStrategyForTest.Charge2 => "CAF";
		string IChargeStrategyForTest.Charge3 => "WAR";
		string IChargeStrategyForTest.Charge4 => "FSC";
		string IChargeStrategyForTest.Charge5 => "FRT5";
		string IChargeStrategyForTest.Charge6 => "FRT6";

		void IChargeStrategyForTest.Setup()
		{
			TestHelper.ChargeCodes.CreateGlobalCharge("FRT5");
			TestHelper.ChargeCodes.CreateGlobalCharge("FRT6");

			Factory.Save();
		}

		TestHelper TestHelper { get; }

		BusinessObjectFactory Factory { get; }
	}
}
