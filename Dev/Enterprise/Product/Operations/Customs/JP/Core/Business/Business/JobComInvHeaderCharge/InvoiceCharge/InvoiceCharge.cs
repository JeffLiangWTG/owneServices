using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.Business
{
	public partial class InvoiceCharge : AutoInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool ShouldResetDefaultIsIncludedInITOT(ZString incoTerm) => true;

		protected override bool ShouldResetDefaultIsIncludedInAmount(ZString incoTerm, ICustomsChargeCode charge) => true;

		protected override ZBool GetDefaultIsIncludedInITOT(ICustomsChargeCode customsChargeCode)
		{
			var charge = IncoTermAndChargeFactory?.GetCharge(J7_ChargeType);
			var result = charge != null && IncoTermAndChargeFactory.GetDefaultIsIncludedInInvoice(Parent.IncoTerm, charge) && !charge.IsDutiable;
			return result ? result : base.GetDefaultIsIncludedInITOT(customsChargeCode);
		}

		[DecimalPlaces(nameof(InvoiceChargeAmountDecimalPlaces))]
		public override ZDecimal J7_Amount { get => base.J7_Amount; set => base.J7_Amount = value; }

		int InvoiceChargeAmountDecimalPlaces => J7_RX_NKCurrency == CurrencyCodes.Japan ? 0 : 2;
	}
}
