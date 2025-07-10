using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLHeaderStatusCalculator))]
	sealed class CusSeaManOBLHeaderStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatusInfo()
		{
			AssertEquals("StatusInfo.Name", "BO_MessageStatus", Calculator.StatusInfo.Name);
		}

		public void TestInterestedMessageTypes()
		{
			AssertEquals("InterestedMessageTypes.Length", 1, Calculator.InterestedMessageTypes.Length);
			AssertEquals("InterestedMessageTypes[0]", CMRMessage.CMRMessageTypes.SEACR, Calculator.InterestedMessageTypes[0]);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<CusSeaManOBLHeader>().Calculator;

		CusSeaManOBLHeaderStatusCalculator calculator;
		CusSeaManOBLHeaderStatusCalculator Calculator => calculator ?? (calculator = (CusSeaManOBLHeaderStatusCalculator)GetNewBusinessObject());
	}
}
