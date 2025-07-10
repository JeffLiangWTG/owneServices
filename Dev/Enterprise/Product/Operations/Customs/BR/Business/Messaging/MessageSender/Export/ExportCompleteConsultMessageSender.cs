using System.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ExportCompleteConsultMessageSender : BaseConsultMessageSender
	{
		public ExportCompleteConsultMessageSender(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override IMessageSendingObject GetMessageSendingObject() => new ExportDeclarationMessageSendingObject(EntryHeader);

		public override string CanSendMessage
		{
			get
			{
				if (EntryHeader.MovementReferenceNumber.IsEmpty)
				{
					return Res.GetString("B44BFB9F-ED83-4AAA-9B11-0430F82FBDCE", "The selected Entry does not contain a Movement Reference Number.");
				}
				else
				{
					var messages = EntryHeader.Messages;
					var lastCompleteConsultInterchange = messages.Where(x => IsOutgoingCompleteConsultMessage(x)).OrderBy(o => o.EM_SystemCreateTimeUtc)?.LastOrDefault()?.Interchange;

					if (lastCompleteConsultInterchange == null)
					{
						return Res.GetString("B5B591CC-9E56-4AE0-AB72-5AFD9C0B5AA3", "No COM - Complete Consult Message has been detected.");
					}
					else
					{
						return messages.Where(x => IsIncomingErrorMessage(x, lastCompleteConsultInterchange.EI_SessionGUID)).Any()
							? null : Res.GetString("C345347D-DC02-40E2-B63D-441EBD5D3A79", "No XER - Customs Error Message has been detected to re-trigger the request.");
					}
				}
			}
		}

		bool IsOutgoingCompleteConsultMessage(EDIMessage message)
		{
			return message.EM_ApplicationCode == EDIMessage.ApplicationCodes.BRCustoms && message.EM_MessageType == MessageTypeList.Codes.CDE && message.EM_MessageSubType == EDIMessageSubTypeList.Codes.CompleteConsult && message.EM_ReceiveTransmit == EDIInterchange.Direction.Transmit;
		}

		bool IsIncomingErrorMessage(EDIMessage message, ZGuid sessionGuid)
		{
			return message.EM_ApplicationCode == EDIMessage.ApplicationCodes.BRCustoms && message.EM_MessageType == MessageTypeList.Codes.XER && message.EM_ReceiveTransmit == EDIInterchange.Direction.Receive && message.Interchange?.EI_SessionGUID == sessionGuid;
		}
	}
}
