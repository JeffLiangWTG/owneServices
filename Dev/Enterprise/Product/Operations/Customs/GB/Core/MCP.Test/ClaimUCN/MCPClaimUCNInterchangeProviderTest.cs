using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.MCP.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.GB.MCP.ClaimUCN.Testing
{
	public class MCPClaimUCNInterchangeProviderTest : InterchangeProviderTestCase
	{
		public override void TestMessagesPopulateNewInterchange()
		{
			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, "ABCD1234320");
				var messages = new NonDependentEDIMessageCollection(Factory);
				var messageBuilder = new UCNMessageBuilder(declaration);
				var message1 = messageBuilder.Build().Message;
				var message2 = messageBuilder.Build().Message;

				messages.AddRange(new EDIMessage[] { message1, message2 });

				var provider = (MCPClaimUCNInterchangeProvider)GetInterchangeProvider(messages);
				provider.PackCollatedMessagesIntoInterchanges();
				Factory.Save();
				message1.Reload();
				message2.Reload();

				var interchanges = provider.Interchanges;

				CombineAssertions(() =>
				{
					AssertEquals("Number Of Interchanges", 2, interchanges.Length);
					AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);
					var interchange1 = interchanges.FirstOrDefault(x => x.PK == message1.EM_EI);
					AssertNotNull("Interchange 1 is linked to message 1", interchange1);
					AssertNotNull("Interchange 2 is linked to message 2", interchanges.FirstOrDefault(x => x.PK == message2.EM_EI));
					AssertEquals("Message 1 is sent", EDIMessageStatusList.Codes.Sent, message1.EM_Status);
					AssertEquals("Message 2 is sent", EDIMessageStatusList.Codes.Sent, message2.EM_Status);

					AssertEquals("Header", ZString.Empty, interchange1.EI_HeaderText);
					AssertEquals("Body", ":CSN~ABCD1234320~~~~~~}", interchange1.EI_BodyText);
					AssertEquals("Footer", ZString.Empty, interchange1.EI_FooterText);
					AssertEquals("Status", EDIInterchangeStatusList.Codes.Queued, interchange1.EI_Status);
					AssertEquals("EI_To", Constants.EDIInterchange.GBCustoms, interchange1.EI_To);
					AssertEquals("EI_From", "CAW", interchange1.EI_From);
				});
			}
		}

		public void TestWithNoValidCredentials()
		{
			using (MCPClaimUCNTestHelper.SetMCPCredentialsInRegistry())
			{
				var declaration = MCPClaimUCNTestHelper.CreateDeclarationWithSingleContainer(Factory, "ABCD1234560");
				var messages = new NonDependentEDIMessageCollection(Factory);
				var messageBuilder = new UCNMessageBuilder(declaration);
				var message = messageBuilder.Build().Message;
				message.EM_MessageOwner = ZString.Empty;
				messages.Add(message);
				var provider = (MCPClaimUCNInterchangeProvider)GetInterchangeProvider(messages);
				AssertExceptionThrown<MessageProcessingException>(() => provider.PackCollatedMessagesIntoInterchanges());
			}
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection messages)
		{
			return new MCPClaimUCNInterchangeProvider(messages);
		}
	}
}
