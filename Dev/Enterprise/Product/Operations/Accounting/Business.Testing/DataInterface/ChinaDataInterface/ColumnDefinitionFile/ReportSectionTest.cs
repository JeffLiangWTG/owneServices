namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class ReportSectionTest : FileOutputTest
	{
		protected override void SetValuesBeforeAssert()
		{
		}

		protected override IFileOutput GetOutputToTest()
		{
			return new ReportSection(Factory);
		}

		protected override string SetExpectedResult()
		{
			return @"[报表]
报表数=0
";
		}
	}
}