using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	internal class AccGeneralLedgerDataFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			var accGeneralLedgerDatas = new AccGeneralLedgerDataCollection(Factory);
			var filterBO = new AccGeneralLedgerDataFilterBusinessObject();

			using (var form = new ZForm())
			{
				var filterControl = new AccGeneralLedgerDataFilterControl(accGeneralLedgerDatas, filterBO);
				form.Controls.Add(filterControl);
				form.Show();

				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_GC_Company").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_GB_Branch").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_GB_TaxBranch").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_GE_Department").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_PostDate").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_PostPeriod").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_AG_GLAccount").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_OSDebitAmount").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_OSCreditAmount").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_LocalDebitAmount").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_LocalCreditAmount").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "OrganizationPK").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_Currency").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_ExchangeRate").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_Type").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "Ledger").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "TransactionNumber").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "DueDate").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "TransactionDate").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "JobHeaderPK").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "ChargeCodePK").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLAccountDesc").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "SubAccount").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "HeaderDescription").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "LineDescription").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "PresentationJournalCategory").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "Units").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "GLD_JournalEntriesNumber").IsVisible);
			}
		}
	}
}
