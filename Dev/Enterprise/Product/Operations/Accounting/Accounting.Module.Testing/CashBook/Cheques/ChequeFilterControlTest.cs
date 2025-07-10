using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	sealed class ChequeFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			var cheques = new AccReceivedChequeCollection(Factory);
			var filterBO = new ChequeFilterBusinessObject();

			using (ZForm form = new ZForm())
			{
				var filterControl = new ChequeFilterControl(cheques, filterBO);
				form.Controls.Add(filterControl);
				form.Show();

				Application.DoEvents();
			}
		}

		public void TestColumnsAddedCorrectly()
		{
			AssertColumnsAddedCorrectly(new (string, bool)[] {
				("RCH_ChequeNumber", true),
				("RCH_ChequeReference", true),
				("RCH_DueDate", true),
				("RCH_Amount", true),
				("RCH_RX_NKChequeCurrency", true),
				("RCH_Status", true),
				("RCH_OH_Drawer", true),
				("RCH_OH_ReceivedFrom", true),
				("ReceivedFrom+OH_FullName", false),
				("RCH_OH_GivenTo", true),
				("GivenTo+OH_FullName", false),
				("RCH_BankName", true),
				("RCH_BankName", true)
			});
		}

		void AssertColumnsAddedCorrectly((string ColumnName, bool IsVisible)[] expectedColumns)
		{
			var cheques = new AccReceivedChequeCollection(Factory);
			var filterBO = new ChequeFilterBusinessObject();
			var filterControl = new ChequeFilterControl(cheques, filterBO);

			using var form = new ZForm();
			form.Controls.Add(filterControl);
			form.Show();

			var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>();

			foreach (var expectedColumn in expectedColumns)
			{
				var column = columns.FirstOrDefault(x => x.ColumnName == expectedColumn.ColumnName);

				AssertNotNull(column);
				Assert("Column should be available.", !column.IsUnavailable);
				Assert("New column should not be read only", !column.IsReadOnly);
				AssertEquals($"Column should be {(expectedColumn.IsVisible ? "visible" : "invisible")}", expectedColumn.IsVisible, column.IsVisible);
			}
		}
	}
}
