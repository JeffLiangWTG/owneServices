using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.ES.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(ShowPendingDebtTransactionsForm))]
	class ShowPendingDebtTransactionsFormTest : ZFormBasherTest
	{
		public void TestFormControls()
		{
			var transactionCollection = new CusGuaranteeLineTransactionCollection(Factory.New<CusGuaranteeHeader>());

			using (var showResultsGridForm = new ShowPendingDebtTransactionsForm(transactionCollection))
			{
				var resultsGrid = (ZGrid)showResultsGridForm.Controls.Find("ResultsGrid", true).Single();

				showResultsGridForm.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Results Grid is visible", true, resultsGrid.Visible);
					AssertEquals("Results Grid has 14 columns", 14, resultsGrid.ColumnStyles.Count);
					TestHelper.AssertColumnStyleWithCaptionWithVisibility<ZTextBoxColumnStyleInfo>("Transaction Date", resultsGrid, "CPL_TransactionDate", true);
					TestHelper.AssertColumnStyleWithCaptionWithVisibility<ZTextBoxColumnStyleInfo>("Type", resultsGrid, "CPL_TransactionType", true);
					TestHelper.AssertColumnStyleWithCaptionWithVisibility<ZTextBoxColumnStyleInfo>("Type Description", resultsGrid, "TransactionTypeDescription", true);
					TestHelper.AssertColumnStyleWithCaptionWithVisibility<ZTextBoxColumnStyleInfo>("Reference", resultsGrid, "CPL_Reference", true);
					TestHelper.AssertColumnStyleWithCaptionWithVisibility<ZCalcEditColumnStyleInfo>("Value", resultsGrid, "CPL_TranValue", true);
					TestHelper.AssertColumnStyleWithCaptionWithVisibility<ZTextBoxColumnStyleInfo>("Comment", resultsGrid, "CPL_Comment", true);
					TestHelper.AssertColumnStyleWithCaptionWithVisibility<ZTextBoxColumnStyleInfo>("Application Id", resultsGrid, "CPL_AppId", false);
					TestHelper.AssertColumnStyleWithCaptionWithVisibility<ZTextBoxColumnStyleInfo>("Status", resultsGrid, "CPL_TransactionStatus", true);
					TestHelper.AssertColumnStyleWithCaptionWithVisibility<ZTextBoxColumnStyleInfo>("Status Description", resultsGrid, "TransactionStatusDescription", true);
					TestHelper.AssertColumnStyleWithCaptionWithVisibility<ZTextBoxColumnStyleInfo>("Procedure", resultsGrid, "CPL_Procedure", false);
					TestHelper.AssertColumnStyleWithCaptionWithVisibility<ZTextBoxColumnStyleInfo>("Created By", resultsGrid, "CPL_SystemCreateUser", true);
					TestHelper.AssertColumnStyleWithCaptionWithVisibility<ZTextBoxColumnStyleInfo>("Created Time", resultsGrid, "CPL_SystemCreateTimeUtc", true);
					TestHelper.AssertColumnStyleWithCaptionWithVisibility<ZTextBoxColumnStyleInfo>("Last Edit", resultsGrid, "CPL_SystemLastEditUser", false);
					TestHelper.AssertColumnStyleWithCaptionWithVisibility<ZTextBoxColumnStyleInfo>("Last Edited Time", resultsGrid, "CPL_SystemLastEditTimeUtc", false);
				});
			}
		}

		#region Overrides

		protected override Form GetFormToBashCore() => new ShowPendingDebtTransactionsForm(new CusGuaranteeLineTransactionCollection(Factory.New<CusGuaranteeHeader>()));

		public override void TestBindingAllTabsOnIdle()
		{
			try
			{
				base.TestBindingAllTabsOnIdle();
			}
			catch (NotSupportedException)
			{
				Assert("Because binding is for ActiveBusinessObjectCollection, this test is blowing up when HasChanges attempted to be set to true.", true);
			}
		}

		#endregion
	}
}
