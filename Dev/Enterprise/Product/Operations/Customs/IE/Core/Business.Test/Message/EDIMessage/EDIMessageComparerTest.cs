using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class EDIMessageComparerTest : TestCaseWithFactory
	{
		public void TestMessageTypeOrder()
		{
			var messageV = Factory.New<AESInboundEDIMessage>();
			var messageW = Factory.New<AESInboundEDIMessage>();
			var messageX = Factory.New<AESInboundEDIMessage>();
			var messageY = Factory.New<AESInboundEDIMessage>();
			var messageZ = Factory.New<AESInboundEDIMessage>();

			Factory.Save();

			messageV.EM_MessageType = "525";
			messageW.EM_MessageType = "RDU";
			messageX.EM_MessageType = "529";
			messageY.EM_MessageType = "528";
			messageZ.EM_MessageType = "504";

			var messages = new List<AESInboundEDIMessage>();
			messages.Add(messageV);
			messages.Add(messageW);
			messages.Add(messageX);
			messages.Add(messageY);
			messages.Add(messageZ);

			var sortedMessages = IEEDIMessageComparer.GetSortedMessages(messages, ListSortDirection.Ascending);
			AssertEquals("IE528 should be processed first", "528", sortedMessages[0].EM_MessageType);
			AssertEquals("IE525 should be processed last", "525", sortedMessages[sortedMessages.Length - 1].EM_MessageType);
		}
	}
}
