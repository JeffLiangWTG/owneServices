using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Moq;

namespace Enterprise.Accounting.Business.Testing;

public class RatingProgressReporterTest : TestCaseWithFactory
{
	public void TestIncrementAndReport()
	{
		var mockAdapter1 = new Mock<IAutoRating>();
		var mockAdapter2 = new Mock<IAutoRating>();
		var mockAdapter3 = new Mock<IAutoRating>();

		var mockRatingAdaptersProvider1 = new Mock<IRatingAdaptersProvider>();
		var mockStrategy1 = new Mock<IAutoRatingStrategy>();
		var mockRatingAdaptersProvider2 = new Mock<IRatingAdaptersProvider>();
		var mockStrategy2 = new Mock<IAutoRatingStrategy>();

		var mockAdapterCount = new Mock<IRatingAdapterCountWrapper>();
		mockAdapterCount
			.Setup(x => x.GetRatingAdaptersCount(mockStrategy1.Object, It.IsAny<AutoRateOptions>()))
			.Returns(1);
		mockAdapterCount
			.Setup(x => x.GetAdaptersProviderType(mockStrategy1.Object))
			.Returns(mockRatingAdaptersProvider1.Object.GetType);

		mockAdapterCount
			.Setup(x => x.GetRatingAdaptersCount(mockStrategy2.Object, It.IsAny<AutoRateOptions>()))
			.Returns(2);
		mockAdapterCount
			.Setup(x => x.GetAdaptersProviderType(mockStrategy2.Object))
			.Returns(mockRatingAdaptersProvider2.Object.GetType);

		var testStrategies = new[] { mockStrategy1.Object, mockStrategy2.Object };
		var reporter = new RatingProgressReporter(testStrategies, new TestInteractor(), "", new AutoRateOptions(), mockAdapterCount.Object);

		CombineAssertions("Precondition: right after creating the reporter", () =>
		{
			AssertEquals("Should not initialize the total value", null, reporter.TotalAdapterCountForTest);
			AssertEquals("Should have 0 processed adapters", 0, reporter.AutoratedAdapterCountForTest);
			AssertEquals("Should have 0 error report", 0, ErrorReporter.TotalErrorCount);
		});

		reporter.IncrementAndReport(mockAdapter1.Object);
		CombineAssertions("First increment", () =>
		{
			AssertEquals("Total should count all adapters from all strategies", 3, reporter.TotalAdapterCountForTest);
			AssertEquals("AutoratedAdapterCount should be the processed adapters", 1, reporter.AutoratedAdapterCountForTest);
			AssertEquals("Should have 0 error report", 0, ErrorReporter.TotalErrorCount);
		});

		reporter.IncrementAndReport(mockAdapter2.Object);
		CombineAssertions("Second increment", () =>
		{
			AssertEquals("Total should remain the same", 3, reporter.TotalAdapterCountForTest);
			AssertEquals("AutoratedAdapterCount should be the processed adapters", 2, reporter.AutoratedAdapterCountForTest);
			AssertEquals("Should have 0 error report", 0, ErrorReporter.TotalErrorCount);
		});

		reporter.IncrementAndReport(mockAdapter3.Object);
		CombineAssertions("Third increment", () =>
		{
			AssertEquals("Total should remain the same", 3, reporter.TotalAdapterCountForTest);
			AssertEquals("AutoratedAdapterCount should be the processed adapters", 3, reporter.AutoratedAdapterCountForTest);
			AssertEquals("Should have 0 error report", 0, ErrorReporter.TotalErrorCount);
		});

		reporter.IncrementAndReport(mockAdapter3.Object);
		CombineAssertions("A wrong extra increment", () =>
		{
			AssertEquals("Total should remain the same", 3, reporter.TotalAdapterCountForTest);
			AssertEquals("AutoratedAdapterCount should exceed the total", 4, reporter.AutoratedAdapterCountForTest);
			AssertEquals("Should have an error report for the extra increment", 1, ErrorReporter.TotalErrorCount);

			var expected = @"The number of processed adapters was greater than the total number.
Done: 4
Total: 3";
			AssertContains(expected, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		});
	}
}
