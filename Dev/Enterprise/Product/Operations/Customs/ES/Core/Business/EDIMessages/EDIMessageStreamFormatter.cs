using System.Collections.Generic;
using System.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business.EDIMessages
{
	public class ESEDIMessageStreamFormatter : IStreamFormatter
	{
		public void FormatStream(ref Stream stream)
		{
			KeyValuePair<string, string>[] replacePairs =
				new KeyValuePair<string, string>[] {
					new KeyValuePair<string, string>("?'", " "),
					new KeyValuePair<string, string>('\''.ToString(), "\r\n"),
					new KeyValuePair<string, string>('\x1f'.ToString(), ":"),
					new KeyValuePair<string, string>('\x1d'.ToString(), "+"),
					new KeyValuePair<string, string>('\x1c'.ToString(), "\r\n"),
					new KeyValuePair<string, string>("><", ">\r\n<"),
				};

			stream = stream.ReplaceStrings(replacePairs);
		}
	}
}
