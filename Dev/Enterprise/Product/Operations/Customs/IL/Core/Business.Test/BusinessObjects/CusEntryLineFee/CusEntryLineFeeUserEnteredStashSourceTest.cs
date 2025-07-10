using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class CusEntryLineFeeUserEnteredStashSourceTest : TestCaseWithFactory
	{
		public void TestStashedPropertiesAndConditions()
		{
			var stashFee = entryLineFee.UserEnteredStashSource;
			AssertContainsExactElementsInAnyOrder(new ZPropertyInfo[] { entryLineFee.CF_MethodOfPaymentInfo }, stashFee.StashedPropertiesAndConditions.Keys.ToArray());
			Assert(stashFee.StashedPropertiesAndConditions[entryLineFee.CF_MethodOfPaymentInfo]);
		}

		public void TestShouldNotStash_WhenAllEmpty()
		{
			var stashFee = entryLineFee.UserEnteredStashSource;
			AssertEquals(false, stashFee.ShouldStash());
		}

		public void TestShouldStash_WhenMethodOfPaymentHasValue()
		{
			entryLineFee.CF_MethodOfPayment = "MP";
			var stashFee = entryLineFee.UserEnteredStashSource;
			AssertEquals(true, stashFee.ShouldStash());
		}

		public void TestGetStashKey()
		{
			entryLineFee.CF_ChargeType = "A001";
			entryLineFee.CF_MethodOfCalculation = "%";
			var expectedStashKey = "A001_%_" + entryLineFee.EntryLine.PK;
			var stashFee = entryLineFee.UserEnteredStashSource;
			AssertEquals(expectedStashKey, stashFee.GetStashKey());
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryLineFee = SetEntryLineFeeData();
		}

		CusEntryLineFee SetEntryLineFeeData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First().MergedLines.Cast<CusEntryLine>()
				.First();
			var entryLineFee = entryLine.Fees.AddNew();
			return entryLineFee;
		}

		CusEntryLineFee entryLineFee;
	}
}
