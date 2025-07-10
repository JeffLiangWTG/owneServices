using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class FileDefinitionLineTest : TestCaseWithFactory
	{
		public void TestOutputForNumeric()
		{
			string expectedResult = "字段=借方金额,数值型(15,2)";
			FileDefinitionLine testDefinitionLine = new FileDefinitionLine(FileLineType.Numeric);
			testDefinitionLine.Length = 15;
			testDefinitionLine.Precision = 2;
			testDefinitionLine.Field = "借方金额";
			AssertEquals(expectedResult, testDefinitionLine.ToString());
		}

		public void TestOutputForAlphaNumeric()
		{
			string expectedResult = "字段=科目代码,字符型(13)";
			FileDefinitionLine testDefinitionLine = new FileDefinitionLine(FileLineType.AlphaNumeric);
			testDefinitionLine.Length = 13;
			testDefinitionLine.Precision = 2;
			testDefinitionLine.Field = "科目代码";
			AssertEquals(expectedResult, testDefinitionLine.ToString());
		}

		public void TestOutputForDateType()
		{
			string expectedResult = "字段=凭证日期,日期型";
			FileDefinitionLine testDefinitionLine = new FileDefinitionLine(FileLineType.Date);
			testDefinitionLine.Length = 13;
			testDefinitionLine.Precision = 2;
			testDefinitionLine.Field = "凭证日期";
			AssertEquals(expectedResult, testDefinitionLine.ToString());
		}
	}
}