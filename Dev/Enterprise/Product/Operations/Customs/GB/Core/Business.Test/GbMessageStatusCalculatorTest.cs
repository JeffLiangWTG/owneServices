using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	class GbMessageStatusCalculatorTest : TestCase
	{
		public void TestGetMessageAwaitingStatus()
		{
			AssertEquals(Enterprise.Customs.Common.EU.MessageStatusList.Codes.AwaitingResponse, GbMessageStatusCalculator.GetMessageAwaitingStatus(new CusdecMessageFunction.New()));
			AssertEquals(Enterprise.Customs.Common.EU.MessageStatusList.Codes.AwaitingResponse, GbMessageStatusCalculator.GetMessageAwaitingStatus(new CusdecMessageFunction.Amended()));
			AssertEquals(Enterprise.Customs.Common.EU.MessageStatusList.Codes.AwaitingResponse, GbMessageStatusCalculator.GetMessageAwaitingStatus(new CusdecMessageFunction.Deleted()));
			AssertEquals(ZString.Empty, GbMessageStatusCalculator.GetMessageAwaitingStatus(new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration)));
		}
	}
}
