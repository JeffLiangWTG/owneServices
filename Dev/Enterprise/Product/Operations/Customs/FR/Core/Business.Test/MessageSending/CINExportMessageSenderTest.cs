using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class CINExportMessageSenderTest : TestCaseWithFactory
	{
		[TestDate(2020, 11, 3)]
		public void TestSendMessage()
		{
			var sendingObject = CreateMessageObject();
			var errorCollector = new ErrorCollector();
			var messageSender = new CINExportMessageSenderForTest(sendingObject, errorCollector);
			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.CIN755;
			messageSender.Send();

			AssertEquals(1, cusEntryHeader.Messages.Count);
			var message = cusEntryHeader.Messages[0];
			AssertType<FREDIMessage>(message);
			AssertContains("UNH+201103#ID_MESSAGE#+755:2", message.EM_MessageText);
		}

		public void TestMessageTypeReturnedByBuilder()
		{
			var sendingObject = CreateMessageObject();
			var errorCollector = new ErrorCollector();
			var messageSender = new CINExportMessageSenderForTest(sendingObject, errorCollector);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.CIN745;
			messageSender.GetEDIMessageText();
			AssertEquals("Message type returned by message builder should be the same as message sender's", MessageTypeList.Codes.CIN, messageSender.messageType);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.CIN755;
			messageSender.GetEDIMessageText();
			AssertEquals("Message type returned by message builder should be the same as message sender's", MessageTypeList.Codes.CIN, messageSender.messageType);
		}

		public void TestSequenceNumberDontIncrease()
		{
			var sendingObject = CreateMessageObject();
			var errorCollector = new ErrorCollector();
			var messageSender = new CINExportMessageSenderForTest(sendingObject, errorCollector);
			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.CIN755;
			Assert(!messageSender.ShouldIncreaseSequenceNumber);
			var oldsequencenumber = cusEntryHeader.CH_SequenceNumber;
			messageSender.Send();
			AssertEquals("sequence number have not increase", cusEntryHeader.CH_SequenceNumber, oldsequencenumber);
			AssertEquals(1, cusEntryHeader.Messages.Count);
		}

		[TestDate(2023, 02, 15)]
		public void TestPostSend()
		{
			var factory = new BusinessObjectFactory();
			var sendingObject = CreateMessageObject();
			var errorCollector = new ErrorCollector();
			var messageSender = new CINExportMessageSenderForTest(sendingObject, errorCollector);
			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.CIN755;
			messageSender.Send();
			cusEntryHeader = factory.Load<CusEntryHeader>(cusEntryHeader.PK);
			var headerLogs = cusEntryHeader.Logs.Find(a => a.SL_SE_NKEvent == Events.MessageSentCode).ToArray();
			AssertEquals(1, headerLogs.Length);
			AssertEquals("755", headerLogs[0].SL_Reference);
			AssertEquals(ZDateTimeOffset.Now, headerLogs[0].EventTimeOffset);
		}

		DeltaGJobDeclarationMessageSendingObject CreateMessageObject()
		{
			var cTO = Factory.New<OrgHeader>();
			cTO.FillWithValidTestData();
			var addressCTO = cTO.Addresses.AddNew();
			addressCTO.FillWithValidTestData();

			var customCodeCTO = cTO.CustomsCodes.AddNew();
			customCodeCTO.OK_CodeType = CusEntryHeader.Schema.CINOACICode;
			customCodeCTO.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customCodeCTO.OK_CustomsRegNo = "FR123456800";
			customCodeCTO.OK_OA_PremisesAddress = addressCTO.PK;

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.MovementReferenceNumberSetter("FR11111111111");
			declaration.JE_MasterBill = "12345";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = addressCTO.PK;

			var messageObject = new DeltaGJobDeclarationMessageSendingObject(declaration.CustomsEntryHeaders[0]);
			messageObject.SequenceNumber = 888;

			return messageObject;
		}
		JobDeclaration declaration;
		CusEntryHeader cusEntryHeader;
	}

	public class CINExportMessageSenderForTest : CINExportMessageSender
	{
		public CINExportMessageSenderForTest(DeltaGJobDeclarationMessageSendingObject decWrapper, ErrorCollector errorCollector) : base(decWrapper, errorCollector)
		{
		}

		public new ZString GetEDIMessageText() => base.GetEDIMessageText();
		public new bool ShouldIncreaseSequenceNumber => base.ShouldIncreaseSequenceNumber;
	}
}
