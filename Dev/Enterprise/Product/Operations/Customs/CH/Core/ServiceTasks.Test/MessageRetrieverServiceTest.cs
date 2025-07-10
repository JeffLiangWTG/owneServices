using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.CH.ServiceTasks.Testing;

[TestedType(typeof(MessageRetrieverService))]
sealed class MessageRetrieverServiceTest : ServiceTaskTestCase<MessageRetrieverService>
{
	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
	{
		get
		{
			return new TaskNudgeInformationForTest[]
			{
					new TaskNudgeInformationForTest(
					EDIInterchangeSchema.Constants.TableName,
					ServiceTaskApplicationCodeList.Descriptions.MessageRetriever + " " + EDIInterchange.ApplicationCodes.CHCustomsEdec,
					EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
					EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
					EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CHCustomsEdec,
					EDIInterchangeSchema.Constants.EI_IsActive + "=Y"),

					new TaskNudgeInformationForTest(
					EDIInterchangeSchema.Constants.TableName,
					ServiceTaskApplicationCodeList.Descriptions.MessageRetriever + " " + EDIInterchange.ApplicationCodes.CHCustomsPassar,
					EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
					EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
					EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CHCustomsPassar,
					EDIInterchangeSchema.Constants.EI_IsActive + "=Y"),

					new TaskNudgeInformationForTest(
					EDIInterchangeSchema.Constants.TableName,
					ServiceTaskApplicationCodeList.Descriptions.MessageRetriever + " " + EDIInterchange.ApplicationCodes.CHCustomsCharteraOutput,
					EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
					EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
					EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CHCustomsCharteraOutput,
					EDIInterchangeSchema.Constants.EI_IsActive + "=Y"),
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
			AssertEquals("Code", ServiceTaskApplicationCodeList.Codes.MessageRetriever, hostedServiceAttribute.Code);
			AssertEquals("Description", ServiceTaskApplicationCodeList.Descriptions.MessageRetriever, hostedServiceAttribute.Description);
			AssertEquals("Category", "CHC", hostedServiceAttribute.Category);
			AssertEquals("RequiresCompanyInCountry", Enterprise.Core.Constants.CountryCodes.Switzerland, hostedServiceAttribute.RequiresCompanyInCountry);
			AssertEquals("Type", typeof(MessageRetrieverService), hostedServiceAttribute.Type);
			AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
			AssertEquals("DefaultScheduleRunEvery", "15minutes", hostedServiceAttribute.DefaultScheduleRunEvery);
		});
	}

	public void TestInitialiseSchedule()
	{
		var testTask = new MessageRetrieverService();
		InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);

		CombineAssertions(() =>
		{
			AssertEquals("IsActive", ZBool.True, taskSchedule.SST_Active);
			Assert("TaskPeriod", taskSchedule.Recurrence.MinutesRange);
			AssertEquals("TaskPeriodCount", 15, taskSchedule.Recurrence.Period);
			AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("Is DailyStartTime empty?", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
		});
	}

	public void TestRunTaskForEdecResponseImportAcceptanceSOAP() => TestRunTask(MessagingConstants.CustomsDestinationCodes.CustomsEdecSoap, TestingData.InputEdecResponseAcceptanceResponseSOAP, TestingData.ExpectedEdecResponseAcceptanceXmlOnlySOAP, JobMessageTypeList.Codes.Import, MessageSubTypeCodeList.Codes.Accepted, 0);

	public void TestRunTaskForEdecResponseImportRejectionSOAP() => TestRunTask(MessagingConstants.CustomsDestinationCodes.CustomsEdecSoap, TestingData.InputEdecEdecResponseRejectionResponseSOAP, TestingData.ExpectedEdecResponseRejectionXmlOnlySOAP, JobMessageTypeList.Codes.Import, MessageSubTypeCodeList.Codes.RuleError, 0);

	public void TestRunTaskForEdecResponseImportDocumentSOAP() => TestRunTask(MessagingConstants.CustomsDestinationCodes.CustomsEdecSoap, TestingData.InputEdecResponseAcceptanceResponseSOAP, TestingData.ExpectedEdecResponseAcceptanceDocOnlySOAP, JobMessageTypeList.Codes.Import, MessageSubTypeCodeList.Codes.Document, 1);

	public void TestRunTaskForEdecResponseExportAcceptanceSOAP() => TestRunTask(MessagingConstants.CustomsDestinationCodes.CustomsEdecSoap, TestingData.InputEdecResponseAcceptanceResponseSOAP, TestingData.ExpectedEdecResponseAcceptanceXmlOnlySOAP, JobMessageTypeList.Codes.Export, MessageSubTypeCodeList.Codes.Accepted, 0);

	public void TestRunTaskForEdecResponseExportRejectionSOAP() => TestRunTask(MessagingConstants.CustomsDestinationCodes.CustomsEdecSoap, TestingData.InputEdecEdecResponseRejectionResponseSOAP, TestingData.ExpectedEdecResponseRejectionXmlOnlySOAP, JobMessageTypeList.Codes.Export, MessageSubTypeCodeList.Codes.RuleError, 0);

