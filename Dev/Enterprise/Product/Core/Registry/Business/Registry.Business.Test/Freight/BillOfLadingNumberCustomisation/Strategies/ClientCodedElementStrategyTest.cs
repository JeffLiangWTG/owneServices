using Enterprise.Registry.Business.BillCustomisationStrategies;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class ClientCodedElementStrategyTest : TestCase
	{
		public void TestProperties()
		{
			IElementStrategy strategy1 = new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded1);
			IElementStrategy strategy2 = new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded2);
			IElementStrategy strategy3 = new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded3);

			AssertEquals("strategy1.Key", BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, strategy1.Key);
			AssertEquals("strategy1.Name", "Custom Element 1", strategy1.Name);

			AssertEquals("strategy2.Key", BillOfLadingNumberCustomisationElement.Keys.ClientCoded2, strategy2.Key);
			AssertEquals("strategy2.Name", "Custom Element 2", strategy2.Name);

			AssertEquals("strategy3.Key", BillOfLadingNumberCustomisationElement.Keys.ClientCoded3, strategy3.Key);
			AssertEquals("strategy3.Name", "Custom Element 3", strategy3.Name);
		}

		public void TestDetailMaxLength()
		{
			var strategy1 = new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded1);
			var strategy2 = new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded2);
			var strategy3 = new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded3);

			AssertEquals(1024, strategy1.DetailMaxLength);
			AssertEquals(1024, strategy2.DetailMaxLength);
			AssertEquals(1024, strategy3.DetailMaxLength);
		}

		public void TestCalcMaxGeneratedLength()
		{
			var strategy = new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded1);
			var customisation = new BillOfLadingNumberCustomisation();

			var element = new BillOfLadingNumberCustomisationElement(customisation, strategy);
			element.Detail = "<AgentCode>";
			AssertEquals(0, strategy.CalcMaxGeneratedLength(element));

			element.Detail = "SS";
			AssertEquals(2, strategy.CalcMaxGeneratedLength(element));

			element.Detail = "<AgentCode";
			AssertEquals(10, strategy.CalcMaxGeneratedLength(element));

			element.Detail = ">AgentCode<";
			AssertEquals(11, strategy.CalcMaxGeneratedLength(element));

			element.Detail = "AgentCode>";
			AssertEquals(10, strategy.CalcMaxGeneratedLength(element));
		}

		public void TestGetRegExForDataType()
		{
			var strategy = new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded1);
			var customisation = new BillOfLadingNumberCustomisation();

			var element = new BillOfLadingNumberCustomisationElement(customisation, strategy);
			element.Detail = "<AgentCode>";
			AssertEquals("(.*)", strategy.GetRegExForDataType(element));

			element.Detail = "AgentCode";
			AssertEquals("AgentCode", strategy.GetRegExForDataType(element));
		}
	}
}
