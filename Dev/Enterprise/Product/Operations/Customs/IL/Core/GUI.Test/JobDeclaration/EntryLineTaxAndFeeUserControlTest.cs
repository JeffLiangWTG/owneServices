using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class EntryLineTaxAndFeeUserControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			using var control = new EntryLineTaxAndFeeUserControl();
			var grid = control.EntryLineDutyAndTaxGrid;

			AssertColumnStyle(grid, CusEntryLineFee.Schema.CF_ChargeType, 80, true, "Type", isCaption: false);
			AssertColumnStyle(grid, Business.CusEntryLineFee.Schema.ChargeTypeDescription, 150, true, "Description", isCaption: false);
			AssertColumnStyle(grid, CusEntryLineFee.Schema.CF_RateOverrideReasonCode, 140, true, "Action", isCaption: false);
			AssertColumnStyle(grid, CusEntryLineFee.Schema.CF_BaseValue, 80, true, "Base Amount", isCaption: false);
			AssertColumnStyle(grid, CusEntryLineFee.Schema.CF_MethodOfCalculation, 150, false, "Method of Calculation", isCaption: false);
			AssertColumnStyle(grid, CusEntryLineFee.Schema.CF_Rate, 80, true, "Tax Rate", isCaption: false);
			AssertColumnStyle(grid, CusEntryLineFee.Schema.CF_ChargeAmount, 80, true, "Total Amount", isCaption: false);
			AssertColumnStyle(grid, CusEntryLineFee.Schema.CF_MethodOfPayment, 120, false, "Method of Payment", isCaption: false);
			AssertColumnStyle(grid, CusEntryLineFee.Schema.CF_IsLandedCostOnly, 160, false, "Is Landed Cost Only", isCaption: false);
		}

		void AssertColumnStyle(ZGrid grid, string columnName, int expectedWidth, bool expectedVisible, string expectedCaption, bool isCaption = false)
		{
			var column = grid.GetColumnStyle(columnName);
			CombineAssertions(columnName, () =>
			{
				AssertNotNull(column);
				AssertEquals(expected: expectedWidth, column.Width);
				AssertEquals(expected: expectedVisible, column.IsVisible);
				AssertEquals(expected: expectedCaption, isCaption ? column.Caption : column.CaptionResourceString.Caption);
			});
		}
	}
}
