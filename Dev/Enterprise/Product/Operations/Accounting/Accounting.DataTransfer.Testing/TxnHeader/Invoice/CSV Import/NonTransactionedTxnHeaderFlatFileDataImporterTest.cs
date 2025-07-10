using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile.Testing
{
	[UseSnapshotProtection]
	public class NonTransactionedTxnHeaderFlatFileDataImporterTest : TestCase
	{
		public void TestOnlySaveDataWhenNoRecordsHaveErrorsWhenImportingMultipleTranactions()
		{
			var importer = new TxnHeaderFlatFileDataImporterTest.TxnHeaderFlatFileDataImporterTestClass();
			var factory = new BusinessObjectFactory();

			var testObjectCreator = new TestObjectCreator(factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);
			factory.Save();

			var glQuery = new ZQuery(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.BalanceSheetAccount);
			glQuery.AddToFilter(AccGLHeaderSchema.AG_IsActive, ZBool.True);
			glQuery.AddToFilter(AccGLHeaderSchema.AG_ControlAccount, ZBool.False);
			glQuery.AddToFilter(AccGLHeaderSchema.AG_DisallowDirectPosting, ZBool.False);

			var glAccount = factory.LoadTop1<AccGLHeader>(glQuery);
			var taxQuery = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
			taxQuery.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var taxRate = factory.LoadTop1<AccTaxRate>(taxQuery);
			taxRate.SetRateNumerator_ForTestOnly(0);
			var invoice = testObjectCreator.CreateAPInvoice<APInvoice>("1", testObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, testObjectCreator.AALSHI);
			invoice.Lines[0].GenericCharge = glAccount.PK;
			invoice.Lines[0].AL_AT = taxRate.PK;
			invoice.RunPreSaveValidation();
			AssertEquals("Precondition: Invoice must have no erorrs.", false, invoice.HasErrors);
			var dataAdapter = new FinancialInvoiceDataAdapter(false);
			var notificationBuffer = new NotificationBuffer();
			Xsd.TxnHeader newTxnHeader = dataAdapter.ExportToValueObject(invoice, new ValueObjectExportContext(notificationBuffer));
			AssertEquals("Precondition: Invoice must be exported without errors.", false, notificationBuffer.HasErrors);

			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			var transactionNumber = 1;

			importer.MaxTransactionCountCreatedInFactory = 10;
			for (int i = 0; i < importer.MaxTransactionCountCreatedInFactory * 3 + 4; i++)
			{
				newTxnHeader = dataAdapter.ExportToValueObject(invoice, new ValueObjectExportContext(new NotificationBuffer()));
				newTxnHeader.TxnNumber = transactionNumber.ToString();
				transactionNumber++;

				txnHeaderCollection.Add(newTxnHeader);
			}

			txnHeaderCollection[importer.MaxTransactionCountCreatedInFactory + 2].PostDate = ZDateTime.Today.AddMonths(1);
			txnHeaderCollection[importer.MaxTransactionCountCreatedInFactory * 3 + 3].PostDate = ZDateTime.Today.AddMonths(1);

			importer.RunExtraValidation = true;
			importer.ImportingSingleTransaction = false;
			importer.OnlySaveDataWhenNoRecordsHaveErrors = true;

			var transactionCountBeforeExport = factory.GetDatabaseCount(typeof(AccTransactionHeader));
			var notifications = new NotificationBuffer();
			var isOperetionSuccessful = importer.ExtractToDataAdapter(txnHeaderCollection, notifications);
			AssertEquals("ExtractToDataAdapter must return false.", false, isOperetionSuccessful);
			AssertNotNull("Imported Invoice", importer.ImportedInvoice);
			AssertEquals("There are no transactions must be saved.", 0, factory.GetDatabaseCount(typeof(AccTransactionHeader)) - transactionCountBeforeExport);
			AssertEquals("Errors for all transactions must be shown.", 2, notifications.Events.Count(notification => notification.Type.GetType() == typeof(ErrorType)));

			txnHeaderCollection[1].PostDate = ZDateTime.Today.AddMonths(1);
			importer.OnlySaveDataWhenNoRecordsHaveErrors = false;

			transactionCountBeforeExport = factory.GetDatabaseCount(typeof(AccTransactionHeader));
			isOperetionSuccessful = importer.ExtractToDataAdapter(txnHeaderCollection, new NotificationBuffer());
			AssertEquals("ExtractToDataAdapter should return true.", true, isOperetionSuccessful);
			AssertNotNull("Imported Invoice", importer.ImportedInvoice);
			AssertEquals("Transactions without errors must be saved.", 31, factory.GetDatabaseCount(typeof(AccTransactionHeader)) - transactionCountBeforeExport);
		}
	}
}
