using System.Collections.Generic;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.CA.ServiceTasks.Testing
{
	[TestedType(typeof(InterchangeSenderServiceTask))]
	sealed class InterchangeSenderServiceTaskTest : ServiceTaskTestCase<InterchangeSenderServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"CA Customs ACI messages outbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAACI,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"CA Customs EXP messages outbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAEXP,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"CA Customs IMP messages outbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAIMP,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"CA Customs messages outbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CACustoms,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}
	}
}
