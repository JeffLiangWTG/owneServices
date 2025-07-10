using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Accounting.Business.JobInvoicing.BranchLevelPostingHelper;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class InvoicingBaseTaxRecordParentTest : TestCaseWithFactory
	{
		[SuspendCriticalValidation]
		public void TestRunOnSavingOperations_InDb()
		{
			var chargeCreatorMock = new Mock<IChargeCreator>();
			objForTest.SubstituteChargeCreator_ForTestOnly(chargeCreatorMock.Object);
			var taxRecordParent = objForTest as ITaxRecordParent;
			taxRecordParent.AddTaxRecoveryLine(testObjectCreator.CC1.PK, ZGuid.Empty, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, "AUD", 10, ZDate.Today, ZGuid.Empty, ZString.Empty);
			AssertEquals("Postcondition: line count", 1, invoice.Lines.Count);

			var charge = Factory.New<Charge>();
			chargeCreatorMock.Setup(x => x.CreateChargeFromJobRelatedRevenueLine(It.IsAny<InvoicingLineBase>(), It.IsAny<InvoicingBase>(), It.IsAny<bool>())).Returns(charge);
			Factory.Save();
			chargeCreatorMock.Verify(x => x.CreateChargeFromJobRelatedRevenueLine(It.IsAny<InvoicingLineBase>(), It.IsAny<InvoicingBase>(), It.IsAny<bool>()), Times.Exactly(1));
			var expectedContext = NewChargeLoadActionOnPosting.RefreshChargesWhenPosted;
			Assert("Has context", Factory.HasContext(expectedContext));

			Assert("Precondition: invoice.IsInDatabase", invoice.IsInDatabase);
			chargeCreatorMock.Invocations.Clear();
			Factory.RemoveContext(expectedContext);
			objForTest.RunOnSavingOperations();
			chargeCreatorMock.Verify(x => x.CreateChargeFromJobRelatedRevenueLine(It.IsAny<InvoicingLineBase>(), It.IsAny<InvoicingBase>(), It.IsAny<bool>()), Times.Exactly(0));
			Assert("Has context after save", !Factory.HasContext(expectedContext));
		}

		public void TestRunOnSavingOperations()
		{
			var chargeCreatorMock = new Mock<IChargeCreator>();
			objForTest.SubstituteChargeCreator_ForTestOnly(chargeCreatorMock.Object);
			var taxRecordParent = objForTest as ITaxRecordParent;
			AssertEquals("Precondition: line count", 0, invoice.Lines.Count);
			invoice.Lines.AddNew();
			taxRecordParent.AddTaxRecoveryLine(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", 0, ZDate.Today, ZGuid.Empty, ZString.Empty);
			invoice.Lines.AddNew();
			invoice.Lines.AddNew();
			taxRecordParent.AddTaxRecoveryLine(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", 0, ZDate.Today, ZGuid.Empty, ZString.Empty);
			AssertEquals("Postcondition: line count", 5, invoice.Lines.Count);

			var passedLines = new List<InvoicingLineBase>();
			InvoicingBase passedInvoice = null;
			chargeCreatorMock.Setup(x => x.CreateChargeFromJobRelatedRevenueLine(It.IsAny<InvoicingLineBase>(), It.IsAny<InvoicingBase>(), It.IsAny<bool>())).Returns(() => null)
				.Callback((InvoicingLineBase line, InvoicingBase invoice, bool isTaxRecoveryLine) =>
				{
					passedLines.Add(line);
					passedInvoice = invoice;
				});
			objForTest.RunOnSavingOperations();
			chargeCreatorMock.Verify(x => x.CreateChargeFromJobRelatedRevenueLine(It.IsAny<InvoicingLineBase>(), It.IsAny<InvoicingBase>(), It.IsAny<bool>()), Times.Exactly(2));
			AssertContainsExactElementsInAnyOrder("passedLines", new InvoicingLineBase[] { invoice.Lines[1], invoice.Lines[4] }, passedLines);
			AssertEquals("passedInvoice", invoice, passedInvoice);
			var expectedContext = NewChargeLoadActionOnPosting.RefreshChargesWhenPosted;
			Assert("Has context when charge is not created", !Factory.HasContext(expectedContext));

			var charge = Factory.New<Charge>();
			passedLines.Clear();
			passedInvoice = null;
			chargeCreatorMock.Invocations.Clear();
			chargeCreatorMock.Setup(x => x.CreateChargeFromJobRelatedRevenueLine(It.IsAny<InvoicingLineBase>(), It.IsAny<InvoicingBase>(), It.IsAny<bool>())).Returns(() => passedInvoice == null ? charge : null)
				.Callback((InvoicingLineBase line, InvoicingBase invoice, bool isTaxRecoveryLine) =>
				{
					passedLines.Add(line);
					passedInvoice = invoice;
				});
			objForTest.RunOnSavingOperations();
			chargeCreatorMock.Verify(x => x.CreateChargeFromJobRelatedRevenueLine(It.IsAny<InvoicingLineBase>(), It.IsAny<InvoicingBase>(), It.IsAny<bool>()), Times.Exactly(2));
			AssertContainsExactElementsInAnyOrder("passedLines", new InvoicingLineBase[] { invoice.Lines[1], invoice.Lines[4] }, passedLines);
			AssertEquals("passedInvoice", invoice, passedInvoice);
			Assert("Has context when charge is created", Factory.HasContext(expectedContext));
		}

		public void TestIsTaxRecoveryLine()
		{
			var taxRecordParent = objForTest as ITaxRecordParent;
			AssertEquals("Precondition: line count", 0, invoice.Lines.Count);
			invoice.Lines.AddNew();
			AssertEquals("Postcondition: line count", 1, invoice.Lines.Count);
			taxRecordParent.AddTaxRecoveryLine(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", 0, ZDate.Today, ZGuid.Empty, ZString.Empty);
			invoice.Lines.AddNew();
			invoice.Lines.AddNew();
			taxRecordParent.AddTaxRecoveryLine(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", 0, ZDate.Today, ZGuid.Empty, ZString.Empty);
			AssertEquals("Postcondition: line count", 5, invoice.Lines.Count);
			var lineArray = invoice.Lines.Cast<InvoicingLineBase>().ToArray();

			var deletedLineIndex = new HashSet<int> { 1, 4 };
			for (int i = 0; i < 5; i++)
			{
				AssertEquals($"line {i} IsTaxRecoveryLine", deletedLineIndex.Contains(i), objForTest.IsTaxRecoveryLine(lineArray[i]));
			}
		}

		public void TestInvoicingBaseTaxRecordParent_SetTransactionHeaderBranch()
		{
			var expectedBranchPK = testObjectCreator.CreateBranch("CB1", GlbCompany.CurrentCompany).PK;
			var taxRecordParent = objForTest as ITaxRecordParent;
			invoice.AH_GB = expectedBranchPK;
			invoice.Lines.AddNew();

			var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
			var branchLevelPostingHelperMock = new Mock<IBranchLevelPostingHelper>();
			ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object);

			mockIAccountingDependencyFactory.Setup(x => x.GetBranchLevelPostingHelper()).Returns(branchLevelPostingHelperMock.Object);

			var initialBranchPKValueInPassedTransaction = ZGuid.Empty;
			branchLevelPostingHelperMock.Setup(x => x.SetTransactionHeaderBranch(invoice, InvoiceProcessingLevelIsAllowingToResetBranch.TaxTransactionCalculation, It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()))
				.Callback<TransactionHeaderWithLines, InvoiceProcessingLevelIsAllowingToResetBranch, ITransactionBranchCalculationDataProviderFromJobCharge>(
					(transaction, invoiceLevel, charges) =>
					{
						initialBranchPKValueInPassedTransaction = transaction.AH_GB;
					}
				);

			taxRecordParent.SetTransactionHeaderBranch();

			AssertEquals("initialBranchPKValueInPassedTransaction: ", expectedBranchPK, initialBranchPKValueInPassedTransaction);
			branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(It.IsAny<InvoicingBase>(), It.IsAny<InvoiceProcessingLevelIsAllowingToResetBranch>(), It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()), Times.Once);
			branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(invoice, InvoiceProcessingLevelIsAllowingToResetBranch.TaxTransactionCalculation, It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()));
		}

		public void TestDeleteAllAddedTaxRecoveryLines()
		{
			var taxRecordParent = objForTest as ITaxRecordParent;
			AssertEquals("Precondition: line count", 0, invoice.Lines.Count);
			invoice.Lines.AddNew();
			AssertEquals("Postcondition: line count", 1, invoice.Lines.Count);
			taxRecordParent.DeleteAllAddedTaxRecoveryLines();
			AssertEquals("line count after delete before adding line", 1, invoice.Lines.Count);
			taxRecordParent.AddTaxRecoveryLine(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", 0, ZDate.Today, ZGuid.Empty, ZString.Empty);
			invoice.Lines.AddNew();
			invoice.Lines.AddNew();
			taxRecordParent.AddTaxRecoveryLine(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "", 0, ZDate.Today, ZGuid.Empty, ZString.Empty);
			AssertEquals("Postcondition: line count", 5, invoice.Lines.Count);
			var lineArray = invoice.Lines.ToArray();
			taxRecordParent.DeleteAllAddedTaxRecoveryLines();
			AssertEquals("line count after first delete after adding lines", 3, invoice.Lines.Count);

			var deletedLineIndex = new HashSet<int> { 1, 4 };
			for (int i = 0; i < 5; i++)
			{
				AssertEquals($"line {i} IsDeleted", deletedLineIndex.Contains(i), lineArray[i].IsDeleted);
			}

			taxRecordParent.DeleteAllAddedTaxRecoveryLines();
			AssertEquals("line count after second delete after adding lines", 3, invoice.Lines.Count);
		}

		public void TestAddTaxRecoveryLine()
		{
			var taxRecordParent = objForTest as ITaxRecordParent;
			var expectedChargePK = testObjectCreator.CC3.PK;
			var expectedJobPK = testObjectCreator.Job1.PK;
			var expectedBranchPK = Factory.New<GlbBranch>().PK;
			var expectedDepartmentPK = Factory.New<GlbDepartment>().PK;
			testObjectCreator.Job1.JH_GE = expectedDepartmentPK;
			var anotherDepartmentPK = Factory.New<GlbDepartment>().PK;
			var expectedOrganisationPK = Factory.New<OrgHeader>().PK;
			var expectedCurrencyCode = CurrencyCodes.UnitedKingdom;
			var expectedExRate = 2.5m;
			var expectedOSAmount = 250m;
			var expectedLineAmount = 100m;
			var expectedTaxDate = ZDate.BrettsBirthday;
			invoice.ExchangeRate.Currency = expectedCurrencyCode;
			invoice.ExchangeRate.Rate = expectedExRate;
			var expectedSupplyType = "SPT";

			AssertEquals("Precondition: line count", 0, invoice.Lines.Count);
			ITaxableTransactionLine taxableLine = taxRecordParent.AddTaxRecoveryLine(expectedChargePK, expectedJobPK, expectedBranchPK, anotherDepartmentPK, expectedCurrencyCode, expectedLineAmount, expectedTaxDate, expectedOrganisationPK, expectedSupplyType);
			AssertEquals("line count", 1, invoice.Lines.Count);
			var line = invoice.Lines[0];
			AssertLineProperties();

			expectedJobPK = ZGuid.Empty;
			expectedOSAmount = 500m;
			expectedLineAmount = 200m;
			expectedOrganisationPK = ZGuid.Empty;
			expectedSupplyType = ZString.Empty;

			var prevTaxableLine = taxableLine;
			taxableLine = taxRecordParent.AddTaxRecoveryLine(expectedChargePK, expectedJobPK, expectedBranchPK, expectedDepartmentPK, expectedCurrencyCode, expectedLineAmount, expectedTaxDate, expectedOrganisationPK, expectedSupplyType);
			AssertEquals("line count", 2, invoice.Lines.Count);
			line = invoice.Lines[1];
			AssertNotEquals("taxableLine.PK", prevTaxableLine.PK, taxableLine.PK);
			AssertLineProperties();

			void AssertLineProperties()
			{
				AssertEquals("AL_PK", taxableLine.PK, line.PK);
				AssertEquals("AL_AC", expectedChargePK, line.AL_AC);
				AssertEquals("AL_JH", expectedJobPK, line.AL_JH);
				AssertEquals("AL_GB", expectedBranchPK, line.AL_GB);
				AssertEquals("AL_GE", expectedDepartmentPK, line.AL_GE);
				AssertEquals("AL_RX_NKTransactionCurrency", expectedCurrencyCode, line.AL_RX_NKTransactionCurrency);
				AssertEquals("AL_RX_NKTransactionCurrency", expectedExRate, line.AL_ExchangeRate);
				AssertEquals("AL_LineAmount", expectedOSAmount, line.AL_OSAmount);
				AssertEquals("AL_LineAmount", expectedLineAmount, line.AL_LineAmount);
				AssertEquals("AL_TaxDate", expectedTaxDate, line.AL_TaxDate);
				AssertEquals("AL_OH", expectedOrganisationPK, line.AL_OH);
				AssertEquals("AL_SupplyType", expectedSupplyType, line.AL_SupplyType);
			}
		}

		public void TestIsInDatabase()
		{
			var taxRecordParent = objForTest as ITaxRecordParent;
			Assert(!taxRecordParent.IsPosted);

			invoice.SaveAsIncomplete();
			Assert(!taxRecordParent.IsPosted);

			invoice.MoveFromIncompleteToPayableLedger();
			Assert(!taxRecordParent.IsPosted);

			Factory.Save();
			Assert(taxRecordParent.IsPosted);
		}

		public void TestAllPropertiesReferToUnderlyingInvoicingBase()
		{
			var taxRecordParent = objForTest as ITaxRecordParent;
			AssertEquals(invoice.Header, taxRecordParent.Org);
			AssertEquals(invoice.AH_Ledger, taxRecordParent.Ledger);

			invoice.AH_RX_NKTransactionCurrency = "XXX";
			AssertEquals("XXX", taxRecordParent.Currency);

			taxRecordParent.OSTaxAmount = 22M;
			taxRecordParent.LocalTaxAmount = 88M;
			AssertEquals(22M, taxRecordParent.OSTaxAmount);
			AssertEquals(88M, taxRecordParent.LocalTaxAmount);
			AssertEquals(22M, invoice.AH_OSTaxAmountOtherTaxes);
			AssertEquals(88M, invoice.AH_LocalTaxAmountOtherTaxes);

			AssertEquals(invoice.Factory, taxRecordParent.Factory);
			AssertEquals(invoice.Company, taxRecordParent.Company);
			AssertEquals(invoice.Department, taxRecordParent.Department);

			AssertEquals(0, taxRecordParent.GetLines().Count);
			var line1 = testObjectCreator.CreateInvoiceLine(invoice, 100m);
			AssertEquals(0, taxRecordParent.GetLines().Count);
			var line2 = testObjectCreator.CreateInvoiceLine(invoice, 100m);
			line2.AL_AC = testObjectCreator.FRT.PK;
			AssertContainsExactElementsInAnyOrder(new[] { line2 }.Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(l)), taxRecordParent.GetLines());
			var line3 = testObjectCreator.CreateInvoiceLine(invoice, 100m);
			line3.AL_AC = testObjectCreator.RevenueChargeCode.PK;
			AssertContainsExactElementsInAnyOrder(new[] { line2, line3 }.Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(l)), taxRecordParent.GetLines());
		}

		public void TestTaxRecordLedger()
		{
			var taxRecordParent = objForTest as ITaxRecordParent;

			AssertEquals(LedgerTypes.AccountsPayable, invoice.AH_Ledger);
			AssertEquals(invoice.AH_Ledger, taxRecordParent.Ledger);

			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			AssertEquals(invoice.AH_Ledger, taxRecordParent.Ledger);

			invoice.AH_Ledger = LedgerTypes.General;
			AssertEquals(invoice.AH_Ledger, taxRecordParent.Ledger);

			invoice.AH_Ledger = LedgerTypes.CashBook;
			AssertEquals(invoice.AH_Ledger, taxRecordParent.Ledger);

			invoice.AH_Ledger = LedgerTypes.JobCosting;
			AssertEquals(invoice.AH_Ledger, taxRecordParent.Ledger);

			invoice.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			AssertEquals(invoice.AH_Ledger, taxRecordParent.Ledger);

			invoice.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
			AssertEquals(invoice.AH_Ledger, taxRecordParent.Ledger);

			invoice.AH_Ledger = LedgerTypes.IncompleteTransactions;
			AssertNotEquals(invoice.AH_Ledger, taxRecordParent.Ledger);
			AssertEquals(LedgerTypes.AccountsPayable, taxRecordParent.Ledger);
		}

		public void TestBranch()
		{
			var branch1 = testObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var branch2 = testObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);
			invoice.AH_GB = branch1.PK;

			var taxRecordParent = objForTest as ITaxRecordParent;
			AssertNull("Precondition:", invoice.TaxBranch);
			AssertEquals(invoice.Branch, taxRecordParent.Branch);

			invoice.AH_GB_TaxBranch = branch2.PK;
			AssertNotNull("Precondition: ", invoice.TaxBranch);
			AssertEquals(invoice.TaxBranch, taxRecordParent.Branch);
		}

		public void TestIsTaxTransactionsCalculatedBeforePosting_RegisterTaxTransactionCollectionAsEditableChild()
		{
			var taxConfig = taxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			var taxRate = testObjectCreator.CreateTaxRate("TID", "TID Desc", 6);
			Factory.Save();

			var apInvoice = testObjectCreator.CreateInvoice(typeof(APInvoice));
			var line = testObjectCreator.CreateInvoiceLine(apInvoice, testObjectCreator.GLHeader1.PK, 100m);

			var taxTransaction = taxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters() { TransactionHeader = apInvoice, TaxConfiguration = taxConfig, TaxId = taxRate, TaxRate = (16, 1) });
			taxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransaction.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));
			TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(apInvoice).IsTaxTransactionsCalculatedBeforePosting = true;

			CombineAssertions(() =>
			{
				//The test to simulate the scenario that invoice has no error when tax transaction has no changes and no errors. Invoice Has errors when tax transaction fields modified i.e., has changes and errors.
				taxTransaction.HasChanges = false;
				Assert("Precondition:", !taxTransaction.HasChanges);
				Assert("Precondition:", !taxTransaction.HasErrors);
				Assert(!apInvoice.HasErrors);

				taxTransaction.ATT_Rate = 110;
				//The test verifies that, after modifying tax transaction editable fields, errors on the tax transactions if any, are identified on invoice level. So, a random error has been used to test here.
				//The below error is when tax transaction rate is greater than 100.
				Assert("Precondition:", taxTransaction.HasChanges);
				Assert("Precondition:", taxTransaction.HasErrors);
				AssertHasError("Precondition:", taxTransaction.ATT_RateInfo, "Rate must be less than 100%");

				Assert(apInvoice.HasChanges);
				Assert(apInvoice.HasErrors);
			});
		}

		public void TestIsTaxTransactionsCalculatedBeforePosting_OnlyAfterInvoicePosted()
		{
			objForTest.IsTaxTransactionsCalculatedBeforePosting = true;
			invoice.SaveAsIncomplete();
			Assert(!invoice.IsPosted);
			AssertEquals("IsOtherTaxesCalculatedBeforePosting before invoice is posted", true, objForTest.IsTaxTransactionsCalculatedBeforePosting);

			invoice.MoveFromIncompleteToPayableLedger();
			Factory.Save();
			Assert(invoice.IsPosted);
			AssertEquals("IsOtherTaxesCalculatedBeforePosting after invoice is posted", false, objForTest.IsTaxTransactionsCalculatedBeforePosting);
		}

		public void TestShouldCalculateOtherTaxes()
		{
			var mockITaxFrameworkConfigurationHelper = taxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();

			Assert("Is being created", invoice.IsBeingCreatedPostedAllocatedApprovedOrIncomplete);
			AssertEquals(TransactionTypes.Invoice, invoice.AH_TransactionType);
			Assert(!invoice.IsReversed);
			Assert(!objForTest.ShouldCalculateTaxTransactions);

			Assert(!objForTest.IsApplicableForTaxTransactions);
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			Assert(objForTest.IsApplicableForTaxTransactions);
			Assert(objForTest.ShouldCalculateTaxTransactions);

			invoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
			Assert(!objForTest.ShouldCalculateTaxTransactions);

			invoice.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			Assert(!objForTest.ShouldCalculateTaxTransactions);

			invoice.AH_TransactionType = TransactionTypes.CreditNotePendingAllocation;
			Assert(!objForTest.ShouldCalculateTaxTransactions);

			invoice.AH_TransactionType = TransactionTypes.UAInvoice;
			Assert(!objForTest.ShouldCalculateTaxTransactions);

			invoice.AH_TransactionType = TransactionTypes.UACreditNote;
			Assert(!objForTest.ShouldCalculateTaxTransactions);

			invoice.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			Assert(objForTest.ShouldCalculateTaxTransactions);

			invoice.AH_TransactionType = TransactionTypes.IncompleteCreditNote;
			Assert(objForTest.ShouldCalculateTaxTransactions);

			invoice.AH_TransactionType = TransactionTypes.IncompleteAdjustmentNote;
			Assert(!objForTest.ShouldCalculateTaxTransactions);

			invoice.AH_TransactionType = TransactionTypes.CreditNote;
			Assert(objForTest.ShouldCalculateTaxTransactions);

			Factory.Save();
			Assert(!objForTest.ShouldCalculateTaxTransactions);

			invoice = testObjectCreator.CreateInvoice(typeof(APInvoice));
			Assert("Is being created", invoice.IsBeingCreatedPostedAllocatedApprovedOrIncomplete);
			testObjectCreator.ReverseTransaction(invoice, out string message);
			AssertNullOrEmpty(message);
			Assert(invoice.IsReversed);
			Assert(!objForTest.ShouldCalculateTaxTransactions);
		}

		public void TestIsApplicableForTaxTransactions()
		{
			Assert(!objForTest.IsApplicableForTaxTransactions);

			var mockITaxFrameworkConfigurationHelper = taxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();

			AssertIsApplicableForTaxTransactions(true, true);
			AssertIsApplicableForTaxTransactions(false, false);

			void AssertIsApplicableForTaxTransactions(ZBool isActiveTaxConfiguration, ZBool expectedIsApplicableForTaxTransactions)
			{
				mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(isActiveTaxConfiguration);

				invoice.AH_TransactionType = TransactionTypes.CreditNote;
				AssertEquals(expectedIsApplicableForTaxTransactions, objForTest.IsApplicableForTaxTransactions);

				invoice.AH_TransactionType = TransactionTypes.Invoice;
				AssertEquals(expectedIsApplicableForTaxTransactions, objForTest.IsApplicableForTaxTransactions);

				invoice.AH_TransactionType = TransactionTypes.IncompleteInvoice;
				AssertEquals(expectedIsApplicableForTaxTransactions, objForTest.IsApplicableForTaxTransactions);

				invoice.AH_TransactionType = TransactionTypes.IncompleteCreditNote;
				AssertEquals(expectedIsApplicableForTaxTransactions, objForTest.IsApplicableForTaxTransactions);

				invoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
				Assert(!objForTest.IsApplicableForTaxTransactions);

				invoice.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
				Assert(!objForTest.IsApplicableForTaxTransactions);

				invoice.AH_TransactionType = TransactionTypes.CreditNotePendingAllocation;
				Assert(!objForTest.IsApplicableForTaxTransactions);

				invoice.AH_TransactionType = TransactionTypes.UAInvoice;
				Assert(!objForTest.IsApplicableForTaxTransactions);

				invoice.AH_TransactionType = TransactionTypes.UACreditNote;
				Assert(!objForTest.IsApplicableForTaxTransactions);

				invoice.AH_TransactionType = TransactionTypes.IncompleteAdjustmentNote;
				Assert(!objForTest.IsApplicableForTaxTransactions);
			}
		}

		public void TestShouldCalculateOtherTaxesForIncompleteInvoice()
		{
			var mockITaxFrameworkConfigurationHelper = taxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();

			Assert("Is being created", invoice.IsBeingCreatedPostedAllocatedApprovedOrIncomplete);
			AssertEquals(TransactionTypes.Invoice, invoice.AH_TransactionType);
			invoice.MakeAsIncomplete(out string message);
			AssertEquals(TransactionTypes.IncompleteInvoice, invoice.AH_TransactionType);

			Assert(!objForTest.IsApplicableForTaxTransactions);
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			Assert(objForTest.IsApplicableForTaxTransactions);
			Assert(objForTest.ShouldCalculateTaxTransactions);

			Factory.Save();
			Assert(objForTest.ShouldCalculateTaxTransactions);
		}

		public void TestOnTaxTransactionsCalculated_RaisedOnlyWhenValueHasChanged()
		{
			int count = 0;
			objForTest.OnOtherTaxesCalculatedBeforePosting_Changed += new EventHandler(OnTaxTransactionsCalculated_Listener);
			Assert(!objForTest.IsTaxTransactionsCalculatedBeforePosting);

			SetTaxTransactionsCalculatedAndAssertListenerCallbacks(false, 0, false);
			SetTaxTransactionsCalculatedAndAssertListenerCallbacks(true, 1, true);
			SetTaxTransactionsCalculatedAndAssertListenerCallbacks(true, 1, true);
			SetTaxTransactionsCalculatedAndAssertListenerCallbacks(false, 2, false);
			SetTaxTransactionsCalculatedAndAssertListenerCallbacks(true, 3, true);

			void OnTaxTransactionsCalculated_Listener(object sender, EventArgs e)
			{
				AssertEquals(objForTest, sender);
				AssertEquals(EventArgs.Empty, e);
				count++;
			}

			void SetTaxTransactionsCalculatedAndAssertListenerCallbacks(bool valueToSet, int expectedCount, bool expectedValue)
			{
				objForTest.IsTaxTransactionsCalculatedBeforePosting = valueToSet;
				AssertEquals("IsTaxTransactionsCalculated has been set", expectedValue, objForTest.IsTaxTransactionsCalculatedBeforePosting);
				AssertEquals("OnTaxTransactionsCalculated_Changed event was raised", expectedCount, count);
			}
		}

		protected override void SetUp()
		{
			testObjectCreator = new TestObjectCreator(Factory);
			taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory);
			invoice = testObjectCreator.CreateInvoice(typeof(APInvoice));
			objForTest = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
		}

		InvoicingBaseTaxRecordParent objForTest;
		InvoicingBase invoice;
		TestObjectCreator testObjectCreator;
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
	}
}
