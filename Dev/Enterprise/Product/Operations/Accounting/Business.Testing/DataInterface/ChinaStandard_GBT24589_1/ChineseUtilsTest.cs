using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public class ChineseUtilsTest : TestCaseWithFactory
	{
		public void TestGetGLAccountTypeFromNumber()
		{
			AssertEquals("资产类", ChineseUtils.GetGLAccountTypeFromNumber(1));
			AssertEquals("负债类", ChineseUtils.GetGLAccountTypeFromNumber(2, false));
			AssertEquals("所有者权益类", ChineseUtils.GetGLAccountTypeFromNumber(3));
			AssertEquals("共同类", ChineseUtils.GetGLAccountTypeFromNumber(3, false));
			AssertEquals("损益类", ChineseUtils.GetGLAccountTypeFromNumber(5));
			AssertEquals("损益类", ChineseUtils.GetGLAccountTypeFromNumber(6, false));
		}

		public void TestConvertDebitCreditToChinese()
		{
			AssertEquals("贷", ChineseUtils.ConvertDebitCreditToChinese(Constants.DebitCredit.Credit));
			AssertEquals("借", ChineseUtils.ConvertDebitCreditToChinese(Constants.DebitCredit.Debit));
		}

		public void TestGetCurrencyNameForChinese()
		{
			var currencyCNY = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.China));
			var currencyAUD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.Australia));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currencyAUD.PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "澳元");
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currencyCNY.PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "中国人民币元");
			Factory.Save();
			AssertEquals("", ChineseUtils.GetCurrencyNameForChinese(Factory, "RMB"));
			AssertEquals("中国人民币元", ChineseUtils.GetCurrencyNameForChinese(Factory, "CNY"));
			AssertEquals("澳元", ChineseUtils.GetCurrencyNameForChinese(Factory, "AUD"));
		}
	}
}
