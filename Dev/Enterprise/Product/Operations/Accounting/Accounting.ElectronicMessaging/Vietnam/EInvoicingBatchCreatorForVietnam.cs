using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public class EInvoicingBatchCreatorForVietnam : EInvoicingDependentBatchCreator
	{
		public EInvoicingBatchCreatorForVietnam(GlbCompany company) : base(company)
		{
		}

		protected override object GetGovernmentAllocatedNumber(ZGuid parentId) => null;
	}
}
