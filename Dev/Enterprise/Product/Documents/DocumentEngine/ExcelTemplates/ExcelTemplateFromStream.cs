using System.IO;

namespace Enterprise.ExcelTemplates
{
	public class ExcelTemplateWrappingStream : ExcelTemplate
	{
		public ExcelTemplateWrappingStream(string name, Stream stream)
			: base(name, "ExcelTemplateFromStream")
		{
			this.stream = stream;
		}
		readonly Stream stream;

		protected override Stream GetAsTemplateStreamInternal()
		{
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}

		protected override byte[] GetAsByteArrayInternal()
		{
			return Enterprise.ZArchitecture.Environment.AttachmentDef.StreamToByteArray(GetAsTemplateStreamInternal());
		}

		protected override long GetFileSizeInBytesCore() => GetAsTemplateStream().Length;
	}
}
