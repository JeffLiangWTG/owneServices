using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	public class CashAdvanceFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGridSplitter()
		{
			using (ZForm form = new ZForm())
			{
				var filterControl = new CashAdvanceFilterControl(HeaderCollection, FilterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
				filterControl.GridSplitter_SplitterMoved(filterControl.GridSplitter, null);
			}
		}

		public void TestCashAdvanceLineGridIsClearedWhenFindButtonIsClicked()
		{
			using (ZForm form = new ZForm())
			{
				var filterControl = new CashAdvanceFilterControl(HeaderCollection, FilterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();

				var headerGrid = filterControl.FilteredGrid;
				var lineGrid = filterControl.CashAdvanceRequestLineDisplayGrid;

				filterControl.Find();

				AssertEquals(0, headerGrid.CurrentRowIndex);
				AssertEquals(0, lineGrid.CurrentRowIndex);

				ModuleNkFilter filter = (ModuleNkFilter)FilterBO["Currency"];

				filter.Property = Core.Constants.CurrencyCodes.Argentina;
				filter.IsActive = true;

				HeaderCollection.Load(FilterBO.Filter);

				filterControl.Find();

				AssertEquals(-1, headerGrid.CurrentRowIndex);
				AssertEquals(-1, lineGrid.CurrentRowIndex);
			}
		}

		public void TestFirstCashAdvanceHeaderIsSelectedWhenHeaderGridIsLoaded()
		{
			using (ZForm form = new ZForm())
			{
				var filterControl = new CashAdvanceFilterControl(HeaderCollection, FilterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();

				var headerGrid = filterControl.FilteredGrid;
				var lineGrid = filterControl.CashAdvanceRequestLineDisplayGrid;
				headerGrid.CurrentRowIndex = 1;

				AssertEquals(1, headerGrid.CurrentRowIndex);

				filterControl.Find();

				AssertEquals(0, headerGrid.CurrentRowIndex);
				AssertEquals(0, lineGrid.CurrentRowIndex);
			}
		}

		public void TestJobBranchAndJobDeclarationAreVisibleModuleGrid()
		{
			using (ZForm form = new ZForm())
			{
				var filterControl = new CashAdvanceFilterControl(HeaderCollection, FilterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();

				var headerGrid = filterControl.FilteredGrid;
				var lineGrid = filterControl.CashAdvanceRequestLineDisplayGrid;

				var visibleColumnNameList = new List<string>(new string[] { "OrganizationCode", "CAH_RX_NKTransactionCurrency", "CAH_OSAmount", "CAH_OSPaidAmount", "CAH_OSOutstandingAmount", "StatusDescription", "CAH_Printed", "OrganizationName", "CAH_LocalAmount", "CAH_LocalPaidAmount", "CAH_LocalOutstandingAmount", "CAH_RequestReferenceNumber", "JobNumber", "JobDepartment", "JobBranch" });
				var columnStyles = headerGrid.ColumnStyles.Cast<ZGridColumnInfo>();

				foreach (var columnName in visibleColumnNameList)
				{
					var columnInfo = columnStyles.FirstOrDefault(s => s.ColumnName == columnName);
					AssertNotNull($"{columnName}", columnInfo);
					AssertEquals($"The column '{columnInfo.ColumnName}' should be visible", true, columnInfo.IsVisible);
				}
			}
		}

		public void TestPrintedColumn_WhenOpenInAPModule_ShouldNotBeLoaded()
		{
			using (ZForm form = new ZForm())
			{
				var filterControl = new CashAdvanceFilterControl(HeaderCollection, new APCashAdvanceFilterBusinessObject());
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();

				var headerGrid = filterControl.FilteredGrid;
				var lineGrid = filterControl.CashAdvanceRequestLineDisplayGrid;
				var columnStyles = headerGrid.ColumnStyles.Cast<ZGridColumnInfo>();

				var columnInfo = columnStyles.FirstOrDefault(s => s.ColumnName == nameof(AccCashAdvanceRequestHeader.CAH_Printed));
				AssertEquals(true, columnInfo.IsUnavailable);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var testObjectCreator = new TestObjectCreator(Factory);

			var job1 = testObjectCreator.CreateJob("S00001000", null, 0, null, 0);
			var job2 = testObjectCreator.CreateJob("S00001111", null, 0, null, 0);

			var cashAdvanceRequestHeader1 = testObjectCreator.CreateCashAdvanceRequestHeader(job1.PK, testObjectCreator.LocalClient.PK, LedgerTypes.AccountsReceivable, 10m, 10m, testObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			var cashAdvanceRequestHeader2 = testObjectCreator.CreateCashAdvanceRequestHeader(job2.PK, testObjectCreator.LocalClient.PK, LedgerTypes.AccountsReceivable, 100m, 100m, testObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			var cashAdvanceRequestLine1 = testObjectCreator.CreateCashAdvanceRequestLine(cashAdvanceRequestHeader1.PK, cashAdvanceRequestHeader1.CAH_LocalAmount, cashAdvanceRequestHeader1.CAH_OSAmount, cashAdvanceRequestHeader1.CAH_Status);
			var cashAdvanceRequestLine2 = testObjectCreator.CreateCashAdvanceRequestLine(cashAdvanceRequestHeader2.PK, cashAdvanceRequestHeader2.CAH_LocalAmount, cashAdvanceRequestHeader2.CAH_OSAmount, cashAdvanceRequestHeader2.CAH_Status);

			cashAdvanceRequestHeader1.Lines.Add(cashAdvanceRequestLine1);
			cashAdvanceRequestHeader2.Lines.Add(cashAdvanceRequestLine2);

			HeaderCollection = new AccCashAdvanceRequestHeaderCollection(Factory);
			HeaderCollection.Load();
			FilterBO = new ARCashAdvanceFilterBusinessObject();
		}

		protected AccCashAdvanceRequestHeaderCollection HeaderCollection;
		protected CashAdvanceFilterBusinessObject FilterBO;
	}
}
