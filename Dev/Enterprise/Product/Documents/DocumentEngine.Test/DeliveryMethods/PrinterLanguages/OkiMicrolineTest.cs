using System;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DeliveryMethods.PrinterLanguages.Testing
{
	sealed class OkiMicrolineTest : TestBase
	{
		public override void TestPaperFeedSequence()
		{
			Base oki = new OkiMicroline();
			byte[] actual, expected;

			actual = oki.PaperFeedSequence(2 * 24);
			expected = new byte[] { 0x1b, 0x7b, 0x21, 0x1b, 0x18, 0x1b, 0x47, (byte)'0', (byte)'4', 0x0c };
			AssertEquals(expected, actual);

			actual = oki.PaperFeedSequence(23 * 24);
			expected = new byte[] { 0x1b, 0x7b, 0x21, 0x1b, 0x18, 0x1b, 0x47, (byte)'4', (byte)'6', 0x0c };
			AssertEquals(expected, actual);
		}

		[ExpectException(typeof(NotImplementedException))]
		public void TestNegative()
		{
			new OkiMicroline().GetNonZeroPaperFeedSequence(-24);
		}

		[ExpectException(typeof(NotImplementedException))]
		public void TestPrecisionGreaterThanHalfAnInch()
		{
			new OkiMicroline().GetNonZeroPaperFeedSequence(1);
		}

		[ExpectException(typeof(NotImplementedException))]
		public void TestTooLarge()
		{
			new OkiMicroline().GetNonZeroPaperFeedSequence(1200);
		}
	}
}
