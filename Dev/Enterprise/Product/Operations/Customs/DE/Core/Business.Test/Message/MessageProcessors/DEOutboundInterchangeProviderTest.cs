using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	class DEOutboundInterchangeProviderTest : InterchangeProviderTestCase
	{
		public override void TestMessagesPopulateNewInterchange()
		{
			var messages = new NonDependentEDIMessageCollection(Factory);
			var message1 = CreateAndPopulateMessage();
			var message2 = CreateAndPopulateMessage();
			messages.AddRange(new AtlasEDIMessage[] { message1, message2 });

			var interchangeProvider = new DEOutboundInterchangeProvider(messages);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			var interchanges = interchangeProvider.Interchanges;
			Factory.Save();
			message1.Reload();
			message2.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("Number Of Interchanges", 2, interchanges.Length);
				AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);
				var interchange1 = interchanges.FirstOrDefault(x => x.PK == message1.EM_EI);
				AssertNotNull("Interchange 1 is linked to message 1", interchange1);
				AssertNotNull("Interchange 2 is linked to message 2", interchanges.FirstOrDefault(x => x.PK == message2.EM_EI));
				AssertEquals("Message 1 is sent", EDIMessage.Status.Sent, message1.EM_Status);
				AssertEquals("Message 2 is sent", EDIMessage.Status.Sent, message2.EM_Status);
				AssertEquals("Footer Text is empty", ZString.Empty, interchange1.EI_FooterText);
			});
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new DEOutboundInterchangeProvider(collection);

		AtlasEDIMessage CreateAndPopulateMessage()
		{
			var message = Factory.New<AtlasEDIMessage>();
			message.EM_MessageSubType = Messaging.TemporaryStorageMessageSubTypeList.Codes.SummaryDeclarationAfterPresentation;
			message.EM_MessageType = Messaging.EDIMessageTypeList.Codes.TemporaryStorage;
			message.EM_MessageOwner = "BASECOLLATIONKEY";
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = "TEST MESSAGE TEXT";
			return message;
		}
	}
}
