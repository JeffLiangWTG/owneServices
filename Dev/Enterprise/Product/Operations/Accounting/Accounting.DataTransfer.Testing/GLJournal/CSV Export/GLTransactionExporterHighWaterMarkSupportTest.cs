using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	public class GLTransactionExporterHighWaterMarkSupportTest : TestCaseWithFactory
	{
		public void TestKnownExportersThatHaveHighWaterMarkEnabled()
		{
			//Add to this list if an exporter implements ISupportHighWaterMark and IsHighWaterMarkEnabled returns true
			var knownExporters = new[]
			{
				"Enterprise.Accounting.DataTransfer.GLJournals.GLTransactionExporter"
			};

			var exporter = Exporter as ISupportHighWaterMark;
			var shouldBeHighWaterMarkEnabled = knownExporters.Contains(Exporter.GetType().FullName);

			AssertEquals(string.Format(@"Exporter {0} should{1} be high water mark enabled.
If you have enabled high water mark support on your exporter, add it to the list of known exporters.
If high water mark support shouldn't be enabled, override the IsHighWaterMarkEnabled property so that it returns false.",
				Exporter.GetType().FullName, shouldBeHighWaterMarkEnabled ? "" : " not"),
				shouldBeHighWaterMarkEnabled, exporter != null && exporter.IsHighWaterMarkEnabled);
		}

		protected virtual GLTransactionExporter Exporter
		{
			get
			{
				return new GLTransactionExporter(new GLTransactionBusinessObject(Factory), new NotificationBuffer());
			}
		}
	}
}
