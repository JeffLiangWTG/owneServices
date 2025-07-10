using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
	{
		public void TestShouldResetDataOnMerging_IfAmountIsEntered()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			lineFee.CF_ChargeAmount = 12.1m;
			AssertEquals(false, lineFee.ShouldResetDataOnMerging);
		}

		public void TestShouldResetDataOnMerging_IfAmountIsEmpty()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			AssertEquals(true, lineFee.ShouldResetDataOnMerging);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.CusEntryLineFee to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryLineFee)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestLocalCurrencyCode()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var lineFee = entryLine.Fees.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Currency from dbo.JobDeclaration", GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, lineFee.LocalCurrencyCode);
				lineFee.CF_CL = ZGuid.Empty;
				AssertEquals("Empty if no JobDeclaration linked", ZString.Empty, lineFee.LocalCurrencyCode);
			});
		}
	}
}
