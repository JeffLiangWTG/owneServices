using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>))]
	public class CusEntryLineFeeCollectionTest : Customs.Business.Testing.CusEntryLineFeeCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			return new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(entryLine, Factory);
		}

		public void TestHasOverrideFeeOfGivenCode()
		{
			const string testRateCode = "XXX";

			var feeCollection = (IEUCusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)GetCollectionToTest();
			AssertEquals("[PRE-CONDITION] Are there any override fees?", false, feeCollection.HasOverrideFeeOfGivenCode(testRateCode));

			var sysFee = feeCollection.AddNew();
			sysFee.CF_ChargeType = testRateCode;
			sysFee.CF_RateOverrideReasonCode = ZString.Empty;
			AssertEquals("(After adding a system calculated fee) Are there any override fees?", false, feeCollection.HasOverrideFeeOfGivenCode(testRateCode));

			var addFee = feeCollection.AddNew();
			addFee.CF_ChargeType = testRateCode;
			addFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
			AssertEquals("(After adding an additional user entered fee) Are there any override fees?", false, feeCollection.HasOverrideFeeOfGivenCode(testRateCode));

			var ovrFee = feeCollection.AddNew();
			ovrFee.CF_ChargeType = testRateCode;
			ovrFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Override;
			AssertEquals("(After adding an override user entered fee) Are there any override fees?", true, feeCollection.HasOverrideFeeOfGivenCode(testRateCode));
		}

		public void TestAllLineFees()
		{
			var feeCollection = (IEUCusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)GetCollectionToTest();
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
			var feeCollection = (IEUCusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)GetCollectionToTest();
			AssertNotNull(nameof(feeCollection.VatRefresher), feeCollection.VatRefresher);
		}
	}

	sealed class CusEntryLineFeeCollection_DoNotInheritTest : TestCaseWithFactory
	{
		public void TestVatRefresher()
		{
			CombineAssertions("When UniversalFeeCalculation is not enabled", () =>
			{
				using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, false))
				{
					var declaration = Factory.New<JobDeclaration>();
					var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
					var fees = (CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)entryLine.Fees;

					Assert(!declaration.Configuration.UseUniversalFeeCalculation(declaration));
					AssertNotNull("VatRefresher", fees.VatRefresher);
					AssertEquals("VatRefresher Type", "DummyVatRefresher", fees.VatRefresher.GetType().Name);
				}
			});

			CombineAssertions("When UniversalFeeCalculation is enabled", () =>
			{
				using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
				{
					var declaration = Factory.New<JobDeclaration>();
					var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
					var fees = (CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)entryLine.Fees;

					Assert(declaration.Configuration.UseUniversalFeeCalculation(declaration));
					AssertNotNull("VatRefresher", fees.VatRefresher);
					AssertType<VatFeeRefresher<CusEntryLineFee, CusEntryLine>>("VatRefresher", fees.VatRefresher);
				}
			});
		}
	}
}
