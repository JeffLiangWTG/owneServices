using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(ErrorMessageProcessor))]
sealed class ErrorMessageProcessorTest : TestCaseWithFactory
{
	public void TestProcessMessage()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var configType = helper.CreateOrGetExistingRefSysConfigType("NACCSMailT", "NACCS Mail Test", "NACCS Mail Test");
		helper.CreateOrUpdateExistingRefSysConfig(configType.ZRT_ConfigCode, "NACCS@Mail.Test.NACCS6", ZDateTime.Now.AddMonths(-1), ZDateTime.Now.AddMonths(1));

		Factory.Save();

		var company = Factory.NewWithValidTestData<GlbCompany>();
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

		var password = Factory.New<GlbExternalPasswordNMC>();
		password.GP_MailBoxID = "XXX";
		password.CurrentDecryptedPassword = "123";
		password.GP_GC = company.PK;
		password.ShouldReceive = true;
		password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

		var branch = Factory.NewWithValidTestData<GlbBranch>();
		branch.GB_GC = company.PK;

		var attributes = new Dictionary<string, string>
		{
			{ Constants.DirectxT.CompanyCodeAttribute, company.GC_Code },
			{ Constants.DirectxT.ClientMailboxAttribute, "XXX@MAIL.TEST.NACCS6" }
		};

		var settings = new MailboxAndRemoteWebPrintClientCredentials { LocalComputerAlias = "dummy", DomainName = "dummy", Status = XtCredentialStatusList.Codes.Unregistered };
		settings.FailureNotificationGroup = GetNotificationGroupCode();
		JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

		var entryHeaderAndMessage = CreateEntryHeaderAndMessage(branch.PK);
		var message = entryHeaderAndMessage.Message;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageData = Encoding.UTF8.GetBytes("Invalid XML");

		var logger = new LoggingInformation();
		var processor = new ErrorMessageProcessor(logger);
		processor.ProcessMessage(message);

		AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
		Assert(logger.DebugLogStrings.Cast<string>().Any(c => c.Contains($"Can't parse a XML Document for message: {message.EM_MessageNum}")));

		message.EM_Status = EDIMessage.Status.Queued;
		message.Interchange.SetHeaderTextWithAttributeDictionary(attributes);
		message.EM_MessageData = GetMessageData("Unauthorized");

		processor.ProcessMessage(message);

		AssertEquals(EDIMessage.Status.ProcessedOK, message.EM_Status);
		AssertEquals(PasswordStatusList.Codes.Invalid, password.GP_PasswordStatus);

		message.Interchange.EI_HeaderText = string.Empty;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageData = GetMessageData("Unauthorized");
		processor.ProcessMessage(message);
		var email = Env.OutgoingMailManager.EmailsCreated.LastOrDefault();

		AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
		Assert(logger.DebugLogStrings.Cast<string>().Any(c => c.Contains($"Unable to process unauthorized failure message: {message.EM_MessageNum}")));

		CombineAssertions(() =>
		{
			AssertEquals("Subject", $"Failure message ({message.EM_MessageNum}) received for host dummy", email.Subject);
			AssertContains("Body", "An error message was received when trying to communicate with NACCS service.", email.Body);
			AssertContains("Body", "The error detail is:", email.Body);
			AssertContains("Body", "Test Reason", email.Body);
			AssertContains("Body", $"Please investigate by going to the module EDI Message and query by the message number {message.EM_MessageNum}.", email.Body);
			AssertEquals("Recipient", "test@mail.com", email.Recipients.Cast<RecipientDef>().Single().Email);
		});

		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_LinkedObject = entryHeaderAndMessage.EntryHeader;
		message.EM_MessageData = GetMessageData("TransmissionError");
		processor.ProcessMessage(message);

		AssertEquals(EDIMessage.Status.Received, message.EM_Status);

