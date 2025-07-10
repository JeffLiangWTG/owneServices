using System;
using Enterprise.Customs.FR.Business.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(EU.NCTS.Business.NctsHeaderWorkflowDescriptor))]
	public class NctsHeaderWorkflowDescriptorTest : EU.NCTS.Business.Testing.NctsHeaderWorkflowDescriptorTest
	{
		protected override Type ExpectedWorkflowTriggerActionType(EU.NCTS.Business.NctsHeader nctsHeader) => typeof(FRSendNCTSMessageProcessor);

		protected override Type ExpectedWorkflowTriggerActionTypeForArrivalNotification(EU.NCTS.Business.NctsHeader nctsHeader) => typeof(FRSendNCTSArrivalNotificationMessageProcessor);
	}
}
