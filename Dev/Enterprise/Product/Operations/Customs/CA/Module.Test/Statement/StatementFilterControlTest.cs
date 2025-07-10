using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class StatementFilterControlTest : TestCaseWithFactory
	{
		public void TestFilterControlForARLStatementOfAccountModule()
		{
			using (var module = new ARLStatementOfAccountModule())
			{
				SetupData(true);
				TestFilterControlCore(module, "Statement Total");
			}
		}

		public void TestFilterControlForDailyNoticeReconciliationModule()
		{
			using (var module = new DailyNoticeReconciliationModule())
			{
				SetupData(false);
				TestFilterControlCore(module, "Total Due");
			}
		}

		void TestFilterControlCore(StatementModule module, string label)
		{
			using (var form = new ZChildForm(module.GridCollection))
			{
				var control = (StatementFilterControl)module.EmbeddedControl;
				form.Controls.Add(control);
				form.Show();

				var grid = control.FilteredGrid;
				grid.ResetColumns();

				Assert("StatementFilterControl grid count must have at least " + ExpectedColumnInfosInSortOrderList.Count, ExpectedColumnInfosInSortOrderList.Count <= grid.Columns.Count);

				for (var i = 0; i < ExpectedColumnInfosInSortOrderList.Count; i++)
				{
					var column = grid.Columns[i];
					AssertNotNull(column);

					var columnInfo = ExpectedColumnInfosInSortOrderList[i];
					AssertEquals("Expected", columnInfo.ColumnName, column.ColumnStyle.MappingName);
					AssertEquals("IsVisible", columnInfo.IsVisible, column.IsVisible);
				}

				var statementAmountColumn = grid.GetColumnStyle(CusStatementHeader.Schema.B2_StatementAmount);
				AssertEquals("CaptionResourceString", label, statementAmountColumn.CaptionResourceString.Caption);
			}
		}

		void SetupData(bool isMonthlyStatement)
		{
			expectedColumnInfosInSortOrderList = null;

			if (isMonthlyStatement)
			{
				ExpectedColumnInfosInSortOrderList.Remove((AutoCusStatementHeader.Schema.B2_ProcessDate, true));
			}
			else
			{
				ExpectedColumnInfosInSortOrderList.Remove((AutoCusStatementHeader.Schema.B2_DueDate, true));
			}
		}

		List<(ZString ColumnName, bool IsVisible)> ExpectedColumnInfosInSortOrderList
		{
			get
			{
				if (expectedColumnInfosInSortOrderList == null)
				{
					expectedColumnInfosInSortOrderList = new List<(ZString ColumnName, bool IsVisible)>
					{
						("ARLMessageType", true),
						(AutoCusStatementHeader.Schema.B2_StatementNumber, true),
						(AutoCusStatementHeader.Schema.B2_OH_Importer, true),
						(AutoCusStatementHeader.Schema.B2_ImporterCustomsID, true),
						(AutoCusStatementHeader.Schema.B2_EntryFilerCode, true),
						(AutoCusStatementHeader.Schema.B2_StatementType, true),
						(AutoCusStatementHeader.Schema.B2_ProcessDate, true),
						(AutoCusStatementHeader.Schema.B2_DueDate, true),
						(AutoCusStatementHeader.Schema.B2_PrintDate, true),
						(AutoCusStatementHeader.Schema.B2_PaidAmount, true),
						(AutoCusStatementHeader.Schema.B2_RefundAmount, true),
						(AutoCusStatementHeader.Schema.B2_StatementAmount, true),
						(AutoCusStatementHeader.Schema.B2_SystemCreateUser, false),
						(AutoCusStatementHeader.Schema.B2_SystemCreateBranch, false),
						(AutoCusStatementHeader.Schema.B2_SystemCreateDepartment, false),
						(AutoCusStatementHeader.Schema.B2_SystemCreateTimeUtc, false),
						(AutoCusStatementHeader.Schema.B2_SystemLastEditUser, false),
						(AutoCusStatementHeader.Schema.B2_SystemLastEditTimeUtc, false),
						(CusStatementHeader.Schema.B2_TotalCustomsDuties, false),
						(CusStatementHeader.Schema.B2_TotalSIMA, false),
						(CusStatementHeader.Schema.B2_TotalExciseTax, false),
						(CusStatementHeader.Schema.B2_TotalGST, false),
						(CusStatementHeader.Schema.B2_TotalOthers, false),
						(CusStatementHeader.Schema.B2_TotalInterests, false)
					};
				}
				return expectedColumnInfosInSortOrderList;
			}
		}
		List<(ZString ColumnName, bool IsVisible)> expectedColumnInfosInSortOrderList;
	}
}
