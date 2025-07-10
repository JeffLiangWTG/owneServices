using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.CH.ServiceTasks.Testing;

[TestedType(typeof(MessageSendingService))]
sealed class MessageSendingServiceTest : ServiceTaskTestCase<MessageSendingService>
{
	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
	{
		get
		{
			return new TaskNudgeInformationForTest[]
			{
				new TaskNudgeInformationForTest(
					EDIMessageSchema.Constants.TableName,
					ServiceTaskApplicationCodeList.Descriptions.MessageSender + " " + ApplicationCodeList.Codes.CHCustomsEdec,
					EDIMessageSchema.Constants.EM_IsActive + "=Y",
					EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
					EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
					EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.CHCustomsEdec,
					EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

				new TaskNudgeInformationForTest(
					EDIMessageSchema.Constants.TableName,
					ServiceTaskApplicationCodeList.Descriptions.MessageSender + " " + ApplicationCodeList.Codes.CHCustomsPassar,
					EDIMessageSchema.Constants.EM_IsActive + "=Y",
					EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
					EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
					EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.CHCustomsPassar,
					EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

				new TaskNudgeInformationForTest(
					EDIMessageSchema.Constants.TableName,
					ServiceTaskApplicationCodeList.Descriptions.MessageSender + " " + ApplicationCodeList.Codes.CHCustomsCharteraOutput,
					EDIMessageSchema.Constants.EM_IsActive + "=Y",
					EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
					EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
					EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.CHCustomsCharteraOutput,
					EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
			};
		}
	}

	public void TestHostedServiceAttribute()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
		var hostedServiceAttribute = hostedServiceAttributes.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Code", "CHS", hostedServiceAttribute.Code);
			AssertEquals("Description", "Swiss Customs Message Sender", hostedServiceAttribute.Description);
			AssertEquals("Category", "CHC", hostedServiceAttribute.Category);
			AssertEquals("RequiresCompanyInCountry", Enterprise.Core.Constants.CountryCodes.Switzerland, hostedServiceAttribute.RequiresCompanyInCountry);
			AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
			AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
			AssertEquals("DefaultScheduleRunEvery", "15minutes", hostedServiceAttribute.DefaultScheduleRunEvery);
		});
	}

	public void TestInitialiseSchedule()
	{
		var serviceTask = new MessageSendingService();
		InitialiseTaskSchedule(serviceTask, out StmServiceTask taskSchedule);

		CombineAssertions(() =>
		{
			AssertEquals("IsActive", ZBool.True, taskSchedule.SST_Active);
			Assert("TaskPeriod", taskSchedule.Recurrence.MinutesRange);
			AssertEquals("TaskPeriodCount", 15, taskSchedule.Recurrence.Period);
			AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("Is DailyStartTime empty?", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
		});
	}

	public void TestRunTask()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();

		var entryHeaderCorrectXML = declaration.CustomsEntryHeaders.AddNew();
		entryHeaderCorrectXML.CH_BGMReference = "CH000001";
		var correctMessageXML = CreateOutboundMessage(ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.Import, EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, MessageBodyText);
		correctMessageXML.EM_LinkedObject = entryHeaderCorrectXML;

		var entryHeaderIncorrectMessageTypeXML = declaration.CustomsEntryHeaders.AddNew();
		entryHeaderIncorrectMessageTypeXML.CH_BGMReference = "CH000002";

		var incorrectApplicationCodeMessage = CreateOutboundMessage("AAA", MessageTypeCodeList.Codes.Import, EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, MessageBodyText);
		var incorrectDirectionMessage = CreateOutboundMessage(ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.Import, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, MessageBodyText);
		var incorrectStatusMessage = CreateOutboundMessage(ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.Import, EDIMessage.Direction.Transmit, EDIMessage.Status.Withdrawn, MessageBodyText);

		var nullLinkedObjectMessage = CreateOutboundMessage(ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.Import, EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, MessageBodyText);

		var entryHeaderEmptyApplication = declaration.CustomsEntryHeaders.AddNew();
		entryHeaderEmptyApplication.CH_BGMReference = "CH000003";

		var entryHeaderEmptyText = declaration.CustomsEntryHeaders.AddNew();
		entryHeaderEmptyText.CH_BGMReference = "CH000004";
		var emptyTextMessage = CreateOutboundMessage(ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.Import, EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, ZString.Empty);
		emptyTextMessage.EM_LinkedObject = entryHeaderEmptyText;

		Factory.Save();

		CombineAssertions("PRE-CONDITION", () =>
		{
			AssertNull($"{nameof(correctMessageXML)} Message Interchange", correctMessageXML.Interchange);
			AssertNull($"{nameof(incorrectApplicationCodeMessage)} Message Interchange", incorrectApplicationCodeMessage.Interchange);
			AssertNull($"{nameof(incorrectDirectionMessage)} Message Interchange", incorrectDirectionMessage.Interchange);
			AssertNull($"{nameof(incorrectStatusMessage)} Message Interchange", incorrectStatusMessage.Interchange);
			AssertNull($"{nameof(nullLinkedObjectMessage)} Message Interchange", nullLinkedObjectMessage.Interchange);
			AssertNull($"{nameof(emptyTextMessage)} Message Interchange", emptyTextMessage.Interchange);
		});

		var logs = InitialiseAndRunTaskSchedule(new MessageSendingService());

		correctMessageXML.Reload();
		incorrectApplicationCodeMessage.Reload();
		incorrectDirectionMessage.Reload();
		incorrectStatusMessage.Reload();
		nullLinkedObjectMessage.Reload();
		emptyTextMessage.Reload();

		CombineAssertions("Status", () =>
		{
			AssertEquals($"{nameof(correctMessageXML)} Status", EDIMessage.Status.Sent, correctMessageXML.EM_Status);
			AssertEquals($"{nameof(incorrectApplicationCodeMessage)} Status", EDIMessage.Status.Queued, incorrectApplicationCodeMessage.EM_Status);
			AssertEquals($"{nameof(incorrectDirectionMessage)} Status", EDIMessage.Status.Queued, incorrectDirectionMessage.EM_Status);
			AssertEquals($"{nameof(incorrectStatusMessage)} Status", EDIMessage.Status.Withdrawn, incorrectStatusMessage.EM_Status);
			AssertEquals($"{nameof(nullLinkedObjectMessage)} Status", EDIMessage.Status.Failed, nullLinkedObjectMessage.EM_Status);
			AssertEquals($"{nameof(emptyTextMessage)} Status", EDIMessage.Status.Failed, emptyTextMessage.EM_Status);

			AssertNotNull($"{nameof(correctMessageXML)} Interchange", correctMessageXML.Interchange);
			AssertNull($"{nameof(incorrectApplicationCodeMessage)} Interchange", incorrectApplicationCodeMessage.Interchange);
			AssertNull($"{nameof(incorrectDirectionMessage)} Interchange", incorrectDirectionMessage.Interchange);
			AssertNull($"{nameof(incorrectStatusMessage)} Interchange", incorrectStatusMessage.Interchange);
			AssertNull($"{nameof(nullLinkedObjectMessage)} Interchange", nullLinkedObjectMessage.Interchange);
			AssertNull($"{nameof(emptyTextMessage)} Interchange", emptyTextMessage.Interchange);
		});
	}

