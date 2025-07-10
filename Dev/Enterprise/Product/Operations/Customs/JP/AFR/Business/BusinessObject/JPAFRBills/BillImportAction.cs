using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class BillImportAction : SailingBillImportAction
	{
		public BillImportAction(JPAFRBills bill)
			: base(bill)
		{
			base.IsSelected = !(bill.IsMessagingInProgress || bill.IsBillAlreadyRegistered);
		}

		protected override bool IsSourceBillOfLadingValid
		{
			get { return base.IsSourceBillOfLadingValid && Bill.Header.BillsOfLading.Contains(this.BillOfLadingSource); }
		}

		public new JPAFRBills Bill
		{
			get { return base.Bill as JPAFRBills; }
		}

		public override ZString BillNumber
		{
			get { return Bill.JPB_BillNumber; }
		}

		[CargoWise.ComponentModel.ReadOnlyMember(nameof(IsSelected_ReadOnly))]
		public override ZBool IsSelected
		{
			get { return base.IsSelected; }
			set
			{
				if (!IsSelected_ReadOnly)
				{
					base.IsSelected = value;
				}
			}
		}

		public bool IsSelected_ReadOnly
		{
			get
			{
				return ((Bill.IsMessagingInProgress || Bill.IsBillAlreadyRegistered) && this.Action == ImportAction.Delete);
			}
		}
	}
}
