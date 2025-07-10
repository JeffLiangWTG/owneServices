using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;
using ZClientEDI.Business.Billing;
using static Enterprise.Client.EDI.Billing.ERouter.Test.EdiERouterChargeableUsageProviderTest;

namespace Enterprise.Client.EDI.Billing.Ebacca.Test
{
	[UseSnapshotProtection]
	public class EbaccaBillingSystemTest : TestCase
	{
		public void TestSystemCode()
		{
			EbaccaBillingSystem ebaccaBilling = new EbaccaBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.eBACCA, ebaccaBilling.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.eBACCA, new ZDateTime(2010, 10, 01), ZGuid.Empty, 10);
			Factory.Save();

			EbaccaBillingSystem ebaccaBilling = new EbaccaBillingSystemForTest();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			TransactionalSystemBill bill = ebaccaBilling.LoadSystemBills(context).First() as TransactionalSystemBill;
			AssertEquals(BillingConstants.BillingSystem.eBACCA, bill.SystemCode);

			var usageContext = new BillingLoadRawUsageContext(Factory, new ZDateTime(2010, 10, 01), ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);

			var builder = new ZStringBuilder();
			ebaccaBilling.LoadRawUsageInCsv(usageContext, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(@"""Company Code"",""Customs Code"",""Job"",""Transmitted""
", builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			ebaccaBilling.LoadRawUsageInCsv(usageContext, true, writer);
			AssertEquals(@"", writer.ToString());

			var builder2 = new ZStringBuilder();
			ebaccaBilling.LoadRawUsageInCsv(usageContext, false, (csv) => { builder2.AppendLine(csv); });
			AssertEquals(@"""Customs Code"",""Job"",""Transmitted""
", builder2.ToString());

			var writer2 = new CsvUsageReportWriterForTest();
			ebaccaBilling.LoadRawUsageInCsv(usageContext, false, writer2);
			AssertEquals(@"", writer2.ToString());
		}

		public void TestLoadOdplRawUsage()
		{
			AssertEquals("TODO: Ebacca database should be created...", true, true);
		}

		#region Implementation

		EDIOrgHeader organisation1;

		BusinessObjectFactory Factory;

		protected override void SetUp()
		{
			TestCaseHelper.RunClientDbCreateScripts();
			Factory = new BusinessObjectFactory();
			base.SetUp();
			organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			BillingTestHelper.CreateDependentOrganisation(organisation1, "AA1");
			BillingTestHelper.CreateOrganisation(Factory, "BBB");

			Factory.Save();

			ERouterUsageTestHelper.CreateUsageTable();
		}

		#endregion
	}

	class EbaccaBillingSystemForTest : EbaccaBillingSystem
	{
		protected override ExternalChargeableUsageProvider GetUsageProvider() => new EdiERouterChargeableUsageProviderForTest(ApplicationCode);
	}

	// Sanity check test to be run on an ediProd+eRouter copy rather than an empty database
	// to verify the raw usage SQL.
	// Comment out before delivery.	
	/*
	public class EbaccaBillingSystemTest2 : TestCaseWithFactory
	{
	    public void TestLoadRawUsage()
	    {
	        var company = Factory.Load<Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceCompany>(new Guid("7AFF2300-43CF-4F47-94A4-150923336FFF"));

	        EbaccaBillingSystem billing = new EbaccaBillingSystem();
	        BillingRunContext context = new BillingRunContext(Factory, new ZDateTime(2010, 11, 01), company.Header.PK);
	        TransactionalSystemRawUsage rawUsage = billing.LoadRawUsage(context) as TransactionalSystemRawUsage;
	        AssertNotNull("Raw usage", rawUsage);
	        Assert(0 < rawUsage.Summary.Lines.Count);
	        AssertEquals(company.Header.PK, rawUsage.OrganisationPK);
	    }
	}	
	*/
}
