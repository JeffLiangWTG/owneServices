using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLHeaderShipmentStatusCalculator))]
	sealed class CusSeaManOBLHeaderShipmentStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatusInfo()
		{
			AssertEquals("StatusInfo.Name", "BO_ShipmentStatus", Calculator.StatusInfo.Name);
		}

		public void TestInterestedMessageTypes()
		{
			AssertEquals("InterestedMessageTypes.Length", 1, Calculator.InterestedMessageTypes.Length);
			AssertEquals("InterestedMessageTypes[0]", CMRMessage.CMRMessageTypes.CARST, Calculator.InterestedMessageTypes[0]);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<CusSeaManOBLHeader>().ShipmentCalculator;

		CusSeaManOBLHeaderShipmentStatusCalculator calculator;
		CusSeaManOBLHeaderShipmentStatusCalculator Calculator => calculator ?? (calculator = (CusSeaManOBLHeaderShipmentStatusCalculator)GetNewBusinessObject());
	}
}
