namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class NumberToString_ZH_CNTest : NumberToWordsTestCase
	{
		public void TestGetNumberAsString()
		{
			CombineAssertions(delegate
			{
				Assert(0, "零");
				Assert(1, "壹");
				Assert(2, "贰");
				Assert(3, "叁");
				Assert(4, "肆");
				Assert(5, "伍");
				Assert(6, "陆");
				Assert(7, "柒");
				Assert(8, "捌");
				Assert(9, "玖");

				Assert(10, "壹拾");
				Assert(100, "壹佰");
				Assert(1000, "壹仟");
				Assert(10000, "壹万");
				Assert(100000000, "壹亿");
				Assert(100000000000, "壹仟亿");
				Assert(11, "壹拾壹");
				Assert(111, "壹佰壹拾壹");
				Assert(12, "壹拾贰");
				Assert(13, "壹拾叁");
				Assert(21, "贰拾壹");
				Assert(32, "叁拾贰");
				Assert(99, "玖拾玖");
				Assert(123, "壹佰贰拾叁");
				Assert(499, "肆佰玖拾玖");
				Assert(1020, "壹仟零贰拾");
				Assert(10020, "壹万零贰拾");
				Assert(1000000000, "壹拾亿");
				Assert(110000, "壹拾壹万");

				Assert(-1, "负壹");
				Assert(-2, "负贰");
				Assert(-3, "负叁");
				Assert(-4, "负肆");
				Assert(-5, "负伍");
				Assert(-6, "负陆");
				Assert(-7, "负柒");
				Assert(-8, "负捌");
				Assert(-9, "负玖");

				Assert(-10, "负壹拾");
				Assert(-100, "负壹佰");
				Assert(-1000, "负壹仟");
				Assert(-10000, "负壹万");
				Assert(-100000000, "负壹亿");
				Assert(-100000000000, "负壹仟亿");
				Assert(-11, "负壹拾壹");
				Assert(-12, "负壹拾贰");
				Assert(-13, "负壹拾叁");
				Assert(-21, "负贰拾壹");
				Assert(-32, "负叁拾贰");
				Assert(-99, "负玖拾玖");
				Assert(-123, "负壹佰贰拾叁");
				Assert(-499, "负肆佰玖拾玖");
				Assert(-1020, "负壹仟零贰拾");
				Assert(-10020, "负壹万零贰拾");
				Assert(-1000000000, "负壹拾亿");
			});
		}

		internal override INumberToWords GetInstance()
		{
			return new NumberToString_ZH_CN();
		}
	}
}
