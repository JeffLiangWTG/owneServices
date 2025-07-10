using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	public class ProfitShareShipmentDetailCollection : NonPersistentBusinessObjectCollection<ProfitShareShipmentDetail>
	{
		public ProfitShareShipmentDetailCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Contains Shipment

		public bool ContainsShipmentAndProfitShareCharges(ZGuid shipmentPK)
		{
			return this.Cast<ProfitShareShipmentDetail>().Any(x => x.Parent.PK == shipmentPK && x.ProfitShareCharges.Any());
		}

		#endregion

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ProfitShareShipmentDetail();
		}

		#endregion
	}
}
