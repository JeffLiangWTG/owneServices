using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.eHub.Common.Extensions;
using CargoWise.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForUSDSubmission : EHubMessageBuilderForUSDIS
	{
		public const string AttachmentPlaceholderStart = "!dOcUmEnTiMaGePlAcEHoLdEr:";
		public const string AttachmentPlaceholderEnd = "!";

		public EHubMessageBuilderForUSDSubmission(EDIInterchange interchange, INotifications notifications)
			: base(interchange, notifications)
		{
		}

		protected override bool RequiresMessage()
		{
			return true;
		}

		protected override Stream ModifyMessageStream(Stream messageStream)
		{
			var reader = new StreamReader(messageStream);
			var stream = new VirtualMemoryStream();
			var writer = new StreamWriter(stream);

			try
			{
				int lineCount = 0;
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					lineCount++;
					ReplacePlaceholdersInLineWithAttachments(line, writer, lineCount);
				}
			}
#pragma warning disable ENT0001
			catch
#pragma warning restore ENT0001
			{
				stream.Dispose();
				throw;
			}

			writer.Flush();
			stream.SeekBegin();
			return stream;
		}

		void ReplacePlaceholdersInLineWithAttachments(string line, StreamWriter writer, int lineCount)
		{
			int placeholderStartPosition = -1;
			int currentPosition = 0;
			while (
				(placeholderStartPosition =
					line.IndexOf(AttachmentPlaceholderStart, currentPosition, StringComparison.InvariantCulture)) != -1)
			{
				writer.Write(line.Substring(currentPosition, placeholderStartPosition - currentPosition));

				int placeholderEndPosition = line.IndexOf(AttachmentPlaceholderEnd,
					placeholderStartPosition + AttachmentPlaceholderStart.Length, StringComparison.InvariantCulture);

				if (placeholderEndPosition == -1)
				{
					string errorMessage = Res.GetString("60C6EBD7-7B56-4898-A31D-12B2F35234EE",
						"There is no Placeholder end tag for Placeholder start tag found in line {0} at  position {1}. Please correct and resubmit the message.",
						lineCount, placeholderStartPosition);
					throw new EHubMessageBuilderInvalidInterchangeException(errorMessage);
				}

				string attachmentPKString = line.Substring(placeholderStartPosition + AttachmentPlaceholderStart.Length,
					placeholderEndPosition - placeholderStartPosition - AttachmentPlaceholderStart.Length);

				Guid attachmentPK;
				if (!Guid.TryParse(attachmentPKString, out attachmentPK))
				{
					string errorMessage = Res.GetString("DA8F53D3-7E61-478C-8D41-D5090DFD18DC",
						"Attachment Id {0} in Placeholder found in line {1} at position {2} is invalid unique identifier. Please correct and resubmit the message.",
						attachmentPKString, lineCount, placeholderStartPosition);
					throw new EHubMessageBuilderInvalidInterchangeException(errorMessage);
				}
				using (Stream attachmentStream = LoadAttachment(attachmentPK))
				{
					using (var encodedAttachmentStream = attachmentStream.EncodeStream())
					{
						encodedAttachmentStream.SeekBegin();
						var encodedAttachmentStreamReader = new StreamReader(encodedAttachmentStream);
						encodedAttachmentStreamReader.CopyTo(writer);
					}
				}
				currentPosition = placeholderEndPosition + AttachmentPlaceholderEnd.Length;
			}

			writer.WriteLine(line.Substring(currentPosition, line.Length - currentPosition));
		}

		Stream LoadAttachment(Guid attachmentPK)
		{
			var attachment = (EDIMessageAttach)interchange.ContainedMessages[0].MessageAttachments.FindByPK(attachmentPK) ?? throw new EHubMessageBuilderInvalidInterchangeException(Res.GetString("1B260580-8BAA-4290-B671-63F8929AF9E7", "There is no attachment with PK = {0} for interchange {1}. Please correct and resubmit the message.", attachmentPK, interchange.EI_InterchangeNum));

			var storageDocs = attachment.GetAttachment() ?? throw new EHubMessageBuilderInvalidInterchangeException(Res.GetString("349B2283-2191-44F4-9810-45B03ABF132E", "Attachment with PK = {0} is empty for interchange {1}. Please correct and resubmit the message.", attachmentPK, interchange.EI_InterchangeNum));

			return storageDocs.GetImageDataReader();
		}
	}
}
