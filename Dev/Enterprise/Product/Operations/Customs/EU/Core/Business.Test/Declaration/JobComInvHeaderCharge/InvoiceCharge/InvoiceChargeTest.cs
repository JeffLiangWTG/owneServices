using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	public class InvoiceChargeTest : Customs.Business.Testing.BaseInvoiceChargeTest
	{
		public void TestDefaultCurrency()
		{
			invoice.JZ_RX_NKInvoice_Currency = "EUR";

			var charge1 = invoice.Charges.AddNew();
			charge1.J7_Amount = 1m;
			AssertEquals("EUR", charge1.J7_RX_NKCurrency);

			charge1.J7_RX_NKCurrency = "";
			var charge2 = invoice.Charges.AddNew();
			charge2.J7_Amount = 1m;
			AssertEquals("EUR", charge2.J7_RX_NKCurrency);

			charge2.J7_RX_NKCurrency = "CNY";
			var charge3 = invoice.Charges.AddNew();
			charge3.J7_Amount = 1m;
			AssertEquals("CNY", charge3.J7_RX_NKCurrency);
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		protected override void PrepareCharge(JobComInvCharge charge)
		{
			base.PrepareCharge(charge);
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
		}

		public void TestJ7_RX_NKCurrency_Caption_Default()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<InvoiceCharge>().J7_RX_NKCurrencyInfo, string.Empty);
			AssertEquals("Caption", "Curr.", captionResourceString.Caption);
		}

		public void TestJ7_RX_NKCurrency_CaptionImportUCC6()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<InvoiceCharge>().J7_RX_NKCurrencyInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("Caption", "Curr.", captionResourceString.Caption);
				AssertEquals("FullDescription", "[14 05 000 000] Invoice currency", captionResourceString.FullDescription);
			});
		}

		public void TestAmountCorrection()
		{
			Assert("Will be implemented by WI's WI00831078 WI00837660 WI00838181 WI00838254 WI00838273 WI00838919 WI00838938 WI00838971 WI00839025 WI00839039 WI00839057", true);
		}

		public void TestCaptions() => CombineAssertions(() =>
		{
			InvoiceChargesTestHelper.AssertCaptions(Factory.New<InvoiceCharge>());
		});
	}
}
