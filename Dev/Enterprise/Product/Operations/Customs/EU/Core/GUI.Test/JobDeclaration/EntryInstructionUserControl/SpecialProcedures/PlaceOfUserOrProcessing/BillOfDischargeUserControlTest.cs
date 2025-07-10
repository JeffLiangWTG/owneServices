using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class BillOfDischargeUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
		}

		public void TestBillOfDischargeGroupBox()
		{
			var groupBox = control.BillOfDischargeGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), groupBox.Location);
				AssertEquals("Caption", "Bill Of Discharge", groupBox.CaptionResourceString.Caption);
			});
		}

		public void TestBillOfDischargeDeadlineCalcEdit()
		{
			var calcEdit = control.BillOfDischargeDeadlineCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>("Type", calcEdit);
				AssertEquals("BindTo", "CustomsEntryInstructions.ZG_BillOfDischargeDeadline", calcEdit.BindTo);
				AssertCollectionContains("Within BillOfDischargeGroupBox", calcEdit, control.BillOfDischargeGroupBox.Controls);
			});
		}

		public void TestBillOfDischargeDetailsTextBox()
		{
			var textBox = control.BillOfDischargeDetailsTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", textBox);
				AssertEquals("BindTo", "CustomsEntryInstructions.BillOfDischargeDetails", textBox.BindTo);
				AssertCollectionContains("Within BillOfDischargeGroupBox", textBox, control.BillOfDischargeGroupBox.Controls);
			});
		}

		public void TestNecessaryCheckBox()
		{
			var checkBox = control.NecessaryCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", checkBox);
				AssertEquals("BindTo", "CustomsEntryInstructions.ZG_BillOfDischargeIsNecessary", checkBox.BindTo);
				AssertCollectionContains("Within BillOfDischargeGroupBox", checkBox, control.BillOfDischargeGroupBox.Controls);
			});
		}

		BillOfDischargeUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new BillOfDischargeUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
