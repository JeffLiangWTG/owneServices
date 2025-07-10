using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class TranslationDataSourceTest : TestCaseWithFactory
	{
		public void TestGetSources()
		{
			EDIDataRegistry.Instance.BillingTranslationExportSupportedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.SharedConstants.Languages.ChineseSimplified });

			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";
			lic1.Company.Header.OH_Language = Core.SharedConstants.Languages.ChineseSimplified;

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = period1;
			priceHeader.L6_UseStandardDiscount = false;

			var chargeCode1 = BillingTestHelper.CreateChargeCode(Factory, null, "CCTEST1");
			chargeCode1.AC_Desc = "CCTEST1 Test 1";
			var chargeCode2 = BillingTestHelper.CreateChargeCode(Factory, null, "CCTEST2");
			chargeCode2.AC_Desc = "CCTEST2 Test 2";
			var chargeCode3 = BillingTestHelper.CreateChargeCode(Factory, null, "CCTEST3");
			chargeCode3.AC_Desc = "CCTEST3 Test 3";

			var item1 = priceHeader.Items.AddNew();
			item1.L7_Code = "C03";
			item1.L7_Price = 40;
			item1.L7_Order = 1;
			item1.L7_FeeType = BillingConstants.FeeType.Transactional;
			item1.L7_ChargeCode = "CCTEST1";
			item1.L7_DepositChargeCode = "CCTEST2";
			item1.L7_DiscountChargeCode = "CCTEST3";
			item1.L7_Language = "";
			item1.L7_Description = "L7_Description Test 1";
			item1.L7_ChargeBasis = "L7_ChargeBasis Test 2";

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = priceHeader.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink1.PHL_ValidFrom = period1;

			lic1.Company.SelfBilling.L4_InvoiceComment = "L4_InvoiceComment Test 3";

			var fee = lic1.Company.Fees.AddNew();
			fee.L8_Description = "L8_Description Test 4";
			fee.L8_Amount = 100m;
			fee.L8_ChargeCode = chargeCode2.AC_Code;
			fee.L8_StartDate = ZDate.Today;

			Factory.Save();

			var dataSource = new TranslationDataSource();
			var values = dataSource.GetTranslatableResources().ToArray();

			var valueAsText = new StringBuilder();

			foreach (var value in values)
			{
				valueAsText.AppendLine(value.Id);

				foreach (var pair in value.Resources.OrderBy(x => x.Key))
				{
					valueAsText.AppendLine($" {pair.Key} - {pair.Value}");
				}
			}

			AssertEquals(@"Registry - Fee Codes
 EDIDataRegistry|InvoicingProcessingFee|DirectDebitDiscount - Direct Debit Discount
 EDIDataRegistry|InvoicingProcessingFee|ManualProcessingFee - Manual Processing Fee
 EDIDataRegistry|InvoicingProcessingFee|None - None
Registry - STL Discount Types
 R!StlDiscountTypes$QUJNIEZpc2NhbCBSZXAgSW52b2ljZQ== - ABM Fiscal Rep Invoice
 StlDiscountTypes|3RDPARTY - 3rd Party Transaction Charge
 StlDiscountTypes|ABMCUST - ABM Customs
 StlDiscountTypes|ABMPORTC - ABM Port Community
 StlDiscountTypes|BWLEGACY - BorderWise Legacy Discount
 StlDiscountTypes|BWSPECIAL - BorderWise Special Condition Discount
 StlDiscountTypes|BWSTUDENT - BorderWise Student Discount
 StlDiscountTypes|CCLPATTAIN - CCLP Attain Certification Discount
 StlDiscountTypes|CCLPMEET - CCLP Meet Requirement Discount
 StlDiscountTypes|CCLPRETAIN - CCLP Retain Certification Discount
 StlDiscountTypes|COMMIT - Commitment
 StlDiscountTypes|CTR - Container Tracking
 StlDiscountTypes|DEVCOUNTRY - Developing Country/Region
 StlDiscountTypes|DEVPARTNER - Dev Partner
 StlDiscountTypes|DOMESTIC - Domestic Entity
 StlDiscountTypes|E2ENONWIP - E2E Non WIP Discount
 StlDiscountTypes|E2EWIP - E2E WIP Discount
 StlDiscountTypes|GROUP - Group Buying
 StlDiscountTypes|HTFN - HTFN
 StlDiscountTypes|LDAASPREPAY - LDaaS Prepayment
 StlDiscountTypes|PREPAY - Prepayment
 StlDiscountTypes|SPECIAL - Special Condition Discount
 StlDiscountTypes|VOLUME - Standard Volume
 StlDiscountTypes|WIM250 - WIM250
 StlDiscountTypes|WISECLOUD - WiseCloud
 StlDiscountTypes|WISEPARTNER - Wise Industry Partner Discount
 StlDiscountTypes|WISESPECIAL - Special Condition WC Discount
Registry - Invoice Description
 EDIDataRegistry|MonthlyUsageInvoiceDescription - Monthly Usage Invoice
Registry - STL Monthly Usage Invoice Description
 StlMonthlyUsageInvoiceDescription|SMF - SmartFreight Monthly Usage Invoice - {Date:MMMM} {Date:yyyy}
Registry - Report Breakdown Comment
 EDIDataRegistry|MonthlyUsageReportBreakdownComment - For more detailed breakdown of usage, visit myaccount.cargowise.com > Usage Reports.
Registry - Prepayment Comment
 EDIDataRegistry|PrepaymentInvoiceComment - If you provide a Simplified Payment Option to ensure you have enough credit funds on your account, this will be processed on the 21st of each month.
Org. - EN1AU1
 L4_InvoiceComment@L4_InvoiceComment Test 3$TDRfSW52b2ljZUNvbW1lbnQgVGVzdCAz - L4_InvoiceComment Test 3
 L8_Description@L8_Description Test 4$TDhfRGVzY3JpcHRpb24gVGVzdCA0 - L8_Description Test 4
Price Header - 
 L7_ChargeBasis@L7_ChargeBasis Test 2$TDdfQ2hhcmdlQmFzaXMgVGVzdCAy - L7_ChargeBasis Test 2
 L7_Description@L7_Description Test 1$TDdfRGVzY3JpcHRpb24gVGVzdCAx - L7_Description Test 1
 VC_Description@CCTEST1 Test 1$Q0NURVNUMSBUZXN0IDE= - CCTEST1 Test 1
 VC_Description@CCTEST2 Test 2$Q0NURVNUMiBUZXN0IDI= - CCTEST2 Test 2
 VC_Description@CCTEST3 Test 3$Q0NURVNUMyBUZXN0IDM= - CCTEST3 Test 3
", valueAsText.ToString());
		}

		protected override void SetUp()
		{
			base.SetUp();
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}
	}
}
