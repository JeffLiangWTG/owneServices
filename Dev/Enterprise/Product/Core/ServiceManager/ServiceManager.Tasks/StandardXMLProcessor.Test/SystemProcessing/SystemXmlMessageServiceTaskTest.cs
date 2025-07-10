using System.Collections.Generic;
using System.Linq;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	[TestedType(typeof(SystemXmlMessageServiceTask))]
	sealed class SystemXmlMessageServiceTaskTest : ServiceTaskTestCase<SystemXmlMessageServiceTask>
	{
		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						SystemMessageList.Descriptions.CustomerServiceResponse,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + SystemMessageList.Codes.CustomerServiceResponse),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						SystemMessageList.Descriptions.ReferenceDataUpdate,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + SystemMessageList.Codes.ReferenceDataUpdate),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						SystemMessageList.Descriptions.TranslationFeedbackEntry,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + SystemMessageList.Codes.TranslationFeedbackEntry),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						SystemMessageList.Descriptions.TranslationFeedbackUpdate,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + SystemMessageList.Codes.TranslationFeedbackUpdate),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						SystemMessageList.Descriptions.LinkTrack,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + SystemMessageList.Codes.LinkTrack)
				};
			}
		}
	}
}
