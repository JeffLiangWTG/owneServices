using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Moq;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile.Testing
{
	class TxnHeaderFlatFileDataImporterTest : TestCaseWithFactory
	{
		public void TestReturnValueFromExtractToDataAdapter()
		{
			AssertEquals("Should return false so it doesn't save", false, Importer.ExtractToDataAdapter(null, new NotificationBuffer()));
			AssertNull("Imported Invoice should not null", Importer.ImportedInvoice);
		}

		public void TestImportedInvoiceFromExtractToAdapter()
		{
			Xsd.TxnHeader newTxnHeader = CreateTransactionHeaderForTest();

			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			txnHeaderCollection.Add(newTxnHeader);

			Importer.ExtractToDataAdapter(txnHeaderCollection, new NotificationBuffer());
			AssertNotNull("Imported Invoice", Importer.ImportedInvoice);
			AssertEquals("Invoice Invoice Ledger", newTxnHeader.Ledger.ToString(), Importer.ImportedInvoice.AH_Ledger.ToString());
			AssertEquals("Invoice Invoice Transaction Type", newTxnHeader.TxnType.ToString(), Importer.ImportedInvoice.AH_TransactionType.ToString());
			AssertEquals("Invoice Invoice Transaction Num", newTxnHeader.TxnNumber.ToString(), Importer.ImportedInvoice.AH_TransactionNum.ToString());

			Assert(
				"Factory should have context BusinessContext.AllowReopenJobWhenImporting",
				Importer.ImportedInvoice.Factory.HasContext(BusinessContext.AllowReopenJobWhenImporting)
			);
		}

		public void TestImportInvoiceUsingExtractToAdapterFails_TxnHeaderCollectionIsNull()
		{
			Xsd.TxnHeaderCollection txnHeaderCollection = null;
			var buffer = new NotificationBuffer();

			var isInvoiceImported = Importer.ExtractToDataAdapter(txnHeaderCollection, buffer);

			AssertEquals(false, isInvoiceImported);
			AssertContains("This file does not contain any transaction headers.", buffer.AsString);
		}

		public void TestImportInvoiceUsingExtractToAdapterFails_TxnHeaderCollectionIsEmpty()
		{
			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			var buffer = new NotificationBuffer();

			var isInvoiceImported = Importer.ExtractToDataAdapter(txnHeaderCollection, buffer);

			AssertEquals(false, isInvoiceImported);
			AssertContains("This file does not contain any transaction headers.", buffer.AsString);
		}

		public void TestImportInvoiceUsingExtractToAdapter_ForDifferentLedgerAndTxnTypeParameters()
		{
			const string invalidLedgerOrTxntypeMessage = "This transaction cannot be imported as the Ledger or Transaction Type is invalid.";

			var buffer = new NotificationBuffer();
			Xsd.TxnHeader newTxnHeader1 = CreateTransactionHeaderForTest();
			Xsd.TxnHeader newTxnHeader2 = CreateTransactionHeaderForTest();
			Xsd.TxnHeader newTxnHeader3 = CreateTransactionHeaderForTest();

			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection
			{
				newTxnHeader1,
				newTxnHeader2,
				newTxnHeader3
			};

			Assert("Precondition", IsTransactionHeaderContainsValidLedgerAndTxnType(newTxnHeader1) && IsTransactionHeaderContainsValidLedgerAndTxnType(newTxnHeader3));

			var txnNumber = 1000;
			var validLedgerAndTxnTypeResultCount = 0;
			var invalidLedgerOrTxnTypeResultCount = 0;

			foreach (var ledger in Enum.GetValues(typeof(Xsd.TxnLedgerType)))
			{
				foreach (var txnType in Enum.GetValues(typeof(Xsd.TxnType)))
				{
					buffer.Clear();

					newTxnHeader1.TxnNumber = (++txnNumber).ToString();
					newTxnHeader2.TxnNumber = (++txnNumber).ToString();
					newTxnHeader3.TxnNumber = (++txnNumber).ToString();

					newTxnHeader2.Ledger = (Xsd.TxnLedgerType)ledger;
					newTxnHeader2.TxnType = (Xsd.TxnType)txnType;

					var isInvoiceImported = Importer.ExtractToDataAdapter(txnHeaderCollection, buffer);

					if (IsTransactionHeaderContainsValidLedgerAndTxnType(newTxnHeader2))
					{
						validLedgerAndTxnTypeResultCount++;
						AssertEquals($"Valid Ledger and Transaction Type. Ledger: {newTxnHeader2.Ledger}, Transaction Type: {newTxnHeader2.TxnType}", true, isInvoiceImported);
						AssertNotContains(invalidLedgerOrTxntypeMessage, buffer.AsString);
					}
					else
					{
						invalidLedgerOrTxnTypeResultCount++;
						AssertEquals($"Invalid Ledger or Transaction Type. Ledger: {newTxnHeader2.Ledger}, Transaction Type: {newTxnHeader2.TxnType}", false, isInvoiceImported);
						AssertContains($"{invalidLedgerOrTxntypeMessage} Ledger: {newTxnHeader2.Ledger}, Transaction Type: {newTxnHeader2.TxnType}. Valid Ledgers are [AP, AR] and Valid Transaction Types are [ADJ, CRD, INV].", buffer.AsString);
					}
				}
			}

			Assert("Make sure all valid cases are tested.", validLedgerAndTxnTypeResultCount > 0);
			Assert("Make sure invalid cases are also tested.", invalidLedgerOrTxnTypeResultCount > 0);
		}

		public void TestImportedInvoiceFromExtractToAdapterWhenImportingSingleTranaction()
		{
			Xsd.TxnHeader newTxnHeader = CreateTransactionHeaderForTest();

			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			txnHeaderCollection.Add(newTxnHeader);

			Importer.ImportingSingleTransaction = true;
			Importer.ExtractToDataAdapter(txnHeaderCollection, new NotificationBuffer());
			AssertNotNull("Imported Invoice", Importer.ImportedInvoice);
			Assert("Should not have BusinessContext.ImportMultipleInvoicesWithSameTransformer as it is single import", !Importer.ImportedInvoice.HasContext(BusinessContext.ImportMultipleInvoicesWithSameTransformer));
			AssertEquals("Invoice.IsInDatabase", false, Importer.ImportedInvoice.IsInDatabase);
		}

		public void TestImportedInvoiceFromExtractToAdapterWhenImportingMultipleTranactions()
		{
			Xsd.TxnHeader newTxnHeader = CreateTransactionHeaderForTest();

			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			txnHeaderCollection.Add(newTxnHeader);

			Importer.ImportingSingleTransaction = false;
			Importer.ExtractToDataAdapter(txnHeaderCollection, new NotificationBuffer());
			AssertNotNull("Imported Invoice", Importer.ImportedInvoice);
			Assert("BusinessContext.ImportMultipleInvoicesWithSameTransformer should be removed after factory save", !Importer.ImportedInvoice.HasContext(BusinessContext.ImportMultipleInvoicesWithSameTransformer));
			AssertEquals("Invoice.IsInDatabase", true, Importer.ImportedInvoice.IsInDatabase);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportedInvoiceFromMultipleTransactionsWithInconsistantLineValue()
		{
			var helper = new TestObjectCreator(Factory);
			var consol = helper.CreateConsol();
			consol.JK_UniqueConsignRef = "C00001280";
			var shipment = helper.CreateShipment("S001", consol);
			var consolCost = helper.CreateConsolCost(consol, helper.FRT);
			consolCost.E6_OSCostAmount = 500m;
			consolCost.E6_ApportionmentMethod = "GWT";
			AssertEquals(1, consolCost.ApportionmentCharges.Count);
			Factory.Save();

			string testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APInvoiceInconsistantLocalAmount.csv";
			AssertNoExceptionThrown(() => Importer.ImportData(testFilePath, new NotificationBuffer(), SourceInfo.EmptySourceInfo));

			testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APInvoiceInconsistantTaxRate.csv";
			AssertNoExceptionThrown(() => Importer.ImportData(testFilePath, new NotificationBuffer(), SourceInfo.EmptySourceInfo));
		}

		public void TestMaxTransactionCountCreatedInFactoryDefaultValue()
		{
			AssertEquals(500, Importer.MaxTransactionCountCreatedInFactory);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 09, 04)]
		public void TestImportedInvoiceFromExtractToAdapterWhenImportingMultipleTranactions_WhenJobIsReadyForFinancialClosure_Error()
		{
			var mock = new Mock<IAccounting>();
			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false;

			TestImportedInvoiceFromExtractToAdapterWhenImportingMultipleTranactions_WhenJobIsReadyForFinancialClosure(
				mock, false, @$"
Processing Transaction: Transaction AP INV AALSHI 123456789ABC: 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Job Number: Cannot post this charge, because the job has Jobs Ready for Financial Closure status.
  This transaction has errors and was not imported


Saving the data to the database...");
		}

		void TestImportedInvoiceFromExtractToAdapterWhenImportingMultipleTranactions_WhenJobIsReadyForFinancialClosure(Mock<IAccounting> mock, bool importedInvoiceIsInDatabase, string expectedMessageContain)
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var mockRegistry = new Mock<IRegistry>();
			var mockRegistryItem = new Mock<IRegistryItem>();
			mock.SetupGet(d => d.Registry).Returns(mockRegistry.Object);
			
			var autoJRJHelperMock = new Mock<IAutoJobRevenueJournalHelper>();
			autoJRJHelperMock.Setup(x => x.IsAutoJRJEnabled).Returns(false);
			autoJRJHelperMock.Setup(x => x.IsExcludedFromAutoJRJ(It.IsAny<GlbBranch>(), It.IsAny<OrgHeader>(), It.IsAny<string>())).Returns(false);

			AccountingPeriodTestHelper period = new AccountingPeriodTestHelper(Factory);
			period.SetupPeriods();
			var buffer = new NotificationBuffer();
			bool importResult = false;

			using (ObjectFactory.Substitute(mock.Object))
			using (ObjectFactory.Substitute(autoJRJHelperMock.Object))
			{
				testObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
				var shipment = testObjectCreator.CreateShipment("S00001");
				var job = testObjectCreator.CreateJob(shipment, setCurrentDepartment: false);
				job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
				var charge = job.Charges.AddNew();
				charge.JR_AC = testObjectCreator.CC1.PK;
				charge.JR_OSCostAmt = 100m;
				Factory.Save();

				this.ReleaseFactory();
				Importer.RunExtraValidation = true;
				var testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APInvoiceForSHPJobType.csv";
				Assert("Precondition", job.IsReadyForFinancialClosure);
				AssertNoExceptionThrown(() => importResult = Importer.ImportData(testFilePath, buffer, SourceInfo.EmptySourceInfo));
				AssertEquals(importedInvoiceIsInDatabase, Importer.ImportedInvoice.IsInDatabase);
				AssertContains(expectedMessageContain, buffer.AsString);
			}
		}

		bool IsTransactionHeaderContainsValidLedgerAndTxnType(Xsd.TxnHeader txnHeader)
		{
			var validLedgerList = new List<Xsd.TxnLedgerType>()
			{
				Xsd.TxnLedgerType.AP,
				Xsd.TxnLedgerType.AR,
			};

			var validTxnTypesList = new List<Xsd.TxnType>()
			{
				Xsd.TxnType.ADJ,
				Xsd.TxnType.CRD,
				Xsd.TxnType.INV
			};

			return validLedgerList.Contains(txnHeader.Ledger) && validTxnTypesList.Contains(txnHeader.TxnType);
		}

		Xsd.TxnHeader CreateTransactionHeaderForTest()
		{
			Xsd.TxnHeader txnHeader = new Xsd.TxnHeader();
			txnHeader.Ledger = Xsd.TxnLedgerType.AP;
			txnHeader.TxnType = Xsd.TxnType.INV;
			txnHeader.TxnNumber = "00001234";

			return txnHeader;
		}

		#region Implementation

		TxnHeaderFlatFileDataImporterTestClass Importer;

		protected override void SetUp()
		{
			Importer = new TxnHeaderFlatFileDataImporterTestClass();
			base.SetUp();
		}

		internal class TxnHeaderFlatFileDataImporterTestClass : TxnHeaderFlatFileDataImporter
		{
			public new bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications)
			{
				return base.ExtractToDataAdapter(xsd, notifications);
			}
		}

		#endregion
	}
}
