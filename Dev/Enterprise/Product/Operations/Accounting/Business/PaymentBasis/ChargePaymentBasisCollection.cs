using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Accounting.Business
{
	public class ChargePaymentBasisCollection : DependentBusinessObjectCollection<JobPaymentBasis, JobCharge>
	{
		public ChargePaymentBasisCollection(JobCharge masterCharge) : base(masterCharge)
		{
		}
	}
}