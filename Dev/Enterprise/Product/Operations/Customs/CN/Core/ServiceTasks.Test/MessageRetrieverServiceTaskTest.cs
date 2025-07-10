using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business.Testing;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.CN.ServiceTasks.Testing
{
	[TestedType(typeof(MessageRetrieverServiceTask))]
	public class MessageRetrieverServiceTaskTest : GMDCustomsMessagingServiceTest<MessageRetrieverServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("60Second", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceRequirementIsApplied()
		{
			var methodInfo = typeof(MessageRetrieverServiceTask).GetMethod(nameof(MessageRetrieverServiceTask.CheckCNSWClientSetting));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			ServiceTaskEnvironmentCheckerTest.TestCheckCNSWClientSetting(Factory, MessageRetrieverServiceTask.CheckCNSWClientSetting);
		}

		protected override MessageRetrieverServiceTask CreateServiceTask()
		{
			return new MessageRetrieverServiceTask();
		}

		protected override GMDCustomsMessagingServiceTestHelperData SetupDataForTesting()
		{
			var newInterchange = CreateInterchangeToTest();
			var result = new GMDCustomsMessagingServiceTestHelperData { InterchangePK = newInterchange.PK };
			return result;
		}

		EDIInterchange CreateInterchangeToTest()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_InterchangeNum = "X0000001";
			interchange.EI_BodyText = messageContent;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow;
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;

			return interchange;
		}

		public void TestServiceAttribute()
		{
			AssertSingleHostedServiceAttribute("CNR", "China Customs Message Retriever", "CNC");
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Codes.CNMessageRetrievingServiceTask,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GenericMessageDelivery),
				};
			}
		}

		protected override void AssertResult(BusinessObjectFactory factory, GMDCustomsMessagingServiceTestHelperData testData, MessageRetrieverServiceTask serviceTask)
		{
			var interchange = factory.Load<EDIInterchange>(testData.InterchangePK);

			AssertEquals("EI_Status", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("ContainedMessages.Count", 1, interchange.ContainedMessages.Count);
			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("EM_ApplicationCode", GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow, createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageNum", "X0000001", createdMessage.EM_MessageNum);
		}

		readonly string messageContent = @"
		<ImportAgrResponse>
			<ResponseInfo>
				<ResponseCode>1</ResponseCode>
				<ResponseMessage>导入成功</ResponseMessage>
				<CopCusCode>3117980008</CopCusCode>
			</ResponseInfo>
			<ConsignNo>20212243495988986</ConsignNo>
		</ImportAgrResponse>";
	}
}
