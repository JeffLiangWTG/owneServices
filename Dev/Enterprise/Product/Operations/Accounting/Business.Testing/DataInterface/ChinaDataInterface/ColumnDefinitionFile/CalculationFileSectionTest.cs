namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class CalculationFileSectionTest : FileOutputTest
	{
		protected override void SetValuesBeforeAssert()
		{
		}

		protected override IFileOutput GetOutputToTest()
		{
			return new CalculationFileSection(Factory);
		}

		protected override string SetExpectedResult()
		{
			return @"[核算]
文件名=
";
		}
	}
}