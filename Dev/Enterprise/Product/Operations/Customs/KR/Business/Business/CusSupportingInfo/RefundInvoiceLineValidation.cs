using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
namespace Enterprise.Customs.KR.Business
{
	public class RefundInvoiceLineValidation : CusSupportingInfoValidation
	{
		public RefundInvoiceLineValidation(RefundInvoiceLine parent)
			: base(parent)
		{
		}
		public new RefundInvoiceLine Parent => (RefundInvoiceLine)base.Parent;

		protected override void CheckCSI_Quantity()
		{
			base.CheckCSI_Quantity();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.CSI_QuantityInfo);
			if (Parent.CSI_Quantity > Parent.CSI_Quantity2)
			{
				Parent.CSI_QuantityInfo.AddMessageError(Res.GetString("1476349F-D7F2-40B9-A30D-DCF3F26608FB", "The refund quantity must be equal to or less than the invoice quantity."));
			}
		}

		protected override void CheckCSI_Quantity2()
		{
			base.CheckCSI_Quantity2();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.CSI_Quantity2Info);
		}

		protected override void CheckCSI_Value()
		{
			base.CheckCSI_Value();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.CSI_ValueInfo);
		}
	}
}
