using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN
{
	public class CIN755SendExpMessageWrapper : ICIN755ExportMessage
	{
		public const string MessageNumber = "755";

		public CIN755SendExpMessageWrapper(DeltaGJobDeclarationMessageSendingObject messageToSend)
		{
			this.messageToSend = Argument.NotNull(messageToSend, "DeltaGJobDeclarationMessageSendingObject cannot be null");
			this.entryHeader = Argument.NotNull(messageToSend.Header, "CustEntryHeader cannot be null");
		}

		public ZString BGMReference => messageToSend.BGMReference;

		public IMessageEnvelope MessageEnvelope => GetMessageEnvelope(entryHeader);

		public ICINNested755Envelope NestedCINMessageEnvelope => GetCINMessageEnvelope(entryHeader);

		public ZDateTime MessageDate
		{
			get
			{
				return ZDateTime.Now;
			}
		}

		public ZString MessageFunctionID => MessageNumber;

		#region Methods
		IMessageEnvelope GetMessageEnvelope(CusEntryHeader cusEntryHeader)
		{
			return new CINExportEnvelopeWrapper(cusEntryHeader, MessageSubTypeList.Codes.CIN755);
		}
		ICINNested755Envelope GetCINMessageEnvelope(CusEntryHeader cusEntryHeader)
		{
			return new CIN755NestedEnvelopeWrapper(cusEntryHeader);
		}
		#endregion

		readonly CusEntryHeader entryHeader;
		readonly DeltaGJobDeclarationMessageSendingObject messageToSend;
	}
}
