using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class BillImportAction : SailingBillImportAction
	{
		public BillImportAction(AsycudaBill bill)
			: base(bill)
		{
		}

		public new AsycudaBill Bill => base.Bill as AsycudaBill;

		public override ZString BillNumber
		{
			get { return Bill.ABL_BillNumber; }
		}

		protected override bool IsSourceBillOfLadingValid
		{
			get { return base.IsSourceBillOfLadingValid && Bill.Header.BillsOfLadings.Contains(((ISailingSynchronisationTarget<BillOfLading>)Bill).Source); }
		}
	}
}
