using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.COD
{
	class CODMessageEnvelopWrapper : MessageEnvelopeWrapper
	{
		public CODMessageEnvelopWrapper(CusEntryHeader itemEntryHeader)
		{
			Argument.NotNull(itemEntryHeader, nameof(itemEntryHeader));
			this.itemEntryHeader = itemEntryHeader;
		}

		readonly CusEntryHeader itemEntryHeader;

		public override ZString SchemaID => codSchema;

		public override ZString SchemaVersion => deltaDSchemaVersion;

		public override ZString PartnerId => CachedValueHelper.GetValue(ref partnerIdCache, () => itemEntryHeader.Declaration.CustomsProfileRelatedAccountRepresentativeID);
		CachedValue<ZString> partnerIdCache;

		public override ZString TransactionId => itemEntryHeader.CorrelationID;

		public override ZShort NumSeq => (ZShort)itemEntryHeader.Messages.Cast<EDIMessage>().Count(x => x.IsTransmitMessage && x.EM_MessageType == MessageTypeList.Codes.COD);
	}
}
