using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	class EInvoicingBatchCreatorForTurkeyTestWrapper : EInvoicingBatchCreatorForTurkey
	{
		public EInvoicingBatchCreatorForTurkeyTestWrapper(GlbCompany company) : base(company) { }

		public DynamicBusinessObjectCollection GetAllReadyToRetryStatusPivotsForTesting() => GetAllReadyToRetryStatusPivots();
	}
}
