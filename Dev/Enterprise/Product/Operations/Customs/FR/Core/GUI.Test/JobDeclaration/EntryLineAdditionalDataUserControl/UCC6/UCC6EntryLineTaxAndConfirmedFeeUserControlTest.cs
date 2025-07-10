using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Testing
{
	sealed class UCC6EntryLineTaxAndConfirmedFeeUserControlTest : TestCaseWithFactory
	{
		public void TestEntryLineConfirmedDutyAndTaxGrid_NationalFeeTypeCode()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(control.EntryLineConfirmedDutyAndTaxGrid, CusEntryLineFee.Schema.NationalFeeTypeCode, "National Type", 90);
		}

		public void TestEntryLineDutyAndTaxGrid_NationalFeeTypeCode()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(control.EntryLineCalculatedDutyAndTaxUserControl.EntryLineDutyAndTaxGrid, CusEntryLineFee.Schema.NationalFeeTypeCode, "National Type", 90);
		}

		public void TestEntryLineConfirmedDutyAndTaxGrid_MethodOfPaymentDescription()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(control.EntryLineConfirmedDutyAndTaxGrid, nameof(FR.Business.Declaration.CusEntryLineFee.MethodOfPaymentDescription), "MOP Description", 90);
		}

		public void TestEntryLineDutyAndTaxGrid_MethodOfPaymentDescription()
		{
			AssertColumnStyle<ZTextBoxColumnStyleInfo>(control.EntryLineCalculatedDutyAndTaxUserControl.EntryLineDutyAndTaxGrid, nameof(FR.Business.Declaration.CusEntryLineFee.MethodOfPaymentDescription), "MOP Description", 90);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new UCC6EntryLineTaxAndConfirmedFeeUserControl();
		}
		UCC6EntryLineTaxAndConfirmedFeeUserControl control;

		protected override void TearDown()
		{
			control?.Dispose();
			base.TearDown();
		}

		void AssertColumnStyle<T>(ZGrid grid, string columnName, string caption, int width) where T : ZGridColumnInfo
		{
			var columnStyle = grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);

			CombineAssertions(() =>
			{
				AssertType<T>(columnStyle);
				AssertEquals("Caption", caption, columnStyle.CaptionResourceString.Caption);
				AssertEquals("Width", width, columnStyle.Width);
			});
		}
	}
}
