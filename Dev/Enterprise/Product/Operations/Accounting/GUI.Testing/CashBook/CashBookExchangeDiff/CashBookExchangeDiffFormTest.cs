using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.CashBook.Testing
{
	[TestedType(typeof(CashBookExchangeDiffForm))]
	public class CashBookExchangeDiffFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CashBookExchangeDiffForm(Factory.New<CashbookExchangeDiff>());
		}

		public void TestBankCurrencyBalanceDecimalPlaces()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var bank = testObjectCreator.USDBankAccount;
			bank.AB_OpenOSBalance = 202.27m;
			Factory.Save();

			var exDiff = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			exDiff.AH_AB = bank.PK;
			exDiff.BankAccounts.Load();
			AssertEquals("Should be 1 accounts", 1, exDiff.BankAccounts.Count);

			using (var testForm = new CashBookExchangeDiffForm(exDiff))
			{
				testForm.Show();
				AssertEquals("Should respect the bank currency decimal places", bank.AB_OpenOSBalance.DecimalPlaces, testForm.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox_ForTestOnly.Decimals);
			}
		}

		public void TestPostDateAffectsBalanceAmount()
		{
			var creator = new TestObjectCreator(Factory);
			var bank = creator.USDBankAccount;

			var date1 = new ZDateTime(2016, 1, 1);
			var payment1 = creator.CreateAPPayment(0.8m, 80m, date1, date1, creator.AALSHI.PK, bank.PK);

			var date2 = new ZDateTime(2016, 2, 1);
			var payment2 = creator.CreateAPPayment(0.8m, 100m, date2, date2, creator.AALSHI.PK, bank.PK);

			Factory.Save();

			var exDiff = Factory.New<CashbookExchangeDiff>();
			exDiff.AH_AB = bank.PK;
			exDiff.AH_RX_NKTransactionCurrency = "USD";
			exDiff.AH_ExchangeRate = 0.8m;
			var date3 = new ZDateTime(2016, 1, 10);
			exDiff.AH_DueDate = exDiff.AH_PostDate = date3;
			using (var testForm = new CashBookExchangeDiffForm(exDiff))
			{
				testForm.Show();
				var amountOnScreen = testForm.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox_ForTestOnly.Controls.Find("AmountCalcEdit", false).FirstOrDefault() as ZCalcEdit;
				AssertNotNull(amountOnScreen);
				AssertEquals("Should show the amount before post date.", -80m, decimal.Parse(amountOnScreen.Text));

				exDiff.AH_PostDate = ZDateTime.Empty;
				AssertEquals("Should show the amount before post date.", 0m, decimal.Parse(amountOnScreen.Text));

				exDiff.AH_PostDate = date2;
				AssertEquals("Should show the amount before post date.", -180m, decimal.Parse(amountOnScreen.Text));
			}
		}

		public void TestControlVisibilityWhenExDiffCategoryIsNotREA()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var glHeader = testObjectCreator.GLHeader1;
			var exDiff = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			exDiff.AH_AG = glHeader.PK;
			exDiff.AH_ExchangeRate = 1m;
			exDiff.AH_TransactionCategory = "";
			Factory.Save();

			using (var form = new CashBookExchangeDiffForm(exDiff))
			{
				form.Show();
				AssertEquals("Cash Book Currency Exchange Adjustment", form.FormCaption.Trim());
				AssertEquals("Date", form.GetControl<ZDateEdit>("AH_InvoiceDateBoundDateEdit").CaptionResourceString.Caption.Trim());
				AssertEquals(false, form.GetControl<ZGuidFindBox>("AH_AGGuidFindBox").Visible);
				AssertEquals(true, form.GetControl<ZCalcFindBox>("AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox").Visible);
			}
		}
	}
}
