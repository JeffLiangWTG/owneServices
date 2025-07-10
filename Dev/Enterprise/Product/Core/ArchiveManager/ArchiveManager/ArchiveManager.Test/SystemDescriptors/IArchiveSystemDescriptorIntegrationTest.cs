namespace Enterprise.ArchiveManager.Test.SystemDescriptors
{
	public interface IArchiveSystemDescriptorIntegrationTest
	{
		void TestArchiveLoggerContainsCorrectStageNames_InOrder();

		void TestArchiveStagesSortByMainDateFilterColumn();

		void TestGeneratedSummaryReport();

		void TestLoggingOfAMUsageData();
	}
}