		var naccsStatus = (INACCSStatus)message.EM_LinkedObject;
		AssertEquals(CustomsStatusList.Codes.Error, naccsStatus.CustomsStatus);
		AssertEquals(JPMessageStatusList.Codes.Error, naccsStatus.MessageStatus);

		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageData = GetMessageData("XXX");
		processor.ProcessMessage(message);

		AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
		Assert(logger.DebugLogStrings.Cast<string>().Any(c => c.Contains($"Unkown response message: {message.EM_MessageNum} Response Type: XXX")));
	}

	(CusEntryHeader EntryHeader, EDIMessage Message) CreateEntryHeaderAndMessage(ZGuid branchPk)
	{
		var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
		declaration.JE_DeclarationReference = "B00240914";
		declaration.JE_HouseBill = "HB0240914";
		declaration.JE_GB = branchPk;

		var entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_BGMReference = "1234567890";

		var outgoingMessage = Factory.New<EDIMessage>();
		outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
		outgoingMessage.EM_MessageType = JPProcedureCodeList.Codes.IDC;
		outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		outgoingMessage.EM_Status = EDIMessage.Status.Sent;
		outgoingMessage.EM_MessageNum = "JRIDA0012345678900000000001";
		outgoingMessage.EM_GB = branchPk;
		outgoingMessage.EM_LinkedObject = entry;

		var transmitInterchange = Factory.New<EDIInterchange>();
		transmitInterchange.EI_InterchangeNum = "IDA00000000001";
		transmitInterchange.EI_From = "WTLDJPEDI";
		transmitInterchange.EI_To = "WTLEDI_JP";
		transmitInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.JPCustoms;
		transmitInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		transmitInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
		transmitInterchange.EI_BodyData = Encoding.ASCII.GetBytes("SendMessage");
		transmitInterchange.ContainedMessages.Add(outgoingMessage);

		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		incomingMessage.EM_Status = EDIMessage.Status.Queued;
		incomingMessage.EM_MessageType = JPMessageTypes.Codes.XER;
		incomingMessage.EM_GB = GlbBranch.CurrentBranch.PK;
		incomingMessage.EM_MessageNum = "JRIDA00ABCDEFG12345678900000000002";

		var receiveInterchange = Factory.New<EDIInterchange>();
		receiveInterchange.EI_InterchangeNum = "JR000000002";
		receiveInterchange.EI_InterchangeType = JPMessageTypes.Codes.XER;
		receiveInterchange.EI_From = Constants.WebPrintParty;
		receiveInterchange.EI_To = "WTLDJPEDI";
		receiveInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.JPCustoms;
		receiveInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		receiveInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
		receiveInterchange.EI_Status = EDIInterchange.Status.Queued;
		receiveInterchange.ContainedMessages.Add(incomingMessage);

		receiveInterchange.EI_BodyData = incomingMessage.EM_MessageData = GetMessageData("TransmissionError");
		receiveInterchange.EI_SessionGUID = transmitInterchange.EI_SessionGUID = ZGuid.NewZGuid();

		return (entry, incomingMessage);
	}

	ZString GetNotificationGroupCode()
	{
		var user = Factory.NewWithValidTestData<GlbStaff>();
		user.GS_EmailAddress = "test@mail.com";

		var group = Factory.NewWithValidTestData<GlbGroup>();
		group.GG_Code = "TST";
		group.Staff.Add(user);

		Factory.Save();

		return group.GG_Code;
	}

	byte[] GetMessageData(string responseType)
	{
		return Encoding.UTF8.GetBytes($@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>WebPrint_NACCSCLIENT</SenderID>
		<RecipientID>WTLDJPCTU</RecipientID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
			<Event>
				<EventTime>2024-09-09T05:05:46</EventTime>
				<EventType>IRJ</EventType>
				<EventParameters>
					<MessageType>XER</MessageType>
					<Type>{responseType}</Type>
					<Reason>Test Reason</Reason>
				</EventParameters>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>");
	}
}
