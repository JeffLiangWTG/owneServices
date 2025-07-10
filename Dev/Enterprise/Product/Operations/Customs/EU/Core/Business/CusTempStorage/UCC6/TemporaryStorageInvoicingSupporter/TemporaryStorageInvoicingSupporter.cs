using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	sealed class TemporaryStorageInvoicingSupporter : JobInvoicingSupporter
	{
		public TemporaryStorageInvoicingSupporter(TemporaryStorageHeader parent) : base(parent)
		{
			this.parent = parent;
		}

		readonly TemporaryStorageHeader parent;

		public override bool CreateAccountingJobOnSavingOfOperationsJob => !parent.IsInDatabaseIncludingChildren;

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore() => Env.Security.CustomsTemporaryStorageJobInvoicing;

		public override JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.CustomsTemporaryStorage;

		public override ZGuid OverriddenDepartmentPK => ObjectFactory.Get<IAccounting>().CustomsExWarehouse;
	}
}
