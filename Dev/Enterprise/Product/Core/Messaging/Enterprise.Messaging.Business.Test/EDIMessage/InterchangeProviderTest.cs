using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.Generic;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.InterchangeProviders.Testing
{
	public class InterchangeProviderTest : TestCaseWithFactory
	{
		public void TestGetCollationKey()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);

			string key = "hello_world";
			EDIMessage message = messages.AddNew();
			message.EM_MessageOwner = key;

			InterchangeProviderBaseTestClass provider = new InterchangeProviderBaseTestClass(messages);

			AssertEquals(key, provider.GetCollationKey_protected(message));
		}

		public void TestInterchangesWhenTenMessagesSortedByMessageOwnerAndMaxOfTwoPerInterchange()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message0 = messages.AddNew();
			message0.FillWithValidTestData();
			message0.EM_MessageText = "0";
			message0.EM_MessageOwner = String.Empty;

			EDIMessage message1 = messages.AddNew();
			message1.FillWithValidTestData();
			message1.EM_MessageText = "1";
			message1.EM_MessageOwner = "owner2";

			EDIMessage message2 = messages.AddNew();
			message2.FillWithValidTestData();
			message2.EM_MessageText = "2";
			message2.EM_MessageOwner = "owner0";

			EDIMessage message3 = messages.AddNew();
			message3.FillWithValidTestData();
			message3.EM_MessageOwner = "owner0";
			message3.EM_MessageText = "3";

			EDIMessage message4 = messages.AddNew();
			message4.FillWithValidTestData();
			message4.EM_MessageOwner = "owner2";
			message4.EM_MessageText = "4";

			EDIMessage message5 = messages.AddNew();
			message5.FillWithValidTestData();
			message5.EM_MessageText = "5";
			message5.EM_MessageOwner = "owner0";

			EDIMessage message6 = messages.AddNew();
			message6.FillWithValidTestData();
			message6.EM_MessageOwner = "owner1";
			message6.EM_MessageText = "6";

			EDIMessage message7 = messages.AddNew();
			message7.FillWithValidTestData();
			message7.EM_MessageOwner = "owner0";
			message7.EM_MessageText = "7";

			EDIMessage message8 = messages.AddNew();
			message8.FillWithValidTestData();
			message8.EM_MessageOwner = "owner0";
			message8.EM_MessageText = "8";

			EDIMessage message9 = messages.AddNew();
			message9.FillWithValidTestData();
			message9.EM_MessageOwner = String.Empty;
			message9.EM_MessageText = "9";

			InterchangeProviderBaseTestClass provider = new InterchangeProviderBaseTestClass(messages);

			EDIInterchange[] result = provider.Interchanges;
			AssertNotNull(result);
			AssertEquals(4, result.Length);

			EDIInterchange interchange0 = result[0];
			EDIInterchange interchange1 = result[1];
			EDIInterchange interchange2 = result[2];
			EDIInterchange interchange3 = result[3];

			AssertEquals("09", interchange0.EI_BodyText);
			AssertEquals("23578", interchange1.EI_BodyText);
			AssertEquals("6", interchange2.EI_BodyText);
			AssertEquals("14", interchange3.EI_BodyText);
		}

		public void TestInterchangesWhenOnlyOneMessage()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message = messages.AddNew();

			InterchangeProviderBaseTestClass provider = new InterchangeProviderBaseTestClass(messages);

			AssertEquals(1, provider.Interchanges.Length);
			EDIInterchange result = provider.Interchanges[0];
			AssertNotNull(result);
		}

		public void TestPassEmptyCollectionAndInterchangesIsNotNull()
		{
			InterchangeProviderBaseTestClass provider = new InterchangeProviderBaseTestClass(new NonDependentEDIMessageCollection(Factory));
			AssertNotNull(provider.Interchanges);
			AssertEquals(0, provider.Interchanges.Length);
		}

		public void TestGetUNG()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			InterchangeProviderBaseTestClass provider = new InterchangeProviderBaseTestClass(messages);
			UNGSegment uNGSegment = provider.GetTestUNG(new ZDateTime(2008, 2, 28, 1, 2, 3), "GSMCAR", "SENDER", "XX", "SRP", "UN", "D", "00A", "SUPRPT");
			AssertEquals("UNG", "UNG+GSMCAR+SENDER:XX+SRP+080228:0102+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>+UN+D:00A:SUPRPT'", uNGSegment.ToString(new UNOACharacterSet()));
		}

		public void TestGetUNEString()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			InterchangeProviderBaseTestClass provider = new InterchangeProviderBaseTestClass(messages);
			ZString uNEString = provider.GetTestUNEString("123");
			AssertEquals("UNG", "UNE+123+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>'", uNEString);
		}

		public void TestGetErrorsOnDiscardedMessage()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message0 = messages.AddNew();
			message0.FillWithValidTestData();
			message0.EM_MessageText = "0";
			message0.EM_MessageOwner = String.Empty;
			message0.EM_Status = EDIMessage.Status.Sent;

			InterchangeProviderBaseTestClass provider = new InterchangeProviderBaseTestClass(messages);
			provider.AddCollatedMessageCollection(messages);

			var errors = provider.GetErrorsOnDiscardedMessages();
			AssertEquals("No error should be found", ZString.Empty, errors);

			messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message1 = messages.AddNew();
			message1.FillWithValidTestData();
			message1.EM_MessageText = "1";
			message1.EM_MessageOwner = "owner2";
			message1.EM_Status = EDIMessage.Status.Discarded;

			StmNote note = Factory.New<StmNote>();
			var error = "This is an error";
			note.ST_Description = "Processing Log";
			note.ST_NoteDataAsText = error;
			note.ST_ParentID = message1.PK;
			note.ST_Table = EDIMessageSchema.Constants.TableName;

			provider.AddCollatedMessageCollection(messages);
			errors = provider.GetErrorsOnDiscardedMessages();
			AssertEquals("An error should be found", "This is an error\r\n", errors);

			provider.AddCollatedMessageCollection(messages);
			errors = provider.GetErrorsOnDiscardedMessages();
			AssertEquals("Two errors should be found", "This is an error\r\nThis is an error\r\n", errors);
		}

		public void TestEI_GP()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message = messages.AddNew();
			message.FillWithValidTestData();
			message.EM_MessageText = "0";
			message.EM_MessageOwner = String.Empty;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_GP = ZGuid.NewZGuid();

			InterchangeProviderBaseTestClass provider = new InterchangeProviderBaseTestClass(messages);

			EDIInterchange[] interchanges = provider.Interchanges;
			provider.SetInterchangeValuesForTransmit(interchanges[0], messages, message.EM_MessageType, "1", "2");
			AssertEquals("EI_GP should be", message.EM_GP, interchanges[0].EI_GP);
		}

		public class InterchangeProviderBaseTestClass : InterchangeProviderBase
		{
			public InterchangeProviderBaseTestClass(NonDependentEDIMessageCollection messages)
				: base(messages)
			{
			}

			protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
			{
				StringBuilder result = new StringBuilder();
				foreach (EDIMessage message in messages)
				{
					result.Append(message.EM_MessageText);
				}
				interchange.EI_BodyText = result.ToString();
			}

			public string GetCollationKey_protected(EDIMessage message)
			{
				return base.GetCollationKey(message);
			}

			public void AddCollatedMessageCollection(NonDependentEDIMessageCollection messages)
			{
				base.AddMessageCollection(messages);
			}

			protected internal override string InstructionHowToSetInterchangeSenderID
			{
				get { return ""; }
			}

			public UNGSegment GetTestUNG(ZDateTime timeOfPreparation, ZString messageType, ZString senderID, ZString senderQualifier, ZString recipientID,
					ZString controllingAgency, ZString messageversion, ZString messageRelease, ZString assignedCode)
			{
				return GetUNG(timeOfPreparation, messageType, senderID, senderQualifier, recipientID, controllingAgency, messageversion, messageRelease, assignedCode, "");
			}

			public ZString GetTestUNEString(ZString groupControlCount)
			{
				return GetUNEString(groupControlCount);
			}
		}

		class InterchangeProviderBaseTestClass2 : InterchangeProviderBase
		{
			public InterchangeProviderBaseTestClass2(NonDependentEDIMessageCollection messages)
				: base(messages)
			{
			}

			protected internal override string InstructionHowToSetInterchangeSenderID
			{
				get { return ""; }
			}

			protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
			{
				SetInterchangeValuesForTransmit(interchange, messages, messages[0].EM_MessageType, "1", "2");
			}

			protected override Type InterchangeType
			{
				get { return typeof(EDIInterchangeSendViaeHubBasedOnType); }
			}
		}

		class EDIInterchangeSendViaeHubBasedOnType : EDIInterchange
		{
			public EDIInterchangeSendViaeHubBasedOnType(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			//EI_InterchangeType is selected as it is currently set after EI_Status
			//This is to test suspend mechanism
			protected override bool ShouldSendViaEHubCore
			{
				get { return EI_InterchangeType == "AAA"; }
			}
		}

		public void TestSuspendeHubQueueStatusCalculationUntilAllFieldsAreSet()
		{
			var messages = new NonDependentEDIMessageCollection(Factory);
			var message = messages.AddNew();
			message.EM_MessageType = "AAA";

			var provider = new InterchangeProviderBaseTestClass2(messages);
			AssertEquals("PreCondition", 1, provider.Interchanges.Length);
			AssertEquals("Should be sent via eHub", EDIInterchange.Status.eHubQueued, provider.Interchanges[0].EI_Status);
			AssertEquals("TransportType should be eHub", EDIInterchange.TransportType.eHub, provider.Interchanges[0].EI_TransportType);
		}
	}
}
