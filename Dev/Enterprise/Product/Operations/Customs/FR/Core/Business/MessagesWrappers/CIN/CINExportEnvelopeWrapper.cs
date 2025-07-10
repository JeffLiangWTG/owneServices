using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN
{
	public class CINExportEnvelopeWrapper : EntryMessageEnvelopeWrapper
	{
		public const string edifact = "EDIFACT";
		public const string xml = "XML";
		public CINExportEnvelopeWrapper(CusEntryHeader entryHeader, ZString subType) : base(entryHeader, subType)
		{
			base.itemEntryHeader = Argument.NotNull(entryHeader, "CustEntryHeader cannot be null");
			this.messageSchema = Argument.NotNullOrEmpty(subType, "Message subtype cannot be null or empty");

			if (subType == MessageSubTypeList.Codes.CIN755)
			{
				this.messageSchemaVersion = edifact;
			}
			else if (subType == MessageSubTypeList.Codes.CIN745)
			{
				this.messageSchemaVersion = xml;
			}
			else
			{
				Argument.NotNullOrEmpty(new ZString(""), "CIN Export message type is unknown");
			}
		}
	}
}
