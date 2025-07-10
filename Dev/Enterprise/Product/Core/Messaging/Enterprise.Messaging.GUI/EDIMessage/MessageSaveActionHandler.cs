using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public class MessageSaveActionHandler
	{
		public MessageSaveActionHandler(EDIMessage message)
		{
			ediMessage = message;
		}
		readonly EDIMessage ediMessage;

		public void SaveMessageAction(FileNameType fileNameType)
		{
			var fileDialog = new ZSaveFileDialog();

			fileDialog.FileName = GetContentFileName(fileNameType);

			if (fileDialog.ShowDialog() == DialogResult.OK)
			{
				SaveMessageContentToFile(fileDialog.OpenFile(), fileNameType, fileDialog.UnmappedFileName);
			}
		}

		public string GetContentFileName(FileNameType fileNameType)
		{
			StringBuilder output = new StringBuilder();
			output.Append(ediMessage.EM_ApplicationCode).Append("_");
			output.Append(ediMessage.EM_ReceiveTransmit).Append("_");
			output.Append(ediMessage.EM_MessageType).Append("_");
			output.Append(ediMessage.EM_InterchangeNumber).Append("_");
			output.Append(ediMessage.EM_MessageNum);

			string extension = ediMessage.IsXMLMessage ? ".xml" : ".txt";
			if (fileNameType == FileNameType.Raw)
			{
				extension += ".raw";
			}

			output.Append(extension);

			return output.ToString();
		}

		public void SaveMessageContentToFile(Stream fileStream, FileNameType fileType, string fileNameForDebugging)
		{
			try
			{
				using (StreamWriter writer = new StreamWriter(fileStream))
				{
					if (fileType == FileNameType.Raw)
					{
						using (var reader = ediMessage.GetEM_MessageTextReader())
						{
							writer.AddStream(reader);
						}
					}
					if (fileType == FileNameType.Formatted)
					{
						writer.AddStream(ediMessage.EM_FormattedMessageTextReader);
					}
				}
			}
			// These two blocks to make warning CA1031 STFU. 
			catch (IOException ex)
			{
				AlertUserToSaveError(ex, fileNameForDebugging);
			}
			catch (ArgumentException ex)
			{
				AlertUserToSaveError(ex, fileNameForDebugging);
			}
		}

		void AlertUserToSaveError(Exception ex, string fileNameForDebugging)
		{
			Globals.Message.ShowError(ResString.GetMultilingualString("6F4FD05E-4C26-40C2-B018-E1DF2761CA38", "Could not save to disk.\r\nFile name was: {0}.\r\nError was: {1}", fileNameForDebugging, ex.Message), ResString.GetMultilingualString("C2079097-AC53-4ADB-8C7E-7CCEA212D8B5", "Could not save to disk"));
		}

		public enum FileNameType
		{
			Raw,
			Formatted
		}
	}
}
