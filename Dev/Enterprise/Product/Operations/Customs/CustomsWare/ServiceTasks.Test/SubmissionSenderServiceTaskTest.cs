using System.Collections.Generic;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using ApplicationCodeList = Enterprise.Messaging.Integration.ApplicationCodeList;

namespace Enterprise.Customs.CustomsWare.ServiceTasks.Testing
{
	[TestedType(typeof(SubmissionSenderServiceTask))]
	sealed class SubmissionSenderServiceTaskTest : ServiceTaskTestCase<SubmissionSenderServiceTask>
	{
		public void TestServiceAttribute()
		{
			AssertSingleHostedServiceAttribute("CWM", "CustomsWare Submission Processor", "EUC");
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(EDIMessageSchema.Constants.TableName,
						"CustomsWare Submission messages outbound",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationCodeList.Codes.CustomsWare,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.CustomsWare,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
					new TaskNudgeInformationForTest(EDIMessageSchema.Constants.TableName,
						"CustomsWare Submission messages outbound retry",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Pending,
						EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationCodeList.Codes.CustomsWare,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.CustomsWare,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL")
				};
			}
		}
	}
}
