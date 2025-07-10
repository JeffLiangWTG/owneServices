using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class DeltaIEMessageSenderTest : TestCaseWithFactory
	{
		public void TestOnlyTickedMessageObjectSent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "40";
			instruction.CEI_SubStyle = "F";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			Factory.Save();
			var messageObjectParent = new DeltaIEJobDeclarationMessageSendingObjectParent(declaration);
			var messageObject = messageObjectParent.SendingObjectsCollection.Cast<DeltaIEJobDeclarationMessageSendingObject>().FirstOrDefault();

			messageObject.ShouldSend = false;
			var sender = new DeltaIEMessageSenderForTest(messageObject, new ErrorCollector());
			sender.Send();
			entryHeader.Reload();
			var message = entryHeader?.Messages[0];
			AssertNull("No message should be created when the message object is not ticked.", message);

			messageObject.ShouldSend = true;
			sender = new DeltaIEMessageSenderForTest(messageObject, new ErrorCollector());
			sender.Send();
			entryHeader.Reload();
			message = entryHeader?.Messages[0];
			AssertNotNull("A message should be created when the message object is ticked.", message);
		}

		public void TestSendMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "40";
			instruction.CEI_SubStyle = "F";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var messageObjectParent = new DeltaIEJobDeclarationMessageSendingObjectParent(declaration);

			var messageObject = messageObjectParent.SendingObjectsCollection.Cast<DeltaIEJobDeclarationMessageSendingObject>().FirstOrDefault();
			messageObject.ShouldSend = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			var sender = new DeltaIEMessageSenderForTest(messageObject, new ErrorCollector());
			Assert(sender.ShouldIncreaseSequenceNumber);

			var oldsequencenumber = entryHeader.CH_SequenceNumber;
			sender.Send();
			var sentMessage = entryHeader.Messages.Last();
			AssertType<DeltaIEFREDIMessage>("The sended message should be of type DeltaIEFREDIMessage without reload.", sentMessage);
			AssertEquals("sequence number have increase", entryHeader.CH_SequenceNumber, oldsequencenumber + 1);
			var msg = entryHeader?.Messages[0];

			entryHeader.Reload();
			AssertNotNull(msg);

			var messageText = msg.EM_MessageText;

			AssertContains("", messageText);
			AssertEquals("DEC", msg.EM_MessageType);
			AssertEquals("415", msg.EM_MessageSubType);
		}

		public class DeltaIEMessageSenderForTest : DeltaIEMessageSender
		{
			public DeltaIEMessageSenderForTest(DeltaIEJobDeclarationMessageSendingObject decWrapper, ErrorCollector errorCollector) : base(decWrapper, errorCollector)
			{
			}
			public new bool ShouldIncreaseSequenceNumber => base.ShouldIncreaseSequenceNumber;
		}
	}
}
