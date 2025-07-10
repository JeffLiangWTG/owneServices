using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class StatementMessageEnvelopeWrapper : MessageEnvelopeWrapper
	{
		public StatementMessageEnvelopeWrapper(CusStatementHeader statement)
		{
			Argument.NotNull(statement, nameof(statement));
			this.statement = statement;
		}

		public override ZString SchemaID => deltaDcgSchema;

		public override ZString SchemaVersion => deltaDSchemaVersion;

		public override ZString PartnerId => CachedValueHelper.GetValue(ref partnerIdCache, () => statement.DeltaAgreementAccountRepresentativeID);
		CachedValue<ZString> partnerIdCache;

		public override ZString TransactionId => ZString.Empty;

		public override ZShort NumSeq => (ZShort)statement.Messages.Cast<EDIMessage>().Count(x => x.IsTransmitMessage);

		readonly CusStatementHeader statement;
	}
}
