using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.AFR.Business
{
	[CodeProperty(Schema.JPM_BillOfLadingNumber), DescriptionProperty(Schema.JPM_BillOfLadingNumber)]
	public sealed class BLLFunctionBill : AutoBLLFunctionBill
	{
		public BLLFunctionBill(JPAFRBills bill) : base(bill.Factory)
		{
			AFRBill = bill;
			JPM_BillOfLadingNumber = AFRBill.JPB_BillNumber;
		}

		public readonly JPAFRBills AFRBill;
	}
}
