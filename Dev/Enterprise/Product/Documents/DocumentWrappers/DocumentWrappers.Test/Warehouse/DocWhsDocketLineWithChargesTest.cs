using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsDocketLineWithCharges))]
	sealed class DocWhsDocketLineWithChargesTest : DocWhsDocketLineTest<WhsReceive, WhsReceiveLine, DocWhsDocketLineWithCharges>
	{
		#region Charge Headers

		public void TestChargeHeader1()
		{
			AssertEquals(0m, DocketLineWrapper.ChargeHeader1);
			DocketLineWrapper.ChargeHeader1 = 1m;
			AssertEquals(1m, DocketLineWrapper.ChargeHeader1);
		}

		public void TestChargeHeader2()
		{
			AssertEquals(0m, DocketLineWrapper.ChargeHeader2);
			DocketLineWrapper.ChargeHeader2 = 2m;
			AssertEquals(2m, DocketLineWrapper.ChargeHeader2);
		}

		public void TestChargeHeader3()
		{
			AssertEquals(0m, DocketLineWrapper.ChargeHeader3);
			DocketLineWrapper.ChargeHeader3 = 3m;
			AssertEquals(3m, DocketLineWrapper.ChargeHeader3);
		}

		public void TestChargeHeader4()
		{
			AssertEquals(0m, DocketLineWrapper.ChargeHeader4);
			DocketLineWrapper.ChargeHeader4 = 4m;
			AssertEquals(4m, DocketLineWrapper.ChargeHeader4);
		}

		public void TestChargeHeader5()
		{
			AssertEquals(0m, DocketLineWrapper.ChargeHeader5);
			DocketLineWrapper.ChargeHeader5 = 5m;
			AssertEquals(5m, DocketLineWrapper.ChargeHeader5);
		}

		public void TestChargeHeader6()
		{
			AssertEquals(0m, DocketLineWrapper.ChargeHeader6);
			DocketLineWrapper.ChargeHeader6 = 6m;
			AssertEquals(6m, DocketLineWrapper.ChargeHeader6);
		}

		public void TestChargeHeader7()
		{
			AssertEquals(0m, DocketLineWrapper.ChargeHeader7);
			DocketLineWrapper.ChargeHeader7 = 7m;
			AssertEquals(7m, DocketLineWrapper.ChargeHeader7);
		}

		public void TestChargeHeader8()
		{
			AssertEquals(0m, DocketLineWrapper.ChargeHeader8);
			DocketLineWrapper.ChargeHeader8 = 8m;
			AssertEquals(8m, DocketLineWrapper.ChargeHeader8);
		}

		public void TestChargeHeader9()
		{
			AssertEquals(0m, DocketLineWrapper.ChargeHeader9);
			DocketLineWrapper.ChargeHeader9 = 9m;
			AssertEquals(9m, DocketLineWrapper.ChargeHeader9);
		}

		public void TestChargeHeader10()
		{
			AssertEquals(0m, DocketLineWrapper.ChargeHeader10);
			DocketLineWrapper.ChargeHeader10 = 10m;
			AssertEquals(10m, DocketLineWrapper.ChargeHeader10);
		}

		#endregion

		#region Implementation

		protected override DocWhsDocketLineWithCharges CreateDocketLineWrapper(WhsReceiveLine docketLine)
		{
			return DocWhsDocketLineWithCharges.New(docketLine, Factory);
		}

		#endregion
	}
}
