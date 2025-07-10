using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1;

namespace Enterprise.Accounting.DataTransfer.DataInterface
{
	public class VoucherKingDeeK3 : Voucher
	{
		public readonly static ZDecimal InvalidExchangeRate = -1;

		public ZDateTime PostDate { get; set; }
		public ZDateTime InvoiceDate { get; set; }
		public ZString AccountingItem { get; set; }
		public ZInt TransactionIndex { get; set; }
		public ZDecimal CalculatedAmount { get; set; }
		public ZString CalculatedCurrencyCode { get; set; }
		public ZDecimal CalculatedExchangeRate { get; set; }
	}
}
