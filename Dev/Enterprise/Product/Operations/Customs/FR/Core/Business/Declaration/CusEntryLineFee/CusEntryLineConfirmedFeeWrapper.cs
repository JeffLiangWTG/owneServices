using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryLineConfirmedFeeWrapper : EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper
	{
		public CusEntryLineConfirmedFeeWrapper(CusEntryLineFee cusEntryLineFee) : base(cusEntryLineFee)
		{
			this.cusEntryLineFee = cusEntryLineFee;
		}
		readonly CusEntryLineFee cusEntryLineFee;

		public ZString MethodOfPaymentDescription => cusEntryLineFee.MethodOfPaymentDescription;
	}
}
