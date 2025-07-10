using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class RefundInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCSI_Quantity()
		{
			var refundInvoiceLine = Factory.New<RefundInvoiceLine>();
			refundInvoiceLine.CSI_Quantity2 = 0;
			refundInvoiceLine.CSI_Quantity = 0;
			AssertNoMessageErrors(refundInvoiceLine.CSI_QuantityInfo);

			refundInvoiceLine.CSI_Quantity = -1;
			AssertHasMessageErrorContaining(refundInvoiceLine.CSI_QuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			refundInvoiceLine.CSI_Quantity = 1;
			AssertHasMessageErrorContaining(refundInvoiceLine.CSI_QuantityInfo, "The refund quantity must be equal to or less than the invoice quantity.");

			refundInvoiceLine.CSI_Quantity2 = 1;
			refundInvoiceLine.CSI_Quantity = 1;
			AssertNoMessageErrors(refundInvoiceLine.CSI_QuantityInfo);
		}

		public void TestCSI_Quantity2()
		{
			var refundInvoiceLine = Factory.New<RefundInvoiceLine>();
			refundInvoiceLine.CSI_Quantity2 = 0;
			AssertNoMessageErrors(refundInvoiceLine.CSI_Quantity2Info);

			refundInvoiceLine.CSI_Quantity2 = -1;
			AssertHasMessageErrorContaining(refundInvoiceLine.CSI_Quantity2Info, MandatoryValidation.ValueCannotBeNegative);

			refundInvoiceLine.CSI_Quantity2 = 1;
			AssertNoMessageErrors(refundInvoiceLine.CSI_Quantity2Info);
		}

		public void TestCSI_Value()
		{
			var refundInvoiceLine = Factory.New<RefundInvoiceLine>();
			refundInvoiceLine.CSI_Value = 0;
			AssertNoMessageErrors(refundInvoiceLine.CSI_ValueInfo);

			refundInvoiceLine.CSI_Value = -1;
			AssertHasMessageErrorContaining(refundInvoiceLine.CSI_ValueInfo, MandatoryValidation.ValueCannotBeNegative);

			refundInvoiceLine.CSI_Value = 1;
			AssertNoMessageErrors(refundInvoiceLine.CSI_ValueInfo);
		}
	}
}
