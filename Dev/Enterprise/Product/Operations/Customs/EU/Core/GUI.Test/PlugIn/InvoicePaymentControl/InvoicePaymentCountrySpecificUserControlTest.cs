using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class InvoicePaymentCountrySpecificUserControlTest : TestCase
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

		InvoicePaymentCountrySpecificUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new InvoicePaymentCountrySpecificUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
