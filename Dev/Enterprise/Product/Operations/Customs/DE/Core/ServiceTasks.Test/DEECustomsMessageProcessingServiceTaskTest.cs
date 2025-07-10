using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.DE.ServiceTasks.Testing
{
	[TestedType(typeof(DEECustomsMessageProcessingServiceTask))]
	sealed class DEECustomsMessageProcessingServiceTaskTest : BranchMessageProcessorServiceTest<DEECustomsMessageProcessingServiceTask>
	{
		public void TestProcessUnknownMessage()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAesSystem;
			message.EM_ApplicationReference = "GCRECE";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = "T1T";
			message.EM_MessageSubType = "TST";
			message.EM_MessageText = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<DECustomsData>
	<LogbookTime>2020-04-27T12:29:00.2347048+01:00</LogbookTime>
	<CustomsData>
		<GCRECE>
		</GCRECE>
	</CustomsData>
</DECustomsData>";
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			ErrorReporter.Clear();
			var serviceTask = new DEECustomsMessageProcessingServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();
			var factory = new BusinessObjectFactory();
			message = factory.Load<EDIMessage>(message.PK);
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
		}

		public void TestHostedServiceBindingAttribute()
		{
			TestHelper.AssertSingleHostedServiceAttribute<DEECustomsMessageProcessingServiceTask>(ServiceTaskApplicationCodeList.Codes.DEEMessageProcessing,
				ServiceTaskApplicationCodeList.Descriptions.DEEMessageProcessing,
				"DEC",
				typeof(DEECustomsMessageProcessingServiceTask),
				"60Seconds",
				Core.Constants.CountryCodes.Germany,
				true);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"DE Aes Message Processing",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.DECustomsAesSystem,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL")
				};
			}
		}

		protected override DEECustomsMessageProcessingServiceTask CreateServiceTask() => new DEECustomsMessageProcessingServiceTask();

		protected override void AssertResult(BusinessObjectFactory factory, BranchMessageProcessorServiceTestHelperData testData, DEECustomsMessageProcessingServiceTask serviceTask)
		{
			CombineAssertions(() =>
			{
				AssertEquals("ResponseMessage.EM_Status", EDIMessage.Status.ProcessedOK, factory.Load<AesInboundEDIMessage<IUnderCustomsControl>>(testData.MessagePK).EM_Status);
				AssertEquals("OriginalMessage.EM_Status", EDIMessage.Status.Acknowledged, factory.Load<AesEDIMessage>(originalMessage.PK).EM_Status);
			});
		}

		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
		{
			originalMessage = GetOriginalMessage();
			responseMessage = GetResponseMessage(originalMessage.EM_MessageNum);

			return new BranchMessageProcessorServiceTestHelperData()
			{
				MessagePK = responseMessage.PK
			};
		}
		AesEDIMessage originalMessage;
		AesInboundEDIMessage<IEXQSTA> responseMessage;

		AesEDIMessage GetOriginalMessage()
		{
			var originalMessage = Factory.New<AesEDIMessage>();
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_MessageNum = "0002342321";
			originalMessage.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			return originalMessage;
		}

		AesInboundEDIMessage<IEXQSTA> GetResponseMessage(ZString referencedMessage)
		{
			var responseMessage = Factory.New<AesInboundEDIMessage<IEXQSTA>>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAesSystem;
			responseMessage.EM_ApplicationReference = nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXQSB);
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = EDIMessageTypeList.Codes.AES;
			responseMessage.EM_MessageNum = "0002342323";
			responseMessage.EM_MessageSubType = "EXQ";
			responseMessage.EM_MessageText = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<DECustomsData>
