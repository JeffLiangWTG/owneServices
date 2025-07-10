using System.Collections.Generic;
using System.IO;

namespace Enterprise.Messaging.Business
{
	public interface IStreamFormatter
	{
		void FormatStream(ref Stream stream);
	}

	public class EDIMessageStreamFormatter : IStreamFormatter
	{
		public virtual void FormatStream(ref Stream stream)
		{
			KeyValuePair<string, string>[] replacePairs =
				new KeyValuePair<string, string>[] {
					new KeyValuePair<string, string>('\r'.ToString(), ""),
					new KeyValuePair<string, string>('\n'.ToString(), ""),
					new KeyValuePair<string, string>('\t'.ToString(), ""),
					new KeyValuePair<string, string>("?'", " "),
					new KeyValuePair<string, string>('\''.ToString(), "\r\n"),
					new KeyValuePair<string, string>('\x1f'.ToString(), ":"),
					new KeyValuePair<string, string>('\x1d'.ToString(), "+"),
					new KeyValuePair<string, string>('\x1c'.ToString(), "\r\n")
				};

			stream = stream.ReplaceStrings(replacePairs);
		}
	}

	public class EDIMessageStreamFormatterForXml : EDIMessageStreamFormatter
	{
		public override void FormatStream(ref Stream stream)
		{
			KeyValuePair<string, string>[] replacePairs =
				new KeyValuePair<string, string>[] {
					new KeyValuePair<string, string>('\x1f'.ToString(), ":"),
					new KeyValuePair<string, string>('\x1d'.ToString(), "+"),
					new KeyValuePair<string, string>('\x1c'.ToString(), "\r\n")
				};

			stream = stream.ReplaceStrings(replacePairs);
		}
	}
}
