using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(DocumentWrapperHelper))]
	sealed class DocumentWrapperHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestFormatNumber()
		{
			AssertEquals(new ZDecimal(1111m).FormatNumber(2), "1,111.00");
			AssertEquals(new ZDecimal(1111m).FormatNumber(3), "1,111.000");
			AssertEquals(new ZDecimal(1111m).FormatNumber(0), "1,111");
			AssertEquals(new ZDecimal(1111m).FormatNumber(-1), "1,111");
			AssertEquals(new ZDecimal(5555555555.5555m).FormatNumber(2), "5,555,555,555.56");
			AssertEquals(new ZDecimal(5555555555.5555m).FormatNumber(4), "5,555,555,555.5555");
			AssertEquals(new ZDecimal(5555555555.55m).FormatNumber(5), "5,555,555,555.55000");
			AssertEquals(new ZDecimal(5555555555.5555m).FormatNumber(), "5,555,555,555.5555");
		}

		public void TestFormatBarCode()
		{
			AssertEquals("*CA 99999*", DocumentWrapperHelper.FormatBarCode("CA 99999"));
			AssertEquals("**", DocumentWrapperHelper.FormatBarCode(""));
		}
	}
}
