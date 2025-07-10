namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class StatisticsSectionTest : FileOutputTest
	{
		protected override void SetValuesBeforeAssert()
		{
		}

		protected override IFileOutput GetOutputToTest()
		{
			return new StatisticsSection(Factory);
		}

		protected override string SetExpectedResult()
		{
			return @"[统计码]
统计码数=0
";
		}
	}
}