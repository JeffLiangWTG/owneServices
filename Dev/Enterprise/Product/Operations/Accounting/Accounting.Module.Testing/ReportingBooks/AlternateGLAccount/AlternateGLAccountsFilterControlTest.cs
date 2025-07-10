using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.Module.Testing
{
	internal class AlternateGLAccountsFilterControlTest : TestCaseWithFactory
	{
		public void TestLoadControl()
		{
			var collection = new AlternateGLAccountCombineParentAccountCollection(Factory);
			var filterBO = new AlternateGLAccountsFilterBusinessObject();

			using (var filterControl = new AlternateGLAccountsFilterControl(collection, filterBO))
			{
				filterControl.Show();

				AssetColoumn(filterControl, "ChartCode", "Chart Code");
				AssetColoumn(filterControl, "IsGlobal", "Is Global Chart");
				AssetColoumn(filterControl, "AlternateGLAccount+AGA_AccountType", "Account Type");
				AssetColoumn(filterControl, "AlternateGLAccount+AccountNumWithSeparator", "Alternate Account");
				AssetColoumn(filterControl, "AlternateGLAccount+AGA_Description", "Alternate Account Name");
				AssetColoumn(filterControl, "AlternateGLAccount+AGA_DebitCredit", "DR/CR");
				AssetColoumn(filterControl, "ParentAccount", "Parent Account");
				AssetColoumn(filterControl, "ORGAttribute", "ORG Attribute");
				AssetColoumn(filterControl, "OCGAttribute", "OCG Attribute");
				AssetColoumn(filterControl, "LFEAttribute", "LFE Attribute");
				AssetColoumn(filterControl, "LFOAttribute", "LFO Attribute");
				AssetColoumn(filterControl, "SPRAttribute", "SPR Attribute");
				AssetColoumn(filterControl, "TICAttribute", "TIC Attribute");

				AssetColoumn(filterControl, "AlternateNum", "Alternate Number", false);
				AssetColoumn(filterControl, "PercentNum", "Percent Number", false);
				AssetColoumn(filterControl, "ConsolidationNum", "Consolidate", false);
				AssetColoumn(filterControl, "AlternateGLAccount+CashFlowType", "Cash Flow Type", false);
				AssetColoumn(filterControl, "AlternateGLAccount+StatisticalUnits", "Units", false);
				AssetColoumn(filterControl, "HeaderDependsOnTotal", "Total Reference", false);
				AssetColoumn(filterControl, "AlternateGLAccount+AGA_ReportSection", "Report Section", false);
				AssetColoumn(filterControl, "AlternateGLAccount+PrefixedAccountNum", "Account For Total", false);
				AssetColoumn(filterControl, "AlternateGLAccount+AGA_TotalLevel", "Total Level", false);
				AssetColoumn(filterControl, "AlternateGLAccount+AGA_PrintSequence", "Print Sequence", false);
				AssetColoumn(filterControl, "CreateTime", "Created Time", false);
				AssetColoumn(filterControl, "LastEditTime", "Last Edited Time", false);
				AssetColoumn(filterControl, "AlternateGLAccount+AGA_SystemCreateUser", "Created By", false);
				AssetColoumn(filterControl, "AlternateGLAccount+AGA_SystemLastEditUser", "Last Edit", false);
				AssetColoumn(filterControl, "ParentAccountName", "Parent Account Name", false);
				AssetColoumn(filterControl, "ChartName", "Chart Name", false);
			}

			void AssetColoumn(AlternateGLAccountsFilterControl filterControl, string columnName, string caption, bool isVisible = true)
			{
				var column = filterControl.FilteredGrid.GetColumnStyle(columnName);
				AssertNotNull($"{columnName} is not null", column);
				AssertEquals($"Visible of {columnName}", isVisible, column.IsVisible);
				AssertEquals($"Caption of {columnName}", caption, column.CaptionResourceString.Caption);
			}
		}
	}
}
