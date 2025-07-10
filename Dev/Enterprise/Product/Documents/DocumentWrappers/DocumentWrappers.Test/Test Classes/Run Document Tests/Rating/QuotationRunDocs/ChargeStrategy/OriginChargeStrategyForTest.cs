using CargoWise.EntityFramework;
using Enterprise.Rating.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class OriginChargeStrategyForTest : IChargeStrategyForTest
	{
		public OriginChargeStrategyForTest(TestHelper testHelper, BusinessObjectFactory factory)
		{
			TestHelper = testHelper;
			Factory = factory;
		}

		string IChargeStrategyForTest.Charge0 => "ORG0";
		string IChargeStrategyForTest.Charge1 => "ORG1";
		string IChargeStrategyForTest.Charge2 => "ORG2";
		string IChargeStrategyForTest.Charge3 => "ORG3";
		string IChargeStrategyForTest.Charge4 => "ORG4";
		string IChargeStrategyForTest.Charge5 => "ORG5";
		string IChargeStrategyForTest.Charge6 => "ORG6";

		void IChargeStrategyForTest.Setup()
		{
			TestHelper.ChargeCodes.CreateGlobalCharge("ORG0");
			TestHelper.ChargeCodes.CreateGlobalCharge("ORG1");
			TestHelper.ChargeCodes.CreateGlobalCharge("ORG2");
			TestHelper.ChargeCodes.CreateGlobalCharge("ORG3");
			TestHelper.ChargeCodes.CreateGlobalCharge("ORG4");
			TestHelper.ChargeCodes.CreateGlobalCharge("ORG5");
			TestHelper.ChargeCodes.CreateGlobalCharge("ORG6");

			Factory.Save();
		}

		TestHelper TestHelper { get; }

		BusinessObjectFactory Factory { get; }
	}
}
