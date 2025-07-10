using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.ELG
{
	[CodeAlive("Used for dynamic batch export functionality in the ELG module.")]
	class SagBatchExportDirector : SagAccountsExportDirector
	{
		public SagBatchExportDirector(BusinessObjectFactory factory, INotifications notificationSubscriber)
			: base(factory, notificationSubscriber)
		{
		}

		protected override void NotifyFailure()
		{
			string message;
			string batchNumber = ZString.Empty;
			if (Exporter.FilterProvider.CurrentBatchNo == 0)
			{
				message = "Batch wasn't created.";
			}
			else
			{
				message = string.Format("\r\nPlease fix the above errors and then manually export batch {0} at Accounts -> Receivables -> Receivables Transactions -> Actions -> Data Transfer -> Export Transactions in Sage Format\r\n", Exporter.FilterProvider.CurrentBatchNo);
			}
			notifications.Notify(new InfoNotification(message));
			base.NotifyFailure();
		}

		protected override void NotifySuccess()
		{
			notifications.Notify(new InfoNotification(" Exported batch number: " + ARExporter.FilterProvider.CurrentBatchNo));
			notifications.Notify(new InfoNotification(ARExporter.GetMessageToDisplayWhenExportIsFinished()));
			base.NotifySuccess();
		}

		protected override Stream OpenFile(string fileName)
			=> File.Open(fileName, FileMode.Create, FileAccess.ReadWrite);

		protected override ZString GetFinalDirectory()
			=> Directory.Exists(ExportPathName) ? ExportPathName : string.Empty;

		protected override bool EmailNotificationEnabled
		{
			get { return true; }
		}
	}
}
