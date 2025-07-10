using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Accounting.DataTransfer.GLJournals.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.OIA.Business.Testing
{
	public class OIAGLTransactionExporterHighWaterMarkSupportTest : GLTransactionExporterHighWaterMarkSupportTest
	{
		protected override GLTransactionExporter Exporter
		{
			get
			{
				return new OIAGLTransactionExporter(new OIAGLTransactionBusinessObject(Factory), new NotificationBuffer());
			}
		}
	}
}
