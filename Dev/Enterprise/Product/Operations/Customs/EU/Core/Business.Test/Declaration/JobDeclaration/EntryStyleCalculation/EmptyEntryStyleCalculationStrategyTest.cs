using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class EmptyEntryStyleCalculationStrategyTest : TestCase
	{
		public void TestCalculate()
		{
			var strategy = (IEntryStyleCalculationStrategy)new EmptyEntryStyleCalculationStrategy();
			AssertEquals("EntryStyle", ZString.Empty, strategy.Calculate());
		}
	}
}
