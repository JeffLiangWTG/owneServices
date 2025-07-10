using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(AirlineMessagingUsage))]
	internal class AirlineMessagingUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = CreateUsage();
			AssertEquals(BillingConstants.BillingSystem.AirlineMessaging, usage.SystemCode);
			AssertEquals("AAA", usage.PriceItemCode);
		}

		public void TestUsageTypes()
		{
			var usage = CreateUsage();

			usage.SubCode = "W1C";
			Assert(usage.IsFWBUsage);

			usage.SubCode = "H1C";
			Assert(usage.IsFHLUsage);

			usage.SubCode = "S1C";
			Assert(usage.IsFSUUsage);

			usage.SubCode = "W1P";
			Assert(usage.IsChargeableTraxonUsage);

			usage.SubCode = "W1N";
			Assert(usage.IsNonChargeableTraxonUsage);

			var org = Factory.New<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.QuickAddOtherPriceList();
			org.LicCompany.PriceHeaders[0].L6_ValidFrom = new ZDateTime(2014, 1, 1);
			org.LicCompany.PriceHeaders[0].LocalOrStandardItems.FindByCode("WXP").L7_Price = -0.20m;
			usage = new AirlineMessagingUsage(Factory, new UsingParty(org), new ZDateTime(2014, 11, 1));
			usage.SubCode = "WXP";
			Assert(usage.IsRemitUsage);
		}

		public void TestGetGeneralSummarySections()
		{
			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(organisation);

			var priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			priceHeader.L6_RX_NKCurrency = "EUR";
			var priceItem1 = BillingTestHelper.AddPriceItem(priceHeader, "W3C", BillingConstants.FeeType.Transactional, "", 0.20m);
			priceItem1.L7_Description = "FWB(net) - CCSJ";
			var priceItem2 = BillingTestHelper.AddPriceItem(priceHeader, "H3C", BillingConstants.FeeType.Transactional, "", 0.10m);
			priceItem2.L7_Description = "FHL(net) - CCSJ";
			var priceItem3 = BillingTestHelper.AddPriceItem(priceHeader, "S3C", BillingConstants.FeeType.Transactional, "", 0.00m);
			priceItem3.L7_Description = "FSU - CCSJ";
			var priceItem4 = BillingTestHelper.AddPriceItem(priceHeader, "W1C", BillingConstants.FeeType.Transactional, "", 0.05m);
			priceItem4.L7_Description = "FWB(net) - BT";

			var usage1 = new AirlineMessagingUsage("W3C", Factory, user, new ZDateTime(2014, 11, 1));
			usage1.TransactionCount = 100;

			var usage2 = new AirlineMessagingUsage("H3C", Factory, user, new ZDateTime(2014, 11, 1));
			usage2.TransactionCount = 100;

			var usage3 = new AirlineMessagingUsage("S3C", Factory, user, new ZDateTime(2014, 11, 1));
			usage3.TransactionCount = 100;

			var usage4 = new AirlineMessagingUsage("W1C", Factory, user, new ZDateTime(2014, 11, 1));
			usage4.TransactionCount = 100;

			var summarySection1 = usage1.GetGeneralSummarySections()[0];
			AssertEquals("FWB(net) - CCSJ", summarySection1.Lines[0].MainDescription);
			AssertEquals("Airline Messaging Usage - CCSJ", summarySection1.Header.MainDescription);
			AssertEquals("", summarySection1.Header.ClientCompanyDescription);

			var summarySection2 = usage2.GetGeneralSummarySections()[0];
			AssertEquals("FHL(net) - CCSJ", summarySection2.Lines[0].MainDescription);
			AssertEquals("Airline Messaging Usage - CCSJ", summarySection2.Header.MainDescription);
			AssertEquals("", summarySection1.Header.ClientCompanyDescription);

			AssertEquals(0, usage3.GetGeneralSummarySections().Length);

			var summarySection4 = usage4.GetGeneralSummarySections()[0];
			AssertEquals("FWB(net) - BT", summarySection4.Lines[0].MainDescription);
			AssertEquals("Airline Messaging Usage - BT", summarySection4.Header.MainDescription);
			AssertEquals("", summarySection1.Header.ClientCompanyDescription);
		}

		#region Implementation

		AirlineMessagingUsage CreateUsage()
		{
			return GetNewBusinessObject() as AirlineMessagingUsage;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AirlineMessagingUsage("AAA", Factory, new UsingParty(), EdiDateTest.MonthToday);
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

		#endregion
	}
}
