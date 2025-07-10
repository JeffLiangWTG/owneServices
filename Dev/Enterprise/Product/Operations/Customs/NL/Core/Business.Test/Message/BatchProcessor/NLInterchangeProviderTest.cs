using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class NLInterchangeProviderTest : InterchangeProviderTestCase
{
	public override void TestMessagesPopulateNewInterchange()
	{
		var senderIDCollection = new SenderInfoCollection
		{
			new SenderInfo()
			{
				OrganizationPK = GlbCompany.CurrentCompany.OrgProxy.PK,
				SenderID = "NL001234567.01.05",
				DefaultSenderID = true,
			},
		};

		var recipientIDCollection = new MessageVersionRegistryCollection
		{
			new MessageVersionRegistry()
			{
				DomainCode = MessageVersionRegistry.DMSDomainCode,
				TargetSystemName = "TEST_DMS",
			},
			new MessageVersionRegistry()
			{
				DomainCode = MessageVersionRegistry.NCTSP5DomainCode,
				TargetSystemName = "TEST_NCTS",
			}
		};

		using (NLCustomsRegistry.Instance.SenderIDs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, senderIDCollection))
		using (NLCustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, recipientIDCollection))
		{
			var messages = new NonDependentEDIMessageCollection(Factory);
			var message1 = CreateAndPopulateMessage();
			var message2 = CreateAndPopulateMessage(NLEDIMessageTypes.Codes.NCT);
			messages.AddRange(new NLEDIMessage[] { message1, message2 });

			var interchangeProvider = new NLInterchangeProvider(messages);
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
				var interchange2 = interchanges.FirstOrDefault(x => x.PK == message2.EM_EI);
				AssertNotNull("Interchange 1 is linked to message 1", interchange1);
				AssertNotNull("Interchange 2 is linked to message 2", interchange2);
				AssertEquals("Message 1 is sent", EDIMessage.Status.Sent, message1.EM_Status);
				AssertEquals("Message 2 is sent", EDIMessage.Status.Sent, message2.EM_Status);
				AssertEquals("Footer Text is empty", ZString.Empty, interchange1.EI_FooterText);

				AssertEquals("Application code should be NLC", EDIInterchange.ApplicationCodes.NLCustoms, interchange1.EI_ApplicationCode);
				AssertEquals("Interchange Type should be DMS for 1st message", "DMS", interchange1.EI_InterchangeType);
				AssertEquals("Interchange Type should be NCT for 2nd message", "NCT", interchange2.EI_InterchangeType);
				AssertEquals("To should be NLCustomsDMS", "NLCustomsDMS", interchange1.EI_To);
				AssertEquals("ReceiveTransmit should be TRX", EDIInterchange.Direction.Transmit, interchange1.EI_ReceiveTransmit);
				AssertEquals("Priority should be HGH", "HGH", interchange1.EI_Priority);
				AssertEquals("From should be message.company", message1.Company.LicenceKeyIdentifier, interchange1.EI_From);
				AssertEquals("Transport type should be XTT", EDIInterchange.TransportType.xT, interchange1.EI_TransportType);
				AssertEquals("Header text", $"{{\"custom.Subject\":\"[v=NL001234567.01.05.05][a=TEST_DMS][k={interchange1.eHubID.KeepAlphanumericCharacters()}][s=0]\"}}", interchange1.EI_HeaderText);
				AssertEquals("Header text", $"{{\"custom.Subject\":\"[v=NL001234567.01.05.04][a=TEST_NCTS][k={interchange2.eHubID.KeepAlphanumericCharacters()}][s=0]\"}}", interchange2.EI_HeaderText);
				AssertEquals("Kenmerk should be 32 characters long", 32, interchange1.eHubID.KeepAlphanumericCharacters().Length);
				AssertNotNull("SessionGuId cannot be null", interchange1.EI_SessionGUID);
			});
		}
	}

	protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
	{
		return new NLInterchangeProvider(collection);
	}

	NLEDIMessage CreateAndPopulateMessage(string messageType = NLEDIMessageTypes.Codes.DMS)
	{
		var message = Factory.New<NLEDIMessage>();
		message.EM_MessageType = messageType;
		message.EM_MessageSubType = ExportSendMessageTypes.Codes.DEC;
		message.EM_MessageOwner = "CW1_Test";
		message.EM_GB = GlbBranch.CurrentBranch.PK;
		message.EM_MessageText = "Test Message Text";
		return message;
	}
}
