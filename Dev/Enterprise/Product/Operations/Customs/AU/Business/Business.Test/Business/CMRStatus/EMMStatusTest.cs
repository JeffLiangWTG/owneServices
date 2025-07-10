using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EMMStatusTest : CMRStatusTest
	{
		public void TestGetRejectedStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Messages.Add(GetIncomingMessage(typeof(CMREMMMessage), "EMMRRejectionMessage.txt"));
			EMMStatus status = new EMMStatus(consol);
			AssertEquals("DocStatus", CMRDocumentStatus.Rejected, status.DocumentStatus);
		}

		public void TestGetClearStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Messages.Add(GetIncomingMessage(typeof(CMREMMMessage), "EMMRClearMessage.txt"));
			EMMStatus status = new EMMStatus(consol);
			AssertEquals("DocStatus", CMRDocumentStatus.Clear, status.DocumentStatus);
			AssertEquals("NumberOfLines", 1, status.Line.Length);
			AssertEquals("Line[0].DocumentStatusConditions", CMRDocumentStatus.Clear, status.Line[0].DocumentStatus);
		}
	}
}
