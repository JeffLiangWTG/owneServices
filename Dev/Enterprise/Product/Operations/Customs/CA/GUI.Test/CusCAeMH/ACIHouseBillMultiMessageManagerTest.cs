using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;

namespace Enterprise.Customs.CA.GUI.Testing
{
	public class ACIHouseBillMultiMessageManagerTest : ACIHouseBillMultiMessageManager
	{
		public ACIHouseBillMultiMessageManagerTest(CusCAeMHMaster master)
			: base(() => master)
		{
		}

		public override IList<Enterprise.Messaging.Business.EDIMessage> SendOriginalMessages(ISendsMessagesToCustoms sender)
		{
			return SendOriginal(sender, DeclarableManagers);
		}
	}
}
