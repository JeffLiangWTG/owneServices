using NUnit.Framework;

namespace Enterprise.DocumentEngine.DeliveryMethods.PrinterLanguages.Testing
{
	internal abstract class TestBase : TestCase
	{
		public abstract void TestPaperFeedSequence();
	}
}
