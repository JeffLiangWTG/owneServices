using System.IO;
using MsgReader.Outlook;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public static class MsgReaderMessageExtensions
	{
		public static byte[] GetData(this Storage.Message message)
		{
			using (var memoryStream = new MemoryStream())
			{
				message.Save(memoryStream);
				return memoryStream.ToArray();
			}
		}

		public static byte[] GetData(this Storage attachment)
		{
			if (attachment is Storage.Attachment storageAttachment)
			{
				return storageAttachment.Data;
			}
			else if (attachment is Storage.Message storageMessage)
			{
				return storageMessage.GetData();
			}

			return System.Array.Empty<byte>();
		}

		public static string GetName(this Storage attachment)
		{
			if (attachment is Storage.Attachment storageAttachment)
			{
				return storageAttachment.FileName;
			}
			else if (attachment is Storage.Message storageMessage)
			{
				return storageMessage.FileName;
			}

			return string.Empty;
		}
	}
}
