using System.IO;
using System.Text;
using System.Xml;

namespace Enterprise.DataTransfer.Native.Business
{
	public static class NativeXmlWriter
	{
		public static XmlWriter Create(Stream stream)
		{
			return XmlWriter.Create(stream, GetSettings());
		}

		public static XmlWriter Create(TextWriter textWriter)
		{
			return XmlWriter.Create(textWriter, GetSettings());
		}

		public static XmlWriter Create(StringBuilder stringBuilder)
		{
			return XmlWriter.Create(stringBuilder, GetSettings());
		}

		static XmlWriterSettings GetSettings()
		{
			return new XmlWriterSettings
			{
				Indent = true,
				OmitXmlDeclaration = true,
				NewLineHandling = NewLineHandling.Replace,
				NewLineChars = "\r\n",
				NamespaceHandling = NamespaceHandling.OmitDuplicates
			};
		}
	}
}
