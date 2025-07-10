using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.EMCS.Business;
using Enterprise.Customs.DE.EMCS.Business.Testing;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.DE.ServiceTasks;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.DE.EMCS.ServiceTasks.Testing
{
	[TestedType(typeof(DEMCustomsMessageProcessingServiceTask))]
	class DEMCustomsMessageProcessingServiceTaskTest : BranchMessageProcessorServiceTest<DEMCustomsMessageProcessingServiceTask>
	{
		public void TestProcessUnknownMessage()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsEmcsSystem;
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
			var serviceTask = new DEMCustomsMessageProcessingServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();
			var factory = new BusinessObjectFactory();
			message = factory.Load<EDIMessage>(message.PK);
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
		}

		public void TestHostedServiceBindingAttribute()
		{
			TestHelper.AssertSingleHostedServiceAttribute<DEMCustomsMessageProcessingServiceTask>(ServiceTaskApplicationCodeList.Codes.DEMMessageProcessing,
				ServiceTaskApplicationCodeList.Descriptions.DEMMessageProcessing,
				"DEC",
				typeof(DEMCustomsMessageProcessingServiceTask),
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
						"DE Emcs Message Processing",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.DECustomsEmcsSystem),
				};
			}
		}

		protected override void AssertResult(BusinessObjectFactory factory, BranchMessageProcessorServiceTestHelperData testData, DEMCustomsMessageProcessingServiceTask serviceTask)
		{
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, factory.Load<EmcsInboundEDIMessage<IUnderCustomsControl>>(testData.MessagePK).EM_Status);
				AssertEquals("JE_EntryStatus", EntryStatusList.Codes.ERJ, factory.Load<Business.EMCSJobDeclaration>(declaration.PK).JE_EntryStatus);
			});
		}

		protected override DEMCustomsMessageProcessingServiceTask CreateServiceTask() => new DEMCustomsMessageProcessingServiceTask();

		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
		{
			declaration = Factory.CreateDeclarationWithEadReference("22ABZABCDE12345678901", "1", EMCSEntryTypeList.Codes.Consignor);
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsEmcsSystem;
			message.EM_MessageType = EDIMessageTypeList.Codes.EMCS;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationReference = nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED839C);
			message.EM_MessageNum = "3480000965";
			message.EM_MessageSubType = EmcsMessageSubTypeList.Codes.Eme;
			message.EM_MessageText = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<DECustomsData>
	<LogbookTime>2020-04-02T15:53:58.2347048+01:00</LogbookTime>
	<CustomsData>
		<ED839C>
			<Header>
				<MessageSender>DE000060</MessageSender>
				<MessageRecipient>DE99000002101</MessageRecipient>
				<DateOfPreparation>2020-04-02</DateOfPreparation>
				<TimeOfPreparation>15:53:00</TimeOfPreparation>
				<InterchangeControlReference>3480000965</InterchangeControlReference>
				<MessageGroup>EME</MessageGroup>
				<MessageIdentifier>3480000965</MessageIdentifier>
				<MessageVersion>C.1.3</MessageVersion>
			</Header>
			<Body>
				<CustomsRejectionOfEad>
					<Attributes>
						<DateAndTimeOfIssuance>2020-04-02T15:53:00</DateAndTimeOfIssuance>
					</Attributes>
					<ExportCrossCheckingDiagnoses>
						<LocalReferenceNumber>B00011456</LocalReferenceNumber>
						<Diagnosis>
							<AdministrativeReferenceCode>11AAZZZZZZZZZZZZZZZZ1</AdministrativeReferenceCode>
							<BodyRecordUniqueReference>111</BodyRecordUniqueReference>
							<DiagnosisCode>1</DiagnosisCode>
						</Diagnosis>
					</ExportCrossCheckingDiagnoses>
					<Rejection>
						<RejectionReasonCode>2</RejectionReasonCode>
						<RejectionDateAndTime>2020-03-20T10:20:00</RejectionDateAndTime>
					</Rejection>
					<CEadVal>
						<SequenceNumber>1</SequenceNumber>
						<AdministrativeReferenceCode>22ABZABCDE12345678901</AdministrativeReferenceCode>
					</CEadVal>
				</CustomsRejectionOfEad>
			</Body>
		</ED839C>
	</CustomsData>
</DECustomsData>";

			Factory.Save();

			var serviceTask = new DEMCustomsMessageProcessingServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();
			var factory = new BusinessObjectFactory();
			message = factory.Load<EDIMessage>(message.PK);

			return new BranchMessageProcessorServiceTestHelperData()
			{
				MessagePK = message.PK
			};
		}

		Business.EMCSJobDeclaration declaration;
	}
}
