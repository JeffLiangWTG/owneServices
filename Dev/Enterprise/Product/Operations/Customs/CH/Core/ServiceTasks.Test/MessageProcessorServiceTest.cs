using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CH.ServiceTasks.Testing;

[TestedType(typeof(MessageProcessorService))]
sealed class MessageProcessorServiceTest : BranchMessageProcessorServiceTest<MessageProcessorService>
{
	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
	{
		get
		{
			return new TaskNudgeInformationForTest[]
			{
					new TaskNudgeInformationForTest(
					EDIMessageSchema.Constants.TableName,
					ServiceTaskApplicationCodeList.Descriptions.MessageProcessor + " " + EDIMessage.ApplicationCodes.CHCustomsEdec,
					EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
					EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
					EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CHCustomsEdec,
					EDIMessageSchema.Constants.EM_IsActive + "=Y",
					EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
					EDIMessageSchema.Constants.TableName,
					ServiceTaskApplicationCodeList.Descriptions.MessageProcessor + " " + EDIMessage.ApplicationCodes.CHCustomsPassar,
					EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
					EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
					EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CHCustomsPassar,
					EDIMessageSchema.Constants.EM_IsActive + "=Y",
					EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
					EDIMessageSchema.Constants.TableName,
					ServiceTaskApplicationCodeList.Descriptions.MessageProcessor + " " + EDIMessage.ApplicationCodes.CHCustomsCharteraOutput,
					EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
					EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
					EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CHCustomsCharteraOutput,
					EDIMessageSchema.Constants.EM_IsActive + "=Y",
					EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
			};
		}
	}

	public void TestHostedServiceAttribute()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();

		CombineAssertions(() =>
		{
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();
			AssertEquals("Code", ServiceTaskApplicationCodeList.Codes.MessageProcessor, hostedServiceAttribute.Code);
			AssertEquals("Description", ServiceTaskApplicationCodeList.Descriptions.MessageProcessor, hostedServiceAttribute.Description);
			AssertEquals("Category", EDIMessage.ApplicationCodes.CHCustomsEdec, hostedServiceAttribute.Category);
			AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Switzerland, hostedServiceAttribute.RequiresCompanyInCountry);
			AssertEquals("Type", typeof(MessageProcessorService), hostedServiceAttribute.Type);
			AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
			AssertEquals("DefaultScheduleRunEvery", "15minutes", hostedServiceAttribute.DefaultScheduleRunEvery);
		});
	}

	public void TestInitialiseSchedule()
	{
		var testTask = new MessageProcessorService();
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

	public void TestRunTaskForEdecResponseRejection() => AssertRunTask("35253", TestingData.InputEdecResponseRejectionXml, MessageSubTypeCodeList.Codes.RuleError, CHLogicalStatusList.Codes.Invalid);

	public void TestRunTaskForEdecResponseAcceptance() => AssertRunTask("35253", TestingData.InputEdecResponseAcceptanceXml, MessageSubTypeCodeList.Codes.Accepted, CHLogicalStatusList.Codes.Accepted);

	public void TestRunTaskWhenBranchIsNotCH()
	{
		var company = Factory.New<GlbCompany>();
		company.GC_Code = "CXX";
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Afghanistan;
		var branch = company.Branches.AddNew();
		branch.GB_Code = "BXX";
		branch.GB_IsActive = true;
		Factory.Save();

		AssertRunTask("35253", TestingData.InputEdecResponseAcceptanceXml, MessageSubTypeCodeList.Codes.Accepted, CHLogicalStatusList.Codes.Accepted, messageBranchPK: branch.PK);
	}

	void AssertRunTask(string referenceBGM, string messageResponseText, string messageSubType, string expectedStatus, ZGuid? messageBranchPK = null)
	{
		(var entryHeader, _) = MessageProcessorTestHelper.CreateHeaderAndMessage(Factory,
			JobMessageTypeList.Codes.Import,
			messageSubType,
			messageResponseText.Replace("#*BGMReference*#", referenceBGM),
			referenceBGM,
			messageBranchPK: messageBranchPK);
		Factory.Save();

		InitialiseAndRunTaskSchedule(new MessageProcessorService());

		entryHeader.Reload();

		AssertEquals("CusEntryHeader.CH_Status", expectedStatus, entryHeader.CH_Status);
	}

	public void TestIsRequiredWithTokenCredentials()
	{
		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("There is no Token Credential / Certificate configured in Switzerland.", MessageProcessorService.IsRequired());

		CredentialsTestHelper.CreateCurrentCompanyTokenCredential();

		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("IsRequred when a swiss company has relevant Token Credentials", "", MessageProcessorService.IsRequired());
	}

	public void TestIsRequiredWithCertificate()
	{
		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("There is no Token Credential / Certificate configured in Switzerland.", MessageProcessorService.IsRequired());

		CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("IsRequred when a swiss company has relevant certificate", "", MessageProcessorService.IsRequired());
	}

	protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
	{
		(_, var message) = MessageProcessorTestHelper.CreateHeaderAndMessage(Factory,
			JobMessageTypeList.Codes.Import,
			MessageSubTypeCodeList.Codes.Accepted,
			TestingData.InputEdecResponseAcceptanceXml.Replace("#*BGMReference*#", "35253"),
			"35253");

		return new BranchMessageProcessorServiceTestHelperData()
		{
			MessagePK = message.PK
		};
	}

	protected override MessageProcessorService CreateServiceTask() => new MessageProcessingServiceForTesting();

	class MessageProcessingServiceForTesting : MessageProcessorService
	{
		protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new InboundMessageProcessorHubForTesting(ApplicationCodes, MessageTypes);
	}

	class InboundMessageProcessorHubForTesting : InboundMessageProcessorHub
	{
		public InboundMessageProcessorHubForTesting(IEnumerable<ZString> applicationCodes, IEnumerable<ZString> messageTypes) : base(applicationCodes, messageTypes)
		{
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessorsCore()
		{
			var result = base.GetMessageProcessorsCore();
			result.Add(new BaseMessageProcessorForTesting(Logger));
			return result;
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			return new BaseMessageProcessorForTesting(Logger);
		}
	}

	class BaseMessageProcessorForTesting : BranchCustomsApplicationTypeMessageProcessor
	{
		public BaseMessageProcessorForTesting(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => "MessageProcessingServiceTestBaseMessageProcessorForTest";

		protected sealed override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsEdec;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			message.EM_Status = EDIMessageStatusList.Codes.Received;
		}

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeForTesting };
	}

	const string MessageTypeForTesting = "XYZ";
}
