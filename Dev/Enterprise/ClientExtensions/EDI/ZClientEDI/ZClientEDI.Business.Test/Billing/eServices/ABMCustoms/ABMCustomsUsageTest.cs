using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(ABMCustomsUsage))]
	class ABMCustomsUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = CreateUsage();
			AssertEquals(BillingConstants.BillingSystem.ABMCustoms, usage.SystemCode);
		}

		public void TestUsageTypes()
		{
			var usage = CreateUsage();

			usage.SubCode = ABMCustomsTransactionTypes.Codes.Customs;
			Assert(usage.IsCustoms);

			usage.SubCode = ABMCustomsTransactionTypes.Codes.PortCommunity;
			Assert(usage.IsPortCommunity);

			usage.SubCode = ABMCustomsTransactionTypes.Codes.FiscalRep;
			Assert(usage.IsFiscalRep);
		}

		public void TestGetGeneralSummarySections()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var user = new UsingParty(lic, lic.Database.ClientCompanies[0]);

			var priceHeader = lic.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader.L6_RX_NKCurrency = "EUR";
			BillingTestHelper.AddPriceItem(priceHeader, "CTM", BillingConstants.FeeType.Transactional, "", 1.0m).L7_Description = "ABM CustomsWare Messaging";
			BillingTestHelper.AddPriceItem(priceHeader, "POC", BillingConstants.FeeType.Transactional, "", 0.33m).L7_Description = "ABM Movement Messaging";
			BillingTestHelper.AddPriceItem(priceHeader, "FRP", BillingConstants.FeeType.Transactional, "", 1.0m).L7_Description = "ABM Fiscal Rep Invoice";

			var usage1 = new ABMCustomsUsage("CTM", Factory, user, new ZDateTime(2014, 02, 28)) { TransactionCount = 60 };
			var usage2 = new ABMCustomsUsage("POC", Factory, user, new ZDateTime(2014, 02, 28)) { TransactionCount = 50 };
			var usage3 = new ABMCustomsUsage("FRP", Factory, user, new ZDateTime(2014, 02, 28)) { TransactionCount = 20 };

			var summarySection = usage1.GetGeneralSummarySections()[0];
			AssertEquals("ABM CustomsWare Messaging", summarySection.Lines[0].MainDescription);
			AssertEquals("ABM CustomsWare Messaging Usage", summarySection.Header.MainDescription);
			AssertEquals("AAA Co (AAA-AAA-AAA)", summarySection.Header.ClientCompanyDescription);

			usage1.Reference1 = "NL";
			usage1.Reference3 = "RTM";
			summarySection = usage1.GetGeneralSummarySections()[0];
			AssertEquals("ABM CustomsWare Messaging", summarySection.Lines[0].MainDescription);
			AssertEquals("NL", summarySection.Lines[0].AdditionalDescription);
			AssertEquals("RTM", summarySection.Lines[0].PurchasedCount);

			summarySection = usage2.GetGeneralSummarySections()[0];
			AssertEquals("ABM Movement Messaging", summarySection.Lines[0].MainDescription);
			AssertEquals("ABM Movement Messaging Usage", summarySection.Header.MainDescription);
			AssertEquals("AAA Co (AAA-AAA-AAA)", summarySection.Header.ClientCompanyDescription);

			usage2.Reference1 = "NL";
			usage2.Reference2 = "PORTBASE";
			summarySection = usage2.GetGeneralSummarySections()[0];
			AssertEquals("ABM Movement Messaging", summarySection.Lines[0].MainDescription);
			AssertEquals("NL", summarySection.Lines[0].AdditionalDescription);
			AssertEquals("PORTBASE", summarySection.Lines[0].UnitCount);

			summarySection = usage3.GetGeneralSummarySections()[0];
			AssertEquals("ABM Fiscal Rep Invoice", summarySection.Lines[0].MainDescription);
			AssertEquals("ABM Fiscal Rep Invoice Usage", summarySection.Header.MainDescription);
			AssertEquals("AAA Co (AAA-AAA-AAA)", summarySection.Header.ClientCompanyDescription);

			usage3.Reference1 = "NL";
			usage3.Reference3 = "RIDCUS";
			summarySection = usage3.GetGeneralSummarySections()[0];
			AssertEquals("ABM Fiscal Rep Invoice", summarySection.Lines[0].MainDescription);
			AssertEquals("NL", summarySection.Lines[0].AdditionalDescription);
			AssertEquals("RIDCUS", summarySection.Lines[0].PurchasedCount);
		}

		#region Implementation

		ABMCustomsUsage CreateUsage()
		{
			return GetNewBusinessObject() as ABMCustomsUsage;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ABMCustomsUsage("CTM", Factory, new UsingParty(), EdiDateTest.MonthToday);
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
