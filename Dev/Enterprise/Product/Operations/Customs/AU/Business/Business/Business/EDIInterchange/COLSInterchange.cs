using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using MimeKit;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class COLSInterchange : EDIInterchange
		, IMessageDataProvider
		, Integration.Customs.AU.ICOLSInterchange
	{
		public COLSInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetMessageNum(string messageText, int messageNumberSequece)
		{
			return new ZString(EI_InterchangeNum + messageNumberSequece.ToString().PadLeft(6, '0')).Left(20);
		}

		protected override Type GetMessageTypeToCreate(ZString messageText) => typeof(COLSMessage);

		protected override void OnMessageGenerated(EDIMessage message)
		{
			base.OnMessageGenerated(message);
			message.EM_MessageType = EI_InterchangeType;
			message.EM_ApplicationReference = ZString.Empty;
		}

		public override bool IsTestInterchange => false;

		public override UNCharacterSet CharacterSet => new UNOACharacterSet();

		#region IMessageDataProvider

		BinaryReader IMessageDataProvider.GetMessageData()
		{
			return IsTransmitInterchange && EI_InterchangeType.EqualsIgnoringCase(AUCOLSMessageTypeList.Codes.AddAttachment)
				? GetEI_BodyWithMessageAttachments()
				: new BinaryReader(GetEI_BodyTextReader().CopyAndDispose());
		}

		BinaryReader GetEI_BodyWithMessageAttachments()
		{
			var message = ContainedMessages[0];
			var boundary = message.PK.ToString();
			var messageJson = JsonSerializer.Deserialize<COLSAttachmentMessage>(EI_BodyText);

			var fileName = messageJson.file;
			var attachment = message.MessageAttachments.Find(x => x.EG_FileName == fileName && x.EG_StorageDocsGuid.IsValid).FirstOrDefault();
			byte[] fileData = attachment?.GetAttachment()?.ImageData;

			// Is it better to throw an error or to send a message with a dud attachment?

			var cleanedFileName = fileName.Replace('\'', '_').Replace('"', '_').Replace(';', '_').Replace('=', '_');
			var contentType = new MediaTypeHeaderValue(MimeTypes.GetMimeType(cleanedFileName));

			var mimeMessage =
$@"Content-Type: multipart/form-data; boundary=""{boundary}""

--{boundary}
Content-Type: text/plain; charset=utf-8
Content-Disposition: form-data; name=lastDoc

{messageJson.lastDoc}
--{boundary}
Content-Type: text/plain; charset=utf-8
Content-Disposition: form-data; name=docReference

{messageJson.docReference}
--{boundary}
Content-Type: text/plain; charset=utf-8
Content-Disposition: form-data; name=docType

{messageJson.docType}
--{boundary}
Content-Type: {contentType}
Content-Disposition: form-data; name=file; filename=""{cleanedFileName}""; filename*=""utf-8''{Uri.EscapeDataString(cleanedFileName)}""

";

			var mimeMessageFooter =
$@"
--{boundary}--
";

			var messageData = BuildMessageData(mimeMessage, mimeMessageFooter, fileData);
			return new BinaryReader(new MemoryStream(messageData));
		}

		byte[] BuildMessageData(string mimeMessage, string mimeMessageFooter, byte[] fileData)
		{
			var messageBytes = Encoding.UTF8.GetBytes(mimeMessage);
			var messageFooterBytes = Encoding.UTF8.GetBytes(mimeMessageFooter);

			var buffer = new byte[messageBytes.Length + (fileData?.Length ?? 0) + messageFooterBytes.Length];

			messageBytes.CopyTo(buffer, 0);
			var pos = messageBytes.Length;

			if (fileData != null)
			{
				fileData.CopyTo(buffer, pos);
				pos += fileData.Length;
			}

			messageFooterBytes.CopyTo(buffer, pos);

			return buffer;
		}

		#endregion
	}
}
