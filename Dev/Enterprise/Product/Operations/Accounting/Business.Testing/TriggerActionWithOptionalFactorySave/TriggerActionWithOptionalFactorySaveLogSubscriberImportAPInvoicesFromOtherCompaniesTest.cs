using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.Testing
{
	[TestedType(typeof(TriggerActionWithOptionalFactorySaveLogSubscriber))]
	class TriggerActionWithOptionalFactorySaveLogSubscriberImportAPInvoicesFromOtherCompaniesTest : TriggerActionWithOptionalFactorySaveLogSubscriberTest
	{
		protected override string WorkflowTriggerActionType => WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies;

		protected override void SetupTriggerProcessorMock(IWorkflowProvider provider, IProcessor processor)
		{
			var mock = new Mock<IOtherCompanyAPInvoiceImportCreator>();
			mock.Setup(x =>
					x.CreateOtherCompanyAPInvoiceImport(It.Is<IWorkflowProvider>(y => y.PK == provider.PK), It.IsAny<ZGuid>()))
				.Returns(() => processor);
			ObjectFactory.Substitute(mock.Object);
		}

		public void TestGetProcessorInstance()
		{
			var forwardingConsol = Factory.New<ForwardingConsol>();
			var trigger = Factory.NewWithValidTestData<ProcessTask>();
			var queuedLog = new Mock<IQueuedLog>();
			queuedLog.Setup(x => x.SJ_ParentTableCode).Returns(ProcessTasksSchema.Constants.Prefix);
			queuedLog.Setup(x => x.SJ_ParentID).Returns(trigger.PK);
			queuedLog.Setup(x => x.Factory).Returns(Factory);

			var parameters = new QueuedLogParameters();
			parameters.WorkflowProvider = forwardingConsol;
			parameters.CompanyPK = ZGuid.NewZGuid();

			var processor = TriggerActionWithOptionalFactorySaveLogSubscriber.GetProcessorInstance_ForTestOnly(WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies, parameters);
			AssertType<UnapprovedTransactionImporter>(processor);
		}
	}
}
