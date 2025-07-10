using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ChargeWrapperCollection : GenericWrapperCollection<ChargeWrapper>
	{
		public ChargeWrapperCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public ChargeWrapperCollection(Job job, BusinessObjectFactory factory)
			: base(factory)
		{
			if (job != null)
			{
				foreach (Charge charge in job.Charges)
				{
					Add(new ChargeWrapper(charge, factory));
				}
			}
		}

		public ZBool IsGSTApplicable
		{
			get { return this.Cast<ChargeWrapper>().Any(o => o.OSSell.Tax.Amount > 0); }
		}
	}
}
