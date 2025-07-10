using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.Testing
{
	[TestedType(typeof(TriggerActionWithOptionalFactorySaveLogSubscriber))]
	class TriggerActionWithOptionalFactorySaveLogSubscriberTransactionAllocationAndPosterCreatorTest : TriggerActionWithOptionalFactorySaveLogSubscriberTest
	{
		protected override string WorkflowTriggerActionType => WorkflowTriggerActionTypeConstants.Codes.TransactionAllocationAndPost;

		protected override void SetupTriggerProcessorMock(IWorkflowProvider provider, IProcessor processor)
		{
			var mock = new Mock<ITransactionAllocationAndPosterCreator>();
			mock.Setup(x => x.CreateTransactionAllocationAndPoster(It.Is<IWorkflowProvider>(y => y.PK == provider.PK))).Returns(processor);
			ObjectFactory.Substitute(mock.Object);
		}

		public void TestGetProcessorInstance()
		{
			var parameters = new QueuedLogParameters();
			parameters.WorkflowProvider = new DummyIWorkflowProvider();

			var processor = TriggerActionWithOptionalFactorySaveLogSubscriber.GetProcessorInstance_ForTestOnly(WorkflowTriggerActionTypeConstants.Codes.TransactionAllocationAndPost, parameters);
			AssertType<TransactionAllocationAndPoster>(processor);
		}
	}
}
