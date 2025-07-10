using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public abstract class MessageEnvelopeWrapper : IMessageEnvelope
	{
		public const string deltaCSchemaImport = "MessageCDecImp";
		public const string deltaCSchemaExport = "MessageCDecExp";
		public const string deltaCSchemaResponseImport = "MessageReponseCDecImp";
		public const string deltaCSchemaResponseExport = "MessageReponseCDecExp";
		public const string deltaDSchemaImport = "MessageDecImp";
		public const string deltaDSchemaExport = "MessageDecExp";
		public const string deltaDSchemaResponseImport = "MessageReponseDecImp";
		public const string deltaDSchemaResponseExport = "MessageReponseDecExp";
		public const string deltaDcgSchema = "MessageDcg";
		public const string deltaDcgSchemaResponse = "MessageReponseDcg";
		public const string cinSchema = "CinMessage";
		public const string ecs507Schema = "MessageReponseIE507";
		public const string ecs618Schema = "MessageReponseIE618";
		public const string ecsEtatSchema = "MessageNotificationEtat";
		public const string deltaCSchemaVersion = "01032013";
		public const string deltaDSchemaVersion = "18122012";
		public const string codSchema = "MessageCOD";

		public abstract ZString SchemaID { get; }

		public abstract ZString SchemaVersion { get; }

		public abstract ZString PartnerId { get; }

		public abstract ZString TransactionId { get; }

		public abstract ZShort NumSeq { get; }
	}
}
