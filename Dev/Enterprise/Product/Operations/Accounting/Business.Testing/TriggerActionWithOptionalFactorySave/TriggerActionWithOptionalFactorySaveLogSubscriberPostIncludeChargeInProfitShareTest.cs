using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.Testing
{
	[TestedType(typeof(TriggerActionWithOptionalFactorySaveLogSubscriber))]
	class TriggerActionWithOptionalFactorySaveLogSubscriberPostIncludeChargeInProfitShareTest : TriggerActionWithOptionalFactorySaveLogSubscriberTest
	{
		protected override string WorkflowTriggerActionType => WorkflowTriggerActionTypeConstants.Codes.IncludeChargeInProfitShare;

		protected override void SetupTriggerProcessorMock(IWorkflowProvider provider, IProcessor processor)
		{
			var mock = new Mock<IIncludeChargeInProfitShareProcessorCreator>();
			mock.Setup(x => x.CreateIncludeChargeInProfitShareProcessor(It.Is<IWorkflowProvider>(p => p.PK == provider.PK))).Returns(processor);
			ObjectFactory.Substitute(mock.Object);
		}
	}
}
