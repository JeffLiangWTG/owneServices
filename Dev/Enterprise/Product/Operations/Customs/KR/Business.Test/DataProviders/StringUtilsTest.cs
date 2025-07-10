using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business.Testing
{
	class StringUtilsTest : TestCaseWithFactory
	{
		public void TestStringTypeTruncate()
		{
			ZString aa = "홍길동이다 한국이름";
			var b = StringUtils.GetSubstringBytes(aa, 12);
			AssertEquals("홍길동이다 ", b);

			aa = "홍길동A이다 한국이름";
			b = StringUtils.GetSubstringBytes(aa, 12);
			AssertEquals("홍길동A이다 ", b);

			aa = "ABCDEFA이다 한국이름";
			b = StringUtils.GetSubstringBytes(aa, 12);
			AssertEquals("ABCDEFA이다 ", b);

			aa = "한국이름";
			b = StringUtils.GetSubstringBytes(aa, 12);
			AssertEquals("한국이름", b);

			aa = @"홍길동
이다 한국이름";
			b = StringUtils.GetSubstringBytes(aa, 12);
			AssertEquals("홍길동\r\n이다", b);
		}
	}
}
