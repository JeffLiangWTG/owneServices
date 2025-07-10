namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class NumberToString_ZH_TWTest : NumberToWordsTestCase
	{
		public void TestGetNumberAsString()
		{
			CombineAssertions(delegate
			{
				Assert(0, "零");
				Assert(1, "壹");
				Assert(2, "貳");
				Assert(3, "叄");
				Assert(4, "肆");
				Assert(5, "伍");
				Assert(6, "陸");
				Assert(7, "柒");
				Assert(8, "捌");
				Assert(9, "玖");

				Assert(10, "壹拾");
				Assert(100, "壹佰");
				Assert(1000, "壹仟");
				Assert(10000, "壹萬");
				Assert(100000000, "壹億");
				Assert(100000000000, "壹仟億");

				Assert(11, "壹拾壹");
				Assert(111, "壹佰壹拾壹");
				Assert(12, "壹拾貳");
				Assert(13, "壹拾叄");
				Assert(21, "貳拾壹");
				Assert(32, "叄拾貳");
				Assert(99, "玖拾玖");
				Assert(123, "壹佰貳拾叄");
				Assert(499, "肆佰玖拾玖");
				Assert(1020, "壹仟零貳拾");
				Assert(10020, "壹萬零貳拾");
				Assert(1000000000, "壹拾億");
				Assert(110000, "壹拾壹萬");

				Assert(-1, "負壹");
				Assert(-2, "負貳");
				Assert(-3, "負叄");
				Assert(-4, "負肆");
				Assert(-5, "負伍");
				Assert(-6, "負陸");
				Assert(-7, "負柒");
				Assert(-8, "負捌");
				Assert(-9, "負玖");

				Assert(-10, "負壹拾");
				Assert(-100, "負壹佰");
				Assert(-1000, "負壹仟");
				Assert(-10000, "負壹萬");
				Assert(-100000000, "負壹億");
				Assert(-100000000000, "負壹仟億");

				Assert(-11, "負壹拾壹");
				Assert(-12, "負壹拾貳");
				Assert(-13, "負壹拾叄");
				Assert(-21, "負貳拾壹");
				Assert(-32, "負叄拾貳");
				Assert(-99, "負玖拾玖");
				Assert(-123, "負壹佰貳拾叄");
				Assert(-499, "負肆佰玖拾玖");
				Assert(-1020, "負壹仟零貳拾");
				Assert(-10020, "負壹萬零貳拾");
				Assert(-1000000000, "負壹拾億");
			});
		}

		internal override INumberToWords GetInstance()
		{
			return new NumberToString_ZH_TW();
		}
	}
}
