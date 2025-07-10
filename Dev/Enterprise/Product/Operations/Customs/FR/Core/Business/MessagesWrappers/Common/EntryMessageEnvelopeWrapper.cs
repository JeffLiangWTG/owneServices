using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class EntryMessageEnvelopeWrapper : MessageEnvelopeWrapper
	{
		public EntryMessageEnvelopeWrapper(CusEntryHeader entryHeader, ZString subType)
		{
			this.itemEntryHeader = Argument.NotNull(entryHeader, "CusEntryHeader cannot be null");
			ZString messageSubType = Argument.NotNullOrEmpty(subType, "Message subtype cannot be null or empty");

			switch (messageSubType)
			{
				case MessageSubTypeList.Codes.IMC:
					this.messageSchema = deltaCSchemaImport;
					this.messageSchemaVersion = deltaCSchemaVersion;
					break;
				case MessageSubTypeList.Codes.EXC:
					this.messageSchema = deltaCSchemaExport;
					this.messageSchemaVersion = deltaCSchemaVersion;
					break;
				case MessageSubTypeList.Codes.IMD:
					this.messageSchema = deltaDSchemaImport;
					this.messageSchemaVersion = deltaDSchemaVersion;
					break;
				case MessageSubTypeList.Codes.EXD:
					this.messageSchema = deltaDSchemaExport;
					this.messageSchemaVersion = deltaDSchemaVersion;
					break;
				case MessageSubTypeList.Codes.DCG:
					this.messageSchema = deltaDcgSchema;
					this.messageSchemaVersion = deltaDSchemaVersion;
					break;
				case MessageSubTypeList.Codes.CIN:
					this.messageSchema = cinSchema;
					this.messageSchemaVersion = ZString.Empty;
					break;
				case MessageTypeList.Codes.COD:
					this.messageSchema = codSchema;
					this.messageSchemaVersion = deltaCSchemaVersion;
					break;
			}
		}

		public override ZString SchemaID => messageSchema;

		public override ZString SchemaVersion => messageSchemaVersion;

		public override ZString PartnerId => CachedValueHelper.GetValue(ref partnerIdCache, () => itemEntryHeader.Declaration.CustomsProfileRelatedAccountRepresentativeID);
		CachedValue<ZString> partnerIdCache;

		public override ZString TransactionId => itemEntryHeader.CorrelationID;

		public override ZShort NumSeq => (ZShort)itemEntryHeader.CH_SequenceNumber;

		protected CusEntryHeader itemEntryHeader;
		protected ZString messageSchema;
		protected ZString messageSchemaVersion;
	}
}
