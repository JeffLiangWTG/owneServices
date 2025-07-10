using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN
{
	public class CINHeaderEnvelopeWrapper : MessageEnvelopeWrapper
	{
		public const string xml = "XML";
		public const string schemaID = "750";

		public CINHeaderEnvelopeWrapper(CusTempStorageJobHeader header) : base()
		{
			this.header = Argument.NotNull(header, "Job Header cannot be null");
		}

		readonly CusTempStorageJobHeader header;

		public override ZString SchemaID => schemaID;

		public override ZString SchemaVersion => xml;

		public override ZString PartnerId => ZString.Empty;

		public override ZString TransactionId => header.CorrelationID;

		public override ZShort NumSeq => 0;
	}
}
