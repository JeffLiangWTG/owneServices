using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManArrivalPortCargoListStatusCalculator))]
	sealed class CusSeaManArrivalPortCargoListStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatusInfo()
		{
			AssertEquals("StatusInfo.Name", "Code", Calculator.StatusInfo.Name);
		}

		public void TestInterestedMessageTypes()
		{
			AssertEquals("InterestedMessageTypes.Length", 1, Calculator.InterestedMessageTypes.Length);
			AssertEquals("InterestedMessageTypes[0]", CMRMessage.CMRMessageTypes.CARLST, Calculator.InterestedMessageTypes[0]);
		}

		protected override BusinessObject GetNewBusinessObject() => new CusSeaManArrivalPortCargoListStatusCalculator(Factory.New<CusSeaManArrivalPort>());

		CusSeaManArrivalPortCargoListStatusCalculator calculator;
		CusSeaManArrivalPortCargoListStatusCalculator Calculator => calculator ?? (calculator = (CusSeaManArrivalPortCargoListStatusCalculator)GetNewBusinessObject());
	}
}