<LogbookTime>2021-07-01T10:25:00.2347048+01:00</LogbookTime>
<CustomsData>
<DEXQSB>
	<preparationDateAndTime>2000-01-01T00:00:00</preparationDateAndTime>
	<messageIdentification>0000012347</messageIdentification>
	<messageGroup>EXQ</messageGroup>
	<messageType>DEXQSB</messageType>
	<messageVersion>B.1.0</messageVersion>
	<correlationIdentifier>{referencedMessage}</correlationIdentifier>
	<MessageSender>
		<referenceNumber>DE001342</referenceNumber>
	</MessageSender>
	<MessageRecipient>
		<identificationNumber>AA!</identificationNumber>
		<subsidiaryNumber>0000</subsidiaryNumber>
	</MessageRecipient>
	<ExportOperation>
		<MRN>00AA00000000000000</MRN>
		<exportStatus>131</exportStatus>
		<exitStatus>332</exitStatus>
		<declarationRecordationDateAndTime>2000-01-01T00:00:00</declarationRecordationDateAndTime>
		<locationRejectionDateAndTime>2000-01-01T00:00:00</locationRejectionDateAndTime>
		<presentationStartDateAndTime>2000-01-01T00:00:00</presentationStartDateAndTime>
		<loadingEndDateAndTime>2000-01-01T00:00:00</loadingEndDateAndTime>
		<declarationAcceptanceDateAndTime>2000-01-01T00:00:00</declarationAcceptanceDateAndTime>
		<amendmentRejectionDateAndTime>2000-01-01T00:00:00</amendmentRejectionDateAndTime>
		<amendmentAcceptanceDateAndTime>2000-01-01T00:00:00</amendmentAcceptanceDateAndTime>
		<supplementRejectionDateAndTime>2000-01-01T00:00:00</supplementRejectionDateAndTime>
		<supplementAcceptanceDateAndTime>2000-01-01T00:00:00</supplementAcceptanceDateAndTime>
		<nonReleaseDateAndTime>2000-01-01T00:00:00</nonReleaseDateAndTime>
		<releaseDateAndTime>2000-01-01T00:00:00</releaseDateAndTime>
		<invalidationRejectionDateAndTime>2000-01-01T00:00:00</invalidationRejectionDateAndTime>
		<invalidationAcceptanceDateAndTime>2000-01-01T00:00:00</invalidationAcceptanceDateAndTime>
		<arrivalDateAndTime>2000-01-01T00:00:00</arrivalDateAndTime>
		<qualificationDateAndTime>2000-01-01T00:00:00</qualificationDateAndTime>
		<controlNotificationDateAndTime>2000-01-01T00:00:00</controlNotificationDateAndTime>
		<permissionDateAndTime>2000-01-01T00:00:00</permissionDateAndTime>
		<exitDateAndTime>2000-01-01T00:00:00</exitDateAndTime>
		<transitDateAndTime>2000-01-01T00:00:00</transitDateAndTime>
		<forwardingDateAndTime>2000-01-01T00:00:00</forwardingDateAndTime>
		<finalizationDateAndTime>2000-01-01T00:00:00</finalizationDateAndTime>
		<interdictionDateAndTime>2000-01-01T00:00:00</interdictionDateAndTime>
		<completionDateAndTime>2000-01-01T00:00:00</completionDateAndTime>
		<cessationDateAndTime>2000-01-01T00:00:00</cessationDateAndTime>
	</ExportOperation>
	<CustomsOfficeOfExport>
		<referenceNumber>AA000000</referenceNumber>
	</CustomsOfficeOfExport>
	<CustomsOfficeOfExitDeclared>
		<referenceNumber>AA000000</referenceNumber>
	</CustomsOfficeOfExitDeclared>
	<CustomsOfficeOfExitActual>
		<referenceNumber>AA000000</referenceNumber>
	</CustomsOfficeOfExitActual>
	<Exporter>
		<identificationNumber>AA!</identificationNumber>
	</Exporter>
	<Declarant>
		<identificationNumber>AA!</identificationNumber>
	</Declarant>
	<Representative>
		<identificationNumber>AA!</identificationNumber>
	</Representative>
	<SubContractor>
		<identificationNumber>AA!</identificationNumber>
	</SubContractor>
	<Error>
		<errorCode>AAA00000</errorCode>
		<errorText>a</errorText>
	</Error>
</DEXQSB>
</CustomsData>
</DECustomsData>";
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			return responseMessage;
		}
	}
}
