using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class RiskChannelListTest : TestCase
	{
		public void TestGetRiskChannelValue()
		{
			AssertEquals("Result empty", string.Empty, RiskChannelList.GetRiskChannelValue(null));
			AssertEquals("Result empty", string.Empty, RiskChannelList.GetRiskChannelValue(""));
			AssertEquals("Result empty", string.Empty, RiskChannelList.GetRiskChannelValue("XX"));
			AssertEquals("VERDE -> 1", "1", RiskChannelList.GetRiskChannelValue("VERDE"));
			AssertEquals("AMARELO -> 2", "2", RiskChannelList.GetRiskChannelValue("AMARELO"));
			AssertEquals("VERMELHO -> 3", "3", RiskChannelList.GetRiskChannelValue("VERMELHO"));
			AssertEquals("CINZA -> 4", "4", RiskChannelList.GetRiskChannelValue("CINZA"));
			AssertEquals("LARANJA -> 5", "5", RiskChannelList.GetRiskChannelValue("LARANJA"));
		}
	}
}
