using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(USCustomsUsage))]
	class USCustomsUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = CreateUsage();
			AssertEquals(BillingConstants.BillingSystem.USCustoms, usage.SystemCode);
		}

		public void TestGetGeneralSummarySections()
		{
			var org = SetupOrgWithPriceList("DDDBBBMEL", true);
			var usage1 = new USCustomsUsage("USC", Factory, new UsingParty(org), new ZDateTime(2014, 2, 28)) { TransactionCount = 60 };
			var usage2 = new USCustomsUsage("USC", Factory, new UsingParty(org), new ZDateTime(2014, 2, 28)) { TransactionCount = 201 };

			var summarySection = usage1.GetGeneralSummarySections()[0];
			CombineAssertions("usage1", () =>
			{
				AssertEquals("USC Service Bureau", summarySection.Header.MainDescription);
				AssertEquals("USC Service Bureau Usage", summarySection.Lines[0].MainDescription);
				AssertEquals("Transaction Count", summarySection.Header.UnitCount);
				AssertEquals("60", summarySection.Lines[0].UnitCount);
				AssertEquals("Minimum Count", summarySection.Header.TotalUnitCount);
				AssertEquals("200", summarySection.Lines[0].TotalUnitCount);
				AssertEquals("", summarySection.Header.ClientCompanyDescription);
			});

			summarySection = usage2.GetGeneralSummarySections()[0];
			CombineAssertions("usage2", () =>
			{
				AssertEquals("", summarySection.Header.TotalUnitCount);
				AssertEquals("", summarySection.Lines[0].TotalUnitCount);
				AssertEquals("201", summarySection.Lines[0].UnitCount);
				AssertEquals("", summarySection.Header.ClientCompanyDescription);
			});
		}

		public void TestUnitCountAndHasMinimumFee()
		{
			var org = SetupOrgWithPriceList("DDDBBBMEL", true);
			var usage1 = new USCustomsUsage("USC", Factory, new UsingParty(org), new ZDateTime(2014, 2, 28)) { TransactionCount = 60 };
			AssertEquals(200, usage1.UnitCount);
			AssertEquals(true, usage1.HasMinimumFee);
			var usage2 = new USCustomsUsage("USC", Factory, new UsingParty(org), new ZDateTime(2014, 2, 28)) { TransactionCount = 201 };
			AssertEquals(201, usage2.UnitCount);
			AssertEquals(false, usage2.HasMinimumFee);
		}

		EDIOrgHeader SetupOrgWithPriceList(ZString orgCode, bool hasMinimumFee)
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = orgCode;
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = org.OH_Code.Substring(0, 3);

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader.L6_RX_NKCurrency = "USD";
			var priceItem = BillingTestHelper.AddPriceItem(priceHeader, "USC", BillingConstants.FeeType.Transactional, "", hasMinimumFee ? 0.10m : 0.05m);
			priceItem.L7_Description = "Messaging Service Bureau";
			priceItem.L7_UnitBreak = hasMinimumFee ? 200 : 0;

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		#region Implementation

		USCustomsUsage CreateUsage()
		{
			return GetNewBusinessObject() as USCustomsUsage;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new USCustomsUsage("USC", Factory, new UsingParty(), EdiDateTest.MonthToday);
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
