using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DepotCusOutturnUnderbondStatusCalculator))]
	sealed class DepotCusOutturnUnderbondStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCalculator()
		{
			var outturn = Factory.New<DepotCusOutturn>();
			var calculator = new DepotCusOutturnUnderbondStatusCalculator(outturn);
			AssertEquals(outturn.C5_MessageStatusInfo, calculator.StatusInfo);
		}

		protected override BusinessObject GetNewBusinessObject() => new DepotCusOutturnUnderbondStatusCalculator(Factory.New<DepotCusOutturn>());
	}
}
