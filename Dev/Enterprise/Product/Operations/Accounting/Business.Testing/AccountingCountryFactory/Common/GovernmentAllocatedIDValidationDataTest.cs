using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	sealed class GovernmentAllocatedIDValidationDataTest : TestCaseWithFactory
	{
		public void TestConstructor_ShouldInitializeProperties()
		{
			var transaction = CreateTransactionWithAddressOverride("IL", "FAL", "123456789", "AP");

			var data = new GovernmentAllocatedIDValidationData(transaction);

			AssertEquals("FAL", data.EReportingStatus);
			AssertEquals("123456789", data.GovernmentAllocatedID);
			AssertEquals("AP", data.Ledger);
			AssertEquals("IL", data.OrgCountryCode);
		}

		public void TestConstructor_ShouldFallbackToMainAddressCountryCode()
		{
			var transaction = CreateTransaction("FAL", "123456789", "AP");

			var data = new GovernmentAllocatedIDValidationData(transaction);

			AssertEquals("FAL", data.EReportingStatus);
			AssertEquals("123456789", data.GovernmentAllocatedID);
			AssertEquals("AP", data.Ledger);
			AssertEquals("AU", data.OrgCountryCode);
		}

		public void TestConstructor_ShouldThrowArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new GovernmentAllocatedIDValidationData(invoicingBase: null));
		}

		InvoicingBase CreateTransaction(string eReportingStatus, string governmentAllocatedID, string ledger)
		{
			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.AH_Ledger = ledger;
			invoice.AH_GovernmentAllocatedID = governmentAllocatedID;

			if (eReportingStatus != null)
			{
				TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, status: eReportingStatus);
			}

			return invoice;
		}

		InvoicingBase CreateTransactionWithAddressOverride(string orgCountryCode, string eReportingStatus, string governmentAllocatedID, string ledger)
		{
			var invoice = CreateTransaction(eReportingStatus, governmentAllocatedID, ledger);
			invoice.AH_OA_InvoiceAddressOverride
				= TestObjectCreator.CreateAddress(TestObjectCreator.Creditor1, orgCountryCode, SharedConstants.Languages.English, "Test Address", "", OrgAddressType.Payables, "").PK;

			return invoice;
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		TestObjectCreator fTestObjectCreator;
	}
}
