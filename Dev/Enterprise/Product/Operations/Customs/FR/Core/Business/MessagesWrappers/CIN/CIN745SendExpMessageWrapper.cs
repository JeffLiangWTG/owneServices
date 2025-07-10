using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN
{
	public class CIN745SendExpMessageWrapper : ICIN745ExportMessage
	{
		public const string Message745 = "745";

		public CIN745SendExpMessageWrapper(DeltaGJobDeclarationMessageSendingObject messageToSend)
		{
			this.messageToSend = Argument.NotNull(messageToSend, "DeltaGJobDeclarationMessageSendingObject cannot be null");
			this.entryHeader = Argument.NotNull(messageToSend.Header, "CustEntryHeader cannot be null");
		}

		public IMessageEnvelope MessageEnvelope => GetMessageEnvelope(entryHeader);

		public ICINNested745Message MessageCIN745 => GetMessage745();

		public ZString BGMReference => messageToSend.BGMReference;

		public ZDateTime MessageDate
		{
			get
			{
				return ZDateTime.Now;
			}
		}

		public ZString MessageFunctionID => Message745;

		#region Methods
		ICINNested745Message GetMessage745()
		{
			return new CIN745NestedEnvelopeWrapper(entryHeader);
		}
		IMessageEnvelope GetMessageEnvelope(CusEntryHeader cusEntryHeader)
		{
			return new CINExportEnvelopeWrapper(cusEntryHeader, MessageSubTypeList.Codes.CIN745);
		}

		#endregion

		readonly CusEntryHeader entryHeader;
		readonly DeltaGJobDeclarationMessageSendingObject messageToSend;
	}
}
