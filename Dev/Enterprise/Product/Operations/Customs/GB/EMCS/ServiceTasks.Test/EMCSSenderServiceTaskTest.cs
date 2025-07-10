using System.Collections.Generic;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.EMCS.ServiceTasks.Testing
{
	[TestedType(typeof(EMCSSenderServiceTask))]
	public class EMCSSenderServiceTaskTest : ServiceTaskTestCase<EMCSSenderServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"UK Customs EMCS messages outbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCustomsEMCS),
				};
			}
		}
	}
}
