using System.IO;
using System.Text;
using System.Xml;

namespace CargoWise.Data
{
	public class CustomXmlWriter : XmlTextWriter
	{
		public CustomXmlWriter(TextWriter writer) : base(writer) { }
		public CustomXmlWriter(Stream stream, Encoding encoding) : base(stream, encoding) { }
		public CustomXmlWriter(string file, Encoding encoding) : base(file, encoding) { }
		public override void WriteString(string text)
		{
			Encoding utfencoder = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("?"), new DecoderReplacementFallback("?"));
			byte[] bytText = utfencoder.GetBytes(text);
			string strEncodedText = utfencoder.GetString(bytText);
			base.WriteString(strEncodedText);
		}
	}
}
