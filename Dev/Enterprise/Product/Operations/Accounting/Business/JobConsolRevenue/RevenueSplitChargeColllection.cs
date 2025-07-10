
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.Business.ConsolRevenue
{
	public class RevenueSplitChargeColllection : ActiveBusinessObjectCollection<Charge>
	{
		public RevenueSplitChargeColllection(BusinessObjectFactory factory)
			: base(factory, new AdhocCollectionRelationship(typeof(Charge)))
		{
		}

		protected override bool AllowNew
		{
			get
			{
				return false;
			}
		}
	}
}

