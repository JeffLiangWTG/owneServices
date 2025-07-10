using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class MiscOptionsLayoutUserControlTest : TestCase
	{
		public void TestPaymentSeparatorUserControl()
		{
			AssertType<SeparatorUserControl>(control.PaymentSeparatorUserControl);
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
