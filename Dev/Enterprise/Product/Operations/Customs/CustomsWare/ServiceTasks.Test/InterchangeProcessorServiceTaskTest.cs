using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.CustomsWare.ServiceTasks.Testing
{
	[TestedType(typeof(InterchangeProcessorServiceTask))]
	public class InterchangeProcessorServiceTaskTest : ServiceTaskTestCase<InterchangeProcessorServiceTask>
	{
		public void TestServiceAttribute()
		{
			AssertSingleHostedServiceAttribute(ApplicationCodeList.Codes.CustomsWare, "CustomsWare Status Processor", "EUC");
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[] { new TaskNudgeInformationForTest(EDIInterchangeSchema.Constants.TableName, "CustomsWare status interchanges inbound", EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued, EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Messaging.Business.EDIInterchange.Direction.Receive, EDIInterchangeSchema.Constants.EI_IsActive + "=Y", EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + ApplicationCodeList.Codes.CustomsWare) };
			}
		}
	}
}
