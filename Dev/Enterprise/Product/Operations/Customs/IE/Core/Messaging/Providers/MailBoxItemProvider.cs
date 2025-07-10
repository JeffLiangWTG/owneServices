using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.MailboxCollectResponse;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class MailBoxItemProvider
	{
		protected readonly MailboxItem mailboxItem;

		public MailBoxItemProvider(TextReader textReader)
		{
			mailboxItem = IEXmlObjectSerializer.Deserialize<MailboxItem>(textReader);
		}

		public ZString MailBoxId => mailboxItem.MailboxId;

		public ZString TransactionId => mailboxItem.TransactionId;

		public string MessageText => messageText ?? (messageText = mailboxItem.Message?.Any?.OuterXml ?? string.Empty);
		string messageText;
	}

	public class MailBoxItemProvider<TXmlObject> : MailBoxItemProvider
	{
		public MailBoxItemProvider(TextReader textReader, bool useXsdValidation = false) : base(textReader)
		{
			this.useXsdValidation = useXsdValidation;
		}
		readonly bool useXsdValidation;

		TXmlObject message;
		public TXmlObject Message
		{
			get
			{
				if (message == null && !string.IsNullOrEmpty(MessageText))
				{
					using (var stringReader = new StringReader(MessageText))
					{
						message = IEXmlObjectSerializer.Deserialize<TXmlObject>(stringReader, useXsdValidation);
					}
				}
				return message;
			}
		}
	}
}
