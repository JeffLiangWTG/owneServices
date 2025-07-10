using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.GenericTests
{
	public class LanguageCodeTest : TestCase
	{
		public void TestChinaLanguageCodeChangeForReports()
		{
			AssertEquals(@"ZH-CN language code is hard coded in following documents / reports:
 * WAKO Class A Invoice Preprinted
 * Class A Invoice Preprinted
 * Class A Invoice
 * China Balance Sheet
 * Cash Flow Statement China
 * China Posting Report For Bank
 * China Statement of Provision for Impairment of Assets
 * China GL Accounts Balances
 * China Profit Appropriation Statement
 * ChinaVATDetailedReport
 * CN AP Accounting Voucher Report
 * CN AR Accounting Voucher Report
 * China GL Summary
 * China GL Transaction
 * China Trail Balance
 * China Profit and Loss - Yearly
 * China Profit and Loss – Monthly
 * China Statement of Shareholders' Equity
Please have them adjusted once change the China language code.", "ZH-CN", Core.Constants.Languages.ChineseSimplified);
		}
	}
}
