using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.ConsolRevenue;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(UnapprovedTransactionConverter))]
	public class UnapprovedTransactionConverterTest : NonPersistentBusinessObjectTestCase
	{
		[SuspendCriticalValidation]
		public void TestConvertToAP()
		{
			setupPeriodManagement(ZDateTime.Today.Year);
			AccountingConfigurationRegistry.Instance.PayableAllowUserToStoreARDoc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AccChargeCode chargeCode1 = TestObjectCreator.CC1;
			AccChargeCode chargeCode2 = TestObjectCreator.CC2;
			AccChargeCode chargeCode3 = TestObjectCreator.CC3;
			chargeCode2.AC_GC = differentCompany.PK;
			chargeCode3.AC_GC = localCompany.PK;

			Factory.Save();

			UAInvoice uAInvoice = (UAInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(UAInvoice), "UAINV", TestObjectCreator.AUD, 1.0m, 555m, 0, 555m, 0);
			uAInvoice.AH_OH = differentCompanyOrgProxy.PK;
			uAInvoice.Lines[0].AL_AC = chargeCode1.PK;
			uAInvoice.Lines[0].AL_JH = TestObjectCreator.Job1.PK;
			TestObjectCreator.CreateCharge(uAInvoice.Lines[0]);

			UACreditNote uACreditNote = (UACreditNote)TestObjectCreator.CreateInvoiceWithLine(typeof(UACreditNote), "UACRD", TestObjectCreator.AUD, 1.0m, 666m, 0, 666m, 0);
			uACreditNote.AH_OH = differentCompanyOrgProxy.PK;
			uACreditNote.Lines[0].AL_AC = chargeCode1.PK;
			uACreditNote.Lines[0].AL_JH = TestObjectCreator.Job1.PK;
			TestObjectCreator.CreateCharge(uACreditNote.Lines[0]);
			Factory.Save();

			ARInvoice aRInvoice1;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				aRInvoice1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m);
				aRInvoice1.AH_OH = TestObjectCreator.Agent.PK;
				aRInvoice1.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
				aRInvoice1.AH_DueDate = aRInvoice1.AH_DueDate.AddDays(5);

				AssertNotNull((Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

				InvoicingLineBase invoice1Line = TestObjectCreator.CreateInvoiceLine(aRInvoice1, TestObjectCreator.AUD, 1.0m, 50, differentBranch1.PK);
				invoice1Line.AL_AC = chargeCode2.PK;

				GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
				GlobalChargeCodeMapPivotIntercompany aPPivot = globalChargeCode.PivotCollection.AddNew();
				aPPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
				aPPivot.YP_AC = chargeCode1.PK;
				GlobalChargeCodeMapPivotIntercompany aRPivot = globalChargeCode.PivotCollection.AddNew();
				aRPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
				aRPivot.YP_AC = chargeCode2.PK;
			}

			ARInvoice aRInvoice2;
			ARCreditNote aRCreditNote;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch2.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				aRInvoice2 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 2.0m);
				aRInvoice2.AH_OH = TestObjectCreator.Agent.PK;

				aRCreditNote = (ARCreditNote)TestObjectCreator.CreateInvoice(typeof(ARCreditNote), TestObjectCreator.AUD, 3.0m);
				aRCreditNote.AH_OH = TestObjectCreator.Agent.PK;
				aRCreditNote.AH_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;

				InvoicingLineBase invoice2Line = TestObjectCreator.CreateInvoiceLine(aRInvoice2, TestObjectCreator.AUD, 1.0m, 333, differentBranch2.PK);
				InvoicingLineBase creditNoteLine = TestObjectCreator.CreateInvoiceLine(aRCreditNote, TestObjectCreator.AUD, 1.0m, 444, differentBranch2.PK);

				invoice2Line.AL_AC = chargeCode3.PK;
				creditNoteLine.AL_AC = chargeCode3.PK;

				Factory.Save();
			}

			var validTransactionsPKs = new[] { aRInvoice1.PK, aRInvoice2.PK, aRCreditNote.PK, uAInvoice.PK, uACreditNote.PK };

			var converter = new UnapprovedTransactionConverterExposed(new BusinessObjectFactory(), null);
			converter.Candidates.Load();
			AssertEquals("Prerequisite: candidates collection should have loaded the new invoices and credit notes", 5, converter.Candidates.Count);
			AssertContainsExactElementsInAnyOrder(validTransactionsPKs, converter.Candidates.Select(x => x.PK));

			foreach (InvoicingBase invoice in converter.Candidates)
			{
				AssertEquals("Prerequisite: candidates should NOT be AH_PostedInternal", false, invoice.AH_PostedInternal);
			}

			InvoicingBase firstInvoice = converter.ConvertToAP(aRInvoice1, false);
			AssertNotNull(firstInvoice);
			AssertEquals("Should Have One Parent Collection", 1, ((IBusinessObjectInternals)firstInvoice).ParentCollections.Length);
			AssertEquals("UnapprovedTransactionCandidateCollection", ((IBusinessObjectInternals)firstInvoice).ParentCollections[0].GetType().Name);
			firstInvoice.Factory.Save();
			AssertValidationTypeIsCorrect(firstInvoice, true);
			AssertEquals("Should Have One AR Invoice Doc", 1, firstInvoice.DocManagerInfo.AllEDocs.Count);
			AssertEquals("Should Have One AR Invoice Doc", string.Format("Invoice {0}.pdf", firstInvoice.AH_TransactionNum), firstInvoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Comapny should be the sending company", differentCompany.GC_Code, converter.contextofCompanyForStoredARInvoice_ForTestOnly);

			InvoicingBase secondInvoice = converter.ConvertToAP(aRInvoice2, false);
			AssertNotNull(secondInvoice);
			secondInvoice.Factory.Save();
			AssertValidationTypeIsCorrect(secondInvoice, true);

			AssertEquals(1, secondInvoice.Lines.Count);
			AssertNotEquals((Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), secondInvoice.Lines[0].AL_AG);
			AssertEquals("Should Have One AR Invoice Doc", 1, secondInvoice.DocManagerInfo.AllEDocs.Count);
			AssertEquals("Should Have One AR Invoice Doc", string.Format("Invoice {0}.pdf", secondInvoice.AH_TransactionNum), secondInvoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Comapny should be the sending company", localCompany.GC_Code, converter.contextofCompanyForStoredARInvoice_ForTestOnly);

			InvoicingBase thirdInvoice = converter.ConvertToAP(aRCreditNote, false);
			AssertNotNull(thirdInvoice);
			AssertEquals("Should Have One Parent Collection", 1, ((IBusinessObjectInternals)thirdInvoice).ParentCollections.Length);
			AssertEquals("UnapprovedTransactionCandidateCollection", ((IBusinessObjectInternals)thirdInvoice).ParentCollections[0].GetType().Name);
			AssertValidationTypeIsCorrect(thirdInvoice, true);

			thirdInvoice.Factory.Save();
			AssertEquals("Should Have One AR Invoice Doc", 1, thirdInvoice.DocManagerInfo.AllEDocs.Count);
			AssertEquals("Should Have One AR Invoice Doc", string.Format("Invoice {0}.pdf", thirdInvoice.AH_TransactionNum), thirdInvoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Comapny should be the sending company", localCompany.GC_Code, converter.contextofCompanyForStoredARInvoice_ForTestOnly);

			InvoicingBase fourthInvoice = converter.ConvertToAP(uAInvoice, false);
			AssertNotNull(fourthInvoice);
			fourthInvoice.Factory.Save();
			AssertValidationTypeIsCorrect(fourthInvoice, true);
			AssertEquals("Should Not Have One AR Invoice Doc", 0, fourthInvoice.DocManagerInfo.AllEDocs.Count);

			InvoicingBase fifthInvoice = converter.ConvertToAP(uACreditNote, false);
			AssertNotNull(fifthInvoice);
			fifthInvoice.Factory.Save();
			AssertValidationTypeIsCorrect(fifthInvoice, true);
			AssertEquals("Should Not Have One AR Invoice Doc", 0, fifthInvoice.DocManagerInfo.AllEDocs.Count);

			AssertEquals("First converted invoice should be of type APInvoice", typeof(APInvoice), firstInvoice.GetType());
			AssertEquals("Second converted invoice should be of type APInvoice", typeof(APInvoice), secondInvoice.GetType());
			AssertEquals("Third converted invoice should be of type APCreditNote", typeof(APCreditNote), thirdInvoice.GetType());
			AssertEquals("Fourth converted invoice should have type UAInvoice", typeof(UAInvoice), fourthInvoice.GetType());
			AssertEquals("Fourth converted invoice should have ledger AP", ZArchitecture.Core.LedgerTypes.AccountsPayable, fourthInvoice.AH_Ledger);
			AssertEquals("Fifth converted invoice should have type UAInvoice", typeof(UACreditNote), fifthInvoice.GetType());
			AssertEquals("Fifth converted invoice should have ledger AP", ZArchitecture.Core.LedgerTypes.AccountsPayable, fifthInvoice.AH_Ledger);

			AssertEquals("First converted invoice should be of type Invoice transaction type", ZArchitecture.Core.TransactionTypes.Invoice, firstInvoice.AH_TransactionType);
			AssertEquals("Second converted invoice should be of type Invoice transaction type", ZArchitecture.Core.TransactionTypes.Invoice, secondInvoice.AH_TransactionType);
			AssertEquals("Third converted invoice should be of type CreditNote transaction type", ZArchitecture.Core.TransactionTypes.CreditNote, thirdInvoice.AH_TransactionType);
			AssertEquals("Fourth converted invoice should be of type Invoice transaction type", ZArchitecture.Core.TransactionTypes.Invoice, fourthInvoice.AH_TransactionType);
			AssertEquals("Fifth converted invoice should be of type Invoice transaction type", ZArchitecture.Core.TransactionTypes.CreditNote, fifthInvoice.AH_TransactionType);

			Assert("IsConvertedFromARInvoice should be true", firstInvoice.IsConvertedFromARInvoice);
			Assert("IsConvertedFromARInvoice should be true", secondInvoice.IsConvertedFromARInvoice);
			Assert("IsConvertedFromARInvoice should be true", thirdInvoice.IsConvertedFromARInvoice);
			Assert("IsConvertedFromARInvoice shouldn't be true", !fourthInvoice.IsConvertedFromARInvoice);
			Assert("IsConvertedFromARInvoice shouldn't be true", !fifthInvoice.IsConvertedFromARInvoice);

			AssertEquals("Transaction Category should be 'STD'", Core.Constants.TransactionCategory.Codes.Standard, firstInvoice.AH_TransactionCategory);
			AssertEquals("Transaction Category should be 'STD'", Core.Constants.TransactionCategory.Codes.Standard, secondInvoice.AH_TransactionCategory);
			AssertEquals("Transaction Category should be 'STD'", Core.Constants.TransactionCategory.Codes.Standard, thirdInvoice.AH_TransactionCategory);
			AssertEquals("Transaction Category should be 'STD'", Core.Constants.TransactionCategory.Codes.Standard, fourthInvoice.AH_TransactionCategory);
			AssertEquals("Transaction Category should be 'STD'", Core.Constants.TransactionCategory.Codes.Standard, fifthInvoice.AH_TransactionCategory);

			AssertEquals("First invoice due date", aRInvoice1.AH_DueDate, firstInvoice.AH_DueDate);
			AssertNotEquals("First invoice due date should not equal invoice date", firstInvoice.AH_InvoiceDate, firstInvoice.AH_DueDate);

			//Need correct AH_Desc setting algorithm
			//AssertEquals("First converted invoice should have correct Description", "Test Invoice", firstInvoice.AH_Desc);
			//AssertEquals("Second converted invoice should have correct Description", "Test Invoice", secondInvoice.AH_Desc);
			//AssertEquals("Third converted invoice should have correct Description", "Test Invoice", thirdInvoice.AH_Desc);
			//AssertEquals("Fourth converted invoice should have correct Description", "AP INVOICE", fourthInvoice.AH_Desc);
			//AssertEquals("Fifth converted invoice should have correct Description", "AP CREDIT NOTE", fifthInvoice.AH_Desc);

			AssertEquals("First converted invoice's ExTax Amount should include Tax because it is cross country", 55M, firstInvoice.AH_OSExTaxAmount);
			AssertEquals("Second converted invoice's ExTax Amount", 333M, secondInvoice.AH_OSExTaxAmount);
			AssertEquals("Third converted invoice's ExTax Amount", 444M, thirdInvoice.AH_OSExTaxAmount);
			AssertEquals("Fourth converted invoice's ExTax Amount", 555M, fourthInvoice.AH_OSExTaxAmount);
			AssertEquals("Fifth converted invoice's ExTax Amount", 666M, fifthInvoice.AH_OSExTaxAmount);

			AssertEquals("First converted invoice's branch", proxyBranch.PK, firstInvoice.AH_GB);
			AssertEquals("Second converted invoice's branch", proxyBranch.PK, secondInvoice.AH_GB);
			AssertEquals("Third converted invoice's branch", proxyBranch.PK, thirdInvoice.AH_GB);
			AssertEquals("Fourth converted invoice's branch", originalBranch.PK, fourthInvoice.AH_GB);
			AssertEquals("Fifth converted invoice's branch", originalBranch.PK, fifthInvoice.AH_GB);

			AssertEquals("First converted invoice's Creditor", aRInvoice1.Branch.OrgProxy.PK, firstInvoice.AH_OH);
			AssertEquals("Second converted invoice's Creditor", aRInvoice2.Branch.OrgProxy.PK, secondInvoice.AH_OH);
			AssertEquals("Third converted invoice's Creditor", aRCreditNote.Branch.OrgProxy.PK, thirdInvoice.AH_OH);

			AssertEquals("First converted invoice's Currency", aRInvoice1.AH_RX_NKTransactionCurrency, firstInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("Second converted invoice's Currency", aRInvoice2.AH_RX_NKTransactionCurrency, secondInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("Third converted invoice's Currency", aRCreditNote.AH_RX_NKTransactionCurrency, thirdInvoice.AH_RX_NKTransactionCurrency);

			AssertEquals("First converted invoice Line should be have Charge code 1", chargeCode1.PK, firstInvoice.Lines[0].AL_AC);

			//AssertEquals("First converted invoice's organisation (should come from branch's orgproxy)", ARInvoice1.Branch.OrgProxy.PK, firstInvoice.AH_OH);
			//AssertEquals("Second converted invoice's organisation (should fall back to company's orgproxy)", ARInvoice2.Branch.Company.OrgProxy.PK, secondInvoice.AH_OH);
			//AssertEquals("Third converted invoice's organisation (should fall back to company's orgproxy)", ARCreditNote.Branch.Company.OrgProxy.PK, thirdInvoice.AH_OH);
			//AssertEquals("Fourth converted invoice's organisation (should fall back to company's orgproxy)", UAInvoice.AH_OH, fourthInvoice.AH_OH);
			//AssertEquals("Fifth converted invoice's organisation (should fall back to company's orgproxy)", UACreditNote.AH_OH, fifthInvoice.AH_OH);

			StmALog approvalEvent = GetApprovalEvent(firstInvoice);
			AssertNotNull(approvalEvent);
			AssertEquals("Intercompany Invoice Approved", approvalEvent.SL_Reference);
			approvalEvent = GetApprovalEvent(secondInvoice);
			AssertNotNull(approvalEvent);
			AssertEquals("Intercompany Invoice Approved", approvalEvent.SL_Reference);
			approvalEvent = GetApprovalEvent(thirdInvoice);
			AssertNotNull(approvalEvent);
			AssertEquals("Intercompany Invoice Approved", approvalEvent.SL_Reference);
			approvalEvent = GetApprovalEvent(fourthInvoice);
			AssertNotNull(approvalEvent);
			AssertEquals("AP Invoice Approved", approvalEvent.SL_Reference);
			approvalEvent = GetApprovalEvent(fifthInvoice);
			AssertNotNull(approvalEvent);
			AssertEquals("AP Invoice Approved", approvalEvent.SL_Reference);

			foreach (InvoicingBase invoice in converter.Candidates)
			{
				AssertEquals("Candidates should now be AH_PostedInternal", invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable, invoice.AH_PostedInternal);
			}

			converter.Candidates.Load();
			AssertEquals("Reloaded candidates collection should be empty", 0, converter.Candidates.Count);
		}

		[SuspendCriticalValidation]
		public void TestConvertToAPWithComplianceNumber()
		{
			setupPeriodManagement(ZDateTime.Today.Year);
			Factory.Save();

			ARInvoice aRInvoice1;
			ARInvoice aRInvoice2;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				aRInvoice1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m);
				aRInvoice1.AH_OH = originalBranch.GB_OH_OrgProxy;
				aRInvoice1.AH_TransactionReference = "";
				var invoice1Line = TestObjectCreator.CreateInvoiceLine(aRInvoice1, TestObjectCreator.AUD, 1.0m, 50, differentBranch1.PK);

				aRInvoice2 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 2.0m);
				aRInvoice2.AH_OH = originalBranch.GB_OH_OrgProxy;
				aRInvoice2.AH_TransactionReference = "COM123456";
				var invoice2Line = TestObjectCreator.CreateInvoiceLine(aRInvoice2, TestObjectCreator.AUD, 1.0m, 333, differentBranch1.PK);

				Factory.Save();
			}

			var converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
			converter.Candidates.Load();
			AssertEquals("Prerequisite: candidates collection should have loaded the 2 new invoices", 2, converter.Candidates.Count);

			var firstInvoice = converter.ConvertToAP(aRInvoice1, false);
			AssertNotNull(firstInvoice);
			AssertEquals("AH_TransactionNum of firstInvoice should default to AH_TransactionNum of ARInvoice1", aRInvoice1.AH_TransactionNum, firstInvoice.AH_TransactionNum);
			AssertEquals("AH_ChequeOrReference of firstInvoice should default to AH_ChequeOrReference of ARInvoice1", aRInvoice1.AH_ChequeOrReference, firstInvoice.AH_ChequeOrReference);
			AssertNullOrEmpty("AH_TransactionReference of firstInvoice should be empty", firstInvoice.AH_TransactionReference);

			var secondInvoice = converter.ConvertToAP(aRInvoice2, false);
			AssertNotNull(secondInvoice);
			AssertEquals("AH_TransactionNum of secondInvoice should default to AH_TransactionReference of ARInvoice2", aRInvoice2.AH_TransactionReference, secondInvoice.AH_TransactionNum);
			AssertEquals("AH_ChequeOrReference of secondInvoice should default to AH_TransactionNum of ARInvoice2", aRInvoice2.AH_TransactionNum, secondInvoice.AH_ChequeOrReference);
			AssertNullOrEmpty("AH_TransactionReference of secondInvoice should be empty", secondInvoice.AH_TransactionReference);

			AccountingConfigurationRegistry.Instance.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
			converter.Candidates.Load();
			AssertEquals("Prerequisite: candidates collection should have loaded the 2 new invoices", 2, converter.Candidates.Count);

			firstInvoice = converter.ConvertToAP(aRInvoice1, false);
			AssertNotNull(firstInvoice);
			AssertEquals("AH_TransactionNum of firstInvoice  should default to AH_TransactionNum of ARInvoice1", aRInvoice1.AH_TransactionNum, firstInvoice.AH_TransactionNum);
			AssertEquals("AH_ChequeOrReference of firstInvoice should default to AH_ChequeOrReference  of ARInvoice1", aRInvoice1.AH_ChequeOrReference, firstInvoice.AH_ChequeOrReference);
			AssertNullOrEmpty("AH_TransactionReference of firstInvoice should be empty", firstInvoice.AH_TransactionReference);

			secondInvoice = converter.ConvertToAP(aRInvoice2, false);
			AssertNotNull(secondInvoice);
			AssertEquals("As registry is set to 'No' AH_TransactionNum of secondInvoice should default to AH_TransactionNum of ARInvoice2", aRInvoice2.AH_TransactionNum, secondInvoice.AH_TransactionNum);
			AssertEquals("As registry is set to 'No' AH_ChequeOrReference of secondInvoice should default to AH_ChequeOrReference of ARInvoice2", aRInvoice2.AH_ChequeOrReference, secondInvoice.AH_ChequeOrReference);
			AssertNullOrEmpty("AH_TransactionReference of secondInvoice should be empty", secondInvoice.AH_TransactionReference);

			AccountingConfigurationRegistry.Instance.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
			converter.Candidates.Load();
			AssertEquals("Prerequisite: candidates collection should have loaded the 2 new invoices", 2, converter.Candidates.Count);

			firstInvoice = converter.ConvertToAP(aRInvoice1, false);
			AssertNotNull(firstInvoice);
			AssertEquals("AH_TransactionNum of firstInvoice  should default to AH_TransactionNum of ARInvoice1", aRInvoice1.AH_TransactionNum, firstInvoice.AH_TransactionNum);
			AssertEquals("AH_ChequeOrReference of firstInvoice should default to AH_ChequeOrReference  of ARInvoice1", aRInvoice1.AH_ChequeOrReference, firstInvoice.AH_ChequeOrReference);
			AssertNullOrEmpty("AH_TransactionReference of firstInvoice should be empty", firstInvoice.AH_TransactionReference);

			secondInvoice = converter.ConvertToAP(aRInvoice2, false);
			AssertNotNull(secondInvoice);
			AssertEquals("As registry is set to 'No' AH_TransactionNum of secondInvoice should default to AH_TransactionNum of ARInvoice2", aRInvoice2.AH_TransactionNum, secondInvoice.AH_TransactionNum);
			AssertEquals("As registry is set to 'No' AH_ChequeOrReference of secondInvoice should default to AH_ChequeOrReference of ARInvoice2", aRInvoice2.AH_ChequeOrReference, secondInvoice.AH_ChequeOrReference);
			AssertNullOrEmpty("AH_TransactionReference of secondInvoice should be empty", secondInvoice.AH_TransactionReference);
		}

		[SuspendCriticalValidation]
		[TestDate(2021, 06, 12)]
		public void TestConvertToAPWithoutComplianceSequenceLinked()
		{
			var currComp = GlbCompany.CurrentCompany;
			using (currComp.TemporarilySetCountry(CountryCodes.Italy))
			{
				setupPeriodManagement(ZDateTime.Today.Year);

				const string subType = "APS";
				var sequence = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, subType, 1, 99, 25);
				sequence.XD_Prefix = "APS-";
				sequence.XD_GC_Company = currComp.PK;
				sequence.XD_GB_BranchOwner = originalBranch.PK;

				Factory.Save();

				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var config = collection.AddNew();
				config.Country = currComp.Country.Code;
				config.SubType = subType;
				config.LedgerType = LedgerTypes.AccountsPayable;
				config.InvoiceType = TransactionTypes.Invoice;
				config.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
				config.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				config.OriginalRule = OriginalRuleCodes.AllTransactions;

				ARInvoice aRInvoice;
				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
				{
					aRInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.EUR, 1.0m);
					aRInvoice.AH_OH = originalBranch.GB_OH_OrgProxy;
					aRInvoice.AH_InvoiceDate = new ZDate(2021, 06, 12);
					aRInvoice.AH_PostDate = new ZDate(2020, 12, 1);

					var invoiceLine = TestObjectCreator.CreateInvoiceLine(aRInvoice, TestObjectCreator.EUR, 1.0m, 50, differentBranch1.PK);

					Factory.Save();
				}

				var registry = AccountingMasterFilesRegistry.Instance;
				using (registry.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				{
					Assert(currComp.Country.SupportComplianceSubType);

					var converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
					converter.Candidates.Load();
					AssertEquals("Prerequisite: candidates collection should have loaded the 1 new invoice", 1, converter.Candidates.Count);

					var aPInvoice = converter.ConvertToAP(aRInvoice, false);
					AssertNotNull(aPInvoice);
					aPInvoice.AH_GB = originalBranch.PK;
					aPInvoice.Lines[0].AL_Desc = "Test Line Desc1";
					aPInvoice.Lines[0].AL_GSTVAT = 10;
					Assert($"Invoice converted should not have errors:\n{string.Join("\n", aPInvoice.NotificationsIncludingChildren.GetUniqueMessageList())}", !aPInvoice.HasErrors);
					bool complSubTypeFound = aPInvoice.SetComplianceSubTypeIfIsNecessary();
					Assert("Invoice converted should set Compliance SubType successfully", complSubTypeFound);

					using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currComp.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
					{
						converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
						converter.Candidates.Load();
						AssertEquals("Prerequisite: candidates collection should have loaded the 1 new invoices", 1, converter.Candidates.Count);

						aPInvoice = converter.ConvertToAP(aRInvoice, false);
						AssertNotNull(aPInvoice);
						aPInvoice.AH_GB = originalBranch.PK;
						aPInvoice.Lines[0].AL_Desc = "Test Line Desc2";
						aPInvoice.Lines[0].AL_GSTVAT = 10;
						Assert($"Invoice converted with Compliance Order by Post Date should not have errors:\n{string.Join("\n", aPInvoice.NotificationsIncludingChildren.GetUniqueMessageList())}", !aPInvoice.HasErrors);
						complSubTypeFound = aPInvoice.SetComplianceSubTypeIfIsNecessary();
						Assert("Invoice converted with Compliance Order by Post Date should set Compliance SubType successfully", complSubTypeFound);
					}
				}
			}
		}

		[SuspendCriticalValidation]
		public void TestConvertToAP_DefaultCreditor()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			AccChargeCode chargeCode1 = TestObjectCreator.CC1;
			AccChargeCode chargeCode2 = TestObjectCreator.CC2;
			AccChargeCode chargeCode3 = TestObjectCreator.CC3;
			chargeCode2.AC_GC = differentCompany.PK;
			chargeCode3.AC_GC = localCompany.PK;

			differentBranchOrgProxy.OH_IsCreditor = true;

			OrgHeader differentBranch2Proxy = Factory.Load<OrgHeader>(differentBranch2.GB_OH_OrgProxy);
			differentBranch2Proxy.OH_IsCreditor = false;
			differentBranch2.Company.OrgProxy.OH_IsCreditor = true;

			Factory.Save();

			ARInvoice aRInvoice1;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				aRInvoice1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m);
				aRInvoice1.AH_OH = TestObjectCreator.Agent.PK;
				aRInvoice1.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
				aRInvoice1.AH_DueDate = aRInvoice1.AH_DueDate.AddDays(5);

				AssertNotNull((Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

				InvoicingLineBase invoice1Line = TestObjectCreator.CreateInvoiceLine(aRInvoice1, TestObjectCreator.AUD, 1.0m, 50, differentBranch1.PK);
				invoice1Line.AL_AC = chargeCode2.PK;

				GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
				GlobalChargeCodeMapPivotIntercompany aPPivot = globalChargeCode.PivotCollection.AddNew();
				aPPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
				aPPivot.YP_AC = chargeCode1.PK;
				GlobalChargeCodeMapPivotIntercompany aRPivot = globalChargeCode.PivotCollection.AddNew();
				aRPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
				aRPivot.YP_AC = chargeCode2.PK;
			}

			ARInvoice aRInvoice2;
			ARCreditNote aRCreditNote;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch2.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				aRInvoice2 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 2.0m);
				aRInvoice2.AH_OH = TestObjectCreator.Agent.PK;

				aRCreditNote = (ARCreditNote)TestObjectCreator.CreateInvoice(typeof(ARCreditNote), TestObjectCreator.AUD, 3.0m);
				aRCreditNote.AH_OH = TestObjectCreator.Agent.PK;
				aRCreditNote.AH_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;

				InvoicingLineBase invoice2Line = TestObjectCreator.CreateInvoiceLine(aRInvoice2, TestObjectCreator.AUD, 1.0m, 333, differentBranch2.PK);
				InvoicingLineBase creditNoteLine = TestObjectCreator.CreateInvoiceLine(aRCreditNote, TestObjectCreator.AUD, 1.0m, 444, differentBranch2.PK);

				invoice2Line.AL_AC = chargeCode3.PK;
				creditNoteLine.AL_AC = chargeCode3.PK;

				Factory.Save();
			}

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
			converter.Candidates.Load();
			AssertEquals("Prerequisite: candidates collection should have loaded the new invoices and credit notes", 3, converter.Candidates.Count);
			foreach (InvoicingBase invoice in converter.Candidates)
			{
				AssertEquals("Prerequisite: candidates should NOT be AH_PostedInternal", false, invoice.AH_PostedInternal);
			}

			InvoicingBase firstInvoice = converter.ConvertToAP(aRInvoice1, false);
			AssertNotNull(firstInvoice);
			AssertEquals("Should Have One Parent Collection", 1, ((IBusinessObjectInternals)firstInvoice).ParentCollections.Length);
			AssertEquals("UnapprovedTransactionCandidateCollection", ((IBusinessObjectInternals)firstInvoice).ParentCollections[0].GetType().Name);
			firstInvoice.Factory.Save();
			AssertValidationTypeIsCorrect(firstInvoice, true);

			InvoicingBase secondInvoice = converter.ConvertToAP(aRInvoice2, false);
			AssertNotNull(secondInvoice);
			secondInvoice.Factory.Save();
			AssertValidationTypeIsCorrect(secondInvoice, true);

			AssertEquals(1, secondInvoice.Lines.Count);
			AssertNotEquals((Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), secondInvoice.Lines[0].AL_AG);

			InvoicingBase thirdInvoice = converter.ConvertToAP(aRCreditNote, false);
			AssertNotNull(thirdInvoice);
			AssertEquals("Should Have One Parent Collection", 1, ((IBusinessObjectInternals)thirdInvoice).ParentCollections.Length);
			AssertEquals("UnapprovedTransactionCandidateCollection", ((IBusinessObjectInternals)thirdInvoice).ParentCollections[0].GetType().Name);
			AssertValidationTypeIsCorrect(thirdInvoice, true);

			thirdInvoice.Factory.Save();

			AssertEquals("First converted invoice should be of type APInvoice", typeof(APInvoice), firstInvoice.GetType());
			AssertEquals("Second converted invoice should be of type APInvoice", typeof(APInvoice), secondInvoice.GetType());
			AssertEquals("Third converted invoice should be of type APCreditNote", typeof(APCreditNote), thirdInvoice.GetType());

			AssertEquals("First converted invoice should be of type Invoice transaction type", ZArchitecture.Core.TransactionTypes.Invoice, firstInvoice.AH_TransactionType);
			AssertEquals("Second converted invoice should be of type Invoice transaction type", ZArchitecture.Core.TransactionTypes.Invoice, secondInvoice.AH_TransactionType);
			AssertEquals("Third converted invoice should be of type CreditNote transaction type", ZArchitecture.Core.TransactionTypes.CreditNote, thirdInvoice.AH_TransactionType);

			Assert("IsConvertedFromARInvoice should be true", firstInvoice.IsConvertedFromARInvoice);
			Assert("IsConvertedFromARInvoice should be true", secondInvoice.IsConvertedFromARInvoice);
			Assert("IsConvertedFromARInvoice should be true", thirdInvoice.IsConvertedFromARInvoice);

			AssertEquals("Transaction Category should be 'STD'", Core.Constants.TransactionCategory.Codes.Standard, firstInvoice.AH_TransactionCategory);
			AssertEquals("Transaction Category should be 'STD'", Core.Constants.TransactionCategory.Codes.Standard, secondInvoice.AH_TransactionCategory);
			AssertEquals("Transaction Category should be 'STD'", Core.Constants.TransactionCategory.Codes.Standard, thirdInvoice.AH_TransactionCategory);

			AssertEquals("First converted invoice's branch", proxyBranch.PK, firstInvoice.AH_GB);
			AssertEquals("Second converted invoice's branch", proxyBranch.PK, secondInvoice.AH_GB);
			AssertEquals("Third converted invoice's branch", proxyBranch.PK, thirdInvoice.AH_GB);

			AssertEquals("First converted invoice's Creditor must be its Branch Proxy which has IsCreditor = true", differentBranchOrgProxy.PK, firstInvoice.AH_OH);
			AssertEquals("Second converted invoice's Creditor must be its Company Proxy which has IsCreditor = true but Branch Proxy has IsCreditor = false", differentBranch2.Company.OrgProxy.PK, secondInvoice.AH_OH);
			AssertEquals("Third converted invoice's Creditor must be its Company Proxy which has IsCreditor = true but Branch Proxy has IsCreditor = false", differentBranch2.Company.OrgProxy.PK, thirdInvoice.AH_OH);

			foreach (InvoicingBase invoice in converter.Candidates)
			{
				AssertEquals("Candidates should now be AH_PostedInternal", invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable, invoice.AH_PostedInternal);
			}

			converter.Candidates.Load();
			AssertEquals("Reloaded candidates collection should be empty", 0, converter.Candidates.Count);
		}

		void setupPeriodManagement(int year)
		{
			PeriodManagementTestHelper.PostPeriodsForEntireYear(year, GlbCompany.CurrentCompany.PK);
			PeriodManagementTestHelper.PostPeriodsForEntireYear(year, differentCompany.PK);
			PeriodManagementTestHelper.PostPeriodsForEntireYear(year, localCompany.PK);
		}

		void setupRegistryForPostDateDefaulting()
		{
			BackDateAPInvoicesConfiguration backDateAPInvoicesConfiguration = new BackDateAPInvoicesConfiguration();
			backDateAPInvoicesConfiguration.PostDateConfigurationCollection.RemoveAll();
			addPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "ALL", "ALL", "", "ARV", "EPM", "SGN", "SGN", "ADD");
			addPostDateConfiguration(backDateAPInvoicesConfiguration, "FCN", "ALL", "ALL", "", "DEP", "EPM", "SGN", "SGN", "ADD");
			AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateAPInvoicesConfiguration);
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		void addPostDateConfiguration(BackDateAPInvoicesConfiguration backDateAPInvoicesConfiguration, string jobType, string direction, string mode, string broker,
									string significantDateCode, string priorClosedPeriod, string priorOpenPeriod, string currentPeriod, string futurePeriod)
		{
			PostDateConfiguration config = backDateAPInvoicesConfiguration.PostDateConfigurationCollection.AddNew();
			config.JobType = jobType;
			config.DirectionCode = direction;
			config.Mode = mode;
			config.BrokerCode = broker;
			config.SignificantDateCode = significantDateCode;
			config.PriorClosedPeriod = priorClosedPeriod;
			config.PriorOpenPeriod = priorOpenPeriod;
			config.CurrentPeriod = currentPeriod;
			config.FuturePeriod = futurePeriod;
		}

		Job createJob(BusinessObjectFactory factory, ZString jobNumber, ZGuid branch, ZGuid department, OrgHeader localClient, OrgHeader agent)
		{
			Job job = factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_JobNum = jobNumber;
			job.JH_GB = branch;
			job.JH_GE = department;
			job.LocalChargesPK = localClient != null ? localClient.PK : ZGuid.Empty;
			job.AgentCollectPK = agent != null ? agent.PK : ZGuid.Empty;
			return job;
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertToAPWithConsolARInvoiceWhenNonJobRelatedLinesPresentAndMultipleInstancesOfSameChargeCode()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);

			originalDepartment.GE_Misc = false;
			GlbDepartment department = Factory.Load<GlbDepartment>(originalDepartment.PK);
			department.GE_Misc = false;

			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(false);
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(false);
			Factory.Save();

			AccChargeCode chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			AccChargeCode chargeCode2 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, differentCompany);

			TestObjectCreator.GLHeader1.AG_AccountType = "P&L";
			AccGLHeader header = TestObjectCreator.CreateAccGLHeader("yo", "TS", "yo", "P&L", "DR");
			AccGLHeader headerDisallowDirectPosting = TestObjectCreator.CreateAccGLHeader("yoDDP", "TS", "yo", "P&L", "DR");
			headerDisallowDirectPosting.AG_DisallowDirectPosting = true;
			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = "AIR";
			ForwardingShipment forwardingShipment1 = consol.Shipments.AddNew();
			forwardingShipment1.JS_UniqueConsignRef = "S00002000";
			ForwardingShipment forwardingShipment2 = consol.Shipments.AddNew();
			forwardingShipment2.JS_UniqueConsignRef = "S00002001";
			Factory.Save();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = chargeCode1.PK;
			cost.E6_ApportionmentMethod = "SHP";
			ZDecimal consolCostAmount = 2000.00m;
			cost.E6_OSCostAmount = consolCostAmount;
			Factory.Save();

			AssertEquals("Consol Cost Amount", consolCostAmount, cost.E6_OSCostAmount);

			ZDecimal jobCharge1Amount = 1000.00m;
			ZDecimal jobCharge2Amount = 500.00m;
			ZDecimal jobCharge3Amount = 500.00m;

			InvoicingBase transaction = null;
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				OrgHeader agent = factory2.Load<OrgHeader>(TestObjectCreator.Agent.PK);
				agent.CompanyData.OB_IsDebtor = true;
				agent.CompanyData.SetARTaxApplicable(false);
				agent.CompanyData.SetAPTaxApplicable(false);

				consol = factory2.Load<ForwardingConsol>(consol.PK);

				ForwardingShipment shipment1 = consol.Shipments[0];
				ForwardingShipment shipment2 = consol.Shipments[1];

				OrgHeader localClient = factory2.NewWithValidTestData<OrgHeader>();

				Job job1 = createJob(factory2, "J00001000", differentBranch1.PK, originalDepartment.PK, localClient, agent);
				job1.JH_ParentID = shipment1.PK;
				job1.JH_ParentTableCode = "JS";
				Job job2 = createJob(factory2, "J00001001", differentBranch1.PK, originalDepartment.PK, localClient, agent);
				job2.JH_ParentID = shipment2.PK;
				job2.JH_ParentTableCode = "JS";
				factory2.Save();

				Charge jobCharge1 = job1.Charges.AddNew();
				jobCharge1.JR_AC = chargeCode2.PK;
				jobCharge1.JR_GB = differentBranch1.PK;
				jobCharge1.JR_GE = originalDepartment.PK;
				jobCharge1.JR_JH = job1.PK;
				jobCharge1.JR_OSCostAmt = jobCharge1Amount;
				jobCharge1.JR_OSSellAmt = jobCharge1Amount;
				jobCharge1.JR_OH_SellAccount = agent.PK;
				jobCharge1.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;

				Charge jobCharge2 = job2.Charges.AddNew();
				jobCharge2.JR_AC = chargeCode2.PK;
				jobCharge2.JR_GB = differentBranch1.PK;
				jobCharge2.JR_GE = originalDepartment.PK;
				jobCharge2.JR_JH = job2.PK;
				jobCharge2.JR_OSCostAmt = jobCharge2Amount;
				jobCharge2.JR_OSSellAmt = jobCharge2Amount;
				jobCharge2.JR_OH_SellAccount = agent.PK;
				jobCharge2.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;

				Charge jobCharge3 = job2.Charges.AddNew();
				jobCharge3.JR_AC = chargeCode2.PK;
				jobCharge3.JR_GB = differentBranch1.PK;
				jobCharge3.JR_GE = originalDepartment.PK;
				jobCharge3.JR_JH = job2.PK;
				jobCharge3.JR_OSCostAmt = jobCharge3Amount;
				jobCharge3.JR_OSSellAmt = jobCharge3Amount;
				jobCharge3.JR_OH_SellAccount = agent.PK;
				jobCharge3.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;

				factory2.Save();

				AssertEquals("Job Charge 1 JR_OSCostAmt", jobCharge1Amount, jobCharge1.JR_OSCostAmt);
				AssertEquals("Job Charge 2 JR_OSCostAmt", jobCharge2Amount, jobCharge2.JR_OSCostAmt);
				AssertEquals("Job Charge 1 JR_OSSellAmt", jobCharge1Amount, jobCharge1.JR_OSSellAmt);
				AssertEquals("Job Charge 2 JR_OSSellAmt", jobCharge2Amount, jobCharge2.JR_OSSellAmt);
				AssertEquals("Job Charge 3 JR_OSSellAmt", jobCharge3Amount, jobCharge3.JR_OSSellAmt);
				AssertEquals("Job Charge 3 JR_OSCostAmt", jobCharge3Amount, jobCharge3.JR_OSCostAmt);

				var jobs = new[] { job1, job2 };
				ConsolInvoicingPostManager transactionCreator = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(factory2, consol));
				TransactionCreatorHashtable transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);
				AssertEquals("One AR transaction should have been created", 1, transactions.ARTransactionsCount);
				InvoicingBase[] arInvoices = transactions.GetAllARInvoicesAndCreditNotes();
				transaction = arInvoices[0];

				InvoiceLine line1 = (InvoiceLine)transaction.Lines.AddNew();
				line1.FillWithValidTestData();
				line1.AL_OSExTaxAmount = -100;
				line1.AL_AG = header.PK;
				line1.AL_Desc = "Line Description";
				Assert(line1.AL_JH.IsEmpty);
				line1.AL_JH = ZGuid.Empty;

				InvoiceLine line2 = (InvoiceLine)transaction.Lines.AddNew();
				line2.FillWithValidTestData();
				line2.AL_OSExTaxAmount = -200;
				line2.AL_AG = headerDisallowDirectPosting.PK;
				line2.AL_Desc = "Line Description";
				Assert(line2.AL_JH.IsEmpty);
				line2.AL_JH = ZGuid.Empty;

				AssertEquals("AR Invoice should have 5 lines", 5, transaction.Lines.Count);
				AssertEquals("AR Invoice Line 1 Amount", jobCharge1Amount, transaction.Lines[0].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 2 Amount", jobCharge2Amount, transaction.Lines[1].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 3 Amount", jobCharge3Amount, transaction.Lines[2].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 4 Amount", -100m, transaction.Lines[3].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 5 Amount", -200m, transaction.Lines[4].AL_OSExTaxAmount);

				AssertEquals("AR Invoice Line 1 ChargeCode", chargeCode2.PK, transaction.Lines[0].AL_AC);
				AssertEquals("AR Invoice Line 2 ChargeCode", chargeCode2.PK, transaction.Lines[1].AL_AC);
				AssertEquals("AR Invoice Line 3 ChargeCode", chargeCode2.PK, transaction.Lines[2].AL_AC);
				AssertEquals("AR Invoice Line 4 GL Account", header.PK, transaction.Lines[3].AL_AG);
				AssertEquals("AR Invoice Line 5 GL Account", headerDisallowDirectPosting.PK, transaction.Lines[4].AL_AG);

				AssertEquals("Invoice total should be 1700", 1700m, transaction.AH_OSTotal);
				factory2.Save();
			}

			factory2 = new BusinessObjectFactory();
			transaction = factory2.Load<ARInvoice>(transaction.PK);

			InvoicingBase convertedInvoice = converter.ConvertToAP(transaction, false);
			AssertNotNull("Converted Invoice should not be null", convertedInvoice);
			Assert("Converted Invoice should be an AP Invoice", convertedInvoice is APInvoice);

			TransactionLinesCollection linesWithNoJob = new TransactionLinesCollection(factory2);
			foreach (InvoicingLineBase line in convertedInvoice.Lines)
			{
				if (line.AL_JH.IsEmpty)
				{
					linesWithNoJob.Add(line);
				}
				else
				{
					Assert(!line.HasErrors());
				}
			}
			AssertEquals("Should be 2 lines with no job", 2, linesWithNoJob.Count);
			InvoicingLineBase lineWithGLAccount = (InvoicingLineBase)linesWithNoJob[0];
			InvoicingLineBase lineWithoutGLAccount = (InvoicingLineBase)linesWithNoJob[1];
			Assert(!lineWithGLAccount.HasErrors());
			Assert(lineWithoutGLAccount.HasErrors());
			AssertHasErrors(lineWithoutGLAccount.GenericChargeInfo);
			lineWithoutGLAccount.GenericCharge = header.PK;

			Assert(string.Format("AP Invoice should not have errors: {0}", convertedInvoice.NotificationsIncludingChildren.ToMessageListString()), !convertedInvoice.HasErrors);
			convertedInvoice.Factory.Save();
			AssertEquals("Line Count", 4, convertedInvoice.Lines.Count);
			ZQuery chargeCode1LinesFilter = new ZQuery(AccTransactionLinesSchema.AL_AC, chargeCode1.PK);
			TransactionLine[] lines = (TransactionLine[])convertedInvoice.Lines.Find(chargeCode1LinesFilter);
			AssertEquals(1000m, lines[0].AL_OSExTaxAmount);
			AssertEquals(1000m, lines[1].AL_OSExTaxAmount);

			ZQuery amountFilter = new ZQuery(AccTransactionLinesSchema.AL_LineAmount, 100m);
			lines = (TransactionLine[])convertedInvoice.Lines.Find(amountFilter);
			AssertNotNull("Should be 1 line with value -100 (AP Invoices have InvertSigns=true)", lines[0]);
			AssertEquals(header.PK, lines[0].AL_AG);

			amountFilter = new ZQuery(AccTransactionLinesSchema.AL_LineAmount, 200m);
			lines = (TransactionLine[])convertedInvoice.Lines.Find(amountFilter);
			AssertNotNull("Should be 1 line with value -200 (AP Invoices have InvertSigns=true)", lines[0]);
			AssertEquals(header.PK, lines[0].AL_AG);

			consol = Factory.Load<ForwardingConsol>(consol.PK);
			apps = new ApportionmentListing(Factory, consol);
			AssertEquals("One Consol Cost should exist on Consol", 1, apps.CostsCollection.Count);
			cost = apps.CostsCollection[0];
			AssertEquals("Consol Cost Amount", jobCharge1Amount + jobCharge2Amount + jobCharge3Amount, cost.E6_OSCostAmount);
			AssertEquals("Consol Cost ChargeCode", chargeCode1.PK, cost.E6_AC_ChargeCode);
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestImportConsolInvoiceWithZeroValuedConsolCost()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);

			originalDepartment.GE_Misc = false;
			GlbDepartment department = Factory.Load<GlbDepartment>(originalDepartment.PK);
			department.GE_Misc = false;
			Factory.Save();

			AccChargeCode chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, differentCompany);
			TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			AccChargeCode chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, differentCompany);
			TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = "AIR";
			ForwardingShipment forwardingShipment1 = consol.Shipments.AddNew();
			forwardingShipment1.JS_UniqueConsignRef = "S00002000";
			ForwardingShipment forwardingShipment2 = consol.Shipments.AddNew();
			forwardingShipment2.JS_UniqueConsignRef = "S00002001";
			Factory.Save();

			ZDecimal jobCharge1Amount = 100M;
			ZDecimal invoiceTotalAmount = 500M;
			ZDecimal job1Charge2Amount = 200M;
			ZDecimal job2Charge2Amount = invoiceTotalAmount - job1Charge2Amount;
			InvoicingBase transaction = null;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				OrgHeader agent = factory2.Load<OrgHeader>(TestObjectCreator.Agent.PK);
				agent.CompanyData.OB_IsDebtor = true;
				agent.CompanyData.SetARTaxApplicable(false);
				agent.CompanyData.SetAPTaxApplicable(false);
				agent.CompanyData.OB_RX_NKAPDefltCurrency = "CNY";
				agent.CompanyData.OB_RX_NKARDDefltCurrency = "CNY";

				new TestObjectCreator(factory2).SetCurrentCompanyReciprocal(true);

				consol = factory2.Load<ForwardingConsol>(consol.PK);

				ForwardingShipment shipment1 = consol.Shipments[0];
				ForwardingShipment shipment2 = consol.Shipments[1];

				OrgHeader localClient = factory2.NewWithValidTestData<OrgHeader>();

				Job job1 = createJob(factory2, "J00001000", differentBranch1.PK, originalDepartment.PK, localClient, agent);
				job1.JH_ParentID = shipment1.PK;
				job1.JH_ParentTableCode = "JS";

				Job job2 = createJob(factory2, "J00001001", differentBranch1.PK, originalDepartment.PK, localClient, agent);
				job2.JH_ParentID = shipment2.PK;
				job2.JH_ParentTableCode = "JS";

				factory2.Save();

				Charge job1Charge1 = job1.Charges.AddNew();
				job1Charge1.JR_OH_SellAccount = agent.PK;
				job1Charge1.JR_AC = chargeCode1.PK;
				job1Charge1.JR_GB = differentBranch1.PK;
				job1Charge1.JR_GE = originalDepartment.PK;
				job1Charge1.JR_JH = job1.PK;
				job1Charge1.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				job1Charge1.JR_RX_NKSellCurrency = "CNY";
				job1Charge1.JR_OSCostAmt = jobCharge1Amount;
				job1Charge1.JR_OSSellAmt = -jobCharge1Amount;
				job1Charge1.RevenueExchangeRate?.SetBuyRate_ForTestOnly(0.7m);

				Charge job1Charge2 = job1.Charges.AddNew();
				job1Charge2.JR_OH_SellAccount = agent.PK;
				job1Charge2.JR_AC = chargeCode2.PK;
				job1Charge2.JR_GB = differentBranch1.PK;
				job1Charge2.JR_GE = originalDepartment.PK;
				job1Charge2.JR_JH = job1.PK;
				job1Charge2.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				job1Charge2.JR_RX_NKSellCurrency = "CNY";
				job1Charge2.JR_OSCostAmt = job1Charge2Amount;
				job1Charge2.JR_OSSellAmt = job1Charge2Amount;
				job1Charge2.RevenueExchangeRate?.SetBuyRate_ForTestOnly(0.7m);

				Charge job2Charge1 = job2.Charges.AddNew();
				job2Charge1.JR_OH_SellAccount = agent.PK;
				job2Charge1.JR_AC = chargeCode1.PK;
				job2Charge1.JR_GB = differentBranch1.PK;
				job2Charge1.JR_GE = originalDepartment.PK;
				job2Charge1.JR_JH = job2.PK;
				job2Charge1.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				job2Charge1.JR_RX_NKSellCurrency = "CNY";
				job2Charge1.JR_OSCostAmt = jobCharge1Amount;
				job2Charge1.JR_OSSellAmt = jobCharge1Amount;
				job2Charge1.RevenueExchangeRate?.SetBuyRate_ForTestOnly(0.7m);

				Charge job2Charge2 = job2.Charges.AddNew();
				job2Charge2.JR_OH_SellAccount = agent.PK;
				job2Charge2.JR_AC = chargeCode2.PK;
				job2Charge2.JR_GB = differentBranch1.PK;
				job2Charge2.JR_GE = originalDepartment.PK;
				job2Charge2.JR_JH = job2.PK;
				job2Charge2.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				job2Charge2.JR_RX_NKSellCurrency = "CNY";
				job2Charge2.JR_OSCostAmt = job2Charge2Amount;
				job2Charge2.JR_OSSellAmt = job2Charge2Amount;
				job2Charge2.RevenueExchangeRate?.SetBuyRate_ForTestOnly(0.7m);

				factory2.Save();

				AssertEquals("Job Charge 1 JR_OSSellAmt", -jobCharge1Amount, job1Charge1.JR_OSSellAmt);
				AssertEquals("Job Charge 2 JR_OSSellAmt", job1Charge2Amount, job1Charge2.JR_OSSellAmt);
				AssertEquals("Job Charge 3 JR_OSSellAmt", jobCharge1Amount, job2Charge1.JR_OSSellAmt);
				AssertEquals("Job Charge 4 JR_OSSellAmt", job2Charge2Amount, job2Charge2.JR_OSSellAmt);

				var jobs = new Job[] { job1, job2 };
				ConsolInvoicingPostManager transactionCreator = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(factory2, consol));
				TransactionCreatorHashtable transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);
				factory2.Save();
				AssertEquals("One AR transaction should have been created", 1, transactions.ARTransactionsCount);
				InvoicingBase[] arInvoices = transactions.GetAllARInvoicesAndCreditNotes();
				transaction = arInvoices[0];
				AssertEquals("AR Invoice should have four lines", 4, transaction.Lines.Count);

				AssertEquals("AR Invoice Line 1 Amount", -70.00m, transaction.Lines[0].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 2 Amount", 140.00m, transaction.Lines[1].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 3 Amount", 70.00m, transaction.Lines[2].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 4 Amount", 210.00m, transaction.Lines[3].AL_OSExTaxAmount);

				AssertEquals("AR Invoice Total Amount", 350.00m, transaction.AH_OSExTaxAmount);

				AssertEquals("AR Invoice Line 1 ChargeCode", chargeCode1.PK, transaction.Lines[0].AL_AC);
				AssertEquals("AR Invoice Line 2 ChargeCode", chargeCode2.PK, transaction.Lines[1].AL_AC);
			}

			InvoicingBase convertedInvoice = converter.ConvertToAP(transaction, true);
			AssertNotNull("Converted Invoice should not be null", convertedInvoice);
			Assert("Converted Invoice should be an AP Invoice", convertedInvoice is APInvoice);
			AssertEquals("Converted AP invoice should have four lines as well", 4, convertedInvoice.Lines.Count);
			convertedInvoice.RunPreSaveValidation();
			Assert(string.Format("AP Invoice should not have errors: {0}", convertedInvoice.NotificationsIncludingChildren.ToMessageListString()), !convertedInvoice.HasErrors);
			Assert(!convertedInvoice.NotificationsIncludingChildren.Contains("Local Amount has been set to zero when foreign amount is valid. Please report this error to CargoWise Support"));
			Assert(!convertedInvoice.NotificationsIncludingChildren.Contains("There are errors on related Consol Cost"));
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertToAPWithConsolARInvoice()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);

			originalDepartment.GE_Misc = false;
			GlbDepartment department = Factory.Load<GlbDepartment>(originalDepartment.PK);
			department.GE_Misc = false;
			Factory.Save();

			AccChargeCode chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			AccChargeCode chargeCode2 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, differentCompany);
			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = "AIR";
			ForwardingShipment forwardingShipment1 = consol.Shipments.AddNew();
			forwardingShipment1.JS_UniqueConsignRef = "S00002000";
			ForwardingShipment forwardingShipment2 = consol.Shipments.AddNew();
			forwardingShipment2.JS_UniqueConsignRef = "S00002001";
			Factory.Save();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = chargeCode1.PK;
			cost.E6_ApportionmentMethod = "SHP";
			ZDecimal consolCostAmount = 2000.00m;
			cost.E6_OSCostAmount = consolCostAmount;
			Factory.Save();

			AssertEquals("Consol Cost Amount", consolCostAmount, cost.E6_OSCostAmount);

			ZDecimal jobCharge1Amount = 1.00m;
			ZDecimal jobCharge2Amount = 5.00m;
			InvoicingBase transaction = null;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				OrgHeader agent = factory2.Load<OrgHeader>(TestObjectCreator.Agent.PK);
				agent.CompanyData.OB_IsDebtor = true;
				agent.CompanyData.SetARTaxApplicable(false);
				agent.CompanyData.SetAPTaxApplicable(false);

				consol = factory2.Load<ForwardingConsol>(consol.PK);

				ForwardingShipment shipment1 = consol.Shipments[0];
				ForwardingShipment shipment2 = consol.Shipments[1];

				OrgHeader localClient = factory2.NewWithValidTestData<OrgHeader>();
				Job job1 = createJob(factory2, "J00001000", differentBranch1.PK, originalDepartment.PK, localClient, agent);
				job1.JH_ParentID = shipment1.PK;
				job1.JH_ParentTableCode = "JS";
				Job job2 = createJob(factory2, "J00001001", differentBranch1.PK, originalDepartment.PK, localClient, agent);
				job2.JH_ParentID = shipment2.PK;
				job2.JH_ParentTableCode = "JS";
				factory2.Save();

				Charge jobCharge1 = job1.Charges.AddNew();
				jobCharge1.JR_AC = chargeCode2.PK;
				jobCharge1.JR_GB = differentBranch1.PK;
				jobCharge1.JR_GE = originalDepartment.PK;
				jobCharge1.JR_JH = job1.PK;
				jobCharge1.JR_OSCostAmt = jobCharge1Amount;
				jobCharge1.JR_OSSellAmt = jobCharge1Amount;
				jobCharge1.JR_OH_SellAccount = agent.PK;
				jobCharge1.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;

				Charge jobCharge2 = job2.Charges.AddNew();
				jobCharge2.JR_AC = chargeCode2.PK;
				jobCharge2.JR_GB = differentBranch1.PK;
				jobCharge2.JR_GE = originalDepartment.PK;
				jobCharge2.JR_JH = job2.PK;
				jobCharge2.JR_OSCostAmt = jobCharge2Amount;
				jobCharge2.JR_OSSellAmt = jobCharge2Amount;
				jobCharge2.JR_OH_SellAccount = agent.PK;
				jobCharge2.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
				factory2.Save();

				AssertEquals("Job Charge 1 JR_OSCostAmt", jobCharge1Amount, jobCharge1.JR_OSCostAmt);
				AssertEquals("Job Charge 2 JR_OSCostAmt", jobCharge2Amount, jobCharge2.JR_OSCostAmt);
				AssertEquals("Job Charge 1 JR_OSSellAmt", jobCharge1Amount, jobCharge1.JR_OSSellAmt);
				AssertEquals("Job Charge 2 JR_OSSellAmt", jobCharge2Amount, jobCharge2.JR_OSSellAmt);

				var jobs = new Job[] { job1, job2 };
				ConsolInvoicingPostManager transactionCreator = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(factory2, consol));
				TransactionCreatorHashtable transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);
				AssertEquals("One AR transaction should have been created", 1, transactions.ARTransactionsCount);
				InvoicingBase[] arInvoices = transactions.GetAllARInvoicesAndCreditNotes();
				transaction = arInvoices[0];
				AssertEquals("AR Invoice should have two lines", 2, transaction.Lines.Count);
				AssertEquals("AR Invoice Line 1 Amount", jobCharge1Amount, transaction.Lines[0].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 2 Amount", jobCharge2Amount, transaction.Lines[1].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 1 ChargeCode", chargeCode2.PK, transaction.Lines[0].AL_AC);
				AssertEquals("AR Invoice Line 2 ChargeCode", chargeCode2.PK, transaction.Lines[1].AL_AC);
				factory2.Save();
			}

			InvoicingBase convertedInvoice = converter.ConvertToAP(transaction, false);
			AssertNotNull("Converted Invoice should not be null", convertedInvoice);
			Assert("Converted Invoice should be an AP Invoice", convertedInvoice is APInvoice);
			Assert(string.Format("AP Invoice should not have errors: {0}", convertedInvoice.NotificationsIncludingChildren.ToMessageListString()), !convertedInvoice.HasErrors);
			convertedInvoice.Factory.Save();
			AssertEquals("Line Count", 2, convertedInvoice.Lines.Count);
			AssertEquals("Line 1 Amount", 1.00m, convertedInvoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("Line 2 Amount", 5.00m, convertedInvoice.Lines[1].AL_OSExTaxAmount);
			AssertEquals("Line 1 Charge Code", chargeCode1.PK, convertedInvoice.Lines[0].AL_AC);
			AssertEquals("Line 2 Charge Code", chargeCode1.PK, convertedInvoice.Lines[1].AL_AC);

			consol = Factory.Load<ForwardingConsol>(consol.PK);
			apps = new ApportionmentListing(Factory, consol);
			AssertEquals("One Consol Cost should exist on Consol", 1, apps.CostsCollection.Count);
			cost = apps.CostsCollection[0];
			AssertEquals("Consol Cost Amount", jobCharge1Amount + jobCharge2Amount, cost.E6_OSCostAmount);
			AssertEquals("Consol Cost ChargeCode", chargeCode1.PK, cost.E6_AC_ChargeCode);

			transaction.Lines[1].AL_JH = ZGuid.Empty;
			transaction.Factory.Save();
			convertedInvoice = converter.ConvertToAP(transaction, false);
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertToAPWithJobNumber()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			AccChargeCode chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			AccChargeCode chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);

			var creditor = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.GB_OH_OrgProxy);
			creditor.CompanyData.OB_IsCreditor = true;
			creditor.CompanyData.SetAPTaxApplicable(true);

			TestObjectCreator.AALSHI.OH_IsDebtor = true;
			TestObjectCreator.AALSHI.OH_IsCreditor = true;
			TestObjectCreator.ZECTRA.OH_IsDebtor = true;
			TestObjectCreator.ZECTRA.OH_IsCreditor = true;
			TestObjectCreator.ZECTRA.CompanyData.OB_APVATConfig = "DEF";

			var consol = TestObjectCreator.CreateConsol(TestObjectCreator.AALSHI.OH_RL_NKClosestPort, TestObjectCreator.ZECTRA.OH_RL_NKClosestPort, "C000001");

			TestObjectCreator.AALSHI.OH_IsForwarder = true;
			TestObjectCreator.ZECTRA.OH_IsForwarder = true;
			consol.JK_OA_SendingForwarderAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = TestObjectCreator.ZECTRA.MainAddress.PK;

			ForwardingShipment forwardingShipment = consol.Shipments.AddNew();
			forwardingShipment.JS_UniqueConsignRef = "S00001000";
			forwardingShipment.JS_ReleaseType = "SWB";
			forwardingShipment.JS_TransportMode = "AIR";
			forwardingShipment.JS_PackingMode = "LSE";

			var job = TestObjectCreator.CreateJob(forwardingShipment);
			job.JH_JobNum = "J00001001";
			job.AgentCollectPK = TestObjectCreator.ZECTRA.PK;

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost1 = apps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = chargeCode1.PK;
			cost1.E6_ApportionmentMethod = "SHP";
			cost1.E6_OSCostAmount = 100.00m;
			cost1.E6_LocalCostAmount = 100.00m;
			cost1.E6_IsForCollectInvoice = true;
			cost1.E6_InvoiceNum = "Test1";
			cost1.E6_PPDCLT = "ALL";
			cost1.E6_ApportionToRelatedShipments = true;
			cost1.E6_InvoiceDate = DateTime.Today;
			cost1.E6_PaymentDate = DateTime.Today;
			cost1.E6_OH_Creditor = TestObjectCreator.ZECTRA.PK;

			JobConsolCost cost2 = apps.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = chargeCode2.PK;
			cost2.E6_ApportionmentMethod = "SHP";
			cost2.E6_OSCostAmount = 200.00m;
			cost2.E6_LocalCostAmount = 200.00m;
			cost2.E6_IsForCollectInvoice = true;
			cost2.E6_InvoiceNum = "Test1";
			cost2.E6_PPDCLT = "ALL";
			cost2.E6_ApportionToRelatedShipments = true;
			cost2.E6_InvoiceDate = DateTime.Today;
			cost2.E6_PaymentDate = DateTime.Today;
			cost2.E6_OH_Creditor = TestObjectCreator.ZECTRA.PK;

			Factory.Save();

			Charge jobCharge1 = job.Charges[0];
			jobCharge1.JR_AC = chargeCode1.PK;
			jobCharge1.JR_OSSellAmt = 0m;
			jobCharge1.JR_LocalSellAmt = 0m;
			jobCharge1.JR_OH_SellAccount = TestObjectCreator.ZECTRA.PK;

			Charge jobCharge2 = job.Charges[1];
			jobCharge2.JR_AC = chargeCode2.PK;
			jobCharge2.JR_OSSellAmt = 0m;
			jobCharge2.JR_LocalSellAmt = 0m;
			jobCharge2.JR_OH_SellAccount = TestObjectCreator.ZECTRA.PK;

			Factory.Save();

			var jobs = new Job[] { job };
			ConsolInvoicingPostManager transactionCreator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);

			Factory.Save();

			var arTransactions = Factory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, "AR").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			var arTransaction = arTransactions[0];

			int invoiceLinesListChangedHitCount = 0;
			Action<InvoicingBase> sourceTransactionInNewFactoryAction = (InvoicingBase transaction) =>
			{
				var listChangedHandler = new ListChangedEventHandler(
					(sender, e) =>
					{ invoiceLinesListChangedHitCount++; }
				);
				((IBindingList)transaction.Lines).ListChanged += listChangedHandler;
			};

			var converter = new UnapprovedTransactionConverterExposed(Factory, sourceTransactionInNewFactoryAction);
			InvoicingBase convertedInvoice = converter.ConvertToAP(arTransaction, false);

			AssertEquals("ListChanged on arTransaction.Lines should be called once", 1, invoiceLinesListChangedHitCount);
			AssertNotNull(convertedInvoice);
			AssertEquals(2, convertedInvoice.Lines.Count);
			AssertEquals(job.JH_JobNum, convertedInvoice.Lines[0].JobNumber);
			AssertEquals(job.JH_JobNum, convertedInvoice.Lines[1].JobNumber);
			var charges = job.Charges.Select(c => c.JR_AC);
			AssertCollectionContains(convertedInvoice.Lines[0].GenericCharge, charges);
			AssertCollectionContains(convertedInvoice.Lines[1].GenericCharge, charges);
			var exTaxAmounts = convertedInvoice.Lines.ToArray<InvoicingLineBase>().Select(c => c.AL_LocalExTaxAmount);
			AssertCollectionContains(-100M, exTaxAmounts);
			AssertCollectionContains(-200M, exTaxAmounts);

			var gstAmounts = convertedInvoice.Lines.ToArray<InvoicingLineBase>().Select(c => c.AL_LocalGSTAmount);
			AssertCollectionContains(-10M, gstAmounts);
			AssertCollectionContains(-20M, gstAmounts);

			var totalAmounts = convertedInvoice.Lines.ToArray<InvoicingLineBase>().Select(c => c.AL_LocalTotalAmount);
			AssertCollectionContains(-110M, totalAmounts);
			AssertCollectionContains(-220M, totalAmounts);
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertToAPWithJobNumber_WithoutMultipleAmount()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);

			AccChargeCode chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.FREECAPGST, TestObjectCreator.WHTFREE1);
			AccChargeCode chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.FREECAPGST, TestObjectCreator.WHTFREE1);

			OrgHeader agent = Factory.Load<OrgHeader>(TestObjectCreator.Agent.PK);
			agent.CompanyData.OB_IsDebtor = true;
			agent.CompanyData.OB_IsCreditor = true;
			agent.CompanyData.SetARTaxApplicable(false);
			agent.CompanyData.SetAPTaxApplicable(false);

			TestObjectCreator.AALSHI.OH_IsDebtor = true;
			TestObjectCreator.AALSHI.OH_IsCreditor = true;
			TestObjectCreator.ZECTRA.OH_IsDebtor = true;
			TestObjectCreator.ZECTRA.OH_IsCreditor = true;

			var consol = TestObjectCreator.CreateConsol(TestObjectCreator.AALSHI.OH_RL_NKClosestPort, TestObjectCreator.ZECTRA.OH_RL_NKClosestPort, "C000001");

			TestObjectCreator.AALSHI.OH_IsForwarder = true;
			TestObjectCreator.ZECTRA.OH_IsForwarder = true;
			consol.JK_OA_SendingForwarderAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = TestObjectCreator.ZECTRA.MainAddress.PK;

			ForwardingShipment forwardingShipment = consol.Shipments.AddNew();
			forwardingShipment.JS_UniqueConsignRef = "S00001000";
			forwardingShipment.JS_ReleaseType = "SWB";
			forwardingShipment.JS_TransportMode = "AIR";
			forwardingShipment.JS_PackingMode = "LSE";

			var job = TestObjectCreator.CreateJob(forwardingShipment);
			job.AgentCollectPK = TestObjectCreator.ZECTRA.PK;

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost1 = apps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = chargeCode1.PK;
			cost1.E6_ApportionmentMethod = "SHP";
			cost1.E6_OSCostAmount = 100.00m;
			cost1.E6_LocalCostAmount = 100.00m;
			cost1.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			cost1.E6_IsForCollectInvoice = true;
			cost1.E6_InvoiceNum = "Test1";
			cost1.E6_PPDCLT = "ALL";
			cost1.E6_ApportionToRelatedShipments = true;
			cost1.E6_InvoiceDate = DateTime.Today;
			cost1.E6_PaymentDate = DateTime.Today;
			cost1.E6_OH_Creditor = TestObjectCreator.ZECTRA.PK;

			JobConsolCost cost2 = apps.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = chargeCode2.PK;
			cost2.E6_ApportionmentMethod = "SHP";
			cost2.E6_OSCostAmount = 200.00m;
			cost2.E6_LocalCostAmount = 200.00m;
			cost2.E6_RX_NKCurrency = TestObjectCreator.AUD.RX_Code;
			cost2.E6_IsForCollectInvoice = true;
			cost2.E6_InvoiceNum = "Test1";
			cost2.E6_PPDCLT = "ALL";
			cost2.E6_ApportionToRelatedShipments = true;
			cost2.E6_InvoiceDate = DateTime.Today;
			cost2.E6_PaymentDate = DateTime.Today;
			cost2.E6_OH_Creditor = TestObjectCreator.ZECTRA.PK;

			Factory.Save();

			Charge jobCharge1 = job.Charges[0];
			jobCharge1.JR_OSSellAmt = 0m;
			jobCharge1.JR_LocalSellAmt = 0m;

			Charge jobCharge2 = job.Charges[1];
			jobCharge2.JR_OSSellAmt = 0m;
			jobCharge2.JR_LocalSellAmt = 0m;

			Factory.Save();

			var jobs = new Job[] { job };
			ConsolInvoicingPostManager transactionCreator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);

			Factory.Save();

			var arTransactions = Factory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, "AR").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			var arTransaction = arTransactions[0];

			InvoicingBase convertedInvoice = converter.ConvertToAP(arTransaction, false);

			AssertNotNull(convertedInvoice);
			AssertEquals(2, convertedInvoice.Lines.Count);
			var line1 = convertedInvoice.Lines.Cast<InvoicingLineBase>().FirstOrDefault(x => x.AL_LocalExTaxAmount == -100M);
			AssertNotNull("We shouldn't get a multiple local amount", line1);
			AssertEquals("We shouldn't get a multiple os amount", -100m, line1.AL_OSAmount);
			var line2 = convertedInvoice.Lines.Cast<InvoicingLineBase>().FirstOrDefault(x => x.AL_LocalExTaxAmount == -200M);
			AssertNotNull("We shouldn't get a multiple local amount", line2);
			AssertEquals("We shouldn't get a multiple os amount", -200m, line2.AL_OSAmount);
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertToAPWithForeignCurrency()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);

			AccChargeCode chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.FREECAPGST, TestObjectCreator.WHTFREE1);
			AccChargeCode chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.FREECAPGST, TestObjectCreator.WHTFREE1);

			OrgHeader agent = Factory.Load<OrgHeader>(TestObjectCreator.Agent.PK);
			agent.CompanyData.OB_IsDebtor = true;
			agent.CompanyData.OB_IsCreditor = true;
			agent.CompanyData.SetARTaxApplicable(false);
			agent.CompanyData.SetAPTaxApplicable(false);

			TestObjectCreator.AALSHI.OH_IsDebtor = true;
			TestObjectCreator.AALSHI.OH_IsCreditor = true;
			TestObjectCreator.ZECTRA.OH_IsDebtor = true;
			TestObjectCreator.ZECTRA.OH_IsCreditor = true;

			TestObjectCreator.SetCurrentCompanyReciprocal(false);

			var consol = TestObjectCreator.CreateConsol(TestObjectCreator.AALSHI.OH_RL_NKClosestPort, TestObjectCreator.ZECTRA.OH_RL_NKClosestPort, "C000001");

			TestObjectCreator.AALSHI.OH_IsForwarder = true;
			TestObjectCreator.ZECTRA.OH_IsForwarder = true;
			consol.JK_OA_SendingForwarderAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = TestObjectCreator.ZECTRA.MainAddress.PK;

			ForwardingShipment forwardingShipment = consol.Shipments.AddNew();
			forwardingShipment.JS_UniqueConsignRef = "S00001000";
			forwardingShipment.JS_ReleaseType = "SWB";
			forwardingShipment.JS_TransportMode = "AIR";
			forwardingShipment.JS_PackingMode = "LSE";

			var job = TestObjectCreator.CreateJob(forwardingShipment);
			job.AgentCollectPK = TestObjectCreator.ZECTRA.PK;

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = chargeCode1.PK;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 100.00m;
			cost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			cost.E6_ExchangeRate = 5.00m;
			cost.E6_LocalCostAmount = 500.00m;
			cost.E6_IsForCollectInvoice = true;
			cost.E6_InvoiceNum = "Test1";
			cost.E6_PPDCLT = "ALL";
			cost.E6_ApportionToRelatedShipments = true;
			cost.E6_InvoiceDate = DateTime.Today;
			cost.E6_PaymentDate = DateTime.Today;
			cost.E6_OH_Creditor = TestObjectCreator.ZECTRA.PK;

			Factory.Save();

			Charge jobCharge = job.Charges[0];
			jobCharge.JR_OSSellAmt = 0m;
			jobCharge.JR_LocalSellAmt = 0m;
			jobCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

			Factory.Save();

			var jobs = new Job[] { job };
			ConsolInvoicingPostManager transactionCreator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);

			Factory.Save();

			var arTransactions = Factory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, "AR").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			var arTransaction = arTransactions[0];

			InvoicingBase convertedInvoice = converter.ConvertToAP(arTransaction, false);

			AssertNotNull(convertedInvoice);
			AssertEquals(1, convertedInvoice.Lines.Count);
			AssertEquals(-100.00m, convertedInvoice.Lines[0].AL_OSExTaxAmount);
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		[DisableZeroExchangeRateOverriding]
		public void TestConvertToAPWithMultiForeignCurrency()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.FREECAPGST, TestObjectCreator.WHTFREE1);
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.FREECAPGST, TestObjectCreator.WHTFREE1);

			var agent = TestObjectCreator.Agent;
			agent.CompanyData.OB_IsDebtor = true;
			agent.CompanyData.OB_IsCreditor = true;
			agent.CompanyData.SetARTaxApplicable(false);
			agent.CompanyData.SetAPTaxApplicable(false);

			TestObjectCreator.AALSHI.OH_IsDebtor = true;
			TestObjectCreator.AALSHI.OH_IsCreditor = true;
			TestObjectCreator.ZECTRA.OH_IsDebtor = true;
			TestObjectCreator.ZECTRA.OH_IsCreditor = true;

			GlbCompany.CurrentCompany.GC_IsReciprocal = false;
			differentBranch1.Company.GC_IsReciprocal = true;

			var consol = TestObjectCreator.CreateConsol(TestObjectCreator.AALSHI.OH_RL_NKClosestPort, TestObjectCreator.ZECTRA.OH_RL_NKClosestPort, "C000001");

			TestObjectCreator.AALSHI.OH_IsForwarder = true;
			TestObjectCreator.ZECTRA.OH_IsForwarder = true;
			consol.JK_OA_SendingForwarderAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = TestObjectCreator.ZECTRA.MainAddress.PK;

			var forwardingShipment = consol.Shipments.AddNew();
			forwardingShipment.JS_UniqueConsignRef = "S00001000";
			forwardingShipment.JS_ReleaseType = "SWB";
			forwardingShipment.JS_TransportMode = "AIR";
			forwardingShipment.JS_PackingMode = "LSE";

			var job = TestObjectCreator.CreateJob(forwardingShipment);
			job.AgentCollectPK = TestObjectCreator.ZECTRA.PK;

			var apps = new ApportionmentListing(Factory, consol);
			var cost1 = apps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = chargeCode1.PK;
			cost1.E6_ApportionmentMethod = "SHP";
			cost1.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			cost1.E6_IsForCollectInvoice = true;
			cost1.E6_InvoiceNum = "Test1";
			cost1.E6_PPDCLT = "ALL";
			cost1.E6_ApportionToRelatedShipments = true;
			cost1.E6_InvoiceDate = DateTime.Today;
			cost1.E6_PaymentDate = DateTime.Today;
			cost1.E6_OH_Creditor = TestObjectCreator.ZECTRA.PK;
			cost1.E6_ExchangeRate = 5.00m;
			cost1.E6_OSCostAmount = 100.00m;
			cost1.E6_LocalCostAmount = 500.00m;

			var cost2 = apps.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = chargeCode2.PK;
			cost2.E6_ApportionmentMethod = "SHP";
			cost2.E6_RX_NKCurrency = TestObjectCreator.GBP.RX_Code;
			cost2.E6_IsForCollectInvoice = true;
			cost2.E6_InvoiceNum = "Test2";
			cost2.E6_PPDCLT = "ALL";
			cost2.E6_ApportionToRelatedShipments = true;
			cost2.E6_InvoiceDate = DateTime.Today;
			cost2.E6_PaymentDate = DateTime.Today;
			cost2.E6_OH_Creditor = TestObjectCreator.ZECTRA.PK;
			cost2.E6_ExchangeRate = 4.00m;
			cost2.E6_OSCostAmount = 200.00m;
			cost2.E6_LocalCostAmount = 800.00m;
			Factory.Save();

			var jobCharge1 = job.Charges[0];
			jobCharge1.JR_OSSellAmt = 0m;
			jobCharge1.JR_LocalSellAmt = 0m;
			jobCharge1.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

			var jobCharge2 = job.Charges[1];
			jobCharge2.JR_OSSellAmt = 0m;
			jobCharge2.JR_LocalSellAmt = 0m;
			jobCharge2.JR_RX_NKSellCurrency = TestObjectCreator.GBP.RX_Code;

			Factory.Save();

			var jobs = new Job[] { job };
			var transactionCreator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);

			Factory.Save();

			var arTransactions = Factory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, "AR").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			var arTransaction = arTransactions[0];
			AssertNotNull(arTransaction);
			AssertEquals(260M, arTransaction.AH_OSTotalAmount);
			AssertEquals(2, arTransaction.Lines.Count);

			var osAmounts = arTransaction.Lines.ToArray<InvoicingLineBase>().Select(c => c.AL_OSExTaxAmount);
			AssertCollectionContains(100M, osAmounts);
			AssertCollectionContains(160M, osAmounts);

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				var factory2 = new BusinessObjectFactory();
				var converter = new UnapprovedTransactionConverter(factory2);

				var convertedInvoice = converter.ConvertToAP(arTransaction, false);

				AssertNotNull(convertedInvoice);
				AssertEquals(260M, convertedInvoice.AH_OSTotalAmount);
				AssertEquals(1, convertedInvoice.Lines.Count);
				AssertEquals(260m, convertedInvoice.Lines[0].AL_OSExTaxAmount);
				AssertNotNull(convertedInvoice.Lines[0].Job);
				convertedInvoice.Lines[0].Job.Dispose();
			}
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertToAPFromConsolARInvoiceWithoutConsolCost_SameCountry()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);

			originalDepartment.GE_Misc = false;
			GlbDepartment department = Factory.Load<GlbDepartment>(originalDepartment.PK);
			department.GE_Misc = false;
			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = true;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(true);

			differentCompany.SetCountry(localCompany.GC_RN_NKCountryCode);
			differentCompanyOrgProxy.OH_RL_NKClosestPort = "AUMEL";
			differentBranchOrgProxy.OH_RL_NKClosestPort = "AUMEL";
			Factory.Save();

			AccChargeCode chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			AccChargeCode chargeCode2 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, differentCompany);
			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			AssertEquals("Companies are located in the same country", localCompany.GC_RN_NKCountryCode, differentCompany.GC_RN_NKCountryCode);

			ZDecimal jobChargeExTaxAmount = 500.00m;
			ZDecimal jobChargeTaxAmount = 50.00m;

			Factory.Save();

			ForwardingShipment shipment1 = null;
			ForwardingShipment shipment2 = null;
			ForwardingShipment shipment3 = null;
			ForwardingConsol consol = null;
			InvoicingBase transaction = null;

			using (new TemporaryUserContext() { DepartmentPK = originalDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				var creator2 = new TestObjectCreator(factory2);

				OrgHeader agent = factory2.Load<OrgHeader>(TestObjectCreator.Agent.PK);
				agent.CompanyData.OB_IsDebtor = true;
				agent.CompanyData.SetARTaxApplicable(true);

				consol = creator2.CreateConsol("AUSYD", "HKHKG", "C00001000");

				shipment1 = creator2.CreateShipment("S00002000", consol);
				shipment2 = creator2.CreateShipment("S00002001", consol);
				shipment3 = creator2.CreateShipment("S00002002", consol);

				OrgHeader localClient = creator2.LocalClient;
				Job job1 = creator2.CreateJob(shipment1, localClient, 0m, agent, 0m);
				job1.JH_GB = differentBranch1.PK;
				job1.JH_GE = originalDepartment.PK;
				Job job2 = creator2.CreateJob(shipment2, localClient, 0m, agent, 0m);
				job2.JH_GB = differentBranch1.PK;
				job2.JH_GE = originalDepartment.PK;
				factory2.Save();

				Charge jobCharge1 = creator2.CreateCharge(job1, chargeCode2, jobChargeExTaxAmount, jobChargeExTaxAmount);
				jobCharge1.JR_OH_SellAccount = agent.PK;
				jobCharge1.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				jobCharge1.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;

				Charge jobCharge2 = creator2.CreateCharge(job2, chargeCode2, jobChargeExTaxAmount, jobChargeExTaxAmount);
				jobCharge2.JR_OH_SellAccount = agent.PK;
				jobCharge2.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				jobCharge2.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
				factory2.Save();

				AssertEquals("Job Charge 1 JR_OSCostAmt", jobChargeExTaxAmount, jobCharge1.JR_OSCostAmt);
				AssertEquals("Job Charge 1 JR_OSSellAmt", jobChargeExTaxAmount, jobCharge1.JR_OSSellAmt);
				AssertEquals("Job Charge 1 JR_AT_SellGSTRate", TestObjectCreator.GST1.PK, jobCharge1.JR_AT_SellGSTRate);
				AssertEquals("Job Charge 1 JR_OSSellGSTAmt", jobChargeTaxAmount, jobCharge1.JR_OSSellGSTAmt_Calc);

				AssertEquals("Job Charge 2 JR_OSCostAmt", jobChargeExTaxAmount, jobCharge2.JR_OSCostAmt);
				AssertEquals("Job Charge 2 JR_OSSellAmt", jobChargeExTaxAmount, jobCharge2.JR_OSSellAmt);
				AssertEquals("Job Charge 2 JR_AT_SellGSTRate", TestObjectCreator.GST1.PK, jobCharge2.JR_AT_SellGSTRate);
				AssertEquals("Job Charge 2 JR_OSSellGSTAmt", jobChargeTaxAmount, jobCharge2.JR_OSSellGSTAmt_Calc);

				ConsolInvoicingPostManager transactionCreator = new ConsolInvoicingPostManager(factory2, new Job[] { job1, job2 }, consol, new ApportionmentListing(factory2, consol));
				TransactionCreatorHashtable transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);
				AssertEquals("One AR transaction should have been created", 1, transactions.ARTransactionsCount);
				InvoicingBase[] arInvoices = transactions.GetAllARInvoicesAndCreditNotes();
				transaction = arInvoices[0];
				AssertEquals("AR Invoice should have two lines", 2, transaction.Lines.Count);

				AssertEquals("AR Invoice Line 1 ChargeCode", chargeCode2.PK, transaction.Lines[0].AL_AC);
				AssertEquals("AR Invoice Line 1 Amount", jobChargeExTaxAmount, transaction.Lines[0].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 1 Tax Rate", TestObjectCreator.GST1.PK, transaction.Lines[0].AL_AT);
				AssertEquals("AR Invoice Line 1 Tax Amount", jobChargeTaxAmount, transaction.Lines[0].AL_OSGSTAmount);

				AssertEquals("AR Invoice Line 2 ChargeCode", chargeCode2.PK, transaction.Lines[1].AL_AC);
				AssertEquals("AR Invoice Line 2 Amount", jobChargeExTaxAmount, transaction.Lines[1].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 1 Tax Rate", TestObjectCreator.GST1.PK, transaction.Lines[1].AL_AT);
				AssertEquals("AR Invoice Line 2 Tax Amount", jobChargeTaxAmount, transaction.Lines[1].AL_OSGSTAmount);
				factory2.Save();
			}

			InvoicingBase convertedInvoice = converter.ConvertToAP(transaction, false);

			try
			{
				AssertNotNull("Converted Invoice should not be null", convertedInvoice);
				Assert("Converted Invoice should be an AP Invoice", convertedInvoice is APInvoice);
				Assert(string.Format("AP Invoice should not have errors: {0}", convertedInvoice.NotificationsIncludingChildren.ToMessageListString()), !convertedInvoice.HasErrors);
				convertedInvoice.Factory.Save();
				AssertEquals("Line Count", 2, convertedInvoice.Lines.Count);

				// Tax ID and Tax Ampount should be preserved when moving Invoice inside of Country
				AssertEquals("Line 1 Charge Code", chargeCode1.PK, convertedInvoice.Lines[0].AL_AC);
				AssertEquals("Line 1 Amount", jobChargeExTaxAmount, convertedInvoice.Lines[0].AL_OSExTaxAmount);
				AssertEquals("Line 1 Tax Rate", TestObjectCreator.GST1.PK, convertedInvoice.Lines[0].AL_AT);
				AssertEquals("Line 1 Tax Amount", jobChargeTaxAmount, convertedInvoice.Lines[0].AL_OSGSTAmount);

				AssertEquals("Line 2 Charge Code", chargeCode1.PK, convertedInvoice.Lines[1].AL_AC);
				AssertEquals("Line 2 Amount", jobChargeExTaxAmount, convertedInvoice.Lines[1].AL_OSExTaxAmount);
				AssertEquals("Line 2 Tax Rate", TestObjectCreator.GST1.PK, convertedInvoice.Lines[1].AL_AT);
				AssertEquals("Line 2 Tax Amount", jobChargeTaxAmount, convertedInvoice.Lines[1].AL_OSGSTAmount);

				using (Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex())
				{
					Assert("Job1 should be saved", job1 != null && job1.IsInDatabase);
				}

				using (Job job2 = new Job.Loader(Factory, shipment2).TryLoadOrCreateWithMutex())
				{
					Assert("Job2 should be saved", job2 != null && job2.IsInDatabase);
				}

				using (Job job3 = new Job.Loader(Factory, shipment3).TryLoadOrCreateWithMutex())
				{
					AssertNull("Job3 should be created by CosolCost OnSaved and locked with Mutex", job3);
				}
			}
			finally
			{
				convertedInvoice.ReleaseAllMutexOnInvoice();
			}
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertToAPFromConsolARInvoiceWithoutConsolCost_SameCountryMoreThanOneTaxPerChargeCode()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);

			originalDepartment.GE_Misc = false;
			GlbDepartment department = Factory.Load<GlbDepartment>(originalDepartment.PK);
			department.GE_Misc = false;
			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = true;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(true);

			differentCompany.SetCountry(localCompany.GC_RN_NKCountryCode);
			differentCompanyOrgProxy.OH_RL_NKClosestPort = "AUMEL";
			differentBranchOrgProxy.OH_RL_NKClosestPort = "AUMEL";
			Factory.Save();

			AccChargeCode chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			AccChargeCode chargeCode2 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, differentCompany);
			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			AssertEquals("Companies are located in the same country", localCompany.GC_RN_NKCountryCode, differentCompany.GC_RN_NKCountryCode);

			ZDecimal jobChargeExTaxAmount = 500.00m;
			ZDecimal jobChargeTaxAmount = 50.00m;

			Factory.Save();

			InvoicingBase transaction = null;

			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "HKHKG", "C00001000");
			ForwardingShipment shipment1 = TestObjectCreator.CreateShipment("S00002000", consol);
			ForwardingShipment shipment2 = TestObjectCreator.CreateShipment("S00002001", consol);

			var apportionment = new ApportionmentListing(Factory, consol);
			var existingCost = apportionment.CostsCollection.TryAddNew();
			existingCost.E6_AC_ChargeCode = chargeCode1.PK;
			existingCost.E6_OSCostAmount = 450m;
			var charges = existingCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToArray();
			charges[0].JR_OSCostAmt = 450M;
			charges[1].JR_OSCostAmt = 0M;

			Factory.Save();

			using (new TemporaryUserContext() { DepartmentPK = originalDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				var creator2 = new TestObjectCreator(factory2);

				OrgHeader agent = factory2.Load<OrgHeader>(TestObjectCreator.Agent.PK);
				agent.CompanyData.OB_IsDebtor = true;
				agent.CompanyData.SetARTaxApplicable(true);

				consol = factory2.Load<ForwardingConsol>(consol.PK);
				shipment1 = factory2.Load<ForwardingShipment>(shipment1.PK);
				shipment2 = factory2.Load<ForwardingShipment>(shipment2.PK);

				OrgHeader localClient = creator2.LocalClient;
				Job job1 = creator2.CreateJob(shipment1, localClient, 0m, agent, 0m);
				job1.JH_GB = differentBranch1.PK;
				job1.JH_GE = originalDepartment.PK;
				Job job2 = creator2.CreateJob(shipment2, localClient, 0m, agent, 0m);
				job2.JH_GB = differentBranch1.PK;
				job2.JH_GE = originalDepartment.PK;
				factory2.Save();

				Charge jobCharge1 = creator2.CreateCharge(job1, chargeCode2, jobChargeExTaxAmount, jobChargeExTaxAmount);
				jobCharge1.JR_OH_SellAccount = agent.PK;
				jobCharge1.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				jobCharge1.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;

				Charge jobCharge2 = creator2.CreateCharge(job2, chargeCode2, jobChargeExTaxAmount, jobChargeExTaxAmount);
				jobCharge2.JR_OH_SellAccount = agent.PK;
				jobCharge2.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
				jobCharge2.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
				factory2.Save();

				AssertEquals("Job Charge 1 JR_OSCostAmt", jobChargeExTaxAmount, jobCharge1.JR_OSCostAmt);
				AssertEquals("Job Charge 1 JR_OSSellAmt", jobChargeExTaxAmount, jobCharge1.JR_OSSellAmt);
				AssertEquals("Job Charge 1 JR_AT_SellGSTRate", TestObjectCreator.GST1.PK, jobCharge1.JR_AT_SellGSTRate);
				AssertEquals("Job Charge 1 JR_OSSellGSTAmt", jobChargeTaxAmount, jobCharge1.JR_OSSellGSTAmt_Calc);

				AssertEquals("Job Charge 2 JR_OSCostAmt", jobChargeExTaxAmount, jobCharge2.JR_OSCostAmt);
				AssertEquals("Job Charge 2 JR_OSSellAmt", jobChargeExTaxAmount, jobCharge2.JR_OSSellAmt);
				AssertEquals("Job Charge 2 JR_AT_SellGSTRate", TestObjectCreator.GSTFREE1.PK, jobCharge2.JR_AT_SellGSTRate);
				AssertEquals("Job Charge 2 JR_OSSellGSTAmt", 0m, jobCharge2.JR_OSSellGSTAmt_Calc);

				ConsolInvoicingPostManager transactionCreator = new ConsolInvoicingPostManager(factory2, new Job[] { job1, job2 }, consol, new ApportionmentListing(factory2, consol));
				TransactionCreatorHashtable transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);
				AssertEquals("One AR transaction should have been created", 1, transactions.ARTransactionsCount);
				InvoicingBase[] arInvoices = transactions.GetAllARInvoicesAndCreditNotes();
				transaction = arInvoices[0];
				AssertEquals("AR Invoice should have two lines", 2, transaction.Lines.Count);

				AssertEquals("AR Invoice Line 1 ChargeCode", chargeCode2.PK, transaction.Lines[0].AL_AC);
				AssertEquals("AR Invoice Line 1 Amount", jobChargeExTaxAmount, transaction.Lines[0].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 1 Tax Rate", TestObjectCreator.GST1.PK, transaction.Lines[0].AL_AT);
				AssertEquals("AR Invoice Line 1 Tax Amount", jobChargeTaxAmount, transaction.Lines[0].AL_OSGSTAmount);

				AssertEquals("AR Invoice Line 2 ChargeCode", chargeCode2.PK, transaction.Lines[1].AL_AC);
				AssertEquals("AR Invoice Line 2 Amount", jobChargeExTaxAmount, transaction.Lines[1].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 1 Tax Rate", TestObjectCreator.GSTFREE1.PK, transaction.Lines[1].AL_AT);
				AssertEquals("AR Invoice Line 2 Tax Amount", 0m, transaction.Lines[1].AL_OSGSTAmount);
				factory2.Save();
			}

			InvoicingBase convertedInvoice = converter.ConvertToAP(transaction, false, true); //Revalidate Lines

			try
			{
				AssertNotNull("Converted Invoice should not be null", convertedInvoice);
				Assert("Converted Invoice should be an AP Invoice", convertedInvoice is APInvoice);
				Assert(string.Format("AP Invoice should not have errors: {0}", convertedInvoice.NotificationsIncludingChildren.ToMessageListString()), !convertedInvoice.HasErrors);
				convertedInvoice.Factory.Save();
				AssertEquals("Line Count", 2, convertedInvoice.Lines.Count);

				// Tax ID and Tax Amount should be preserved when moving Invoice inside of Country
				AssertEquals("Line 1 Charge Code", chargeCode1.PK, convertedInvoice.Lines[0].AL_AC);
				AssertEquals("Line 1 Amount", jobChargeExTaxAmount, convertedInvoice.Lines[0].AL_OSExTaxAmount);
				AssertEquals("Line 1 Tax Rate", TestObjectCreator.GST1.PK, convertedInvoice.Lines[0].AL_AT);
				AssertEquals("Line 1 Tax Amount", jobChargeTaxAmount, convertedInvoice.Lines[0].AL_OSGSTAmount);

				AssertEquals("Line 2 Charge Code", chargeCode1.PK, convertedInvoice.Lines[1].AL_AC);
				AssertEquals("Line 2 Amount", jobChargeExTaxAmount, convertedInvoice.Lines[1].AL_OSExTaxAmount);
				AssertEquals("Line 2 Tax Rate", TestObjectCreator.GSTFREE1.PK, convertedInvoice.Lines[1].AL_AT);
				AssertEquals("Line 2 Tax Amount", 0m, convertedInvoice.Lines[1].AL_OSGSTAmount);

				AssertEquals("Two Consol Costs were created by converting Invoice", 2, convertedInvoice.ConsolCosting.ConsolCosts.Count);
				AssertNotNull("exiting Consol Cost was reused", convertedInvoice.ConsolCosting.ConsolCosts.FindByPK(existingCost.PK));
			}
			finally
			{
				convertedInvoice.ReleaseAllMutexOnInvoice();
			}
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertToAPFromConsolARInvoiceWithoutConsolCost_DifferentCountries()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);

			originalDepartment.GE_Misc = false;
			GlbDepartment department = Factory.Load<GlbDepartment>(originalDepartment.PK);
			department.GE_Misc = false;
			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = true;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(true);
			Factory.Save();

			AccChargeCode chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			AccChargeCode chargeCode2 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, differentCompany);
			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			AssertNotEquals("Companies are located in two different countries", localCompany.GC_RN_NKCountryCode, differentCompany.GC_RN_NKCountryCode);

			ZDecimal jobChargeExTaxAmount = 500.00m;
			ZDecimal jobChargeTaxAmount = 50.00m;

			Factory.Save();

			ForwardingShipment shipment1 = null;
			ForwardingShipment shipment2 = null;
			ForwardingShipment shipment3 = null;
			ForwardingConsol consol = null;
			InvoicingBase transaction = null;

			using (new TemporaryUserContext() { DepartmentPK = originalDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();

				OrgHeader agent = factory2.Load<OrgHeader>(TestObjectCreator.Agent.PK);
				agent.CompanyData.OB_IsDebtor = true;
				agent.CompanyData.SetARTaxApplicable(true);

				consol = factory2.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "SGSIN";
				consol.JK_UniqueConsignRef = "C00001000";
				consol.JK_TransportMode = "AIR";

				shipment1 = consol.Shipments.AddNew();
				shipment1.JS_UniqueConsignRef = "S00002000";
				shipment2 = consol.Shipments.AddNew();
				shipment2.JS_UniqueConsignRef = "S00002001";
				shipment3 = consol.Shipments.AddNew();
				shipment3.JS_UniqueConsignRef = "S00002002";

				OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
				Job job1 = createJob(factory2, "J00001000", differentBranch1.PK, originalDepartment.PK, localClient, agent);
				job1.JH_ParentID = shipment1.PK;
				job1.JH_ParentTableCode = "JS";
				Job job2 = createJob(factory2, "J00001001", differentBranch1.PK, originalDepartment.PK, localClient, agent);
				job2.JH_ParentID = shipment2.PK;
				job2.JH_ParentTableCode = "JS";
				factory2.Save();

				Charge jobCharge1 = job1.Charges.AddNew();
				jobCharge1.JR_AC = chargeCode2.PK;
				jobCharge1.JR_GB = differentBranch1.PK;
				jobCharge1.JR_GE = originalDepartment.PK;
				jobCharge1.JR_JH = job1.PK;
				jobCharge1.JR_OSCostAmt = jobChargeExTaxAmount;
				jobCharge1.JR_OH_SellAccount = agent.PK;
				jobCharge1.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				jobCharge1.JR_OSSellAmt = jobChargeExTaxAmount;
				jobCharge1.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;

				Charge jobCharge2 = job2.Charges.AddNew();
				jobCharge2.JR_AC = chargeCode2.PK;
				jobCharge2.JR_GB = differentBranch1.PK;
				jobCharge2.JR_GE = originalDepartment.PK;
				jobCharge2.JR_JH = job2.PK;
				jobCharge2.JR_OSCostAmt = jobChargeExTaxAmount;
				jobCharge2.JR_OH_SellAccount = agent.PK;
				jobCharge2.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				jobCharge2.JR_OSSellAmt = jobChargeExTaxAmount;
				jobCharge2.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
				factory2.Save();

				AssertEquals("Job Charge 1 JR_OSCostAmt", jobChargeExTaxAmount, jobCharge1.JR_OSCostAmt);
				AssertEquals("Job Charge 1 JR_OSSellAmt", jobChargeExTaxAmount, jobCharge1.JR_OSSellAmt);
				AssertEquals("Job Charge 1 JR_AT_SellGSTRate", TestObjectCreator.GST1.PK, jobCharge1.JR_AT_SellGSTRate);
				AssertEquals("Job Charge 1 JR_OSSellGSTAmt", jobChargeTaxAmount, jobCharge1.JR_OSSellGSTAmt_Calc);

				AssertEquals("Job Charge 2 JR_OSCostAmt", jobChargeExTaxAmount, jobCharge2.JR_OSCostAmt);
				AssertEquals("Job Charge 2 JR_OSSellAmt", jobChargeExTaxAmount, jobCharge2.JR_OSSellAmt);
				AssertEquals("Job Charge 2 JR_AT_SellGSTRate", TestObjectCreator.GST1.PK, jobCharge2.JR_AT_SellGSTRate);
				AssertEquals("Job Charge 2 JR_OSSellGSTAmt", jobChargeTaxAmount, jobCharge2.JR_OSSellGSTAmt_Calc);

				var jobs = new Job[] { job1, job2 };
				ConsolInvoicingPostManager transactionCreator = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(factory2, consol));
				TransactionCreatorHashtable transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);
				AssertEquals("One AR transaction should have been created", 1, transactions.ARTransactionsCount);
				InvoicingBase[] arInvoices = transactions.GetAllARInvoicesAndCreditNotes();
				transaction = arInvoices[0];
				AssertEquals("AR Invoice should have two lines", 2, transaction.Lines.Count);

				AssertEquals("AR Invoice Line 1 ChargeCode", chargeCode2.PK, transaction.Lines[0].AL_AC);
				AssertEquals("AR Invoice Line 1 Amount", jobChargeExTaxAmount, transaction.Lines[0].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 1 Tax Rate", TestObjectCreator.GST1.PK, transaction.Lines[0].AL_AT);
				AssertEquals("AR Invoice Line 1 Tax Amount", jobChargeTaxAmount, transaction.Lines[0].AL_OSGSTAmount);

				AssertEquals("AR Invoice Line 2 ChargeCode", chargeCode2.PK, transaction.Lines[1].AL_AC);
				AssertEquals("AR Invoice Line 2 Amount", jobChargeExTaxAmount, transaction.Lines[1].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 1 Tax Rate", TestObjectCreator.GST1.PK, transaction.Lines[1].AL_AT);
				AssertEquals("AR Invoice Line 2 Tax Amount", jobChargeTaxAmount, transaction.Lines[1].AL_OSGSTAmount);
				factory2.Save();
			}

			InvoicingBase convertedInvoice = converter.ConvertToAP(transaction, false);

			try
			{
				AssertNotNull("Converted Invoice should not be null", convertedInvoice);
				Assert("Converted Invoice should be an AP Invoice", convertedInvoice is APInvoice);
				Assert(string.Format("AP Invoice should not have errors: {0}", convertedInvoice.NotificationsIncludingChildren.ToMessageListString()), !convertedInvoice.HasErrors);
				convertedInvoice.Factory.Save();
				AssertEquals("Line Count", 2, convertedInvoice.Lines.Count);

				// Tax ID cannot be preserved when moving between countries. Tax amount is added to ExTaxAmount
				AssertEquals("Line 1 Charge Code", chargeCode1.PK, convertedInvoice.Lines[0].AL_AC);
				AssertEquals("Line 1 Amount", jobChargeExTaxAmount + jobChargeTaxAmount, convertedInvoice.Lines[0].AL_OSExTaxAmount);
				AssertEquals("Line 1 Tax Rate", ZGuid.Empty, convertedInvoice.Lines[0].AL_AT);
				AssertEquals("Line 1 Tax Amount", 0m, convertedInvoice.Lines[0].AL_OSGSTAmount);

				AssertEquals("Line 2 Charge Code", chargeCode1.PK, convertedInvoice.Lines[1].AL_AC);
				AssertEquals("Line 2 Amount", jobChargeExTaxAmount + jobChargeTaxAmount, convertedInvoice.Lines[1].AL_OSExTaxAmount);
				AssertEquals("Line 2 Tax Rate", ZGuid.Empty, convertedInvoice.Lines[1].AL_AT);
				AssertEquals("Line 2 Tax Amount", 0m, convertedInvoice.Lines[1].AL_OSGSTAmount);

				using (Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex())
				{
					Assert("Job1 should be saved", job1 != null && job1.IsInDatabase);
				}

				using (Job job2 = new Job.Loader(Factory, shipment2).TryLoadOrCreateWithMutex())
				{
					Assert("Job2 should be saved", job2 != null && job2.IsInDatabase);
				}

				using (Job job3 = new Job.Loader(Factory, shipment3).TryLoadOrCreateWithMutex())
				{
					AssertNull("Job3 should be created by CosolCost OnSaved and locked with Mutex", job3);
				}
			}
			finally
			{
				convertedInvoice.ReleaseAllMutexOnInvoice();
			}
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertToAPFromConsolARInvoiceWithoutConsolCostDefaultingPostDate()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			setupRegistryForPostDateDefaulting();

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);

			originalDepartment.GE_Misc = false;
			GlbDepartment department = Factory.Load<GlbDepartment>(originalDepartment.PK);
			department.GE_Misc = false;
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(false);
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(false);
			Factory.Save();

			AccChargeCode chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			AccChargeCode chargeCode2 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, differentCompany);
			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			ZDecimal jobChargeAmount = 500.00m;
			ZDateTime departureDate = new ZDateTime(2007, 06, 06);

			Factory.Save();

			ForwardingShipment shipment1 = null;
			ForwardingShipment shipment2 = null;
			ForwardingShipment shipment3 = null;
			ForwardingConsol consol = null;
			InvoicingBase transaction = null;

			using (new TemporaryUserContext() { DepartmentPK = originalDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();

				OrgHeader agent = factory2.Load<OrgHeader>(TestObjectCreator.Agent.PK);
				agent.CompanyData.OB_IsDebtor = true;
				agent.CompanyData.SetARTaxApplicable(false);
				agent.CompanyData.SetAPTaxApplicable(false);

				consol = factory2.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "SGSIN";
				consol.JK_UniqueConsignRef = "C00001000";
				consol.JK_TransportMode = "AIR";

				shipment1 = consol.Shipments.AddNew();
				shipment1.JS_UniqueConsignRef = "S00002000";
				shipment2 = consol.Shipments.AddNew();
				shipment2.JS_UniqueConsignRef = "S00002001";
				shipment3 = consol.Shipments.AddNew();
				shipment3.JS_UniqueConsignRef = "S00002002";

				consol.Transports.DepartureTransport.JW_ATD = departureDate;

				OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
				Job job1 = createJob(factory2, "J00001000", differentBranch1.PK, originalDepartment.PK, localClient, agent);
				job1.JH_ParentID = shipment1.PK;
				job1.JH_ParentTableCode = "JS";
				Job job2 = createJob(factory2, "J00001001", differentBranch1.PK, originalDepartment.PK, localClient, agent);
				job2.JH_ParentID = shipment2.PK;
				job2.JH_ParentTableCode = "JS";
				factory2.Save();

				Charge jobCharge1 = job1.Charges.AddNew();
				jobCharge1.JR_AC = chargeCode2.PK;
				jobCharge1.JR_GB = differentBranch1.PK;
				jobCharge1.JR_GE = originalDepartment.PK;
				jobCharge1.JR_JH = job1.PK;
				jobCharge1.JR_OSCostAmt = jobChargeAmount;
				jobCharge1.JR_OSSellAmt = jobChargeAmount;
				jobCharge1.JR_OH_SellAccount = agent.PK;
				jobCharge1.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;

				Charge jobCharge2 = job2.Charges.AddNew();
				jobCharge2.JR_AC = chargeCode2.PK;
				jobCharge2.JR_GB = differentBranch1.PK;
				jobCharge2.JR_GE = originalDepartment.PK;
				jobCharge2.JR_JH = job2.PK;
				jobCharge2.JR_OSCostAmt = jobChargeAmount;
				jobCharge2.JR_OSSellAmt = jobChargeAmount;
				jobCharge2.JR_OH_SellAccount = agent.PK;
				jobCharge2.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
				factory2.Save();

				AssertEquals("Job Charge 1 JR_OSCostAmt", jobChargeAmount, jobCharge1.JR_OSCostAmt);
				AssertEquals("Job Charge 2 JR_OSCostAmt", jobChargeAmount, jobCharge2.JR_OSCostAmt);
				AssertEquals("Job Charge 1 JR_OSSellAmt", jobChargeAmount, jobCharge1.JR_OSSellAmt);
				AssertEquals("Job Charge 2 JR_OSSellAmt", jobChargeAmount, jobCharge2.JR_OSSellAmt);

				var jobs = new Job[] { job1, job2 };
				ConsolInvoicingPostManager transactionCreator = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(factory2, consol));
				TransactionCreatorHashtable transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);
				AssertEquals("One AR transaction should have been created", 1, transactions.ARTransactionsCount);
				InvoicingBase[] arInvoices = transactions.GetAllARInvoicesAndCreditNotes();
				transaction = arInvoices[0];
				AssertEquals("AR Invoice should have two lines", 2, transaction.Lines.Count);
				AssertEquals("AR Invoice Line 1 Amount", jobChargeAmount, transaction.Lines[0].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 2 Amount", jobChargeAmount, transaction.Lines[1].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 1 ChargeCode", chargeCode2.PK, transaction.Lines[0].AL_AC);
				AssertEquals("AR Invoice Line 2 ChargeCode", chargeCode2.PK, transaction.Lines[1].AL_AC);
				factory2.Save();
			}

			InvoicingBase convertedInvoice = converter.ConvertToAP(transaction, false);

			try
			{
				AssertNotNull("Converted Invoice should not be null", convertedInvoice);
				Assert("Converted Invoice should be an AP Invoice", convertedInvoice is APInvoice);
				Assert(string.Format("AP Invoice should not have errors: {0}", convertedInvoice.NotificationsIncludingChildren.ToMessageListString()), !convertedInvoice.HasErrors);
				convertedInvoice.Factory.Save();
				AssertEquals("Line Count", 2, convertedInvoice.Lines.Count);
				AssertEquals("Line 1 Amount", jobChargeAmount, convertedInvoice.Lines[0].AL_OSExTaxAmount);
				AssertEquals("Line 2 Amount", jobChargeAmount, convertedInvoice.Lines[1].AL_OSExTaxAmount);
				AssertEquals("Line 1 Charge Code", chargeCode1.PK, convertedInvoice.Lines[0].AL_AC);
				AssertEquals("Line 2 Charge Code", chargeCode1.PK, convertedInvoice.Lines[1].AL_AC);
				AssertEquals("AP Invoice Post Date", departureDate, convertedInvoice.AH_PostDate);

				using (Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex())
				{
					Assert("Job1 should be saved", job1 != null && job1.IsInDatabase);
				}

				using (Job job2 = new Job.Loader(Factory, shipment2).TryLoadOrCreateWithMutex())
				{
					Assert("Job2 should be saved", job2 != null && job2.IsInDatabase);
				}

				using (Job job3 = new Job.Loader(Factory, shipment3).TryLoadOrCreateWithMutex())
				{
					AssertNull("Job3 should be created by CosolCost OnSaved and locked with Mutex", job3);
				}
			}
			finally
			{
				convertedInvoice.ReleaseAllMutexOnInvoice();
			}
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertToAPFromConsolARInvoiceWithoutConsolCost_RelatedShipments()
		{
			setupPeriodManagement(ZDateTime.Today.Year);
			AccountingConfigurationRegistry.Instance.ConsolCostDefaultRelatedShipmentsApportionment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);

			AccChargeCode chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			AccChargeCode chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);

			OrgHeader agent = Factory.Load<OrgHeader>(TestObjectCreator.Agent.PK);
			agent.CompanyData.OB_IsDebtor = true;
			agent.CompanyData.OB_IsCreditor = true;
			agent.CompanyData.SetARTaxApplicable(false);
			agent.CompanyData.SetAPTaxApplicable(false);

			TestObjectCreator.AALSHI.OH_IsDebtor = true;
			TestObjectCreator.AALSHI.OH_IsCreditor = true;
			TestObjectCreator.ZECTRA.OH_IsDebtor = true;
			TestObjectCreator.ZECTRA.OH_IsCreditor = true;

			var consol = TestObjectCreator.CreateConsol(TestObjectCreator.AALSHI.OH_RL_NKClosestPort, TestObjectCreator.ZECTRA.OH_RL_NKClosestPort, "C000001");

			TestObjectCreator.AALSHI.OH_IsForwarder = true;
			TestObjectCreator.ZECTRA.OH_IsForwarder = true;
			consol.JK_OA_SendingForwarderAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = TestObjectCreator.ZECTRA.MainAddress.PK;

			ForwardingShipment masterForwardingShipment = consol.Shipments.AddNew();
			masterForwardingShipment.JS_UniqueConsignRef = "S00001001";
			masterForwardingShipment.JS_ReleaseType = "SWB";
			masterForwardingShipment.JS_TransportMode = "AIR";
			masterForwardingShipment.JS_ShipmentType = "BCN";
			masterForwardingShipment.JS_PackingMode = "BCN";

			ForwardingShipment subForwardingShipment = consol.Shipments.AddNew();
			subForwardingShipment.JS_UniqueConsignRef = "S00001002";
			subForwardingShipment.JS_ReleaseType = "SWB";
			subForwardingShipment.JS_TransportMode = "AIR";
			subForwardingShipment.JS_ShipmentType = "STD";
			subForwardingShipment.JS_PackingMode = "LSE";
			subForwardingShipment.JS_JS_ColoadMasterShipment = masterForwardingShipment.PK;

			var job1 = TestObjectCreator.CreateJob(masterForwardingShipment);
			job1.JH_JobNum = "J00001001";
			job1.AgentCollectPK = TestObjectCreator.ZECTRA.PK;

			var job2 = TestObjectCreator.CreateJob(subForwardingShipment);
			job2.JH_JobNum = "J00001002";
			job2.AgentCollectPK = TestObjectCreator.ZECTRA.PK;

			Factory.Save();

			ConsolRevenueMaster revenueMaster = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue.ConsolRevenue revenue1 = revenueMaster.Revenues.AddNew();
			revenue1.ChargeCode = chargeCode1.PK;
			revenue1.ApportionmentMethod = "CHG";
			revenue1.SellAmount = 50.00M;

			ConsolRevenue.ConsolRevenue revenue2 = revenueMaster.Revenues.AddNew();
			revenue2.ChargeCode = chargeCode2.PK;
			revenue2.ApportionmentMethod = "SHP";
			revenue2.SellAmount = 1300.00M;

			Factory.Save();

			job1.Charges.Load();
			job2.Charges.Load();
			foreach (Charge charge in job1.Charges)
			{
				charge.JR_OH_SellAccount = TestObjectCreator.ZECTRA.PK;
			}

			foreach (Charge charge in job2.Charges)
			{
				charge.JR_OH_SellAccount = TestObjectCreator.ZECTRA.PK;
			}
			Factory.Save();

			var jobs = new Job[] { job1, job2 };
			ConsolInvoicingPostManager transactionCreator = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(Factory, consol));
			var transaction = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);

			var arTransactions = transaction.GetAllARInvoicesAndCreditNotes();
			var arTransaction = arTransactions[0];
			Factory.Save();

			InvoicingBase apInvoice = converter.ConvertToAP(arTransaction, false);

			AssertNotNull(apInvoice);
			AssertEquals(4, apInvoice.Lines.Count);
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertToAPFromShipmentARInvoiceDefaultingPostDate()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			setupRegistryForPostDateDefaulting();

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);

			originalDepartment.GE_Misc = false;
			GlbDepartment department = Factory.Load<GlbDepartment>(originalDepartment.PK);
			department.GE_Misc = false;
			TestObjectCreator.Agent.CompanyData.SetARTaxApplicable(false);
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(false);
			Factory.Save();

			AccChargeCode chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			AccChargeCode chargeCode2 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, differentCompany);
			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			ZDecimal jobChargeAmount = 500.00m;
			ZDateTime arrivalDate = new ZDateTime(2007, 06, 06);

			Factory.Save();

			ForwardingShipment shipment1 = null;
			InvoicingBase transaction = null;

			using (new TemporaryUserContext() { DepartmentPK = originalDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();

				OrgHeader agent = factory2.Load<OrgHeader>(TestObjectCreator.Agent.PK);
				agent.CompanyData.OB_IsDebtor = true;
				agent.CompanyData.SetARTaxApplicable(false);
				agent.CompanyData.SetAPTaxApplicable(false);

				shipment1 = factory2.New<ForwardingShipment>();
				shipment1.JS_UniqueConsignRef = "S00002000";
				shipment1.JS_E_ARV = arrivalDate;

				OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
				Job job1 = createJob(factory2, "J00001000", differentBranch1.PK, originalDepartment.PK, localClient, agent);
				job1.JH_ParentID = shipment1.PK;
				job1.JH_ParentTableCode = "JS";
				factory2.Save();

				Charge jobCharge1 = job1.Charges.AddNew();
				jobCharge1.JR_AC = chargeCode2.PK;
				jobCharge1.JR_GB = differentBranch1.PK;
				jobCharge1.JR_GE = originalDepartment.PK;
				jobCharge1.JR_JH = job1.PK;
				jobCharge1.JR_OSCostAmt = jobChargeAmount;
				jobCharge1.JR_OSSellAmt = jobChargeAmount;
				jobCharge1.JR_OH_SellAccount = agent.PK;
				jobCharge1.JR_InvoiceType = ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
				factory2.Save();

				AssertEquals("Job Charge 1 JR_OSCostAmt", jobChargeAmount, jobCharge1.JR_OSCostAmt);
				AssertEquals("Job Charge 1 JR_OSSellAmt", jobChargeAmount, jobCharge1.JR_OSSellAmt);

				InvoicingPostManager transactionCreator = new InvoicingPostManager(job1);
				TransactionCreatorHashtable transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);
				AssertEquals("One AR transaction should have been created", 1, transactions.ARTransactionsCount);
				InvoicingBase[] arInvoices = transactions.GetAllARInvoicesAndCreditNotes();
				transaction = arInvoices[0];
				AssertEquals("AR Invoice should have two lines", 1, transaction.Lines.Count);
				AssertEquals("AR Invoice Line 1 Amount", jobChargeAmount, transaction.Lines[0].AL_OSExTaxAmount);
				AssertEquals("AR Invoice Line 1 ChargeCode", chargeCode2.PK, transaction.Lines[0].AL_AC);
				factory2.Save();
			}

			using (Job job2 = new Job.Loader(Factory, shipment1).TryCreateWithMutex())
			{
				job2.JH_ParentID = shipment1.PK;
				job2.JH_ParentTableCode = "JS";
				job2.JH_JobNum = "J00001001";
				job2.JH_GB = GlbBranch.CurrentBranch.PK;
				job2.JH_GE = GlbDepartment.CurrentDepartment.PK;

				Factory.Save();
				InvoicingBase convertedInvoice = converter.ConvertToAP(transaction, false);

				try
				{
					AssertNotNull("Converted Invoice should not be null", convertedInvoice);
					Assert("Converted Invoice should be an AP Invoice", convertedInvoice is APInvoice);
					Assert(string.Format("AP Invoice should not have errors: {0}", convertedInvoice.NotificationsIncludingChildren.ToMessageListString()), !convertedInvoice.HasErrors);
					convertedInvoice.Factory.Save();
					AssertEquals("Line Count", 1, convertedInvoice.Lines.Count);
					AssertEquals("Line 1 Amount", jobChargeAmount, convertedInvoice.Lines[0].AL_OSExTaxAmount);
					AssertEquals("Line 1 Charge Code", chargeCode1.PK, convertedInvoice.Lines[0].AL_AC);
					AssertEquals("AP Invoice Post Date", arrivalDate, convertedInvoice.AH_PostDate);

					using (Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex())
					{
						Assert("Job1 should be saved", job1 != null && job1.IsInDatabase);
					}
				}
				finally
				{
					convertedInvoice.ReleaseAllMutexOnInvoice();
				}
			}
		}

		[TestDate(2015, 1, 15)]
		public void TestDefaultingPostDate()
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			var agentInSisterCompanyFactory = TestObjectCreator.Agent;

			AccChargeCode chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			AccChargeCode chargeCode2 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, differentCompany);

			TestObjectCreator.CreateExchangeRate(Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD"), "BUY", 1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			ForwardingShipment jobShipment = TestObjectCreator.CreateShipment("0099");
			jobShipment.JS_UniqueConsignRef = "S0000555";

			InvoicingBase transaction = null;
			using (new TemporaryUserContext() { DepartmentPK = originalDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				var factory2 = new BusinessObjectFactory();
				var testObjectCreator = new TestObjectCreator(factory2);

				var currency = testObjectCreator.USD;
				differentCompany.GC_RX_NKLocalCurrency = currency.Code;

				OrgHeader agent = factory2.Load<OrgHeader>(agentInSisterCompanyFactory.PK);
				agent.CompanyData.OB_IsDebtor = true;

				var job = testObjectCreator.CreateJob(testObjectCreator.AALSHI, 0M, agent, 0M);
				job.JH_ParentID = jobShipment.PK;
				job.JH_ParentTableCode = "JS";
				job.AddCurrency(differentCompany.LocalCurrency, 0.5M, ExchangeRateValidLedgerEnum.None);

				var charge = testObjectCreator.CreateCharge(job, chargeCode2, "desc", currency, 0M, testObjectCreator.AALSHI, currency, 100M, agent);
				charge.JR_GB = differentBranch1.PK;
				charge.JR_GE = originalDepartment.PK;
				charge.JR_RX_NKSellInvoiceCurrency = currency.Code;
				factory2.Save();

				InvoicingPostManager transactionCreator = new InvoicingPostManager(job);
				TransactionCreatorHashtable transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);
				AssertEquals("One AR transaction should have been created", 1, transactions.ARTransactionsCount);

				InvoicingBase[] arInvoices = transactions.GetAllARInvoicesAndCreditNotes();
				transaction = arInvoices[0];
				transaction.AH_PostDate = ZDateTime.Today.AddDays(-7); //the invoice has post date in the past
				factory2.Save();
			}

			using (Job job2 = new Job.Loader(Factory, jobShipment).TryCreateWithMutex())
			{
				job2.JH_ParentID = jobShipment.PK;
				job2.JH_ParentTableCode = "JS";
				job2.JH_JobNum = "J00001001";
				job2.JH_GB = GlbBranch.CurrentBranch.PK;
				job2.JH_GE = GlbDepartment.CurrentDepartment.PK;

				Factory.Save();
				UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
				InvoicingBase convertedInvoice = converter.ConvertToAP(transaction, false);

				try
				{
					AssertNotNull("Converted Invoice should not be null", convertedInvoice);
					Assert("Converted Invoice should be an AP Invoice", convertedInvoice is APInvoice);
					Assert(string.Format("AP Invoice should not have errors: {0}", convertedInvoice.NotificationsIncludingChildren.ToMessageListString()), !convertedInvoice.HasErrors);
				}
				finally
				{
					convertedInvoice.ReleaseAllMutexOnInvoice();
				}
			}
		}

		[TestDate(2011, 11, 29, 12, 00, 00)]
		public void TestImportingAPInvoicesFromSisterCompanies()
		{
			var creator = new TestObjectCreator(Factory);
			var converter = new UnapprovedTransactionConverter(Factory);

			var shipment = Factory.New<ForwardingShipment>();
			var testJob = creator.CreateJob(shipment, false);

			var transaction = creator.CreateARInvoice<ARInvoice>("100101", creator.AUD, 1m, differentBranchOrgProxy);
			transaction.AH_JH = testJob.PK;
			var line1 = creator.CreateARInvoiceLine(transaction, testJob, creator.CC4, creator.AUD, 1.0m, "HIHIHI", 10000);
			line1.AL_GB = originalBranch.PK;
			line1.AL_GE = originalDepartment.PK;
			creator.CreateJobCharge(line1, testJob, creator.CC4, creator.AUD);
			line1.AL_JH = testJob.PK;

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = creator.AALSHI.PK;
			creator.NonCurrentCompany.GC_OH_OrgProxy = creator.ABIGAS.PK;

			Factory.Save();

			using (new TemporaryUserContext() { DepartmentPK = creator.FISDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				var testJobInAnotherCompany = creator.CreateJob(shipment, false);
				testJobInAnotherCompany.JH_GB = differentBranch2.PK;
				testJobInAnotherCompany.JH_GE = creator.FESDepartment.PK;
				InvoicingBase resultofconverter = null;

				using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
				{
					resultofconverter = converter.ConvertToAPUnsafe(transaction, Factory, false);

					CombineAssertions(delegate
					{
						AssertEquals("Branch on original Transaction", line1.AL_GB, transaction.AH_GB);
						AssertEquals("Department on original Transaction", line1.AL_GE, transaction.AH_GE);
						AssertEquals("Branch on result Transaction", GlbBranch.CurrentBranch.PK, resultofconverter.AH_GB);
						AssertEquals("Department on result Transaction", testJobInAnotherCompany.JH_GE.ToGuid(), resultofconverter.AH_GE);
						AssertEquals("Branch on result Transaction", testJobInAnotherCompany.JH_GB.ToGuid(), resultofconverter.Lines[0].AL_GB);
						AssertEquals("Department on result Transaction", testJobInAnotherCompany.JH_GE.ToGuid(), resultofconverter.Lines[0].AL_GE);
					});

					var exportFactories = PersistentFactoryCacheManager.Instance.TrackedFactories_ForTest.Where(f => f.NameForDebugging == "GetXMLInvoiceHeaderFactory");
					AssertEquals("GetXMLInvoiceHeaderFactory factory has been created", 1, exportFactories.Count());

					var exportFactory = exportFactories.First();
					var jobsPKsInExportFactory = exportFactory.Load<Job>(new ZQuery() { FetchOnlyFromLocalCache = true }).Select(j => j.PK);
					Assert("Should contain AR company job", jobsPKsInExportFactory.Contains(testJob.PK));
					Assert("Should not contain AP company job", !jobsPKsInExportFactory.Contains(testJobInAnotherCompany.PK));
					AssertEquals("Job count", 1, jobsPKsInExportFactory.Count());
				}
			}
		}

		public void TestImportingAPInvoicesFromSisterCompaniesWithInvoiceDateExchangeRate()
		{
			using (PostingExRateRegistryAP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code))
			{
				var creator = new TestObjectCreator(Factory);
				var converter = new UnapprovedTransactionConverter(Factory);

				var createExchangeRate = new Action<ZGuid, ZString, ZDecimal>((companyPK, rateType, rate) =>
				{
					var exchangeRate = creator.USD.ExchangeRates.AddNew();
					exchangeRate.RE_GC = companyPK;
					exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-10);
					exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(-1);
					exchangeRate.RE_ExRateType = rateType;
					exchangeRate.RE_RX_NKExCurrency = "USD";
					exchangeRate.RE_SellRate = rate;

					var exchangeRate2 = creator.USD.ExchangeRates.AddNew();
					exchangeRate2.RE_GC = companyPK;
					exchangeRate2.RE_StartDate = ZDateTime.Today;
					exchangeRate2.RE_ExpiryDate = ZDateTime.Today.AddDays(30);
					exchangeRate2.RE_ExRateType = rateType;
					exchangeRate2.RE_RX_NKExCurrency = "USD";
					exchangeRate2.RE_SellRate = rate + 0.20M;

					Factory.Save();
				});

				createExchangeRate(GlbCompany.CurrentCompany.PK, ExchangeRateTypes.Code.SellRate, 1.25M);
				createExchangeRate(differentCompany.PK, ExchangeRateTypes.Code.BuyRate, 1.10M);

				var shipment = Factory.New<ForwardingShipment>();
				var testJob = creator.CreateJob(shipment, false);

				var transaction = creator.CreateInvoice(typeof(ARInvoice), creator.USD, 1.258M, creator.ABIGAS, ZDateTime.Today.AddDays(-6));
				transaction.AH_TransactionNum = "100101";
				transaction.AH_JH = testJob.PK;
				var line1 = creator.CreateARInvoiceLine(transaction as ARInvoice, testJob, creator.CC4, creator.USD, 1.258M, "HIHIHI", 10000);
				line1.AL_GB = originalBranch.PK;
				line1.AL_GE = originalDepartment.PK;
				creator.CreateJobCharge(line1, testJob, creator.CC4, creator.USD);
				line1.AL_JH = testJob.PK;

				GlbCompany.CurrentCompany.GC_OH_OrgProxy = creator.AALSHI.PK;
				testJob.ExchangeRates[0].JF_BaseRate = 1; //prevent failing on saving
				Factory.Save();

				using (new TemporaryUserContext() { DepartmentPK = creator.FISDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
				{
					var resultofconverter = converter.ConvertToAPUnsafe(transaction, Factory, false);
					AssertNoMessageErrors(resultofconverter.AH_ExchangeRateInfo);

					resultofconverter.AH_ExchangeRate = 1.30M;
					Assert(resultofconverter.AH_ExchangeRateInfo.Notifications.GetFirstMessage().Contains("The \"AP Invoice Posting Exchange Rate Option\" has been set to \"Exchange Rate based on Invoice Date\".\r\nWith this option, the exchange rate should not be changed manually"));

					resultofconverter.Lines?.OfType<InvoicingLineBase>()?.ToList().ForEach(x => x.Job?.Dispose());
				}
			}
		}

		[TestDate(2022, 05, 02, 18, 00, 00)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestImportingAPInvoicesFromSisterCompaniesWhenInvoiceDateGreaterThanPostDate()
		{
			TestObjectCreator creator = null;
			ForwardingShipment shipment = null;
			UnapprovedTransactionConverter converter = null;
			ARInvoice transaction = null;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				TestDateAttribute.UseUNLOCO = true;

				creator = new TestObjectCreator(Factory);
				converter = new UnapprovedTransactionConverter(Factory);
				shipment = Factory.New<ForwardingShipment>();
				var testJob = creator.CreateJob(shipment, false);

				transaction = creator.CreateARInvoice<ARInvoice>("100101", creator.AUD, 1m, creator.ABIGAS);
				transaction.AH_JH = testJob.PK;
				var line1 = creator.CreateARInvoiceLine(transaction, testJob, creator.CC4, creator.AUD, 1.0m, "HIHIHI", 10000);
				line1.AL_GB = originalBranch.PK;
				line1.AL_GE = originalDepartment.PK;
				creator.CreateJobCharge(line1, testJob, creator.CC4, creator.AUD);
				line1.AL_JH = testJob.PK;

				GlbCompany.CurrentCompany.GC_OH_OrgProxy = creator.AALSHI.PK;
				creator.NonCurrentCompany.GC_OH_OrgProxy = creator.ABIGAS.PK;

				Factory.Save();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.PreventInvoiceDateGreaterThanPostDate.SetTemporaryValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (new TemporaryUserContext() { DepartmentPK = creator.FISDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				var testJobInAnotherCompany = creator.CreateJob(shipment, false);

				testJobInAnotherCompany.JH_GB = ZGuid.Empty;
				testJobInAnotherCompany.JH_GE = ZGuid.Empty;

				AssertEquals(ZGuid.Empty, testJobInAnotherCompany.JH_GB);
				AssertEquals(ZGuid.Empty, testJobInAnotherCompany.JH_GE);

				TestDateAttribute.UseUNLOCO = false;
				var resultofconverter = converter.ConvertToAPUnsafe(transaction, Factory, false);

				Assert(resultofconverter.AH_InvoiceDateInfo.Notifications.Contains("Invoice Date must be earlier or same as the Post Date. This is controlled by the registry Accounting > Payable Defaults > Default Settings > Prevent Posting Invoice Date Greater Than Post Date."));

				resultofconverter.Lines?.OfType<InvoicingLineBase>()?.ToList().ForEach(x => x.Job?.Dispose());
			}
		}

		#region Compliance Sequence Allocation Errors

		ARInvoice CreateTransactionFromSisterCompany(TestObjectCreator creator, UnapprovedTransactionConverter converter, ForwardingShipment shipment)
		{
			TestDateAttribute.UseUNLOCO = true;

			var testJob = creator.CreateJob(shipment, false);

			var transaction = creator.CreateARInvoice<ARInvoice>("100101", creator.AUD, 1m, creator.ABIGAS);
			transaction.AH_JH = testJob.PK;
			var line1 = creator.CreateARInvoiceLine(transaction, testJob, creator.CC1, creator.AUD, 1.0m, "HIHIHI", 10000);
			line1.AL_LineAmount = 10005;
			line1.AL_GSTVAT = 5m;
			line1.AL_OSExTaxAmount = 10000;
			line1.AL_OSTaxAmount = 5m;
			line1.AL_GB = originalBranch.PK;
			line1.AL_GE = originalDepartment.PK;
			creator.CreateJobCharge(line1, testJob, creator.CC1, creator.AUD);
			line1.AL_JH = testJob.PK;

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = creator.AALSHI.PK;
			creator.NonCurrentCompany.GC_OH_OrgProxy = creator.ABIGAS.PK;

			Factory.Save();

			return transaction;
		}

		(ComplianceSubTypeAttributionRuleConfigurationCollection, ZDate) SetupComplianceSequenceAndCollection(TestObjectCreator creator, string sequenceClass, bool expiredComplianceSequence, bool postDateEarlierThanLastDateUsed)
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			var startDate = new ZDate(2022, 04, 01);
			var expiryDate = expiredComplianceSequence ? new ZDate(2022, 04, 30) : ZDate.Empty;
			var lastDateUsed = postDateEarlierThanLastDateUsed ? new ZDate(2022, 05, 20) : new ZDate(2022, 04, 15);
			var sequence = creator.SetupComplianceSequence(ZGuid.Empty, sequenceClass, "APS-", 1, 99, 1, differentCompany.PK, differentBranch1.PK, startDate, expiryDate);

			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var config = collection.AddNew();
			config.Country = Constants.CountryCodes.Italy;
			config.SubType = sequenceClass;
			config.LedgerType = LedgerTypes.AccountsPayable;
			config.InvoiceType = TransactionTypes.Invoice;
			config.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
			config.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			config.OriginalRule = OriginalRuleCodes.AllTransactions;

			Factory.Save();

			return (collection, lastDateUsed);
		}

		void CreateExistingAPTransactions(AccountingMasterFilesRegistry registry, TestObjectCreator creator, string sequenceClass, ZDate lastDateUsed, Boolean createSparseBookTransaction)
		{
			using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var invoices = new APInvoice[5];
				for (var i = 0; i < 5; i++)
				{
					invoices[i] = creator.CreateAPInvoice<APInvoice>("INV00" + i, creator.EUR, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, creator.Debtor);
					invoices[i].AH_ComplianceSubType = sequenceClass;
					invoices[i].AH_PostDate = lastDateUsed.AddDays(i);
				}
				Factory.Save();
			}

			if (createSparseBookTransaction)
			{
				using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
				{
					var sparseBookinvoice = creator.CreateAPInvoice<APInvoice>("INV009", creator.EUR, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, creator.Debtor);
					sparseBookinvoice.AH_ComplianceSubType = sequenceClass;
					sparseBookinvoice.AH_PostDate = new ZDate(2022, 04, 20);
					Factory.Save();
				}
			}
		}

		void AssertComplianceSequenceBookError(AccountingMasterFilesRegistry registry, TestObjectCreator creator, UnapprovedTransactionConverter converter, ForwardingShipment shipment, ARInvoice transaction, string expectedError)
		{
			using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var testJobInAnotherCompany = creator.CreateJob(shipment, false);

				testJobInAnotherCompany.JH_GB = ZGuid.Empty;
				testJobInAnotherCompany.JH_GE = ZGuid.Empty;

				AssertEquals(ZGuid.Empty, testJobInAnotherCompany.JH_GB);
				AssertEquals(ZGuid.Empty, testJobInAnotherCompany.JH_GE);

				TestDateAttribute.UseUNLOCO = false;
				var resultofconverter = converter.ConvertToAPUnsafe(transaction, Factory, false, isAutoImport: true);

				Assert(resultofconverter.Notifications.Contains(expectedError));
				resultofconverter.Lines?.OfType<InvoicingLineBase>()?.ToList().ForEach(x => x.Job?.Dispose());
			}
		}

		void AssertComplianceSequenceAllocationErrors(bool expiredComplianceSequence, bool postDateEarlierThanLastDateUsed, bool createSparseBookTransaction, string expectedError)
		{
			TestObjectCreator creator = null;
			UnapprovedTransactionConverter converter = null;
			ForwardingShipment shipment = null;
			ARInvoice transaction = null;
			const string sequenceClass = "APS";
			ZDate lastDateUsed;
			ComplianceSubTypeAttributionRuleConfigurationCollection collection = null;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				creator = new TestObjectCreator(Factory);
				converter = new UnapprovedTransactionConverter(Factory);
				shipment = Factory.New<ForwardingShipment>();
				transaction = CreateTransactionFromSisterCompany(creator, converter, shipment);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				(collection, lastDateUsed) = SetupComplianceSequenceAndCollection(creator, sequenceClass, expiredComplianceSequence, postDateEarlierThanLastDateUsed);
				var registry = AccountingMasterFilesRegistry.Instance;
				using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
				using (registry.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
				using (new TemporaryUserContext() { DepartmentPK = creator.FISDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
				{
					transaction.Company.OrgProxy.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);
					CreateExistingAPTransactions(registry, creator, sequenceClass, lastDateUsed, createSparseBookTransaction);
					AssertComplianceSequenceBookError(registry, creator, converter, shipment, transaction, expectedError);
				}
			}
		}

		[TestDate(2022, 05, 15, 10, 00, 00)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestImportingAPInvoicesFromSisterCompaniesWhenOrderByPostDate_ComplianceSequenceAllocationErrors_SequenceBookNotExistOrExpired()
		{
			var expectedError = "Error - Accounts Payable Invoice: Please check your Compliance Invoice Book Setups. \r\n A Compliance Invoice Book for the relevant Compliance Sub-Type, Branch, Active Status and Start / Expiry Date does not exist.";
			AssertComplianceSequenceAllocationErrors(true, false, false, expectedError);
		}

		[TestDate(2022, 05, 15, 10, 00, 00)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestImportingAPInvoicesFromSisterCompaniesWhenOrderByPostDate_ComplianceSequenceAllocationErrors_PostDateEarlierThanLastDateUsed()
		{
			var expectedError = "Error - Accounts Payable Invoice: Compliance Numbers cannot be allocated.\r\n Last posted transaction with the same Compliance Sub Type APS has Post Date = 24-May-22, that is greater than the current one(s).";
			AssertComplianceSequenceAllocationErrors(false, true, false, expectedError);
		}

		[TestDate(2022, 05, 15, 10, 00, 00)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestImportingAPInvoicesFromSisterCompaniesWhenOrderByPostDate_ComplianceSequenceAllocationErrors_SparseBook()
		{
			var expectedError = "Error - Accounts Payable Invoice: Compliance Numbers cannot be allocated.\r\n There is some transaction with the same Compliance Sub Type APS in earlier Post Date and Compliance Number empty.\r\n Please allocate Compliance Number to all transactions with Post Date < 15-May-22.";
			AssertComplianceSequenceAllocationErrors(false, false, true, expectedError);
		}

		#endregion

		[TestDate(2011, 11, 29, 12, 00, 00)]
		public void TestImportingAPInvoicesFromSisterCompaniesFromConsol()
		{
			var creator = new TestObjectCreator(Factory);
			var converter = new UnapprovedTransactionConverter(Factory);

			var consol = creator.CreateConsol("AU", "HK", "1010101");

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00002000";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00002001";

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var job1 = creator.CreateJob(shipment1, false);
			var job2 = creator.CreateJob(shipment2, false);

			var transaction = creator.CreateARInvoice<ARInvoice>("100101", creator.AUD, 1m, differentBranchOrgProxy);

			var line1 = creator.CreateARInvoiceLine(transaction, job1, creator.CC4, creator.AUD, 1.0m, "HIHIHI", 10000);
			line1.AL_GB = originalBranch.PK;
			line1.AL_GE = originalDepartment.PK;
			creator.CreateJobCharge(line1, job1, creator.CC4, creator.AUD);
			line1.AL_JH = job1.PK;

			var line2 = creator.CreateARInvoiceLine(transaction, job2, creator.CC4, creator.AUD, 1.0m, "HIHIHI", 10000);
			line2.AL_GB = originalBranch.PK;
			line2.AL_GE = originalDepartment.PK;
			creator.CreateJobCharge(line2, job2, creator.CC4, creator.AUD);
			line2.AL_JH = job2.PK;

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = creator.AALSHI.PK;
			creator.NonCurrentCompany.GC_OH_OrgProxy = creator.ABIGAS.PK;

			Factory.Save();

			using (new TemporaryUserContext() { DepartmentPK = creator.FISDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				var testJobInAnotherCompany1 = creator.CreateJob(shipment1, false);
				testJobInAnotherCompany1.JH_GB = differentBranch2.PK;
				testJobInAnotherCompany1.JH_GE = creator.FESDepartment.PK;

				var testJobInAnotherCompany2 = creator.CreateJob(shipment2, false);
				testJobInAnotherCompany2.JH_GB = differentBranch2.PK;
				testJobInAnotherCompany2.JH_GE = creator.FESDepartment.PK;

				var resultofconverter = converter.ConvertToAPUnsafe(transaction, Factory, false);

				CombineAssertions(delegate
				{
					AssertEquals("Branch on original Transaction", line1.AL_GB, transaction.AH_GB);
					AssertEquals("Department on original Transaction", line1.AL_GE, transaction.AH_GE);
					AssertEquals("Branch on original Transaction", line2.AL_GB, transaction.AH_GB);
					AssertEquals("Department on original Transaction", line2.AL_GE, transaction.AH_GE);

					AssertEquals("Branch on result Transaction", differentBranch1.PK.ToGuid(), resultofconverter.AH_GB);
					AssertEquals("Department on result Transaction", creator.FESDepartment.PK.ToGuid(), resultofconverter.AH_GE);
					AssertEquals("Branch on result Transaction line 1", testJobInAnotherCompany1.JH_GB.ToGuid(), resultofconverter.Lines[0].AL_GB);
					AssertEquals("Department on result Transaction line 1", testJobInAnotherCompany1.JH_GE.ToGuid(), resultofconverter.Lines[0].AL_GE);

					AssertEquals("Branch on result Transaction line 2", testJobInAnotherCompany2.JH_GB.ToGuid(), resultofconverter.Lines[1].AL_GB);
					AssertEquals("Department on result Transaction line 2", testJobInAnotherCompany2.JH_GE.ToGuid(), resultofconverter.Lines[1].AL_GE);
				});
			}

			Assert(true);
		}

		[TestDate(2011, 11, 29, 12, 00, 00)]
		public void TestImportingAPInvoicesFromSisterCompaniesFromConsolWithChargeCodeBranchOverride()
		{
			var creator = new TestObjectCreator(Factory);
			var converter = new UnapprovedTransactionConverter(Factory);
			OrgHeader newOrg = TestObjectCreator.CreateOrgHeader("NEWORG", true, true);
			var consol = creator.CreateConsol("AU", "HK", "1010101");
			consol.JK_OA_ReceivingForwarderAddress = newOrg.MainAddress.PK;
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00002000";
			var job1 = creator.CreateJob(shipment1, false);
			var arInvoice = creator.CreateARInvoice<ARInvoice>("100101", creator.AUD, 1m, creator.ABIGAS);
			var line1 = creator.CreateARInvoiceLine(arInvoice, job1, creator.CC4, creator.AUD, 1.0m, "HIHIHI", 10000);
			line1.AL_GB = originalBranch.PK;
			line1.AL_GE = originalDepartment.PK;
			creator.CreateJobCharge(line1, job1, creator.CC4, creator.AUD);
			line1.AL_JH = job1.PK;

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = creator.AALSHI.PK;
			creator.NonCurrentCompany.GC_OH_OrgProxy = creator.ABIGAS.PK;

			Factory.Save();

			AssertEquals("Branch on original Transaction", arInvoice.AH_GB, line1.AL_GB);

			using (new TemporaryUserContext() { DepartmentPK = creator.FISDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				var chargeCode = TestObjectCreator.CC4;
				var chargeBranchOverride = chargeCode.BranchOverrides.AddNew();
				chargeBranchOverride.YA_JobType = JobInvoicingConsumerTypes.Shipment.Code;
				chargeBranchOverride.YA_Direction = Constants.FreightShipmentDirection.Code.All;
				chargeBranchOverride.YA_TransportMode = Enterprise.Core.Constants.TransportModes.All;
				chargeBranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.ReceivingAgent;

				GlbBranch overrideBranch = TestObjectCreator.CreateNewBranch(localCompany, "AB3");
				overrideBranch.GB_GC = GlbCompany.CurrentCompany.PK;
				overrideBranch.GB_OH_OrgProxy = newOrg.PK;

				Factory.Save();

				var testJobInLocalCompany = creator.CreateJob(shipment1, false);
				testJobInLocalCompany.JH_GB = differentBranch2.PK;
				testJobInLocalCompany.JH_GE = creator.FESDepartment.PK;

				var resultofconverter = converter.ConvertToAPUnsafe(arInvoice, Factory, false);
				AssertEquals("Expect the overridden branch", overrideBranch.PK, resultofconverter.Lines[0].AL_GB);
			}
		}

		StmALog GetApprovalEvent(InvoicingBase invoice)
		{
			StmALog result = null;
			StmALog[] logs = invoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TransactionApprovalActioned.Code));
			if (logs.Length > 0)
			{
				result = logs[0];
			}
			return result;
		}

		public void TestConvertToUA()
		{
			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);

			APInvoice testAPInvoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m);
			APCreditNote testAPCreditNote = (APCreditNote)TestObjectCreator.CreateInvoice(typeof(APCreditNote), TestObjectCreator.AUD, 3.0m);
			TestObjectCreator.CreateInvoiceLine(testAPInvoice, TestObjectCreator.AUD, 1.0m, 50);
			TestObjectCreator.CreateInvoiceLine(testAPCreditNote, TestObjectCreator.AUD, 1.0m, 444);

			InvoicingBase firstInvoice = converter.ConvertToUA(testAPInvoice);
			InvoicingBase secondInvoice = converter.ConvertToUA(testAPCreditNote);

			AssertEquals("The first converted invoice should be of type UAInvoice", TransactionTypes.UAInvoice, firstInvoice.AH_TransactionType);
			AssertEquals("A line in the first converted invoice should be of type UAInvoiceLine", TransactionLineTypes.UnapprovedCost, firstInvoice.Lines[0].AL_LineType);
			AssertEquals("1st OsExTaxAmount must left the same", 50m, firstInvoice.Lines[0].AL_OSExTaxAmount);
			Assert("1st AH_OSTaxAmount must be set", firstInvoice.AH_OSTaxAmount != 0);
			Assert("1st AH_OSExTaxAmount must be set", firstInvoice.AH_OSExTaxAmount != 0);

			AssertEquals("The second converted invoice should be of type UACreditNote", TransactionTypes.UACreditNote, secondInvoice.AH_TransactionType);
			AssertEquals("A line in the second converted invoice should be of type UACreditNoteLine", TransactionLineTypes.UnapprovedCost, secondInvoice.Lines[0].AL_LineType);
			AssertEquals("2nd OsExTaxAmount must left the same", 444m, secondInvoice.Lines[0].AL_OSExTaxAmount);
			Assert("2nd AH_OSTaxAmount must be set", secondInvoice.AH_OSTaxAmount != 0);
			Assert("2nd AH_OSExTaxAmount must be set", secondInvoice.AH_OSExTaxAmount != 0);
		}

		[DisableZeroExchangeRateOverriding]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestExchangeRatesForJobRelatedInvoices()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.JK_TransportMode = "AIR";
			consol.Transports.ArrivalTransport.JW_ATA = ZDateTime.Now.AddDays(-10);
			consol.Transports.DepartureTransport.JW_ATD = ZDateTime.Now.AddDays(-20);

			ForwardingShipment shipment = consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			freightCost.E6_ExchangeRate = 4.5m;
			freightCost.E6_OSCostAmount = 100m;

			freightCost.E6_ApportionmentMethod = "SHP";

			RefExchangeRate rate = TestObjectCreator.USD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Now.Date;
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(1);
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 1.5m;

			rate = TestObjectCreator.USD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Now.AddDays(-10).Date;
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(-9);
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 2.5m;

			rate = TestObjectCreator.USD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Now.AddDays(-20).Date;
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(-19);
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 3.5m;

			AccChargeCode chargeCode = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			chargeCode.AC_GC = differentCompany.PK;

			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
			ARInvoice arInvoice = null;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				rate = TestObjectCreator.AUD.ExchangeRates.AddNew();
				rate.RE_StartDate = ZDateTime.Now.Date;
				rate.RE_ExpiryDate = ZDateTime.Now.AddDays(1);
				rate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;
				rate.RE_SellRate = 1.0m;
				rate.Factory.Save();

				Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();

				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 1.0m);
				arInvoice.AH_JH = job.PK;
				arInvoice.AH_OH = originalBranch.GB_OH_OrgProxy.ToGuid();
				arInvoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
				arInvoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef + "/A";
				InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1.0m, 50, differentBranch1.PK);
				line.AL_AC = chargeCode.PK;
				line.AL_JH = job.PK;
				line.AL_GB = arInvoice.AH_GB;
				TestObjectCreator.CreateJobCharge(line, job, chargeCode, TestObjectCreator.AUD);
			}

			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "IMP", "AIR", preference: "HAR");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "EXP", "AIR", preference: "TDR");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "DOM", "AIR", preference: "CER");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "OTH", "AIR", preference: "TDR");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "IMP", "SEA", preference: "HAR");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "EXP", "SEA", preference: "HDR");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "DOM", "SEA", preference: "TDR");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "OTH", "SEA", preference: "TDR");
			GlbCompany.CurrentCompany.Factory.Save();

			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			APInvoice aPInvoice = (APInvoice)converter.ConvertToAP(arInvoice, true);

			AssertEquals("OTH, AIR -> TDR", 1.5m, aPInvoice.AH_ExchangeRate);

			consol.JK_RL_NKLoadPort = "USLAX";
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort = "AUMEL";
			Factory.Save();
			AssertEquals("IMP, AIR -> HAR", 2.5m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort = "AUMEL";
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();
			AssertEquals("EXP, SEA -> HDR", 3.5m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			shipment.JS_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUMEL";
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort = "AUSYD";
			Factory.Save();
			AssertEquals("DOM, AIR -> CER", 4.5m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			AssertEquals("OTH, AIR -> TDR", 1.5m, aPInvoice.AH_ExchangeRate);

			TestObjectCreator.USD.ExchangeRates.DeleteAll();

			rate = TestObjectCreator.USD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Now.AddDays(-30).Date;
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(-29);
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 3.5m;

			AssertEquals(1, TestObjectCreator.USD.ExchangeRates.Count);

			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort = "NZAKL";
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();
			AssertEquals("OTH, AIR -> TDR", 0m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			consol.JK_RL_NKLoadPort = "AUMEL";
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort = "AUMEL";
			Factory.Save();
			AssertEquals("IMP, AIR -> HAR", 0m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort = "AUMEL";
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();
			AssertEquals("EXP, SEA -> HDR", 0m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort = "NZAKL";
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();
			AssertEquals("OTH, AIR -> TDR", 3.5m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			consol.JK_RL_NKLoadPort = "AUMEL";
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort = "AUMEL";
			Factory.Save();
			AssertEquals("IMP, AIR -> HAR", 3.5m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort = "AUMEL";
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();
			AssertEquals("EXP, SEA -> HDR", 3.5m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestCorrectLocalAmountOnARCreditNote()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.JK_UniqueConsignRef = "C00001";
			consol.JK_TransportMode = "AIR";

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			RefExchangeRate rate = TestObjectCreator.USD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Now.AddDays(-10).Date;
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(10);
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 1.5861m;

			AccChargeCode chargeCode = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			chargeCode.AC_GC = differentCompany.PK;

			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
			ARInvoice arInvoice = null;

			Job job1, job2;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				job1 = TestObjectCreator.CreateJob(shipment1, false);
				job2 = TestObjectCreator.CreateJob(shipment2, false);

				ApportionmentListing apps = new ApportionmentListing(Factory, consol);
				JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
				freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				freightCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
				freightCost.E6_ExchangeRate = 1m;
				freightCost.E6_OSCostAmount = 1100m;
				freightCost.E6_ApportionmentMethod = "MAN";
				freightCost.ApportionmentCharges[0].JR_OSCostAmt = 600M;
				freightCost.ApportionmentCharges[1].JR_OSCostAmt = 500M;

				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 1.0m);
				arInvoice.AH_JH = ZGuid.Empty;
				arInvoice.AH_OH = originalBranch.GB_OH_OrgProxy.ToGuid();
				arInvoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
				arInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef + "/A";
				InvoicingLineBase line1 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 1.0m, 600m, differentBranch1.PK);
				line1.AL_AC = chargeCode.PK;
				line1.AL_JH = job1.PK;
				line1.AL_GB = arInvoice.AH_GB;
				var charge1 = TestObjectCreator.CreateJobCharge(line1, job1, chargeCode, TestObjectCreator.USD);
				InvoicingLineBase line2 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 1.0m, 500m, differentBranch1.PK);
				line2.AL_AC = chargeCode.PK;
				line2.AL_JH = job2.PK;
				line2.AL_GB = arInvoice.AH_GB;
				var charge2 = TestObjectCreator.CreateJobCharge(line2, job2, chargeCode, TestObjectCreator.USD);
			}
			job1.ExchangeRates[0].JF_BaseRate = 1m; //assign non-zero to allow saving
			job2.ExchangeRates[0].JF_BaseRate = 1m; //assign non-zero to allow saving
			Factory.Save();

			APInvoice apInvoice = (APInvoice)converter.ConvertToAP(arInvoice, true);
			Factory.Save();

			ARCreditNote arCreditNote;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				((IReversing)arInvoice).GenerateReverseTransaction(true);
				arCreditNote = (ARCreditNote)arInvoice.ReverseInvoice;

				TestObjectCreator.CreateJobCharge(arCreditNote.Lines[0], job1, chargeCode, TestObjectCreator.USD);
				TestObjectCreator.CreateJobCharge(arCreditNote.Lines[1], job2, chargeCode, TestObjectCreator.USD);
			}

			Factory.Save();

			APCreditNote apCreditNote = (APCreditNote)converter.ConvertToAP(arCreditNote, true);

			AssertEquals("AH_OSExTaxAmount", 1100m, apCreditNote.AH_OSExTaxAmount);
			AssertEquals("AH_ExchangeRate", 1.5861m, apCreditNote.AH_ExchangeRate);
			AssertEquals("AH_LocalExTaxAmount should be properly rounded", 693.52m, apCreditNote.AH_LocalExTaxAmount);
		}

		public void TestGatewayAgentConsol()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			chargeCode.AC_GC = differentCompany.PK;
			Factory.Save();

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.AUD, "SEL", 1.0m, ZDateTime.Now.Date, ZDateTime.Now.AddDays(1));

			var converter = new UnapprovedTransactionConverter(Factory);
			var arInvoice = GetARInvoiceFromConsol(true, chargeCode, "C001", "S001", "INV001");
			var result = converter.ConvertToAP(arInvoice, true);
			AssertEquals("There should be no consol cost.", 0, result.ConsolCosting.ConsolCosts.Count);

			arInvoice = GetARInvoiceFromConsol(false, chargeCode, "C002", "S002", "INV002");
			result = converter.ConvertToAP(arInvoice, true);
			AssertEquals("There should be no consol cost.", 1, result.ConsolCosting.ConsolCosts.Count);
		}

		ARInvoice GetARInvoiceFromConsol(bool isGateway, AccChargeCode chargeCode, string consolNum, string shipmentNum, string transactionNum)
		{
			ARInvoice arInvoice = null;
			var consol = isGateway ? TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", receivingGatewayCompany: GlbCompany.CurrentCompany) : TestObjectCreator.CreateConsol("USLAX", "AUMEL", consolNum, true);
			var shipment = TestObjectCreator.CreateShipment(shipmentNum, consol);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1.5m, ZDateTime.Now.Date, ZDateTime.Now.AddDays(1));

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job = TestObjectCreator.CreateJob(shipment);

				arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>(transactionNum, TestObjectCreator.USD, 1.0m, originalBranch.OrgProxy);
				arInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef + "/A";

				var line = TestObjectCreator.CreateInvoiceLine(arInvoice, job, chargeCode, 50, TestObjectCreator.AUD, 1.0m);
				TestObjectCreator.CreateJobCharge(line, job, chargeCode, TestObjectCreator.AUD);
				Factory.Save();
			}
			return arInvoice;
		}

		[DisableZeroExchangeRateOverriding]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestExchangeRatesForConsolInvoices()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.JK_UniqueConsignRef = "C00001";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_TransportMode = "AIR";
			consol.Transports.ArrivalTransport.JW_ATA = ZDateTime.Now.AddDays(-10);
			consol.Transports.DepartureTransport.JW_ATD = ZDateTime.Now.AddDays(-20);

			var shipment = consol.Shipments.AddNew();

			var rate = TestObjectCreator.USD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Now.Date;
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(1);
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 1.5m;

			rate = TestObjectCreator.USD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Now.AddDays(-10).Date;
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(-9);
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 2.5m;

			rate = TestObjectCreator.USD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Now.AddDays(-20).Date;
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(-19);
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 3.5m;

			var chargeCode = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			chargeCode.AC_GC = differentCompany.PK;

			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			var converter = new UnapprovedTransactionConverter(Factory);
			ARInvoice arInvoice = null;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				rate = TestObjectCreator.AUD.ExchangeRates.AddNew();
				rate.RE_StartDate = ZDateTime.Now.Date;
				rate.RE_ExpiryDate = ZDateTime.Now.AddDays(1);
				rate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;
				rate.RE_SellRate = 1.0m;
				rate.Factory.Save();

				var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();

				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 1.0m);
				arInvoice.AH_JH = ZGuid.Empty;
				arInvoice.AH_OH = originalBranch.GB_OH_OrgProxy.ToGuid();
				arInvoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
				arInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef + "/A";
				var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1.0m, 50, differentBranch1.PK);
				line.AL_AC = chargeCode.PK;
				line.AL_JH = job.PK;
				line.AL_GB = arInvoice.AH_GB;
				TestObjectCreator.CreateJobCharge(line, job, chargeCode, TestObjectCreator.AUD);
			}

			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "IMP", "AIR", preference: "HAR");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "EXP", "AIR", preference: "TDR");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("GCN", "OTH", "AIR", preference: "TDR");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "IMP", "SEA", preference: "HAR");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("GCN", "EXP", "SEA", preference: "HDR");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "DOM", "SEA", preference: "TDR");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "OTH", "SEA", preference: "TDR");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("GCN", "IMP", "AIR", preference: "HAR");
			GlbCompany.CurrentCompany.Factory.Save();

			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			APInvoice aPInvoice = (APInvoice)converter.ConvertToAP(arInvoice, true);

			AssertEquals("OTH, AIR -> TDR", 1.5m, aPInvoice.AH_ExchangeRate);

			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "USLAX";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			Factory.Save();

			AssertEquals("IMP, AIR -> HAR", 2.5m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			gatewayAgentPort.O5_PortOrCountry = "AUMEL";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			Factory.Save();
			AssertEquals("EXP, SEA -> HDR", 3.5m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			Factory.Save();
			AssertEquals("DOM, AIR -> TDR", 1.5m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			AssertEquals("OTH, AIR -> TDR", 1.5m, aPInvoice.AH_ExchangeRate);

			TestObjectCreator.USD.ExchangeRates.DeleteAll();

			rate = TestObjectCreator.USD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Now.AddDays(-30).Date;
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(-29);
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 3.5m;

			AssertEquals(1, TestObjectCreator.USD.ExchangeRates.Count);

			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();
			AssertEquals("OTH, AIR -> TDR", 0m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayAgentPort.O5_PortOrCountry = "AUMEL";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			Factory.Save();
			AssertEquals("DOM, AIR -> TDR", 0m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayAgentPort.O5_PortOrCountry = "AUMEL";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			Factory.Save();
			AssertEquals("EXP, SEA -> HDR", 0m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayAgentPort.O5_PortOrCountry = "NZAKL";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			Factory.Save();
			AssertEquals("OTH, AIR -> TDR", 3.5m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayAgentPort.O5_PortOrCountry = "AUMEL";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			Factory.Save();
			AssertEquals("DOM, AIR -> TDR", 3.5m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);

			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayAgentPort.O5_PortOrCountry = "AUMEL";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			Factory.Save();
			AssertEquals("EXP, SEA -> HDR", 3.5m, converter.ConvertToAP(arInvoice, true).AH_ExchangeRate);
		}

		public void TestDoesConvertTOUACallOnSaveTwice()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();
			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
			invoice = converter.ConvertToUA(invoice);
			Factory.Save();
			AssertEquals("00001000", invoice.AH_ConsolidatedInvoiceRef);
			AssertEquals(1, Factory.GetBizOsForPK(invoice.PK.ToGuid()).Length);
		}

		public void TestConvertConsolARInvoiceToAPWhenJobIsNotCreated()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S00001005", "AUSYD", "NZAKL", consol);
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				var job1 = TestObjectCreator.CreateJob(shipment1, false);
				var job2 = TestObjectCreator.CreateJob(shipment2, false);
				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M);
				arInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
				arInvoice.AH_OH = originalBranch.GB_OH_OrgProxy.ToGuid();
				var line1 = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Revenue, arInvoice, job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 100M);
				AssertEquals("Precondition: line1.AL_GB", differentBranch1.PK, line1.AL_GB);
				var line2 = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Revenue, arInvoice, job2, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 100M);
				AssertEquals("Precondition: line1.AL_GB", differentBranch1.PK, line1.AL_GB);
				var charge1 = TestObjectCreator.CreateCharge(line1);
				var charge2 = TestObjectCreator.CreateCharge(line2);
				Factory.Save();
			}

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
			converter.Candidates.Load();
			AssertEquals("Precondition: candidates collection should have loaded the new AR invoice", 1, converter.Candidates.Count);

			var candidateARInvoice = (ARInvoice)converter.Candidates[0];
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, candidateARInvoice.AH_GB.ToGuid(), candidateARInvoice.AH_GE.ToGuid()))
			{
				candidateARInvoice.RunPreSaveValidation();
				AssertNoErrors("Test not implemented correctly, this transaction has the following errors", candidateARInvoice);
				AssertNoRowErrors("Test not implemented correctly, this transaction has the following errors", candidateARInvoice);
				AssertEquals("Precondition: IsConsolInvoice", true, candidateARInvoice.IsConsolInvoice);
			}

			InvoicingBase firstInvoice = converter.ConvertToAP(candidateARInvoice, true);
			AssertNotNull(firstInvoice);

			converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
			converter.Candidates.Load();
			candidateARInvoice = (ARInvoice)converter.Candidates[0];
			firstInvoice = converter.ConvertToAP(candidateARInvoice, false);
			AssertNotNull(firstInvoice);

			converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
			converter.Candidates.Load();
			candidateARInvoice = (ARInvoice)converter.Candidates[0];
			try
			{
				converter.ConvertToAP(candidateARInvoice, false);
				Assert("MutexException shuold be raise.", false);
			}
			catch (JobCreationException ex)
			{
				AssertNotNull("Unable to load or create JobHeader for transaction line.\r\nYou have created the job S00001004 on another form, but haven't saved it yet.\r\nPlease close or save other forms that use job S00001004 to continue.",
					ex.Message);
				ExceptionReporterTestListener.Instance.Clear();
				firstInvoice.ReleaseAllMutexOnInvoice();
			}
		}

		public void TestConvertToAPReleaseJobHeaderMutex()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S00001005", "AUSYD", "NZAKL", consol);
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				var job1 = TestObjectCreator.CreateJob(shipment1, false);
				var job2 = TestObjectCreator.CreateJob(shipment2, false);
				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M);
				arInvoice.AH_OH = originalBranch.GB_OH_OrgProxy.ToGuid();
				var line1 = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Revenue, arInvoice, job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 100M);
				AssertEquals("Precondition: line1.AL_GB", differentBranch1.PK, line1.AL_GB);
				var line2 = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Revenue, arInvoice, job2, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 100M);
				AssertEquals("Precondition: line1.AL_GB", differentBranch1.PK, line1.AL_GB);
				var charge1 = TestObjectCreator.CreateCharge(line1);
				var charge2 = TestObjectCreator.CreateCharge(line2);
				Factory.Save();
			}

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
			converter.Candidates.Load();
			AssertEquals("Precondition: candidates collection should have loaded the new AR invoice", 1, converter.Candidates.Count);
			var candidateARInvoice = (ARInvoice)converter.Candidates[0];

			InvoicingBase invoice = converter.ConvertToAP(candidateARInvoice, false);
			ZGlobalMutex mutex1 = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipment1.PK + "_" + GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("mutex should be locked", true, mutex1.IsLocked);
			ZGlobalMutex mutex2 = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipment2.PK + "_" + GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("mutex should be locked", true, mutex2.IsLocked);

			invoice.ReleaseAllMutexOnInvoice();

			invoice = converter.ConvertToAP(candidateARInvoice, true);
			mutex1 = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipment1.PK + "_" + GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("mutex should be released", false, mutex1.IsLocked);
			mutex2 = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipment2.PK + "_" + GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("mutex should be released", false, mutex2.IsLocked);
		}

		public void TestAuthorisationLevelReleasesJobHeaderMutexOnUninvoicedShipment()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S00001005", "AUSYD", "NZAKL", consol);
			var shipment3 = TestObjectCreator.CreateShipment("S00001006", "AUSYD", "NZAKL", consol);
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				var job = TestObjectCreator.CreateJob(shipment1, false);
				var job2 = TestObjectCreator.CreateJob(shipment2, false);
				shipment1.ShipmentJobHeader.JH_OA_AgentCollectAddr = originalBranch.OrgProxy.MainAddress.PK;
				shipment2.ShipmentJobHeader.JH_OA_AgentCollectAddr = originalBranch.OrgProxy.MainAddress.PK;

				var apportionments = new ApportionmentListing(Factory, consol);
				var consolCost = apportionments.CostsCollection.TryAddNew();
				consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				consolCost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				consolCost.E6_OSCostAmount = 11m;
				consolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
				consolCost.ApportionmentCharges[0].JR_OH_SellAccount = originalBranch.OrgProxy.PK;
				consolCost.ApportionmentCharges[1].JR_OH_SellAccount = originalBranch.OrgProxy.PK;
				Factory.Save();

				var jobCollection = new[] { job, job2 };
				var apportionmentListing = new ApportionmentListing(Factory, consol);
				var postManager = new ConsolInvoicingPostManager(Factory, jobCollection, consol, apportionmentListing);
				var chargesToPost = new JobInvoicing.Posting.IReceivablesPostingChargeCollection();
				chargesToPost.Key = new JobInvoicing.Posting.PostingChargeKey(originalBranch.OrgProxy.PK, "", consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0);
				chargesToPost.Add(job.Charges[0]);
				chargesToPost.Add(job2.Charges[0]);
				postManager.Poster.Post(chargesToPost);

				Factory.Save();
			}

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
			converter.Candidates.Load();
			AssertEquals("Precondition: candidates collection should have loaded the new AR invoice", 1, converter.Candidates.Count);
			var candidateARInvoice = (ARInvoice)converter.Candidates[0];
			Assert(!candidateARInvoice.AH_ConsolidatedInvoiceRef.IsEmpty);
			Assert(candidateARInvoice.AH_JH.IsEmpty);
			Assert("Is consol invoice", candidateARInvoice.IsConsolInvoice);

			var letsQueryAuthLevel = candidateARInvoice.AuthorisationLevel;

			ZGlobalMutex mutex1 = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipment1.PK + "_" + GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("mutex should not be locked", false, mutex1.IsLocked);
			ZGlobalMutex mutex2 = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipment2.PK + "_" + GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("mutex should not be locked", false, mutex2.IsLocked);
			ZGlobalMutex mutex3 = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipment3.PK + "_" + GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("mutex should not be locked", false, mutex3.IsLocked);
		}

		[TestDate(2012, 02, 27)]
		public void TestConvertToAPWhenPeriodClosed()
		{
			PeriodManager periodManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2012, 1, 1));
			Period firstPeriod = periodManager.Periods[0];
			UAInvoice uAInvoice = (UAInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(UAInvoice), "UAINV", TestObjectCreator.AUD, 1.0m, 555m, 0, 555m, 0);
			uAInvoice.AH_PostDate = new ZDateTime(2012, 1, 15);
			uAInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			uAInvoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			uAInvoice.Lines[0].AL_JH = TestObjectCreator.Job1.PK;
			TestObjectCreator.CreateCharge(uAInvoice.Lines[0]);
			Factory.Save();

			AssertEquals("Period Not Closed when Posting UA Invoice", false, firstPeriod.AM_IsSubLedgerClosed);

			firstPeriod.AM_IsSubLedgerClosed = true;
			Factory.Save();

			AssertEquals("Period Closed", true, firstPeriod.AM_IsSubLedgerClosed);
			AssertEquals(new ZDateTime(2012, 01, 15), uAInvoice.AH_PostDate);

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
			converter.Candidates.Load();
			InvoicingBase convertedInvoice = converter.ConvertToAP(uAInvoice, false);

			AssertEquals(new ZDateTime(2012, 02, 27), convertedInvoice.AH_PostDate);
		}

		public void TestUnapprovedTransactionCandidateCollectionValidationSuspender()
		{
			var converter = new UnapprovedTransactionConverter(Factory);
			Assert("Validation should not be suspended", !converter.IsUnapprovedTransactionCandidateCollectionValidationSuspended);
			using (converter.GetUnapprovedTransactionCandidateCollectionValidationSuspender())
			{
				using (converter.GetUnapprovedTransactionCandidateCollectionValidationSuspender())
				{
					Assert("Validation should be suspended (suspender count = 2)", converter.IsUnapprovedTransactionCandidateCollectionValidationSuspended);
				}
				Assert("Validation should be suspended (suspender count = 1)", converter.IsUnapprovedTransactionCandidateCollectionValidationSuspended);
			}
			Assert("Validation should not be suspended", !converter.IsUnapprovedTransactionCandidateCollectionValidationSuspended);
		}

		public void TestConvertToAPUnsafeDoesNotAddIncorrectValidationsToConsolCosts()
		{
			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, differentCompany);
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001000");
			var forwardingShipment1 = consol.Shipments.AddNew();
			forwardingShipment1.JS_UniqueConsignRef = "S00002000";
			var forwardingShipment2 = consol.Shipments.AddNew();
			forwardingShipment2.JS_UniqueConsignRef = "S00002001";
			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = chargeCode1.PK;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 600m;
			Factory.Save();

			var jobCharge1Amount = 100m;
			var jobCharge2Amount = 500m;
			InvoicingBase transaction = null;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				var agent = TestObjectCreator.Agent;
				agent.CompanyData.OB_IsDebtor = true;
				agent.CompanyData.SetARTaxApplicable(false);

				var consolInNewFactory = Factory.Load<ForwardingConsol>(consol.PK);
				var shipment1InNewFactory = consolInNewFactory.Shipments[0];
				var shipment2InNewFactory = consolInNewFactory.Shipments[1];

				var localClient = Factory.NewWithValidTestData<OrgHeader>();
				Job job1 = createJob(Factory, "J00001000", differentBranch1.PK, originalDepartment.PK, localClient, agent);
				job1.JH_ParentID = shipment1InNewFactory.PK;
				job1.JH_ParentTableCode = "JS";
				Job job2 = createJob(Factory, "J00001001", differentBranch1.PK, originalDepartment.PK, localClient, agent);
				job2.JH_ParentID = shipment2InNewFactory.PK;
				job2.JH_ParentTableCode = "JS";
				Factory.Save();

				var jobCharge1 = TestObjectCreator.CreateCharge(job1, chargeCode2, "", null, jobCharge1Amount, null, "", null, jobCharge1Amount, agent);
				jobCharge1.JR_GB = differentBranch1.PK;
				jobCharge1.JR_GE = originalDepartment.PK;

				var jobCharge2 = TestObjectCreator.CreateCharge(job2, chargeCode2, "", null, jobCharge2Amount, null, "", null, jobCharge2Amount, agent);
				jobCharge2.JR_GB = differentBranch1.PK;
				jobCharge2.JR_GE = originalDepartment.PK;
				Factory.Save();

				var jobs = new Job[] { job1, job2 };
				var transactionCreator = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(Factory, consol));
				var transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Agent);
				InvoicingBase[] arInvoices = transactions.GetAllARInvoicesAndCreditNotes();
				transaction = arInvoices[0];
				Factory.Save();
			}

			var converter = new UnapprovedTransactionConverter(Factory);
			var convertedAPInvoice = converter.ConvertToAPUnsafe(transaction, Factory, false);
			AssertNotNull("Precondition: Converted Invoice should not be null", convertedAPInvoice);
			AssertEquals("Precondition: One Consol Cost should exist on Consol", 1, convertedAPInvoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals("Sum of the apportioned amounts is equal to the consol cost amount", (jobCharge1Amount + jobCharge2Amount), convertedAPInvoice.ConsolCosting.ConsolCosts[0].E6_OSCostAmount);
			AssertEquals("Unapportioned amount is zero", 0m, convertedAPInvoice.ConsolCosting.ConsolCosts[0].UnApportionedAmount);
			AssertEquals("Consol Cost must not have errors", false, convertedAPInvoice.ConsolCosting.NotificationsIncludingChildren.HasErrors());
		}

		[TestDate(2011, 11, 29, 12, 00, 00)]
		public void TestImportingAPInvoicesFromSisterCompaniesWithInvalidGBGE()
		{
			var creator = new TestObjectCreator(Factory);
			var converter = new UnapprovedTransactionConverter(Factory);

			var shipment = Factory.New<ForwardingShipment>();
			var testJob = creator.CreateJob(shipment, false);

			var transaction = creator.CreateARInvoice<ARInvoice>("100101", creator.AUD, 1m, creator.ABIGAS);
			transaction.AH_JH = testJob.PK;
			var line1 = creator.CreateARInvoiceLine(transaction, testJob, creator.CC4, creator.AUD, 1.0m, "HIHIHI", 10000);
			line1.AL_GB = originalBranch.PK;
			line1.AL_GE = originalDepartment.PK;
			creator.CreateJobCharge(line1, testJob, creator.CC4, creator.AUD);
			line1.AL_JH = testJob.PK;

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = creator.AALSHI.PK;
			creator.NonCurrentCompany.GC_OH_OrgProxy = creator.ABIGAS.PK;

			Factory.Save();

			using (new TemporaryUserContext { DepartmentPK = creator.FISDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				var testJobInAnotherCompany = creator.CreateJob(shipment, false);
				//the GB/GE set to invalid values due to certain defaulting rules.
				testJobInAnotherCompany.JH_GB = ZGuid.Empty;
				testJobInAnotherCompany.JH_GE = ZGuid.Empty;

				AssertEquals(ZGuid.Empty, testJobInAnotherCompany.JH_GB);
				AssertEquals(ZGuid.Empty, testJobInAnotherCompany.JH_GE);

				var resultofconverter = converter.ConvertToAPUnsafe(transaction, Factory, false);

				CombineAssertions(delegate
				{
					AssertEquals(GlbBranch.CurrentBranch.PK, testJobInAnotherCompany.JH_GB);
					AssertEquals(GlbDepartment.CurrentDepartment.PK, testJobInAnotherCompany.JH_GE);

					AssertEquals("Branch on original Transaction", line1.AL_GB, transaction.AH_GB);
					AssertEquals("Department on original Transaction", line1.AL_GE, transaction.AH_GE);
					AssertEquals("Branch on result Transaction", testJobInAnotherCompany.JH_GB.ToGuid(), resultofconverter.AH_GB);
					AssertEquals("Department on result Transaction", testJobInAnotherCompany.JH_GE.ToGuid(), resultofconverter.AH_GE);
					AssertEquals("Branch on result line", testJobInAnotherCompany.JH_GB.ToGuid(), resultofconverter.Lines[0].AL_GB);
					AssertEquals("Department on result line", testJobInAnotherCompany.JH_GE.ToGuid(), resultofconverter.Lines[0].AL_GE);
				});
			}
		}

		[TestDate(2011, 11, 29, 12, 00, 00)]
		public void TestImportingAPInvoicesFromSisterCompaniesWithValidGBGE()
		{
			var converter = new UnapprovedTransactionConverter(Factory);

			// Job related AR invoice
			var shipment = TestObjectCreator.CreateShipment("S001");
			var testJob = TestObjectCreator.CreateJob(shipment, false);

			var jobARInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "JobINV", organisation: differentBranchOrgProxy);
			jobARInvoice.AH_JH = testJob.PK;
			var line = TestObjectCreator.CreateInvoiceLine(jobARInvoice, testJob, TestObjectCreator.CC4, 10000);
			line.AL_GB = originalBranch.PK;
			line.AL_GE = originalDepartment.PK;
			TestObjectCreator.CreateCharge(line);

			// Consol related AR invoice
			var consol = TestObjectCreator.CreateConsol("AUSYD", "SGSIN", "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S002", "AUSYD", "SGSIN", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S003", "AUSYD", "SGSIN", consol);

			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);

			var consolRelatedARInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "ConsolINV", organisation: differentBranchOrgProxy);
			consolRelatedARInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;

			var line1 = TestObjectCreator.CreateInvoiceLine(consolRelatedARInvoice, job1, TestObjectCreator.CC1, 100M);
			line1.AL_GB = originalBranch.PK;
			line1.AL_GE = originalDepartment.PK;
			TestObjectCreator.CreateCharge(line1);
			var line2 = TestObjectCreator.CreateInvoiceLine(consolRelatedARInvoice, job2, TestObjectCreator.CC1, 100M);
			line2.AL_GB = originalBranch.PK;
			line2.AL_GE = originalDepartment.PK;
			TestObjectCreator.CreateCharge(line2);

			var periodicARInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "PeriodicINV", organisation: differentBranchOrgProxy);
			var periodicLine1 = TestObjectCreator.CreateInvoiceLine(periodicARInvoice, job1, TestObjectCreator.CC1, 100M);
			periodicLine1.AL_GB = originalBranch.PK;
			periodicLine1.AL_GE = originalDepartment.PK;
			periodicLine1.AL_Sequence = 2;
			TestObjectCreator.CreateCharge(periodicLine1);
			var periodicLine2 = TestObjectCreator.CreateInvoiceLine(periodicARInvoice, job2, TestObjectCreator.CC1, 100M);
			periodicLine2.AL_GB = originalBranch.PK;
			periodicLine2.AL_GE = originalDepartment.PK;
			periodicLine2.AL_Sequence = 1;
			TestObjectCreator.CreateCharge(periodicLine2);

			// non job related AR invoice
			var nonjobARInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "MiscINV", organisation: differentBranchOrgProxy);
			var nonJobLine = TestObjectCreator.CreateInvoiceLine(nonjobARInvoice, null, TestObjectCreator.CC4, 10000);
			nonJobLine.AL_GE = TestObjectCreator.MiscDepartment.PK;

			var importBranch = TestObjectCreator.CreateNewBranch(differentCompany, "BA2");

			Factory.Save();

			using (new TemporaryUserContext { DepartmentPK = TestObjectCreator.FEADepartment.PK.ToGuid(), BranchPK = importBranch.PK.ToGuid() }.Set())
			{
				var testJobInImportCompany = TestObjectCreator.CreateJob(shipment, false);
				//the GB/GE set to invalid values due to certain defaulting rules.
				testJobInImportCompany.JH_GB = ZGuid.Empty;
				testJobInImportCompany.JH_GE = ZGuid.Empty;

				var job1InImportCompany = TestObjectCreator.CreateJob(shipment1, false);
				job1InImportCompany.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
				job1InImportCompany.JH_GE = TestObjectCreator.FESDepartment.PK;
				var job2InImportCompany = TestObjectCreator.CreateJob(shipment2, false);
				job2InImportCompany.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
				job2InImportCompany.JH_GE = TestObjectCreator.FISDepartment.PK;

				// job related
				var importedJobARInvoice = converter.ConvertToAPUnsafe(jobARInvoice, Factory, true, isAutoImport: true);
				AssertEquals("Branch on result of job related AR Transaction for auto import", differentBranch1.PK, importedJobARInvoice.AH_GB);
				AssertEquals("Department on result of job related AR Transaction for auto import: it does not default empty job branch/department", ZGuid.Empty, importedJobARInvoice.AH_GE);

				importedJobARInvoice = converter.ConvertToAPUnsafe(jobARInvoice, Factory, true);
				AssertEquals("Branch on result of job related AR Transaction for manual import", differentBranch1.PK, importedJobARInvoice.AH_GB);
				AssertEquals("Department on result of job related AR Transaction for manual import", TestObjectCreator.FEADepartment.PK, importedJobARInvoice.AH_GE);

				// consol related
				var importedConsolARInvoice = converter.ConvertToAPUnsafe(consolRelatedARInvoice, Factory, true);
				AssertEquals("Branch on result of Consol related AR Transaction for manual import", differentBranch1.PK, importedConsolARInvoice.AH_GB);
				AssertEquals("Department on result of Consol related AR Transaction for manual import", TestObjectCreator.FIADepartment.PK, importedConsolARInvoice.AH_GE);

				importedConsolARInvoice = converter.ConvertToAPUnsafe(consolRelatedARInvoice, Factory, true, isAutoImport: true);
				AssertEquals("Branch on result of job related AR Transaction for auto import", importBranch.PK, importedConsolARInvoice.AH_GB);
				AssertEquals("Department on result of Consol related AR Transaction for auto import", TestObjectCreator.FIADepartment.PK, importedConsolARInvoice.AH_GE);

				var importedperiodicARInvoice = converter.ConvertToAPUnsafe(periodicARInvoice, Factory, true);
				AssertEquals("Branch on result of periodic AR Transaction for manual import", differentBranch1.PK, importedperiodicARInvoice.AH_GB);
				AssertEquals("Department on result of periodic AR Transaction for manual import", TestObjectCreator.FISDepartment.PK, importedperiodicARInvoice.AH_GE);

				importedperiodicARInvoice = converter.ConvertToAPUnsafe(periodicARInvoice, Factory, true, isAutoImport: true);
				AssertEquals("Branch on result of periodic AR Transaction for auto import", differentBranch1.PK, importedperiodicARInvoice.AH_GB);
				AssertEquals("Department on result of periodic AR Transaction for auto import", TestObjectCreator.FISDepartment.PK, importedperiodicARInvoice.AH_GE);

				// non job related
				var importedNonJobARInvoice = converter.ConvertToAPUnsafe(nonjobARInvoice, Factory, true);
				CombineAssertions(delegate
				{
					AssertEquals("Department on result of non job related AR transaction", nonjobARInvoice.AH_GE, importedNonJobARInvoice.AH_GE);
					AssertNotEquals("Department on result of non job related AR transaction line does not go from header", importedNonJobARInvoice.AH_GE, importedNonJobARInvoice.Lines[0].AL_GE);
					AssertEquals("Department on result of non job related AR transaction line", TestObjectCreator.MiscDepartment.PK, importedNonJobARInvoice.Lines[0].AL_GE);
					AssertEquals("Branch on result of non job related AR transaction line", importedNonJobARInvoice.AH_GB, importedNonJobARInvoice.Lines[0].AL_GB);
				});
			}
		}

		[TestDate(2018, 5, 21, 12, 00, 00)]
		[DisableZeroExchangeRateOverriding]
		public void TestImportingAPInvoicesFromSisterCompaniesProducesCorrectJobCharge_SetTransactionExRate()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("CUSDSB", "Charge Code 1", Core.Constants.ChargeType.Disbursement, 100m, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1);
			var chargeCodeInImportCompany = TestObjectCreator.CreateChargeCode("CUSDSB", "Charge Code 1", Core.Constants.ChargeType.Disbursement, 100m, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1, company: differentCompany);

			// Job related AR invoice
			var shipment = TestObjectCreator.CreateShipment("S001");
			var testJob = TestObjectCreator.CreateJob(shipment, null, 0m, differentBranchOrgProxy, 0m);

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.EUR, 0.2m, differentBranchOrgProxy);
			arInvoice.AH_JH = testJob.PK;
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice, testJob, chargeCode, 1000m);
			line.AL_GB = originalBranch.PK;
			line.AL_GE = originalDepartment.PK;
			TestObjectCreator.CreateCharge(line);

			Factory.Save();

			using (new TemporaryUserContext { DepartmentPK = originalDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				var jobInImportCompany = TestObjectCreator.CreateJob(shipment, null, 0m, originalBranch.OrgProxy, 0m);
				jobInImportCompany.JH_GB = differentBranch1.PK;
				jobInImportCompany.JH_GE = originalDepartment.PK;
				jobInImportCompany.AddCurrency(TestObjectCreator.EUR, 0.8m, ExchangeRateValidLedgerEnum.AR);
				AssertEquals(1, jobInImportCompany.ExchangeRates.Count);
				jobInImportCompany.ExchangeRates[0].JF_CFXPercent = 2m;
				jobInImportCompany.ExchangeRates[0].JF_CFXMinimum = 0.20m;
				jobInImportCompany.ExchangeRates[0].JF_IsTransformed = true;

				Factory.Save();

				var converter = new UnapprovedTransactionConverter(Factory);

				var importedInvoice = converter.ConvertToAPUnsafe(arInvoice, Factory, true);
				Assert("No system ex rate to populate", importedInvoice.AH_ExchangeRate.IsEmpty);
				importedInvoice.AH_ExchangeRate = 0.8m; // Do editing before posting

				AssertEquals(0, jobInImportCompany.Charges.Count);
				AssertNoExceptionThrown(() => importedInvoice.Factory.Save());

				AssertEquals(1, jobInImportCompany.Charges.Count);
				var chargeInImportCompany = jobInImportCompany.Charges[0];
				AssertEquals(chargeCodeInImportCompany.PK, chargeInImportCompany.JR_AC);

				AssertEquals(TestObjectCreator.EUR.Code, chargeInImportCompany.JR_RX_NKCostCurrency);
				AssertEquals(1000m, chargeInImportCompany.JR_OSCostAmt);
				AssertEquals(0.8m, chargeInImportCompany.JR_OSCostExRate);
				AssertEquals(1250m, chargeInImportCompany.JR_LocalCostAmt);

				AssertEquals(TestObjectCreator.EUR.Code, chargeInImportCompany.JR_RX_NKSellCurrency);
				AssertEquals(1000m, chargeInImportCompany.JR_OSSellAmt);
				AssertEquals(0.784m, chargeInImportCompany.JR_OSSellExRate);
				AssertEquals(1275.51m, chargeInImportCompany.JR_LocalSellAmt);
			}
		}

		[TestDate(2018, 5, 21, 12, 00, 00)]
		[DisableZeroExchangeRateOverriding]
		public void TestImportingAPInvoicesFromSisterCompaniesCorrectlyUpdatesJobCharge_SetTransactionExRate()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("CUSDSB", "Charge Code 1", Core.Constants.ChargeType.Disbursement, 100m, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1);
			var chargeCodeInImportCompany = TestObjectCreator.CreateChargeCode("CUSDSB", "Charge Code 1", Core.Constants.ChargeType.Disbursement, 100m, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1, company: differentCompany);

			// Job related AR invoice
			var shipment = TestObjectCreator.CreateShipment("S001");
			var testJob = TestObjectCreator.CreateJob(shipment, false);

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.EUR, 0.2m, differentBranchOrgProxy);
			arInvoice.AH_JH = testJob.PK;
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice, testJob, chargeCode, 1000m);
			line.AL_GB = originalBranch.PK;
			line.AL_GE = originalDepartment.PK;
			TestObjectCreator.CreateCharge(line);

			Factory.Save();

			using (new TemporaryUserContext { DepartmentPK = originalDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			using (AccountingConfigurationRegistry.Instance.CreateWIPWhenCostIsGreaterThanAccrual.SetTemporaryValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var jobInImportCompany = TestObjectCreator.CreateJob(shipment, false);
				jobInImportCompany.JH_GB = differentBranch1.PK;
				jobInImportCompany.JH_GE = originalDepartment.PK;
				jobInImportCompany.AddCurrency(TestObjectCreator.EUR, 0.8m, ExchangeRateValidLedgerEnum.AR);
				AssertEquals(1, jobInImportCompany.ExchangeRates.Count);
				jobInImportCompany.ExchangeRates[0].JF_CFXPercent = 2m;
				jobInImportCompany.ExchangeRates[0].JF_CFXMinimum = 0.20m;
				jobInImportCompany.AddCurrency(TestObjectCreator.EUR, 0.7m, ExchangeRateValidLedgerEnum.AP);
				var chargeInImportCompany = TestObjectCreator.CreateCharge(jobInImportCompany, chargeCodeInImportCompany, "Test", TestObjectCreator.EUR, 123.45m, null, TestObjectCreator.EUR, 123.45m, null);
				AssertEquals("EUR", chargeInImportCompany.JR_RX_NKSellCurrency);
				AssertEquals(123.45m, chargeInImportCompany.JR_OSSellAmt);
				AssertEquals(0.784m, chargeInImportCompany.JR_OSSellExRate);
				AssertEquals(157.46m, chargeInImportCompany.JR_LocalSellAmt);

				Factory.Save();

				var converter = new UnapprovedTransactionConverter(Factory);

				var importedInvoice = converter.ConvertToAPUnsafe(arInvoice, Factory, true);
				Assert("No system ex rate to populate", importedInvoice.AH_ExchangeRate.IsEmpty);
				importedInvoice.AH_ExchangeRate = 0.8m; // Do editing before posting

				AssertNoExceptionThrown(() => importedInvoice.Factory.Save());

				AssertEquals(1, jobInImportCompany.Charges.Count);
				chargeInImportCompany = jobInImportCompany.Charges[0];
				AssertEquals(chargeCodeInImportCompany.PK, chargeInImportCompany.JR_AC);

				AssertEquals("EUR", chargeInImportCompany.JR_RX_NKCostCurrency);
				AssertEquals(1000m, chargeInImportCompany.JR_OSCostAmt);
				AssertEquals(0.8m, chargeInImportCompany.JR_OSCostExRate);
				AssertEquals(1250m, chargeInImportCompany.JR_LocalCostAmt);

				AssertEquals("EUR", chargeInImportCompany.JR_RX_NKSellCurrency);
				AssertEquals(965.18m, chargeInImportCompany.JR_OSSellAmt);
				AssertEquals(0.784m, chargeInImportCompany.JR_OSSellExRate);
				AssertEquals(1231.10m, chargeInImportCompany.JR_LocalSellAmt);
			}
		}

		[TestDate(2018, 5, 21, 12, 00, 00)]
		[DisableZeroExchangeRateOverriding]
		public void TestImportingAPInvoicesFromSisterCompaniesProducesCorrectJobCharge_SetUseJobExRate()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("CUSDSB", "Charge Code 1", Core.Constants.ChargeType.Disbursement, 100m, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1);
			var chargeCodeInImportCompany = TestObjectCreator.CreateChargeCode("CUSDSB", "Charge Code 1", Core.Constants.ChargeType.Disbursement, 100m, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1, company: differentCompany);

			// Job related AR invoice
			var shipment = TestObjectCreator.CreateShipment("S001");
			var testJob = TestObjectCreator.CreateJob(shipment, false);

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.EUR, 0.8m, differentBranchOrgProxy);
			arInvoice.AH_JH = testJob.PK;
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice, testJob, chargeCode, 1000m);
			line.AL_GB = originalBranch.PK;
			line.AL_GE = originalDepartment.PK;
			TestObjectCreator.CreateCharge(line);
			var orgProxyPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			Factory.Save();

			using (new TemporaryUserContext { DepartmentPK = originalDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				var jobInImportCompany = TestObjectCreator.CreateJob(shipment, false);
				jobInImportCompany.JH_GB = differentBranch1.PK;
				jobInImportCompany.JH_GE = originalDepartment.PK;
				jobInImportCompany.AddCurrency(TestObjectCreator.EUR, 0.8m, ExchangeRateValidLedgerEnum.AR);
				AssertEquals(1, jobInImportCompany.ExchangeRates.Count);
				jobInImportCompany.ExchangeRates[0].JF_CFXPercent = 2m;
				jobInImportCompany.ExchangeRates[0].JF_CFXMinimum = 0.20m;
				jobInImportCompany.ExchangeRates[0].JF_IsTransformed = true;
				jobInImportCompany.AddCurrency(TestObjectCreator.EUR, 0.6m, orgProxyPK, ExchangeRateValidLedgerEnum.AP);

				Factory.Save();

				var converter = new UnapprovedTransactionConverter(Factory);

				var importedInvoice = converter.ConvertToAPUnsafe(arInvoice, Factory, true);
				Assert("No system ex rate to populate", importedInvoice.AH_ExchangeRate.IsEmpty);
				importedInvoice.AH_PostedToEFT = true; // set Use Job Ex Rate
				AssertEquals(0.599999m, importedInvoice.AH_ExchangeRate);

				AssertEquals(0, jobInImportCompany.Charges.Count);
				AssertNoExceptionThrown(() => importedInvoice.Factory.Save());

				AssertEquals(1, jobInImportCompany.Charges.Count);
				var chargeInImportCompany = jobInImportCompany.Charges[0];
				AssertEquals(chargeCodeInImportCompany.PK, chargeInImportCompany.JR_AC);

				AssertEquals("EUR", chargeInImportCompany.JR_RX_NKCostCurrency);
				AssertEquals(1000m, chargeInImportCompany.JR_OSCostAmt);
				AssertEquals(0.6m, chargeInImportCompany.JR_OSCostExRate);
				AssertEquals(1666.67m, chargeInImportCompany.JR_LocalCostAmt);

				AssertEquals("EUR", chargeInImportCompany.JR_RX_NKSellCurrency);
				AssertEquals(1000m, chargeInImportCompany.JR_OSSellAmt);
				AssertEquals(0.784m, chargeInImportCompany.JR_OSSellExRate);
				AssertEquals(1275.51m, chargeInImportCompany.JR_LocalSellAmt);
			}
		}

		public void TestImportingAPInvoiceFromSisterCompanyConsolARInvoiceCreatedWithNegativeCost()
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			var converter = new UnapprovedTransactionConverter(Factory);

			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);

			var creditor = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.GB_OH_OrgProxy);
			creditor.CompanyData.OB_IsCreditor = true;
			creditor.CompanyData.SetAPTaxApplicable(true);

			TestObjectCreator.AALSHI.OH_IsDebtor = true;
			TestObjectCreator.AALSHI.OH_IsCreditor = true;
			TestObjectCreator.ZECTRA.OH_IsDebtor = true;
			TestObjectCreator.ZECTRA.OH_IsCreditor = true;
			TestObjectCreator.ZECTRA.CompanyData.OB_APVATConfig = "DEF";

			var consol = TestObjectCreator.CreateConsol(TestObjectCreator.AALSHI.OH_RL_NKClosestPort, TestObjectCreator.ZECTRA.OH_RL_NKClosestPort, "C000001");

			TestObjectCreator.AALSHI.OH_IsForwarder = true;
			TestObjectCreator.ZECTRA.OH_IsForwarder = true;
			consol.JK_OA_SendingForwarderAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = TestObjectCreator.ZECTRA.MainAddress.PK;

			var forwardingShipment = consol.Shipments.AddNew();
			forwardingShipment.JS_UniqueConsignRef = "S00001000";
			forwardingShipment.JS_ReleaseType = "SWB";
			forwardingShipment.JS_TransportMode = "AIR";
			forwardingShipment.JS_PackingMode = "LSE";

			var job = TestObjectCreator.CreateJob(forwardingShipment);
			job.JH_JobNum = "J00001001";
			job.AgentCollectPK = TestObjectCreator.ZECTRA.PK;

			var apps = new ApportionmentListing(Factory, consol);
			var cost1 = apps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = chargeCode1.PK;
			cost1.E6_ApportionmentMethod = "SHP";
			cost1.E6_OSCostAmount = -250.00m;
			cost1.E6_LocalCostAmount = -250.00m;
			cost1.E6_IsForCollectInvoice = true;
			cost1.E6_InvoiceNum = "Test1";
			cost1.E6_PPDCLT = "ALL";
			cost1.E6_ApportionToRelatedShipments = true;
			cost1.E6_InvoiceDate = DateTime.Today;
			cost1.E6_PaymentDate = DateTime.Today;
			cost1.E6_OH_Creditor = TestObjectCreator.ZECTRA.PK;

			var cost2 = apps.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = chargeCode2.PK;
			cost2.E6_ApportionmentMethod = "SHP";
			cost2.E6_OSCostAmount = 100.00m;
			cost2.E6_LocalCostAmount = 100.00m;
			cost2.E6_IsForCollectInvoice = true;
			cost2.E6_InvoiceNum = "Test1";
			cost2.E6_PPDCLT = "ALL";
			cost2.E6_ApportionToRelatedShipments = true;
			cost2.E6_InvoiceDate = DateTime.Today;
			cost2.E6_PaymentDate = DateTime.Today;
			cost2.E6_OH_Creditor = TestObjectCreator.ZECTRA.PK;

			Factory.Save();

			var jobCharge1 = job.Charges[0];
			jobCharge1.JR_AC = chargeCode1.PK;
			jobCharge1.JR_OSSellAmt = 0m;
			jobCharge1.JR_LocalSellAmt = 0m;
			jobCharge1.JR_OH_SellAccount = TestObjectCreator.ZECTRA.PK;

			var jobCharge2 = job.Charges[1];
			jobCharge2.JR_AC = chargeCode2.PK;
			jobCharge2.JR_OSSellAmt = 0m;
			jobCharge2.JR_LocalSellAmt = 0m;
			jobCharge2.JR_OH_SellAccount = TestObjectCreator.ZECTRA.PK;

			Factory.Save();

			var jobs = new Job[] { job };
			var postingManager = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);
			postingManager.CreateTransactions(JobInvoicingPostingOption.All);

			Factory.Save();

			var arTransactions = Factory.Load<ARInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, "AR").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			var arTransaction = arTransactions[0];

			var convertedAPInvoice = converter.ConvertToAP(arTransaction, false);

			AssertNotNull(convertedAPInvoice);
			AssertEquals(2, convertedAPInvoice.Lines.Count);

			var exTaxAmounts = convertedAPInvoice.Lines.ToArray<InvoicingLineBase>().Select(c => c.AL_LocalExTaxAmount);
			AssertCollectionContains(-250M, exTaxAmounts);
			AssertCollectionContains(100M, exTaxAmounts);

			var gstAmounts = convertedAPInvoice.Lines.ToArray<InvoicingLineBase>().Select(c => c.AL_LocalGSTAmount);
			AssertCollectionContains(-25M, gstAmounts);
			AssertCollectionContains(10M, gstAmounts);

			var totalAmounts = convertedAPInvoice.Lines.ToArray<InvoicingLineBase>().Select(c => c.AL_LocalTotalAmount);
			AssertCollectionContains(-275M, totalAmounts);
			AssertCollectionContains(110M, totalAmounts);

			AssertEquals(-150m, convertedAPInvoice.AH_LocalExTaxAmount);
			AssertEquals(-15m, convertedAPInvoice.AH_LocalTaxAmount);
			AssertEquals(-165m, convertedAPInvoice.AH_LocalTotalAmount);
		}

		public void TestImportingAPInvoicesFromSisterCompaniesWillNotThrownExceptonWhenDecimalsChange()
		{
			var currentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK));
			var originalValue = currentCompany.GC_IsReciprocal;

			try
			{
				setupPeriodManagement(ZDateTime.Today.Year);
				currentCompany.GC_IsReciprocal = true;

				var vnCompany = TestObjectCreator.CreateNewCompany("DVN", CountryCodes.VietNam);
				vnCompany.GC_IsReciprocal = true;
				vnCompany.GC_IsActive = true;
				vnCompany.GC_RX_NKLocalCurrency = CurrencyCodes.VietNam;
				var vnBranch = TestObjectCreator.CreateNewBranch(vnCompany, "VNB");
				vnBranch.GB_IsActive = true;
				var vNOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYC", true, true);
				vNOrgProxy.OH_RL_NKClosestPort = vnBranch.GB_RL_NKHomePort;
				vnCompany.GC_OH_OrgProxy = vNOrgProxy.PK;

				var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", ChargeType.Margin, 100, TestObjectCreator.FREECAPGST, TestObjectCreator.WHTFREE1);
				var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2", ChargeType.Margin, 100, TestObjectCreator.FREECAPGST, TestObjectCreator.WHTFREE1);

				Factory.Save();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment, null, 0, TestObjectCreator.Agent, 0);
				job.AgentCollectPK = vNOrgProxy.PK;
				job.JH_OA_AgentCollectAddr = vNOrgProxy.MainAddress.PK;
				job.AddCurrency(TestObjectCreator.USD, 4.7m, vNOrgProxy.PK, ExchangeRateValidLedgerEnum.AR);
				AssertEquals(1, job.ExchangeRates.Count);

				var jobCharge1 = job.Charges.AddNew();
				jobCharge1.JR_AC = chargeCode1.PK;
				jobCharge1.JR_OH_SellAccount = vNOrgProxy.PK;
				jobCharge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
				jobCharge1.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
				jobCharge1.JR_OSSellAmt = 10m;
				jobCharge1.JR_OSCostAmt = 0m;
				jobCharge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

				Factory.Save();

				var postManager = new InvoicingPostManager(job);
				postManager.SetCancelPostingForTestOnly(false);
				var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Agent);
				Assert(transactions.Count > 0);

				Factory.Save();

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, vnBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
				{
					var newFactory = new BusinessObjectFactory();
					var testObjectCreator2 = new TestObjectCreator(newFactory);

					var chargeCode3 = testObjectCreator2.CreateChargeCode("CC1", "Charge Code 4", ChargeType.Margin, 100, testObjectCreator2.FREECAPGST, testObjectCreator2.WHTFREE1);
					var chargeCode4 = testObjectCreator2.CreateChargeCode("CC2", "Charge Code 5", ChargeType.Margin, 100, testObjectCreator2.FREECAPGST, testObjectCreator2.WHTFREE1);

					shipment = newFactory.Load<ForwardingShipment>(shipment.PK);

					var jobInImportCompany = testObjectCreator2.CreateJob(shipment, testObjectCreator2.LocalClient, 0, null, 0);
					jobInImportCompany.JH_GB = vnBranch.PK;
					jobInImportCompany.JH_GE = originalDepartment.PK;
					jobInImportCompany.AddCurrency(testObjectCreator2.USD, 22.875m, testObjectCreator2.LocalClient.PK, ExchangeRateValidLedgerEnum.AR);
					AssertEquals(1, jobInImportCompany.ExchangeRates.Count);

					var charge2 = jobInImportCompany.Charges.AddNew();
					charge2.JR_AC = chargeCode3.PK;
					charge2.JR_OH_SellAccount = testObjectCreator2.LocalClient.PK;
					charge2.JR_RX_NKSellCurrency = testObjectCreator2.USD.RX_Code;
					charge2.JR_OSSellAmt = 0m;
					charge2.JR_OSCostAmt = 0m;

					var charge3 = jobInImportCompany.Charges.AddNew();
					charge3.JR_AC = chargeCode4.PK;
					charge3.JR_OH_SellAccount = testObjectCreator2.LocalClient.PK;
					charge3.JR_RX_NKSellCurrency = testObjectCreator2.USD.RX_Code;
					charge3.JR_OSSellAmt = 117.37m;
					charge3.JR_OSCostAmt = 0m;
					AssertEquals(2685m, charge3.JR_LocalSellAmt);

					newFactory.Save();

					InvoicingBase arInvoice = transactions.GetAllARInvoicesAndCreditNotes().FirstOrDefault();
					AssertNotNull(arInvoice);
					arInvoice = newFactory.Load<ARInvoice>(arInvoice.PK);

					var converter = new UnapprovedTransactionConverter(newFactory);
					var creditNote = converter.ConvertToAP(arInvoice, true);
					AssertNotNull(creditNote);

					creditNote.AH_RX_NKTransactionCurrency = testObjectCreator2.USD.RX_Code;
					creditNote.AH_ExchangeRate = 12000m;
					creditNote.AH_GB = vnBranch.PK;
					creditNote.AH_GE = originalDepartment.PK;
					AssertNoExceptionThrown(creditNote.Factory.Save);
				}
			}
			finally
			{
				currentCompany.GC_IsReciprocal = originalValue;
				Factory.Save();
			}
		}

		[TestDate(2020, 2, 2)]
		public void TestConvertARInvoiceNoJobToAPUsesCorrectExchangeRate_NonJob_UseJobExchangeRate() => AssertConvertARInvoiceNoJobToAPUsesCorrectExchangeRate_NonJob(true, 3.000300m, 3m);

		[TestDate(2020, 2, 2)]
		public void TestConvertARInvoiceNoJobToAPUsesCorrectExchangeRate_NonJob_DoNotUseJobExchangeRate() => AssertConvertARInvoiceNoJobToAPUsesCorrectExchangeRate_NonJob(false, 2m, 2m);

		void AssertConvertARInvoiceNoJobToAPUsesCorrectExchangeRate_NonJob(bool useJobExchageRate, decimal expectedHeaderRate, decimal expectedLineRate)
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, useJobExchageRate);
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(AccountingMasterFilesConstants.JobTypes.NonJobRelated, "ALL", "ALL", ExchangeRateTypes.Code.C01Rate);
			GlbCompany.CurrentCompany.Factory.Save();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 2m, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, 2m, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.C01Rate, 3m, ZDateTime.Today, ZDateTime.Today);

			var transaction = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "12345", TestObjectCreator.USD, 1, 100, 0, 100, 0, TestObjectCreator.AALSHI, TestObjectCreator.FRT.PK);
			Factory.Save();

			var converter = new UnapprovedTransactionConverter(Factory);
			var convertedInvoice = converter.ConvertToAP(transaction, false);

			AssertEquals(1, convertedInvoice.Lines.Count);
			AssertEquals(expectedLineRate, convertedInvoice.Lines[0].AL_ExchangeRate);
			AssertEquals(expectedHeaderRate, convertedInvoice.AH_ExchangeRate);
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertARInvoiceWithJobToAPUsesCorrectExchangeRate_Consol_UseJobExchangeRate()
		{
			AssertConvertARInvoiceWithJobToAPUsesCorrectExchangeRate(useJobExchageRate: true, isConsolInvoice: true);
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertARInvoiceWithJobToAPUsesCorrectExchangeRate_Consol()
		{
			AssertConvertARInvoiceWithJobToAPUsesCorrectExchangeRate(useJobExchageRate: false, isConsolInvoice: true);
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertARInvoiceWithJobToAPUsesCorrectExchangeRate_Shipment_UseJobExchangeRate()
		{
			AssertConvertARInvoiceWithJobToAPUsesCorrectExchangeRate(useJobExchageRate: true, isConsolInvoice: false);
		}

		[TestDate(2007, 06, 15, 12, 00, 00)]
		public void TestConvertARInvoiceWithJobToAPUsesCorrectExchangeRate_Shipment()
		{
			AssertConvertARInvoiceWithJobToAPUsesCorrectExchangeRate(useJobExchageRate: false, isConsolInvoice: false);
		}

		void AssertConvertARInvoiceWithJobToAPUsesCorrectExchangeRate(bool useJobExchageRate, bool isConsolInvoice)
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, useJobExchageRate);

			var converter = new UnapprovedTransactionConverter(Factory);
			var arTransaction = GetARTransactionWithJobPostedByForeignCountry(isConsolInvoice);
			AssertNotNull("arTransaction should not be null", arTransaction);
			AssertEquals("arTransaction should(not) be consol invoice", isConsolInvoice, arTransaction.IsConsolInvoice);

			var convertedInvoice = converter.ConvertToAP(arTransaction, false);
			AssertNotNull("Converted Invoice should not be null", convertedInvoice);
			Assert("Converted Invoice should be an AP Invoice", convertedInvoice is APInvoice);
			Assert(string.Format("AP Invoice should not have errors: {0}", convertedInvoice.NotificationsIncludingChildren.ToMessageListString()), !convertedInvoice.HasErrors);

			if (isConsolInvoice)
			{
				var jobChargeAmounts = new ZDecimal[] { 113278m, 16376m, 53935m, 62398m, 6760m, 23086m, 19531m, 6961m, 9515m };
				var localLineAmounts = new ZDecimal[9] { 887.61m, 128.32m, 422.62m, 488.93m, 52.97m, 180.89m, 153.04m, 54.54m, 74.56m }; // 0

				AssertEquals("Before save: AH_ExchangeRate is recalculated when use JobExchangeRate", useJobExchageRate ? 127.621261m : 127.6215m, convertedInvoice.AH_ExchangeRate);

				convertedInvoice.Factory.Save();

				AssertEquals("After save: AH_OSExTaxAmount", 311840m, convertedInvoice.AH_OSExTaxAmount);
				AssertEquals("After save: AH_LocalExTaxAmount", 2443.48m, convertedInvoice.AH_LocalExTaxAmount);
				AssertEquals("After save: useJobExchangeRate", useJobExchageRate, convertedInvoice.UseJobExchangeRate);
				AssertEquals("After save: AH_ExchangeRate is recalculated when use JobExchangeRate", useJobExchageRate ? 127.621261m : 127.6215m, convertedInvoice.AH_ExchangeRate);

				AssertEquals("After save: Line Count", 9, convertedInvoice.Lines.Count);
				// Check Local Amounts are the same in both cases (use/not use jobexchangerate)
				for (int i = 0; i < 9; i++)
				{
					AssertEquals("After save: Charge Code on Line " + i.ToString(), TestObjectCreator.GetChargeCode(Core.Constants.ChargeType.Margin).PK, convertedInvoice.Lines[i].AL_AC);
					Assert("After save: OS Amount on Line " + i.ToString() + " = " + convertedInvoice.Lines[i].AL_OSExTaxAmount.ToString(), jobChargeAmounts.Contains(convertedInvoice.Lines[i].AL_OSExTaxAmount));
					Assert("After save: Local Amount on Line " + i.ToString() + " = " + convertedInvoice.Lines[i].AL_LocalExTaxAmount.ToString(), localLineAmounts.Contains(convertedInvoice.Lines[i].AL_LocalExTaxAmount));
				}
			}
			else
			{
				AssertEquals("Before save: AH_ExchangeRate is recalculated when use JobExchangeRate", useJobExchageRate ? 125.495093m : 127.6215m, convertedInvoice.AH_ExchangeRate);

				convertedInvoice.Factory.Save();

				AssertEquals("After save: AH_OSExTaxAmount", 129654m, convertedInvoice.AH_OSExTaxAmount);
				AssertEquals("After save: AH_LocalExTaxAmount", useJobExchageRate ? 1033.14m : 1015.93m, convertedInvoice.AH_LocalExTaxAmount);
				AssertEquals("After save: useJobExchangeRate", useJobExchageRate, convertedInvoice.UseJobExchangeRate);
				AssertEquals("After save: AH_ExchangeRate is recalculated when use JobExchangeRate", useJobExchageRate ? 125.495093m : 127.6215m, convertedInvoice.AH_ExchangeRate);

				AssertEquals("After save: Line Count", 2, convertedInvoice.Lines.Count);

				AssertEquals("After save: OS Amount on Line 1", 113278m, convertedInvoice.Lines[0].AL_OSExTaxAmount);
				AssertEquals("After save: Local Amount on Line ", useJobExchageRate ? 902.65m : 887.61m, convertedInvoice.Lines[0].AL_LocalExTaxAmount);
				AssertEquals("After save: OS Amount on Line 2", 16376m, convertedInvoice.Lines[1].AL_OSExTaxAmount);
				AssertEquals("After save: Local Amount on Line ", useJobExchageRate ? 130.49m : 128.32m, convertedInvoice.Lines[1].AL_LocalExTaxAmount);
			}
		}

		InvoicingBase GetARTransactionWithJobPostedByForeignCountry(bool isConsolInvoice)
		{
			setupPeriodManagement(ZDateTime.Today.Year);

			originalDepartment.GE_Misc = false;
			var department = Factory.Load<GlbDepartment>(originalDepartment.PK);
			department.GE_Misc = false;
			Factory.Save();

			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1);
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GSTFREE1, TestObjectCreator.WHTFREE1, differentCompany);

			// This will be our Debtor of AR Invoice
			var currentBranchOrgProxy = GlbBranch.CurrentBranch.OrgProxy;
			// This is going to be our AP Invoice Creditor after conversion 
			var sourceBranchOrgProxy = differentBranch1.OrgProxy;
			sourceBranchOrgProxy.CompanyData.OB_IsCreditor = true;
			// Setting up Org specific Ex Rate Configuration
			var config = sourceBranchOrgProxy.CompanyData.AccAPExchangeRateConfigurations.AddNew();
			config.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = nameof(ExchangeRateType.C01);
			Factory.Save();

			differentCompany.GC_RX_NKLocalCurrency = "JPY";
			differentCompany.GC_IsReciprocal = true;

			var exRate = Factory.New<RefExchangeRate>();
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			exRate.RE_RX_NKExCurrency = differentCompany.GC_RX_NKLocalCurrency;
			exRate.RE_StartDate = new ZDateTime(ZDateTime.Today.Year, 1, 1);
			exRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.Year, 12, 31);
			exRate.RE_SellRate = 127.6195m;

			exRate = Factory.New<RefExchangeRate>();
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_ExRateType = nameof(ExchangeRateType.C01);
			exRate.RE_RX_NKExCurrency = differentCompany.GC_RX_NKLocalCurrency;
			exRate.RE_StartDate = new ZDateTime(ZDateTime.Today.Year, 1, 1);
			exRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.Year, 12, 31);
			exRate.RE_SellRate = 127.6215m;
			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var jobChargeAmounts = new ZDecimal[] { 113278m, 16376m, 53935m, 62398m, 6760m, 23086m, 19531m, 6961m, 9515m }; // 
			var chargeables = new ZDecimal[] { 2.262m, 0.327m, 1.077m, 1.246m, 0.135m, 0.461m, 0.390m, 0.139m, 0.19m };

			InvoicingBase transaction;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = "SEA";

			if (isConsolInvoice)
			{
				var shipments = new ForwardingShipment[9];
				var jobs = new Job[9];

				for (int i = 0; i < 9; i++)
				{
					shipments[i] = consol.Shipments.AddNew();
					shipments[i].JS_UniqueConsignRef = "S0000200" + i.ToString();
					shipments[i].JS_INCO = "EXW";
					shipments[i].JS_ActualChargeable = chargeables[i];
					AssertEquals("M3", shipments[i].JS_ChargeableUnit);

					jobs[i] = createJob(Factory, shipments[i].JS_UniqueConsignRef, Env.CurrentBranch.PK, originalDepartment.PK, TestObjectCreator.LocalClient, TestObjectCreator.Agent);
					jobs[i].JH_ParentID = shipments[i].PK;
					jobs[i].JH_ParentTableCode = "JS";

					jobs[i].AddCurrency(differentCompany.LocalCurrency, 125.4956m, ExchangeRateValidLedgerEnum.None);
				}
				Factory.Save();

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
				{
					var factory2 = new BusinessObjectFactory();
					var agent = factory2.Load<OrgHeader>(TestObjectCreator.Agent.PK);
					agent.CompanyData.OB_IsDebtor = true;
					agent.CompanyData.SetARTaxApplicable(false);
					agent.CompanyData.SetAPTaxApplicable(false);
					var localClient = factory2.NewWithValidTestData<OrgHeader>();

					consol = factory2.Load<ForwardingConsol>(consol.PK);

					var differentJobs = new Job[9];
					for (int i = 0; i < 9; i++)
					{
						differentJobs[i] = createJob(factory2, "J0000100" + i.ToString(), differentBranch1.PK, originalDepartment.PK, localClient, agent);
						differentJobs[i].JH_ParentID = shipments[i].PK;
						differentJobs[i].JH_ParentTableCode = "JS";
					}
					factory2.Save();

					var apportions = new ApportionmentListing(factory2, consol);
					var cost = apportions.CostsCollection.TryAddNew();
					cost.E6_OH_Creditor = agent.PK;
					cost.E6_AC_ChargeCode = chargeCode2.PK;
					cost.E6_ApportionmentMethod = "CHG";
					cost.E6_OSCostAmount = 311840;
					cost.E6_IsForCollectInvoice = true;
					factory2.Save();

					for (int i = 0; i < 9; i++)
					{
						AssertEquals("There should be only one Charge on the Job " + differentJobs[i].JH_JobNum, 1, differentJobs[i].Charges.Count);
						var jobCharge = differentJobs[i].Charges[0];
						AssertEquals("Charge Code on Charge of the Job " + differentJobs[i].JH_JobNum, chargeCode2.PK, jobCharge.JR_AC);
						AssertEquals("SellAmount on Charge of the Job " + differentJobs[i].JH_JobNum, jobChargeAmounts[i], jobCharge.JR_OSSellAmt);
						jobCharge.JR_OH_SellAccount = agent.PK;
					}
					factory2.Save();

					var transactions = new ConsolInvoicingPostManager(Factory, differentJobs, consol, new ApportionmentListing(factory2, consol)).CreateTransactions(JobInvoicingPostingOption.Agent);

					AssertEquals("One AR transaction should have been created", 1, transactions.ARTransactionsCount);
					var arInvoices = transactions.GetAllARInvoicesAndCreditNotes();
					transaction = arInvoices[0];
					AssertEquals("AR Invoice should have nine lines", 9, transaction.Lines.Count);
					for (int i = 0; i < 9; i++)
					{
						AssertEquals("Charge Code on Line " + i.ToString(), chargeCode2.PK, transaction.Lines[i].AL_AC);
						AssertEquals("Amount on Line " + i.ToString(), jobChargeAmounts[i], transaction.Lines[i].AL_OSExTaxAmount);
					}
					factory2.Save();
				}
			}
			else
			{
				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "S00002000";
				shipment.JS_INCO = "EXW";
				shipment.JS_ActualChargeable = chargeables[0];
				AssertEquals("M3", shipment.JS_ChargeableUnit);

				var job = createJob(Factory, shipment.JS_UniqueConsignRef, Env.CurrentBranch.PK, originalDepartment.PK, TestObjectCreator.LocalClient, TestObjectCreator.Agent);
				job.JH_ParentID = shipment.PK;
				job.JH_ParentTableCode = "JS";

				var jobExRate = job.ExchangeRates.AddNew();
				jobExRate.JF_RX_NKRateCurrency = differentCompany.GC_RX_NKLocalCurrency;
				jobExRate.JF_BaseRate = 125.4956m;

				Factory.Save();
				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
				{
					var factory2 = new BusinessObjectFactory();
					var debtor = factory2.Load<OrgHeader>(currentBranchOrgProxy.PK);
					debtor.CompanyData.OB_IsDebtor = true;
					debtor.CompanyData.SetARTaxApplicable(false);
					debtor.CompanyData.SetAPTaxApplicable(false);
					var localClient = factory2.NewWithValidTestData<OrgHeader>();

					var differentJob = createJob(factory2, "J00001000", differentBranch1.PK, originalDepartment.PK, localClient, debtor);
					differentJob.JH_ParentID = shipment.PK;
					differentJob.JH_ParentTableCode = "JS";
					factory2.Save();

					var jobCharge1 = differentJob.Charges.AddNew();
					jobCharge1.JR_AC = chargeCode2.PK;
					jobCharge1.JR_GB = differentBranch1.PK;
					jobCharge1.JR_GE = originalDepartment.PK;
					jobCharge1.JR_JH = differentJob.PK;
					jobCharge1.JR_OSCostAmt = jobChargeAmounts[0];
					jobCharge1.JR_OSSellAmt = jobChargeAmounts[0];
					jobCharge1.JR_OH_SellAccount = debtor.PK;
					factory2.Save();

					var jobCharge2 = differentJob.Charges.AddNew();
					jobCharge2.JR_AC = chargeCode2.PK;
					jobCharge2.JR_GB = differentBranch1.PK;
					jobCharge2.JR_GE = originalDepartment.PK;
					jobCharge2.JR_JH = differentJob.PK;
					jobCharge2.JR_OSCostAmt = jobChargeAmounts[1];
					jobCharge2.JR_OSSellAmt = jobChargeAmounts[1];
					jobCharge2.JR_OH_SellAccount = debtor.PK;
					factory2.Save();

					AssertEquals("There should be only 2 charges on the Job " + differentJob.JH_JobNum, 2, differentJob.Charges.Count);
					var jobCharge = differentJob.Charges[0];
					AssertEquals("Charge Code on Charge of the Job " + differentJob.JH_JobNum, chargeCode2.PK, jobCharge.JR_AC);
					AssertEquals("SellAmount on Charge of the Job " + differentJob.JH_JobNum, jobChargeAmounts[0], jobCharge.JR_OSSellAmt);
					jobCharge.JR_OH_SellAccount = debtor.PK;
					factory2.Save();

					var transactions = new InvoicingPostManager(differentJob).CreateTransactions(JobInvoicingPostingOption.Agent);

					AssertEquals("One AR transaction should have been created", 1, transactions.ARTransactionsCount);
					var arInvoices = transactions.GetAllARInvoicesAndCreditNotes();
					transaction = arInvoices[0];

					AssertEquals("AR Invoice should not be ConsolInvoice", false, transaction.IsConsolInvoice);
					AssertEquals("AR Invoice should have 2 lines", 2, transaction.Lines.Count);
					AssertEquals("Charge Code :", chargeCode2.PK, transaction.Lines[0].AL_AC);
					AssertEquals("Amount on Line 1:", jobChargeAmounts[0], transaction.Lines[0].AL_OSExTaxAmount);
					AssertEquals("Charge Code :", chargeCode2.PK, transaction.Lines[1].AL_AC);
					AssertEquals("Amount on Line 2:", jobChargeAmounts[1], transaction.Lines[1].AL_OSExTaxAmount);
					factory2.Save();
				}
			}
			return transaction;
		}

		public void TestImportingAPInvoicesFromSisterCompanies_SplitTaxIntoSeparaterLine_TaxRegisteredCountry()
		{
			AssertConvertToAPFromSisterCompanyInDifferentCountry(true);
		}

		public void TestImportingAPInvoicesFromSisterCompanies_SplitTaxIntoSeparaterLine_TaxNotRegisteredCountry()
		{
			AssertConvertToAPFromSisterCompanyInDifferentCountry(false);
		}

		void AssertConvertToAPFromSisterCompanyInDifferentCountry(bool isGSTRegistered)
		{
			var shipment = Factory.New<ForwardingShipment>();
			var testJob = TestObjectCreator.CreateJob(shipment, false);

			var chargeCode1 = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", ChargeType.Margin, 100, TestObjectCreator.FREECAPGST, TestObjectCreator.WHTFREE1);
			var chargeCode2 = TestObjectCreator.CreateChargeCode("CC2", "Charge Code 2", ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1);
			var chargeCode3 = TestObjectCreator.CreateChargeCode("CC3", "Charge Code 3", ChargeType.Margin, 100, TestObjectCreator.GST2, TestObjectCreator.WHTFREE1);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("000001", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			arInvoice.AH_JH = testJob.PK;
			var line1 = TestObjectCreator.CreateARInvoiceLine(arInvoice, testJob, chargeCode1, TestObjectCreator.AUD, 1.0m, "Line1 Description", 100M);
			line1.AL_GB = originalBranch.PK;
			line1.AL_GE = originalDepartment.PK;
			line1.AL_AT = TestObjectCreator.GSTFREE1.PK;
			TestObjectCreator.CreateJobCharge(line1, testJob, chargeCode1, TestObjectCreator.AUD);

			var line2 = TestObjectCreator.CreateARInvoiceLine(arInvoice, testJob, chargeCode2, TestObjectCreator.AUD, 1.0m, "Line2 Description", 200M);
			line2.AL_GB = originalBranch.PK;
			line2.AL_GE = originalDepartment.PK;
			line2.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.CreateJobCharge(line2, testJob, chargeCode2, TestObjectCreator.AUD);

			var line3 = TestObjectCreator.CreateARInvoiceLine(arInvoice, testJob, chargeCode3, TestObjectCreator.AUD, 1.0m, "Line3 Description", 300M);
			line3.AL_GB = originalBranch.PK;
			line3.AL_GE = originalDepartment.PK;
			line3.AL_AT = TestObjectCreator.GST2.PK;
			TestObjectCreator.CreateJobCharge(line3, testJob, chargeCode3, TestObjectCreator.AUD);

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.NonCurrentCompany.GC_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
			GlbCompany.CurrentCompany.Factory.Save();
			Factory.Save();

			AssertLineDetails(arInvoice, chargeCode1, ZString.Empty, 100M, 100M, 0M, 0M, TestObjectCreator.GSTFREE1.PK);
			AssertLineDetails(arInvoice, chargeCode2, ZString.Empty, 200M, 200M, 20M, 20M, TestObjectCreator.GST1.PK);
			AssertLineDetails(arInvoice, chargeCode3, ZString.Empty, 300M, 300M, 60M, 60M, TestObjectCreator.GST2.PK);

			using (new TemporaryUserContext { DepartmentPK = TestObjectCreator.FISDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				var newFactory = new BusinessObjectFactory();
				var testObjectCreator2 = new TestObjectCreator(newFactory);

				chargeCode1 = testObjectCreator2.CreateChargeCode("CC1", "Charge Code 1", ChargeType.Margin, 100, testObjectCreator2.FREECAPGST, testObjectCreator2.WHTFREE1);
				chargeCode2 = testObjectCreator2.CreateChargeCode("CC2", "Charge Code 2", ChargeType.Margin, 100, testObjectCreator2.GST1, testObjectCreator2.WHTFREE1);
				chargeCode3 = testObjectCreator2.CreateChargeCode("CC3", "Charge Code 3", ChargeType.Margin, 100, testObjectCreator2.GST2, testObjectCreator2.WHTFREE1);
				var overheaderChargeCode = testObjectCreator2.CreateChargeCode("CCO1", "Overhead Charge Code 1", ChargeType.Overhead, 0, testObjectCreator2.FREECAPGST, testObjectCreator2.WHTFREE1, differentBranch1.Company);

				AccountingConfigurationRegistry.Instance.SplitIntercompanyInvoiceTaxAmountIntoSeparateLine.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, overheaderChargeCode.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.MainNotReportableTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, testObjectCreator2.ExtraServiceTax.PK.ToGuid());

				differentBranch1 = newFactory.Load<GlbBranch>(differentBranch1.PK);
				differentBranch1.Company.GC_IsGSTRegistered = isGSTRegistered;
				testObjectCreator2.AALSHI.OH_IsCreditor = true;
				differentBranch1.GB_OH_OrgProxy = testObjectCreator2.AALSHI.PK;
				testObjectCreator2.AALSHI.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);
				newFactory.Save();

				var converter = new UnapprovedTransactionConverter(newFactory);
				shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				arInvoice = newFactory.Load<ARInvoice>(arInvoice.PK);
				var testJobInAnotherCompany = testObjectCreator2.CreateJob(shipment, false);
				var apInvoice = converter.ConvertToAPUnsafe(arInvoice, newFactory, false);

				AssertNotNull("AP invoice is created", apInvoice);
				AssertEquals("AP invoice has 4 lines", 4, apInvoice.Lines.Count);

				var expectedDescription = @"Overhead Charge Code 1
-Line2 Description 20.00
-Line3 Description 60.00";
				if (isGSTRegistered)
				{
					AssertLineDetails(apInvoice, chargeCode1, ZString.Empty, 100M, 100M, 0M, 0M, testObjectCreator2.ExtraServiceTax.PK);
					AssertLineDetails(apInvoice, chargeCode2, ZString.Empty, 200M, 200M, 0M, 0M, testObjectCreator2.ExtraServiceTax.PK);
					AssertLineDetails(apInvoice, chargeCode3, ZString.Empty, 300M, 300M, 0M, 0M, testObjectCreator2.ExtraServiceTax.PK);
					AssertLineDetails(apInvoice, overheaderChargeCode, expectedDescription, 80M, 80M, 0M, 0M, testObjectCreator2.ExtraServiceTax.PK);
				}
				else
				{
					AssertLineDetails(apInvoice, chargeCode1, ZString.Empty, 100M, 100M, 0M, 0M, ZGuid.Empty);
					AssertLineDetails(apInvoice, chargeCode2, ZString.Empty, 200M, 200M, 0M, 0M, ZGuid.Empty);
					AssertLineDetails(apInvoice, chargeCode3, ZString.Empty, 300M, 300M, 0M, 0M, ZGuid.Empty);
					AssertLineDetails(apInvoice, overheaderChargeCode, expectedDescription, 80M, 80M, 0M, 0M, ZGuid.Empty);
				}
			}
		}

		void AssertLineDetails(InvoicingBase invoice, AccChargeCode chargeCode, ZString lineDescription, ZDecimal osAmount, ZDecimal localAmount, ZDecimal osTaxAmount, ZDecimal localTaxAmount, ZGuid taxRatePK)
		{
			var line = invoice.Lines.Cast<InvoicingLineBase>().FirstOrDefault(x => x.AL_AC == chargeCode.PK);
			AssertNotNull($"Line: Charge Code {chargeCode.AC_Code} should exist.", line);

			CombineAssertions(delegate
			{
				if (!lineDescription.IsEmpty)
				{
					AssertEquals("Line: Description", lineDescription, line.AL_Desc);
				}
				AssertEquals("Line: OS Amount", osAmount, line.AL_OSExTaxAmount);
				AssertEquals("Line: Local Amount", localAmount, line.AL_LocalExTaxAmount);
				AssertEquals("Line: OS Tax Amount", osTaxAmount, line.AL_OSTaxAmount);
				AssertEquals("Line: Local Tax Amount", localTaxAmount, line.AL_LocalTaxAmount);
				AssertEquals("Line: Tax Rate", taxRatePK, line.AL_AT);
			});
		}

		#region Implementation

		GlbCompany differentCompany;
		GlbBranch differentBranch1;
		GlbCompany localCompany;
		GlbBranch differentBranch2;
		OrgHeader differentCompanyOrgProxy;
		OrgHeader differentBranchOrgProxy;
		GlbBranch originalBranch;
		GlbDepartment originalDepartment;
		AccountingPeriodTestHelper PeriodManagementTestHelper;
		GlbBranch proxyBranch;

		protected override void SetUp()
		{
			base.SetUp();

			PeriodManagementTestHelper = new AccountingPeriodTestHelper();

			// create a new proxy branch
			proxyBranch = TestObjectCreator.CreateBranch("TST", GlbCompany.CurrentCompany, TestObjectCreator.Agent);

			originalBranch = GlbBranch.CurrentBranch;
			originalDepartment = GlbDepartment.CurrentDepartment;

			differentCompany = TestObjectCreator.CreateNewCompany("ABC", "IT");

			differentBranch1 = TestObjectCreator.CreateNewBranch(differentCompany, "AB1");
			differentBranch1.GB_IsActive = true;
			differentCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYC", true, true);
			differentCompanyOrgProxy.OH_RL_NKClosestPort = differentBranch1.GB_RL_NKHomePort;
			differentCompany.GC_OH_OrgProxy = differentCompanyOrgProxy.PK;
			differentCompanyOrgProxy.CompanyData.SetAPTaxApplicable(false);

			differentBranchOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYB", true, true);
			differentBranchOrgProxy.OH_RL_NKClosestPort = differentBranch1.GB_RL_NKHomePort;
			differentBranch1.GB_OH_OrgProxy = differentBranchOrgProxy.PK;
			differentBranchOrgProxy.CompanyData.SetAPTaxApplicable(false);

			localCompany = TestObjectCreator.CreateNewCompany("ABA");
			localCompany.GC_OH_OrgProxy = TestObjectCreator.CreateOrgHeader("LOCALCOMP", true, true).PK;
			differentBranch2 = TestObjectCreator.CreateNewBranch(localCompany, "AB2");
			differentBranch2.GB_OH_OrgProxy = TestObjectCreator.CreateOrgHeader("LOCALORG2", true, true).PK;

			Factory.Save();
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator fTestObjectCreator;

		void AssertValidationTypeIsCorrect(InvoicingBase invoice, bool isAlreadyInCandidateContext)
		{
			Func<InvoicingBase, string> getInvoiceDescription = invoiceForDesc =>
				string.Format(" (Type: '{0}', AH_Ledger:: '{1}', AH_TransactionType: '{2}')", invoiceForDesc.GetType().Name, invoiceForDesc.AH_Ledger, invoiceForDesc.AH_TransactionType);
			bool isInvoice = invoice is APInvoice;

			if (!isAlreadyInCandidateContext)
			{
				var validationType = isInvoice ? typeof(APInvoiceValidation) : typeof(APCreditNoteValidation);
				AssertType("Validation type" + getInvoiceDescription(invoice), validationType, invoice.Validation);

				var collection = new UnapprovedTransactionCandidateCollection(Factory);
				collection.Add(invoice);
			}
			var validationTypeInCandidateContext = isInvoice ? typeof(UnapprovedInvoiceCandidateValidation) : typeof(UnapprovedCreditNoteCandidateValidation);
			AssertType("Validation type when in UnapprovedTransactionCandidateCollection" + getInvoiceDescription(invoice), validationTypeInCandidateContext, invoice.Validation);
		}

		class UnapprovedTransactionConverterExposed : UnapprovedTransactionConverter
		{
			public UnapprovedTransactionConverterExposed(BusinessObjectFactory factory, Action<InvoicingBase> sourceTransactionInNewFactoryAction)
			: base(factory)
			{
				sourceTransactionInNewFactoryAction_ForTestOnly = sourceTransactionInNewFactoryAction;
			}

			readonly Action<InvoicingBase> sourceTransactionInNewFactoryAction_ForTestOnly;
			internal string contextofCompanyForStoredARInvoice_ForTestOnly;

			protected override (InvoicingBase convertedAPTransaction, ZString approvalEventReference) ConvertToAPUnsafeFromARTransactionCore(ARTransactionToAPTransactionConverterBase arToAPConverter, InvoicingBase transaction, BusinessObjectFactory factoryForConvertedObject, bool releaseJobHeaderMutex, bool revalidateLines, bool generateInvoicePdf, bool isAutoImport)
			{
				if (sourceTransactionInNewFactoryAction_ForTestOnly != null)
				{
					arToAPConverter.sourceTransactionInNewFactoryAction_ForTestOnly += sourceTransactionInNewFactoryAction_ForTestOnly;
				}
				var convertedAPTransactionAndEventReference = base.ConvertToAPUnsafeFromARTransactionCore(arToAPConverter, transaction, factoryForConvertedObject, releaseJobHeaderMutex, revalidateLines, generateInvoicePdf, isAutoImport);
				contextofCompanyForStoredARInvoice_ForTestOnly = arToAPConverter.contextofCompanyForStoredARInvoice_ForTestOnly;
				return convertedAPTransactionAndEventReference;
			}
		}

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;

		#endregion
	}
}
