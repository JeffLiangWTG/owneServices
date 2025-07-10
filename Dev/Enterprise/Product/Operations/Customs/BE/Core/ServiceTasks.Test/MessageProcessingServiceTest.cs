using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.BE.ServiceTasks.Testing;

[TestedType(typeof(MessageProcessingService))]
sealed class MessageProcessingServiceTest : BranchMessageProcessorServiceTest<MessageProcessingService>
{
	static readonly ZString messageTypeForTesting = "TST";

	public void TestCanRunInAnyBranch()
	{
		SetupDataForTesting();
		Factory.Save();
		var service = new MessageProcessingService();
		InitialiseTaskSchedule(service);
		using (Env.Instance.TemporaryServiceTaskContext(BEMessageServiceTypeList.Codes.BeCustomsMessagesInbound, canRunInAnyBranch: true))
		{
			AssertNoExceptionThrown(service.RunTask);
		}
	}

	public void TestHostedServiceAttribute()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
		var hostedServiceAttribute = hostedServiceAttributes.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Code", "BEP", hostedServiceAttribute.Code);
			AssertEquals("Description", "BE Customs messages inbound", hostedServiceAttribute.Description);
			AssertEquals("Category", "BEC", hostedServiceAttribute.Category);
			AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Belgium, hostedServiceAttribute.RequiresCompanyInCountry);
			AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
		});
	}
	public void TestHostedServiceRequirement()
	{
		var hostedServiceRequirementMethod = typeof(MessageProcessingService).GetMethods().FirstOrDefault(p => Attribute.IsDefined(p, typeof(HostedServiceRequirementAttribute)));
		AssertNotNull("Method with Attribute HostedServiceRequirement should exist on MessageProcessingService", hostedServiceRequirementMethod);

		CombineAssertions(() =>
		{
			var combinations = new List<(ZString passwordType, string requiredExpected)>
									{ ("", "There is no Credential configured in Belgium."),
									  (PasswordTypesList.Codes.BEC, string.Empty) };
			var credential = BE.Business.BEGlbCompanyWrapper.GetWrapper<BE.Business.BEGlbCompanyWrapper>(GlbCompany.CurrentCompany).Credential;

			foreach (var combination in combinations)
			{
				credential.GP_PasswordType = combination.passwordType;
				credential.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
				credential.Factory.Save();
				CertificateRequirementChecker.ResetForTesting();
				AssertEquals($"Credential Password Type: {combination.passwordType}", combination.requiredExpected, hostedServiceRequirementMethod.Invoke(null, null));
			}
		});
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[]
	{
		new TaskNudgeInformationForTest(
			EDIMessageSchema.Constants.TableName,
			"BE Customs messages inbound",
			EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
			EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
			EDIMessageSchema.Constants.EM_IsActive + "=Y",
			EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.BECustoms,
			EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
		new TaskNudgeInformationForTest(
			EDIMessageSchema.Constants.TableName,
			"BE Customs Inbound Message Processing",
			EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.PreProcessedOK,
			EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
			EDIMessageSchema.Constants.EM_IsActive + "=Y",
			EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.BECustoms,
			EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
	};

	protected override MessageProcessingService CreateServiceTask() => new MessageProcessingServiceForTest();

	protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
	{
		var message = CreateMessage();

		return new BranchMessageProcessorServiceTestHelperData()
		{
			MessagePK = message.PK
		};
	}

	BEMessage CreateMessage()
	{
		var message = Factory.New<BEMessage>();

		message.EM_MessageType = messageTypeForTesting;
		message.EM_ApplicationCode = EDIMessage.ApplicationCodes.BECustoms;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_Status = EDIMessageStatusList.Codes.Queued;
		message.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<CC556C xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://ecs.dgtaxud.ec"">
	<messageSender xmlns="""">AES</messageSender>
	<messageRecipient xmlns="""">CW1@BE0452814806</messageRecipient>
	<preparationDateAndTime xmlns="""">2022-04-01T12:34:56.123456789Z</preparationDateAndTime>
	<messageIdentification xmlns="""">12af74cb-93e8-4632-98b0-c10b477eaf10</messageIdentification>
	<messageType xmlns="""">CC556C</messageType>
	<correlationIdentifier xmlns="""">37abde20-076c-40d6-ba58-28a5eae74b2d</correlationIdentifier>
	<ExportOperation xmlns="""">
		<LRN>22045281480600000001</LRN>
		<MRN>22BEE00000000012J1</MRN>
		<businessRejectionType>515</businessRejectionType>
		<rejectionDateAndTime>2022-04-01T12:34:56.123456789Z</rejectionDateAndTime>
		<rejectionCode>4</rejectionCode>
		<rejectionReason>invalid value transport type</rejectionReason>
	</ExportOperation>
	<CustomsOfficeOfExport xmlns="""">
		<referenceNumber>BE101000</referenceNumber>
	</CustomsOfficeOfExport>
	<Declarant xmlns="""">
		<identificationNumber>BE0449424358</identificationNumber>
		<name></name>
	</Declarant>
	<Representative xmlns="""">
		<identificationNumber>BE0452806814</identificationNumber>
		<status>2</status>
	</Representative>
	<FunctionalError xmlns="""">
		<errorPointer>cc515c.DepartureTransportMeans(2).typeOfIdentification</errorPointer>
		<errorCode>12</errorCode>
		<errorReason>Type of transport does not exist</errorReason>
		<originalAttributeValue>32</originalAttributeValue>
	</FunctionalError>
</CC556C>";

		Factory.Save();
		return message;
	}

	class MessageProcessingServiceForTest : MessageProcessingService
	{
		protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new BECIncomingMessageProcessorForTest();
	}

	class BECIncomingMessageProcessorForTest : BECIncomingMessageProcessor
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessorsCore()
		{
			var result = base.GetMessageProcessorsCore();
			result.Add(new BaseMessageProcessorForTest(Logger));
			return result;
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			return new BaseMessageProcessorForTest(Logger);
		}
	}

	class BaseMessageProcessorForTest : BranchCustomsApplicationTypeMessageProcessor
	{
		public BaseMessageProcessorForTest(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => "MessageProcessingServiceTestBaseMessageProcessorForTest";

		protected sealed override string ApplicationCodeCore => ApplicationCodeList.Codes.BECustoms;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			message.EM_Status = EDIMessageStatusList.Codes.Received;
		}

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { messageTypeForTesting };
	}
}
