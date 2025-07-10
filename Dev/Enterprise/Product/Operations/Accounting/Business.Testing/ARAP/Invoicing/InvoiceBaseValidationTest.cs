using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class InvoiceBaseValidationTest : InvoicingBaseCommonValidationTest<InvoiceBaseValidation>
	{
		[TestDate(2020, 3, 2, 00, 00, 0)]
		public void TestCheckAH_GovernmentAllocatedID()
		{
			AssertCheckAH_GovernmentAllocatedID_RunsValidation();
		}

		public void TestCheckOriginalTransactionReferenceWhenTaxRateIsInvalid()
		{
			if (InvoiceType == typeof(ARCreditNote))
			{
				using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00001001", TestObjectCreator.LocalCurrency, 1, TestObjectCreator.Debtor);
					var arInvoiceLine = TestObjectCreator.CreateARInvoiceLine(arInvoice, null, TestObjectCreator.FRT, TestObjectCreator.LocalCurrency, 1, "desc", 100);
					arInvoiceLine.AL_AT = Guid.Empty;
					Factory.Save();

					var arCreditNote = TestObjectCreator.CreateARCreditNote("00002002", TestObjectCreator.Debtor, null, null);
					arCreditNote.OriginalTransactionReference = arInvoice.PK;
					AssertNoErrors(arCreditNote.OriginalTransactionReferenceInfo);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckOriginalTransactionReferenceWhenNotAllLinesWithComplianceDocumentNumber()
		{
			var errorMsg = "Please create compliance document record and allocate compliance document number for the selected invoice before proceeding to raise a credit note referencing it.";
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate1.AT_Code = "TX1";
				taxRate1.AT_PostingGroupId = 0;

				var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate2.AT_Code = "TX2";
				taxRate2.AT_PostingGroupId = 1;

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00001001", TestObjectCreator.LocalCurrency, 1, TestObjectCreator.Debtor);
				var invoiceLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, TestObjectCreator.LocalCurrency, 1, "desc", 100);
				invoiceLine.AL_AT = taxRate1.PK;
				var invoiceLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.RevenueNoTaxChargeCode, TestObjectCreator.LocalCurrency, 1, "desc2", 101);
				invoiceLine2.AL_AT = taxRate2.PK;

				new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				Factory.Save();
				var header = invoice.GetTransactionGeneratedComplianceDocument();
				header.ADH_DocumentNumber = "TX00000001";

				var creditNote = TestObjectCreator.CreateARCreditNote("00001001", TestObjectCreator.Debtor, null, null);
				creditNote.OriginalTransactionReference = invoice.PK;

				AssertHasErrorContaining(creditNote.OriginalTransactionReferenceInfo, errorMsg);
			}
		}

		public void TestCheckOriginalTransactionReferenceWhenNotAllLinesWithComplianceDocumentRecordsCreated()
		{
			var errorMsg = "Please create compliance document record and allocate compliance document number for the selected invoice before proceeding to raise a credit note referencing it.";
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00001001", TestObjectCreator.LocalCurrency, 1, TestObjectCreator.Debtor);
				var invoiceLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, TestObjectCreator.LocalCurrency, 1, "desc", 100);
				invoiceLine.AL_AT = TestObjectCreator.CAP.PK;
				var invoiceLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, TestObjectCreator.LocalCurrency, 1, "desc2", 101);

				new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				Factory.Save();

				var creditNote = TestObjectCreator.CreateARCreditNote("00001001", TestObjectCreator.Debtor, null, null);
				creditNote.OriginalTransactionReference = invoice.PK;

				AssertHasErrorContaining(creditNote.OriginalTransactionReferenceInfo, errorMsg);
			}
		}

		public void TestCheckOriginalTransactionReferenceWhenNoLinesWithComplianceDocument()
		{
			var errorMsg = "Please create compliance document record and allocate compliance document number for the selected invoice before proceeding to raise a credit note referencing it.";
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00001001", TestObjectCreator.LocalCurrency, 1, TestObjectCreator.Debtor);
				var invoiceLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, TestObjectCreator.LocalCurrency, 1, "desc", 100);
				invoiceLine.AL_AT = TestObjectCreator.CAP.PK;
				Factory.Save();

				var creditNote = TestObjectCreator.CreateARCreditNote("00001001", TestObjectCreator.Debtor, null, null);
				creditNote.OriginalTransactionReference = invoice.PK;

				AssertHasErrorContaining(creditNote.OriginalTransactionReferenceInfo, errorMsg);
			}
		}

		public virtual void TestCheckOriginalTransactionReference()
		{
			AssertCheckOriginalTransactionReference(true, true, true, "AA0001", true, "00001001", true, true);
			AssertCheckOriginalTransactionReference(true, true, true, "AA0001", true, "00001013", true, false);
			AssertCheckOriginalTransactionReference(true, true, true, "AA0001", true, "00001014", false, true);
			AssertCheckOriginalTransactionReference(true, true, true, "AA0001", true, "00001015", false, false);

			AssertCheckOriginalTransactionReference(true, true, false, "", true, "00001002", true, true);
			AssertCheckOriginalTransactionReference(true, true, false, "", true, "00001016", true, false);
			AssertCheckOriginalTransactionReference(true, true, false, "", true, "00001017", false, true);
			AssertCheckOriginalTransactionReference(true, true, false, "", true, "00001018", false, false);

			AssertCheckOriginalTransactionReference(false, true, true, "AA0002", true, "00001004",false, false);
			AssertCheckOriginalTransactionReference(false, true, false, "", true, "00001005", false, false);
			AssertCheckOriginalTransactionReference(false, false, false, "", true, "00001006", false, false);

			AssertCheckOriginalTransactionReference(true, true, true, "AA0001", false, "00001007", false, false);
			AssertCheckOriginalTransactionReference(true, true, false, "", false, "00001008", false, false);
			AssertCheckOriginalTransactionReference(true, false, false, "", false, "00001009", false, false);

			AssertCheckOriginalTransactionReference(false, true, true, "AA0002", false, "00001010", false, false);
			AssertCheckOriginalTransactionReference(false, true, false, "", false, "00001011", false, false);
			AssertCheckOriginalTransactionReference(false, false, false, "", false, "000010012", false, false);
		}

		void AssertCheckOriginalTransactionReference(bool configurationEnabled, bool createDocument, bool hasDocumentNumber, string documentNumber, bool isAR, string invoiceNumber, bool isTXE, bool isEReportSUC)
		{
			var firstErrorMsg = "Please create compliance document record and allocate compliance document number for the selected invoice before proceeding to raise a credit note referencing it.";
			var secondErrorMsg = "Credit Note can only be created after the Original TXE documents have been successfully uploaded (i.e., E-Reporting Status = SUC)."; 

			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurationEnabled))
			{
				var invoice = isAR
					? TestObjectCreator.CreateARInvoice<ARInvoice>(invoiceNumber, TestObjectCreator.LocalCurrency, 1, TestObjectCreator.Debtor) as InvoicingBase
					: TestObjectCreator.CreateAPInvoice<APInvoice>(invoiceNumber, TestObjectCreator.LocalCurrency, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Creditor1);
				invoice.Lines.RemoveAndDeleteAll();
				var invoiceLine = isAR
					? TestObjectCreator.CreateARInvoiceLine(invoice as ARInvoice, null, TestObjectCreator.FRT, TestObjectCreator.LocalCurrency, 1, "desc", 100) as InvoicingLineBase
					: TestObjectCreator.CreateAPInvoiceLine(invoice as APInvoice, null, TestObjectCreator.OverheadChargeCode, TestObjectCreator.LocalCurrency, 1, "desc", 100);

				invoiceLine.AL_AT = TestObjectCreator.CAP.PK;
				Factory.Save();

				if (createDocument)
				{
					new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
					Factory.Save();

					if (hasDocumentNumber)
					{
						var header = invoice.GetTransactionGeneratedComplianceDocument();
						header.ADH_DocumentNumber = documentNumber;
					}
					if (isTXE)
					{
						var header = invoice.GetTransactionGeneratedComplianceDocument();
						header.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
						if (isEReportSUC)
						{
							var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
							pivot.AIP_ParentID = header.PK;
							pivot.AIP_Status = EInvoicingPivotState.Succeed;
							pivot.AIP_ParentTableCode = AccComplianceDocumentHeaderSchema.Constants.Prefix;
							Factory.Save();
						}
					}
				}

				var creditNote = isAR
					? TestObjectCreator.CreateARCreditNote(invoiceNumber, TestObjectCreator.Debtor, null, null) as CreditNote
					: TestObjectCreator.CreateAPCreditNote(invoiceNumber, TestObjectCreator.Creditor1, TestObjectCreator.LocalCurrency, 1m, "desc");
				creditNote.OriginalTransactionReference = invoice.PK;

				if (configurationEnabled)
				{
					if (isAR)
					{
						if (createDocument)
						{
							if (hasDocumentNumber)
							{
								AssertNoErrorContaining(creditNote.OriginalTransactionReferenceInfo, firstErrorMsg);
							}
							else
							{
								AssertHasErrorContaining(creditNote.OriginalTransactionReferenceInfo, firstErrorMsg);
							}

							if (isTXE)
							{
								if (isEReportSUC)
								{
									AssertNoErrorContaining(creditNote.OriginalTransactionReferenceInfo, secondErrorMsg);
								}
								else
								{
									AssertHasErrorContaining(creditNote.OriginalTransactionReferenceInfo, secondErrorMsg);
								}
							}
						}
						else
						{
							AssertHasErrorContaining(creditNote.OriginalTransactionReferenceInfo, firstErrorMsg);
						}
					}
					else
					{
						AssertNoErrorContaining(creditNote.OriginalTransactionReferenceInfo, firstErrorMsg);
					}
				}
				else
				{
					if (createDocument)
					{
						AssertNoErrorContaining(creditNote.OriginalTransactionReferenceInfo, firstErrorMsg);
					}
					else
					{
						AssertNoErrorContaining(creditNote.OriginalTransactionReferenceInfo, firstErrorMsg);
					}
				}
			}
		}

		[TestDate(2017, 2, 1)]
		public void TestCheckAH_PostDateNotInPast()
		{
			if (InvoiceType == typeof(ARInvoice) || InvoiceType == typeof(ARCreditNote) || InvoiceType == typeof(ARAdjustmentNote))
			{
				new AccountingPeriodTestHelper().PostPeriodsForEntireYear(2017);
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "MTH");
				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2017, 1, 1));

				var header = (TransactionHeader)Factory.New(InvoiceType);
				var invoice = (InvoicingBase)Factory.New(InvoiceType);
				header.AH_PostDate = ZDateTime.Today;
				header.AH_Ledger = LedgerTypes.AccountsReceivable;
				header.AH_TransactionType = invoice.AH_TransactionType;

				Assert("Precondition", header.AH_PostDate < ZDateTime.Today);

				header.Validation.ValidateAH_PostDate();

				AssertEquals("Invoice date should be the last day of previous month.", header.AH_InvoiceDate, new DateTime(2017, 1, 31));
				AssertEquals("Post date should be the last day of previous month.", header.AH_PostDate, new DateTime(2017, 1, 31));
				AssertNoErrors("Post date should not have any error.", header.AH_PostDateInfo);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckValidateExpectedInvoiceTotal()
		{
			if (InvoiceType == typeof(ARInvoice) || InvoiceType == typeof(ARCreditNote) || InvoiceType == typeof(ARAdjustmentNote))
			{
				Assert(true);
			}
			else
			{
				bool originalRegistryValue = AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.Value;
				try
				{
					AssertValidateExpectedInvoiceTotal(true, true, true, true);
					AssertValidateExpectedInvoiceTotal(true, true, true, false);
					AssertValidateExpectedInvoiceTotal(true, true, false, true);
					AssertValidateExpectedInvoiceTotal(true, true, false, false);
					AssertValidateExpectedInvoiceTotal(true, false, true, true);
					AssertValidateExpectedInvoiceTotal(true, false, true, false);
					AssertValidateExpectedInvoiceTotal(true, false, false, true);
					AssertValidateExpectedInvoiceTotal(true, false, false, false);
					AssertValidateExpectedInvoiceTotal(false, true, true, true);
					AssertValidateExpectedInvoiceTotal(false, true, true, false);
					AssertValidateExpectedInvoiceTotal(false, true, false, true);
					AssertValidateExpectedInvoiceTotal(false, true, false, false);
					AssertValidateExpectedInvoiceTotal(false, false, true, true);
					AssertValidateExpectedInvoiceTotal(false, false, true, false);
					AssertValidateExpectedInvoiceTotal(false, false, false, true);
					AssertValidateExpectedInvoiceTotal(false, false, false, false);
				}
				finally
				{
					AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalRegistryValue);
				}
			}
		}

		void AssertValidateExpectedInvoiceTotal(bool value, bool defaultRegistryValue, bool enableValidationOfValidateExpectedInvoiceTotal, bool securityIsAllowed)
		{
			InvoicingBase invoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			bool shouldHaveError = enableValidationOfValidateExpectedInvoiceTotal && !securityIsAllowed && value != defaultRegistryValue;
			AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultRegistryValue);
			Env.Security.AllowAPAdjustNoteChangeDefaultExpectedTotalValue.IsAllowed = securityIsAllowed;
			Env.Security.AllowAPCreditNoteChangeDefaultExpectedTotalValue.IsAllowed = securityIsAllowed;
			Env.Security.AllowAPInvoiceChangeDefaultExpectedTotalValue.IsAllowed = securityIsAllowed;

			if (enableValidationOfValidateExpectedInvoiceTotal)
			{
				invoice.EnableValidationOfValidateExpectedInvoiceTotal();
			}
			invoice.ValidateExpectedInvoiceTotal = value;
			string securityError = @"You do not have sufficient security rights to modify the 'Expected Total' tick box.
