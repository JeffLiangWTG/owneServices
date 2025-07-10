using CargoWise.Types;

namespace Enterprise.Customs.JP.Business
{
	public class GroupInvoiceChargeValidation : Customs.Business.BaseGroupInvoiceChargeValidation
	{
		public GroupInvoiceChargeValidation(GroupInvoiceCharge groupInvoiceCharge)
			: base(groupInvoiceCharge)
		{
		}

		public GroupInvoiceCharge GroupInvoiceCharge
		{
			get { return Parent; }
		}

		protected new GroupInvoiceCharge Parent
		{
			get { return (GroupInvoiceCharge)base.Parent; }
		}

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();
			var parent = Parent;
			if (parent.J7_RX_NKCurrency != (parent.GroupInvoice?.JZ_RX_NKInvoice_Currency ?? ZString.Empty))
			{
				parent.J7_RX_NKCurrencyInfo.AddMessageError(ValidationConstants.Charge.CurrencyIsDifferentToInvoice);
			}
		}
	}
}
