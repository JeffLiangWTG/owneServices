using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Enterprise.DataTransfer.IO.Testing
{
	sealed class StreamReaderReseterTest : TextReaderResetterTest
	{
		[ExpectException(typeof(InvalidOperationException))]
		public void TestUsingStreamReaderWithPositionZero()
		{
			StreamReader reader = (StreamReader)GetNewTextReader("Text");
			reader.Read();
			AssertEquals("Position should not be zero for the test", true, reader.BaseStream.Position > 0);

			using (TextReaderResetter resetter = TextReaderResetter.New(reader))
			{
				// exception should be raised as BaseStream.Position > 0 is not currently supported
			}
		}

		protected override TextReader GetNewTextReader(string textToRead)
		{
			return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(textToRead)));
		}
	}
}
