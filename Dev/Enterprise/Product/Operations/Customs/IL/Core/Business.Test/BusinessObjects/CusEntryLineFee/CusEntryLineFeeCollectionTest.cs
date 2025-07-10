using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>))]
	public class CusEntryLineFeeCollectionTest : Customs.Business.Testing.CusEntryLineFeeCollectionTest
	{
		public void TestHasOverrideFeeOfGivenCode()
		{
			const string testRateCode = "XXX";

			var feeCollection = (ICusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)GetCollectionToTest();
			AssertEquals("[PRE-CONDITION] Are there any override fees?", false, feeCollection.HasOverrideFeeOfGivenCode(testRateCode));

			var sysFee = feeCollection.AddNew();
			sysFee.CF_ChargeType = testRateCode;
			sysFee.CF_RateOverrideReasonCode = ZString.Empty;
			AssertEquals("(After adding a system calculated fee) Are there any override fees?", false, feeCollection.HasOverrideFeeOfGivenCode(testRateCode));

			var addFee = feeCollection.AddNew();
			addFee.CF_ChargeType = testRateCode;
			addFee.CF_RateOverrideReasonCode = ILRateOverrideReasonList.Codes.Additional;
			AssertEquals("(After adding an additional user entered fee) Are there any override fees?", false, feeCollection.HasOverrideFeeOfGivenCode(testRateCode));

			var ovrFee = feeCollection.AddNew();
			ovrFee.CF_ChargeType = testRateCode;
			ovrFee.CF_RateOverrideReasonCode = ILRateOverrideReasonList.Codes.Override;
			AssertEquals("(After adding an override user entered fee) Are there any override fees?", true, feeCollection.HasOverrideFeeOfGivenCode(testRateCode));
		}

		public void TestAllLineFees()
		{
			var feeCollection = (ICusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)GetCollectionToTest();
			AssertNotNull("Not null", feeCollection.AllLineFees);
			AssertArrayEqualsByElements("No items", System.Array.Empty<CusEntryLineFee>(), feeCollection.AllLineFees.ToArray());

			var fee1 = feeCollection.AddNew();
			var fee2 = feeCollection.AddNew();
			AssertArrayEqualsByElements("Two items", new CusEntryLineFee[] { fee1, fee2 }, feeCollection.AllLineFees.ToArray());

			feeCollection.RemoveAndDelete(fee1);
			AssertArrayEqualsByElements("One item", new CusEntryLineFee[] { fee2 }, feeCollection.AllLineFees.ToArray());
		}

		public void TestVatRefresherNotNull()
		{
			var feeCollection = (ICusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)GetCollectionToTest();
			AssertNotNull(nameof(feeCollection.VatRefresher), feeCollection.VatRefresher);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var testDec = Factory.New<JobDeclaration>();
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			return new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(entryLine, Factory);
		}
	}
}
