using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class SubAccountsControlTest : TestCaseWithFactory
	{
		public void TestShowSubAccountsControl()
		{
			using (var form = new ZForm())
			{
				var subAccountsControl = GetExceptedSubAcountsControlCore();
				form.Controls.Add(subAccountsControl);
				form.Show();
				var columnNameList = new List<string>(new string[] { "AL1_Calc_SubClassParent", "AL1_SubClassParentId", "AL1_Calc_SubAccountDescription" });
				var columnStyles = subAccountsControl.FindSingleOrDefault<ZGrid>("SubAccountsGrid").ColumnStyles.Cast<ZGridColumnInfo>();
				foreach (var columnName in columnNameList)
				{
					var columnInfo = columnStyles.FirstOrDefault(s => s.ColumnName == columnName);
					AssertNotNull("Pre-condition", columnInfo);
					AssertEquals($"The column name '{columnInfo.ColumnName}' in grid should be visible", columnInfo.IsVisible, true);
				}
				AssertEquals("column count should be 3", 3, columnStyles.Count());
			}
		}

		public void TestSubAccountsProperties()
		{
			using (var subAccountsControl = GetExceptedSubAcountsControlCore())
			{
				AssertEquals(typeof(Business.Base.Transaction.TransactionHeaderWithLines).FullName, subAccountsControl.BindingSource.DataSourceType.FullName);
				AssertEquals(true, subAccountsControl.CaptionRenderingEnabled);
			}
		}

		public void TestSubAccountsGridBindingMember()
		{
			using (var subAccountsControl = GetExceptedSubAcountsControlCore())
			{
				var grid = subAccountsControl.FindSingleOrDefault<ZGrid>("SubAccountsGrid");
				AssertNotNull("Pre-condition", grid);
				AssertEquals("BindingMember", ExceptedSubAccountsGridBindingMemberCore, grid.GetBindingMember());
			}
		}

		protected virtual string ExceptedSubAccountsGridBindingMemberCore => "FilteredLines.SubAccounts";

		protected virtual SubAccountsControl GetExceptedSubAcountsControlCore() => new SubAccountsControl();
	}
}
