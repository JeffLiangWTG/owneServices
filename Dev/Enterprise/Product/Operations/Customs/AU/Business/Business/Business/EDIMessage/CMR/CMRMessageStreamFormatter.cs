using System.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class CMRMessageStreamFormatter : EDIMessageStreamFormatter
	{
		readonly string header;

		public CMRMessageStreamFormatter(string header)
		{
			this.header = header;
		}

		public override void FormatStream(ref Stream stream)
		{
			base.FormatStream(ref stream);
			stream = stream.AddHeader(header);
		}
	}
}
