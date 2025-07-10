using System.IO;
using System.Text;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class AttachmentDef
	{
		public readonly string DisplayName;
		public readonly byte[] Data;

		public static AttachmentDef CreateZippedAttachment(string displayName, string originFileName)
		{
			using (FileStream stream = new FileStream(originFileName, FileMode.Open, FileAccess.Read))
			{
				return CreateZippedAttachment(displayName, originFileName, stream);
			}
		}

		public static AttachmentDef CreateZippedAttachment(string displayName, string fileName, Stream originStream)
		{
			using (TempFile tempZipFile = TempFile.NewWithExtension("zip"))
			{
				using (MemoryStream stream = new MemoryStream())
				{
					new ZipCreator().ZipStream(fileName, originStream, stream);
					return new AttachmentDef(displayName, stream.ToArray());
				}
			}
		}

		public AttachmentDef(string displayName, string filePath)
		{
			DisplayName = displayName;

			using (FileStream fileReadStream = File.OpenRead(filePath))
			{
				Data = StreamToByteArray(fileReadStream);
			}
		}

		public AttachmentDef(string filePath)
		{
			DisplayName = new FileInfo(filePath).Name;

			using (FileStream fileReadStream = File.OpenRead(filePath))
			{
				Data = StreamToByteArray(fileReadStream);
			}
		}

		public AttachmentDef(string displayName, byte[] data)
		{
			DisplayName = displayName;
			Data = data;
		}

		/// <summary>
		/// In case you want to build an attachment from a string.
		/// </summary>
		public static byte[] StringToByteArray(string stringToConvert)
		{
			return Encoding.ASCII.GetBytes(stringToConvert);
		}

		public static byte[] StreamToByteArray(Stream streamToConvert)
		{
			byte[] buffer = new byte[32768];
			using (MemoryStream resultStream = new MemoryStream())
			{
				int readLength;
				while ((readLength = streamToConvert.Read(buffer, 0, buffer.Length)) > 0)
				{
					resultStream.Write(buffer, 0, readLength);
				}
				return resultStream.ToArray();
			}
		}
	}
}
