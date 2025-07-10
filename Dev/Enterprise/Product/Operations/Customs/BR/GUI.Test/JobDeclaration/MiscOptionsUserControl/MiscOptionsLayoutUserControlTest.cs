using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class MiscOptionsLayoutUserControlTest : TestCase
	{
		public void TestBankAccountGuidFindBox()
		{
			AssertType<ZGuidFindBox>(control.BankAccountGuidFindBox);
		}

		public void TestPaymentSeparatorUserControl()
		{
			AssertType<SeparatorUserControl>(control.PaymentSeparatorUserControl);
			AssertEquals("Payment", control.PaymentSeparatorUserControl.CaptionResourceString.Caption);
		}

		MiscOptionsLayoutUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new MiscOptionsLayoutUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
