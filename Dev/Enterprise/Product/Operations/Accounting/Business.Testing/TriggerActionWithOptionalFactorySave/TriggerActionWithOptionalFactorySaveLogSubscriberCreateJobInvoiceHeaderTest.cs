using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.Testing
{
	[TestedType(typeof(TriggerActionWithOptionalFactorySaveLogSubscriber))]
	class TriggerActionWithOptionalFactorySaveLogSubscriberCreateJobInvoiceHeaderTest : TriggerActionWithOptionalFactorySaveLogSubscriberTest
	{
		protected override string WorkflowTriggerActionType => WorkflowTriggerActionTypeConstants.Codes.CreateJobInvoiceHeader;

		protected override void SetupTriggerProcessorMock(IWorkflowProvider provider, IProcessor processor)
		{
			var mock = new Mock<IJobInvoiceHeaderProcessorCreator>();
			mock.Setup(x => x.CreateJobInvoiceHeaderProcessor(It.Is<IWorkflowProvider>(p => p.PK == provider.PK))).Returns(processor);
			ObjectFactory.Substitute(mock.Object);
		}
	}
}

