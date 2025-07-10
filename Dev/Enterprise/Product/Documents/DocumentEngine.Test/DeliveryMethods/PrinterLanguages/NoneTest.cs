namespace Enterprise.DocumentEngine.DeliveryMethods.PrinterLanguages.Testing
{
	sealed class NoneTest : TestBase
	{
		public override void TestPaperFeedSequence()
		{
			Assert("Should not print any escape sequence", new None().GetNonZeroPaperFeedSequence(123).IsEmpty);
		}
	}
}
