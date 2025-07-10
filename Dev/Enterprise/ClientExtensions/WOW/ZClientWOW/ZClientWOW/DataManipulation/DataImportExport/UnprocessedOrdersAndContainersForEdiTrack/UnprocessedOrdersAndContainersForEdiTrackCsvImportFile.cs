
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Client.Wow
{
	public class UnprocessedOrdersAndContainersForEdiTrackCsvImportFile : CsvImportFileFromMI
	{
		public UnprocessedOrdersAndContainersForEdiTrackCsvImportFile(BusinessObjectFactoryProvider factoryProvider, StreamReader reader)
			: base(factoryProvider, reader)
		{
		}

		public override void BatchDownloadRequiredData(INotifications notify)
		{
		}

		public override bool ShouldSendToEdiTrack
		{
			get { return true; }
		}

		public override void VerifyFileContentValid(INotifications notify)
		{
		}

		public override bool ShouldUpdateBusinessData
		{
			get { return false; }
		}
	}
}
