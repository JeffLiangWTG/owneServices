using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.Business
{
	public partial class InvoiceLineCharge : AutoInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[DecimalPlaces(nameof(InvoiceLineChargeAmountDecimalPlaces))]
		public override ZDecimal J7_Amount { get => base.J7_Amount; set => base.J7_Amount = value; }

		int InvoiceLineChargeAmountDecimalPlaces => J7_RX_NKCurrency == CurrencyCodes.Japan ? 0 : 2;
	}
}
