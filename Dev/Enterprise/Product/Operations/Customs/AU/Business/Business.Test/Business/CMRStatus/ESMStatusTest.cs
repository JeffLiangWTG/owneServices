using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ESMStatusTest : CMRStatusTest
	{
		public void TestGetSimpleStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Messages.Add(GetIncomingMessage(typeof(CMRESMMessage), "ESMRErrorMessage.txt"));
			ESMStatus status = new ESMStatus(consol);
			AssertEquals("DocStatus", CMRDocumentStatus.Error, status.DocumentStatus);
			AssertEquals("DocumentStatusConditions", CMRDocumentStatusConditions.Validation, status.DocumentStatusConditions);

			AssertEquals("NumberOfLines", 1, status.Line.Length);
			AssertEquals("Line[0].DocumentStatusConditions", CMRDocumentStatus.Error, status.Line[0].DocumentStatus);
			AssertEquals("Line[0].DocumentStatusConditions", null, status.Line[0].DocumentStatusConditions);
			AssertEquals("Line[0].CAN", "AAAACNPKX", status.Line[0].CAN);
		}
	}
}
