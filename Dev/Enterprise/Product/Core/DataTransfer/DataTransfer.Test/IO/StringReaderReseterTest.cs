using System.IO;

namespace Enterprise.DataTransfer.IO.Testing
{
	sealed class StringReaderReseterTest : TextReaderResetterTest
	{
		protected override TextReader GetNewTextReader(string textToRead)
		{
			StringReader result = new StringReader("BeforeTextToRead\r\n" + textToRead);
			result.ReadLine();
			return result;
		}
	}
}
