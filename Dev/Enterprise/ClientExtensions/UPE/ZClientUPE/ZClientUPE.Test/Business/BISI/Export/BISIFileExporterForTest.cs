using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	sealed class BISIFileExporterForTest : BISIFileExporter
	{
		public BISIFileExporterForTest(INotifications notify) : base(notify)
		{
		}

		protected override BusinessObjectFactory NewFactory()
		{
			var result = base.NewFactory();
			result.RefreshEnabled = true; // required for testing
			return result;
		}

		internal new INotifications Notifications => base.Notifications;

		internal new void DoExport(StreamWriter writer, int batchNumber, BISIUploadRecordList recordCollection)
			=> base.DoExport(writer, batchNumber, recordCollection);
	}
}
