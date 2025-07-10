using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XtCredential;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(JPCredentialChangeCustomsMessageProcessor))]
sealed class JPCredentialChangeCustomsMessageProcessorTest : TestCaseWithFactory
{
	public void TestProcessMessageCore_IAK()
	{
		var logger = new LoggingInformation();
		var processor = new JPCredentialChangeCustomsMessageProcessor();
		message.EM_MessageText = GetResourceStream("IncomingMessage_IAK.txt");

		Env.OutgoingMailManager.EmailsCreated.Clear();

		var settings = new MailboxAndRemoteWebPrintClientCredentials { LocalComputerAlias = "dummy", DomainName = "dummy", Status = XtCredentialStatusList.Codes.Unregistered };
		settings.FailureNotificationGroup = GetNotificationGroupCode();

		JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
		AssertEquals(XtCredentialStatusList.Codes.Unregistered, JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.Value.Status);

		processor.ProcessMessage(message, logger);
		AssertEquals(XtCredentialStatusList.Codes.Registered, JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.Value.Status);
		AssertEquals("Status", EDIMessage.Status.Received, message.EM_Status);

		var email = Env.OutgoingMailManager.EmailsCreated.LastOrDefault();

		CombineAssertions(() =>
		{
			AssertEquals("Subject", "WebPrint endpoint for NACCS integration has been set up successfully", email.Subject);
			AssertContains("Body", "As the result of setting up Customs -> Country or Region Specific -> Japan -> NACCS Messaging -> Remote WebPrint Client Configurations, the WebPrint endpoint has been provisioned successfully.", email.Body);
			AssertContains("Body", "Messages from CargoWise directed to NACCS will now be routed through this endpoint and ready for relay by the WebPrint client.", email.Body);
			AssertContains("Body", "Please install WebPrint client and set up to connect to CargoWise and NACCS to finish the configuration.", email.Body);
			AssertEquals("Recipient", "test@mail.com", email.Recipients.Cast<RecipientDef>().Single().Email);
		});
	}

	public void TestProcessMessageCore_IRJ()
	{
		var logger = new LoggingInformation();
		var processor = new JPCredentialChangeCustomsMessageProcessor();
		message.EM_MessageText = GetResourceStream("IncomingMessage_IRJ.txt");

		Env.OutgoingMailManager.EmailsCreated.Clear();

		var settings = new MailboxAndRemoteWebPrintClientCredentials { LocalComputerAlias = "dummy", DomainName = "dummy", Status = XtCredentialStatusList.Codes.Unregistered };
		settings.FailureNotificationGroup = GetNotificationGroupCode();

		JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
		AssertEquals(XtCredentialStatusList.Codes.Unregistered, JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.Value.Status);

		processor.ProcessMessage(message, logger);
		AssertEquals(XtCredentialStatusList.Codes.Error, JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.Value.Status);

		var email = Env.OutgoingMailManager.EmailsCreated.LastOrDefault();

		CombineAssertions(() =>
		{
			AssertEquals("Subject", "Failed to set up WebPrint endpoint for NACCS integration", email.Subject);
			AssertContains("Body", "Please investigate by going to the Maintain > EDI Messaging > EDI Message module and query by Message Number - 0000000000000001.", email.Body);
			AssertContains("Body", "The endpoint is necessary as part of Customs -> Country or Region Specific -> Japan -> NACCS Messaging -> Remote WebPrint Client Configurations.", email.Body);
			AssertEquals("Recipient", "test@mail.com", email.Recipients.Cast<RecipientDef>().Single().Email);
		});
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

	string GetResourceStream(string fileName)
	{
		using (var stream = GetType().Assembly.GetManifestResourceStream($"Enterprise.Customs.JP.Common.Testing.UCMP.TestFiles.{fileName}"))
		{
			return new StreamReader(stream).ReadToEnd();
		}
	}

	public void TestGetLinkedBusinessObjectMetaData_ReturnsDefault()
	{
		var processor = new JPCredentialChangeCustomsMessageProcessor();

		var result = processor.GetLinkedBusinessObjectMetaData(message, new LoggingInformation());
		AssertEquals("LinkTableName should match the message.EM_LinkTable", message.EM_LinkTable, result.ReturnValue.LinkTableName);
		AssertEquals("LinkUniqueId should match the message.EM_LinkUniqueId", message.EM_LinkUniqueID, result.ReturnValue.LinkUniqueID);
		AssertEquals("BranchPk should match the message.EM_GB", message.EM_GB, result.ReturnValue.BranchPk);
		AssertEquals("JobNumber should be empty", ZString.Empty, result.ReturnValue.JobNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var sessionGuid = ZGuid.NewZGuid();
		outgoingInterchange = Factory.New<EDIInterchange>();
		var incomingInterchange = Factory.New<EDIInterchange>();
		incomingInterchange.EI_SessionGUID = sessionGuid;
		incomingInterchange.EI_ApplicationCode = "CFG";
		incomingInterchange.EI_To = "WTLDJPCTU";
		incomingInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
		incomingInterchange.EI_From = XtCredentialConstants.CustomsCredentialChange;
		outgoingInterchange.EI_SessionGUID = sessionGuid;
		outgoingInterchange.EI_ApplicationCode = incomingInterchange.EI_ApplicationCode;
		outgoingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		outgoingInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
		outgoingInterchange.EI_From = "WTLDJPCTU";
		outgoingInterchange.EI_To = XtCredentialConstants.CustomsCredentialChange;
		outgoingInterchange.EI_InterchangeType = EDIMessage.ApplicationCodes.JPCustoms;
		outgoingInterchange.EI_BodyText = GetResourceStream("OutgoingInterchange.txt");
		message = Factory.New<EDIMessage>();
		message.EM_EI = incomingInterchange.PK;
		message.EM_LinkTable = "ABC";
		message.EM_LinkUniqueID = ZGuid.NewZGuid();

		Factory.Save();
	}

	EDIMessage message;
	EDIInterchange outgoingInterchange;
}
