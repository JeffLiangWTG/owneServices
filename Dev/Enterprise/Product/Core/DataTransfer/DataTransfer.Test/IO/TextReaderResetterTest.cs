using System.IO;
using NUnit.Framework;

namespace Enterprise.DataTransfer.IO.Testing
{
	abstract class TextReaderResetterTest : TestCase
	{
		public void TestReset()
		{
			string sampleText = "Line1\r\nLine2";
			using (TextReader reader = GetNewTextReader(sampleText))
			using (TextReaderResetter reseter = TextReaderResetter.New(reader))
			{
				TextReader initialReader = reseter.Reader;
				string firstLine = initialReader.ReadLine();
				AssertEquals("Line1", firstLine);

				TextReader resetReader = reseter.Reset();
				firstLine = resetReader.ReadLine();
				AssertEquals("Line1", firstLine);
				string secondLine = resetReader.ReadLine();
				AssertEquals("Line2", secondLine);
			}
		}

		protected abstract TextReader GetNewTextReader(string textToRead);
	}
}
