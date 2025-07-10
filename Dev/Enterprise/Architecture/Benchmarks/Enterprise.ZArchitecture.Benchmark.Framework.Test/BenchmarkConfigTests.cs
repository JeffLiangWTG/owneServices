using System;
using Enterprise.ZArchitecture.Benchmark.Framework.TestInterfaces;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Benchmark.Framework.Test
{
	[TestFixture]
	sealed class BenchmarkConfigTests
	{
		[Test]
		public void Defaults()
		{
			var config = new BenchmarkConfig();
			Assert.That(config.TargetTime, Is.EqualTo(TimeSpan.FromSeconds(15)));
			Assert.That(config.MinimumRuns, Is.EqualTo(1024));
			Assert.That(config.ExecutionsPerIteration, Is.EqualTo(1));
			Assert.That(config.WarmUpIterations, Is.EqualTo(32));
			Assert.That(config.SaveResultsToTempFolder, Is.True);
			Assert.That(config.TestContext, Is.TypeOf<NUnit1TestContext>());
			Assert.That(config.Environment, Is.Not.Null);
		}

		[Test]
		public void WithMethods()
		{
			var config = new BenchmarkConfig()
				.WithExecutionsPerIteration(64)
				.WithMinimumRuns(16_384)
				.WithWarmUpIterations(128)
				.WithTargetTime(TimeSpan.FromSeconds(20))
				.WithSaveResultsToTempFolder(false)
				.WithTestContext(new NUnit4TestContext());

			Assert.That(config.TargetTime, Is.EqualTo(TimeSpan.FromSeconds(20)));
			Assert.That(config.MinimumRuns, Is.EqualTo(16_384));
			Assert.That(config.ExecutionsPerIteration, Is.EqualTo(64));
			Assert.That(config.WarmUpIterations, Is.EqualTo(128));
			Assert.That(config.SaveResultsToTempFolder, Is.False);
			Assert.That(config.TestContext, Is.TypeOf<NUnit4TestContext>());
			Assert.That(config.Environment, Is.Not.Null);
		}

		[Test]
		public void WithNUnit1TestContext()
		{
			var config = new BenchmarkConfig().WithNUnit1TestContext();
			Assert.That(config.TestContext, Is.TypeOf<NUnit1TestContext>());
		}

		[Test]
		public void WithNUnit4TestContext()
		{
			var config = new BenchmarkConfig().WithNUnit4TestContext();
			Assert.That(config.TestContext, Is.TypeOf<NUnit4TestContext>());
		}

		[Test]
		public void WithEnvironment()
		{
			var configOnDat = new BenchmarkConfig().WithTestContext(new TestContext_ForTest(isRunningOnDat: true));
			Assert.That(configOnDat.Environment, Is.TypeOf<NullEnvironment>());
			Assert.That(configOnDat.Environment.Username, Is.Empty);

			var configOnLocal = new BenchmarkConfig().WithTestContext(new TestContext_ForTest(isRunningOnDat: false));
			Assert.That(configOnLocal.Environment, Is.TypeOf<LocalEnvironment>());
			Assert.That(configOnLocal.Environment.Username, Is.Not.Empty);

			var specialEnvironment = new Mock<ILocalEnvironment>();
			specialEnvironment.Setup(x => x.Username).Returns("TheSpecialEnvironment");
			var config = new BenchmarkConfig()
							.WithTestContext(new TestContext_ForTest(isRunningOnDat: false))
							.WithEnvironment(specialEnvironment.Object);
			Assert.That(config.Environment.Username, Is.EqualTo("TheSpecialEnvironment"));

			var noDbConfig = new BenchmarkConfig().WithNoDatabaseEnvironment();
			Assert.That(noDbConfig.Environment.Username, Is.Not.Empty);
			Assert.That(noDbConfig.Environment.DatabaseServerName, Is.Null.Or.Empty);
			Assert.That(noDbConfig.Environment.DatabaseFullVersionText, Is.Null.Or.Empty);
		}
	}
}
