using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsPickableDocketLineWithCharges))]
	sealed class DocWhsPickableDocketLineWithChargesTest : DocWhsPickableDocketLineTest
	{
		#region Charge Headers

		public void TestChargeHeader1()
		{
			WhsPickableDocketLine docketLine = Docket.Lines.AddNew();
			DocWhsPickableDocketLineWithCharges docWithCharges = (DocWhsPickableDocketLineWithCharges)CreateDocketLineWrapper(docketLine);
			AssertEquals(0m, docWithCharges.ChargeHeader1);
			docWithCharges.ChargeHeader1 = 1m;
			AssertEquals(1m, (docWithCharges.ChargeHeader1));
		}

		public void TestChargeHeader2()
		{
			WhsPickableDocketLine docketLine = Docket.Lines.AddNew();
			DocWhsPickableDocketLineWithCharges docWithCharges = (DocWhsPickableDocketLineWithCharges)CreateDocketLineWrapper(docketLine);
			AssertEquals(0m, docWithCharges.ChargeHeader2);
			docWithCharges.ChargeHeader2 = 1m;
			AssertEquals(1m, (docWithCharges.ChargeHeader2));
		}

		public void TestChargeHeader3()
		{
			WhsPickableDocketLine docketLine = Docket.Lines.AddNew();
			DocWhsPickableDocketLineWithCharges docWithCharges = (DocWhsPickableDocketLineWithCharges)CreateDocketLineWrapper(docketLine);
			AssertEquals(0m, docWithCharges.ChargeHeader3);
			docWithCharges.ChargeHeader3 = 1m;
			AssertEquals(1m, (docWithCharges.ChargeHeader3));
		}

		public void TestChargeHeader4()
		{
			WhsPickableDocketLine docketLine = Docket.Lines.AddNew();
			DocWhsPickableDocketLineWithCharges docWithCharges = (DocWhsPickableDocketLineWithCharges)CreateDocketLineWrapper(docketLine);
			AssertEquals(0m, docWithCharges.ChargeHeader4);
			docWithCharges.ChargeHeader4 = 1m;
			AssertEquals(1m, (docWithCharges.ChargeHeader4));
		}

		public void TestChargeHeader5()
		{
			WhsPickableDocketLine docketLine = Docket.Lines.AddNew();
			DocWhsPickableDocketLineWithCharges docWithCharges = (DocWhsPickableDocketLineWithCharges)CreateDocketLineWrapper(docketLine);
			AssertEquals(0m, docWithCharges.ChargeHeader5);
			docWithCharges.ChargeHeader5 = 1m;
			AssertEquals(1m, (docWithCharges.ChargeHeader5));
		}

		public void TestChargeHeader6()
		{
			WhsPickableDocketLine docketLine = Docket.Lines.AddNew();
			DocWhsPickableDocketLineWithCharges docWithCharges = (DocWhsPickableDocketLineWithCharges)CreateDocketLineWrapper(docketLine);
			AssertEquals(0m, docWithCharges.ChargeHeader6);
			docWithCharges.ChargeHeader6 = 1m;
			AssertEquals(1m, (docWithCharges.ChargeHeader6));
		}

		public void TestChargeHeader7()
		{
			WhsPickableDocketLine docketLine = Docket.Lines.AddNew();
			DocWhsPickableDocketLineWithCharges docWithCharges = (DocWhsPickableDocketLineWithCharges)CreateDocketLineWrapper(docketLine);
			AssertEquals(0m, docWithCharges.ChargeHeader7);
			docWithCharges.ChargeHeader7 = 1m;
			AssertEquals(1m, (docWithCharges.ChargeHeader7));
		}

		public void TestChargeHeader8()
		{
			WhsPickableDocketLine docketLine = Docket.Lines.AddNew();
			DocWhsPickableDocketLineWithCharges docWithCharges = (DocWhsPickableDocketLineWithCharges)CreateDocketLineWrapper(docketLine);
			AssertEquals(0m, docWithCharges.ChargeHeader8);
			docWithCharges.ChargeHeader8 = 1m;
			AssertEquals(1m, (docWithCharges.ChargeHeader8));
		}

		public void TestChargeHeader9()
		{
			WhsPickableDocketLine docketLine = Docket.Lines.AddNew();
			DocWhsPickableDocketLineWithCharges docWithCharges = (DocWhsPickableDocketLineWithCharges)CreateDocketLineWrapper(docketLine);
			AssertEquals(0m, docWithCharges.ChargeHeader9);
			docWithCharges.ChargeHeader9 = 1m;
			AssertEquals(1m, (docWithCharges.ChargeHeader9));
		}

		public void TestChargeHeader10()
		{
			WhsPickableDocketLine docketLine = Docket.Lines.AddNew();
			DocWhsPickableDocketLineWithCharges docWithCharges = (DocWhsPickableDocketLineWithCharges)CreateDocketLineWrapper(docketLine);
			AssertEquals(0m, docWithCharges.ChargeHeader10);
			docWithCharges.ChargeHeader10 = 1m;
			AssertEquals(1m, (docWithCharges.ChargeHeader10));
		}

		#endregion
		protected override DocWhsPickableDocketLine CreateDocketLineWrapper(WhsPickableDocketLine docketLine)
		{
			return DocWhsPickableDocketLineWithCharges.New(docketLine, Factory);
		}
	}
}
