using System.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CA.Business
{
	class SyntaxErrorMessageStreamFormatter : EDIMessageStreamFormatter
	{
		readonly string header;
		readonly string delimiter;

		public SyntaxErrorMessageStreamFormatter(string header, string delimiter)
			: base()
		{
			this.header = header;
			this.delimiter = delimiter;
		}

		public override void FormatStream(ref Stream stream)
		{
			base.FormatStream(ref stream);
			stream = stream.AddHeader(header + delimiter);
		}
	}
}
