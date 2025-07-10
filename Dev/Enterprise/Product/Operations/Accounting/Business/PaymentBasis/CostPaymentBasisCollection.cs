using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class CostPaymentBasisCollection : DependentBusinessObjectCollection<JobPaymentBasis, JobConsolCost>
	{
		public CostPaymentBasisCollection(JobConsolCost masterConsolCost) : base(masterConsolCost, new ZQuery(JobPaymentBasisSchema.PBS_IsCost, true))
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((JobPaymentBasis)child).PBS_IsCost = true;
		}
	}
}