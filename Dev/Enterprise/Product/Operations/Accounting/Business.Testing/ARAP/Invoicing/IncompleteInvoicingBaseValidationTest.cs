using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class IncompleteInvoicingBaseValidationTest : InvoicingBaseCommonValidationTest<IncompleteInvoicingBaseValidation>
	{
		[TestDate(2020, 3, 2, 00, 00, 0)]
		public void TestCheckAH_GovernmentAllocatedID()
		{
			Header.Factory.SetContext(BusinessContext.SavingIncompleteTransaction);
			AssertCheckAH_GovernmentAllocatedID_RunsValidation();
		}

		public void TestCheckAH_OH()
		{
			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			var validation = GetValidation(invoice);
			invoice.AH_OH = TestObjectCreator.Debtor.PK;
			validation.ValidateAH_OH();
			AssertHasError(invoice.AH_OHInfo, "Enter a valid Account.");
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			validation.ValidateAH_OH();
			AssertNoErrors(invoice.AH_OHInfo);
		}

		public void TestCheckAH_ExchangeRate()
		{
			var invoice = (InvoicingBase)Factory.New(InvoiceType);

			if (invoice.IsSettingLineExchangeRateSupported)
			{
				var line = (InvoicingLineBase)invoice.Lines.AddNew();

				line.AL_ExchangeRate = 0m;
				AssertHasErrors(line.AL_ExchangeRateInfo);

				line.AL_ExchangeRate = 1m;
				AssertNoErrors(line.AL_ExchangeRateInfo);

				line.AL_ExchangeRate = -0.5m;
				AssertHasErrors(line.AL_ExchangeRateInfo);

				line.AL_ExchangeRate = 1m;
				AssertNoErrors(line.AL_ExchangeRateInfo);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckAH_OSTotalAmount()
		{
			var newInvoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			newInvoice.Lines.AddNew();
			newInvoice.Lines[0].AL_OSExTaxAmount = -1234m;
			newInvoice.AH_OSTotalAmount = -1234;

			var testValidation = GetValidation(newInvoice);
			testValidation.ValidateAH_OSTotalAmount();
			Assert(newInvoice.AH_OSTotalAmountInfo.HasError("The sum of the transaction lines should be greater than zero."));
		}

		public void TestCheckAH_DueDate()
		{
			var newInvoice = Factory.NewWithValidTestData<APInvoice>();
			var now = ZDateTime.Now;

			newInvoice.AH_InvoiceDate = now;
			newInvoice.AH_DueDate = now.AddDays(-1);

			var testValidation = GetValidation(newInvoice);
			testValidation.ValidateAH_DueDate();
			AssertHasError(newInvoice.AH_DueDateInfo, "Due date should be after or equal to Invoice Date");

			newInvoice.AH_DueDate = now.AddDays(3);
			testValidation.ValidateAH_DueDate();
			AssertNoError(newInvoice.AH_DueDateInfo, "Due date should be after or equal to Invoice Date");
		}

		public void TestCheckExpectedInvoiceTotal()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.Lines.AddNew();
			invoice.ExpectedInvoiceTotal = 10;
			invoice.ValidateExpectedInvoiceTotal = true;

			var testValidation = GetValidation(invoice);
			testValidation.ValidateAll();
			AssertNoErrors(invoice.ExpectedInvoiceTotalInfo);
		}

		public void TestCheckExpectedInvoiceTaxTotal()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.Lines.AddNew();
			invoice.ExpectedInvoiceTaxTotal = 10;
			invoice.ValidateExpectedInvoiceTotal = true;

			var testValidation = GetValidation(invoice);
			testValidation.ValidateAll();
			AssertNoErrors(invoice.ExpectedInvoiceTaxTotalInfo);
		}

		public void TestCheckExpectedInvoiceExclTaxTotal()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.Lines.AddNew();
			invoice.ExpectedInvoiceExclTaxTotal = 10;
			invoice.ValidateExpectedInvoiceTotal = true;

			var testValidation = GetValidation(invoice);
			testValidation.ValidateAll();
			AssertNoErrors(invoice.ExpectedInvoiceExclTaxTotalInfo);
		}

		public void TestCheckAH_TransactionNumForSelfBilled()
		{
			var invoice = Factory.New<APInvoice>();
			var validation = GetValidation(invoice);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			validation.ValidateAH_TransactionNum();
			AssertHasError(invoice.AH_TransactionNumInfo, "Please enter a " + invoice.AH_TransactionNumInfo.Description + ".");

			invoice.IsSelfBillingInvoice = true;
			validation.ValidateAH_TransactionNum();
			AssertNoErrors(invoice.AH_TransactionNumInfo);
		}

		public void TestCheckAH_TransactionNumForSavedInvoice_Standard()
		{
			AssertCheckAH_TransactionNumForSavedInvoice(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today,
				ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		public void TestCheckAH_TransactionNumForSavedInvoice_Calendar()
		{
			AssertCheckAH_TransactionNumForSavedInvoice(
				AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today,
				new ZDateTime(ZDateTime.Today.Year + 1, 1, 1));
		}

		void AssertCheckAH_TransactionNumForSavedInvoice(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				var savedInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1, 10, 0, 10, 0, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
				savedInvoice1.AH_InvoiceDate = invoiceDate;
				savedInvoice1.SaveAsIncomplete();
				AssertEquals("Precondition: savedInvoice1.IsInDatabase", true, savedInvoice1.IsInDatabase);
				AssertEquals("Precondition: savedInvoice1.AH_Ledger", LedgerTypes.IncompleteTransactions, savedInvoice1.AH_Ledger);

				var savedInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1, 10, 0, 10, 0, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
				savedInvoice2.SaveAsIncomplete();
				AssertEquals("Precondition: savedInvoice2.IsInDatabase", true, savedInvoice2.IsInDatabase);
				AssertEquals("Precondition: savedInvoice2.AH_Ledger", LedgerTypes.IncompleteTransactions, savedInvoice2.AH_Ledger);

				var validation = GetValidation(savedInvoice2);
				validation.ValidateAH_TransactionNum();
				AssertNoErrors(savedInvoice2.AH_TransactionNumInfo);

				savedInvoice2.AH_TransactionNum = savedInvoice1.AH_TransactionNum;
				validation.ValidateAH_TransactionNum();
				AssertHasError(savedInvoice2.AH_TransactionNumInfo, "The transaction number is already in use by Incomplete Transaction. Please select another one.");

				savedInvoice2.AH_InvoiceDate = invoiceDate2;
				validation.ValidateAH_TransactionNum();
				AssertHasError(savedInvoice2.AH_TransactionNumInfo, "The transaction number is already in use by Incomplete Transaction. Please select another one.");
			}
		}

		public void TestCheckAH_TransactionNum_Standard()
		{
			AssertCheckAH_TransactionNum(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today,
				ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths)
			);
		}

		public void TestCheckAH_TransactionNum_Calendar()
		{
			AssertCheckAH_TransactionNum(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today,
				new ZDateTime(ZDateTime.Today.Year + 1, 1, 1)
			);
		}

		void AssertCheckAH_TransactionNum(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				var savedInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1, 10, 0, 10, 0, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
				savedInvoice1.AH_InvoiceDate = invoiceDate;
				savedInvoice1.SaveAsIncomplete();

				AssertEquals("Precondition: savedInvoice1.IsInDatabase", true, savedInvoice1.IsInDatabase);
				AssertEquals("Precondition: savedInvoice1.AH_Ledger", LedgerTypes.IncompleteTransactions, savedInvoice1.AH_Ledger);

				var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1, 10, 0, 10, 0, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
				var validation = GetValidation(invoice);
				validation.ValidateAH_TransactionNum();
				AssertNoErrors(invoice.AH_TransactionNumInfo);

				invoice.AH_TransactionNum = savedInvoice1.AH_TransactionNum;
				validation.ValidateAH_TransactionNum();
				AssertHasError(invoice.AH_TransactionNumInfo, "The transaction number is already in use by Incomplete Transaction. Please select another one.");

				invoice.AH_InvoiceDate = invoiceDate2;
				validation.ValidateAH_TransactionNum();
				AssertHasError(invoice.AH_TransactionNumInfo, "The transaction number is already in use by Incomplete Transaction. Please select another one.");
			}
		}

		public void TestCheckIsSelfBillingInvoice_TransactionCount()
		{
			APInvoice invoice;

			TestObjectCreator.AALSHI.CompanyData.OB_APCostsSelfBilled = true;

			for (byte i = 1; i < byte.MaxValue; i++)
			{
				invoice = Factory.NewWithValidTestData<APInvoice>();
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				invoice.IsSelfBillingInvoice = true;
				invoice.MoveToIncompleteLedger();
				Factory.Save();
			}

			invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.IsSelfBillingInvoice = true;
			invoice.MoveToIncompleteLedger();
			var validation = GetValidation(invoice);
			validation.ValidateIsSelfBillingInvoice();
			AssertNoErrors(invoice.IsSelfBillingInvoiceInfo);

			Factory.Save();

			invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.IsSelfBillingInvoice = true;
			invoice.MoveToIncompleteLedger();
			validation = GetValidation(invoice);
			validation.ValidateIsSelfBillingInvoice();
			AssertHasError(invoice.IsSelfBillingInvoiceInfo,
				$@"The maximum supported number of Incomplete Self Billing Invoices ({byte.MaxValue}) have already been entered into the system.
You cannot create any more Incomplete Self Billing Invoices at this time.");
		}

		public void TestBranchDepartmentCombinationValidation_UAInvoiceUAInvoiceLine()
		{
			var header = Factory.NewWithValidTestData<UAInvoice>();
			var line = Factory.NewWithValidTestData<UAInvoiceLine>();
			line.AL_AH = header.PK;
			Factory.Save();

			var headerValidation = GetValidation(header);
			var lineValidation = new UAInvoiceLineValidation(line);

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObjInDatabase(Factory, header,
				() => { headerValidation.ValidateAH_GE(); }, header.AH_GEInfo);

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObjInDatabase(Factory, line,
				() => { lineValidation.ValidateAL_GE(); }, line.AL_GEInfo);
		}

		public void TestClearValidatedConsolCost()
		{
			var consol = TestObjectCreator.CreateConsol();
			var forwardingShipment = TestObjectCreator.CreateShipment("S00008000", consol);
			TestObjectCreator.CreateJob(forwardingShipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC12, 250m);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			var importer = new InvoicingBaseConsolCostImporter(new BusinessObjectFactory(), originatingCost, invoice);
			importer.ImportCostsIntoCosting(new BusinessObject[] { consolCost });
			AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);
			invoice.ImportAllApportionmentsFromCosting();
			var line = invoice.Lines[0];

			var chargeCodeInNewFactory = new BusinessObjectFactory().Load<AccChargeCode>(TestObjectCreator.CC12.PK);
			chargeCodeInNewFactory.AC_IsActive = false;
			chargeCodeInNewFactory.Factory.Save();

			var invoiceValidation = GetValidation(invoice);
			var lineValidation = new InvoicingLineBaseValidation(line);
			ValidateInvoiceAndLine();

			var errorMessage = "The related consol cost is invalid, please fix the following errors in the consol cost from which this line was apportioned:\r\nCharge Code: This Charge Code is inactive - it may not be used.\r\nCharge Code: Enter a valid Charge Code.\r\n";
			AssertHasRowError(line, errorMessage);

			chargeCodeInNewFactory.AC_IsActive = true;
			chargeCodeInNewFactory.Factory.Save();

			ValidateInvoiceAndLine();
			AssertNoRowErrors(line);

			void ValidateInvoiceAndLine()
			{
				invoiceValidation.ValidateAll();
				lineValidation.ValidateAll();
			}
		}

		public void TestValidateComplianceSequenceNotNull()
		{
			var currCompany = Env.CurrentCompany.PK;
			var accMasterRegistry = AccountingMasterFilesRegistry.Instance;
			var errorMessage = ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage;

			var invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			AssertEquals(typeof(APInvoice), InvoiceType);

			using (accMasterRegistry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(currCompany, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				foreach (var dateRegValue in new[] {
					AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code,
					AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code
				})
				{
					using (accMasterRegistry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currCompany, Guid.Empty, Guid.Empty, dateRegValue))
					{
						invoice.AH_ComplianceSubType = ZString.Empty;

						((InvoiceBaseValidation)invoice.Validation).ValidateComplianceSequenceNotNull_ForTestOnly();
						if (dateRegValue == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code)
						{
							AssertNoRowError(invoice, errorMessage);
						}
						else
						{
							AssertHasRowError(invoice, errorMessage);
						}

						GetValidation(invoice).ValidateAll();
						AssertNoRowError("No error on compliance sequence", invoice, errorMessage);
					}
				}
			}
		}

		protected override IncompleteInvoicingBaseValidation GetValidation(TransactionHeader parent) =>
			new IncompleteInvoicingBaseValidation(parent as InvoicingBase);

		protected override Type InvoiceType => typeof(APInvoice);
	}
}
