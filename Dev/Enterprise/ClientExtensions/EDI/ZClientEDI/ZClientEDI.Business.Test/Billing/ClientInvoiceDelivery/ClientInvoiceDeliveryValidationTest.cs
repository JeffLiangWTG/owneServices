using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientInvoiceDeliveryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckL9_UseParentPrices()
		{
			var parent = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var child = BillingTestHelper.CreateDependentOrganisation(parent, "BBB");
			var invoiceDelivery = child.LicCompany.InvoiceDeliveries[0];
			invoiceDelivery.L9_UseParentPrices = true;
			AssertNoErrors(invoiceDelivery.L9_UseParentPricesInfo);

			invoiceDelivery.L9_UseParentPrices = false;
			AssertNoErrors(invoiceDelivery.L9_UseParentPricesInfo);

			invoiceDelivery.L9_OH_InvoiceTo = child.PK;
			invoiceDelivery.L9_UseParentPrices = true;
			AssertHasErrors(invoiceDelivery.L9_UseParentPricesInfo);

			invoiceDelivery.L9_OH_InvoiceTo = ZGuid.Empty;
			invoiceDelivery.Validation.ValidateL9_UseParentPrices();
			AssertHasErrors(invoiceDelivery.L9_UseParentPricesInfo);

			invoiceDelivery.L9_UseParentPrices = false;
			AssertNoErrors(invoiceDelivery.L9_UseParentPricesInfo);
		}

		public void TestCheckL9_SystemCode()
		{
			ClientInvoiceDelivery invoiceDelivery = Factory.New<ClientInvoiceDelivery>();
			invoiceDelivery.Validation.ValidateAll();
			AssertNoErrors(invoiceDelivery.L9_SystemCodeInfo);

			invoiceDelivery.L9_SystemCode = "XXX";
			AssertHasErrors(invoiceDelivery.L9_SystemCodeInfo);

			invoiceDelivery.L9_SystemCode = BillingConstants.BillingSystem.ODM;
			AssertNoErrors(invoiceDelivery.L9_SystemCodeInfo);

			ClientInvoiceDeliveryCollection collection = new ClientInvoiceDeliveryCollection(Factory.New<LicenceCompany>());
			collection.Add(invoiceDelivery);

			ClientInvoiceDelivery anotherDelivery = collection.AddNew();
			anotherDelivery.L9_SystemCode = BillingConstants.BillingSystem.eBACCA;
			AssertNoErrors(anotherDelivery.L9_SystemCodeInfo);

			ClientInvoiceDelivery duplicatedDelivery = collection.AddNew();
			duplicatedDelivery.L9_SystemCode = BillingConstants.BillingSystem.eBACCA;
			AssertHasErrors(duplicatedDelivery.L9_SystemCodeInfo);
		}

		public void TestCheckL9_ServerCode()
		{
			var licCompany = Factory.New<LicenceCompany>();
			ClientInvoiceDelivery invoiceDelivery = licCompany.InvoiceDeliveries.AddNew();

			invoiceDelivery.Validation.ValidateAll();
			AssertNoErrors(invoiceDelivery.L9_ServerCodeInfo);

			invoiceDelivery.L9_ServerCode = "";
			AssertNoErrors(invoiceDelivery.L9_ServerCodeInfo);

			invoiceDelivery.L9_ServerCode = "AAA";
			AssertHasErrors(invoiceDelivery.L9_ServerCodeInfo);

			var db = invoiceDelivery.Company.LicDatabases.AddNew();
			db.LD_ServerCode = "AAA";
			invoiceDelivery.Validation.ValidateAll();
			AssertNoErrors(invoiceDelivery.L9_ServerCodeInfo);

			invoiceDelivery.ServerCodeForDisplay = ClientInvoiceDelivery.AllServerCodeForDisplay;
			AssertNoErrors(invoiceDelivery.ServerCodeForDisplayInfo);

			invoiceDelivery.ServerCodeForDisplay = "999";
			AssertHasErrors(invoiceDelivery.ServerCodeForDisplayInfo);
		}

		public void TestCheckL9_GB_InvoicingBranch()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			EDIDataRegistry.Instance.AllowedInvoicingBranches.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, true);
			EDIDataRegistry.Instance.AllowedInvoicingBranches.SetValue(branch2.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, false);

			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "CO1");
			ClientInvoiceDelivery invoiceDelivery = licCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_IsBilled = true;

			invoiceDelivery.Validation.ValidateL9_GB_InvoicingBranch();
			AssertHasErrors("branch is mandatory", invoiceDelivery.L9_GB_InvoicingBranchInfo);

			invoiceDelivery.L9_GB_InvoicingBranch = branch2.PK;
			AssertHasErrors("branch set to not allowed", invoiceDelivery.L9_GB_InvoicingBranchInfo);

			invoiceDelivery.L9_GB_InvoicingBranch = branch3.PK;
			AssertHasErrors("branch not set", invoiceDelivery.L9_GB_InvoicingBranchInfo);

			invoiceDelivery.L9_GB_InvoicingBranch = branch1.PK;
			AssertNoErrors("branch set to allowed", invoiceDelivery.L9_GB_InvoicingBranchInfo);

			Factory.Save();
			EDIDataRegistry.Instance.AllowedInvoicingBranches.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, false);
			invoiceDelivery.Validation.ValidateL9_GB_InvoicingBranch();
			AssertNoErrors(invoiceDelivery.L9_GB_InvoicingBranchInfo);
			AssertHasWarnings("branch not allowed, but is saved so only warning", invoiceDelivery.L9_GB_InvoicingBranchInfo);
		}

		public void TestCheckL9_GB_InvoicingBranch_UsageBillingSettings()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			UsageBillingSettingsTest.SetupValidTestRegistry();
			EDIDataRegistry.Instance.AllowedInvoicingBranches.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, true);
			EDIDataRegistry.Instance.AllowedInvoicingBranches.SetValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true);

			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", true);
			var licCompany = lic.Company;
			var ld = licCompany.ActiveOrAllLicDatabases[0];
			Factory.Save();

			var invoiceDelivery = licCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_IsBilled = true;
			invoiceDelivery.L9_GB_InvoicingBranch = branch1.PK;
			AssertNoErrors(invoiceDelivery.L9_GB_InvoicingBranchInfo);
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranchPK;
			AssertNoErrors(invoiceDelivery.L9_GB_InvoicingBranchInfo);

			ld.LD_Product = "ABC";
			invoiceDelivery.L9_ServerCode = ld.LD_ServerCode;
			invoiceDelivery.L9_GB_InvoicingBranch = branch1.PK;
			AssertHasError(invoiceDelivery.L9_GB_InvoicingBranchInfo, "Branch has not been configured for invoicing in Registry: WiseTech Global Client Extensions -> Licence Billing -> Products (non-CW1) Enabled for Usage Billing");
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranchPK;
			AssertNoErrors(invoiceDelivery.L9_GB_InvoicingBranchInfo);

			invoiceDelivery.L9_GB_InvoicingBranch = branch1.PK;
			Factory.Save();
			invoiceDelivery.Validation.ValidateL9_GB_InvoicingBranch();
			AssertHasWarning(invoiceDelivery.L9_GB_InvoicingBranchInfo, "Branch has not been configured for invoicing in Registry: WiseTech Global Client Extensions -> Licence Billing -> Products (non-CW1) Enabled for Usage Billing");
		}

		public void TestCheckL9_RX_NKInvoiceCurrency()
		{
			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "CO1");
			ClientInvoiceDelivery invoiceDelivery = licCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_IsBilled = true;
			invoiceDelivery.Validation.ValidateL9_RX_NKInvoiceCurrency();
			AssertHasErrors(invoiceDelivery.L9_RX_NKInvoiceCurrencyInfo);

			invoiceDelivery.L9_RX_NKInvoiceCurrency = "_1_";
			invoiceDelivery.Validation.ValidateL9_RX_NKInvoiceCurrency();
			AssertHasErrors(invoiceDelivery.L9_RX_NKInvoiceCurrencyInfo);
			invoiceDelivery.L9_IsBilled = false;
			invoiceDelivery.Validation.ValidateL9_RX_NKInvoiceCurrency();
			AssertNoErrors(invoiceDelivery.L9_RX_NKInvoiceCurrencyInfo);

			invoiceDelivery.L9_IsBilled = true;
			invoiceDelivery.L9_RX_NKInvoiceCurrency = "AUD";
			invoiceDelivery.Validation.ValidateL9_RX_NKInvoiceCurrency();
			AssertNoErrors(invoiceDelivery.L9_RX_NKInvoiceCurrencyInfo);
		}

		public void TestCheckInvoicingBranchWithInvoiceCurrency()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			UsageBillingSettingsTest.SetupValidTestRegistry();
			EDIDataRegistry.Instance.AllowedInvoicingBranches.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, true);
			EDIDataRegistry.Instance.AllowedInvoicingBranches.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, true);
			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "CO1");

			var db1 = licCompany.LicDatabases.AddNew();
			db1.LD_ServerCode = "LD1";
			var db2 = licCompany.LicDatabases.AddNew();
			db2.LD_ServerCode = "LD2";

			var invoiceDelivery = licCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_IsBilled = true;
			invoiceDelivery.L9_ServerCode = "LD1";
			invoiceDelivery.L9_GB_InvoicingBranch = branch1.PK;
			invoiceDelivery.L9_RX_NKInvoiceCurrency = "AUD";

			var invoiceDelivery2 = licCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery2.L9_IsBilled = true;
			invoiceDelivery2.L9_ServerCode = "LD2";
			invoiceDelivery2.L9_GB_InvoicingBranch = branch1.PK;
			invoiceDelivery2.L9_RX_NKInvoiceCurrency = "USD";
			AssertHasError(invoiceDelivery2.L9_RX_NKInvoiceCurrencyInfo, "Cannot use a different currency if the issuing branch is the same.");
			invoiceDelivery2.L9_RX_NKInvoiceCurrency = "AUD";
			AssertNoErrors(invoiceDelivery.L9_RX_NKInvoiceCurrencyInfo);

			invoiceDelivery2.L9_GB_InvoicingBranch = branch2.PK;
			invoiceDelivery2.L9_RX_NKInvoiceCurrency = "USD";
			invoiceDelivery2.L9_GB_InvoicingBranch = branch1.PK;
			AssertHasError(invoiceDelivery2.L9_GB_InvoicingBranchInfo, "Cannot use the same branch if currency is not the same.");
			invoiceDelivery2.L9_GB_InvoicingBranch = branch2.PK;
			AssertNoErrors(invoiceDelivery.L9_GB_InvoicingBranchInfo);
		}
	}
}
