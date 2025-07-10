using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class DependentAccTransLinePayCollection : ActiveBusinessObjectCollection<AccTransLinePay>
	{
		public DependentAccTransLinePayCollection(InvoicingLineBase master)
			: base(master)
		{
		}
	}
}