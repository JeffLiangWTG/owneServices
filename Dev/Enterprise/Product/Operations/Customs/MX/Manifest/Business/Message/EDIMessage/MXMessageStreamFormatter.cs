using System.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class MXMessageStreamFormatter : IStreamFormatter
	{
		readonly string header;

		public MXMessageStreamFormatter(string header)
		{
			this.header = header;
		}

		public void FormatStream(ref Stream stream)
		{
			stream = stream.AddHeader(header);
		}
	}
}
