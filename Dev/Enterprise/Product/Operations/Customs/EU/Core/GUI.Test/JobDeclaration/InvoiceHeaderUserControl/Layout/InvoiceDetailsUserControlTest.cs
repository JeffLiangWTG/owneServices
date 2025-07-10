using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class InvoiceDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestAgreedPlaceCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.AgreedPlaceCodeFindBox);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals(true, control.CaptionRenderingEnabled);
		}

		public void TestTransportChargesMethodOfPaymentDropEdit()
		{
			AssertType<ZDropEdit>(control.TransportChargesMethodOfPaymentDropEdit);
		}

		public void TestIncotermsAgreedPlaceTextBox()
		{
			CombineAssertions(() =>
			{
				var incoTermsAgreedPlaceControl = control.IncoTermsAgreedPlaceLongTextControl;
				AssertType<LongTextControl>(incoTermsAgreedPlaceControl);
				AssertEquals("BindingMember", nameof(JobComInvoiceHeader.IncoTermsAgreedPlace), incoTermsAgreedPlaceControl.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new InvoiceDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		InvoiceDetailsUserControl control;
	}
}