Please contact your system administrator for right to modify this field.";
			if (shouldHaveError)
			{
				AssertHasErrorContaining(invoice.ValidateExpectedInvoiceTotalInfo, securityError);
			}
			else
			{
				AssertNoErrorContaining(invoice.ValidateExpectedInvoiceTotalInfo, securityError);
			}
		}

		#region Invoice with Approval Request unique TransactionNumber tests

		public void TestUniqueTransactionNumberForNewInvoiceWithApprovalRequest_Standard()
		{
			AssertUniqueTransactionNumberForNewInvoiceWithApprovalRequest(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD);
		}

		public void TestUniqueTransactionNumberForNewInvoiceWithApprovalRequest_Calendar()
		{
			AssertUniqueTransactionNumberForNewInvoiceWithApprovalRequest(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL);
		}

		void AssertUniqueTransactionNumberForNewInvoiceWithApprovalRequest(string allowDuplicateInvoiceNumberRule)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				if (InvoiceType != typeof(APInvoice) && InvoiceType != typeof(APCreditNote))
				{
					Assert("Approval request can't be created for this types", true);
					return;
				}

				var invoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
				invoice.SetContext(APInvoiceChargesApprovalRequest.Context.Editing);

				AssertTransactionNumForAPInvNumAlreadyExist(invoice, "INV1");
				AssertTransactionNumForUAInvNumAlreadyExist(invoice, "INV2");
				AssertTransactionNumForPAInvNumAlreadyExist(invoice, "INV3");
				AssertTransactionNumForINInvNumAlreadyExist(invoice, "INV4");
			}
		}

		public void TestUniqueTransactionNumberForExistingInvoiceWithApprovalRequest_Standard()
		{
			AssertUniqueTransactionNumberForExistingInvoiceWithApprovalRequest(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD);
		}

		public void TestUniqueTransactionNumberForExistingInvoiceWithApprovalRequest_Calendar()
		{
			AssertUniqueTransactionNumberForExistingInvoiceWithApprovalRequest(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL);
		}

		void AssertUniqueTransactionNumberForExistingInvoiceWithApprovalRequest(string allowDuplicateInvoiceNumberRule)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				if (InvoiceType != typeof(APInvoice) && InvoiceType != typeof(APCreditNote))
				{
					Assert("Approval request can't be created for this types", true);
					return;
				}

				var invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest(InvoiceType, TestObjectCreator.Creditor1, 10);
				TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoice);

				AssertTransactionNumForAPInvNumAlreadyExist(invoice, "INV1");
				AssertTransactionNumForUAInvNumAlreadyExist(invoice, "INV2");
				AssertTransactionNumForPAInvNumAlreadyExist(invoice, "INV3");
				AssertTransactionNumForINInvNumAlreadyExist(invoice, "INV4");
			}
		}

		public void TestUniqueTransactionNumberForNewInvoiceWhereApprovalRequestWillBeCreated_Standard()
		{
			AssertUniqueTransactionNumberForNewInvoiceWhereApprovalRequestWillBeCreated(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD);
		}

		public void TestUniqueTransactionNumberForNewInvoiceWhereApprovalRequestWillBeCreated_Calendar()
		{
			AssertUniqueTransactionNumberForNewInvoiceWhereApprovalRequestWillBeCreated(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL);
		}

		void AssertUniqueTransactionNumberForNewInvoiceWhereApprovalRequestWillBeCreated(string allowDuplicateInvoiceNumberRule)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				if (InvoiceType != typeof(APInvoice) && InvoiceType != typeof(APCreditNote))
				{
					Assert("Approval request can't be created for this types", true);
					return;
				}

				var invoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
				invoice.SetContext(APInvoiceChargesApprovalRequest.Context.Editing);

				AssertTransactionNumForAPInvNumAlreadyExist(invoice, "INV1");
				AssertTransactionNumForUAInvNumAlreadyExist(invoice, "INV2");
				AssertTransactionNumForPAInvNumAlreadyExist(invoice, "INV3");
				AssertTransactionNumForINInvNumAlreadyExist(invoice, "INV4");
			}
		}

		void AssertTransactionNumForAPInvNumAlreadyExist(InvoicingBase invoiceWithApprovalRequest, string existingNumber)
		{
			AssertTransactionNumAlreadyExist<APInvoice, APCreditNote>(invoiceWithApprovalRequest, existingNumber, "The transaction number is already in use.");
		}

		void AssertTransactionNumForINInvNumAlreadyExist(InvoicingBase invoiceWithApprovalRequest, string existingNumber)
		{
			AssertTransactionNumAlreadyExist<APInvoice, APCreditNote>(invoiceWithApprovalRequest, existingNumber, "", isIncomplete: true);
		}

		void AssertTransactionNumForUAInvNumAlreadyExist(InvoicingBase invoiceWithApprovalRequest, string existingNumber)
		{
			AssertTransactionNumAlreadyExist<UAInvoice, UACreditNote>(invoiceWithApprovalRequest, existingNumber, "The transaction number is already in use by Unapproved Invoice.");
		}

		void AssertTransactionNumForPAInvNumAlreadyExist(InvoicingBase invoiceWithApprovalRequest, string existingNumber)
		{
			AssertTransactionNumAlreadyExist<TransactionPendingAllocation, TransactionPendingAllocation>(invoiceWithApprovalRequest, existingNumber, "The transaction number is already in use by Transaction Pending Allocation.");
		}

		void AssertTransactionNumAlreadyExist<ExistingInvoiceType, ExistingCreditNoteType>(InvoicingBase invoiceWithApprovalRequest, string existingNumber, string errorMessage, bool isIncomplete = false)
			where ExistingInvoiceType : TransactionHeader
			where ExistingCreditNoteType : TransactionHeader
		{
			var newFactory = new BusinessObjectFactory();
			TransactionHeader existingInvoice;
			if (InvoiceType == typeof(APCreditNote))
			{
				existingInvoice = newFactory.NewWithValidTestData<ExistingCreditNoteType>();
				existingInvoice.AH_OSExTaxAmount = -100;
			}
			else
			{
				existingInvoice = newFactory.NewWithValidTestData<ExistingInvoiceType>();
				existingInvoice.AH_OSExTaxAmount = 100;
			}
			existingInvoice.AH_TransactionNum = existingNumber;
			existingInvoice.AH_OH = TestOrg.PK;
			if (isIncomplete)
			{
				((InvoicingBase)existingInvoice).SaveAsIncomplete();
			}
			else
			{
				existingInvoice.Factory.Save();
			}

			var newInvoice = invoiceWithApprovalRequest;
			newInvoice.AH_TransactionNum = existingNumber;
			newInvoice.AH_OH = TestOrg.PK;
			var testValidation = GetValidation(newInvoice);

			testValidation.ValidateAH_TransactionNum();
			if (string.IsNullOrEmpty(errorMessage))
			{
				AssertNoErrors(newInvoice.AH_TransactionNumInfo);
			}
			else
			{
				AssertHasErrorContaining(newInvoice.AH_TransactionNumInfo, errorMessage);
			}
		}

		#endregion

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public virtual void TestCheckTransactionNumForAPInvNumAlreadyExist_Standard()
		{
			AssertCheckTransactionNumForAPInvNumAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today,
				ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public virtual void TestCheckTransactionNumForAPInvNumAlreadyExist_Calendar()
		{
			AssertCheckTransactionNumForAPInvNumAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today,
				new ZDateTime(ZDateTime.Today.Year + 1, 1, 1));
		}

		void AssertCheckTransactionNumForAPInvNumAlreadyExist(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				string existingAPInvoiceNum = "TESTAP5787";
				InvoicingBase existingInvoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
				existingInvoice.AH_TransactionNum = existingAPInvoiceNum;
				existingInvoice.AH_OH = TestOrg.PK;
				existingInvoice.AH_InvoiceDate = invoiceDate;

				Factory.Save();

				InvoicingBase newInvoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				newInvoice.AH_TransactionNum = existingAPInvoiceNum;
				newInvoice.AH_OH = TestOrg.PK;
				InvoiceBaseValidation testValidation = GetValidation(newInvoice);

				testValidation.ValidateAH_TransactionNum();
				AssertHasErrorContaining(newInvoice.AH_TransactionNumInfo, "The transaction number is already in use");

				newInvoice.AH_InvoiceDate = invoiceDate2;
				testValidation.ValidateAH_TransactionNum();
				if (newInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					AssertHasWarningContaining(newInvoice.AH_TransactionNumInfo, "This transaction number can be used.");
				}
				else
				{
					AssertHasErrorContaining(newInvoice.AH_TransactionNumInfo, "The transaction number is already in use");
				}
			}
		}

		public virtual void TestCheckAPTransactionNumForIncompleteInvNumAlreadyExist_Standard()
		{
			var date = new ZDateTime(2017, 01, 01);
			AssertCheckAPTransactionNumForIncompleteInvNumAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				date, date.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		public virtual void TestCheckAPTransactionNumForIncompleteInvNumAlreadyExist_Calendar()
		{
			var date = new ZDateTime(2017, 01, 01);
			AssertCheckAPTransactionNumForIncompleteInvNumAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				date, new ZDateTime(date.Year + 1, 1, 1));
		}

		void AssertCheckAPTransactionNumForIncompleteInvNumAlreadyExist(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2)
		{
			if (InvoiceType == typeof(APInvoice) || InvoiceType == typeof(APCreditNote) || InvoiceType == typeof(APAdjustmentNote))
			{
				using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
				{
					InvoicingBase existingINInvoice;
					InvoicingBase newInvoice;
					string existingAPInvoiceNum = "TESTAP1111";

					if (InvoiceType.IsSubclassOf(typeof(CreditNote)))
					{
						existingINInvoice = Factory.NewWithValidTestData<APCreditNote>();
					}
					else
					{
						existingINInvoice = Factory.NewWithValidTestData<APInvoice>();
					}

					existingINInvoice.AH_TransactionNum = existingAPInvoiceNum;
					existingINInvoice.AH_OH = TestOrg.PK;
					existingINInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
					existingINInvoice.AH_InvoiceDate = invoiceDate;
					existingINInvoice.SaveAsIncomplete();

					if (InvoiceType.IsSubclassOf(typeof(CreditNote)))
					{
						newInvoice = Factory.NewWithValidTestData<APCreditNote>();
					}
					else
					{
						newInvoice = Factory.NewWithValidTestData<APInvoice>();
					}

					newInvoice.AH_TransactionNum = existingAPInvoiceNum;
					newInvoice.AH_OH = TestOrg.PK;
					newInvoice.AH_InvoiceDate = existingINInvoice.AH_InvoiceDate;

					newInvoice.Validation.ValidateAH_TransactionNum();
					AssertHasErrorContaining(newInvoice.AH_TransactionNumInfo, "The transaction number is already in use by Incomplete Transaction.");

					newInvoice.AH_InvoiceDate = invoiceDate2;
					newInvoice.Validation.ValidateAH_TransactionNum();
					AssertHasWarningContaining(newInvoice.AH_TransactionNumInfo, "This transaction number can be used.");
					var duplicateTransactionNumberDetails = newInvoice.GetPreviousSameNumberTransactionDetails();
					Assert(duplicateTransactionNumberDetails.HasValue);
					AssertEquals(existingINInvoice.AH_InvoiceDate, duplicateTransactionNumberDetails.Value.PreviousInvoiceDate);
					AssertEquals(1, duplicateTransactionNumberDetails.Value.PreviousTransactionCount);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestCheckAPTransactionNumForInvNumAlreadyExistInvDateIsEmpty_Standard()
		{
			var date = new ZDateTime(2017, 01, 01);
			AssertCheckAPTransactionNumForInvNumAlreadyExistInvDateIsEmpty(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				date, date.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		public virtual void TestCheckAPTransactionNumForInvNumAlreadyExistInvDateIsEmpty_Calendar()
		{
			var date = new ZDateTime(2017, 01, 01);

			AssertCheckAPTransactionNumForInvNumAlreadyExistInvDateIsEmpty(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				date, new ZDateTime(date.Year + 1, 1, 1));
		}

		void AssertCheckAPTransactionNumForInvNumAlreadyExistInvDateIsEmpty(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2)
		{
			if (InvoiceType == typeof(APInvoice) || InvoiceType == typeof(APCreditNote) || InvoiceType == typeof(APAdjustmentNote))
			{
				using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
				{
					InvoicingBase existingINInvoice;
					InvoicingBase newInvoice;
					string existingAPInvoiceNum = "TESTAP1111";

					if (InvoiceType.IsSubclassOf(typeof(CreditNote)))
					{
						existingINInvoice = Factory.NewWithValidTestData<APCreditNote>();
					}
					else
					{
						existingINInvoice = Factory.NewWithValidTestData<APInvoice>();
					}

					existingINInvoice.AH_TransactionNum = existingAPInvoiceNum;
					existingINInvoice.AH_OH = TestOrg.PK;
					existingINInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
					existingINInvoice.AH_InvoiceDate = invoiceDate;
					Factory.Save();

					if (InvoiceType.IsSubclassOf(typeof(CreditNote)))
					{
						newInvoice = Factory.NewWithValidTestData<APCreditNote>();
					}
					else
					{
						newInvoice = Factory.NewWithValidTestData<APInvoice>();
					}

					newInvoice.AH_TransactionNum = existingAPInvoiceNum;
					newInvoice.AH_OH = TestOrg.PK;
					newInvoice.AH_InvoiceDate = ZDateTime.Empty;

					newInvoice.Validation.ValidateAH_TransactionNum();
					AssertHasError(newInvoice.AH_TransactionNumInfo, "The transaction number is already in use. Cannot check if the duplicate transaction number is allowed as the Invoice Date is empty.");

					newInvoice.AH_InvoiceDate = invoiceDate2;
					newInvoice.Validation.ValidateAH_TransactionNum();
					AssertHasWarningContaining(newInvoice.AH_TransactionNumInfo, "This transaction number can be used.");
					var duplicateTransactionNumberDetails = newInvoice.GetPreviousSameNumberTransactionDetails();
					Assert(duplicateTransactionNumberDetails.HasValue);
					AssertEquals(existingINInvoice.AH_InvoiceDate, duplicateTransactionNumberDetails.Value.PreviousInvoiceDate);
					AssertEquals(1, duplicateTransactionNumberDetails.Value.PreviousTransactionCount);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestCheckAPTransactionNumForUAInvNumAlreadyExist_Standard()
		{
			var date = new ZDateTime(2017, 01, 01);
			AssertCheckAPTransactionNumForUAInvNumAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				date, date.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		public virtual void TestCheckAPTransactionNumForUAInvNumAlreadyExist_Calendar()
		{
			var date = new ZDateTime(2017, 01, 01);
			AssertCheckAPTransactionNumForUAInvNumAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				date, new ZDateTime(date.Year + 1, 1, 1));
		}

		void AssertCheckAPTransactionNumForUAInvNumAlreadyExist(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2)
		{
			if (InvoiceType == typeof(APInvoice) || InvoiceType == typeof(APCreditNote) || InvoiceType == typeof(APAdjustmentNote))
			{
				using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
				{
					InvoicingBase existingUAInvoice;
					InvoicingBase newInvoice;
					string existingAPInvoiceNum = "TESTAP1111";

					if (InvoiceType.IsSubclassOf(typeof(CreditNote)))
					{
						existingUAInvoice = Factory.NewWithValidTestData<UACreditNote>();
					}
					else
					{
						existingUAInvoice = Factory.NewWithValidTestData<UAInvoice>();
					}

					existingUAInvoice.AH_TransactionNum = existingAPInvoiceNum;
					existingUAInvoice.AH_OH = TestOrg.PK;
					existingUAInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.UnapprovedPayableTransactions;
					existingUAInvoice.AH_InvoiceDate = invoiceDate;
					Factory.Save();

					if (InvoiceType.IsSubclassOf(typeof(CreditNote)))
					{
						newInvoice = Factory.NewWithValidTestData<APCreditNote>();
					}
					else
					{
						newInvoice = Factory.NewWithValidTestData<APInvoice>();
					}

					newInvoice.AH_TransactionNum = existingAPInvoiceNum;
					newInvoice.AH_OH = TestOrg.PK;
					newInvoice.AH_InvoiceDate = existingUAInvoice.AH_InvoiceDate;

					newInvoice.Validation.ValidateAH_TransactionNum();
					AssertHasErrorContaining(newInvoice.AH_TransactionNumInfo, "The transaction number is already in use by Unapproved Invoice.");

					newInvoice.AH_InvoiceDate = invoiceDate2;
					newInvoice.Validation.ValidateAH_TransactionNum();
					AssertHasWarningContaining(newInvoice.AH_TransactionNumInfo, "This transaction number can be used.");
					var duplicateTransactionNumberDetails = newInvoice.GetPreviousSameNumberTransactionDetails();
					Assert(duplicateTransactionNumberDetails.HasValue);
					AssertEquals(existingUAInvoice.AH_InvoiceDate, duplicateTransactionNumberDetails.Value.PreviousInvoiceDate);
					AssertEquals(1, duplicateTransactionNumberDetails.Value.PreviousTransactionCount);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestAPInvoiceWithExistingInvNumOutsideOfPeriod_Standard()
		{
			var date = new ZDateTime(2016, 01, 01);
			AssertAPInvoiceWithExistingInvNumOutsideOfPeriod(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				date, date.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		public virtual void TestAPInvoiceWithExistingInvNumOutsideOfPeriod_Calendar()
		{
			var date = new ZDateTime(2016, 01, 01);
			AssertAPInvoiceWithExistingInvNumOutsideOfPeriod(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				date, new ZDateTime(date.Year + 1, 1, 1));
		}

		void AssertAPInvoiceWithExistingInvNumOutsideOfPeriod(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2)
		{
			if (InvoiceType == typeof(APInvoice) || InvoiceType == typeof(APCreditNote) || InvoiceType == typeof(APAdjustmentNote))
			{
				using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
				{
					InvoicingBase existingAPInvoice;
					InvoicingBase newInvoice;
					string existingAPInvoiceNum = "TESTAP1111";

					if (InvoiceType.IsSubclassOf(typeof(CreditNote)))
					{
						existingAPInvoice = Factory.NewWithValidTestData<APCreditNote>();
					}
					else
					{
						existingAPInvoice = Factory.NewWithValidTestData<APInvoice>();
					}

					existingAPInvoice.AH_TransactionNum = existingAPInvoiceNum;
					existingAPInvoice.AH_OH = TestOrg.PK;
					existingAPInvoice.AH_InvoiceDate = invoiceDate;
					existingAPInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
					existingAPInvoice.AH_TransactionCount = 2;
					Factory.Save();

					if (InvoiceType.IsSubclassOf(typeof(CreditNote)))
					{
						newInvoice = Factory.NewWithValidTestData<APCreditNote>();
					}
					else
					{
						newInvoice = Factory.NewWithValidTestData<APInvoice>();
					}

					newInvoice.AH_TransactionNum = existingAPInvoiceNum;
					newInvoice.AH_OH = TestOrg.PK;
					newInvoice.AH_InvoiceDate = invoiceDate2;
					newInvoice.Validation.ValidateAH_TransactionNum();

					AssertHasWarningContaining(newInvoice.AH_TransactionNumInfo, "This transaction number can be used.");
					var duplicateTransactionNumberDetails = newInvoice.GetPreviousSameNumberTransactionDetails();
					Assert(duplicateTransactionNumberDetails.HasValue);
					AssertEquals(existingAPInvoice.AH_InvoiceDate, duplicateTransactionNumberDetails.Value.PreviousInvoiceDate);
					AssertEquals(existingAPInvoice.AH_TransactionCount, duplicateTransactionNumberDetails.Value.PreviousTransactionCount);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestAPInvoiceNumberDoesntCheckInvoiceNumbersForOtherCompanies()
		{
			string invoiceNumber = "INVOICENUM";
			GlbBranch nonCurrentCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			using (TestObjectCreator.SwitchEnvToBranch(nonCurrentCompanyBranch))
			{
				CreateTransaction();
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			InvoicingBase invoiceForCurrentCompany = newFactory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			invoiceForCurrentCompany.AH_OH = TestOrg.PK;
			invoiceForCurrentCompany.AH_TransactionNum = invoiceNumber;

			invoiceForCurrentCompany.Validation.ValidateAH_TransactionNum();
			AssertNoErrors("Should be no errors as saved invoice belongs to different company", invoiceForCurrentCompany.AH_TransactionNumInfo);

			CreateTransaction();
			Factory.Save();

			var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			InvoicingBase newInvoiceForCurrentCompany = newFactory2.NewWithValidTestData(InvoiceType) as InvoicingBase;
			newInvoiceForCurrentCompany.AH_OH = TestOrg.PK;
			newInvoiceForCurrentCompany.AH_TransactionNum = invoiceNumber;

			newInvoiceForCurrentCompany.Validation.ValidateAH_TransactionNum();
			AssertHasErrorContaining(newInvoiceForCurrentCompany.AH_TransactionNumInfo, "The transaction number is already in use");

			InvoicingBase CreateTransaction()
			{
				InvoicingBase postedInvoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
				postedInvoice.AH_TransactionNum = invoiceNumber;
				postedInvoice.AH_OH = TestOrg.PK;
				return postedInvoice;
			}
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public virtual void TestCheckTransactionNumForUAInvNumAlreadyExist_Standard()
		{
			AssertCheckTransactionNumForUAInvNumAlreadyExist(
				AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today, ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public virtual void TestCheckTransactionNumForUAInvNumAlreadyExist_Calendar()
		{
			AssertCheckTransactionNumForUAInvNumAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today, new ZDateTime(ZDateTime.Today.Year + 1, 1, 1));
		}

		void AssertCheckTransactionNumForUAInvNumAlreadyExist(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				string existingUAInvoiceNum = "TESTUA5787";
				InvoicingBase existingInvoice;
				if (InvoiceType.IsSubclassOf(typeof(CreditNote)))
				{
					existingInvoice = Factory.NewWithValidTestData<UACreditNote>();
				}
				else
				{
					existingInvoice = Factory.NewWithValidTestData<UAInvoice>();
				}

				existingInvoice.AH_TransactionNum = existingUAInvoiceNum;
				existingInvoice.AH_OH = TestOrg.PK;
				existingInvoice.AH_InvoiceDate = invoiceDate;
				existingInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.UnapprovedPayableTransactions;

				Factory.Save();

				InvoicingBase newInvoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				newInvoice.AH_TransactionNum = existingUAInvoiceNum;
				newInvoice.AH_OH = TestOrg.PK;
				InvoiceBaseValidation testValidation = GetValidation(newInvoice);

				testValidation.ValidateAH_TransactionNum();
				if (newInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					AssertHasErrorContaining(newInvoice.AH_TransactionNumInfo, "The transaction number is already in use by Unapproved Invoice.");
				}
				else
				{
					Assert(newInvoice.AH_TransactionNumInfo.HasError("The transaction number is already in use by Unapproved Invoice. Please select another one."));
				}

				newInvoice.AH_TransactionNum = existingUAInvoiceNum;
				newInvoice.AH_InvoiceDate = invoiceDate2;
				testValidation.ValidateAH_TransactionNum();
				if (newInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					AssertHasWarningContaining(newInvoice.AH_TransactionNumInfo, "This transaction number can be used.");
				}
				else
				{
					Assert(newInvoice.AH_TransactionNumInfo.HasError("The transaction number is already in use by Unapproved Invoice. Please select another one."));
				}
			}
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public virtual void TestCheckTransactionNumForUAInvNumExcludingItselfAlreadyExist_Standard()
		{
			AssertCheckTransactionNumForUAInvNumExcludingItselfAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today, ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public virtual void TestCheckTransactionNumForUAInvNumExcludingItselfAlreadyExist_Calendar()
		{
			AssertCheckTransactionNumForUAInvNumExcludingItselfAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today, new ZDateTime(ZDateTime.Today.Year + 1, 1, 1));
		}

		void AssertCheckTransactionNumForUAInvNumExcludingItselfAlreadyExist(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				string existingUAInvoiceNum = "TESTUA5787";
				InvoicingBase existingInvoice;
				if (InvoiceType.IsSubclassOf(typeof(CreditNote)))
				{
					existingInvoice = Factory.NewWithValidTestData<UACreditNote>();
				}
				else
				{
					existingInvoice = Factory.NewWithValidTestData<UAInvoice>();
				}

				existingInvoice.AH_TransactionNum = existingUAInvoiceNum;
				existingInvoice.AH_OH = TestOrg.PK;
				existingInvoice.AH_InvoiceDate = invoiceDate;
				existingInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.UnapprovedPayableTransactions;

				Factory.Save();

				var testValidation = GetValidation(existingInvoice);
				if (testValidation != null)
				{
					testValidation.ValidateAH_TransactionNum();
					AssertNoErrors(existingInvoice.AH_TransactionNumInfo);

					InvoicingBase newInvoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
					newInvoice.AH_TransactionNum = existingUAInvoiceNum;
					newInvoice.AH_OH = TestOrg.PK;
					testValidation = GetValidation(newInvoice);

					testValidation.ValidateAH_TransactionNum();
					if (newInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
					{
						AssertHasErrorContaining(newInvoice.AH_TransactionNumInfo, "The transaction number is already in use by Unapproved Invoice.");
					}
					else
					{
						Assert(newInvoice.AH_TransactionNumInfo.HasError("The transaction number is already in use by Unapproved Invoice. Please select another one."));
					}

					newInvoice.AH_TransactionNum = existingUAInvoiceNum;
					newInvoice.AH_InvoiceDate = invoiceDate2;
					testValidation.ValidateAH_TransactionNum();
					if (newInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
					{
						AssertHasWarningContaining(newInvoice.AH_TransactionNumInfo, "This transaction number can be used.");
					}
					else
					{
						Assert(newInvoice.AH_TransactionNumInfo.HasError("The transaction number is already in use by Unapproved Invoice. Please select another one."));
					}
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestCheckTransactionNumForUAInvNumExcludingItselfAlreadyExist_JobRelated()
		{
			if (InvoiceType == typeof(UAInvoice) || InvoiceType == typeof(UACreditNote))
			{
				string existingUAInvoiceNum = "TESTUA5787";

				var existingInvoice = (InvoicingBase)Factory.New(InvoiceType);
				existingInvoice.AH_TransactionNum = existingUAInvoiceNum;
				existingInvoice.AH_OH = TestOrg.PK;
				existingInvoice.AH_InvoiceDate = ZDateTime.Today;

				var existingLine = (InvoicingLineBase)existingInvoice.Lines.AddNew();
				existingLine.AL_AC = TestObjectCreator.CC1.PK;
				existingLine.AL_JH = TestObjectCreator.Job1.PK;

				var charge = TestObjectCreator.CreateCharge(existingLine);
				charge.JR_OH_CostAccount = TestOrg.PK;
				charge.JR_APInvoiceNum = existingUAInvoiceNum;
				charge.JR_AL_APLine = existingLine.PK;
				charge.JR_JH = TestObjectCreator.Job1.PK;
				charge.JR_AC = TestObjectCreator.CC1.PK;
				Factory.Save();

				existingInvoice.Validation.ValidateAH_TransactionNum();
				AssertNoErrors(existingInvoice.AH_TransactionNumInfo);

				var newInvoice = (InvoicingBase)Factory.New(InvoiceType);
				newInvoice.AH_TransactionNum = existingUAInvoiceNum;
				newInvoice.AH_OH = TestOrg.PK;
				newInvoice.Validation.ValidateAH_TransactionNum();
				AssertHasErrors(newInvoice.AH_TransactionNumInfo);

				newInvoice.AH_InvoiceDate = existingInvoice.AH_InvoiceDate.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths);
				newInvoice.Validation.ValidateAH_TransactionNum();
				AssertHasErrors(newInvoice.AH_TransactionNumInfo);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public virtual void TestCheckTransactionNumForUnallocatedInvNumAlreadyExist_Standard()
		{
			AssertCheckTransactionNumForUnallocatedInvNumAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today, ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public virtual void TestCheckTransactionNumForUnallocatedInvNumAlreadyExist_Calendar()
		{
			AssertCheckTransactionNumForUnallocatedInvNumAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today, new ZDateTime(ZDateTime.Today.Year + 1, 1, 1));
		}

		void AssertCheckTransactionNumForUnallocatedInvNumAlreadyExist(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				string existingUAInvoiceNum = "TESTUA5787";
				TransactionPendingAllocation unallocatedTransaction = Factory.New<TransactionPendingAllocation>();
				unallocatedTransaction.AH_TransactionNum = existingUAInvoiceNum;
				unallocatedTransaction.AH_OH = TestOrg.PK;
				unallocatedTransaction.AH_OSExTaxAmount = 100m;
				unallocatedTransaction.AH_InvoiceDate = invoiceDate;

				Factory.Save();

				APInvoice newInvoice = Factory.NewWithValidTestData<APInvoice>();
				newInvoice.AH_TransactionNum = existingUAInvoiceNum;
				newInvoice.AH_OH = TestOrg.PK;
				InvoiceBaseValidation testValidation = (InvoiceBaseValidation)newInvoice.Validation;

				testValidation.ValidateAH_TransactionNum();
				AssertHasErrorContaining(newInvoice.AH_TransactionNumInfo, "The transaction number is already in use by Transaction Pending Allocation.");

				newInvoice.AH_InvoiceDate = invoiceDate2;
				newInvoice.Validation.ValidateAH_TransactionNum();
				AssertHasWarningContaining(newInvoice.AH_TransactionNumInfo, "This transaction number can be used.");
			}
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public virtual void TestCheckTransactionNumForIncompleteInvoices_Standard()
		{
			AssertCheckTransactionNumForIncompleteInvoices(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today, ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public virtual void TestCheckTransactionNumForIncompleteInvoices_Calendar()
		{
			AssertCheckTransactionNumForIncompleteInvoices(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today, new ZDateTime(ZDateTime.Today.Year + 1, 1, 1));
		}

		void AssertCheckTransactionNumForIncompleteInvoices(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("APInv1", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
				apInvoice.AH_OH = TestOrg.PK;
				apInvoice.AH_InvoiceDate = invoiceDate;
				Factory.Save();

				var incompleteInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("INAPInv1", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
				incompleteInvoice1.AH_OH = TestOrg.PK;
				incompleteInvoice1.SaveAsIncomplete();

				var incompleteInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("INAPInv2", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
				incompleteInvoice2.AH_OH = TestOrg.PK;
				incompleteInvoice2.SaveAsIncomplete();

				var newFactory = new BusinessObjectFactory();
				var loadedIncompleteInvoice1 = newFactory.Load<APInvoice>(incompleteInvoice1.PK);
				loadedIncompleteInvoice1.RestoreSavedData();

				var testValidation = (InvoiceBaseValidation)loadedIncompleteInvoice1.Validation;

				loadedIncompleteInvoice1.AH_TransactionNum = "APInv1";
				testValidation.ValidateAH_TransactionNum();
				AssertNoErrors(loadedIncompleteInvoice1.AH_TransactionNumInfo);

				loadedIncompleteInvoice1.AH_TransactionNum = "INAPInv2";
				testValidation.ValidateAH_TransactionNum();
				AssertHasError(loadedIncompleteInvoice1.AH_TransactionNumInfo, "The transaction number is already in use by Incomplete Transaction. Please select another one.");

				loadedIncompleteInvoice1.AH_TransactionNum = "TestInv";
				testValidation.ValidateAH_TransactionNum();
				AssertNoErrors(loadedIncompleteInvoice1.AH_TransactionNumInfo);

				loadedIncompleteInvoice1.MoveFromIncompleteToPayableLedger(); // This replicates a user clicks "Post" an incomplete invoice
				loadedIncompleteInvoice1.AH_TransactionNum = "APInv1";
				testValidation.ValidateAH_TransactionNum();
				AssertHasErrorContaining(loadedIncompleteInvoice1.AH_TransactionNumInfo, "The transaction number is already in use.");

				loadedIncompleteInvoice1.AH_TransactionNum = "TestInv";
				testValidation.ValidateAH_TransactionNum();
				AssertNoErrors(loadedIncompleteInvoice1.AH_TransactionNumInfo);

				loadedIncompleteInvoice1.AH_TransactionNum = "APInv1";
				loadedIncompleteInvoice1.AH_InvoiceDate = invoiceDate2;
				testValidation.ValidateAH_TransactionNum();
				AssertHasWarningContaining(loadedIncompleteInvoice1.AH_TransactionNumInfo, "This transaction number can be used.");
			}
		}

		public virtual void TestUAInvoiceNumberDoesntCheckInvoiceNumbersForOtherCompanies()
		{
			string invoiceNumber = "INVOICENUM";
			GlbBranch nonCurrentCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			using (TestObjectCreator.SwitchEnvToBranch(nonCurrentCompanyBranch))
			{
				CreateUATransaction();
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			InvoicingBase invoiceForCurrentCompany = newFactory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			invoiceForCurrentCompany.AH_OH = TestOrg.PK;
			invoiceForCurrentCompany.AH_TransactionNum = invoiceNumber;

			invoiceForCurrentCompany.Validation.ValidateAH_TransactionNum();
			AssertNoErrors("Should be no errors as saved invoice belongs to different company", invoiceForCurrentCompany.AH_TransactionNumInfo);

			CreateUATransaction();
			Factory.Save();

			var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			InvoicingBase newInvoiceForCurrentCompany = newFactory2.NewWithValidTestData(InvoiceType) as InvoicingBase;
			newInvoiceForCurrentCompany.AH_OH = TestOrg.PK;
			newInvoiceForCurrentCompany.AH_TransactionNum = invoiceNumber;

			newInvoiceForCurrentCompany.Validation.ValidateAH_TransactionNum();
			if (newInvoiceForCurrentCompany.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				AssertHasErrorContaining(newInvoiceForCurrentCompany.AH_TransactionNumInfo, "The transaction number is already in use by Unapproved Invoice.");
			}
			else
			{
				AssertHasErrorContaining(newInvoiceForCurrentCompany.AH_TransactionNumInfo, "The transaction number is already in use by Unapproved Invoice. Please select another one.");
			}

			InvoicingBase CreateUATransaction()
			{
				InvoicingBase postedInvoice;
				if (InvoiceType.IsSubclassOf(typeof(CreditNote)))
				{
					postedInvoice = Factory.NewWithValidTestData<UACreditNote>();
				}
				else
				{
					postedInvoice = Factory.NewWithValidTestData<UAInvoice>();
				}
				postedInvoice.AH_TransactionNum = invoiceNumber;
				postedInvoice.AH_OH = TestOrg.PK;
				postedInvoice.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
				return postedInvoice;
			}
		}

		public void TestCheckTransactionNumForAPInvNum()
		{
			string existingAPInvoiceNum = "TESTAP5787";
			InvoicingBase existingInvoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			existingInvoice.AH_TransactionNum = existingAPInvoiceNum;
			existingInvoice.AH_OH = TestOrg.PK;

			Factory.Save();

			InvoicingBase newInvoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			newInvoice.AH_TransactionNum = "0000000000";
			newInvoice.AH_OH = TestOrg.PK;
			InvoiceBaseValidation testValidation = GetValidation(newInvoice);

			testValidation.CheckAH_TransactionNum_ForTestOnly();
			Assert(!newInvoice.AH_TransactionNumInfo.HasError("The transaction number is already in use on Job Invoicing. Please select another one."));

			newInvoice.IsSelfBillingInvoice = true; // setting this proprerty to true will set AH_TransactionNum to empty
			newInvoice.Validation.ValidateAH_TransactionNum();
			Assert(!newInvoice.AH_TransactionNumInfo.HasErrors());
		}

		public void TestCheckTransactionNumForAPInvNumUsedInJobInvoicing_Standard()
		{
			AssertCheckTransactionNumForAPInvNumUsedInJobInvoicing(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today, ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		public void TestCheckTransactionNumForAPInvNumUsedInJobInvoicing_Calendar()
		{
			AssertCheckTransactionNumForAPInvNumUsedInJobInvoicing(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today, new ZDateTime(ZDateTime.Today.Year + 1, 1, 1));
		}

		void AssertCheckTransactionNumForAPInvNumUsedInJobInvoicing(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2)
		{
			if (InvoiceType == typeof(ARInvoice) || InvoiceType == typeof(ARCreditNote) || InvoiceType == typeof(ARAdjustmentNote))
			{
				Assert(true);
			}
			else
			{
				using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
				{
					var existingAPInvNum1 = "TestAP0001";
					var existingAPInvNum2 = "TestAP0002";
					var shipment = TestObjectCreator.CreateShipment("S00001");
					var job = TestObjectCreator.CreateJob(shipment);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 100m);
					charge1.JR_APInvoiceNum = existingAPInvNum1;
					charge1.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
					var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, -100m, -100m);
					charge2.JR_APInvoiceNum = existingAPInvNum2;
					charge2.JR_APInvoiceDate = invoiceDate;
					charge2.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
					Factory.Save();

					InvoicingBase invoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
					invoice.AH_TransactionNum = existingAPInvNum1;
					invoice.AH_OH = TestObjectCreator.AALSHI.PK;
					InvoiceBaseValidation validation = GetValidation(invoice);
					validation.ValidateAH_TransactionNum();
					AssertHasError(invoice.AH_TransactionNumInfo, "The transaction number is already in use on Job Invoicing of the following Job(s): S00001. Please select another one.");

					invoice.AH_TransactionNum = existingAPInvNum2;
					invoice.AH_OH = TestObjectCreator.AALSHI.PK;
					validation.ValidateAH_TransactionNum();
					AssertHasError(invoice.AH_TransactionNumInfo, "The transaction number is already in use on Job Invoicing of the following Job(s): S00001. Please select another one.");

					invoice.AH_TransactionNum = "Test";
					invoice.AH_OH = TestObjectCreator.AALSHI.PK;
					validation.ValidateAH_TransactionNum();
					AssertNoError(invoice.AH_TransactionNumInfo, "The transaction number is already in use on Job Invoicing of the following Job(s): S00001. Please select another one.");

					invoice.AH_TransactionNum = existingAPInvNum2;
					invoice.AH_InvoiceDate = invoiceDate2;
					validation.ValidateAH_TransactionNum();
					AssertHasError(invoice.AH_TransactionNumInfo, "The transaction number is already in use on Job Invoicing of the following Job(s): S00001. Please select another one.");
				}
			}
		}

		public void TestChangingOrganisationValidatesTransactionNumberOnAPInv()
		{
			APInvoice aPInv1 = Factory.NewWithValidTestData<APInvoice>();
			aPInv1.AH_TransactionNum = "1";
			aPInv1.AH_OH = TestObjectCreator.AALSHI.PK;
			APInvoiceLine line1 = (APInvoiceLine)aPInv1.Lines.AddNew();
			line1.AL_OSExTaxAmount = 100M;
			line1.GenericCharge = TestObjectCreator.CC1.PK;
			line1.AL_AC = TestObjectCreator.CC1.PK;
			TestObjectCreator.AttachJobToAPLine(line1);
			TestObjectCreator.AttachChargeToAPLine(line1);
			Factory.Save();

			APInvoice aPInv2 = Factory.NewWithValidTestData<APInvoice>();
			aPInv2.AH_TransactionNum = "1";
			aPInv2.Validation.ValidateAH_TransactionNum();
			Assert("Precondition: Transaction number does not have errors", !aPInv2.AH_TransactionNumInfo.HasErrors());

			aPInv2.AH_OH = TestObjectCreator.AALSHI.PK;

			Assert("Transaction number should have errors since 1 is already used", aPInv2.AH_TransactionNumInfo.HasErrors());
		}

		public virtual void TestJobInvoicingExist()
		{
			string existingAPInvoiceNum = "TESTAP5787";
			string existingJobInvoiceNum = "TEST5787";

			InvoicingBase existingInvoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			existingInvoice.AH_TransactionNum = existingAPInvoiceNum;
			existingInvoice.AH_OH = TestOrg.PK;
			((InvoiceLine)existingInvoice.Lines.AddNew()).AL_AG = TestObjectCreator.GLHeader1.PK;

			Job testJob = TestObjectCreator.CreateJob(TestOrg, 100.0m, TestOrg, 120.0m);

			testJob.Charges.AddNew();

			testJob.Charges[0].JR_OH_CostAccount = TestOrg.PK;
			testJob.Charges[0].JR_OSCostAmt = -120.0m;
			testJob.Charges[0].JR_LocalCostAmt = -120.0m;
			testJob.Charges[0].JR_AL_APLine = existingInvoice.Lines[0].PK;
			existingInvoice.Lines[0].AL_OSAmount = existingInvoice.Lines[0].AL_LineAmount = -testJob.Charges[0].JR_LocalCostAmt;
			testJob.Charges[0].JR_AC = ChargeCode.PK;
			testJob.Charges[0].JR_APInvoiceNum = existingJobInvoiceNum;
			testJob.Charges[0].JR_AT_CostGSTRate = ZGuid.Empty;
			Factory.Save();

			InvoicingBase newInvoice = new BusinessObjectFactory().NewWithValidTestData(InvoiceType) as InvoicingBase;
			newInvoice.AH_TransactionNum = existingJobInvoiceNum;
			newInvoice.AH_OH = TestOrg.PK;

			InvoiceBaseValidation testValidation = GetValidation(newInvoice);
			testValidation.ValidateAH_TransactionNum();
			AssertHasError(newInvoice.AH_TransactionNumInfo, string.Format("The transaction number is already in use on Job Invoicing of the following Job(s): {0}. Please select another one.", testJob.JH_JobNum));

			newInvoice.AH_TransactionNum = "ABC123";
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var chargeCompanyBeforeChangingBranch = testJob.Charges[0].JR_GC;
				testJob.Charges[0].JR_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
				AssertNotEquals("Changing branch also changed company", chargeCompanyBeforeChangingBranch, testJob.Charges[0].JR_GC);
				Factory.Save();
			}

			AssertNotEquals("Charge company is not same as job header company", testJob.JH_GC, testJob.Charges[0].JR_GC);
			newInvoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			newInvoice.AH_TransactionNum = existingJobInvoiceNum;
			newInvoice.AH_OH = TestOrg.PK;
			testValidation = GetValidation(newInvoice);
			testValidation.ValidateAH_TransactionNum();
			AssertNoError("There are no charges in current company with same invoice number.", newInvoice.AH_TransactionNumInfo, string.Format("The transaction number is already in use on Job Invoicing of the following Job(s): {0}. Please select another one.", testJob.JH_JobNum));
		}

		public virtual void TestCheckAH_OSTotalAmount_DetectsCreditedExceedInvoiced()
		{
			var originalInvoiceWithPositiveTaxAmount = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			originalInvoiceWithPositiveTaxAmount.AH_InvoiceAmount = 8m;
			originalInvoiceWithPositiveTaxAmount.AH_LocalTaxAmountOtherTaxes = 2m;
			originalInvoiceWithPositiveTaxAmount.AH_GSTAmount = 0;
			AssertEquals(10m, originalInvoiceWithPositiveTaxAmount.AH_LocalTotal);

			var expectedWarning = "Posting this transaction will result in the Receivables Organization being credited more than they have been invoiced in the related transaction/s. Do you want to continue posting?";
			var expectedError = "This amending transaction cannot be posted. Posting this transaction would result in the Receivable Organization being credited more than they have been invoiced in the related transaction/s. Please review the charges you are attempting to credit.";
			if (originalInvoiceWithPositiveTaxAmount is IAmending amending && originalInvoiceWithPositiveTaxAmount.HasImplementedGenerateAmendingTransaction)
			{
				//test original invoice with positive tax.
				AccountingConfigurationRegistry.Instance.AmendingTransactionLocalTotalBehavior.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				var invoiceToTest1 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest1.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				invoiceToTest1.AH_InvoiceAmount = 5m;
				invoiceToTest1.AH_GSTAmount = 0;
				AssertEquals(5m, invoiceToTest1.AH_LocalTotal);
				//testValidation.ValidateAH_OSTotalAmount();
				AssertNoWarning("Total amount is 15, so expect no warning", invoiceToTest1.AH_OSTotalAmountInfo, expectedWarning);

				var invoiceToTest2 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest2.AH_InvoiceAmount = -15m;
				invoiceToTest2.AH_LocalTaxAmountOtherTaxes = -5m;
				invoiceToTest2.AH_GSTAmount = 0;
				AssertEquals(-20m, invoiceToTest2.AH_LocalTotal);
				var testValidation = GetValidation(invoiceToTest2);
				testValidation.ValidateAH_OSTotalAmount();
				AssertHasWarning("Total amount is -5 so expect warning", invoiceToTest2.AH_OSTotalAmountInfo, expectedWarning);

				var invoiceToTest3 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest3.AH_InvoiceAmount = 1m;
				invoiceToTest3.AH_GSTAmount = 0;
				AssertEquals(1m, invoiceToTest3.AH_LocalTotal);
				testValidation = GetValidation(invoiceToTest3);
				testValidation.ValidateAH_OSTotalAmount();
				AssertNoWarning("Although total amount is -4, but since new invoice amount is positive, so expect no warning", invoiceToTest3.AH_OSTotalAmountInfo, expectedWarning);

				var invoiceToTest4 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest4.AH_InvoiceAmount = -1m;
				invoiceToTest4.AH_GSTAmount = 0;
				AssertEquals(-1m, invoiceToTest4.AH_LocalTotal);
				testValidation = GetValidation(invoiceToTest4);
				testValidation.ValidateAH_OSTotalAmount();
				AssertHasWarning("Total amount is -5 and new invoice amount is negative, so expect warning", invoiceToTest4.AH_OSTotalAmountInfo, expectedWarning);

				//test original invoice with negative tax 
				AccountingConfigurationRegistry.Instance.AmendingTransactionLocalTotalBehavior.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var originalInvoiceWithNegativeTaxAmount = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				originalInvoiceWithNegativeTaxAmount.AH_InvoiceAmount = -10m;
				originalInvoiceWithNegativeTaxAmount.AH_GSTAmount = 0;
				AssertEquals(-10m, originalInvoiceWithNegativeTaxAmount.AH_LocalTotal);

				amending = originalInvoiceWithNegativeTaxAmount as IAmending;

				var invoiceToTest5 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest5.AH_InvoiceAmount = 5m;
				invoiceToTest5.AH_GSTAmount = 0;
				AssertEquals(5m, invoiceToTest5.AH_LocalTotal);
				testValidation = GetValidation(invoiceToTest5);
				testValidation.ValidateAH_OSTotalAmount();
				AssertNoError("New invoice amount is positive, so expect no error", invoiceToTest5.AH_OSTotalAmountInfo, expectedError);

				var invoiceToTest6 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest6.AH_InvoiceAmount = -2m;
				invoiceToTest6.AH_GSTAmount = 0;
				AssertEquals(-2m, invoiceToTest6.AH_LocalTotal);
				testValidation = GetValidation(invoiceToTest6);
				testValidation.ValidateAH_OSTotalAmount();
				AssertHasError("Total amount is negative, expect error", invoiceToTest6.AH_OSTotalAmountInfo, expectedError);
			}
			else
			{
				Assert("Not applicable because it does not implement IAmending", true);
			}
		}

		public virtual void TestCheckAH_LocalTotalAmount_DetectsCreditedExceedInvoiced()
		{
			var originalInvoiceWithPositiveTaxAmount = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			var expectedWarning = "Posting this transaction will result in the Receivables Organization being credited more than they have been invoiced in the related transaction/s. Do you want to continue posting?";
			var expectedError = "This amending transaction cannot be posted. Posting this transaction would result in the Receivable Organization being credited more than they have been invoiced in the related transaction/s. Please review the charges you are attempting to credit.";

			if (originalInvoiceWithPositiveTaxAmount is IAmending amending && originalInvoiceWithPositiveTaxAmount.HasImplementedGenerateAmendingTransaction)
			{
				//test original invoice with positive tax.
				AccountingConfigurationRegistry.Instance.AmendingTransactionLocalTotalBehavior.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				originalInvoiceWithPositiveTaxAmount.AH_InvoiceAmount = 10m;
				originalInvoiceWithPositiveTaxAmount.AH_GSTAmount = 0;
				AssertEquals(10m, originalInvoiceWithPositiveTaxAmount.AH_LocalTotal);

				var invoiceToTest1 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest1.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				invoiceToTest1.ExchangeRate.Currency = "USD";
				invoiceToTest1.AH_InvoiceAmount = 5m;
				invoiceToTest1.AH_GSTAmount = 0;
				AssertEquals(5m, invoiceToTest1.AH_LocalTotal);

				var testValidation = GetValidation(invoiceToTest1);
				testValidation.ValidateAH_LocalTotalAmount();
				AssertNoWarning("Total amount is 15, so expect no warning", invoiceToTest1.AH_LocalTotalAmountInfo, expectedWarning);

				var invoiceToTest2 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest2.ExchangeRate.Currency = "USD";
				invoiceToTest2.AH_InvoiceAmount = -20m;
				invoiceToTest2.AH_GSTAmount = 0;
				AssertEquals(-20m, invoiceToTest2.AH_LocalTotal);
				testValidation = GetValidation(invoiceToTest2);
				testValidation.ValidateAH_LocalTotalAmount();
				AssertHasWarning("Total amount is -5 so expect warning", invoiceToTest2.AH_LocalTotalAmountInfo, expectedWarning);

				var invoiceToTest3 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest3.ExchangeRate.Currency = "USD";
				invoiceToTest3.AH_InvoiceAmount = 1m;
				invoiceToTest3.AH_GSTAmount = 0;
				AssertEquals(1m, invoiceToTest3.AH_LocalTotal);
				testValidation = GetValidation(invoiceToTest3);
				testValidation.ValidateAH_LocalTotalAmount();
				AssertNoWarning("Although total amount is -4, but since new invoice amount is positive, so expect no warning", invoiceToTest3.AH_LocalTotalAmountInfo, expectedWarning);

				var invoiceToTest4 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest4.ExchangeRate.Currency = "USD";
				invoiceToTest4.AH_InvoiceAmount = -1m;
				invoiceToTest4.AH_GSTAmount = 0;
				AssertEquals(-1m, invoiceToTest4.AH_LocalTotal);
				testValidation = GetValidation(invoiceToTest4);
				testValidation.ValidateAH_LocalTotalAmount();
				AssertHasWarning("Total amount is -5 and new invoice amount is negative, so expect warning", invoiceToTest4.AH_LocalTotalAmountInfo, expectedWarning);

				//test original invoice with negative tax 
				AccountingConfigurationRegistry.Instance.AmendingTransactionLocalTotalBehavior.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				var originalInvoiceWithNegativeTaxAmount = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				originalInvoiceWithNegativeTaxAmount.AH_InvoiceAmount = -10m;
				originalInvoiceWithNegativeTaxAmount.AH_GSTAmount = 0;
				AssertEquals(-10m, originalInvoiceWithNegativeTaxAmount.AH_LocalTotal);

				var invoiceToTest5 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest5.ExchangeRate.Currency = "USD";
				invoiceToTest5.AH_InvoiceAmount = 5m;
				invoiceToTest5.AH_GSTAmount = 0;
				AssertEquals(5m, invoiceToTest5.AH_LocalTotal);
				testValidation = GetValidation(invoiceToTest5);
				testValidation.ValidateAH_LocalTotalAmount();
				AssertNoError("New invoice amount is positive, so expect no error", invoiceToTest5.AH_LocalTotalAmountInfo, expectedError);

				var invoiceToTest6 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest6.ExchangeRate.Currency = "USD";
				invoiceToTest6.AH_InvoiceAmount = -2m;
				invoiceToTest6.AH_GSTAmount = 0;
				AssertEquals(-2m, invoiceToTest6.AH_LocalTotal);
				testValidation = GetValidation(invoiceToTest6);
				testValidation.ValidateAH_LocalTotalAmount();
				AssertHasError("Total amount is negative, so expect error", invoiceToTest6.AH_LocalTotalAmountInfo, expectedError);
			}
			else
			{
				Assert("Not applicable because it does not implement IAmending", true);
			}
		}

		public virtual void TestCheckAH_OSTaxAmount_DetectsTaxCreditedExceedInvoiced()
		{
			InvoicingBase originalInvoiceWithPositiveTaxAmount = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			originalInvoiceWithPositiveTaxAmount.AH_GSTAmount = 10m;

			var expectedWarning = "Posting this transaction will result in the Receivables Organization being credited more Tax than has been invoiced to your Receivables Organization in the related transaction/s. Do you want to continue posting?";

			if (originalInvoiceWithPositiveTaxAmount is IAmending amending && originalInvoiceWithPositiveTaxAmount.HasImplementedGenerateAmendingTransaction)
			{
				//test original invoice with positive tax.
				var invoiceToTest1 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest1.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				invoiceToTest1.AH_GSTAmount = 5m;

				var testValidation = GetValidation(invoiceToTest1);
				testValidation.ValidateAH_OSTaxAmount();
				AssertNoWarnings("Total tax is 15, so expect no warning", invoiceToTest1.AH_OSTaxAmountInfo);

				var invoiceToTest2 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest2.AH_GSTAmount = -20m;
				testValidation = GetValidation(invoiceToTest2);
				testValidation.ValidateAH_OSTaxAmount();
				AssertHasWarning("Total tax is -5 so expect warning", invoiceToTest2.AH_OSTaxAmountInfo, expectedWarning);

				var invoiceToTest3 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest3.AH_GSTAmount = 1m;
				testValidation = GetValidation(invoiceToTest3);
				testValidation.ValidateAH_OSTaxAmount();
				AssertNoWarnings("Although total is -4, but since new invoice tax amount is positive, i.e. +1, so expect no warning", invoiceToTest3.AH_OSTaxAmountInfo);

				var invoiceToTest4 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest4.AH_GSTAmount = -1m;
				testValidation = GetValidation(invoiceToTest4);
				testValidation.ValidateAH_OSTaxAmount();
				AssertHasWarning("Total tax is -5 and new invoice tax amount is negative, i.e. -1, so expect warning", invoiceToTest4.AH_OSTaxAmountInfo, expectedWarning);

				//test original invoice with negative tax.
				var originalInvoiceWithNegativeTaxAmount = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				originalInvoiceWithNegativeTaxAmount.AH_GSTAmount = -10m;
				amending = originalInvoiceWithNegativeTaxAmount as IAmending;

				var invoiceToTest5 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest5.AH_GSTAmount = 5m;
				testValidation = GetValidation(invoiceToTest5);
				testValidation.ValidateAH_OSTaxAmount();
				AssertNoWarnings("new invoice tax is positive, i.e. +5, so expect no warning", invoiceToTest5.AH_OSTaxAmountInfo);

				var invoiceToTest6 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest6.AH_GSTAmount = -2m;
				testValidation = GetValidation(invoiceToTest6);
				testValidation.ValidateAH_OSTaxAmount();
				AssertNoWarnings("Total amount of tax amends is still positive, expect no warning", invoiceToTest6.AH_OSTaxAmountInfo);

				var invoiceToTest7 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest7.AH_GSTAmount = -4m;
				testValidation = GetValidation(invoiceToTest7);
				testValidation.ValidateAH_OSTaxAmount();
				AssertHasWarning("Total amount of tax amends is negative, i.e. -1, so expect warning", invoiceToTest7.AH_OSTaxAmountInfo, expectedWarning);
			}
			else
			{
				Assert("Not applicable because it does not implement IAmending", true);
			}
		}

		public virtual void TestCheckAH_LocalTaxAmount_DetectsTaxCreditedExceedInvoiced()
		{
			InvoicingBase originalInvoiceWithPositiveTaxAmount = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			originalInvoiceWithPositiveTaxAmount.AH_GSTAmount = 10m;

			var expectedWarning = "Posting this transaction will result in the Receivables Organization being credited more Tax than has been invoiced to your Receivables Organization in the related transaction/s. Do you want to continue posting?";

			if (originalInvoiceWithPositiveTaxAmount is IAmending amending && originalInvoiceWithPositiveTaxAmount.HasImplementedGenerateAmendingTransaction)
			{
				//test original invoice with positive tax.
				InvoicingBase invoiceToTest1 = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				invoiceToTest1.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				invoiceToTest1.AH_TransactionBelongsToGroup = originalInvoiceWithPositiveTaxAmount.PK;
				invoiceToTest1.ExchangeRate.Currency = "USD";
				invoiceToTest1.AH_GSTAmount = 5m;

				var testValidation = GetValidation(invoiceToTest1);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertNoWarnings("Total tax is 15, so expect no warning", invoiceToTest1.AH_LocalTaxAmountInfo);

				var invoiceToTest2 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest2.ExchangeRate.Currency = "USD";
				invoiceToTest2.AH_GSTAmount = -20m;
				testValidation = GetValidation(invoiceToTest2);

				testValidation.ValidateAH_LocalTaxAmount();
				AssertHasWarning("Total tax is -5 so expect warning", invoiceToTest2.AH_LocalTaxAmountInfo, expectedWarning);

				var invoiceToTest3 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest3.ExchangeRate.Currency = "USD";
				invoiceToTest3.AH_GSTAmount = 1m;
				testValidation = GetValidation(invoiceToTest3);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertNoWarnings("Although total is -4, but since new invoice tax amount is positive, i.e. +1, so expect no warning", invoiceToTest3.AH_LocalTaxAmountInfo);

				var invoiceToTest4 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest4.ExchangeRate.Currency = "USD";
				invoiceToTest4.AH_GSTAmount = -1m;
				testValidation = GetValidation(invoiceToTest4);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertHasWarning("Total tax is -5 and new invoice tax amount is negative, i.e. -1, so expect warning", invoiceToTest4.AH_LocalTaxAmountInfo, expectedWarning);

				//test original invoice with negative tax.
				var originalInvoiceWithNegativeTaxAmount = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				originalInvoiceWithNegativeTaxAmount.AH_GSTAmount = -10m;
				amending = originalInvoiceWithNegativeTaxAmount as IAmending;

				var invoiceToTest5 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest5.ExchangeRate.Currency = "USD";
				invoiceToTest5.AH_GSTAmount = 5m;
				testValidation = GetValidation(invoiceToTest5);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertNoWarnings("new invoice tax is positive, i.e. +5, so expect no warning", invoiceToTest5.AH_LocalTaxAmountInfo);

				var invoiceToTest6 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest6.ExchangeRate.Currency = "USD";
				invoiceToTest6.AH_GSTAmount = -2m;
				testValidation = GetValidation(invoiceToTest6);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertNoWarnings("Total amount of tax amends is still positive, expect no warning", invoiceToTest6.AH_LocalTaxAmountInfo);

				var invoiceToTest7 = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest7.ExchangeRate.Currency = "USD";
				invoiceToTest7.AH_GSTAmount = -4m;
				testValidation = GetValidation(invoiceToTest7);
				testValidation.ValidateAH_LocalTaxAmount();
				AssertHasWarning("Total amount of tax amends is negative, i.e. -1, so expect warning", invoiceToTest7.AH_LocalTaxAmountInfo, expectedWarning);
			}
			else
			{
				Assert("Not applicable because it does not implement IAmending", true);
			}
		}

		public void TestCheckAH_OSTotalAmount_NotValid()
		{
			InvoicingBase newInvoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			newInvoice.AH_OH = TestOrg.PK;
			newInvoice.Lines.AddNew();

			InvoiceBaseValidation testValidation = GetValidation(newInvoice);
			testValidation.ValidateAH_OSTotalAmount();
			AssertHasZeroBalanceError("Could not be zero balanced with non-comment charge.", true, false);

			testValidation.ValidateAH_OSTotalAmount();
			AssertHasZeroBalanceError("Could not be zero balanced with non-comment charge, when set registry AllowZeroValueARInvoices to YES", true, false);

			ChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			newInvoice.Lines[0].AL_AC = ChargeCode.PK;

			using (AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				testValidation.ValidateAH_OSTotalAmount();
				AssertHasZeroBalanceError("Could not be zero balanced with comment charge, only when is AR Transaction.", false, newInvoice.IsARInvoiceOrCreditNoteOrAdjustmentNote);
			}

			testValidation.ValidateAH_OSTotalAmount();
			AssertHasZeroBalanceError("Allow zero balanced with comment charge, when set registry AllowZeroValueARInvoices to YES", false, false);

			void AssertHasZeroBalanceError(string assertMsg, bool shouldHaveOldErrorMsg, bool shouldHaveNewErrorMsg)
			{
				AssertEquals(assertMsg, shouldHaveOldErrorMsg, newInvoice.AH_OSTotalAmountInfo.HasError(OldTotalAmountZeroError));
				AssertEquals(assertMsg, shouldHaveNewErrorMsg, newInvoice.AH_OSTotalAmountInfo.HasError(NewTotalAmountZeroError));
			}
		}

		public void TestCheckAH_OSTotalAmount_Valid()
		{
			InvoicingBase newInvoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			newInvoice.AH_OH = TestOrg.PK;
			newInvoice.Lines.AddNew();
			newInvoice.Lines[0].AL_OSExTaxAmount = 120m;

			InvoiceBaseValidation testValidation = GetValidation(newInvoice);
			testValidation.ValidateAH_OSTotalAmount();
			Assert(!newInvoice.AH_OSTotalAmountInfo.HasError("The sum of the transaction lines should not equal zero."));
		}

		public void TestCheckAH_OsTotalAmount_InvoicingLevels()
		{
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			var registryForTest = new AuthorizationModeAndSettings();
			var valuesForTest = registryForTest.AuthorisationSettings;
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			newSetting.Range = RangeCodes.UpTo;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			newSetting.Range = RangeCodes.Above;

			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), registryForTest);

			InvoicingBase invoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			int multiplier = invoice is AdjustmentNote ? -1 : 1;
			invoice.AH_OSExTaxAmount = 300 * multiplier;
			bool isLevelsApplicable = invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable &&
				(invoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote ||
				invoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.AdjustmentNote);
			if (isLevelsApplicable)
			{
				var expectedWarning = "You do not have rights to create transaction for this amount without authorization.";
				AssertHasWarning(invoice.AH_OSTotalAmountInfo, expectedWarning);

				invoice.AH_OSExTaxAmount = 180M * multiplier;
				AssertNoWarnings(invoice.AH_OSTotalAmountInfo);

				invoice.AH_OSTaxAmount = 30M * multiplier;
				AssertHasWarning(invoice.AH_OSTotalAmountInfo, expectedWarning);

				invoice.AH_LocalExTaxAmount = 150M * multiplier;
				AssertNoWarnings(invoice.AH_OSTotalAmountInfo);

				invoice.AH_LocalTaxAmount = 60M * multiplier;
				AssertHasWarning(invoice.AH_OSTotalAmountInfo, expectedWarning);
			}
			else
			{
				AssertEquals("There are not warnings for AH_OSTotalAmount", false, invoice.AH_OSTotalAmountInfo.HasWarnings());
			}
		}

		public void TestCheckAH_OsTotalAmount_CostVariance()
		{
			GlbBranch sYD = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
			GlbDepartment fEA = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");
			AccChargeCode cAF = TestObjectCreator.CC1;

			AssertNotNull(sYD);
			AssertNotNull(fEA);

			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			Charge charge = TestObjectCreator.CreateCharge(job, cAF, "",
				TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = sYD.PK;
			charge.JR_GE = fEA.PK;

			Factory.Save();

			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement upTo = valuesForTest.AuthorisationRequirements.AddNew();
			upTo.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			upTo.Range = RangeCodes.UpTo;
			upTo.Amount = 100M;
			CostVarianceApprovalAuthorisationRequirement above = valuesForTest.AuthorisationRequirements.AddNew();
			above.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			above.Range = RangeCodes.Above;
			above.Amount = upTo.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = true;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = cAF.PK;
			line.AL_GB = sYD.PK;
			line.AL_GE = fEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;

			line.AL_OSExTaxAmount = 250M;
			AssertHasWarning(invoice.AH_OSTotalAmountInfo, "You do not have rights to create transaction for this amount without authorization.");

			line.AL_OSExTaxAmount = 150M;
			AssertNoWarnings("There are not warnings for AH_OSTotalAmount", invoice.AH_OSTotalAmountInfo);

			line.AL_LocalExTaxAmount = 250M;
			AssertHasWarning(invoice.AH_OSTotalAmountInfo, "You do not have rights to create transaction for this amount without authorization.");
		}

		public void TestCheckExpectedInvoiceTotal()
		{
			InvoicingBase newInvoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			newInvoice.AH_OH = TestOrg.PK;
			newInvoice.Lines.AddNew();
			newInvoice.Lines.AddNew();
			newInvoice.Lines[0].AL_OSExTaxAmount = 120m;
			newInvoice.Lines[1].AL_OSExTaxAmount = 33m;
			newInvoice.ExpectedInvoiceTotal = 100m;

			InvoiceBaseValidation testValidation = GetValidation(newInvoice);
			testValidation.ValidateExpectedInvoiceTotal();

			var oSCurrencyDecimals = newInvoice.OSCurrencyDecimals;
			var expectedErrorMessage = string.Format(@"The invoice total of {0} {1} does not equal to the expected amount of {2} {1}.
The difference is {3} {1}.", newInvoice.AH_OSTotalAmount.ToString(oSCurrencyDecimals), newInvoice.AH_RX_NKTransactionCurrency, newInvoice.ExpectedInvoiceTotal.ToString(oSCurrencyDecimals), newInvoice.UnallocatedInvoiceTotal.ToString(oSCurrencyDecimals));

			Assert(!newInvoice.ExpectedInvoiceTotalInfo.HasError(expectedErrorMessage));

			newInvoice.ValidateExpectedInvoiceTotal = true;
			testValidation.ValidateExpectedInvoiceTotal();

			Assert("Should have error", newInvoice.ExpectedInvoiceTotalInfo.HasError(expectedErrorMessage));

			using (newInvoice.SuspendExpectedInvoiceTotalValidation)
			{
				testValidation.ValidateExpectedInvoiceTotal();

				Assert("Should not have error", !newInvoice.ExpectedInvoiceTotalInfo.HasError(expectedErrorMessage));
			}
		}

		public virtual void TestValidateAH_LocalOutstandingAmount()
		{
			ZQuery filter = new ZQuery(OrgCompanyDataSchema.OB_OH, TestOrg.PK);
			OrgCompanyData companyData = Factory.LoadTop1<OrgCompanyData>(filter);
			companyData.OB_APCreditLimit = 10m;
			companyData.OB_ARCreditLimit = 10m;

			Factory.Save();

			InvoicingBase newInvoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			newInvoice.AH_OH = TestOrg.PK;

			AssertEquals("OrgMiscServ Credit Limit should be 10", 10m, newInvoice.Header.MiscServ.OM_APCreditLimit);
			AssertEquals("OrgMiscServ Credit Limit should be 10", 10m, newInvoice.Header.MiscServ.OM_ARCreditLimit);

			newInvoice.Lines.AddNew();
			newInvoice.AH_LocalExTaxAmount = 110m;

			if (newInvoice is ARInvoice || newInvoice is ARAdjustmentNote)
			{
				AssertHasWarnings("Should be a warning", newInvoice.AH_OHInfo);
			}
			else
			{
				AssertNoWarnings("Should be no warning", newInvoice.AH_OHInfo);
			}
		}

		public virtual void TestCheckAH_OHCreditOnHold()
		{
			SetInvoices();

			InvoicingBase invoiceToTest = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			InvoiceBaseValidation testValidation = GetValidation(invoiceToTest);
			invoiceToTest.AH_OH = TestOrg.PK;

			TestOrg.MiscServ.OM_AROnCreditHold = ZBool.True;

			Factory.Save();
			TestOrg.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

			testValidation.ValidateAH_OH();

			if (invoiceToTest.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				AssertCreditOnHoldError(invoiceToTest.AH_OHInfo);
			}
			else
			{
				AssertNoErrors(invoiceToTest.AH_OHInfo);
			}
		}

		protected virtual void AssertCreditOnHoldError(ZPropertyInfo aH_OHInfo)
		{
			Assert(aH_OHInfo.HasError("This account is on credit hold and cannot be billed to"));
		}

		public void TestCheckAH_DueDateMandatory()
		{
			InvoicingBase invoiceToTest = TestObjectCreator.CreateInvoiceWithLine(InvoiceType, "12341234", TestObjectCreator.AUD, 1.0m, 200m, 0m, 200m, 0m);
			invoiceToTest.AH_DueDate = ZDateTime.Empty;
			InvoiceBaseValidation testValidation = GetValidation(invoiceToTest);
			testValidation.ValidateAH_DueDate();
			Assert(invoiceToTest.AH_DueDateInfo.HasError("Please enter a Due Date."));
		}

		public virtual void TestCheckAH_OHCreditLimit()
		{
			InvoicingBase invoiceToTest = TestObjectCreator.CreateInvoiceWithLine(InvoiceType, "12341234", TestObjectCreator.AUD, 1.0m, 200m, 0m, 200m, 0m);
			InvoiceBaseValidation testValidation = GetValidation(invoiceToTest);
			TestOrg.CompanyData.OB_APPaymentTerms = Core.Constants.InvoiceTerms.FromMonthEnd;
			invoiceToTest.AH_OH = TestOrg.PK;
			Factory.Save();

			testValidation.ValidateAH_OH();

			Assert("Should be no errors", !invoiceToTest.AH_OHInfo.HasErrors());
			Assert("Should be no errors", !invoiceToTest.AH_OA_InvoiceAddressOverrideInfo.HasErrors());
			Assert("Should be no warnings", !invoiceToTest.AH_OHInfo.HasWarnings());
			Assert("Should be no warnings", !invoiceToTest.AH_OA_InvoiceAddressOverrideInfo.HasWarnings());

			TestOrg.MiscServ.OM_APCreditLimit = 100m;
			TestOrg.MiscServ.OM_ARCreditLimit = 100m;
			TestOrg.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			Factory.Save();

			testValidation.ValidateAH_OH();
			testValidation.ValidateAH_OA_InvoiceAddressOverride();

			Assert("Should be no errors", !invoiceToTest.AH_OHInfo.HasErrors());
			Assert("Should be no errors", !invoiceToTest.AH_OA_InvoiceAddressOverrideInfo.HasErrors());

			if (invoiceToTest is ARInvoice || invoiceToTest is ARAdjustmentNote)
			{
				AssertHasWarnings("Should be warnings", invoiceToTest.AH_OHInfo);
				AssertHasWarnings("Should be warnings", invoiceToTest.AH_OA_InvoiceAddressOverrideInfo);
			}
			else
			{
				AssertNoWarnings("Should be no warnings", invoiceToTest.AH_OHInfo);
				AssertNoWarnings("Should be no warnings", invoiceToTest.AH_OA_InvoiceAddressOverrideInfo);
			}
		}

		public void TestCheckAH_OH_IsValid()
		{
			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			var validation = GetValidation(invoice);
			var validAccount = invoice.AH_Ledger == LedgerTypes.AccountsReceivable ? TestObjectCreator.Debtor.PK : TestObjectCreator.Creditor1.PK;
			var invalidAccount = invoice.AH_Ledger != LedgerTypes.AccountsReceivable ? TestObjectCreator.Debtor.PK : TestObjectCreator.Creditor1.PK;
			invoice.AH_OH = invalidAccount;
			validation.ValidateAH_OH();
			AssertHasError(invoice.AH_OHInfo, "Enter a valid Account.");
			invoice.AH_OH = validAccount;
			validation.ValidateAH_OH();
			AssertNoErrors(invoice.AH_OHInfo);
		}

		public void TestCheckAH_OHIsActive()
		{
			Org1.OH_IsActive = false;
			Org1.CompanyData.OB_IsCreditor = false;
			Org1.CompanyData.OB_IsDebtor = false;

			Factory.Save();

			InvoicingBase invoiceToTest = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoiceToTest.AH_OH = Org1.PK;

			AssertHasError(invoiceToTest.AH_OHInfo, "This Account is inactive - it may not be used.");
		}

		public void TestCheckAmendingTransactionAH_OH()
		{
			var org1 = TestObjectCreator.CreateOrgHeader("AA1", true, true);
			var org2 = TestObjectCreator.CreateOrgHeader("AA2", true, true);

			var originalInvoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			if (originalInvoice is IAmending originalAmending && originalInvoice.HasImplementedGenerateAmendingTransaction)
			{
				originalInvoice.AH_OH = org1.PK;
				Factory.Save();

				var invoiceToTest = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				invoiceToTest.AH_OH = org2.PK;
				var testValidation = GetValidation(invoiceToTest);
				AssertNotEquals("Should have different accounts", originalInvoice.AH_OH, invoiceToTest.AH_OH);

				AssertEquals("Not an Amending Transaction yet", false, (invoiceToTest as IAmending).IsAmendingTransaction);
				testValidation.ValidateAH_OH();
				AssertNoErrors("No errors so far", invoiceToTest.AH_OHInfo);

				invoiceToTest = originalAmending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest.AH_OH = org2.PK;

				AssertEquals("Should be Amending Transaction", true, (invoiceToTest as IAmending).IsAmendingTransaction);
				testValidation.ValidateAH_OH();
				AssertHasErrorContaining(invoiceToTest.AH_OHInfo, "This is an amending transaction and must have the same account as the original transaction.");

				invoiceToTest.AH_OH = originalInvoice.AH_OH;
				AssertNoErrors("No errors because Accounts match", invoiceToTest.AH_OHInfo);
			}
			else
			{
				Assert("Not applicable because it does not implement IAmending", true);
			}
		}

		public void TestCheckAmendingTransactionAH_OH_SkippedForManuallyAmendingCreditNote()
		{
			var transactions = new InvoicingBase[] { Factory.NewWithValidTestData<ARCreditNote>(), Factory.NewWithValidTestData<APCreditNote>() };
			foreach (var creditNote in transactions)
			{
				creditNote.AH_OriginalTransactionNum = "123456";
				AssertNull(creditNote.OriginalTransaction);
				AssertEquals("Should be Amending Transaction", true, (creditNote as IAmending).IsAmendingTransaction);
				creditNote.AH_OH = creditNote is ARCreditNote ? TestObjectCreator.ABIGAS.PK : TestObjectCreator.AALSHI.PK;
				((InvoiceBaseValidation)creditNote.Validation).ValidateAH_OH();
				AssertNoErrors(creditNote.AH_OHInfo);
			}
		}

		public void TestCheckAmendingTransactionAH_LocalTotalAmount_SkippedForManuallyAmendingCreditNote()
		{
			var transactions = new InvoicingBase[] { Factory.NewWithValidTestData<ARCreditNote>(), Factory.NewWithValidTestData<APCreditNote>() };
			foreach (var creditNote in transactions)
			{
				creditNote.AH_OriginalTransactionNum = "123456";
				creditNote.AH_OH = TestObjectCreator.LocalClient.PK;
				creditNote.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				creditNote.ExchangeRate.Currency = "USD";
				creditNote.AH_InvoiceAmount = 5m;
				creditNote.AH_LocalTaxAmount = 1m;
				//creditNote.AH_GSTAmount = 0;
				AssertNull(creditNote.OriginalTransaction);
				AssertEquals("Should be Amending Transaction", true, (creditNote as IAmending).IsAmendingTransaction);
				((InvoiceBaseValidation)creditNote.Validation).ValidateAH_LocalTotalAmount();
				AssertNoErrors(creditNote.AH_LocalTotalAmountInfo);
				((InvoiceBaseValidation)creditNote.Validation).ValidateAH_LocalTaxAmount();
				AssertNoErrors(creditNote.AH_LocalTaxAmountInfo);
				((InvoiceBaseValidation)creditNote.Validation).ValidateAH_OSTaxAmount();
				AssertNoErrors(creditNote.AH_OSTaxAmountInfo);
			}
		}

		public void TestRunCreditLimitChecking_WhenAmendingTransactionAndDebtorIsCreditOnHold()
		{
			var originalInvoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			if (originalInvoice is IAmending amending && originalInvoice.HasImplementedGenerateAmendingTransaction && !(originalInvoice is APInvoice))
			{
				originalInvoice.AH_OH = TestOrg.PK;
				TestOrg.MiscServ.OM_AROnCreditHold = true;
				Factory.Save();

				TestOrg.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

				var invoiceToTest = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				invoiceToTest.AH_OH = TestOrg.PK;
				var testValidation = GetValidation(invoiceToTest);

				AssertEquals("Not an Amending Transaction", false, (invoiceToTest as IAmending).IsAmendingTransaction);
				testValidation.ValidateAH_OH();
				AssertHasError(invoiceToTest.AH_OHInfo, "This account is on credit hold and cannot be billed to");

				invoiceToTest = amending.GenerateAmendingTransaction(InvoiceType);
				invoiceToTest.AH_OH = TestOrg.PK;
				AssertEquals("Should be Amending Transaction", true, (invoiceToTest as IAmending).IsAmendingTransaction);
				testValidation.ValidateAH_OH();
				AssertNoErrors("Amending Transaction, do not check credit on hold", invoiceToTest.AH_OHInfo);
			}
			else
			{
				Assert("Not applicable because it does not implement IAmending", true);
			}
		}

		public void TestCheckAH_OH_AgreedPaymentMethodOverride()
		{
			var testOrg = TestObjectCreator.CreateOrgHeader("Test1", true, true);
			testOrg.CompanyData.OB_ARCreditAgreedPaymentMethod = "XYZ";
			testOrg.CompanyData.OB_APCreditAgreedPaymentMethod = "XYZ";
			var invoiceToTest = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoiceToTest.AH_OH = testOrg.PK;
			InvoiceBaseValidation testValidation = GetValidation(invoiceToTest);
			testValidation.ValidateAH_OH();
			AssertHasError("Expect invalid agreed payment method error", invoiceToTest.AH_OHInfo, "This organization has an invalid agreed payment method 'XYZ'. Please check organization setup and the registry setting at Organization > AR/AP > Agreed Payment Method.");
		}

		public void TestCheckOrgsRegistrationNumber()
		{
			var testOrg = TestObjectCreator.CreateOrgHeader("Test1", true, true);
			Assert(testOrg.PrimaryRegistrationNumber.Number.IsEmpty);
			var invoiceToTest = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoiceToTest.AH_OH = testOrg.PK;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				InvoiceBaseValidation testValidation = GetValidation(invoiceToTest);
				testValidation.ValidateAH_OH();
				AssertHasWarning("Expect invalid agreed payment method error", invoiceToTest.AH_OHInfo, "Tax Registration Number is missing. This is mandatory for your country/region reporting.");
			}
		}

		public void TestValidateOrgDependantLineItemsForGST()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);

			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			invoice.AH_OH = GSTRegisteredOrg.PK;

			invoice.Lines.AddNew();
			invoice.Lines.AddNew();

			invoice.Lines[0].AL_AT = GST10TaxRate.PK;

			((InvoiceBaseValidation)invoice.Validation).ValidateOrgDependantLineItems();

			Assert(!invoice.Lines[0].AL_ATInfo.HasError("Please enter a Tax ID."));
			Assert(invoice.Lines[1].AL_ATInfo.HasError("Please enter a Tax ID."));
		}

		public void TestCheckGSTInclusiveAmounts()
		{
			InvoicingBase newInvoice = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			newInvoice.AH_OH = TestOrg.PK;
			newInvoice.Lines.AddNew();
			newInvoice.Lines.AddNew();
			newInvoice.GSTInclusiveAmounts = true;
			newInvoice.Lines[0].AL_OSExTaxAmount = 120m;
			newInvoice.Lines[1].AL_OSExTaxAmount = 33m;

			newInvoice.Lines[0].Validation.ValidateAll();
			newInvoice.Lines[1].Validation.ValidateAll();

			Assert("Should has errors.", newInvoice.Lines[0].GSTInclusiveAmountInfo.HasErrors());
			Assert("Should has errors.", newInvoice.Lines[1].GSTInclusiveAmountInfo.HasErrors());

			newInvoice.GSTInclusiveAmounts = false;
			InvoiceBaseValidation testValidation = GetValidation(newInvoice);
			testValidation.CheckGSTInclusiveAmounts_ForTestOnly();

			Assert("Should hasn't errors.", !newInvoice.Lines[0].GSTInclusiveAmountInfo.HasErrors());
			Assert("Should hasn't errors.", !newInvoice.Lines[1].GSTInclusiveAmountInfo.HasErrors());
		}

		public void TestCheckOutstandingAmountAndFullyPaidDateAreValid()
		{
			ARInvoice aRInvoiceToTest = Factory.NewWithValidTestData<ARInvoice>();
			InvoiceBaseValidation testValidation = new InvoiceBaseValidation(aRInvoiceToTest);

			testValidation.ValidateAll();

			Assert("Should be valid", !aRInvoiceToTest.Notifications.ContainsNotificationContaining(OutstandingAmountAndFullyPaidDateError));

			aRInvoiceToTest.AH_InvoiceAmount = 5m;
			aRInvoiceToTest.Lines.AddNew().AL_LineAmount = 5m;
			aRInvoiceToTest.AH_OutstandingAmount = ZDecimal.Zero;
			TransactionMatchLink matchLink = ((IMatching)aRInvoiceToTest).CurrentMatchGroup.AddNew();
			matchLink.AP_AH = aRInvoiceToTest.PK;
			matchLink.AP_Amount = 5m;
			aRInvoiceToTest.AH_FullyPaidDate = ZDateTime.Empty;

			ExceptionReporterTestListener.Instance.Clear();
			testValidation.ValidateAll();
			Assert("Critical Validation moved on saving, so this error shouldn't be shown here", !aRInvoiceToTest.Notifications.ContainsNotificationContaining(OutstandingAmountAndFullyPaidDateError));
		}

		public void TestCreditLimitCheck()
		{
			InvoicingBase invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", TestObjectCreator.AUD, 1.0m, 1000m, 0m, 1000m, 0m);
			OrgHeader org = TestObjectCreator.ABIGAS;
			org.CompanyData.OB_ARCreditLimit = 500m;
			invoice.AH_OH = org.PK;
			Factory.Save();

			InvoicingBase invoice2 = new TestObjectCreator(new BusinessObjectFactory()).CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m);
			invoice2.AH_OH = org.PK;

			int decimalPlaces = GlbCompany.CurrentCompany.LocalCurrency.Decimals;
			string warning = string.Format("The Credit Limit for {0} is set to {1} {3}. Credit approved.\r\nThe Total Outstanding Balance is {2} {3}, which is over the credit limit.", org.OH_Code.Trim(), numberFormat(500.00m, decimalPlaces), numberFormat(1000.00m, decimalPlaces), "AUD");
			warning += "\r\n\r\nThe Total Outstanding Balance is calculated by summing the following:";
			warning += string.Format("\r\n  *  the Posted Outstanding Balance, {0} {1}", numberFormat(1000.00m, decimalPlaces), "AUD");
			AssertHasWarning(invoice2.AH_OHInfo, warning);
		}

		public void TestARCreditLimitCheckIsDebtorSet()
		{
			var org = TestObjectCreator.ABIGAS;
			org.CompanyData.OB_ARCreditLimit = 500m;
			org.CompanyData.OB_IsCreditor = true;
			org.CompanyData.OB_IsDebtor = false;
			Factory.Save();

			var invoice1 = new TestObjectCreator(new BusinessObjectFactory()).CreateInvoiceWithLine(typeof(ARInvoice), "00001000", TestObjectCreator.AUD, 1.0m, 1000m, 0m, 1000m, 0m);
			invoice1.AH_OH = org.PK;

			var decimalPlaces = GlbCompany.CurrentCompany.LocalCurrency.Decimals;
			var warning = string.Format("The Credit Limit for {0} is set to {1} {3}. Credit approved.\r\nThe Total Outstanding Balance is {2} {3}, which is over the credit limit.", org.OH_Code.Trim(), numberFormat(500.00m, decimalPlaces), numberFormat(1000.00m, decimalPlaces), "AUD");
			warning += "\r\n\r\nThe Total Outstanding Balance is calculated by summing the following:";
			warning += string.Format("\r\n  *  the Posted Outstanding Balance, {0} AUD", numberFormat(0.00m, decimalPlaces), "AUD");
			warning += string.Format("\r\n  *  the Current Transaction Amount, {0} AUD", numberFormat(1000.00m, decimalPlaces), "AUD");

			AssertNoWarning(invoice1.AH_OHInfo, warning);

			org.CompanyData.OB_IsCreditor = false;
			org.CompanyData.OB_IsDebtor = true;
			Factory.Save();

			var invoice2 = new TestObjectCreator(new BusinessObjectFactory()).CreateInvoiceWithLine(typeof(ARInvoice), "00001001", TestObjectCreator.AUD, 1.0m, 1000m, 0m, 1000m, 0m);
			invoice2.AH_OH = org.PK;
			AssertHasWarning(invoice2.AH_OHInfo, warning);
		}

		public void TestCreditLimitCheckWithOrgNotDebtor()
		{
			InvoicingBase invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", TestObjectCreator.AUD, 1.0m, 1000m, 0m, 1000m, 0m);
			OrgHeader org = TestObjectCreator.ABIGAS;
			org.CompanyData.OB_IsDebtor = false;
			org.CompanyData.OB_ARCreditLimit = 500m;
			invoice.AH_OH = org.PK;
			Factory.Save();

			InvoicingBase invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m);
			invoice2.AH_OH = org.PK;
			Factory.Save();

			int decimalPlaces = GlbCompany.CurrentCompany.LocalCurrency.Decimals;
			string warning = string.Format("The Credit Limit for {0} is set to {1} {3}. Credit approved.\r\nThe Total Outstanding Balance is {2} {3}, which is over the credit limit.", org.OH_Code.Trim(), numberFormat(500.00m, decimalPlaces), numberFormat(1000.00m, decimalPlaces), "AUD");
			warning += "\r\n\r\nThe Total Outstanding Balance is calculated by summing the following:";
			warning += string.Format("\r\n  *  the Posted Outstanding Balance, {0} {1}", numberFormat(1000.00m, decimalPlaces), "AUD");
			AssertEquals("invoice2 has no warning since org is not set to debtor", false, invoice2.AH_OHInfo.HasWarning(warning));
		}

		public void TestCheckAH_PostDateForIncompleteInvoiceWithBackPosting()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001000", TestObjectCreator.AUD, 1.0m, 20m, 0m, 20m, 0m, TestObjectCreator.AALSHI, ZGuid.Empty);
			invoice.AH_PostDate = ZDateTime.Today.AddDays(-50);
			invoice.SaveAsIncomplete();
			InvoiceBaseValidation validation = new InvoiceBaseValidation(invoice);

			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			validation.ValidateAH_PostDate();
			AssertEquals("Precondition: invoice.AllowBackPosting", true, invoice.AllowBackPosting);
			AssertNoErrors("If post date is in the past, and back posting is allowed, then AH_PostDate should have no errors", invoice.AH_PostDateInfo);
			AssertHasWarning("If post date is in the past, then the AH_PostDate should have warnings", invoice.AH_PostDateInfo, "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems \r\n - Financial Reports\r\n - Sub-Ledger Reports\r\n - Bank Reconciliation\r\n - Reversing");

			Action assertInvoiceHasError = () =>
			{
				validation.ValidateAH_PostDate();
				AssertEquals("Precondition: invoice.AllowBackPosting", false, invoice.AllowBackPosting);
				AssertHasError("If post date is in the past, and back posting is NOT allowed, then AH_PostDate should have errors", invoice.AH_PostDateInfo, "The post date cannot be in the past");
				AssertNoWarnings("If post date is in the past, then the AH_PostDate should have warnings", invoice.AH_PostDateInfo);
			};

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			assertInvoiceHasError();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			assertInvoiceHasError();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			assertInvoiceHasError();
		}

		public void TestCheckAH_PostDateForIncompleteInvoiceWithNoBackPosting()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001000", TestObjectCreator.AUD, 1.0m, 20m, 0m, 20m, 0m, TestObjectCreator.AALSHI, ZGuid.Empty);
			invoice.AH_PostDate = ZDateTime.Today;
			invoice.SaveAsIncomplete();
			InvoiceBaseValidation validation = new InvoiceBaseValidation(invoice);

			Action assertInvoiceHasError = () =>
			{
				validation.ValidateAH_PostDate();
				AssertNoErrors("If post date is today, then the AH_PostDate shouldn't have errors regardless of registry setting", invoice.AH_PostDateInfo);
				AssertNoWarnings("If post date is today, then the AH_PostDate shouldn't have warnings regardless of registry setting", invoice.AH_PostDateInfo);
			};

			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition: invoice.AllowBackPosting", true, invoice.AllowBackPosting);
			assertInvoiceHasError();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AssertEquals("Precondition: invoice.AllowBackPosting", false, invoice.AllowBackPosting);
			assertInvoiceHasError();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AssertEquals("Precondition: invoice.AllowBackPosting", false, invoice.AllowBackPosting);
			assertInvoiceHasError();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AssertEquals("Precondition: invoice.AllowBackPosting", false, invoice.AllowBackPosting);
			assertInvoiceHasError();
		}

		public override void TestCheckAH_ComplianceSubType()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Spain);
			new AccountingPeriodTestHelper().SetupPeriods();

			var invoice = TestObjectCreator.CreateInvoiceWithLine(this.InvoiceType, "00001000", TestObjectCreator.AUD, 1.0m, 20m, 0m, 20m, 0m, TestObjectCreator.AALSHI, ZGuid.Empty);
			Factory.Save();

			var newFactory = new BusinessObjectFactory(); // Report should be loaded in a separate Factory as it saves own factory on applying new Status
			var report = newFactory.NewWithValidTestData<AccComplianceReport>();
			newFactory.Save();

			if (!invoice.AH_ComplianceSubType.IsEmpty)
			{
				invoice.AH_ComplianceSubType = "";  // To make the same starting point for all types of Transactions
				Factory.Save();
			}
			Assert("AH_ComplianceSubType HasChanges", !invoice.AH_ComplianceSubTypeInfo.HasChanges);
			Assert("AH_ComplianceSubType IsEmpty", invoice.AH_ComplianceSubType.IsEmpty);

			AssertNoErrors(invoice.AH_ComplianceSubTypeInfo);
			var validation = new InvoiceBaseValidation(invoice);

			invoice.AH_ComplianceSubType = "TXI";
			Assert("AH_ComplianceSubType HasChanges", invoice.AH_ComplianceSubTypeInfo.HasChanges);
			AssertNoErrors(invoice.AH_ComplianceSubTypeInfo);

			TestObjectCreator.CreateComplianceReportQueueEntry(report, invoice);
			validation.ValidateAH_ComplianceSubType();
			AssertNoErrors(invoice.AH_ComplianceSubTypeInfo);

			TestObjectCreator.CreateComplianceReportTransactionPivot(report, invoice);
			validation.ValidateAH_ComplianceSubType();
			AssertNoErrors(invoice.AH_ComplianceSubTypeInfo);

			report.Finalise();
			invoice.AH_ComplianceSubType = "TCR";   // To make HasChanges true as Finalise saves Factory
			Assert("AH_ComplianceSubType HasChanges", invoice.AH_ComplianceSubTypeInfo.HasChanges);

			validation.ValidateAH_ComplianceSubType();
			AssertHasError(invoice.AH_ComplianceSubTypeInfo, string.Format("Cannot be changed because transaction is already included in finalized Compliance Reports of the following types: {0}.", report.ACR_ReportType));
		}

		public void TestCheckAH_ComplianceSubTypeBlankNotAllowedValidation()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var registry = AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype;
			var complianceNumberAllocationDateRegistry = this.Header.AH_Ledger == LedgerTypes.AccountsReceivable ? AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR : AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP;
			var dateOptions = this.Header.AH_Ledger == LedgerTypes.AccountsReceivable ? new[] { AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code }
			: new[] { AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code };

			foreach (var dateOption in dateOptions)
			{
				using (complianceNumberAllocationDateRegistry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dateOption))
				using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var invoice = TestObjectCreator.CreateInvoiceWithLine(this.InvoiceType, "00001001", TestObjectCreator.AUD, 1.0m, 20m, 0m, 20m, 0m, TestObjectCreator.AALSHI, ZGuid.Empty);
					invoice.AH_ComplianceSubType = "";
					if (invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions && invoice.AH_TransactionType == TransactionTypes.UACreditNote)
					{
						invoice.RunPreSaveValidation();
						AssertNoErrors(invoice.AH_ComplianceSubTypeInfo);
					}
					else
					{
						invoice.RunPreSaveValidation();
						AssertHasError(invoice.AH_ComplianceSubTypeInfo, "You cannot post invoices with a blank compliance sub-type. Please check the registry setting at Accounting > Government Compliance Invoice Document > Disallow posting transactions with empty compliance subtype.");
					}
				}
			}

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			{
				var invoice = TestObjectCreator.CreateInvoiceWithLine(this.InvoiceType, "00001000", TestObjectCreator.AUD, 1.0m, 20m, 0m, 20m, 0m, TestObjectCreator.AALSHI, ZGuid.Empty);
				invoice.AH_ComplianceSubType = "";
				invoice.RunPreSaveValidation();
				AssertNoErrors(invoice.AH_ComplianceSubTypeInfo);
			}

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			{
				var invoice = TestObjectCreator.CreateInvoiceWithLine(this.InvoiceType, "00001000", TestObjectCreator.AUD, 1.0m, 20m, 0m, 20m, 0m, TestObjectCreator.AALSHI, ZGuid.Empty);
				invoice.AH_ComplianceSubType = "";
				invoice.RunPreSaveValidation();
				AssertHasError(invoice.AH_ComplianceSubTypeInfo, "You cannot post invoices with a blank compliance sub-type. Please check the registry setting at Accounting > Government Compliance Invoice Document > Disallow posting transactions with empty compliance subtype.");
			}
		}

		public void TestCheckAH_ComplianceSubType_China()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
			var complianceSubTypeValidation = new Mock<IComplianceSubTypeValidation>();

			complianceSubTypeValidation.Setup(x => x.ErrorMessageForComplianceSubTypeValidation(It.IsAny<AccTransactionHeader>())).Returns("error message");
			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeValidation(It.IsAny<ZString>())).Returns(complianceSubTypeValidation.Object);

			var invoice = TestObjectCreator.CreateInvoiceWithLine(this.InvoiceType, "00001000", TestObjectCreator.AUD, 1.0m, 20m, 0m, 20m, 0m, TestObjectCreator.AALSHI, ZGuid.Empty);

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				invoice.AH_ComplianceSubType = "TXB";
				Factory.Save();

				Assert("AH_ComplianceSubType HasChanges", !invoice.AH_ComplianceSubTypeInfo.HasChanges);
				AssertHasError(invoice.AH_ComplianceSubTypeInfo, "error message");
			}

			countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
			complianceSubTypeValidation = new Mock<IComplianceSubTypeValidation>();

			complianceSubTypeValidation.Setup(x => x.ErrorMessageForComplianceSubTypeValidation(It.IsAny<AccTransactionHeader>())).Returns(string.Empty);
			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeValidation(It.IsAny<ZString>())).Returns(complianceSubTypeValidation.Object);

			using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				invoice.AH_ComplianceSubType = "TXA";
				Factory.Save();

				Assert("AH_ComplianceSubType HasChanges", !invoice.AH_ComplianceSubTypeInfo.HasChanges);
				AssertNoErrors(invoice.AH_ComplianceSubTypeInfo);
			}
		}

		public void TestCheckAH_ComplianceSubType_IsComplianceSubTypeValueProtected()
		{
			var protectionErrorMessage = "You cannot change the original 'PIN' value of Compliance Sub Type for Payables e-Reporting transactions.";

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var transaction = Factory.NewWithValidTestData(HeaderType) as InvoicingBase;
				transaction.AH_ComplianceSubType = "PIN";
				AssertNoErrors(transaction.AH_ComplianceSubTypeInfo);
				Factory.Save();

				var validation = new InvoiceBaseValidation(transaction);

				var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();

				var complianceSubTypeProtectValidation = new Mock<IProtectComplianceSubTypeForEInvoicingTransactions>();
				complianceSubTypeProtectValidation.Setup(x => x.ErrorMessageIfComplianceSubTypeIsProtected(It.IsAny<AccTransactionHeader>())).Returns(transaction.IsAPInvoice || transaction.IsARCreditNote ? protectionErrorMessage : "");

				var complianceSubTypeValidation = new Mock<IComplianceSubTypeValidation>();
				complianceSubTypeValidation.Setup(x => x.ErrorMessageForComplianceSubTypeValidation(It.IsAny<AccTransactionHeader>())).Returns("");

				countryComplianceFactoryMock.Setup(c => c.GetIProtectComplianceSubTypeForEInvoicingTransactions(It.IsAny<ZString>())).Returns(complianceSubTypeProtectValidation.Object);
				countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeValidation(It.IsAny<ZString>())).Returns(complianceSubTypeValidation.Object);

				ObjectFactory.Substitute(countryComplianceFactoryMock.Object);

				transaction.AH_ComplianceSubType = "PAR";
				validation.ValidateAH_ComplianceSubType();

				// In Turkey, when AP Invoices are allocated as Return of Sales, they will be AR Credit Note. AP Credit Note will be Return of Purchases and its behavior will be like AR Invoice.
				if (transaction.IsAPInvoice || transaction.IsARCreditNote)
				{
					AssertHasError(transaction.AH_ComplianceSubTypeInfo, protectionErrorMessage);
					complianceSubTypeProtectValidation.Verify(x => x.ErrorMessageIfComplianceSubTypeIsProtected(It.IsAny<AccTransactionHeader>()), Times.Once);
					complianceSubTypeValidation.Verify(x => x.ErrorMessageForComplianceSubTypeValidation(It.IsAny<AccTransactionHeader>()), Times.Once);
				}
				else
				{
					AssertNoErrors(transaction.AH_ComplianceSubTypeInfo);
					complianceSubTypeProtectValidation.Verify(x => x.ErrorMessageIfComplianceSubTypeIsProtected(It.IsAny<AccTransactionHeader>()), Times.Once);
					complianceSubTypeValidation.Verify(x => x.ErrorMessageForComplianceSubTypeValidation(It.IsAny<AccTransactionHeader>()), Times.Once);
				}

				complianceSubTypeProtectValidation.Setup(x => x.ErrorMessageIfComplianceSubTypeIsProtected(It.IsAny<AccTransactionHeader>())).Returns("");

				transaction.AH_ComplianceSubType = "PIC";
				validation.ValidateAH_ComplianceSubType();
				AssertNoErrors(transaction.AH_ComplianceSubTypeInfo);

				complianceSubTypeProtectValidation.Verify(x => x.ErrorMessageIfComplianceSubTypeIsProtected(It.IsAny<AccTransactionHeader>()), Times.Exactly(2));
				complianceSubTypeValidation.Verify(x => x.ErrorMessageForComplianceSubTypeValidation(It.IsAny<AccTransactionHeader>()), Times.Exactly(2));
			}
		}

		public void TestCheckAH_ComplianceSubType_WarningMessage()
		{
			if (HeaderType == typeof(ARInvoice))
			{
				var mockIFeatureControlManager = new FeatureControlTestDataFactory().CreateKoreaSouthComplianceSubTypeFeatureControlMock();

				using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
					{
						var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
						arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
						arInvoice.AH_TransactionType = TransactionTypes.Invoice;
						arInvoice.AH_ComplianceSubType = "101";

						AssertHasWarning(arInvoice.AH_ComplianceSubTypeInfo, "You have selected a Compliance Sub Type manually. Please note that incorrect Compliance Sub Type allocation may result in a failure E-Reporting submission.");

						arInvoice = Factory.NewWithValidTestData<ARInvoice>();
						arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
						arInvoice.AH_TransactionType = TransactionTypes.Invoice;
						arInvoice.AH_ComplianceSubType = "999";

						AssertHasError("PreRequisite", arInvoice.AH_ComplianceSubTypeInfo, "The Compliance Sub Type of the Invoice should be '101', '102' or '301'.");
						AssertNoWarnings(arInvoice.AH_ComplianceSubTypeInfo);
					}

					using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Romania))
					{
						var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
						arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
						arInvoice.AH_TransactionType = TransactionTypes.Invoice;
						arInvoice.AH_ComplianceSubType = "101";

						AssertNoWarnings(arInvoice.AH_ComplianceSubTypeInfo);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		string numberFormat(decimal number, int decimalPlaces)
		{
			return number.ToString(string.Format("N{0}", decimalPlaces));
		}

		public void TestCheckAH_InvoiceDateWarnings()
		{
			if (InvoiceType == typeof(ARInvoice) || InvoiceType == typeof(ARCreditNote) || InvoiceType == typeof(ARAdjustmentNote))
			{
				var warningMsg = @"Invoice Date is non-editable as the Registry 'Invoice and Post Dates Defaulting Behavior' is set to 'MTH - Month End Suspension'.";
				var invoice = (InvoicingBase)Factory.New(InvoiceType);

				invoice.AH_InvoiceDate = ZDateTime.Now.AddDays(1);
				AssertNoWarning(invoice.AH_InvoiceDateInfo, warningMsg);

				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
				invoice.AH_InvoiceDate = ZDateTime.Now.AddDays(2);
				AssertNoWarning(invoice.AH_InvoiceDateInfo, warningMsg);

				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
				invoice.AH_InvoiceDate = ZDateTime.Now.AddDays(3);
				AssertHasWarning(invoice.AH_InvoiceDateInfo, warningMsg);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckAH_InvoiceDateInFutureWarningForPTLoginCompany()
		{
			var countries = new string[] { Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Portugal, Core.Constants.CountryCodes.Japan };

			foreach (var country in countries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					if (InvoiceType == typeof(ARInvoice) || InvoiceType == typeof(ARCreditNote) || InvoiceType == typeof(ARAdjustmentNote))
					{
						var warningMsg = @"Invoice Date is in the future. Please check the Invoice Date against the current system date and time. If this invoice is posted, future invoices cannot use current or previous date.";
						var invoice = (InvoicingBase)Factory.New(InvoiceType);
						invoice.AH_InvoiceDate = ZDateTime.Today;
						AssertNoWarning(invoice.AH_InvoiceDateInfo, warningMsg);
						invoice.AH_InvoiceDate = ZDateTime.Today.AddDays(1);
						if (country == Core.Constants.CountryCodes.Portugal)
						{
							AssertHasWarning(invoice.AH_InvoiceDateInfo, warningMsg);
						}
						else
						{
							AssertNoWarning(invoice.AH_InvoiceDateInfo, warningMsg);
						}
					}
					else
					{
						Assert(true);
					}
				}
			}
		}

		public void TestCheckAH_InvoiceDate_AddErrorIfInvoiceDateIsInTheFuture()
		{
			if (InvoiceType == typeof(ARInvoice) || InvoiceType == typeof(ARCreditNote) || InvoiceType == typeof(ARAdjustmentNote))
			{
				var errorMsg = @"The invoice date cannot be in the future because the registry 'Accounting > Receivable > Default Settings > Disallow Posting Invoices With A Future Invoice Date' is set to Yes.";
				var invoice = (InvoicingBase)Factory.New(InvoiceType);

				AccountingMasterFilesRegistry.Instance.DisallowPostingInvoicesWithAFutureInvoiceDate.SetValue(invoice.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, true);

				invoice.AH_InvoiceDate = ZDateTime.Today;
				AssertNoError(invoice.AH_InvoiceDateInfo, errorMsg);
				invoice.AH_InvoiceDate = ZDateTime.Today.AddDays(1);
				AssertHasError(invoice.AH_InvoiceDateInfo, errorMsg);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckAH_InvoiceDate_IInvoiceDateValidation()
		{
			var invoice = (InvoicingBase)Factory.New(InvoiceType);

			var validationMock = new Mock<IInvoiceDateValidation>();
			var countryFactoryMock = new Mock<IAccountingCountryFactory>();
			countryFactoryMock.As<IInstanceProvider<IInvoiceDateValidation>>().Setup(x => x.Get()).Returns(validationMock.Object);
			var factoryMock = new Mock<IGlobalAccountingCountryFactory>();
			factoryMock.Setup(c => c.GetCountryFactory(It.IsAny<ZString>())).Returns(countryFactoryMock.Object);

			using (ObjectFactory.Substitute(factoryMock.Object))
			{
				validationMock.Setup(x => x.ValidateInvoiceDate(It.IsAny<AccTransactionHeader>())).Returns((ResourceString)null);
				invoice.AH_InvoiceDate = ZDateTime.Now;
				AssertNoErrors(invoice.AH_InvoiceDateInfo);

				validationMock.Reset();

				validationMock.Setup(x => x.ValidateInvoiceDate(It.IsAny<AccTransactionHeader>())).Returns(ResString.GetMultilingualString("Test", "Dummy Error"));
				invoice.AH_InvoiceDate = ZDateTime.Now.AddDays(1);
				AssertHasError(invoice.AH_InvoiceDateInfo, "Dummy Error");
			}
		}

		public void TestCheckAH_InvoiceDateWarningsForSavedData()
		{
			var invoice = (InvoicingBase)Factory.New(InvoiceType);

			var postingExRateRegistry = invoice.GetExRateLedger() == ExchangeRateValidLedgerEnum.AR
				? AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR
				: AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 5m, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 6m, ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(1));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, 5m, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, 6m, ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(1));
			ExchangeRateReader.GetReaderInstance().ClearCache();

			invoice.FillWithValidTestData();
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;

			if (invoice is not TransactionPendingAllocation)
			{
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
				invoice.AH_InvoiceDate = ZDateTime.Today;
			}

			Factory.Save();

			var testValidation = new InvoiceBaseValidation(invoice);
			invoice.AH_InvoiceDate = ZDateTime.Today.AddDays(1);
			testValidation.ValidateAH_InvoiceDate();

			if (!invoice.ShouldSetExchangeRateWhenSetInvoiceDate)
			{
				var warningMsg = @"Overriding the 'Invoice Date' will not result in the exchange rate being updated with reference to the 'AP Invoice Posting Exchange Rate Option' registry value.";
				AssertHasWarning(invoice.AH_InvoiceDateInfo, warningMsg);

				postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code);
				testValidation.ValidateAH_InvoiceDate();
				AssertNoWarnings(invoice.AH_InvoiceDateInfo);
			}
			else
			{
				AssertNoWarnings(invoice.AH_InvoiceDateInfo);
			}
		}

		public override void TestCheckAH_PostDate()
		{
			if (InvoiceType == typeof(ARInvoice) || InvoiceType == typeof(ARCreditNote) || InvoiceType == typeof(ARAdjustmentNote))
			{
				var warningMsg = @"Post Date is non-editable as the Registry 'Invoice and Post Dates Defaulting Behavior' is set to 'MTH - Month End Suspension'.";
				var invoice = (InvoicingBase)Factory.New(InvoiceType);

				invoice.AH_PostDate = ZDateTime.Now.AddDays(1);
				AssertNoWarning(invoice.AH_PostDateInfo, warningMsg);

				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
				invoice.AH_PostDate = ZDateTime.Now.AddDays(2);
				AssertNoWarning(invoice.AH_PostDateInfo, warningMsg);

				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
				invoice.AH_PostDate = ZDateTime.Now.AddDays(3);
				AssertHasWarning(invoice.AH_PostDateInfo, warningMsg);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2020, 3, 15)]
		public void TestCheckAH_PostOrInvoiceDateForCompliance_INV()
		{
			AssertCheckAH_PostOrInvoiceDateForCompliance(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		[TestDate(2020, 3, 15)]
		public void TestCheckAH_PostOrInvoiceDateForCompliance_PST()
		{
			AssertCheckAH_PostOrInvoiceDateForCompliance(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		void AssertCheckAH_PostOrInvoiceDateForCompliance(string dateOption)
		{
			var regMaster = AccountingMasterFilesRegistry.Instance;
			string subType = null;
			CodePairRegistryItem complianceDocumentNumberAllocationRegistry = null;
			CodePairRegistryItem complianceNumberAllocationDateRegistry = null;
			switch (InvoiceType.Name)
			{
				case string x when x.Contains("AP"):
					if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
					{
						Assert(true);
						return; //Invoice date option is not available for AP registry
					}
					subType = ItalyComplianceInfo.ComplianceSubTypeCodes.API;
					complianceDocumentNumberAllocationRegistry = regMaster.ComplianceDocumentNumberAllocation_Payables;
					complianceNumberAllocationDateRegistry = regMaster.ComplianceNumberAllocationDate_AP;
					break;
				case string x when x.Contains("AR"):
					subType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
					complianceDocumentNumberAllocationRegistry = regMaster.ComplianceDocumentNumberAllocation_Receivables;
					complianceNumberAllocationDateRegistry = regMaster.ComplianceNumberAllocationDate_AR;
					break;
				default:
					Assert(true);
					return;
			}

			var now = ZDateTime.Now;
			var companyPK = Env.CurrentCompany.PK;

			new AccountingPeriodTestHelper().SetupPeriods();

			using (complianceDocumentNumberAllocationRegistry.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			using (complianceNumberAllocationDateRegistry.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, dateOption))
			using (AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, true))
			{
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				var sequence = TestObjectCreator.SetupComplianceSequence(menuPK, subType, subType + ".20-", 1, 100, 2);
				sequence.XD_StartDate = new ZDate(now.Year, now.Month, 1);
				sequence.XD_ExpiryDate = sequence.XD_StartDate.AddMonths(1).AddDays(-1);
				sequence.XD_IsActive = true;
				Factory.Save();

				var invoice = TestObjectCreator.CreateInvoice(InvoiceType);
				invoice.AH_GC = sequence.XD_GC_Company;
				invoice.AH_GB = sequence.XD_GB_BranchOwner;
				invoice.AH_ComplianceSubType = sequence.XD_SequenceClass;

				SetAllocationDate(invoice, now);
				AssertNoErrors(invoice.AH_PostDateInfo);
				AssertNoErrors(invoice.AH_InvoiceDateInfo);

				var previousInvoiceWithEarlierDate = TestObjectCreator.CreateInvoice(InvoiceType);
				previousInvoiceWithEarlierDate.AH_ComplianceSubType = sequence.XD_SequenceClass;
				previousInvoiceWithEarlierDate.AH_XD_ComplianceBook = sequence.PK;

				if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code)
				{
					previousInvoiceWithEarlierDate.AH_PostDate = now.AddDays(1);
					previousInvoiceWithEarlierDate.AH_InvoiceDate = now;
				}
				else
				{
					previousInvoiceWithEarlierDate.AH_PostDate = now;
					previousInvoiceWithEarlierDate.AH_InvoiceDate = now.AddDays(1);
				}

				Factory.Save();

				var invoice2 = TestObjectCreator.CreateInvoice(InvoiceType);
				invoice2.AH_GC = sequence.XD_GC_Company;
				invoice2.AH_GB = sequence.XD_GB_BranchOwner;
				invoice2.AH_ComplianceSubType = sequence.XD_SequenceClass;

				SetAllocationDate(invoice2, now);
				AssertAllocationDateError(invoice2, string.Format(ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToAllocationDateEarlierThanLastDateUsedMessage(dateOption),
					sequence.XD_SequenceClass, now.AddDays(1).ToShortDateString()));

				var invNoComplNr = TestObjectCreator.CreateInvoice(InvoiceType);
				invNoComplNr.AH_GC = sequence.XD_GC_Company;
				invNoComplNr.AH_GB = sequence.XD_GB_BranchOwner;
				invNoComplNr.AH_ComplianceSubType = sequence.XD_SequenceClass;
				SetAllocationDate(invNoComplNr, now.AddDays(-3));
				Factory.Save();

				previousInvoiceWithEarlierDate.AH_XD_ComplianceBook = ZGuid.Empty;
				Factory.Save();

				var inv2 = TestObjectCreator.CreateInvoice(InvoiceType);
				inv2.AH_GC = sequence.XD_GC_Company;
				inv2.AH_GB = sequence.XD_GB_BranchOwner;
				inv2.AH_ComplianceSubType = sequence.XD_SequenceClass;
				SetAllocationDate(inv2, now);
				AssertAllocationDateError(inv2, string.Format(ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToSparseComplianceBookMessage(dateOption),
					sequence.XD_SequenceClass, now.ToShortDateString()));

				var invNoSubType = TestObjectCreator.CreateInvoice(InvoiceType);
				invNoSubType.AH_GC = sequence.XD_GC_Company;
				invNoSubType.AH_GB = sequence.XD_GB_BranchOwner;
				SetAllocationDate(invNoSubType, now);
				AssertNoErrors(invoice.AH_PostDateInfo);
				AssertNoErrors(invoice.AH_InvoiceDateInfo);
			}

			void SetAllocationDate(InvoicingBase invoice, ZDateTime date)
			{
				if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code)
				{
					invoice.AH_PostDate = date;
					invoice.Validation.ValidateAH_InvoiceDate();
				}
				else if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
				{
					invoice.AH_InvoiceDate = date;
					invoice.Validation.ValidateAH_PostDate();
				}
			}

			void AssertAllocationDateError(InvoicingBase invoice, string expectedError)
			{
				if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code)
				{
					AssertHasError(invoice.AH_PostDateInfo, expectedError);
					AssertNoErrors(invoice.AH_InvoiceDateInfo);
				}
				else if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
				{
					AssertHasError(invoice.AH_InvoiceDateInfo, expectedError);
					AssertNoErrors(invoice.AH_PostDateInfo);
				}
			}
		}

		public void TestValidateTotalLines()
		{
			var expectedErrorMessage = "The Korea National Tax Service only accepts up to 99 transaction lines per invoice. Please split the charges into multiple invoices.";

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV2", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			for (var i = 0; i < 98; i++)
			{
				TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.KRW, 1, 100m);
				TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.KRW, 1, 100m);
			}
			TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.KRW, 1, 100m);

			AssertEquals("Pre-condition", 100, invoice1.Lines.Count);
			AssertEquals("Pre-condition", 99, invoice2.Lines.Count);

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			{
				AssertEquals("Pre-condition", CountryCodes.KoreaSouth, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Pre-condition", true, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);

				(invoice1.Validation as InvoiceBaseValidation).ValidateLines();
				(invoice2.Validation as InvoiceBaseValidation).ValidateLines();
				AssertHasRowError("Should have errors", invoice1, expectedErrorMessage);
				AssertNoRowErrors("Should have no errors when there is no more than 99 lines.", invoice2);
			}

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, false))
			{
				AssertEquals("Pre-condition", CountryCodes.KoreaSouth, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Pre-condition", false, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);

				(invoice1.Validation as InvoiceBaseValidation).ValidateLines();
				(invoice2.Validation as InvoiceBaseValidation).ValidateLines();
				AssertNoRowErrors("Should have no errors when E-Invoicing is not enabled.", invoice1);
				AssertNoRowErrors("Should have no errors when E-Invoicing is not enabled.", invoice2);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				AssertNotEquals("Pre-condition", CountryCodes.KoreaSouth, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				(invoice1.Validation as InvoiceValidation).ValidateLines();
				AssertNoRowErrors("Should have no errors when company is not Korea.", invoice1);
			}
		}

		public void TestValidateConsolCostsWithoutLines()
		{
			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			try
			{
				AssertEquals("has no consol costs", 0, invoice.ConsolCosting.ConsolCosts.Count);
				AssertEquals("has no lines", 0, invoice.Lines.Count);

				var consol1 = TestObjectCreator.CreateConsol(consolNum: "C0001");
				consol1.Shipments.AddNew();

				var consolCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol1);
				consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				consolCost.E6_OSCostAmount = 15M;
				AssertEquals(0, invoice.Lines.Count);
				((InvoiceBaseValidation)invoice.Validation).ValidateConsolCostsWithoutLines_ForTestOnly();
				AssertHasRowError("Consol costs without related transaction lines", invoice, "There are consol costs without related transaction lines, please apportion all consol costs again.");

				invoice.ImportAllApportionmentsFromCosting();
				AssertEquals(1, invoice.Lines.Count);
				AssertEquals(consolCost.PK, invoice.Lines[0].ImportedApportionmentID);
				((InvoiceBaseValidation)invoice.Validation).ValidateConsolCostsWithoutLines_ForTestOnly();
				AssertNoRowError(invoice, "There are consol costs without related transaction lines, please apportion all consol costs again.");
			}
			finally
			{
				invoice.ReleaseAllMutexOnInvoice();
			}
		}

		public void TestValidateComplianceSequenceNotNull()
		{
			var accountingMasterFilesRegistry = AccountingMasterFilesRegistry.Instance;

			using (accountingMasterFilesRegistry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
			{
				AssertComplianceSequenceRowError<ARInvoice>(accountingMasterFilesRegistry.ComplianceDocumentNumberAllocation_Receivables, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, true);
				AssertComplianceSequenceRowError<ARInvoice>(accountingMasterFilesRegistry.ComplianceDocumentNumberAllocation_Receivables, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, false);
			}

			using (accountingMasterFilesRegistry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
			{
				AssertComplianceSequenceRowError<APInvoice>(accountingMasterFilesRegistry.ComplianceDocumentNumberAllocation_Payables, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, true);
				AssertComplianceSequenceRowError<APInvoice>(accountingMasterFilesRegistry.ComplianceDocumentNumberAllocation_Payables, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, false);
			}

			using (accountingMasterFilesRegistry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)) //AP registry do not allow INV value
			{
				AssertComplianceSequenceRowError<ARInvoice>(accountingMasterFilesRegistry.ComplianceDocumentNumberAllocation_Receivables, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, true);
				AssertComplianceSequenceRowError<ARInvoice>(accountingMasterFilesRegistry.ComplianceDocumentNumberAllocation_Receivables, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, false);
			}

			using (accountingMasterFilesRegistry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code))
			{
				AssertComplianceSequenceRowError<ARInvoice>(accountingMasterFilesRegistry.ComplianceDocumentNumberAllocation_Receivables, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, false);
				AssertComplianceSequenceRowError<ARInvoice>(accountingMasterFilesRegistry.ComplianceDocumentNumberAllocation_Receivables, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, false);
			}

			using (accountingMasterFilesRegistry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code))
			{
				AssertComplianceSequenceRowError<APInvoice>(accountingMasterFilesRegistry.ComplianceDocumentNumberAllocation_Payables, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, false);
				AssertComplianceSequenceRowError<APInvoice>(accountingMasterFilesRegistry.ComplianceDocumentNumberAllocation_Payables, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, false);
			}

			void AssertComplianceSequenceRowError<T>(CodePairRegistryItem registry, string registryValue, bool rowErrorExpected) where T : Invoice
			{
				var invoice = Factory.NewWithValidTestData<T>();
				var errorMessage = "Please check your Compliance Invoice Book Setups. \r\n A Compliance Invoice Book for the relevant Compliance Sub-Type, Branch, Active Status and Start / Expiry Date does not exist.";

				using (registry.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, registryValue))
				{
					invoice.AH_ComplianceSubType = ZString.Empty;
					((InvoiceBaseValidation)invoice.Validation).ValidateComplianceSequenceNotNull_ForTestOnly();

					if (rowErrorExpected)
					{
						AssertHasRowError("Compliance sequence not found", invoice, errorMessage);
					}
					else
					{
						AssertNoRowError("No error on compliance sequence", invoice, errorMessage);
					}
				}
			}
		}

		public void TestValidateTransactionNumAfterInvoiceDateUpdate_Standard()
		{
			var date = new ZDateTime(2016, 01, 01);

			AssertValidateTransactionNumAfterInvoiceDateUpdate(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				date, date.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		public void TestValidateTransactionNumAfterInvoiceDateUpdate_Calendar()
		{
			var date = new ZDateTime(2016, 01, 01);
			AssertValidateTransactionNumAfterInvoiceDateUpdate(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				date, new ZDateTime(date.Year + 1, 1, 1));
		}

		void AssertValidateTransactionNumAfterInvoiceDateUpdate(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2)
		{
			if (InvoiceType == typeof(APInvoice) || InvoiceType == typeof(APCreditNote) || InvoiceType == typeof(APAdjustmentNote))
			{
				using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
				{
					InvoicingBase existingAPInvoice;
					InvoicingBase newInvoice;
					string existingAPInvoiceNum = "TESTAP1111";

					if (InvoiceType.IsSubclassOf(typeof(CreditNote)))
					{
						existingAPInvoice = Factory.NewWithValidTestData<APCreditNote>();
					}
					else
					{
						existingAPInvoice = Factory.NewWithValidTestData<APInvoice>();
					}

					existingAPInvoice.AH_TransactionNum = existingAPInvoiceNum;
					existingAPInvoice.AH_OH = TestOrg.PK;
					existingAPInvoice.AH_InvoiceDate = invoiceDate;
					existingAPInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
					existingAPInvoice.AH_TransactionCount = 2;
					Factory.Save();

					if (InvoiceType.IsSubclassOf(typeof(CreditNote)))
					{
						newInvoice = Factory.NewWithValidTestData<APCreditNote>();
					}
					else
					{
						newInvoice = Factory.NewWithValidTestData<APInvoice>();
					}

					newInvoice.AH_TransactionNum = existingAPInvoiceNum;
					newInvoice.AH_OH = TestOrg.PK;
					newInvoice.AH_InvoiceDate = invoiceDate2;

					AssertHasWarningContaining(newInvoice.AH_TransactionNumInfo, "This transaction number can be used.");
					var duplicateTransactionNumberDetails = newInvoice.GetPreviousSameNumberTransactionDetails();
					Assert(duplicateTransactionNumberDetails.HasValue);
					AssertEquals(existingAPInvoice.AH_InvoiceDate, duplicateTransactionNumberDetails.Value.PreviousInvoiceDate);
					AssertEquals(existingAPInvoice.AH_TransactionCount, duplicateTransactionNumberDetails.Value.PreviousTransactionCount);

					newInvoice.AH_InvoiceDate = existingAPInvoice.AH_InvoiceDate;
					AssertHasErrorContaining(newInvoice.AH_TransactionNumInfo, "The transaction number is already in use");
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckAH_InvoiceDateErrors()
		{
			if (InvoiceType == typeof(APInvoice) || InvoiceType == typeof(APCreditNote) || InvoiceType == typeof(APAdjustmentNote) || InvoiceType == typeof(UAInvoice) || InvoiceType == typeof(UACreditNote))
			{
				var errorMsg = @"Invoice Date must be earlier or same as the Post Date. This is controlled by the registry Accounting > Payable Defaults > Default Settings > Prevent Posting Invoice Date Greater Than Post Date.";
				var invoice = (InvoicingBase)Factory.New(InvoiceType);
				var today = ZDateTime.Now;
				var tomorrow = today.AddDays(1);
				invoice.AH_PostDate = today;

				using (AccountingMasterFilesRegistry.Instance.PreventInvoiceDateGreaterThanPostDate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
				{
					invoice.AH_InvoiceDate = tomorrow;
					AssertNoError(invoice.AH_InvoiceDateInfo, errorMsg);
				}

				using (AccountingMasterFilesRegistry.Instance.PreventInvoiceDateGreaterThanPostDate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
				{
					invoice.AH_InvoiceDate = tomorrow;
					AssertHasError(invoice.AH_InvoiceDateInfo, errorMsg);

					invoice.AH_InvoiceDate = today;
					AssertNoError(invoice.AH_InvoiceDateInfo, errorMsg);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckAH_OH_TransactionCreationValidationWhenPECOrgCusCodeIsNotEnteredForTurkeyCountry()
		{
			var expectedError1 = @"A Post Box Alias is required for this debtor to allow the receivables transaction to be successfully posted.
Before attempting to post the charges, please update the organization record for the debtor to include a valid Post Box Alias email address using the registration number type PEC.

This can be updated at Maintain > Master Data > Organization. (Details > Config > Registration Numbers/Codes sub-tab)";
			var expectedError2 = @"An email address is required for this Debtor to allow the receivables transaction to be successfully posted.
Before attempting to post the charges, please update the Organization record for the Debtor to include a valid email address (Maintain > Master Data > Organization).

This can be updated at Maintain > Master Data > Organization. (Details > Config > Registration Numbers/Codes sub-tab)";

			var org = TestObjectCreator.CreateOrgHeader("TST", true, true);
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var validation = new InvoiceValidation(invoice);
			invoice.AH_OH = org.PK;
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_InvoiceTerm = TransactionTypes.Invoice;
			validation.ValidateAH_OH();

			AssertNoError("Should not have proper validation error", invoice.AH_OHInfo, expectedError1);
			AssertNoError("Should not have proper validation error", invoice.AH_OHInfo, expectedError2);

			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			validation.ValidateAH_OH();

			AssertNoError("Should not have proper validation error", invoice.AH_OHInfo, expectedError1);
			AssertNoError("Should not have proper validation error", invoice.AH_OHInfo, expectedError2);

			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(invoice.Branch.PK.ToGuid(), DateTime.Today.AddDays(-30)))
			{
				validation.ValidateAH_OH();

				AssertNoError("Should not have proper validation error", invoice.AH_OHInfo, expectedError1);
				AssertNoError("Should not have proper validation error", invoice.AH_OHInfo, expectedError2);

				invoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
				validation.ValidateAH_OH();

				AssertHasWarning("Should have proper validation warning", invoice.AH_OHInfo, expectedError1);
				AssertNoError("Should not have proper validation error", invoice.AH_OHInfo, expectedError2);

				invoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR;
				validation.ValidateAH_OH();

				AssertNoWarning("Should not have proper validation warning", invoice.AH_OHInfo, expectedError1);
				AssertHasError("Should have proper validation error", invoice.AH_OHInfo, expectedError2);

				var pecCusCode = org.CustomsCodes.AddNew("PEC", "debtor@testmailaddress.com");

				validation.ValidateAH_OH();

				AssertNoError("Should not have proper validation error", invoice.AH_OHInfo, expectedError1);
				AssertNoError("Should not have proper validation error", invoice.AH_OHInfo, expectedError2);
			}
		}

		protected virtual BooleanRegistryItem OriginalInvoiceDetailsMandatoryRegistryItem { get; }
		protected virtual Type OriginalInvoiceType { get; }

		[TestDate(2020, 2, 2)]
		public void TestOriginalInvoiceDetailsMandatory()
		{
			if (OriginalInvoiceDetailsMandatoryRegistryItem == null || OriginalInvoiceType == null)
			{
				Assert("test doesn't apply to all invocie type", true);
			}
			else
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_IsDebtor = true;
				org.OH_IsCreditor = true;
				Factory.Save();

				var transaction = Factory.New(InvoiceType) as InvoicingBase;
				bool originalDetailsVisible = InvoiceType.In(typeof(ARCreditNote), typeof(APCreditNote), typeof(ARInvoice));

				void assertHasError(ZPropertyInfo info, string error)
				{
					if (originalDetailsVisible)
					{
						AssertHasError(info, error);
					}
					else
					{
						AssertNoErrors(info);
					}
				}

				var item = OriginalInvoiceDetailsMandatoryRegistryItem;
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				string registryLocation = item.Location();

				using (InitaliseComplianceFactory(shouldShowOriginalInvoiceReferenceFields: originalDetailsVisible, areOriginalTransactionReferenceFieldsMandatory: false))
				{
					transaction.Validation.ValidateAll();
					assertHasError(transaction.OriginalTransactionReferenceInfo, $"Original Invoice Number and Date must be recorded. This is controlled by the registry {registryLocation}.");
					assertHasError(transaction.AH_OriginalInvoiceDateInfo, $"Original Invoice Number and Date must be recorded. This is controlled by the registry {registryLocation}.");
					assertHasError(transaction.AH_OriginalTransactionNumInfo, $"Original Invoice Number and Date must be recorded. This is controlled by the registry {registryLocation}.");

					var originalInvoice = Factory.NewWithValidTestData(OriginalInvoiceType) as InvoicingBase;
					originalInvoice.AH_InvoiceDate = ZDateTime.Now.AddDays(-4);
					originalInvoice.AH_TransactionNum = "A000443";
					originalInvoice.AH_OH = org.PK;
					transaction.AH_OH = org.PK;

					transaction.OriginalTransactionReference = originalInvoice.PK;
					AssertNoErrors(transaction.OriginalTransactionReferenceInfo);
					AssertNoErrors(transaction.AH_OriginalTransactionNumInfo);
					AssertNoErrors(transaction.AH_OriginalInvoiceDateInfo);

					transaction.OriginalTransactionReference = ZGuid.Empty;
					transaction.AH_OriginalInvoiceDate = ZDate.Empty;
					transaction.AH_OriginalTransactionNum = "777";
					assertHasError(transaction.OriginalTransactionReferenceInfo, $"Original Invoice Number and Date must be recorded. This is controlled by the registry {registryLocation}.");
					assertHasError(transaction.AH_OriginalInvoiceDateInfo, $"Original Invoice Number and Date must be recorded. This is controlled by the registry {registryLocation}.");
					AssertNoErrors(transaction.AH_OriginalTransactionNumInfo);

					transaction.AH_OriginalInvoiceDate = ZDate.Today.AddDays(-3);
					transaction.Validation.ValidateAll();
					AssertNoErrors(transaction.OriginalTransactionReferenceInfo);
					AssertNoErrors(transaction.AH_OriginalInvoiceDateInfo);
					AssertNoErrors(transaction.AH_OriginalTransactionNumInfo);

					transaction.AH_OriginalTransactionNum = string.Empty;
					transaction.Validation.ValidateAll();
					assertHasError(transaction.OriginalTransactionReferenceInfo, $"Original Invoice Number and Date must be recorded. This is controlled by the registry {registryLocation}.");
					AssertNoErrors(transaction.AH_OriginalInvoiceDateInfo);
					assertHasError(transaction.AH_OriginalTransactionNumInfo, $"Original Invoice Number and Date must be recorded. This is controlled by the registry {registryLocation}.");

					item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					transaction.Validation.ValidateAll();
					AssertNoErrors(transaction.OriginalTransactionReferenceInfo);
					AssertNoErrors(transaction.AH_OriginalInvoiceDateInfo);
					AssertNoErrors(transaction.AH_OriginalTransactionNumInfo);
				}
			}
		}

		public static IDisposable InitaliseComplianceFactory(bool shouldShowOriginalInvoiceReferenceFields = true,
			bool areAllOriginalInvoiceReferenceFieldsEnabled = true,
			bool shouldShowOriginalInvoiceReferenceReasonFields = true,
			bool areOriginalTransactionReferenceFieldsMandatory = false)
		{
			var originalInvoiceReferenceMock = new Mock<IOriginalInvoiceReference>();
			originalInvoiceReferenceMock.Setup(x => x.ShouldShowOriginalInvoiceReferenceFields(It.IsAny<string>(), It.IsAny<string>())).Returns(shouldShowOriginalInvoiceReferenceFields);
			originalInvoiceReferenceMock.Setup(x => x.GetAreAllOriginalInvoiceReferenceFieldsEnabled(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(areAllOriginalInvoiceReferenceFieldsEnabled);
			originalInvoiceReferenceMock.Setup(x => x.ShouldShowOriginalInvoiceReferenceReasonFields(It.IsAny<string>(), It.IsAny<string>())).Returns(shouldShowOriginalInvoiceReferenceReasonFields);
			originalInvoiceReferenceMock.Setup(x => x.GetAreOriginalTransactionReferenceFieldsMandatory(It.IsAny<string>(), It.IsAny<string>())).Returns(areOriginalTransactionReferenceFieldsMandatory);
			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
			countryComplianceFactoryMock.Setup(c => c.GetIOriginalInvoiceReference(It.IsAny<ZString>())).Returns(originalInvoiceReferenceMock.Object);
			return ObjectFactory.Substitute(countryComplianceFactoryMock.Object);
		}

		public virtual void TestValidateAH_Calc_AmendStatusCode()
		{
			var mockIAmendStatusCodeProvider = new Mock<IAmendStatusCodeProvider>();
			var mockIAmendStatusCodeValidationProvider = new Mock<AmendStatusCodeValidationProvider>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			var amendStatusCodeList = new CodeDescriptionPairList();
			amendStatusCodeList.AddPair("01", "Test List Value");

			mockIAmendStatusCodeProvider.Setup(x => x.AmendStatusCodeList).Returns(amendStatusCodeList);
			mockIAmendStatusCodeProvider.Setup(x => x.ShouldShowAmendStatusCode()).Returns(true);
			mockIAmendStatusCodeProvider.Setup(x => x.AmendStatusCodeReferenceType).Returns("KRE");
			mockIAmendStatusCodeValidationProvider.CallBase = true;
			mockIAccountingCountryFactory.As<IInstanceProvider<IAmendStatusCodeProvider>>().Setup(x => x.Get()).Returns(mockIAmendStatusCodeProvider.Object);
			mockIAccountingCountryFactory.As<IInstanceProvider<IAmendStatusCodeValidationProvider>>().Setup(x => x.Get()).Returns(mockIAmendStatusCodeValidationProvider.Object);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			var invoicingBase = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;

			AssertValidateAH_Calc_AmendStatusCode(invoicingBase, false, false);

			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			AssertValidateAH_Calc_AmendStatusCode(invoicingBase, true, false);
		}

		protected void AssertValidateAH_Calc_AmendStatusCode(InvoicingBase invoice, bool isCurrentContextSupportAmendStatusCode, bool isAllowModifyAmendStatusCode)
		{
			AssertEquals("PreCondition", isAllowModifyAmendStatusCode, invoice.IsAllowModifyAmendStatusCode);
			invoice.AH_Calc_AmendStatusCode = ZString.Empty;

			if (invoice.Validation is InvoiceBaseValidation validation)
			{
				validation.ValidateAH_Calc_AmendStatusCode();
			}
			else if (invoice.Validation is InvoicingBaseReversalValidation reversalValidation)
			{
				reversalValidation.ValidateAH_Calc_AmendStatusCode();
			}
			else
			{
				Assert("Validation type is incorrect", true);
				return;
			}

			if (isAllowModifyAmendStatusCode)
			{
				AssertHasError(invoice.AH_Calc_AmendStatusCodeInfo, "An amendment status code is required for the amending transaction. Please select an amendment status code.");
			}
			else
			{
				AssertNoErrors("Do not check empty value in default.", invoice.AH_Calc_AmendStatusCodeInfo);
			}

			var settingValue = "AA";
			AssertEquals("PreCondition", false, invoice.Lookups.AmendStatusCodeList.ContainsCode(settingValue));
			invoice.AH_Calc_AmendStatusCode = settingValue;

			if (isCurrentContextSupportAmendStatusCode)
			{
				AssertHasError(invoice.AH_Calc_AmendStatusCodeInfo, "Enter a valid Amend Status Code.");
			}
			else
			{
				AssertNoErrors("There should be no validation when Amend Status Code is not supported", invoice.AH_Calc_AmendStatusCodeInfo);
			}
		}

		#region Implementation

		string OutstandingAmountAndFullyPaidDateError
		{
			get { return "invalid Fully Paid Date with respect to the outstanding amount"; }
		}

		void SetInvoices()
		{
			APInvoice aPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			APInvoice aPInvoice2 = Factory.NewWithValidTestData<APInvoice>();
			APInvoice aPInvoice3 = Factory.NewWithValidTestData<APInvoice>();
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData<ARInvoice>();

			aPInvoice1.AH_OH = Org1.PK;
			aPInvoice2.AH_OH = Org2.PK;
			aPInvoice3.AH_OH = Org1.PK;
			aRInvoice1.AH_OH = Org1.PK;

			aPInvoice1.AH_OutstandingAmount = -120m;
			aPInvoice1.AH_GSTAmount = -120m;
			aPInvoice2.AH_OutstandingAmount = -70m;
			aPInvoice2.AH_GSTAmount = -70m;
			aPInvoice3.AH_OutstandingAmount = -40m;
			aPInvoice3.AH_GSTAmount = -40m;
			aRInvoice1.AH_OutstandingAmount = 210m;
			aRInvoice1.AH_GSTAmount = 210m;

			Factory.Save();
		}

		protected OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = TestObjectCreator.CreateOrgHeader("TSTREGORG", true, true, true, false, true, false);
				}
				return fTestOrg;
			}
		}

		protected AccChargeCode ChargeCode
		{
			get
			{
				if (fChargeCode == null)
				{
					fChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));
				}
				return fChargeCode;
			}
		}

		OrgHeader fTestOrg;
		AccChargeCode fChargeCode;

		OrgHeader fNonWHTRegisteredOrg;
		protected OrgHeader NonWHTRegisteredOrg
		{
			get
			{
				if (fNonWHTRegisteredOrg == null)
				{
					fNonWHTRegisteredOrg = TestObjectCreator.CreateOrgHeader("TSTNONWHT", true, true, true, false, true, false);
				}
				return fNonWHTRegisteredOrg;
			}
		}

		OrgHeader fWHTRegisteredOrg;
		protected OrgHeader WHTRegisteredOrg
		{
			get
			{
				if (fWHTRegisteredOrg == null)
				{
					fWHTRegisteredOrg = TestObjectCreator.CreateOrgHeader("TSTWHTREG", true, true, false, true, false, true);
				}
				return fWHTRegisteredOrg;
			}
		}

		AccTaxRate fGST10TaxRate;
		OrgHeader fGSTRegisteredOrg;
		OrgHeader fNonGSTRegisteredOrg;

		protected AccTaxRate GST10TaxRate
		{
			get
			{
				if (fGST10TaxRate == null)
				{
					fGST10TaxRate = TestObjectCreator.CreateTaxRate("TSTGST", "Test GST Code", 10);
				}
				return fGST10TaxRate;
			}
		}

		protected OrgHeader GSTRegisteredOrg
		{
			get
			{
				if (fGSTRegisteredOrg == null)
				{
					fGSTRegisteredOrg = TestObjectCreator.CreateOrgHeader("TSTREGORG", true, true, true, false, true, false);
				}
				return fGSTRegisteredOrg;
			}
		}

		protected OrgHeader NonGSTRegisteredOrg
		{
			get
			{
				if (fNonGSTRegisteredOrg == null)
				{
					fNonGSTRegisteredOrg = TestObjectCreator.CreateOrgHeader("TSTNONREG", true, true, false, false, false, false);
				}
				return fNonGSTRegisteredOrg;
			}
		}

		#endregion
	}
}
