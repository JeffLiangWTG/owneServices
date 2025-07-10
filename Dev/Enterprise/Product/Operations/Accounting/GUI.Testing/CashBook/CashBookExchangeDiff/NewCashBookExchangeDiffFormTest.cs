using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.GUI.CashBook;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(NewCashBookExchangeDiffForm))]
	public class NewCashBookExchangeDiffFormTest : AccountingZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new NewCashBookExchangeDiffForm(Factory.New<NewCashbookExchangeDiffHeader>());
		}

		[TestDate(2020, 2, 6)]
		public void TestDoDisplayModeBrowse()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var bank = TestObjectCreator.CreateBankAccount("TST", "Test Bank", TestObjectCreator.USD, TestObjectCreator.GLHeader1);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 4.8m, new ZDateTime(2020, 2, 1), new ZDateTime(2020, 2, 29));
			Factory.Save();

			var header = Factory.New<NewCashbookExchangeDiffHeader>();
			var newCashbookExchangeDiff = header.NewCashbookExchangeDiffCollection.AddNew();
			newCashbookExchangeDiff.AH_AB = bank.PK;
			using (var form = new NewCashBookExchangeDiffForm(header))
			{
				form.Show();
				form.DisplayMode = ODisplayMode.New;

				var filterControl = form.FindSingleOrDefault<Control>("NewCashbookExchangeDiffBankAccountFilterControl");
				AssertNotNull("Pre-condition", filterControl);
				var selectAllButton = form.FindSingleOrDefault<ZButton>("SelectAllButton");
				AssertNotNull("Pre-condition", selectAllButton);
				var deselectAllButton = form.FindSingleOrDefault<ZButton>("DeselectAllButton");
				AssertNotNull("Pre-condition", deselectAllButton);
				var postingButtonsUserControl = form.FindSingleOrDefault<ZPostingButtonsUserControl>("PostingButtonsUserControl");
				AssertNotNull("Pre-condition", postingButtonsUserControl);
				var cashbookExchangeDiff = header.NewCashbookExchangeDiffCollection.Cast<NewCashbookExchangeDiff>().FirstOrDefault();
				AssertNotNull("Pre-condition", cashbookExchangeDiff);

				AssertEquals("filter control enabled", true, filterControl.Enabled);
				AssertEquals("SelectAllButton enabled", true, selectAllButton.Enabled);
				AssertEquals("DeselectAllButton enabled", true, deselectAllButton.Enabled);
				AssertEquals("NewCashbookExchangeDiffHeader readonly", false, header.ReadOnly);
				AssertEquals("NewCashbookExchangeDiff readonly", false, cashbookExchangeDiff.ReadOnly);

				cashbookExchangeDiff.AH_AG = TestObjectCreator.GLHeader1.PK;
				postingButtonsUserControl.SaveButton.PerformClick();
				AssertEquals("filter control enabled after save", false, filterControl.Enabled);
				AssertEquals("SelectAllButton enabled after save", false, selectAllButton.Enabled);
				AssertEquals("DeselectAllButton enabled after save", false, deselectAllButton.Enabled);
				AssertEquals("NewCashbookExchangeDiffHeader readonly after save", true, header.ReadOnly);
				AssertEquals("NewCashbookExchangeDiff readonly after save", true, cashbookExchangeDiff.ReadOnly);
			}
		}

		public void TestButtions()
		{
			var header = Factory.New<NewCashbookExchangeDiffHeader>();
			var newCashbookExchangeDiff = header.NewCashbookExchangeDiffCollection.AddNew();
			using (var form = new NewCashBookExchangeDiffForm(header))
			{
				form.Show();
				form.DisplayMode = ODisplayMode.New;

				var filterControl = form.FindSingleOrDefault<Control>("NewCashbookExchangeDiffBankAccountFilterControl");
				AssertNotNull("Pre-condition", filterControl);
				var selectAllButton = form.FindSingleOrDefault<ZButton>("SelectAllButton");
				AssertNotNull("Pre-condition", selectAllButton);
				var deselectAllButton = form.FindSingleOrDefault<ZButton>("DeselectAllButton");
				AssertNotNull("Pre-condition", deselectAllButton);

				AssertEquals("SelectAllButton text", "Select All", selectAllButton.Text);
				AssertEquals("DeselectAllButton text", "Deselect All", deselectAllButton.Text);

				AssertEquals("Pre-condition", 1, header.NewCashbookExchangeDiffCollection.Count);

				AssertEquals("Include shoud be ticked", true, newCashbookExchangeDiff.Include);
				deselectAllButton.PerformClick();
				AssertEquals("Include shoud be unticked after click DeselectAllButton", false, newCashbookExchangeDiff.Include);
				selectAllButton.PerformClick();
				AssertEquals("Include shoud be ticked after click SelectAllButton", true, newCashbookExchangeDiff.Include);
			}
		}

		public void TestControlsVisible()
		{
			var header = Factory.New<NewCashbookExchangeDiffHeader>();
			using (var form = new NewCashBookExchangeDiffForm(header))
			{
				form.Show();
				var columnNameList = new List<string>(new string[] { "Include", "AH_TransactionNum","AH_AB","BankCurrency","BankCurrencyBalance","CurrentExchangeRate","LocalAmountBeforeAdjustment",
"AH_ExchangeRate","LocalAmountAfterAdjustment","ForeignCurrencyGainLoss","BankAccountDescription","AH_NumberOfSupportingDocuments" });
				foreach (var columnName in columnNameList)
				{
					var grid = form.FindSingleOrDefault<ZGrid>("NewCashBookExchangeDiffGrid");
					var columnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == columnName);
					AssertNotNull("Pre-condition", columnInfo);
					AssertEquals($"The column name '{columnInfo.ColumnName}' in grid should be visibility", columnInfo.IsVisible, true);
				}
			}
		}
	}
}
