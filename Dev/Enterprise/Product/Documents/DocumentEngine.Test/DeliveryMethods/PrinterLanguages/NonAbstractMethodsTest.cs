using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DeliveryMethods.PrinterLanguages.Testing
{
	sealed class NonAbstractMethodsTest : TestCase
	{
		class MockLanguage : Base
		{
			protected internal override ZBlob GetNonZeroPaperFeedSequence(int twentyFourthsOfAnInchToFeed)
			{
				return new byte[] { 42 };
			}
		}

		readonly Base TestLanguage = new MockLanguage();

		public void TestWhenZero()
		{
			Assert(TestLanguage.PaperFeedSequence(0).IsEmpty);
		}

		public void TestWhenNonZero()
		{
			AssertEquals((byte)42, TestLanguage.PaperFeedSequence(1)[0]);
			AssertEquals((byte)42, TestLanguage.PaperFeedSequence(1234)[0]);
		}
	}
}
