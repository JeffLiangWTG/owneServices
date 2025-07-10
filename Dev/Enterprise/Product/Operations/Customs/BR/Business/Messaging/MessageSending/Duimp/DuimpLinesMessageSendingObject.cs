using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.Duimp;

namespace Enterprise.Customs.BR.Business
{
	public class DuimpLinesMessageSendingObject : IMessageSendingObject
	{
		public DuimpLinesMessageSendingObject(IEnumerable<CusEntryLine> entryLines, ZString messageType)
		{
			EntryLines = CargoWise.Common.Argument.NotNull(entryLines, nameof(entryLines)).ToArray();
			Header = EntryLines[0].Header;

			MessageType = messageType;
		}

		public CusEntryHeader Header { get; }
		public IReadOnlyList<CusEntryLine> EntryLines { get; }

		public ZString MessageType { get; set; }

		public BusinessObjectFactory Factory => Header.Factory;

		public BusinessObject MessageAttachee => Header;

		public ZString GetApplicationReference()
		{
			return ZString.Join("|", GetReferences().ToArray());

			IEnumerable<ZString> GetReferences()
			{
				yield return Header.MovementReferenceNumber;
				yield return Header.CH_AuthorityVersion;

				if (MessageType == EDIMessageSubTypeList.Codes.Deletion)
				{
					yield return EntryLines[0].CL_LineNumber.ToString();
				}
			}
		}

		public ZGuid GetGlbExternalPasswordPK() => Header.Declaration.BrokerCertificate?.PK ?? ZGuid.Empty;

		public ZString GetMessageOwner() => ZString.Empty;

		public ZString GetMessageTypeForEDIMessage() => MessageTypeList.Codes.CIL;

		public ZString GetMessageText()
		{
			IJsonMessageBuilder messageBuilder = null;

			if (MessageType != EDIMessageSubTypeList.Codes.Deletion)
			{
				messageBuilder = new DuimpLinesMessageBuilder(DuimpLinesProvider.New(EntryLines));
			}
			return messageBuilder?.GenerateJsonMessage().GetSerializedString();
		}
	}
}
