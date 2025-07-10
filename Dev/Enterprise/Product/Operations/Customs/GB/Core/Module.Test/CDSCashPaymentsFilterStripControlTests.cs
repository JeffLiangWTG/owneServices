using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.Module.Testing
{
	class CDSCashPaymentsFilterStripControlTests : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			var payments = new CusEntryPayInfoCollection(Factory);
			var filterBO = new CDSCashPaymentsFilterStripBusinessObject();
			using (var userControl = new CDSCashPaymentsFilterStripControl(payments, filterBO))
			{
				var grid = userControl.FindSingle<ZGrid>("FilteredGrid");
				AssertNotNull("Could not find the grid on the control", grid);

				CombineAssertions(() =>
				{
					AssertColumn(grid, CusEntryPayInfo.Schema.C9_PaymentDate, 95);
					AssertColumn(grid, CusEntryPayInfo.Schema.Importer, 80);
					AssertColumn(grid, CusEntryPayInfo.Schema.ImporterName, 190);
					AssertColumn(grid, CusEntryPayInfo.Schema.C9_PaymentAmount, 110);
					AssertColumn(grid, CusEntryPayInfo.Schema.C9_PaymentReference, 120);
					AssertColumn(grid, CusEntryPayInfo.Schema.C9_IncomingPayResponseNo, 160);
					AssertColumn(grid, CusEntryPayInfo.Schema.C9_TransactionType, 115);
					AssertColumn(grid, CusEntryPayInfo.Schema.TransactionTypeDescription, 165);
					AssertColumn(grid, CusEntryPayInfo.Schema.C9_PaymentStatus, 105);
					AssertColumn(grid, CusEntryPayInfo.Schema.PaymentStatusDescription, 160);
					AssertColumn(grid, CusEntryPayInfo.Schema.MRN, 135);
					AssertColumn(grid, CusEntryPayInfo.Schema.LRN, 170);
					AssertColumn(grid, CusEntryPayInfo.Schema.DeclarationReference, 130);
					AssertColumn(grid, CusEntryPayInfo.Schema.C9_ReceiptDate, 100);
				});
			}
		}

		void AssertColumn(ZGrid grid, string columnName, int expectedWidth)
		{
			var column = grid.GetColumnStyle(columnName);
			AssertNotNull(columnName, column);
			if (column != null)
			{
				AssertEquals($"{columnName}.Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(expectedWidth), column.Width);
			}
		}
	}
}
