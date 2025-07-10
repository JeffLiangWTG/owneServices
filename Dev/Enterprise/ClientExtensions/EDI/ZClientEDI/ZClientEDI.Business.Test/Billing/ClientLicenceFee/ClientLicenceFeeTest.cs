using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicenceFee))]
	internal class ClientLicenceFeeTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestL8_TaxDateCode()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var fee = org.LicCompany.Fees.AddNew();

			fee.L8_Type = "ESV";
			fee.L8_Description = "eServices Fee";
			fee.L8_Amount = 10.24m;
			fee.L8_ChargeCode = "FEECODE";
			fee.L8_StartDate = new ZDateTime(2010, 12, 1);
			foreach (ICodeDescription taxDateCode in ClientLicenceFeeLookups.AllTaxDateCodes)
			{
				fee.L8_TaxDateCode = taxDateCode.Code;
				Factory.Save();
			}
		}

		public void TestCalculateTaxDate()
		{
			var billPeriod = BillingTestHelper.MonthToday.AddMonths(-1);

			var fee = Factory.New<ClientLicenceFee>();
			fee.L8_RenewalMonths = 1;
			fee.L8_StartDate = billPeriod.AddMonths(-12);
			fee.L8_TaxDateCode = BillingConstants.Fee.TaxDateCode.FeeTaxAtCurrentDate;
			AssertEquals("when FeeTaxAtInvoiceDate", ZDateTime.Empty, fee.CalculateTaxDate(billPeriod));

			fee.L8_TaxDateCode = BillingConstants.Fee.TaxDateCode.FeeTaxAtStartDate;
			AssertEquals("when FeeTaxAtStartDate", billPeriod, fee.CalculateTaxDate(billPeriod));

			fee.L8_TaxDateCode = BillingConstants.Fee.TaxDateCode.FeeTaxAtEndDate;
			AssertEquals("when FeeTaxAtEndDate and 1 month renewal", billPeriod.AddMonths(1).AddDays(-1), fee.CalculateTaxDate(billPeriod));

			fee.L8_TaxDateCode = BillingConstants.Fee.TaxDateCode.FeeTaxAtEndDate;
			fee.L8_RenewalMonths = 12;
			AssertEquals("when FeeTaxAtEndDate and 12 month renewal", billPeriod.AddMonths(12).AddDays(-1), fee.CalculateTaxDate(billPeriod));

			AssertEquals("when FeeTaxAtEndDate and 12 month renewal and billPeriod empty", ZDateTime.Empty, fee.CalculateTaxDate(ZDateTime.Empty));
		}

		public void TestAmount_Decimals()
		{
			var fee = Factory.New<ClientLicenceFee>();
			fee.L8_Amount = 2.77m;
			AssertEquals("PRE", 2.77m, fee.L8_Amount);
			fee.L8_RX_NKCurrency = "TWD";
			AssertEquals("rounded to currency decimals", 3m, fee.L8_Amount);
		}

		public void TestCompany()
		{
			var company = Factory.New<LicenceCompany>();
			ClientLicenceFee licenceFee = company.Fees.AddNew();
			AssertEquals(company, licenceFee.Company);
		}

		public void TestIsDateRangeMatched()
		{
			ClientLicenceFee licenceFee = Factory.New<ClientLicenceFee>();
			ZDateTime billingDate = new ZDateTime(2010, 12, 01);
			AssertEquals("Both dates are empty", true, licenceFee.IsDateRangeMatched(billingDate));

			licenceFee.L8_StartDate = billingDate.AddMonths(-1);
			AssertEquals("Billing date >= Start Date, End Date is empty", true, licenceFee.IsDateRangeMatched(billingDate));

			licenceFee.L8_StartDate = billingDate.AddMonths(1);
			AssertEquals("Start Date > Billing date", false, licenceFee.IsDateRangeMatched(billingDate));

			licenceFee.L8_StartDate = ZDateTime.Empty;
			licenceFee.L8_EndDate = billingDate.AddMonths(1);
			AssertEquals("Start Date is empty, Billing date < End Date", true, licenceFee.IsDateRangeMatched(billingDate));

			licenceFee.L8_EndDate = billingDate.AddMonths(-1);
			AssertEquals("End Date < Billing date", false, licenceFee.IsDateRangeMatched(billingDate));

			licenceFee.L8_StartDate = billingDate.AddMonths(-1);
			licenceFee.L8_EndDate = billingDate.AddMonths(1);
			AssertEquals("Start Date <= Billing Date <= End Date", true, licenceFee.IsDateRangeMatched(billingDate));

			licenceFee.L8_StartDate = billingDate;
			licenceFee.L8_EndDate = billingDate.AddMonths(-1);
			AssertEquals("End Date < Billing date", false, licenceFee.IsDateRangeMatched(billingDate));

			licenceFee.L8_StartDate = billingDate.AddMonths(1);
			licenceFee.L8_EndDate = ZDateTime.Empty;
			AssertEquals("Start Date > Billing date", false, licenceFee.IsDateRangeMatched(billingDate));

			licenceFee.L8_StartDate = billingDate.AddMonths(-1);
			licenceFee.L8_RenewalMonths = 3;
			AssertEquals("Billing date is not a recurrence date", false, licenceFee.IsDateRangeMatched(billingDate));

			licenceFee.L8_StartDate = billingDate.AddMonths(-2);
			AssertEquals("Billing date is not a recurrence date", false, licenceFee.IsDateRangeMatched(billingDate));

			licenceFee.L8_StartDate = billingDate.AddMonths(-3);
			AssertEquals("Billing date is a recurrence date", true, licenceFee.IsDateRangeMatched(billingDate));
		}

		public void TestPropertiesReadOnly()
		{
			ClientLicenceFee licenceFee = Factory.New<ClientLicenceFee>();

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			foreach (ZPropertyInfo propertyInfo in licenceFee.ZPropertyInfoHash)
			{
				if (propertyInfo.Name != licenceFee.DiscountChargeCodeInfo.Name)
				{
					AssertEquals(propertyInfo.Name, false, propertyInfo.ReadOnly);
				}
			}

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			foreach (ZPropertyInfo propertyInfo in licenceFee.ZPropertyInfoHash)
			{
				AssertEquals(propertyInfo.Name, true, propertyInfo.ReadOnly);
			}
		}

		public void TestSystemCode()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicenceFee fee = organisation.LicCompany.Fees.AddNew();
			AssertEquals(BillingConstants.BillingSystem.ODM, fee.L8_SystemCode);

			fee.L8_SystemCode = BillingConstants.BillingSystem.Maintenance;
			fee.L8_RenewalMonths = 12;
			fee.L8_SystemCode = BillingConstants.BillingSystem.ODM;
			AssertEquals("monthly usage fee does not affect RenewalMonths", 12, fee.L8_RenewalMonths);
		}

		public void TestLogChanges()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicenceFee licenceFee = organisation.LicCompany.Fees.AddNew();

			licenceFee.L8_Type = "ESV";
			licenceFee.L8_Description = "eServices Fee";
			licenceFee.L8_Amount = 10.24m;
			licenceFee.L8_ChargeCode = "FEECODE";
			licenceFee.L8_StartDate = new ZDateTime(2010, 12, 1);
			Factory.Save();

			ZQuery feeLogQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Fee");
			StmALog[] logs = organisation.Logs.Find(feeLogQuery);
			AssertEquals("No log for new added fee", 0, logs.Length);

			licenceFee.L8_Type = "XXX";
			Factory.Save();

			logs = organisation.Logs.Find(feeLogQuery);
			AssertEquals("Should be one log for fee changes", 1, logs.Length);

			string expectedMessage = "Fee|Typ:ESV=>XXX|Amt:10.24|Chrg:FEECODE|Sta:01-Dec-10|Desc:eServices Fee";
			AssertEquals(expectedMessage, logs[0].SL_Reference);
			AssertEquals(Events.EditedARecordCode, logs[0].SL_SE_NKEvent);

			licenceFee.L8_Type = "AAA";
			licenceFee.L8_Description = "AAA description";
			licenceFee.L8_Amount = 20.48m;
			licenceFee.L8_ChargeCode = "FEECODE1";
			licenceFee.L8_StartDate = new ZDateTime(2011, 12, 1);
			licenceFee.L8_EndDate = new ZDateTime(2020, 12, 31);
			Factory.Save();

			logs = organisation.Logs.Find(feeLogQuery);
			AssertEquals("Should be 2 logs for fee changes", 2, logs.Length);

			expectedMessage = "Fee|Typ:XXX=>AAA|Amt:10.24=>20.48|Chrg:FEECODE=>FEECODE1|Sta:01-Dec-10=>01-Dec-11|End:=>31-Dec-20|Desc:eServices Fee=>AAA description";
			AssertEquals(true, logs.Any(x => x.SL_Reference == expectedMessage));
		}

		public void TestL8_Type()
		{
			var fee = Factory.New<ClientLicenceFee>();
			fee.L8_Type = "1ST";
			AssertNoErrors(fee.L8_TypeInfo);
			AssertEquals("1 stop not discountable - configured in EDIDataRegistry.Instance.LicenceFeeTypes", false, fee.L8_IsDiscountable);

			fee.L8_Type = "ESV";
			AssertNoErrors(fee.L8_TypeInfo);
			AssertEquals("ESV is discountable - configured in EDIDataRegistry.Instance.LicenceFeeTypes", true, fee.L8_IsDiscountable);
		}

		public void TestDiscountChargeCode()
		{
			var codes = new CodeDescriptionPairList();
			codes.AddPair("CHARGE1", "DISCOUNT1");
			codes.AddPair("CHARGE2", "DISCOUNT2");
			EDIDataRegistry.Instance.FeeBillingDiscountChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codes);
			var fee = Factory.New<ClientLicenceFee>();
			AssertEquals("", fee.DiscountChargeCode);

			fee.L8_ChargeCode = "CHARGE1";
			AssertEquals("DISCOUNT1", fee.DiscountChargeCode);

			fee.L8_ChargeCode = "CHARGE2";
			AssertEquals("DISCOUNT2", fee.DiscountChargeCode);
		}

		public void TestL8_Description_Translatable()
		{
			var obj = Factory.New<ClientLicenceFee>();
			obj.L8_Description = "A";
			var resKey = obj.L8_DescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(obj, "A").ResourceKey;
			AssertEquals("A", obj.L8_DescriptionMultilingual);
			using (Res.TemporarilySwitchLanguage("CHS"))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "B"));
				AssertEquals("B", obj.L8_DescriptionMultilingual);
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(factory, "AAA");
			ClientLicenceFee licenceFee = organisation.LicCompany.Fees.AddNew();

			return licenceFee;
		}

		#endregion
	}
}
