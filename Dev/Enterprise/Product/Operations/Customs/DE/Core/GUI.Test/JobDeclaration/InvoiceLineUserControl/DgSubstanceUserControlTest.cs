using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class DgSubstanceUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
		}

		public void TestDgSubstanceTextBox()
		{
			AssertEquals("BindTo", nameof(JobComInvoiceLine.DgSubstance), control.DgSubstanceTextBox.BindTo);
		}

		public void TestMoreButton()
		{
			AssertEquals("Caption", "More..", control.MoreButton.CaptionResourceString.Caption);
		}

		public void TestMoreButtonClickEvent_Empty()
		{
			AssertNoExceptionThrown("More button click with no handler", () => control.MoreButton.PerformClick());
		}

		public void TestMoreButtonClickEvent_Click()
		{
			var clicked = false;
			control.MoreButtonClick += (s, e) => clicked = true;
			control.MoreButton.PerformClick();
			AssertEquals("More button click handled", true, clicked);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new DgSubstanceUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		DgSubstanceUserControl control;
	}
}
