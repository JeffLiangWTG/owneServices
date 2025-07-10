using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManTranHeadStatusCalculator))]
	sealed class CusSeaManTranHeadStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatusInfo()
		{
			AssertEquals("StatusInfo.Name", "Code", Calculator.StatusInfo.Name);
		}

		public void TestInterestedMessageTypes()
		{
			AssertEquals("InterestedMessageTypes.Length", 1, Calculator.InterestedMessageTypes.Length);
			AssertEquals("InterestedMessageTypes[0]", CMRMessage.CMRMessageTypes.SEAIAR, Calculator.InterestedMessageTypes[0]);
		}

		protected override BusinessObject GetNewBusinessObject() => new CusSeaManTranHeadStatusCalculator(Factory.New<CusSeaManTranHead>());

		CusSeaManTranHeadStatusCalculator calculator;
		CusSeaManTranHeadStatusCalculator Calculator => calculator ?? (calculator = (CusSeaManTranHeadStatusCalculator)GetNewBusinessObject());
	}
}
