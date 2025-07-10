using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;
using Enterprise.Accounting.Export.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using static Enterprise.MasterFiles.Business.CountryCompliance.TurkeyComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	public class PartyTest : TestCaseWithFactory
	{
		readonly TurkeyEInvoiceTestHelper Helper = new TurkeyEInvoiceTestHelper();

		public void TestAccountingParties()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, ComplianceSubTypeCodes.EAR);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
				var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertSupplier(eInvoice);
					AssertCustomer(eInvoice);
				}
			}
		}

		public void TestVknTcknIsCorrectForDebtor()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, "AR001", ComplianceSubTypeCodes.EAR, true);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					Helper.TestObjectCreator.DebtorTR.OH_Category = OrgConstants.Category.Business;
					Helper.TestObjectCreator.DebtorTR.Factory.Save();
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch, true);
					var customerPartyIdentification = eInvoice.Invoice.AccountingCustomerParty.Party.PartyIdentification;
					AssertEquals("34567890123", customerPartyIdentification[0].ID.Value);
					AssertEquals("VKN", customerPartyIdentification[0].ID.schemeID);
					AssertNotEquals("TCKN", customerPartyIdentification[0].ID.schemeID);
				}

				var arInvoice1 = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, "AR002", ComplianceSubTypeCodes.EIN, true, "C0002");
				var invoicingBatch1 = Helper.CreateInvoiceBatch(arInvoice1,2);
				using (var transactionBatch = exporter.CreateTransactionBatch(invoicingBatch1, SchemaVersionManager.Current.Namespace))
				{
					Helper.TestObjectCreator.DebtorTR.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
					Helper.TestObjectCreator.DebtorTR.Factory.Save();
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch, true);
					var customerPartyIdentification = eInvoice.Invoice.AccountingCustomerParty.Party.PartyIdentification;
					AssertEquals("12345678901", customerPartyIdentification[0].ID.Value);
					AssertNotEquals("VKN", customerPartyIdentification[0].ID.schemeID);
					AssertEquals("TCKN", customerPartyIdentification[0].ID.schemeID);
				}
			}
		}

		public void TestVknTcknIsCorrectForDebtor_EmptyCustomCodes()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, ComplianceSubTypeCodes.EAR, createDebtorCustomeCodes: false);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);
					var customerPartyIdentification = eInvoice.Invoice.AccountingCustomerParty.Party.PartyIdentification;
					AssertEquals(string.Empty, customerPartyIdentification[0].ID.Value);
					AssertEquals("VKN", customerPartyIdentification[0].ID.schemeID);
					AssertNotEquals("TCKN", customerPartyIdentification[0].ID.schemeID);
				}
			}
		}

		void AssertSupplier(InvoiceInfo eInvoice)
		{
			var accountingSupplierParty = eInvoice.Invoice.AccountingSupplierParty.Party;
			AssertEquals("www.istanbul-sariyer.com", accountingSupplierParty.WebsiteURI.Value);

			var partyIdentification = eInvoice.Invoice.AccountingSupplierParty.Party.PartyIdentification;
			AssertEquals("34567890123", partyIdentification[0].ID.Value);
			AssertEquals("VKN", partyIdentification[0].ID.schemeID);

			AssertEquals("IST", partyIdentification[1].ID.Value);
			AssertEquals("SUBENO", partyIdentification[1].ID.schemeID);

			AssertEquals("123456789-987654321", partyIdentification[2].ID.Value);
			AssertEquals("TICARETSICILNO", partyIdentification[2].ID.schemeID);

			AssertEquals("1098765432123456", partyIdentification[3].ID.Value);
			AssertEquals("MERSISNO", partyIdentification[3].ID.schemeID);

			AssertEquals("Ulukom Test Company Name", accountingSupplierParty.PartyName.Name.Value);

			var postalAddress = accountingSupplierParty.PostalAddress;
			AssertEquals("Reşitpaşa Mah. Katar Cad. İTÜ Ayazağa Kamüsü Teknokent ARI 1 Binası No:2/5/7 Maslak", postalAddress.StreetName.Value);
			AssertEquals("Sarıyer", postalAddress.CitySubdivisionName.Value);
			AssertEquals("Istanbul", postalAddress.CityName.Value);
			AssertEquals("Türkiye", postalAddress.Country.Name.Value);
			AssertEquals("12345", postalAddress.PostalZone.Value);

			var partyTaxScheme = accountingSupplierParty.PartyTaxScheme;
			AssertEquals("KURUMLAR", partyTaxScheme.TaxScheme.Name.Value);

			var contact = accountingSupplierParty.Contact;
			AssertEquals("+90 333 222 11 00", contact.Telephone.Value);
			AssertEquals("+90 222 333 22 22", contact.Telefax.Value);
			AssertEquals("istanbul@wisetech.com", contact.ElectronicMail.Value);
		}

		void AssertCustomer(InvoiceInfo eInvoice)
		{
			var accountingCustomerParty = eInvoice.Invoice.AccountingCustomerParty.Party;
			AssertEquals("www.istanbul-sisli.com", accountingCustomerParty.WebsiteURI.Value);

			var partyIdentification = accountingCustomerParty.PartyIdentification;
			AssertEquals("12345678901", partyIdentification[0].ID.Value);
			AssertNotEquals("VKN", partyIdentification[0].ID.schemeID);
			AssertEquals("TCKN", partyIdentification[0].ID.schemeID);

			AssertEquals("Istanbul Debtor Test Company", accountingCustomerParty.PartyName.Name.Value);

			var postalAddress = accountingCustomerParty.PostalAddress;
			AssertEquals("Mecidiyeköy Mah. 1. Taş Ocağı Cad. Burç Sok. No:10 Kat 4", postalAddress.StreetName.Value);
			AssertEquals("Şişli", postalAddress.CitySubdivisionName.Value);
			AssertEquals("Istanbul", postalAddress.CityName.Value);
			AssertEquals("", postalAddress.PostalZone.Value);
			AssertEquals("Türkiye", postalAddress.Country.Name.Value);

			var partyTaxScheme = accountingCustomerParty.PartyTaxScheme;
			AssertEquals("KURUMLAR", partyTaxScheme.TaxScheme.Name.Value);

			var contact = accountingCustomerParty.Contact;
			AssertEquals("+90 212 212 26 92", contact.Telephone.Value);
			AssertEquals("+90 212 212 26 93", contact.Telefax.Value);
			AssertEquals("sisli@istanbul.com", contact.ElectronicMail.Value);

			var person = accountingCustomerParty.Person;
			AssertNotNull(person);
			AssertEquals("Istanbul Debtor Test", person.FirstName.Value);
			AssertEquals("Company", person.FamilyName.Value);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			DataAccess = new BatchExportDataAccess(connection, transaction);
		}

		BatchExportDataAccess DataAccess;
	}
}
