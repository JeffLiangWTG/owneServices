using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class MiscOptionsLayoutUserControlTest : TestCase
	{
		public void TestTrainingCheckBox()
		{
			AssertType<ZCheckBox>(control.TrainingCheckBox);
		}

		public void TestShipmentTypeDropEdit()
		{
			AssertType<ZDropEdit>(control.ShipmentTypeDropEdit);
		}

		public void TestRouteFRequestedCheckBox()
		{
			AssertType<ZCheckBox>(control.RouteFRequestedCheckBox);
		}

		public void TestLCPDepartDateEdit()
		{
			AssertType<ZDateEdit>(control.LCPDepartDateEdit);
		}

		public void TestLCPInspectDateEdit()
		{
			AssertType<ZDateEdit>(control.LCPInspectDateEdit);
		}

		public void TestSupportingInformationUserControl()
		{
			AssertType<SupportingInformationControl>(control.SupportingInformationUserControl);
		}

		public void TestPaymentMethodDropEdit()
		{
			AssertType<ZDropEdit>(control.PaymentMethodDropEdit);
		}

		public void TestDeferralSeparatorUserControl()
		{
			AssertType<SeparatorUserControl>(control.DeferralSeparatorUserControl);
		}

		public void TestDeferralSeparatorUserControl_Caption()
		{
			AssertEquals("Deferral", control.DeferralSeparatorUserControl.CaptionResourceString.Caption);
		}

		public void TestRelatedDeclarationsUserControl()
		{
			AssertType<RelatedDeclarationsUserControl>(control.RelatedDeclarationsUserControl);
		}

		public void TestItineraryCountriesUserControl()
		{
			AssertType<ItineraryCountriesUserControl>(control.ItineraryCountriesUserControl);
		}

		public void TestItineraryCountriesSeparatorUserControl()
		{
			AssertType<SeparatorUserControl>(control.ItineraryCountriesSeparatorUserControl);
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
