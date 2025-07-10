using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class CommonEntryDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestNoPacksCalcEdit()
		{
			AssertType<ZCalcEdit>(control.NoPacksCalcEdit);
		}

		public void TestGrossWeightCalcEdit()
		{
			AssertType<ZCalcDropEdit>("Check correct type zCalcDropEdit", control.GrossWeightCalcDropEdit);
		}

		public void TestNetWeightCalcEdit()
		{
			AssertType<ZCalcDropEdit>("Check correct type zCalcDropEdit", control.NetWeightCalcDropEdit);
		}

		public void TestCustomsQuantityCalcEdit()
		{
			AssertType<ZCalcDropEdit>("Check correct type zCalcDropEdit", control.CustomsQuantityCalcDropEdit);
		}

		public void TestDutyCalcEdit()
		{
			AssertType<ZCalcEdit>(control.DutyCalcEdit);
		}

		public void TestVatCalcEdit()
		{
			AssertType<ZCalcEdit>(control.VatCalcEdit);
		}

		public void TestEntryLinesCountCalcEdit()
		{
			AssertType<ZCalcEdit>(control.EntryLinesCountCalcEdit);
		}

		public void TestTotalsLabel()
		{
			var totalsLabel = control.TotalsLabel;
			AssertType<ZLabel>(totalsLabel);
			AssertEquals("Caption", "==== Totals ====", totalsLabel.CaptionResourceString.Caption);
		}

		public void TestCustomsLabel()
		{
			var customsLabel = control.CustomsLabel;
			AssertType<ZLabel>(customsLabel);
			AssertEquals("Caption", "==== Customs ====", customsLabel.CaptionResourceString.Caption);
		}

		public void TestReferenceNumberTextBox()
		{
			AssertType<ZTextBox>(control.ReferenceNumberTextBox);
		}

		public void TestSubmittedDateDateEdit()
		{
			AssertType<ZDateEdit>(control.SubmittedDateDateEdit);
		}

		public void TestMRNTextBox()
		{
			AssertType<ZTextBox>(control.MRNTextBox);
		}

		public void TestReleaseDateDateEdit()
		{
			AssertType<ZDateEdit>(control.ReleaseDateDateEdit);
		}

		public void TestEntryStatusDropEdit()
		{
			AssertType<ZDropEdit>(control.EntryStatusDropEdit);
		}

		public void TestAcceptanceDateDateEdit()
		{
			AssertType<ZDateEdit>(control.AcceptanceDateDateEdit);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new CommonEntryDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		CommonEntryDetailsUserControl control;
	}
}
