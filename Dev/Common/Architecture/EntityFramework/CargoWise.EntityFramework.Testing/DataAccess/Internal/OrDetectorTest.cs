using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class OrDetectorTest : TestCase
	{
		public void TestGetTextContainsOrOperator()
		{
			OrDetector orDetector = new OrDetector();

			AssertEquals("1", false, orDetector.ContainsOrOperator(""));
			AssertEquals("2", true, orDetector.ContainsOrOperator("or"));
			AssertEquals("3", true, orDetector.ContainsOrOperator("OR"));
			AssertEquals("4", false, orDetector.ContainsOrOperator("order = 7"));
			AssertEquals("5", true, orDetector.ContainsOrOperator("oR(order = 7)"));
			AssertEquals("6", true, orDetector.ContainsOrOperator(" and(order = 7 Or number = 1)"));
			AssertEquals("7", true, orDetector.ContainsOrOperator(" (order = 7 OR number = 1)  "));
		}
	}
}
