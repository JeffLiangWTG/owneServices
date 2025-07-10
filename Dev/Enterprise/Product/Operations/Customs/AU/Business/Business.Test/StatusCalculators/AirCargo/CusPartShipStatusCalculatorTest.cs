using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusPartShipStatusCalculator))]
	sealed class CusPartShipStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInterestedMessageTypes()
		{
			CusPartShipStatusCalculator calculator = GetNewBusinessObject() as CusPartShipStatusCalculator;
			AssertEquals("InterestedMessageTypes.Length", 2, calculator.InterestedMessageTypes.Length);
			AssertEquals("InterestedMessageTypes[0]", CMRMessage.CMRMessageTypes.AIRCR, calculator.InterestedMessageTypes[0]);
			AssertEquals("InterestedMessageTypes[1]", CMRMessage.CMRMessageTypes.CARST, calculator.InterestedMessageTypes[1]);
		}

		protected override BusinessObject GetNewBusinessObject() => new CusPartShipStatusCalculator(Factory.New<CusPartShip>());
	}
}
