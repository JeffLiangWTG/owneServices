using Enterprise.Customs.Business.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class BatchProcessorEnvironmentCheckerTest : Customs.Business.BatchProcessor.Testing.BaseEnvironmentCheckerTest
	{
		protected override BaseEnvironmentChecker GetEnvironmentChecker() => new BatchProcessorEnvironmentChecker();
	}
}
