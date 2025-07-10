using System.IO;

namespace CargoWise.EntityFramework
{
	public class StringReaderSource : ITextReaderSource
	{
		public StringReaderSource(string str)
		{
			this.str = str;
		}

		readonly string str;

		public TextReader GetReader(bool closeUnderlyingStream = true)
		{
			return new StringReader(str);
		}
	}
}
