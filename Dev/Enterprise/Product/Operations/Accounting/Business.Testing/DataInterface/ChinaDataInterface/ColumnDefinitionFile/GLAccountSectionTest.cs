namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class GLAccountSectionTest : FileOutputTest
	{
		protected override void SetValuesBeforeAssert()
		{
		}

		protected override IFileOutput GetOutputToTest()
		{
			return new GLAccountFileSection(Factory);
		}

		protected override string SetExpectedResult()
		{
			return @"[科目]
文件名=ACCOUNT.TXT
科目结构=8,3
分隔符=.
字段=科目代码,字符型(13)
字段=科目名称,字符型(40)
字段=借贷方向,数值型(2,0)
";
		}
	}
}