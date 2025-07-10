using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM415V;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM917;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	class MailBoxItemProviderTest : TestCaseWithFactory
	{
		public void TestMailBoxItemProvider()
		{
			using (var reader = new StringReader(AISInterchangeProcessorTestHelper.GetStandardIE415VInterchangeText("6debb28a-c9b0-44fb-9e85-0737b42aef48", "ACPTEST IM099044", "21IEDUB11A782454R2", includeResponseWrap: false)))
			{
				var provider = new MailBoxItemProvider(reader);
				CombineAssertions("AISMailBoxItemProvider properties", () =>
				{
					AssertEquals("MailBoxId", "ce45c655-c780-43be-94f8-69ef936ea871", provider.MailBoxId);
					AssertEquals("TransactionId", "6debb28a-c9b0-44fb-9e85-0737b42aef48", provider.TransactionId);
					AssertNotNull("Message not null", provider.MessageText);
				});
			}
		}

		public void TestMailBoxItemProviderIM415V()
		{
			using (var reader = new StringReader(AISInterchangeProcessorTestHelper.GetStandardIE415VInterchangeText("6debb28a-c9b0-44fb-9e85-0737b42aef48", "ACPTEST IM099044", "21IEDUB11A782454R2", includeResponseWrap: false)))
			{
				var provider = new MailBoxItemProvider<Im415V>(reader);
				AssertEquals("D", provider.Message.ImportOperation.AdditionalDeclarationType);
			}
		}

		public void TestMailBoxItemProviderIM917()
		{
			using (var reader = new StringReader(AISInterchangeProcessorTestHelper.GetStandardIM917InterchangeText("020ddfa8-b792-452a-bb65-51b36a83937c", includeResponseWrap: false)))
			{
				var provider = new MailBoxItemProvider<Im917>(reader);
				AssertEquals(1, provider.Message.XmlNegativeAcknowledgement.Count);
			}
		}
	}
}