	public void TestFailedMessagesInInterchange()
	{
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_BGMReference = "CH000001";

				var ediMessage = CreateOutboundMessage(ApplicationCodeList.Codes.CHCustomsEdec, string.Empty, EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, MessageBodyText);
				Factory.Save();
				AssertEquals("Pre-Requisite: EM_ApplicationCode is blank -> meaning EI_to is blank, which will cause a failure to package it", string.Empty, ediMessage.EM_MessageType);

				Factory.Save();

				var logs = InitialiseAndRunTaskSchedule(new MessageSendingService());
				AssertContains("Logger Warning Message", "No Interchange has been created", logs[0]);

				var reloadedMessage = new BusinessObjectFactory().Load<CHEDIMessage>(ediMessage.PK);
				AssertEquals("Message has no Interchange ", ZGuid.Empty, ediMessage.EM_EI);
			});
		});
	}

	public void TestIsRequiredWithTokenCredentials()
	{
		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("There is no Token Credential / Certificate configured in Switzerland.", MessageSendingService.IsRequired());

		CredentialsTestHelper.CreateCurrentCompanyTokenCredential();

		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("IsRequred when a swiss company has relevant Token Credentials", "", MessageSendingService.IsRequired());
	}

	public void TestIsRequiredWithCertificate()
	{
		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("There is no Token Credential / Certificate configured in Switzerland.", MessageSendingService.IsRequired());

		CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("IsRequred when a swiss company has relevant certificate", "", MessageSendingService.IsRequired());
	}

	EDIMessage CreateOutboundMessage(ZString applicationCode, ZString messageType, ZString direction, ZString status, ZString messageText)
	{
		var message = Factory.New<CHEDIMessage>();
		message.EM_ApplicationCode = applicationCode;
		message.EM_MessageType = messageType;
		message.EM_MessageSubType = ""; //DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
		message.EM_ReceiveTransmit = direction;
		message.EM_Status = status;
		message.EM_MessageText = messageText;
		message.EM_IsTestMessage = true;
		message.EM_ApplicationReference = "[CertificateName]"; //Certificate.CertificateName;
		return message;
	}

	ZString MessageBodyText => @"<?xml version=""1.0"" encoding=""utf-8""?>
<goodsDeclarations xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" schemaVersion=""4.0"" xmlns=""http://www.e-dec.ch/xml/schema/edec/v4"">
  <goodsDeclaration>
  </goodsDeclaration>
</goodsDeclarations>";
}
