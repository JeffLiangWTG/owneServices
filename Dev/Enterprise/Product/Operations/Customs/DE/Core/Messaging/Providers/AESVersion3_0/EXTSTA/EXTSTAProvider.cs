using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using CargoWise.Customs.Shared.MessageContracts;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	[CodeAlive("Provider will be used in processor")]
	public class EXTSTAProvider : IEXTSTA
	{
		public EXTSTAProvider(DEXTSE message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly DEXTSE message;

		public string ExitStatus => message.ExportOperation.exitStatus.XmlEnumToString().ValueOrNullIfEmpty();

		public string MovementReferenceNumber => message.ExportOperation.MRN;

		public string LocalReferenceNumber => message.ExportOperation.LRN;

		public string ReferencedMessageIdentifier => message.correlationIdentifier;

		public string MessageIdentifier => message.messageIdentification;
	}
}
