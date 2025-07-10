using System;
using System.IO;
using FlexCel.Pdf;

namespace Enterprise.DocumentEngine.FlexCelInterface
{
	public class FlexCelPdfA3FileInfo
	{
		public FlexCelPdfA3FileInfo(string fileName, string mimeType, string description, TPdfAttachmentKind attachmentKind)
			: this(fileName, mimeType, description, File.GetLastWriteTime(fileName), attachmentKind, null)
		{
			DataProvider = delegate(TPdfAttachmentWriter value2)
			{
				using var data = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				value2.Write(data);
			};
		}

		public FlexCelPdfA3FileInfo(string fileName, string mimeType, string description, DateTime modificationTimeLocalTimeZone, TPdfAttachmentKind attachmentKind, TPdfAttachmentDataProviderDelegate dataProvider)
		{
			FileName = fileName;
			MimeType = mimeType;
			Description = description;
			AttachmentKind = attachmentKind;
			ModificationTimeLocalTimeZone = modificationTimeLocalTimeZone;
			DataProvider = dataProvider;
		}

		public string FileName { get; set; }
		public string MimeType { get; set; }
		public string Description { get; set; }
		public DateTime ModificationTimeLocalTimeZone { get; set; }
		public TPdfAttachmentKind AttachmentKind { get; set; }
		public TPdfAttachmentDataProviderDelegate DataProvider { get; set; }
	}
}
