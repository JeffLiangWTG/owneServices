using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class NetworkVisualisationErrorReporterTest : TestCase
	{
		public void TestErrorReporter()
		{
			AssertExceptionThrown(typeof(NetworkVisualisationException), () => NetworkVisualisationErrorReporter.ReportError("failing is like bailing on succeeding"));
			AssertExceptionThrown(typeof(NetworkVisualisationException), () => NetworkVisualisationErrorReporter.ReportError());

			AssertNoExceptionThrown(() => NetworkVisualisationErrorReporter.ReportIfAssertionFailed(true));
			AssertExceptionThrown(typeof(NetworkVisualisationException), () => NetworkVisualisationErrorReporter.ReportIfAssertionFailed(false));
		}
	}
}
