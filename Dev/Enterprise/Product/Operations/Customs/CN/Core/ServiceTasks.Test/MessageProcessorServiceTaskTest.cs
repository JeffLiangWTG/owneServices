using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.Business.Testing;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.CN.ServiceTasks.Testing
{
	[TestedType(typeof(MessageProcessorServiceTask))]
	public class MessageProcessorServiceTaskTest : BranchMessageProcessorServiceTest<MessageProcessorServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("60Second", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceRequirementIsApplied()
		{
			var methodInfo = typeof(MessageProcessorServiceTask).GetMethod(nameof(MessageProcessorServiceTask.CheckCNSWClientSetting));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			ServiceTaskEnvironmentCheckerTest.TestCheckCNSWClientSetting(Factory, MessageProcessorServiceTask.CheckCNSWClientSetting);
		}

		protected override MessageProcessorServiceTask CreateServiceTask()
		{
			return new MessageProcessorServiceTask();
		}

		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
		{
			var cnOrg = Factory.NewWithValidTestData<GlbCompany>();
			cnOrg.GC_RN_NKCountryCode = "CN";
			var cnBranch = cnOrg.Branches.AddNew();
			cnBranch.GB_Code = "CN1";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_GC = cnOrg.PK;
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var outgoingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "INT00001";
			outgoingInterchange.EI_From = "WTLDCNWZ2";
			outgoingInterchange.EI_To = "CNCustoms";
			var outgoingMessage = Factory.NewWithValidTestData<EDIMessageForTesting>();
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			outgoingMessage.EM_LinkedObject = cusEntryHeader;
			outgoingMessage.EM_ApplicationCode = "CSW";

			var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = "CSW";
			incomingInterchange.EI_InterchangeNum = "INT00001";
			incomingInterchange.EI_To = "WTLDCNWZ2";
			incomingInterchange.EI_From = "CNCustoms";
			var incomingMessage = (EDIMessage)(incomingInterchange.ContainedMessages.FirstOrDefault() ?? incomingInterchange.ContainedMessages.AddNew());
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_ApplicationCode = "CSW";
			incomingMessage.EM_ReceiveTransmit = "RCV";
			incomingMessage.EM_IsActive = true;
			incomingMessage.EM_GB = cnBranch.PK;
			incomingMessage.EM_MessageType = "ACD";

			return new BranchMessageProcessorServiceTestHelperData { MessagePK = incomingMessage.PK };
		}

		public void TestServiceAttribute()
		{
			AssertSingleHostedServiceAttribute("CNP", "China Customs Message Processor", "CNC");
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Codes.CNMessageProcessingServiceTask,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
					)
				};
			}
		}

		protected override void AssertResult(BusinessObjectFactory factory, BranchMessageProcessorServiceTestHelperData testData, MessageProcessorServiceTask serviceTask)
		{
			var message = factory.Load<EDIMessage>(testData.MessagePK);
			AssertEquals("Message should have been processed.", EDIMessage.Status.PreProcessedOK, message.EM_Status);
		}
	}

	class EDIMessageForTesting : EDIMessage
	{
		public EDIMessageForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override string GetMessageReferenceNumber()
		{
			return "MSG001";
		}
	}
}
