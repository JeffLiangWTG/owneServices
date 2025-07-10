using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CargoControlNumberSynchroniser : BusinessObjectSynchroniser
	{
		public CargoControlNumberSynchroniser(CargoControlNumber destination, CusEntryNumber source)
			: base(destination, source)
		{
		}

		public new CargoControlNumber Destination => (CargoControlNumber)base.Destination;

		public new CusEntryNumber Source => (CusEntryNumber)base.Source;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.CY_CargoControlNumberInfo, Source.CE_EntryNumInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_CU_CCNInfoBillInfo, GetCCNRelatedBill, GetInfosAffectingCCNRelatedBill));
		}

		IZType GetCCNRelatedBill()
		{
			var result = ZGuid.Empty;

			if (Source.Parent is ForwardingShipment shipment)
			{
				var houseBill = shipment.JS_HouseBill;
				var matchedBill = Destination.Parent.Bills.OfType<Bill>().FirstOrDefault(x => x.CU_BillNum == houseBill);
				result = matchedBill?.PK ?? result;
			}

			return result;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingCCNRelatedBill()
		{
			if (Source.Parent is ForwardingShipment shipment)
			{
				yield return shipment.JS_HouseBillInfo;
			}
		}
	}
}
