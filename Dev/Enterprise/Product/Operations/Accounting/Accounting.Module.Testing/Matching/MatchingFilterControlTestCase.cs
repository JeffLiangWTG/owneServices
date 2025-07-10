using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class MatchingFilterControlTestCase : TestCaseWithFactory
	{
		protected MatchingFilterControl fMatchingFilterControl;
		protected UnmatchingRowCollection fUnmatchingRows;
		protected MatchingBaseFilterBusinessObject fFilterBizO;

		protected abstract MatchingBaseFilterBusinessObject GetTestFilterBizO();

		protected override void SetUp()
		{
			base.SetUp();
			fUnmatchingRows = new UnmatchingRowCollection(Factory);
			fFilterBizO = GetTestFilterBizO();
		}

		#region TestIsCurrentSelectionValid

		public void TestIsCurrentSelectionValid()
		{
			using (fMatchingFilterControl = new MatchingFilterControl(fUnmatchingRows, fFilterBizO))
			{
				fMatchingFilterControl.OnLoad_Exposed();
				AssertEquals("Pre-condition: no current row", -1, fMatchingFilterControl.FilteredGrid.CurrentRowIndex);
				Assert("No rows selected - current selection should not be valid", !fMatchingFilterControl.IsCurrentSelectionValid);
				UnmatchingRow unmatchRow1 = fUnmatchingRows.AddNew();
				UnmatchingRow unmatchRow2 = fUnmatchingRows.AddNew();

				fMatchingFilterControl.FilteredGrid.CurrentRowIndex = 0;
				AssertNotNull("Row 1 selected - current selection should be valid", fMatchingFilterControl.IsCurrentSelectionValid);
			}
		}

		#endregion

		#region TestResetTransactionHeadersForSelectedMatchGroup

		public void TestResetTransactionHeadersForSelectedMatchGroup()
		{
			ARInvoice testARInv = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			APInvoice testAPInv = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;

			TransactionMatchLink aRInvMatch = ((IMatching)testAPInv).CurrentMatchGroup.AddNew();
			aRInvMatch.AP_AH = testARInv.PK;
			aRInvMatch.AP_MatchGroupNum = "M00002390";

			TransactionMatchLink aPInvMatch = ((IMatching)testAPInv).CurrentMatchGroup.AddNew();
			aPInvMatch.AP_AH = testAPInv.PK;
			aPInvMatch.AP_MatchGroupNum = "M00002390";
			TestObjectCreator.SetupMatchLinkMatchDate(testAPInv);

			UnmatchingRow unmatchRow1 = fUnmatchingRows.AddNew();
			unmatchRow1.MatchGroupNum = "M00002390";

			ARInvoice testARInv2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			APInvoice testAPInv2 = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;

			TransactionMatchLink aRInv2Match = ((IMatching)testARInv2).CurrentMatchGroup.AddNew();
			aRInv2Match.AP_AH = testARInv2.PK;
			aRInv2Match.AP_MatchGroupNum = "M00001298";

			TransactionMatchLink aPInv2Match = ((IMatching)testARInv2).CurrentMatchGroup.AddNew();
			aPInv2Match.AP_AH = testAPInv2.PK;
			aPInv2Match.AP_MatchGroupNum = "M00001298";
			TestObjectCreator.SetupMatchLinkMatchDate(testARInv2);

			UnmatchingRow unmatchRow2 = fUnmatchingRows.AddNew();
			unmatchRow2.MatchGroupNum = "M00001298";

			Factory.Save();

			fUnmatchingRows.Sort(UnmatchingRow.Schema.MatchGroupNum, System.ComponentModel.ListSortDirection.Ascending);

			using (var form = new ZForm())
			using (fMatchingFilterControl = new MatchingFilterControl(fUnmatchingRows, fFilterBizO))
			{
				form.Controls.Add(fMatchingFilterControl);
				form.Show();
				fMatchingFilterControl.OnLoad_Exposed();
				AssertEquals("There should be 2 elements in the list", 2, fMatchingFilterControl.FilteredGrid.List.Count);
				AssertNull("Previous matchgroup should be null", fMatchingFilterControl.fPreviousMatchGroup);
				fMatchingFilterControl.FilteredGrid.CurrentRowIndex = 1;
				fMatchingFilterControl.ResetTransactionHeadersForSelectedMatchGroup();
				AssertEquals("Previous matchgroup should be the 2nd element in the list, since index 1 was selected", fUnmatchingRows[1], fMatchingFilterControl.fPreviousMatchGroup);
				AssertEquals("There should be 2 elements in the MatchedTransactionsList", 2, fFilterBizO.TransactionHeaders.Count);
				Assert("MatchedTransactionsList should contain ARInv", fFilterBizO.TransactionHeaders.Contains(testARInv));
				Assert("MatchedTransactionsList should contain APInv", fFilterBizO.TransactionHeaders.Contains(testAPInv));

				fMatchingFilterControl.FilteredGrid.CurrentRowIndex = 0;
				fMatchingFilterControl.ResetTransactionHeadersForSelectedMatchGroup();
				AssertEquals("Previous matchgroup should be the 1st slement in the list, since index 0 was selected", fUnmatchingRows[0], fMatchingFilterControl.fPreviousMatchGroup);
				AssertEquals("There should be 2 elements in the MatchedTransactionsList", 2, fFilterBizO.TransactionHeaders.Count);
				Assert("MatchedTransactionsList should contain ARInv2", fFilterBizO.TransactionHeaders.Contains(testARInv2));
				Assert("MatchedTransactionList should contain APInv2", fFilterBizO.TransactionHeaders.Contains(testAPInv2));
				AssertNotNull("MatchTransactionsDisplayGrid.DataSource", fMatchingFilterControl.MatchTransactionsDisplayGrid.DataSource);
				fMatchingFilterControl.MatchTransactionsDisplayGrid.SelectAllElements();
				AssertEquals("Should be two elements in the MatchTransactionsDisplayGrid", 2, fMatchingFilterControl.MatchTransactionsDisplayGrid.SelectedElements.GetLength(0));
			}
		}

		#endregion
	}
}
