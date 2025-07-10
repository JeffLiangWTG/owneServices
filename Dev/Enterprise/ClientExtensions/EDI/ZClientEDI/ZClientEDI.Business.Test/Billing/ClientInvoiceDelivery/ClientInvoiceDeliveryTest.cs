using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientInvoiceDelivery))]
	internal class ClientInvoiceDeliveryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsAllSystemCode()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientInvoiceDelivery billing = organisation.LicCompany.InvoiceDeliveries.AddNew();
			billing.L9_SystemCode = BillingConstants.BillingSystem.All;
			Assert(billing.IsAllSystemCode);
			billing.L9_SystemCode = BillingConstants.BillingSystem.Maintenance;
			Assert(!billing.IsAllSystemCode);
		}

		public void TestGetServerCodes()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientInvoiceDelivery billing = organisation.LicCompany.InvoiceDeliveries.AddNew();

			var serverCodes = billing.GetServerCodes();
			AssertEquals(2, billing.GetServerCodes().Count);
			AssertEquals(ClientInvoiceDelivery.AllServerCodeForDisplay.Length, serverCodes.MaxCodeLength);
			Assert(serverCodes.ContainsCode(ClientInvoiceDelivery.AllServerCodeForDisplay));
			Assert(serverCodes.ContainsCode("AAA"));

			LicenceDatabase db1 = billing.Company.LicDatabases.AddNew();
			db1.LD_ServerCode = "CCC";

			LicenceDatabase db2 = billing.Company.LicDatabases.AddNew();
			db2.LD_ServerCode = "ZZZ";

			LicenceDatabase db3 = billing.Company.LicDatabases.AddNew();
			db3.LD_ServerCode = "BBB";

			serverCodes = billing.GetServerCodes();
			AssertEquals(5, serverCodes.Count);
			Assert(serverCodes.ContainsCode(ClientInvoiceDelivery.AllServerCodeForDisplay));
			Assert(serverCodes.ContainsCode("AAA"));
			Assert(serverCodes.ContainsCode("BBB"));
			Assert(serverCodes.ContainsCode("ZZZ"));
			Assert(serverCodes.ContainsCode("CCC"));
		}

		public void TestServerCodeForDisplay()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientInvoiceDelivery billing = organisation.LicCompany.InvoiceDeliveries.AddNew();

			billing.ServerCodeForDisplay = ClientInvoiceDelivery.AllServerCodeForDisplay;
			AssertEquals("all servers is indicated by blank server code", "", billing.L9_ServerCode);

			billing.ServerCodeForDisplay = "AAA";
			AssertEquals("AAA", billing.L9_ServerCode);

			billing.L9_ServerCode = "";
			AssertEquals(ClientInvoiceDelivery.AllServerCodeForDisplay, billing.ServerCodeForDisplay);
		}

		public void TestIsAllServerCode()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientInvoiceDelivery billing = organisation.LicCompany.InvoiceDeliveries.AddNew();
			AssertEquals("default is all servers", true, billing.IsAllServerCode);

			billing.L9_ServerCode = "AAA";
			AssertEquals(false, billing.IsAllServerCode);

			billing.L9_ServerCode = "";
			AssertEquals(true, billing.IsAllServerCode);
		}

		public void TestPartner()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			EDIOrgHeader partner = BillingTestHelper.CreateOrganisation(Factory, "PAR");
			ClientInvoiceDelivery billing = organisation.LicCompany.InvoiceDeliveries.AddNew();

			AssertNull("no invoice to", billing.Partner);
			AssertEquals(false, billing.IsInvoicedByPartner);

			billing.L9_OH_InvoiceTo = partner.PK;
			AssertNull("invoice to non-partner", billing.Partner);
			AssertEquals(false, billing.IsInvoicedByPartner);

			partner.LicCompany.SelfBilling.L4_IsPartner = true;
			AssertNotNull("invoice to partner", billing.Partner);
			AssertEquals(true, billing.IsInvoicedByPartner);
		}

		[TestDate(2010, 1, 1)]
		public void TestLogChanges()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicDatabases.AddNew().LD_ServerCode = "PRD";
			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();

			Factory.Save();

			ZQuery query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "InvoiceTo");
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			StmALog[] logs = org.Logs.Find(query);
			AssertEquals("No log for new added invoicing setting", 0, logs.Length);

			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "DLX";
			EDIOrgHeader payingEntity = Factory.NewWithValidTestData<EDIOrgHeader>();
			payingEntity.OH_Code = "DDDNYC";

			invoiceDelivery.L9_OH_InvoiceTo = payingEntity.PK;
			invoiceDelivery.L9_ServerCode = "PRD";

			Factory.Save();

			logs = org.Logs.Find(query);
			AssertEquals("Should be one log for invoice delivery changes", 1, logs.Length);

			string expected = "InvoiceTo Server:=>PRD System:ALL | To:Self=>DDDNYC";
			AssertEquals(expected, logs[0].SL_Reference);
			AssertEquals(Events.EditedARecordCode, logs[0].SL_SE_NKEvent);

			TestDateAttribute.Date = new DateTime(2010, 1, 2);
			invoiceDelivery.L9_IsBilled = false;
			invoiceDelivery.L9_SystemCode = BillingConstants.BillingSystem.Fax;
			Factory.Save();
			logs = org.Logs.Find(query);
			AssertEquals("Should be another log for invoice delivery changes", 2, logs.Length);
			expected = "InvoiceTo Server:PRD System:ALL=>FAX | To:DDDNYC=>None";
			AssertEquals(expected, logs[0].SL_Reference);
			AssertEquals(Events.EditedARecordCode, logs[0].SL_SE_NKEvent);
		}

		public void TestInvoiceToText()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicDatabases.AddNew().LD_ServerCode = "PRD";
			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();

			AssertEquals("Self", invoiceDelivery.InvoiceToText);

			invoiceDelivery.L9_IsBilled = false;
			AssertEquals("None", invoiceDelivery.InvoiceToText);

			EDIOrgHeader org2 = Factory.New<EDIOrgHeader>();
			org2.OH_Code = "AAABBB";
			invoiceDelivery.L9_IsBilled = true;
			invoiceDelivery.L9_OH_InvoiceTo = org2.PK;
			AssertEquals("AAABBB", invoiceDelivery.InvoiceToText);
		}

		public void TestSetDefaultValues()
		{
			ClientInvoiceDelivery billing = Factory.New<ClientInvoiceDelivery>();
			AssertEquals(BillingConstants.BillingSystem.All, billing.L9_SystemCode);
		}

		public void TestPropertiesReadOnly()
		{
			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			LicenceHeader lic2 = BillingTestHelper.CreateLicence(Factory, "XYZ");
			ClientInvoiceDelivery invoiceDelivery = lic.Company.InvoiceDeliveries.AddNew();

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			foreach (ZPropertyInfo propertyInfo in invoiceDelivery.ZPropertyInfoHash)
			{
				bool expectReadonly = propertyInfo.Name == ClientInvoiceDelivery.Schema.DoNotBillReason;
				AssertEquals(propertyInfo.Name, expectReadonly, propertyInfo.ReadOnly);
			}

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			foreach (ZPropertyInfo propertyInfo in invoiceDelivery.ZPropertyInfoHash)
			{
				AssertEquals(propertyInfo.Name, true, propertyInfo.ReadOnly);
			}
		}
	}
}
