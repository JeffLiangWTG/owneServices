using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.DE.ServiceTasks.Testing
{
	[TestedType(typeof(DEACustomsMessageProcessingServiceTask))]
	sealed class DEACustomsMessageProcessingServiceTaskTest : BranchMessageProcessorServiceTest<DEACustomsMessageProcessingServiceTask>
	{
		public void TestProcessUnknownMessage()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
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
			var serviceTask = new DEACustomsMessageProcessingServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();
			var factory = new BusinessObjectFactory();
			message = factory.Load<EDIMessage>(message.PK);
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
		}

		public void TestHostedServiceBindingAttribute()
		{
			TestHelper.AssertSingleHostedServiceAttribute<DEACustomsMessageProcessingServiceTask>(ServiceTaskApplicationCodeList.Codes.DEAMessageProcessing,
				ServiceTaskApplicationCodeList.Descriptions.DEAMessageProcessing,
				"DEC",
				typeof(DEACustomsMessageProcessingServiceTask),
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
						"DE Atlas Message Processing",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.DECustomsAtlasSystem,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}

		protected override void AssertResult(BusinessObjectFactory factory, BranchMessageProcessorServiceTestHelperData testData, DEACustomsMessageProcessingServiceTask serviceTask)
		{
			var message = factory.Load<AtlasInboundEDIMessage<ICUSTST>>(testData.MessagePK);
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		}

		protected override DEACustomsMessageProcessingServiceTask CreateServiceTask() => new DEACustomsMessageProcessingServiceTask();

		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
		{
			var message = ATLASMessageHelper.CreateAnyKnownATLASMessage(Factory);
			return new BranchMessageProcessorServiceTestHelperData()
			{
				MessagePK = message.PK
			};
		}
	}
}
