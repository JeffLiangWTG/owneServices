using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing
{
	class InvoicingBasePDFPasswordProviderTest : TestCaseWithFactory
	{
		#region GetPasswordForKRElectronicInvoice

		public void TestGetPasswordForKRElectronicInvoice_ForMenuPath()
		{
			var deliverableInfo1 = new DeliverableInfo("Test", menuItem: KRElectronicInvoiceMenuItem);
			var deliverableInfo2 = new DeliverableInfo("Test", menuItem: NonKRElectronicInvoiceMenuItem);
			var deliverableInfo3 = new DeliverableInfo("Test", menuItem: null);
			var deliverableInfo4 = new DeliverableInfo("Test", menuItem: CustomizedElectronicInvoiceMenuItem);
			var deliverableInfo5 = new DeliverableInfo("Test", menuItem: KRElectronicExemptInvoiceMenuItem);

			using (AccountingConfigurationRegistry.Instance.ElectronicInvoiceDocumentFallbackPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestPassword"))
			{
				AssertEquals("TestPassword", new InvoicingBasePDFPasswordProvider(InvoicingBase, deliverableInfo1).GetPassword());
				AssertNullOrEmpty(new InvoicingBasePDFPasswordProvider(InvoicingBase, deliverableInfo2).GetPassword());
				AssertNullOrEmpty(new InvoicingBasePDFPasswordProvider(InvoicingBase, deliverableInfo3).GetPassword());
				AssertEquals("TestPassword", new InvoicingBasePDFPasswordProvider(InvoicingBase, deliverableInfo4).GetPassword());
				AssertEquals("TestPassword", new InvoicingBasePDFPasswordProvider(InvoicingBase, deliverableInfo5).GetPassword());
			}
		}

		public void TestGetPasswordForKRElectronicInvoice_ForDeliverableInfoOrgHeader()
		{
			AssertGetPasswordForKRMenuItem_ForDeliverableInfoOrgHeader(KRElectronicInvoiceMenuItem);
		}

		void AssertGetPasswordForKRMenuItem_ForDeliverableInfoOrgHeader(StmMenuItem menuItem)
		{
			var deliverableInfo1 = new DeliverableInfo("Test", menuItem: menuItem, orgHeader: TestObjectCreator.ABIGAS);
			var deliverableInfo2 = new DeliverableInfo("Test", menuItem: menuItem, orgHeader: TestObjectCreator.AALSHI);
			var deliverableInfo3 = new DeliverableInfo("Test", menuItem: menuItem, orgHeader: null);

			var invoicingBase1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: TestObjectCreator.ZECTRA);
			var invoicingBase2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: null);

			TestObjectCreator.ABIGAS.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "TestPassword", CountryCodes.KoreaSouth);

			AssertEquals("TestPassword", new InvoicingBasePDFPasswordProvider(invoicingBase1, deliverableInfo1).GetPassword());
			AssertNullOrEmpty(new InvoicingBasePDFPasswordProvider(invoicingBase1, deliverableInfo2).GetPassword());
			AssertNullOrEmpty(new InvoicingBasePDFPasswordProvider(invoicingBase1, deliverableInfo3).GetPassword());

			AssertEquals("TestPassword", new InvoicingBasePDFPasswordProvider(invoicingBase2, deliverableInfo1).GetPassword());
			AssertNullOrEmpty(new InvoicingBasePDFPasswordProvider(invoicingBase2, deliverableInfo2).GetPassword());
			AssertNullOrEmpty(new InvoicingBasePDFPasswordProvider(invoicingBase2, deliverableInfo3).GetPassword());
		}

		public void TestGetPasswordForKRElectronicInvoice_ForInvoicingBaseOrgHeader()
		{
			AssertGetPasswordForKRMenuItem_ForInvoicingBaseOrgHeader(KRElectronicInvoiceMenuItem);
		}

		void AssertGetPasswordForKRMenuItem_ForInvoicingBaseOrgHeader(StmMenuItem menuItem)
		{
			var invoicingBase1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: TestObjectCreator.ABIGAS);
			var invoicingBase2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: TestObjectCreator.AALSHI);
			var invoicingBase3 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: null);

			var deliverableInfo1 = new DeliverableInfo("Test", menuItem: menuItem, orgHeader: TestObjectCreator.ZECTRA);
			var deliverableInfo2 = new DeliverableInfo("Test", menuItem: menuItem, orgHeader: null);

			TestObjectCreator.ABIGAS.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "TestPassword", CountryCodes.KoreaSouth);

			AssertEquals("TestPassword", new InvoicingBasePDFPasswordProvider(invoicingBase1, deliverableInfo1).GetPassword());
			AssertNullOrEmpty(new InvoicingBasePDFPasswordProvider(invoicingBase2, deliverableInfo1).GetPassword());
			AssertNullOrEmpty(new InvoicingBasePDFPasswordProvider(invoicingBase3, deliverableInfo1).GetPassword());

			AssertEquals("TestPassword", new InvoicingBasePDFPasswordProvider(invoicingBase1, deliverableInfo2).GetPassword());
			AssertNullOrEmpty(new InvoicingBasePDFPasswordProvider(invoicingBase2, deliverableInfo2).GetPassword());
			AssertNullOrEmpty(new InvoicingBasePDFPasswordProvider(invoicingBase3, deliverableInfo2).GetPassword());
		}

		public void TestGetPasswordForKRElectronicInvoice_FallBackLogic()
		{
			AssertGetPasswordForKRMenuItem_FallBackLogic(KRElectronicInvoiceMenuItem);
		}

		void AssertGetPasswordForKRMenuItem_FallBackLogic(StmMenuItem menuItem)
		{
			var deliverableInfo = new DeliverableInfo("Test", menuItem: menuItem, orgHeader: TestObjectCreator.AALSHI);
			AssertNotNull("Precondition", deliverableInfo.MenuItem);
			AssertEquals("Precondition", TestObjectCreator.AALSHI, deliverableInfo.OrgHeader);
			AssertEquals("Precondition", TestObjectCreator.ABIGAS, InvoicingBase.Header);

			var provider = new InvoicingBasePDFPasswordProvider(InvoicingBase, deliverableInfo);
			using (AccountingConfigurationRegistry.Instance.ElectronicInvoiceDocumentFallbackPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Password1"))
			{
				AssertEquals("Password1", provider.GetPassword());

				TestObjectCreator.ABIGAS.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "Password2", CountryCodes.KoreaSouth);
				AssertEquals("Password2", provider.GetPassword());

				TestObjectCreator.AALSHI.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "Password3", CountryCodes.KoreaSouth);
				AssertEquals("Password3", provider.GetPassword());
			}
		}

		#endregion

		#region GetPasswordForKRElectronicExemptInvoice

		public void TestGetPasswordForKRElectronicExemptInvoice_ForDeliverableInfoOrgHeader()
		{
			AssertGetPasswordForKRMenuItem_ForDeliverableInfoOrgHeader(KRElectronicExemptInvoiceMenuItem);
		}

		public void TestGetPasswordForKRElectronicExemptInvoice_ForInvoicingBaseOrgHeader()
		{
			AssertGetPasswordForKRMenuItem_ForInvoicingBaseOrgHeader(KRElectronicExemptInvoiceMenuItem);
		}

		public void TestGetPasswordForKRElectronicExemptInvoice_FallBackLogic()
		{
			AssertGetPasswordForKRMenuItem_FallBackLogic(KRElectronicExemptInvoiceMenuItem);
		}

		#endregion

		protected override void SetUp()
		{
			InvoicingBase = TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: TestObjectCreator.ABIGAS);

			KRElectronicInvoiceMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			KRElectronicInvoiceMenuItem.SU_MenuName = EInvoicingKoreaSouthConstants.KRElectronicInvoice;
			KRElectronicInvoiceMenuItem.SU_MenuPath = EInvoicingKoreaSouthConstants.ElectronicInvoiceMenuPath;

			KRElectronicExemptInvoiceMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			KRElectronicExemptInvoiceMenuItem.SU_MenuName = EInvoicingKoreaSouthConstants.KRElectronicExemptInvoice;
			KRElectronicExemptInvoiceMenuItem.SU_MenuPath = EInvoicingKoreaSouthConstants.ElectronicInvoiceMenuPath;

			NonKRElectronicInvoiceMenuItem = Factory.NewWithValidTestData<StmMenuItem>();

			CustomizedElectronicInvoiceMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			CustomizedElectronicInvoiceMenuItem.SU_MenuName = "Test KRElectronicInvoice";
			CustomizedElectronicInvoiceMenuItem.SU_MenuPath = EInvoicingKoreaSouthConstants.ElectronicInvoiceMenuPath;
		}

		InvoicingBase InvoicingBase;
		StmMenuItem KRElectronicInvoiceMenuItem;
		StmMenuItem KRElectronicExemptInvoiceMenuItem;
		StmMenuItem NonKRElectronicInvoiceMenuItem;
		StmMenuItem CustomizedElectronicInvoiceMenuItem;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
