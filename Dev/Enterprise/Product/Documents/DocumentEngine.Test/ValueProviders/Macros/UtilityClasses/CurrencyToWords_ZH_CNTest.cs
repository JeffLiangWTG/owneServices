using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWords_ZH_CNTest : TestCaseWithFactory
	{
		public void TestReplacement()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_Desc", currency.PK, "ZH-CN", "RX", "美元");
			Factory.Save();
			CombineAssertions(delegate
			{
				var chs = new CurrencyToWords_ZH_CN();
				AssertEquals("美元叁拾伍元整", chs.ConvertToWords(35, "USD"));
				AssertEquals("美元叁拾伍元伍角整", chs.ConvertToWords(35.50, "USD"));
				AssertEquals("美元叁拾伍元伍角壹分", chs.ConvertToWords(35.51, "USD"));
				AssertEquals("美元壹佰叁拾伍元整", chs.ConvertToWords(135, "USD"));
				AssertEquals("美元壹佰元整", chs.ConvertToWords(100, "USD"));
				AssertEquals("美元伍佰元整", chs.ConvertToWords(500, "USD"));
				AssertEquals("美元壹仟元整", chs.ConvertToWords(1000, "USD"));
				AssertEquals("美元壹仟壹佰元整", chs.ConvertToWords(1100, "USD"));
				AssertEquals("美元壹万元整", chs.ConvertToWords(10000, "USD"));
				AssertEquals("美元壹万零壹元整", chs.ConvertToWords(10001, "USD"));
				AssertEquals("美元陆拾壹万零壹元整", chs.ConvertToWords(610001, "USD"));

				AssertEquals("美元负叁拾伍元整", chs.ConvertToWords(-35, "USD"));
				AssertEquals("美元负叁拾伍元伍角整", chs.ConvertToWords(-35.50, "USD"));
				AssertEquals("美元负叁拾伍元伍角壹分", chs.ConvertToWords(-35.51, "USD"));
				AssertEquals("美元负壹佰叁拾伍元整", chs.ConvertToWords(-135, "USD"));
				AssertEquals("美元负壹佰元整", chs.ConvertToWords(-100, "USD"));
				AssertEquals("美元负伍佰元整", chs.ConvertToWords(-500, "USD"));
				AssertEquals("美元负壹仟元整", chs.ConvertToWords(-1000, "USD"));
				AssertEquals("美元负壹仟壹佰元整", chs.ConvertToWords(-1100, "USD"));
				AssertEquals("美元负壹万元整", chs.ConvertToWords(-10000, "USD"));
				AssertEquals("美元负壹万零壹元整", chs.ConvertToWords(-10001, "USD"));
				AssertEquals("美元负陆拾壹万零壹元整", chs.ConvertToWords(-610001, "USD"));

				AssertEquals("Should not throw for unknown currencies", "XXX叁拾伍元整", chs.ConvertToWords(35, "XXX"));
			});
		}
	}
}
