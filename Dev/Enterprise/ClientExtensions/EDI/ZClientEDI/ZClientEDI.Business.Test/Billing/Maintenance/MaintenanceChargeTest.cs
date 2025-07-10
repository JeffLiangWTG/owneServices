using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance.Test
{
	[TestedType(typeof(MaintenanceCharge))]
	internal class MaintenanceChargeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var lic = BillingTestHelper.CreateMaintenanceLicence(Factory, "AAA");
			var recipient = NewRecipient(lic);
			ClientInvoiceDelivery delivery = lic.Company.InvoiceDeliveries[0];

			var charge = new MaintenanceChargeForTest(Factory, recipient, delivery);
			AssertEquals("IsBilled", true, charge.IsBilled);
			AssertEquals("Recipient", recipient, charge.Recipient);

			delivery.L9_IsBilled = false;
			charge = new MaintenanceChargeForTest(Factory, recipient, delivery);
			AssertEquals("IsBilled", false, charge.IsBilled);
		}

		public void TestNotifications()
		{
			EDIOrgHeader partner = BillingTestHelper.CreateOrganisation(Factory, "PAR");
			partner.LicCompany.SelfBilling.L4_IsPartner = true;

			var lic = BillingTestHelper.CreateMaintenanceLicence(Factory, "AAA");
			var recipient = NewRecipient(lic);

			var charge = new MaintenanceChargeForTest(Factory, recipient, null);
			charge.AddRowNotifications_Exposed(charge);
			AssertHasRowMessageError(charge, "No Invoicing Delivery Instructions found");

			ClientInvoiceDelivery delivery = lic.Company.InvoiceDeliveries[0];
			charge = new MaintenanceChargeForTest(Factory, recipient, delivery);
			charge.AddRowNotifications_Exposed(charge);
			AssertNoNotifications(charge);

			delivery.L9_RX_NKInvoiceCurrency = "USD";
			charge = new MaintenanceChargeForTest(Factory, recipient, delivery);
			charge.AddRowNotifications_Exposed(charge);
			AssertHasRowMessageError(charge, "Exchange rate for today not found.");

			delivery.L9_RX_NKInvoiceCurrency = "AUD";
			delivery.L9_IsBilled = false;
			charge = new MaintenanceChargeForTest(Factory, recipient, delivery);
			charge.AddRowNotifications_Exposed(charge);
			AssertHasRowMessageError(charge, "Not billable");

			delivery.L9_IsBilled = true;
			delivery.L9_OH_InvoiceTo = partner.PK;
			recipient = NewRecipient(lic);
			charge = new MaintenanceChargeForTest(Factory, recipient, delivery);
			charge.AddRowNotifications_Exposed(charge);
			AssertHasRowMessageError(charge, "Partner billing not implemented");
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MaintenanceChargeForTest(Factory, null, null);
		}

		MaintenanceBillRecipient NewRecipient(LicenceHeader lic)
		{
			return new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, lic.Company.LC_OH, "AUD", ZDateTime.Now);
		}

		#endregion
	}

	internal class MaintenanceChargeForTest : MaintenanceCharge
	{
		public MaintenanceChargeForTest(BusinessObjectFactory factory, MaintenanceBillRecipient billRecipient, ClientInvoiceDelivery invoiceDelivery)
			: base(factory, billRecipient, invoiceDelivery)
		{
		}

		public void AddRowNotifications_Exposed(BusinessObject owner)
		{
			base.AddRowNotifications(owner);
		}

		public override ZString PriceCurrencyCode
		{
			get { return "AUD"; }
		}
	}
}
