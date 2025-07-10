using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public sealed class UnapprovedTransactionImportProcessorCreatorTest : TestCaseWithFactory
	{
		public void TestCreateOtherCompanyAPInvoiceImport()
		{
			var creator = new UnapprovedTransactionImportProcessorCreator();

			var forwardingConsol = Factory.New<ForwardingConsol>();
			var queuedLog = new Mock<IQueuedLog>();
			queuedLog.Setup(x => x.SJ_ParentTableCode).Returns("Invalid");
			var triggerActionProcessor = creator.CreateOtherCompanyAPInvoiceImport(forwardingConsol, ZGuid.Empty);
			AssertNull(triggerActionProcessor);

			var trigger = Factory.NewWithValidTestData<ProcessTask>();
			queuedLog.Setup(x => x.SJ_ParentTableCode).Returns(ProcessTasksSchema.Constants.Prefix);
			queuedLog.Setup(x => x.SJ_ParentID).Returns(trigger.PK);
			queuedLog.Setup(x => x.Factory).Returns(Factory);
			triggerActionProcessor = creator.CreateOtherCompanyAPInvoiceImport(null, ZGuid.Empty);
			AssertNull(triggerActionProcessor);

			triggerActionProcessor = creator.CreateOtherCompanyAPInvoiceImport(forwardingConsol, ZGuid.NewZGuid());
			AssertType<UnapprovedTransactionImporter>(triggerActionProcessor);
		}
	}
}
