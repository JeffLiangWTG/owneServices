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

namespace Enterprise.Client.EDI.Billing.ExDocs.Test
{
	[UseSnapshotProtection]
	public class ExDocsBillingSystemTest : TestCase
	{
		public void TestSystemCode()
		{
			ExDocsBillingSystem exDocsBilling = new ExDocsBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.ExDocs, exDocsBilling.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ExDocs, new ZDateTime(2010, 10, 01), ZGuid.Empty, 10);
			Factory.Save();

			ExDocsBillingSystem exDocsBilling = new ExDocsBillingSystemForTest();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			TransactionalSystemBill bill = exDocsBilling.LoadSystemBills(context).First() as TransactionalSystemBill;
			AssertEquals(BillingConstants.BillingSystem.ExDocs, bill.SystemCode);

			var usageContext = new BillingLoadRawUsageContext(Factory, new ZDateTime(2010, 10, 01), ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);

			var builder = new ZStringBuilder();
			exDocsBilling.LoadRawUsageInCsv(usageContext, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(@"""Company Code"",""Message Type"",""Transmitted""
", builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			exDocsBilling.LoadRawUsageInCsv(usageContext, true, writer);
			AssertEquals(@"", writer.ToString());

			var builder2 = new ZStringBuilder();
			exDocsBilling.LoadRawUsageInCsv(usageContext, false, (csv) => { builder2.AppendLine(csv); });
			AssertEquals(@"""Message Type"",""Transmitted""
", builder2.ToString());

			var writer2 = new CsvUsageReportWriterForTest();
			exDocsBilling.LoadRawUsageInCsv(usageContext, false, writer2);
			AssertEquals(@"", writer2.ToString());
		}

		public void TestLoadRawUsage()
		{
			AssertEquals("TODO: ExDocs database should be created...", true, true);
		}

		#region Implementation

		EDIOrgHeader organisation1;
		BusinessObjectFactory Factory;

		protected override void SetUp()
		{
			TestCaseHelper.RunClientDbCreateScripts();
			Factory = new BusinessObjectFactory();
			base.SetUp();
			organisation1 = BillingTestHelper.CreateOrganisation(Factory, "DDD");
			BillingTestHelper.CreateDependentOrganisation(organisation1, "DDA");
			BillingTestHelper.CreateOrganisation(Factory, "DBB");
			Factory.Save();

			ERouterUsageTestHelper.CreateUsageTable();
		}

		#endregion
	}

	class ExDocsBillingSystemForTest : ExDocsBillingSystem
	{
		protected override ExternalChargeableUsageProvider GetUsageProvider() => new EdiERouterChargeableUsageProviderForTest(ApplicationCode);
	}

	// Sanity check test to be run on an ediProd+eRouter copy rather than an empty database
	// to verify the raw usage SQL.
	// Comment out before delivery.
	/*
	public class ExDocsBillingSystemTest2 : TestCaseWithFactory
	{
		public void TestLoadRawUsage()
		{
			var company = Factory.Load<Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceCompany>(new Guid("A99461E3-63C6-40E7-8172-20ECA7344C6A"));

			ExDocsBillingSystem billing = new ExDocsBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, new ZDateTime(2010, 11, 30), company.Header.PK);
			TransactionalSystemRawUsage rawUsage = billing.LoadRawUsage(context) as TransactionalSystemRawUsage;
			AssertNotNull("Raw usage", rawUsage);
			Assert(0 < rawUsage.Summary.Lines.Count);
			AssertEquals(company.Header.PK, rawUsage.OrganisationPK);
		}
	}
	*/
}
