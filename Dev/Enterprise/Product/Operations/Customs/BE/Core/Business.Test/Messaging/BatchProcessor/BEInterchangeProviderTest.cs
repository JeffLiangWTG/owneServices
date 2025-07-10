using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class BEInterchangeProviderTest : InterchangeProviderTestCase
{
	public void TestSetTADMessageInterchangeReceiver()
	{
		AssertMessageSendToSameEnvAs015("TAD", Constants.InterchangeRecievers.LIVE, Constants.InterchangeRecievers.PRE, Constants.InterchangeRecievers.PRE);
		AssertMessageSendToSameEnvAs015("TAD", Constants.InterchangeRecievers.PRE, Constants.InterchangeRecievers.TEST, Constants.InterchangeRecievers.TEST);
		AssertMessageSendToSameEnvAs015("TAD", Constants.InterchangeRecievers.TEST, Constants.InterchangeRecievers.LIVE, Constants.InterchangeRecievers.LIVE);
	}

	public void TestSetFOLMessageInterchangeReceiver()
	{
		AssertMessageSendToSameEnvAs015("FOL", Constants.InterchangeRecievers.LIVE, Constants.InterchangeRecievers.PRE, Constants.InterchangeRecievers.PRE);
		AssertMessageSendToSameEnvAs015("FOL", Constants.InterchangeRecievers.PRE, Constants.InterchangeRecievers.TEST, Constants.InterchangeRecievers.TEST);
		AssertMessageSendToSameEnvAs015("FOL", Constants.InterchangeRecievers.TEST, Constants.InterchangeRecievers.LIVE, Constants.InterchangeRecievers.LIVE);
	}

	public void TestCustomsReference()
	{
		var messages = new NonDependentEDIMessageCollection(Factory);
		var messageTS414 = CreateAndPopulateMessage("TSD", "414");
		var messageTS207 = CreateAndPopulateMessage("TSD", "207");
		var messageTS215 = CreateAndPopulateMessage("TSD", "215");
		var messageTS614 = CreateAndPopulateMessage("REN", "614");
		var messageNCTTAD = CreateAndPopulateMessage("NCT", "TAD", linkToCusInBondMoveHeader: true);
		var messageNCTFOL = CreateAndPopulateMessage("NCT", "FOL", linkToCusInBondMoveHeader: true);

		messages.AddRange(new BEMessage[] { messageTS414, messageTS207, messageTS215, messageTS614, messageNCTTAD, messageNCTFOL });

		var interchangeProvider = new BEInterchangeProvider(messages);
		interchangeProvider.PackCollatedMessagesIntoInterchanges();
		var interchanges = interchangeProvider.Interchanges;
		Factory.Save();
		messageTS414.Reload();
		messageTS207.Reload();
		messageTS215.Reload();
		messageTS614.Reload();
		messageNCTTAD.Reload();
		messageNCTFOL.Reload();

		CombineAssertions(() =>
		{
			var interchangeTS414 = interchanges.Single(x => x.PK == messageTS414.EM_EI);
			var interchangeTS207 = interchanges.Single(x => x.PK == messageTS207.EM_EI);
			var interchangeTS215 = interchanges.Single(x => x.PK == messageTS215.EM_EI);
			var interchangeTS614 = interchanges.Single(x => x.PK == messageTS614.EM_EI);
			var interchangeNCTTAD = interchanges.Single(x => x.PK == messageNCTTAD.EM_EI);
			var interchangeNCTFOL = interchanges.Single(x => x.PK == messageNCTFOL.EM_EI);

			AssertContains("TS414", "\"custom.CustomsReference\":\"CRN\"", interchangeTS414.EI_HeaderText);
			AssertContains("TS207", "\"custom.CustomsReference\":\"MRN\"", interchangeTS207.EI_HeaderText);
			AssertContains("TS215", "\"custom.CustomsReference\":\"MRN\"", interchangeTS215.EI_HeaderText);
			AssertContains("TS614", "\"custom.CustomsReference\":\"MRN\"", interchangeTS614.EI_HeaderText);
			AssertContains("NCTTAD", "\"custom.CustomsReference\":\"MRN\",\"custom.Language\":\"en\"", interchangeNCTTAD.EI_HeaderText);
			AssertContains("NCTFOL", "\"custom.CustomsReference\":\"MRN\",\"custom.Language\":\"de\"", interchangeNCTFOL.EI_HeaderText);
		});
	}

	public override void TestMessagesPopulateNewInterchange()
	{
		var messages = new NonDependentEDIMessageCollection(Factory);
		var message1 = CreateAndPopulateMessage(isTestMessage: false);
		var message2 = CreateAndPopulateMessage(isTestMessage: true);
		var message3 = CreateAndPopulateMessage("AES", isTestMessage: true);
		messages.AddRange(new BEMessage[] { message1, message2, message3 });

		var interchangeProvider = new BEInterchangeProvider(messages);
		interchangeProvider.PackCollatedMessagesIntoInterchanges();
		var interchanges = interchangeProvider.Interchanges;
		Factory.Save();
		message1.Reload();
		message2.Reload();

		CombineAssertions(() =>
		{
			AssertEquals("Number Of Interchanges - 3", 3, interchanges.Length);

			Assert("Confirmed different interchanges for each message no collation", IsAllUnique(messages.Select(m => m.EM_EI)));

			var interchange1 = interchanges.Single(x => x.PK == message1.EM_EI);
			AssertInterchange(interchange1, message1, "NCT", "LIVE");

			var interchange2 = interchanges.Single(x => x.PK == message2.EM_EI);
			AssertInterchange(interchange2, message2, "NCT", "TEST");

			var interchange3 = interchanges.Single(x => x.PK == message3.EM_EI);
			AssertInterchange(interchange3, message3, "AES", "TEST");
		});
	}

	public void TestMessagesPopulateNewInterchang_DetermineTestSystem()
	{
		var messages = new NonDependentEDIMessageCollection(Factory);
		var message1 = CreateAndPopulateMessage(isTestMessage: false);
		var message2 = CreateAndPopulateMessage(isTestMessage: true);
		var message3 = CreateAndPopulateMessage("AES", isTestMessage: true);
		messages.AddRange(new BEMessage[] { message1, message2, message3 });

		InterchangeProviderBase interchangeProvider;
		EDIInterchange[] interchanges;
		using (BECustomsRegistry.Instance.DetermineTestSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			interchangeProvider = new BEInterchangeProvider(messages);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
		}
		interchanges = interchangeProvider.Interchanges;
		Factory.Save();
		message1.Reload();
		message2.Reload();

		CombineAssertions(() =>
		{
			AssertEquals("Number Of Interchanges - 3", 3, interchanges.Length);

			Assert("Confirmed different interchanges for each message no collation", IsAllUnique(messages.Select(m => m.EM_EI)));

			var interchange1 = interchanges.Single(x => x.PK == message1.EM_EI);
			AssertInterchange(interchange1, message1, "NCT", "LIVE");

			var interchange2 = interchanges.Single(x => x.PK == message2.EM_EI);
			AssertInterchange(interchange2, message2, "NCT", "PRE");

			var interchange3 = interchanges.Single(x => x.PK == message3.EM_EI);
			AssertInterchange(interchange3, message3, "AES", "PRE");
		});
	}

	public void TestMessagesPopulateNewInterchange_SendUserAndSecretInHeader()
	{
		using (BECustomsRegistry.Instance.SendUserAndSecretInHeader.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			var messages = new NonDependentEDIMessageCollection(Factory);
			var message1 = CreateAndPopulateMessage();
			messages.Add(message1);
			var password = GlbCompanyWrapper.GetWrapper<BEGlbCompanyWrapper>(message1.Company).Credential;
			password.CurrentDecryptedPassword = "TestPassWord";
			password.GP_UserID = "TestUserId";

			var interchangeProvider = new BEInterchangeProvider(messages);
			interchangeProvider.PackCollatedMessagesIntoInterchanges();
			var interchanges = interchangeProvider.Interchanges;
			Factory.Save();
			message1.Reload();
			var interchange1 = interchanges.First(x => x.PK == message1.EM_EI);

			AssertEquals("EI_HeaderText", "{\"oauth2.client-id\":\"TestUserId\",\"oauth2.client-secret\":\"TestPassWord\",\"custom.MessageSubType\":\"007\"}", interchange1.EI_HeaderText);
		}
	}

	protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
	{
		return new BEInterchangeProvider(collection);
	}

	BEMessage CreateAndPopulateMessage(string messageType = "NCT", string messageSubType = "007", bool isTestMessage = false, bool linkToCusInBondMoveHeader = false)
	{
		var message = Factory.New<BEMessage>();
		message.EM_MessageType = messageType;
		message.EM_MessageOwner = "CW1_Test";
		message.EM_MessageText = "Test Message Text";
		message.EM_MessageSubType = messageSubType;
		message.EM_IsTestMessage = isTestMessage;

		if (linkToCusInBondMoveHeader)
		{
			var nctsHeader = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.BE.ICusInBondHeader>();
			var cusEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, "MRN", Core.Constants.CountryCodes.Belgium);
			nctsHeader.BH_HeaderType = "D";
			nctsHeader.BH_CommunicationLanguage = "DE";
			cusEntryNumber.CE_EntryNum = "MRN";
			message.EM_LinkTable = nctsHeader.MovementHeader.TableName;
			message.EM_LinkUniqueID = nctsHeader.MovementHeader.PK;
		}
		else
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.CRN = "CRN";
			entryHeader.MovementReferenceNumberSetter("MRN");
			message.EM_LinkedObject = entryHeader;
		}
		return message;
	}

	void AssertInterchange(EDIInterchange interchange, BEMessage message, string expectedInterchangeType, string expectedTo)
	{
		AssertEquals("Message is sent", EDIMessage.Status.Sent, message.EM_Status);
		AssertNotNull("Interchange 1 is linked to message 1", interchange);
		AssertEquals("Footer Text is empty", ZString.Empty, interchange.EI_FooterText);
		AssertNotEquals("EI_GP should be set correctly", ZGuid.Empty, interchange.EI_GP);
		AssertEquals("Application code should be BEC", EDIInterchange.ApplicationCodes.BECustoms, interchange.EI_ApplicationCode);
		AssertEquals($"Interchange Type should be {expectedInterchangeType}", expectedInterchangeType, interchange.EI_InterchangeType);
		AssertEquals($"To should be {expectedTo}", expectedTo, interchange.EI_To);
		AssertEquals("ReceiveTransmit should be TRX", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
		AssertEquals("Priority should be empty", ZString.Empty, interchange.EI_Priority);
		AssertEquals("From should be message.company", message.Company.LicenceKeyIdentifier, interchange.EI_From);
		AssertEquals("Transport type should be XTT", EDIInterchange.TransportType.xT, interchange.EI_TransportType);
		AssertNotNull("SessionGuId cannot be null", interchange.EI_SessionGUID);
		AssertContains("EI_HeaderText", "\"custom.MessageSubType\":\"007\"", interchange.EI_HeaderText);
	}

	void AssertMessageSendToSameEnvAs015(string messageSubType, string old015MessageReceiver, string new015MessageReceiver, string expectedMessageReceiver)
	{
		var messages015 = new NonDependentEDIMessageCollection(Factory);

		var oldMessage015 = CreateAndPopulateMessage("NCT", "015", linkToCusInBondMoveHeader: true);
		var newMessage015 = CreateAndPopulateMessage("NCT", "015", linkToCusInBondMoveHeader: true);
		newMessage015.EM_LinkUniqueID = oldMessage015.EM_LinkUniqueID;

		oldMessage015.EM_MessageNum = "99";
		newMessage015.EM_MessageNum = "100";
		messages015.AddRange(new BEMessage[] { oldMessage015, newMessage015 });

		var interchangeProvider015 = new BEInterchangeProvider(messages015);
		interchangeProvider015.PackCollatedMessagesIntoInterchanges();
		oldMessage015.Interchange.EI_To = old015MessageReceiver;
		newMessage015.Interchange.EI_To = new015MessageReceiver;
		Factory.Save();

		var messages = new NonDependentEDIMessageCollection(Factory);
		var message = CreateAndPopulateMessage("NCT", messageSubType, linkToCusInBondMoveHeader: true);
		message.EM_LinkUniqueID = oldMessage015.EM_LinkUniqueID;
		messages.Add(message);
		var interchangeProvider = new BEInterchangeProvider(messages);
		interchangeProvider.PackCollatedMessagesIntoInterchanges();
		Factory.Save();
		AssertEquals(expectedMessageReceiver, message.EM_InterchangeReceiver);
	}

	bool IsAllUnique<T>(IEnumerable<T> values)
	{
		var hash_set = new HashSet<T>();

		return values.All(x => hash_set.Add(x));
	}
}
