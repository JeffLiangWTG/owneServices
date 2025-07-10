using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors
{
	public class ImportClearanceEmailObject : IImportClearanceEmailProvider
	{
		public ImportClearanceEmailObject(ZString csvClearance, ZString csvImportCertificate)
		{
			CSVClearance = csvClearance;
			CSVImportCertificate = csvImportCertificate;
		}

		public ZString CSVClearance { get; }
		public ZString CSVImportCertificate { get; }
	}
}
