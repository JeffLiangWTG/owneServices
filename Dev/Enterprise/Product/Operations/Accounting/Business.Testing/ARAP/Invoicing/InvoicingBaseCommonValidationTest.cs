using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class InvoicingBaseCommonValidationTest<T> : TransactionHeaderWithLinesValidationTest where T : InvoicingBaseCommonValidation
	{
		public void TestCheckAH_OH_ImportedXMLValues()
		{
			if (InvoiceType != typeof(APInvoice) && InvoiceType != typeof(APCreditNote))
			{
				Assert(true);
				return;
			}
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.AddressShortCode = "Str";
			orgAddress.Address1 = "Street";
			var expectedOrganizationCode = "ForeignOrg";
			orgAddress.OrganizationCode = expectedOrganizationCode;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.Ledger = LedgerTypes.AccountsPayable;
			universalTransaction.OrganizationAddress = orgAddress;
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.PostingJournalCollection.Add(universalLine1);

			var invoice = TestObjectCreator.CreateAndAllocateInvoiceWithUniversalTransactionInAllocationApprovalRequest(InvoiceType, universalTransaction, false);
			AssertEquals("Precondition: ImportedCreditorXmlCode", expectedOrganizationCode, invoice.ImportedCreditorXmlCode);
			AssertEquals("Precondition: ImportedCreditor", ZString.Empty, invoice.ImportedCreditor);

			var validation = GetValidation(invoice);
			validation.ValidateAH_OH();
			string expectedMessage = "Organization is not found for imported XML code 'ForeignOrg'. New matching rule will be created for foreign code 'ForeignOrg'.";
			AssertHasWarning(invoice.AH_OHInfo, expectedMessage);
			AssertHasWarning("Address should have the same message to show it on invoice control", invoice.AH_OA_InvoiceAddressOverrideInfo, expectedMessage);

			invoice.AH_OH = ZGuid.Empty;
			AssertNoWarnings(invoice.AH_OHInfo);
			AssertNoWarnings(invoice.AH_OA_InvoiceAddressOverrideInfo);

			var currentCompnay = GlbCompany.GetCurrentCompany(new BusinessObjectFactory());
			OrgPatternMatchOverride patternMatchOverride = TestObjectCreator.AddMatchingRuleForOrganization(currentCompnay.OrgProxy, orgAddress.OrganizationCode.Value, TestObjectCreator.Creditor1);
			patternMatchOverride.Factory.Save();

			invoice.AllocationApprovalRequest.PostingDetails.SourceXML = " " + invoice.AllocationApprovalRequest.PostingDetails.SourceXML; //to run UpdateUniversalTransaction to remap xml codes 

			AssertEquals("Precondition: ImportedCreditorXmlCode", expectedOrganizationCode, invoice.ImportedCreditorXmlCode);
			AssertEquals("Precondition: ImportedCreditor", TestObjectCreator.Creditor1.OH_Code, invoice.ImportedCreditor);
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			AssertNoWarnings(invoice.AH_OHInfo);
			AssertNoWarnings(invoice.AH_OA_InvoiceAddressOverrideInfo);
			invoice.AH_OH = TestObjectCreator.Creditor2.PK;
			expectedMessage = "Creditor value is different to a value matched by default. Organization matching rule for foreign code 'ForeignOrg' will be updated.";
			AssertHasWarning(invoice.AH_OHInfo, expectedMessage);
			AssertHasWarning("Address should have the same message to show it on invoice control", invoice.AH_OA_InvoiceAddressOverrideInfo, expectedMessage);

			universalTransaction.OrganizationAddress.OrganizationCode = null;
			invoice.AllocationApprovalRequest.PostingDetails.SourceXML = universalTransaction.Serialize();
			AssertEquals("Precondition: ImportedCreditorXmlCode", ZString.Empty, invoice.ImportedCreditorXmlCode);
			AssertEquals("Precondition: ImportedCreditor", ZString.Empty, invoice.ImportedCreditor);
			validation.ValidateAH_OH();
			AssertNoWarnings(invoice.AH_OHInfo);
			AssertNoWarnings(invoice.AH_OA_InvoiceAddressOverrideInfo);

			universalTransaction.OrganizationAddress.OrganizationCode = expectedOrganizationCode;
			invoice.AllocationApprovalRequest.PostingDetails.SourceXML = universalTransaction.Serialize();
			AssertEquals("Precondition: ImportedCreditorXmlCode", expectedOrganizationCode, invoice.ImportedCreditorXmlCode);
			AssertEquals("Precondition: ImportedCreditor", TestObjectCreator.Creditor1.OH_Code, invoice.ImportedCreditor);
			invoice.RunPreSaveValidation();
			AssertHasWarning("Postcondition:", invoice.AH_OHInfo, expectedMessage);
			AssertHasWarning("RunPreSaveValidation: Address should have the same message to show it on invoice control", invoice.AH_OA_InvoiceAddressOverrideInfo, expectedMessage);
		}

		public void TestCheckAH_OH_ImportedXMLValues_CrossLedger()
		{
			if (InvoiceType != typeof(APInvoice) && InvoiceType != typeof(APCreditNote))
			{
				Assert(true);
				return;
			}

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.None),
				AddressShortCode = "Str",
				Address1 = "Street"
			};

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				Ledger = LedgerTypes.AccountsPayable,
				OrganizationAddress = orgAddress,
				BranchAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.None), OrganizationCode = TestObjectCreator.NonCurrentCompany.OrgProxy.OH_Code, Country = UniversalDataBuss.DataObjects.Universal.Country.New(TestObjectCreator.NonCurrentCompany.Country) }
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.DataContext.SetCompanyAndDataProviderDetails(TestObjectCreator.NonCurrentCompany);

			var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.PostingJournalCollection.Add(universalLine1);

			var invoice = TestObjectCreator.CreateAndAllocateInvoiceWithUniversalTransactionInAllocationApprovalRequest(InvoiceType, universalTransaction, true);
			AssertEquals("Precondition: ImportedCreditorXmlCode", universalTransaction.DataContext.DataProviderForCodeMapping, invoice.ImportedCreditorXmlCode);
			AssertEquals("Precondition: ImportedCreditor", ZString.Empty, invoice.ImportedCreditor);

			var validation = GetValidation(invoice);
			validation.ValidateAH_OH();
			AssertHasWarning(invoice.AH_OHInfo, "Organization is not found for imported XML code 'EDIDATDEM'. New matching rule will be created for foreign code 'EDIDATDEM'.");

			invoice.AH_OH = ZGuid.Empty;
			AssertNoWarnings(invoice.AH_OHInfo);

			var currentCompnay = GlbCompany.GetCurrentCompany(new BusinessObjectFactory());
			OrgPatternMatchOverride patternMatchOverride = TestObjectCreator.AddMatchingRuleForOrganization(currentCompnay.OrgProxy, universalTransaction.DataContext.DataProviderForCodeMapping, TestObjectCreator.Creditor1);
			patternMatchOverride.Factory.Save();

			invoice.AllocationApprovalRequest.PostingDetails.SourceXML = " " + invoice.AllocationApprovalRequest.PostingDetails.SourceXML; //to run UpdateUniversalTransaction to remap xml codes 

			AssertEquals("Precondition: ImportedCreditorXmlCode", universalTransaction.DataContext.DataProviderForCodeMapping, invoice.ImportedCreditorXmlCode);
			AssertEquals("Precondition: ImportedCreditor", TestObjectCreator.Creditor1.OH_Code, invoice.ImportedCreditor);
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			AssertNoWarnings(invoice.AH_OHInfo);
			invoice.AH_OH = TestObjectCreator.Creditor2.PK;
			AssertHasWarning(invoice.AH_OHInfo, "Creditor value is different to a value matched by default. Organization matching rule for foreign code 'EDIDATDEM' will be updated.");

			universalTransaction.DataContext = null;
			invoice.AllocationApprovalRequest.PostingDetails.SourceXML = universalTransaction.Serialize();
			AssertEquals("Precondition: ImportedCreditorXmlCode", ZString.Empty, invoice.ImportedCreditorXmlCode);
			AssertEquals("Precondition: ImportedCreditor", ZString.Empty, invoice.ImportedCreditor);
			validation.ValidateAH_OH();
			AssertNoWarnings(invoice.AH_OHInfo);
		}

		[TestDate(2022, 11, 10)]
		public void TestCheckExporterExemption_NotValid()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupDataForExporterExemption(exvCompanyPk: GlbCompany.CurrentCompany.PK.ToString(), expired: true);

				var invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1M, 1800M, 70M, 0M);
				line.AL_AT = dichIntTaxRate.PK;
				invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				invoice.AH_OH = TestObjectCreator.Debtor.PK;

				var expectedErrorMessage = "If [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] is set to Yes, the DICH.INT Tax ID can only be used for a Debtor with a valid Exporter Exemption Certificate, where the certificate Ceiling Limit has not been exceeded.";
				AssertEquals("No valid Exemption Document found for the Organization", true, invoice.AH_OHInfo.Notifications.GetErrors().Contains(expectedErrorMessage));
			}
		}

		[TestDate(2022, 11, 10)]
		public void TestCheckExporterExemption()
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			var expiringDateDoc1 = ZDate.Today.AddDays(30).ToShortDateString();
			var expiringDateDoc2 = ZDate.Today.AddDays(29).ToShortDateString();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				SetupDataForExporterExemption(exvCompanyPk: GlbCompany.CurrentCompany.PK.ToString());

				var debtor_expiringWarningDoc1 = createExpiringWarningMessage("000001", expiringDateDoc1);
				var debtor_expiringWarningDoc2 = createExpiringWarningMessage("000002", expiringDateDoc2);
				var debtor_expectedWarningMessage = ZString.Format("{0}\r\n{1}", debtor_expiringWarningDoc1, debtor_expiringWarningDoc2);

				var creditor_expiringWarningDoc1 = createExpiringWarningMessage("000004", expiringDateDoc1);
				var creditor_expiringWarningDoc2 = createExpiringWarningMessage("000005", expiringDateDoc2);
				var creditor_expectedWarningMessage = ZString.Format("{0}\r\n{1}", creditor_expiringWarningDoc1, creditor_expiringWarningDoc2);

				using (registry.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					if (Header.AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						var exceedingWarningMessage = @"The Exporter Exemption Ceiling Limit Threshold of 25% has exceeded.
The Total Certificate Ceiling Limit is EUR 2000.50.
Total Posted Transactions Amount using the exporter exemption certificates is EUR 700.00.";

						var invoice = (InvoicingBase)Factory.New(InvoiceType);
						invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
						invoice.AH_OH = TestObjectCreator.Debtor.PK;
						AssertHasWarning("Expected certificate expired warning", invoice.AH_OHInfo, debtor_expectedWarningMessage);

						AccountingConfigurationRegistry.Instance.ExporterExemptionCellingLimitThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 50);
						invoice.AH_OH = TestObjectCreator.Debtor.PK;
						AssertEquals("Not expected exemption ceiling limit threshold warning on 50% Threshold", false, invoice.AH_OHInfo.Notifications.GetWarnings().Contains("The Exporter Exemption Ceiling Limit Threshold of 50% has exceeded."));

						AccountingConfigurationRegistry.Instance.ExporterExemptionCellingLimitThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 25);
						invoice.AH_OH = TestObjectCreator.Debtor.PK;
						AssertHasWarning("Expected exemption ceiling limit threshold warning on 25% Threshold", invoice.AH_OHInfo, ZString.Format("{0}\r\n{1}", debtor_expectedWarningMessage, exceedingWarningMessage));
					}
					else
					{
						var notARTransaction = (InvoicingBase)Factory.New(InvoiceType);
						notARTransaction.AH_Ledger = Header.AH_Ledger;
						notARTransaction.AH_OH = TestObjectCreator.Creditor1.PK;
						AssertEquals("Not expected certificate expired warning for Payables", false, notARTransaction.AH_OHInfo.Notifications.GetWarnings().Contains(debtor_expectedWarningMessage));
						AssertEquals("Not expected any ceiling limit threshold warning for Payables", false, notARTransaction.AH_OHInfo.Notifications.GetWarnings().Contains("The Exporter Exemption Ceiling Limit Threshold of"));
					}
				}

				using (registry.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AccountingConfigurationRegistry.Instance.ExporterExemptionCellingLimitThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 0);

					if (Header.AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						var expectedErrorMessage = createStillAvailableAmountErrorMessage("ZZGST1, ZZGST2", "EUR", "1300.50");

						var invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
						var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1M, 4M, 70M, 0M);
						line.AL_AT = TestObjectCreator.GST1.PK;
						var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1M, 4M, 70M, 0M);
						line2.AL_AT = TestObjectCreator.GST2.PK;
						var line3 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1M, 4M, 70M, 0M);
						line3.AL_AT = TestObjectCreator.GST2.PK;
						invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
						invoice.AH_OH = TestObjectCreator.Debtor.PK;
						AssertEquals("Expected certificate expired warning", true, invoice.AH_OHInfo.Notifications.GetWarnings().Contains(debtor_expectedWarningMessage));
						AssertEquals("Expected still available amount error", true, invoice.AH_OHInfo.Notifications.GetErrors().Contains(expectedErrorMessage));

						invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
						line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1M, 1800M, 70M, 0M);
						line.AL_AT = dichIntTaxRate.PK;
						invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
						invoice.AH_OH = TestObjectCreator.Debtor.PK;
						AssertEquals("Expected certificate expired warning", true, invoice.AH_OHInfo.Notifications.GetWarnings().Contains(debtor_expectedWarningMessage));

						expectedErrorMessage = createExceedingErrorMessage("EUR", "499.50");

						AccountingConfigurationRegistry.Instance.ExporterExemptionCellingLimitThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 50);
						invoice.AH_OH = TestObjectCreator.Debtor.PK;
						AssertEquals("Expected certificate expired warning", true, invoice.AH_OHInfo.Notifications.GetWarnings().Contains(debtor_expectedWarningMessage));
						AssertEquals("Expected exemption ceiling limit error (Threshold of 50% overriden to 100% because of Registry YES)", true, invoice.AH_OHInfo.Notifications.GetErrors().Contains(expectedErrorMessage));
					}
					else
					{
						var notARTransaction = (InvoicingBase)Factory.New(InvoiceType);
						var line = TestObjectCreator.CreateInvoiceLine(notARTransaction, TestObjectCreator.EUR, 1M, 1800M, 70M, 0M);
						line.AL_AT = dichIntTaxRate.PK;
						notARTransaction.AH_Ledger = Header.AH_Ledger;
						notARTransaction.AH_OH = TestObjectCreator.Creditor1.PK;
						AssertEquals("Expected certificate expired warning for Payables", true, notARTransaction.AH_OHInfo.Notifications.GetWarnings().Contains(creditor_expectedWarningMessage));
						AssertEquals("Not expected any ceiling limit threshold error for Payables", false, notARTransaction.AH_OHInfo.Notifications.GetErrors().Contains("The Exporter Exemption Ceiling Limit Threshold of"));
					}
				}
			}

			ZString createExpiringWarningMessage(string docNumber, string expiringDate)
			{
				return ZString.Format("The Exporter Exemption Certificate Number {0} is expiring on {1}.", docNumber, expiringDate);
			}

			ZString createExceedingErrorMessage(string currencyCode, string exceedingAmount)
			{
				return ZString.Format(@"There are errors that need to be corrected before saving the current Accounts Receivable Invoice.
TAX ID: DICH.INT, based on the Registry [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] it is not possible to proceed with the post because you are posting a transaction on a debtor that has one or more EXV-VAT/GST Exporter Exemption document with CEILING LIMIT.
The sum of the transactions that contain DICH.INT Tax ID including this one you are posting, exceeds the CEILING LIMIT by {0} {1}.
To proceed with the post please fix Tax ID Code or save a new EXV-VAT/GST Exporter Exemption in the Debtor Organization eDocs, with an higher CEILING LIMIT or disable the Registry.", currencyCode, exceedingAmount);
			}

			ZString createStillAvailableAmountErrorMessage(string vatList, string currencyCode, string stillAvailableAmount)
			{
				return ZString.Format(@"ZDebtor: There are errors that need to be corrected before this Accounts Receivable Invoice can be saved.
TAX ID: {0} based on the registry [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] it is not possible to proceed with the post because you are posting a transaction on a debtor that has one or more EXV-VAT/GST Exporter Exemption documents that still have available {1} {2} CEILING LIMIT.
To proceed with the post fix Tax ID Code or disable the above registry item.", vatList, currencyCode, stillAvailableAmount);
			}
		}

		[TestDate(2022, 11, 10)]
		public void TestCheckExporterExemption_TotalAmountWithCreditNote()
		{
			var currCompany = GlbCompany.CurrentCompany;
			using (currCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupDataForExporterExemption(exvCompanyPk: GlbCompany.CurrentCompany.PK.ToString());

				var dichIntCreditNote = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "CRD002", TestObjectCreator.EUR, 1M, TestObjectCreator.Debtor);
				var lineCrd = TestObjectCreator.CreateInvoiceLine(dichIntCreditNote, TestObjectCreator.EUR, 1M, 700M, 70M, 0M);
				lineCrd.AL_AT = dichIntTaxRate.PK;

				Factory.Save();

				var expectedErrorMessage = @"There are errors that need to be corrected before saving the current Accounts Receivable Invoice.
TAX ID: DICH.INT, based on the Registry [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] it is not possible to proceed with the post because you are posting a transaction on a debtor that has one or more EXV-VAT/GST Exporter Exemption document with CEILING LIMIT.
The sum of the transactions that contain DICH.INT Tax ID including this one you are posting, exceeds the CEILING LIMIT by EUR 199.50.
To proceed with the post please fix Tax ID Code or save a new EXV-VAT/GST Exporter Exemption in the Debtor Organization eDocs, with an higher CEILING LIMIT or disable the Registry.";

				var invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1M, 2200M, 70M, 0M);
				line.AL_AT = dichIntTaxRate.PK;
				invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				invoice.AH_OH = TestObjectCreator.Debtor.PK;
				AssertEquals("Total Invoices = 700 EUR. Credit notes = 700 EUR. Total Used Amount is 0 so Expect exemption ceiling limit error of 199.50 EUR.", true, invoice.AH_OHInfo.Notifications.GetErrors().Contains(expectedErrorMessage));
			}
		}

		[TestDate(2022, 11, 10)]
		public void TestCheckExporterExemption_NoErrorsForCreditNoteWithIVA_WithCeilingLimit()
		{
			var currCompany = GlbCompany.CurrentCompany;
			using (currCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupDataForExporterExemption(exvCompanyPk: GlbCompany.CurrentCompany.PK.ToString());

				var creditNote = (InvoicingBase)Factory.New(typeof(ARCreditNote));
				var line = TestObjectCreator.CreateInvoiceLine(creditNote, TestObjectCreator.EUR, 1M, 2200M, 70M, 0M);
				line.AL_AT = TestObjectCreator.GST1.PK;
				creditNote.AH_Ledger = LedgerTypes.AccountsReceivable;
				creditNote.AH_OH = TestObjectCreator.Debtor.PK;
				AssertEquals("No errors posting an AR Credit Note with IVA", creditNote.AH_OHInfo.Notifications.GetErrors().Count(), 0);
			}
		}

		[TestDate(2022, 11, 10)]
		public void TestCheckExporterExemption_DifferentLoginCompany()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "XC1";
			otherCompany.SetCountry(CountryCodes.Italy);
			var otherCompanyBranch = Factory.New<GlbBranch>();
			otherCompanyBranch.GB_GC = otherCompany.PK;
			otherCompanyBranch.GB_Code = "XB1";
			otherCompanyBranch.GB_RN_NKCountryCode = CountryCodes.Italy;

			Factory.Save();

			using (new TemporaryUserContext { BranchPK = otherCompanyBranch.PK.ToGuid() }.Set())
			using (AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupDataForExporterExemption(exvCompanyPk: currentCompany.PK.ToString());

				var invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1M, 100, 10M, 0M);
				line.AL_AT = dichIntTaxRate.PK;
				invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				invoice.AH_OH = TestObjectCreator.Debtor.PK;
				var expectedErrorMessage = "If [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] is set to Yes, the DICH.INT Tax ID can only be used for a Debtor with a valid Exporter Exemption Certificate, where the certificate Ceiling Limit has not been exceeded.";
				AssertEquals("No valid Exemption Document found for the Organization within the XC1 Company", true, invoice.AH_OHInfo.Notifications.GetErrors().Contains(expectedErrorMessage));

				invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
				line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1M, 100, 10M, 0M);
				line.AL_AT = TestObjectCreator.GST1.PK;
				invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				invoice.AH_OH = TestObjectCreator.Debtor.PK;
				AssertEquals("Transaction can be posted with a not zero TaxID rated (IVA).", false, invoice.AH_OHInfo.Notifications.GetErrors().Contains(expectedErrorMessage));
			}
		}

		void SetupDataForExporterExemption(string exvCompanyPk, bool expired = false)
		{
			var stampDutyRecharge = new StampDutyRecharge
			{
				StampDutyRechargeOrganizationType = StampDutyRechargeOrganizationType.NotRecharging,
				StampDutyRechargeTransactionType = StampDutyRechargeTransactionType.All
			};
			AccountingConfigurationRegistry.Instance.StampDutyRecharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, stampDutyRecharge);

			var query = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, CountryCodes.Italy);
			query.AddToFilter(AccTaxRateSchema.AT_Code, "DICH.INT");
			dichIntTaxRate = Factory.LoadTop1<AccTaxRate>(query);

			var dichInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV002", TestObjectCreator.EUR, 1M, TestObjectCreator.Debtor);
			var line = TestObjectCreator.CreateInvoiceLine(dichInvoice, TestObjectCreator.EUR, 1M, 700M, 70M, 0M);
			line.AL_AT = dichIntTaxRate.PK;

			var dichInvoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV003", TestObjectCreator.EUR, 1M, TestObjectCreator.Creditor1);
			var line1 = TestObjectCreator.CreateInvoiceLine(dichInvoice1, TestObjectCreator.EUR, 1M, 700M, 70M, 0M);
			line1.AL_AT = dichIntTaxRate.PK;

			var dateReceived1 = expired ? ZDate.Today.AddDays(-30) : ZDate.Today.AddDays(-1);
			var validToDate1 = expired ? ZDate.Today.AddDays(-1) : ZDate.Today.AddDays(30);
			var validToDate2 = expired ? ZDate.Today.AddDays(-1) : ZDate.Today.AddDays(29);

			CreateEXVDocument(dichInvoice, JobRequiredDocument.DocUsage.Debtor, CountryCodes.Italy, "000001", dateReceived1, validToDate1, "1000");
			CreateEXVDocument(dichInvoice, JobRequiredDocument.DocUsage.Debtor, CountryCodes.Italy, "000002", dateReceived1, validToDate2, "1000.50");
			CreateEXVDocument(dichInvoice, JobRequiredDocument.DocUsage.Debtor, CountryCodes.UnitedStates, "000003", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(28));
			CreateEXVDocument(dichInvoice1, JobRequiredDocument.DocUsage.Creditor, CountryCodes.Italy, "000004", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(30), "1000");
			CreateEXVDocument(dichInvoice1, JobRequiredDocument.DocUsage.Creditor, CountryCodes.Italy, "000005", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(29), "1000.50");
			CreateEXVDocument(dichInvoice1, JobRequiredDocument.DocUsage.Creditor, CountryCodes.UnitedStates, "000006", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(28));

			Factory.Save();

			void CreateEXVDocument(InvoicingBase invoice, ZString docUsage, ZString country, ZString docNumber, ZDateTime dateReceived, ZDateTime validToDate, string ceilingLimit = null)
			{
				var doc = invoice.Header.RequiredDocuments.AddNew();
				doc.EQ_DocCategory = ReferenceTypes.ClientSupplierRelationship;
				doc.EQ_DocType = RefDocTypes.VATExporterExemption;
				doc.EQ_DocUsage = docUsage;
				doc.EQ_RN_NKRelatedCountry = country;
				doc.EQ_DocNumber = docNumber;
				doc.EQ_DateReceived = dateReceived.ToDateTimeOffset(null);
				doc.EQ_ValidToDate = validToDate;

				if (ceilingLimit != null)
				{
					var ceilingLimitAttribute = doc.Attributes.AddNew();
					ceilingLimitAttribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CeilingLimit;
					ceilingLimitAttribute.D0_AttribValue = ceilingLimit;
					var companyAttribute = doc.Attributes.AddNew();
					companyAttribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
					companyAttribute.D0_AttribValue = exvCompanyPk;
				}
			}
		}

		AccTaxRate dichIntTaxRate;

		public void TestCheckAH_PostedToEFT()
		{
			ZString localCurrency = "AUD";

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = localCurrency;

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			APCreditNote creditNote = Factory.NewWithValidTestData<APCreditNote>();

			bool[] trueFalse = { true, false };
			ZString[] currencys = { "AUD", "USD" };

			foreach (bool defaultValue in trueFalse)
			{
				AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);
				foreach (bool isAllowed in trueFalse)
				{
					Env.Security.AllowAPInvoiceChangeDefaultUseJobExchangeRate.IsAllowed = isAllowed;
					Env.Security.AllowAPCreditNoteChangeDefaultUseJobExchangeRate.IsAllowed = isAllowed;
					foreach (bool value in trueFalse)
					{
						foreach (ZString currency in currencys)
						{
							invoice.AH_RX_NKTransactionCurrency = currency;
							invoice.AH_PostedToEFT = value;
							IncompleteInvoicingBaseValidation testValidation = new IncompleteInvoicingBaseValidation(invoice);
							testValidation.ValidateAH_PostedToEFT();

							bool valueHasBeenChanged = value != defaultValue;
							bool isForeignCurrency = currency != localCurrency;
							bool shouldHaveError = !isAllowed && valueHasBeenChanged && isForeignCurrency;
							string message = string.Format("Invoice should {0}have error (IsAllowed = {1}, AH_PostedToEFT = {2}, DefaultValue = {3}, Is Foreign Invoice = {4})", (shouldHaveError ? "" : "not "), isAllowed, value, defaultValue, isForeignCurrency);

							if (shouldHaveError)
							{
								AssertHasErrors(message, invoice.AH_PostedToEFTInfo);
							}
							else
							{
								AssertNoErrors(message, invoice.AH_PostedToEFTInfo);
							}

							creditNote.AH_PostedToEFT = value;
							creditNote.AH_RX_NKTransactionCurrency = currency;
							testValidation = new IncompleteInvoicingBaseValidation(creditNote);
							testValidation.ValidateAH_PostedToEFT();

							message = string.Format("Credit Note should {0}have error (IsAllowed = {1}, AH_PostedToEFT = {2}, DefaultValue = {3}, Is Foreign Invoice = {4})", (shouldHaveError ? "" : "not "), isAllowed, value, defaultValue, isForeignCurrency);

							if (shouldHaveError)
							{
								AssertHasErrors(message, invoice.AH_PostedToEFTInfo);
							}
							else
							{
								AssertNoErrors(message, invoice.AH_PostedToEFTInfo);
							}
						}
					}
				}
			}
		}

		public void TestCheckIsSelfBillingInvoice()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
			InvoicingBaseCommonValidation validation = GetValidation(invoice);

			TestObjectCreator.AALSHI.CompanyData.OB_APCostsSelfBilled = false;
			validation.ValidateIsSelfBillingInvoice();
			AssertHasErrors(invoice.IsSelfBillingInvoiceInfo);

			TestObjectCreator.AALSHI.CompanyData.OB_APCostsSelfBilled = true;
			validation.ValidateIsSelfBillingInvoice();
			AssertNoErrors(invoice.IsSelfBillingInvoiceInfo);
		}

		[TestDate(2015, 5, 1)]
		public void TestCheckAH_ExchangeRate_ExRateOptions_Job() => AssertCheckAH_ExchangeRate_ExRateOptions(true);

		[TestDate(2015, 5, 1)]
		public void TestCheckAH_ExchangeRate_ExRateOptions_NonJob() => AssertCheckAH_ExchangeRate_ExRateOptions(false);

		public void AssertCheckAH_ExchangeRate_ExRateOptions(bool attachJobToInvoice)
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 2, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, 2, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.C07Rate, 3, ZDateTime.Today, ZDateTime.Today);

			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.RemoveAndDeleteAll();
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("ALL", "ALL", "ALL", ExchangeRateTypes.Code.C07Rate);
			GlbCompany.CurrentCompany.Factory.Save();

			var invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);

			var postingExRateRegistry = invoice.GetExRateLedger() == Enterprise.Integration.Accounting.ExchangeRateValidLedgerEnum.AR
				? AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR
				: AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;

			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
			if (attachJobToInvoice)
			{
				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				invoice.AH_JH = job.PK;
			}

			invoice.Company.AccExchangeRateConfigurations.Reload(true);
			AssertEquals("Pre-Condition: Config Rate Type to use is C07", ExchangeRateType.C07, AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationRateType(invoice.GetExchangeRateConfigurationRateConsumer(invoice.Job as Job), invoice.Header, ExchangeRateEnumsExtensions.GetLedgerFromCode(invoice.AH_Ledger), string.Empty));

			invoice.AH_ExchangeRate = 2;
			invoice.RunPreSaveValidation();
			AssertNoErrors(invoice.AH_ExchangeRateInfo);
			AssertNoWarnings(invoice.AH_ExchangeRateInfo);

			invoice.AH_ExchangeRate = 3;
			invoice.RunPreSaveValidation();
			AssertNoErrors(invoice.AH_ExchangeRateInfo);
			AssertNoWarnings(invoice.AH_ExchangeRateInfo);

			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);

			var ledgerStr = invoice.AH_Ledger == LedgerTypes.AccountsReceivable ? "AR" : "AP";

			if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				invoice.AH_ExchangeRate = 2;
				invoice.RunPreSaveValidation();
				AssertNoWarnings(invoice.AH_ExchangeRateInfo);
				AssertHasError("AR Ledger should use Ex Rate generated from Config Rate Type despite the value of UseJobExchangeRate", invoice.AH_ExchangeRateInfo, $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected 3.000000 rate but 2.000000 was entered.");

				invoice.AH_ExchangeRate = 3;
				invoice.RunPreSaveValidation();
				AssertNoWarnings(invoice.AH_ExchangeRateInfo);
				AssertNoErrors(invoice.AH_ExchangeRateInfo);
			}
			else
			{
				invoice.AH_ExchangeRate = 3;
				invoice.RunPreSaveValidation();
				AssertNoWarnings(invoice.AH_ExchangeRateInfo);
				AssertHasError("Other Ledger types should only use Ex Rate generated from Config Rate Type when UseJobExchangeRate is true", invoice.AH_ExchangeRateInfo, $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected 2.000000 rate but 3.000000 was entered.");

				invoice.AH_ExchangeRate = 2;
				invoice.RunPreSaveValidation();
				AssertNoWarnings(invoice.AH_ExchangeRateInfo);
				AssertNoErrors(invoice.AH_ExchangeRateInfo);
			}

			invoice.UseJobExchangeRate = true;

			AssertEquals(1m, invoice.AH_ExchangeRate);
			invoice.RunPreSaveValidation();
			AssertNoErrors(invoice.AH_ExchangeRateInfo);
			AssertHasWarning("Ex Rate generated from Config Rate Type should be used when UseJobExchangeRate", invoice.AH_ExchangeRateInfo, $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected 3.000000 rate but 1.000000 was entered.");

			invoice.UseJobExchangeRate = false;
			if (invoice is IAmending amending && invoice.HasImplementedGenerateAmendingTransaction && !(invoice is APInvoice))
			{
				var invoiceParent = Factory.New<ARInvoice>();
				invoice.AH_TransactionBelongsToGroup = invoiceParent.PK;
				amending.FlagAsCreatedAmending();

				if (attachJobToInvoice)
				{
					invoice.AH_JH = invoiceParent.AH_JH;
				}

				AssertEquals(true, invoice.IsAmendingOrReversal);

				invoice.AH_ExchangeRate = 5;
				using (TestObjectCreator.SetupAmendingTransactionCopyExchangeRateRegistry(invoice.AH_Ledger,invoice.AH_GC, true))
				{
					invoice.RunPreSaveValidation();
					AssertNoErrors(invoice.AH_ExchangeRateInfo);
					AssertHasWarning("Amendings should also validate exchange rate", invoice.AH_ExchangeRateInfo, $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected 3.000000 rate but 5.000000 was entered.");
				}

				using (TestObjectCreator.SetupAmendingTransactionCopyExchangeRateRegistry(invoice.AH_Ledger, invoice.AH_GC, false))
				{
					invoice.RunPreSaveValidation();
					AssertHasError("Amendings should also validate exchange rate", invoice.AH_ExchangeRateInfo, $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected 3.000000 rate but 5.000000 was entered.");
					AssertNoWarnings(invoice.AH_ExchangeRateInfo);

					invoice.UseJobExchangeRate = true;

					invoice.RunPreSaveValidation();
					AssertNoErrors(invoice.AH_ExchangeRateInfo);
					AssertHasWarning("Amendings should also validate exchange rate", invoice.AH_ExchangeRateInfo, $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected 3.000000 rate but 1.000000 was entered.");
				}

				invoice.UseJobExchangeRate = false;
				invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.EUR.Code;
				invoice.AH_ExchangeRate = 5;
				using (TestObjectCreator.SetupAmendingTransactionCopyExchangeRateRegistry(invoice.AH_Ledger, invoice.AH_GC, true))
				{
					invoice.RunPreSaveValidation();
					AssertHasWarning(invoice.AH_ExchangeRateInfo, $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the EUR exchange rate is not set for the date 01-May-15. Please check your data and try again.");
					AssertNoErrors(invoice.AH_ExchangeRateInfo);
				}

				using (TestObjectCreator.SetupAmendingTransactionCopyExchangeRateRegistry(invoice.AH_Ledger, invoice.AH_GC, false))
				{
					invoice.RunPreSaveValidation();
					AssertHasError(invoice.AH_ExchangeRateInfo, $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the EUR exchange rate is not set for the date 01-May-15. Please check your data and try again.");
					AssertNoWarnings(invoice.AH_ExchangeRateInfo);

					invoice.UseJobExchangeRate = true;

					invoice.RunPreSaveValidation();
					AssertHasWarning(invoice.AH_ExchangeRateInfo, $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the EUR exchange rate is not set for the date 01-May-15. Please check your data and try again.");
					AssertNoErrors(invoice.AH_ExchangeRateInfo);
				}

				invoice.UseJobExchangeRate = false;
				invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
				invoice.AH_TransactionBelongsToGroup = ZGuid.Empty;
				if (attachJobToInvoice)
				{
					invoice.AH_JH = ZGuid.Empty;
				}

				AssertEquals(false, invoice.IsAmendingOrReversal);
			}

			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.EUR.Code;
			invoice.AH_ExchangeRate = 2;
			invoice.RunPreSaveValidation();
			AssertHasError(invoice.AH_ExchangeRateInfo, $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the EUR exchange rate is not set for the date 01-May-15. Please check your data and try again.");
			AssertNoWarnings(invoice.AH_ExchangeRateInfo);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public void Test_AH_ExchangeRate_Overriding_WithSecurityRightsAndRegistryConfig()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 2, ZDateTime.Today, ZDateTime.Today);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP001", TestObjectCreator.USD, 2.000000m, 1000m, 100m, 0m, 2000m, 200m, 0m);
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;

			var postingExRateRegistry = AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);

			invoice.AH_ExchangeRate = 2.5;
			invoice.RunPreSaveValidation();

			AssertHasError(invoice.AH_ExchangeRateInfo, $@"The ""AP Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected 2.000000 rate but 2.500000 was entered.");

			Env.Security.NewPayablesOverridePostingExchangeRateAllows.IsAllowed = true;
			invoice.AH_OverrideExchangeRate = true;

			invoice.RunPreSaveValidation();

			AssertNoErrors(invoice.AH_ExchangeRateInfo);
			AssertNoWarnings(invoice.AH_ExchangeRateInfo);

			Factory.Save();

			var logsWithMessageCount = invoice.Logs.GetAllLogs().Where(x => x.SL_Reference == "Transaction Exchange Rate Updated From: '2.000000' to '2.5'").Count();
			AssertEquals("Event log added for Transaction exchange rate update", 1, logsWithMessageCount);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public void TestCheckAH_OSTotalAmount_Implementation()
		{
			var valid = (false, false);
			var oldInvalid = (true, false);
			var newInvalid = (false, true);

			var invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);

			if (invoice.IsARInvoiceOrCreditNoteOrAdjustmentNote)
			{
				AssertCheckAH_OSTotalAmount_ZeroBalance(false, 0m, oldInvalid, newInvalid, oldInvalid, newInvalid, newInvalid, newInvalid);
			}
			else
			{
				AssertCheckAH_OSTotalAmount_ZeroBalance(false, 0m, oldInvalid, valid, oldInvalid, valid, valid, valid);
			}

			AssertCheckAH_OSTotalAmount_ZeroBalance(false, 100m, valid, valid, valid, valid, valid, valid);
			AssertCheckAH_OSTotalAmount_ZeroBalance(true, 0m, oldInvalid, valid, oldInvalid, valid, valid, valid);
			AssertCheckAH_OSTotalAmount_ZeroBalance(true, 100m, valid, valid, valid, valid, valid, valid);
		}

		void AssertCheckAH_OSTotalAmount_ZeroBalance(bool isRegistryOn, decimal headerOSTotalAmount, params (bool ShouldHaveOldError, bool ShouldHaveNewError)[] expectedValues)
		{
			AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryOn);
			int index = 0;

			var invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoice.AH_OSTotalAmount = headerOSTotalAmount;
			invoice.HeaderValidation.ValidateAH_OSTotalAmount();
			AssertIsValidAndErrorMessage("No lines");

			invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoice.AH_OSTotalAmount = headerOSTotalAmount;
			var line = invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CommentChargeCode.PK;
			invoice.HeaderValidation.ValidateAH_OSTotalAmount();
			AssertIsValidAndErrorMessage("1 comment charge line");

			invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoice.AH_OSTotalAmount = headerOSTotalAmount;
			line = invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.FRT.PK;
			invoice.HeaderValidation.ValidateAH_OSTotalAmount();
			AssertIsValidAndErrorMessage("1 non-comment charge line");

			invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoice.AH_OSTotalAmount = headerOSTotalAmount;
			line = invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.FRT.PK;
			line = invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CommentChargeCode.PK;
			invoice.HeaderValidation.ValidateAH_OSTotalAmount();
			AssertIsValidAndErrorMessage("1 comment charge line, 1 non-comment");

			invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoice.AH_OSTotalAmount = headerOSTotalAmount;
			line = invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CommentChargeCode.PK;
			line = invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CommentChargeCode.PK;
			invoice.HeaderValidation.ValidateAH_OSTotalAmount();
			AssertIsValidAndErrorMessage("2 comment charge lines");

			invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoice.AH_OSTotalAmount = headerOSTotalAmount;
			line = invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.FRT.PK;
			line = invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.FRT.PK;
			invoice.HeaderValidation.ValidateAH_OSTotalAmount();
			AssertIsValidAndErrorMessage("2 non-comment charge lines");

			void AssertIsValidAndErrorMessage(string assertionMsg)
			{
				var fullAssertionMsg = $"{InvoiceType.ToString()}, header OS amount {headerOSTotalAmount}, registry value {isRegistryOn}, {assertionMsg}";
				var expectedValue = expectedValues[index++];

				invoice.HeaderValidation.ValidateAH_OSTotalAmount();

				AssertEquals(fullAssertionMsg, expectedValue.ShouldHaveOldError, invoice.AH_OSTotalAmountInfo.HasError(OldTotalAmountZeroError));
				AssertEquals(fullAssertionMsg, expectedValue.ShouldHaveNewError, invoice.AH_OSTotalAmountInfo.HasError(NewTotalAmountZeroError));
			}
		}

		#region Implementation

		protected abstract T GetValidation(TransactionHeader parent);
		protected abstract Type InvoiceType { get; }
		protected override Type HeaderType => InvoiceType;

		protected const string OldTotalAmountZeroError = "The sum of the transaction lines should not equal zero.";
		protected const string NewTotalAmountZeroError = "Transaction Total cannot be zero. This is controlled by the registry: Accounting -> Receivable Defaults -> Default Settings -> Allow Posting of Zero Value AR Invoices";

		#endregion
	}
}
