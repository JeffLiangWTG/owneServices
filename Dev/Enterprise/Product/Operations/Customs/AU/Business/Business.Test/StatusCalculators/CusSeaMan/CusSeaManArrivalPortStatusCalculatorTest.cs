using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManArrivalPortStatusCalculator))]
	sealed class CusSeaManArrivalPortStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatusInfo()
		{
			AssertEquals("StatusInfo.Name", "Code", Calculator.StatusInfo.Name);
		}

		public void TestInterestedMessageTypes()
		{
			AssertEquals("InterestedMessageTypes.Length", 1, Calculator.InterestedMessageTypes.Length);
			AssertEquals("InterestedMessageTypes[0]", CMRMessage.CMRMessageTypes.SEAAAR, Calculator.InterestedMessageTypes[0]);
		}

		protected override BusinessObject GetNewBusinessObject() => new CusSeaManArrivalPortStatusCalculator(Factory.New<CusSeaManArrivalPort>());

		CusSeaManArrivalPortStatusCalculator calculator;
		CusSeaManArrivalPortStatusCalculator Calculator => calculator ?? (calculator = (CusSeaManArrivalPortStatusCalculator)GetNewBusinessObject());
	}
}