	public void TestRunTaskForEdecResponseExportDocumentSOAP() => TestRunTask(MessagingConstants.CustomsDestinationCodes.CustomsEdecSoap, TestingData.InputEdecResponseAcceptanceResponseSOAP, TestingData.ExpectedEdecResponseAcceptanceDocOnlySOAP, JobMessageTypeList.Codes.Export, MessageSubTypeCodeList.Codes.Document, 1);

	public void TestRunTaskForEdecResponseImportAcceptanceMail() => TestRunTask(MessagingConstants.CustomsDestinationCodes.CustomsEdecEmail, TestingData.InputEdecResponseAcceptanceResponseMail, TestingData.ExpectedEdecResponseAcceptanceXmlOnlyMail, JobMessageTypeList.Codes.Import, MessageSubTypeCodeList.Codes.Accepted, 0);

	public void TestRunTaskForEdecResponseImportRejectionMail() => TestRunTask(MessagingConstants.CustomsDestinationCodes.CustomsEdecEmail, TestingData.InputEdecEdecResponseRejectionResponseMail, TestingData.ExpectedEdecResponseRejectionXmlOnlyMail, JobMessageTypeList.Codes.Import, MessageSubTypeCodeList.Codes.CustomsRejected, 0);

	public void TestRunTaskForEdecResponseImportStatusMail() => TestRunTask(MessagingConstants.CustomsDestinationCodes.CustomsEdecEmail, TestingData.InputEdecEdecResponseStatusResponseMail, TestingData.ExpectedEdecResponseStatusXmlOnlyMail, JobMessageTypeList.Codes.Import, MessageSubTypeCodeList.Codes.Status, 0);

	public void TestRunTaskForEdecResponseExportAcceptanceMail() => TestRunTask(MessagingConstants.CustomsDestinationCodes.CustomsEdecEmail, TestingData.InputEdecResponseAcceptanceResponseMail, TestingData.ExpectedEdecResponseAcceptanceXmlOnlyMail, JobMessageTypeList.Codes.Export, MessageSubTypeCodeList.Codes.Accepted, 0);

	public void TestRunTaskForEdecResponseExportRejectionMail() => TestRunTask(MessagingConstants.CustomsDestinationCodes.CustomsEdecEmail, TestingData.InputEdecEdecResponseRejectionResponseMail, TestingData.ExpectedEdecResponseRejectionXmlOnlyMail, JobMessageTypeList.Codes.Export, MessageSubTypeCodeList.Codes.CustomsRejected, 0);

	public void TestRunTaskForEdecResponseExportStatusMail() => TestRunTask(MessagingConstants.CustomsDestinationCodes.CustomsEdecEmail, TestingData.InputEdecEdecResponseStatusResponseMail, TestingData.ExpectedEdecResponseStatusXmlOnlyMail, JobMessageTypeList.Codes.Export, MessageSubTypeCodeList.Codes.Status, 0);

	public void TestIsRequiredWithTokenCredentials()
	{
		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("There is no Token Credential / Certificate configured in Switzerland.", MessageRetrieverService.IsRequired());

		CredentialsTestHelper.CreateCurrentCompanyTokenCredential();

		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("IsRequred when a swiss company has relevant Token Credentials", "", MessageRetrieverService.IsRequired());
	}

	public void TestIsRequiredWithCertificate()
	{
		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("There is no Token Credential / Certificate configured in Switzerland.", MessageRetrieverService.IsRequired());

		CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("IsRequred when a swiss company has relevant certificate", "", MessageRetrieverService.IsRequired());
	}

	void TestRunTask(string from, string edecResponse, string edecResponseXmlOnly, string interchangeType, string expectedMessageSubType, int expectedMessageIndex)
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CHCustomsEdec;
		interchange.EI_BodyText = edecResponse;
		interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange.EI_Status = EDIInterchange.Status.Queued;
		interchange.EI_InterchangeType = interchangeType;
		interchange.EI_From = from;
		interchange.EI_To = "TEST";

		Factory.Save();

		var logs = InitialiseAndRunTaskSchedule(new MessageRetrieverService());
		interchange.Reload();

		CombineAssertions("Interchange Message", () =>
		{
			var createdMessage = interchange.ContainedMessages[expectedMessageIndex];
			AssertEquals("EM_ApplicationCode", interchange.EI_ApplicationCode, createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", interchange.EI_InterchangeType, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", expectedMessageSubType, createdMessage.EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage.EM_Status);
			AssertEquals("EM_MessageNum", (++expectedMessageIndex).ToString().PadLeft(4, '0'), createdMessage.EM_MessageNum);
			AssertEquals("EM_EI", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_GB", interchange.EI_GB, createdMessage.EM_GB);

			var actualXml = XDocument.Parse(createdMessage.EM_MessageText);
			var expectedXml = XDocument.Parse(edecResponseXmlOnly);
			AssertEquals(true, XNode.DeepEquals(actualXml, expectedXml));
		});
	}
}
