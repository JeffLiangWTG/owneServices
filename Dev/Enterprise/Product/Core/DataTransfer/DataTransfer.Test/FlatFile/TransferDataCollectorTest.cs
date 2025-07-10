using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class TransferDataCollectorTest : TestCase
	{
		public void TestUnitsProcessed()
		{
			TransferDataCollector collector = new TransferDataCollector();
			AssertEquals(0, collector.UnitsProcessed);
			collector.UnitsProcessed = 500;
			AssertEquals(500, collector.UnitsProcessed);
		}

		public void TestEOF()
		{
			ITransferDataCollector collector = new TransferDataCollector();
			Assert(!collector.BOF);

			collector.EOF = true;
			Assert(collector.EOF);
		}

		public void TestBOF()
		{
			ITransferDataCollector collector = new TransferDataCollector();
			Assert(!collector.BOF);

			collector.BOF = true;
			Assert(collector.BOF);
		}
	}
}
