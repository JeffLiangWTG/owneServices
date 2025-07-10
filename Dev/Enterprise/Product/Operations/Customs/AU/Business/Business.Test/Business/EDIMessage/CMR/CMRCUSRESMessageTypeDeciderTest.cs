using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRCUSRESMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			CMRCUSRESMessageTypeDecider typeDecider = new CMRCUSRESMessageTypeDecider();
			CMRCUSRESMessage message = Factory.New<CMRCUSRESMessage>();

			message.EM_MessageType = CMRMessage.CMRMessageTypes.PAYINV;
			AssertEquals("PAYINV Message", typeof(CMRPAYINVMessage), typeDecider.GetTypeForLoad(((INeedRow)message).Row, Factory));

			message.EM_MessageType = CMRMessage.CMRMessageTypes.PAYSTD;
			AssertEquals("PAYSTDR Message", typeof(CMRPAYSTDRMessage), typeDecider.GetTypeForLoad(((INeedRow)message).Row, Factory));

			message.EM_MessageType = CMRMessage.CMRMessageTypes.SEQ;
			AssertEquals("SEQ Message", typeof(CMRSEQRMessage), typeDecider.GetTypeForLoad(((INeedRow)message).Row, Factory));

			message.EM_MessageType = CMRMessage.CMRMessageTypes.SEI;
			AssertEquals("SEI Message", typeof(CMRSEIMessage), typeDecider.GetTypeForLoad(((INeedRow)message).Row, Factory));

			message.EM_MessageType = CMRMessage.CMRMessageTypes.CLNTDUP;
			AssertEquals("CLNTDUP Message", typeof(CMRCLNTDUPMessage), typeDecider.GetTypeForLoad(((INeedRow)message).Row, Factory));

			message.EM_MessageType = CMRMessage.CMRMessageTypes.CLREG;
			AssertEquals("CLREG Message", typeof(CMRCLREGRMessage), typeDecider.GetTypeForLoad(((INeedRow)message).Row, Factory));

			message.EM_MessageType = CMRMessage.CMRMessageTypes.EXREL;
			AssertEquals("EXREL Message", typeof(CMREXRELMessage), typeDecider.GetTypeForLoad(((INeedRow)message).Row, Factory));
		}
	}
}
