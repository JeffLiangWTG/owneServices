using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class InvoiceLinePaymentCountrySpecificUserControlTest : TestCase
	{
		public void TestCommercialPaymentCodeDropEdit()
		{
			AssertType<ZDropEdit>(control.CommercialPaymentCodeDropEdit);
		}

		public void TestPaymentAmountCalcEdit()
		{
			AssertType<ZCalcEdit>(control.PaymentAmountCalcEdit);
		}

		public void TestPaymentNoTextBox()
		{
			AssertType<ZTextBox>(control.PaymentNoTextBox);
		}

		public void TestPaymentDateEdit()
		{
			AssertType<ZDateEdit>(control.PaymentDateEdit);
		}

		InvoiceLinePaymentCountrySpecificUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new InvoiceLinePaymentCountrySpecificUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
