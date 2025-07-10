using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors
{
	public interface IImportClearanceEmailProvider
	{
		ZString CSVClearance { get; }
		ZString CSVImportCertificate { get; }
	}
}
