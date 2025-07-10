namespace Enterprise.DocumentEngine.DeliveryMethods.PrinterLanguages.Testing
{
	sealed class IbmProPrinterTest : TestBase
	{
		public override void TestPaperFeedSequence()
		{
			Base ibm = new IbmProPrinter();

			byte[] actual, expected;

			actual = ibm.PaperFeedSequence(48);
			expected = new byte[] { 0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x1b, 0x43, 0x00, 0x02, 0x0c };
			AssertEquals("42 24ths", expected, actual);
		}
	}
}
