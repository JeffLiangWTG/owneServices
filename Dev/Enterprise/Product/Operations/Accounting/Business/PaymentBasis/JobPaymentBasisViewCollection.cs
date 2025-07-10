using CargoWise.EntityFramework;
using Enterprise.Rating.Business;

namespace Enterprise.Accounting.Business
{
	public class JobPaymentBasisViewCollection : BusinessObjectCollection<JobPaymentBasis>
	{
		public JobPaymentBasisViewCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override bool ReadOnly => true;
		protected override bool AllowNewCore => false;
	}
}
