namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class VoucherSectionTest : FileOutputTest
	{
		protected override void SetValuesBeforeAssert()
		{
		}

		protected override IFileOutput GetOutputToTest()
		{
			return new VoucherSection(Factory);
		}

		protected override string SetExpectedResult()
		{
			return @"[凭证]
文件名=VOUCHER.TXT
字段=科目代码,字符型(13)
字段=凭证日期,日期型
字段=借方金额,数值型(15,2)
字段=贷方金额,数值型(15,2)
字段=凭证类型,字符型(6)
字段=凭证编号,字符型(8)
字段=附件,数值型(3,0)
字段=摘要,字符型(10)
";
		}
	}
}