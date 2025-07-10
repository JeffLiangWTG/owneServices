namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class BalanceSectionTest : FileOutputTest
	{
		protected override void SetValuesBeforeAssert()
		{
		}

		protected override IFileOutput GetOutputToTest()
		{
			return new BalanceSection(Factory);
		}

		protected override string SetExpectedResult()
		{
			return @"[年初余额]
文件名=BALANCE.TXT
字段=科目代码,字符型(13)
字段=年初金额,数值型(15,2)
";
		}
	}
}