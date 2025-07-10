using System;
using System.IO;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.DataTransfer.IO.Testing
{
	sealed class TextReaderResetterTest_ForCoreFunctionality : TestCase
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsExceptionWhenWrongArgument()
		{
			using (MyReader reader = new MyReader())
			{
				TextReaderResetter reseter = TextReaderResetter.New(reader);
			}
		}

		public void TestWhenReaderNotAccessedBeforeReset()
		{
			using (TextReaderResetter resetter = TextReaderResetter.New(new StringReader("Text")))
			{
				AssertNotNull(resetter.Reader);
			}
			AssertEquals("Reset() must be called before disposing TextReaderResetter", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestWhenResetNotCalled()
		{
			using (TextReaderResetter resetter = TextReaderResetter.New(new StringReader("Text")))
			{
				resetter.Reset();
			}
			AssertEquals("The reader returned from the Reader property must be accessed before Reset() is called, because, in the case of StringReader, it may be a different Reader than what was passed into the constructor", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		class MyReader : TextReader
		{
		}
	}
}
