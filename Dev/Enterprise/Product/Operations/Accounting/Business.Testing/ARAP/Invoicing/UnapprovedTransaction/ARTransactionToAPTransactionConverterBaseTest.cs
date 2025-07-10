using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.UnapprovedTransaction
{
	public class ARTransactionToAPTransactionConverterBaseTest : TestCaseWithFactory
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestTaxTypeShouldBeNotReportWhenCrossLedgerImport()
		{
			var companyForID = Factory.NewWithValidTestData<GlbCompany>();
			companyForID.GC_Code = "DID";
			companyForID.GC_RN_NKCountryCode = "ID";
			companyForID.GC_OH_OrgProxy = TestObjectCreator.TestOrganisation.PK;
			companyForID.GC_IsGSTRegistered = true;

			var branchForID = Factory.NewWithValidTestData<GlbBranch>();
			branchForID.GB_Code = "JKT";
			branchForID.GB_GC = companyForID.PK;
			branchForID.OrgProxy.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

			var shipment = TestObjectCreator.CreateShipment("S00001017");

			var jobForAU = TestObjectCreator.CreateJob(shipment, false);

			var jobForID = TestObjectCreator.CreateJob(shipment, false, setCurrentBranch: false);
			jobForID.JH_GB = branchForID.PK;
			jobForID.JH_GC = companyForID.PK;

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("S00002798", TestObjectCreator.EUR, 1m, TestObjectCreator.TestOrganisation);
			arInvoice.AH_ConsolidatedInvoiceRef = "1234";

			var taxGST = TestObjectCreator.CreateTaxRate("GST", "GST Demo", 5);
			var taxPPN = TestObjectCreator.CreateTaxRate("PPN", "PPN Demo", 2);

			var chargeCodeFRTForAU = TestObjectCreator.CreateChargeCode("FRT", "", "MRG", 100, taxGST, null, "ALL");
			var aRInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, jobForAU, chargeCodeFRTForAU, 3m, TestObjectCreator.USD, 1.0m, taxRate: taxGST);
			TestObjectCreator.CreateCharge(aRInvoiceLine);

			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_OH = arInvoice.Branch.OrgProxy.PK;
			companyData.OB_GC = companyForID.PK;
			companyData.OB_APVATConfig = "DEF";

			var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD00001", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
			arCreditNote.OriginalTransactionReference = arInvoice.PK;
			Factory.Save();

			TestObjectCreator.CreateChargeCode("FRT", "", "MRG", 100, taxPPN, null, "ALL", company: companyForID);

			Factory.Save();

			AssertEquals(5m, arCreditNote.Lines[0].AL_TaxRateCalc);
			AssertEquals(3m, arCreditNote.Lines[0].AL_LocalExTaxAmount);
			AssertEquals(0.15m, arCreditNote.Lines[0].AL_LocalTaxAmount);
			AssertEquals(3.15m, arCreditNote.Lines[0].AL_LocalTotalAmount);
			AssertEquals(arInvoice.PK, arCreditNote.OriginalTransactionReference);

			using (Env.SetTemporaryUserContext(TestObjectCreator.Staff.PK.ToGuid(), branchForID.PK.ToGuid(), TestObjectCreator.FEADepartment.PK.ToGuid()))
			{
				var arToApConverter = new ARTransactionToAPTransactionConverterBase(new NotificationBuffer());
				var conversationFactoryForInvoice = new BusinessObjectFactory();
				(var apInvoice, _) = arToApConverter.ConvertToAPTransactionFromARTransaction(arInvoice, conversationFactoryForInvoice, false, false, false, true);
				conversationFactoryForInvoice.Save();

				var conversationFactoryForCreditNote = new BusinessObjectFactory();
				(var apCreditNote, _) = arToApConverter.ConvertToAPTransactionFromARTransaction(arCreditNote, conversationFactoryForCreditNote, false, false, false, true);
				AssertEquals("NOTREPORT", apCreditNote.Lines[0].TaxRate.AT_Code);
				AssertEquals(0m, apCreditNote.Lines[0].AL_TaxRateCalc);
				AssertEquals(3.15m, apCreditNote.Lines[0].AL_LocalExTaxAmount);
				AssertEquals(0m, apCreditNote.Lines[0].AL_LocalTaxAmount);
				AssertEquals(3.15m, apCreditNote.Lines[0].AL_LocalTotalAmount);
				AssertEquals(apInvoice.PK, apCreditNote.OriginalTransactionReference);
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestTaxTypeShouldBeNotReportWhenCrossLedgerImport_NoARInvoiceInSourceCompany()
		{
			var companyForID = Factory.NewWithValidTestData<GlbCompany>();
			companyForID.GC_Code = "DID";
			companyForID.GC_RN_NKCountryCode = "ID";
			companyForID.GC_OH_OrgProxy = TestObjectCreator.TestOrganisation.PK;
			companyForID.GC_IsGSTRegistered = true;

			var branchForID = Factory.NewWithValidTestData<GlbBranch>();
			branchForID.GB_Code = "JKT";
			branchForID.GB_GC = companyForID.PK;
			branchForID.OrgProxy.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

			Factory.Save();

			var taxGST = TestObjectCreator.CreateTaxRate("GST", "GST Demo", 5);
			var taxPPN = TestObjectCreator.CreateTaxRate("PPN", "PPN Demo", 2);

			var chargeCodeFRTForAU = TestObjectCreator.CreateChargeCode("FRT", "", "MRG", 100, taxGST, null, "ALL");
			var chargeCodeFRTForIndonesia = TestObjectCreator.CreateChargeCode("FRT", "", "MRG", 100, taxPPN, null, "ALL", company: companyForID);

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S00001017");

			var jobForAU = TestObjectCreator.CreateJob(shipment, false);

			var jobForID = TestObjectCreator.CreateJob(shipment, false, setCurrentBranch: false);
			jobForID.JH_GB = branchForID.PK;
			jobForID.JH_GC = companyForID.PK;

			var apInvoiceForID = TestObjectCreator.CreateAPInvoice<APInvoice>("000001111", TestObjectCreator.AUD, 1.0m, 50m, 0m, 0m, 50m, 0m, 0m, TestObjectCreator.AALSHI);
			apInvoiceForID.AH_ConsolidatedInvoiceRef = "1234";
			apInvoiceForID.AH_GC = companyForID.PK;
			apInvoiceForID.AH_GB = branchForID.PK;
			apInvoiceForID.AH_InvoiceDate = new ZDate(2023, 03, 20);

			var apInvoiceLine = apInvoiceForID.Lines[0];
			apInvoiceLine.AL_GB = branchForID.PK;
			apInvoiceLine.AL_GC = companyForID.PK;
			apInvoiceLine.AL_JH = jobForID.PK;
			apInvoiceLine.AL_AC = chargeCodeFRTForIndonesia.PK;
			TestObjectCreator.CreateCharge(apInvoiceLine);

			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_OH = TestObjectCreator.AALSHI.PK;
			companyData.OB_GC = companyForID.PK;
			companyData.OB_APVATConfig = "DEF";

			var arCreditNoteForAU = TestObjectCreator.CreateARCreditNote("ARCRD00001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m);
			arCreditNoteForAU.AH_OriginalTransactionNum = apInvoiceForID.AH_TransactionNum;
			arCreditNoteForAU.AH_OriginalInvoiceDate = new ZDate(2023, 03, 20);
			arCreditNoteForAU.Branch.GB_OH_OrgProxy = TestObjectCreator.AALSHI.PK;

			var line = Factory.NewWithValidTestData<ARCreditNoteLine>();
			line.AL_AC = chargeCodeFRTForAU.PK;
			line.AL_AT = taxGST.PK;
			line.AL_LocalExTaxAmount = 10m;

			AssertEquals(5m, line.AL_TaxRateCalc);
			AssertEquals(0.5m, line.AL_LocalTaxAmount);
			AssertEquals(10.5m, line.AL_LocalTotalAmount);
			AssertEquals(10m, line.AL_LocalExTaxAmount);

			arCreditNoteForAU.Lines.Add(line);

			TestObjectCreator.CreateChargeCode("FRT", "", "MRG", 100, taxPPN, null, "ALL", company: companyForID);

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestObjectCreator.Staff.PK.ToGuid(), branchForID.PK.ToGuid(), TestObjectCreator.FEADepartment.PK.ToGuid()))
			{
				var arToApConverter = new ARTransactionToAPTransactionConverterBase(new NotificationBuffer());
				var conversationFactoryForCreditNote = new BusinessObjectFactory();
				(var apCreditNote, _) = arToApConverter.ConvertToAPTransactionFromARTransaction(arCreditNoteForAU, conversationFactoryForCreditNote, false, false, false, true);
				AssertEquals("NOTREPORT", apCreditNote.Lines[0].TaxRate.AT_Code);
				AssertEquals(0m, apCreditNote.Lines[0].AL_TaxRateCalc);
				AssertEquals(10.5m, apCreditNote.Lines[0].AL_LocalExTaxAmount);
				AssertEquals(0m, apCreditNote.Lines[0].AL_LocalTaxAmount);
				AssertEquals(10.5m, apCreditNote.Lines[0].AL_LocalTotalAmount);
				AssertEquals(apInvoiceForID.PK, apCreditNote.OriginalTransactionReference);
			}
		}

		public void TestShouldNotGenerateAndAttachInvoicePdf_WhenGenerateInvoicePdfIsFalse()
		{
			AccountingConfigurationRegistry.Instance.PayableAllowUserToStoreARDoc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			InvoicingBase arInvoice;
			using (Env.SetTemporaryUserContext(TestObjectCreator.Staff.PK.ToGuid(), TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), TestObjectCreator.DefaultDepartmentPK))
			{
				arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, TestObjectCreator.AALSHI, TestObjectCreator.RevenueChargeCode.PK);
				arInvoice.Factory.Save();
			}

			var arToApConverter = new ARTransactionToAPTransactionConverterBase(new NotificationBuffer());
			var (apInvoice, _) = arToApConverter.ConvertToAPTransactionFromARTransaction(arInvoice, new BusinessObjectFactory(), false, false, generateInvoicePdf: false, false);
			AssertEquals("AllEDocs.Count", 0, apInvoice.DocManagerInfo.AllEDocs.Count);

			(apInvoice, _) = arToApConverter.ConvertToAPTransactionFromARTransaction(arInvoice, new BusinessObjectFactory(), false, false, generateInvoicePdf: true, false);
			AssertEquals("Postcondition: AllEDocs.Count", 1, apInvoice.DocManagerInfo.AllEDocs.Count);
		}

		public void TestShouldNotGenerateAndAttachInvoicePdf_WhenPayableAllowUserToStoreARDocRegistryIsFalse()
		{
			InvoicingBase arInvoice;
			using (Env.SetTemporaryUserContext(TestObjectCreator.Staff.PK.ToGuid(), TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), TestObjectCreator.DefaultDepartmentPK))
			{
				arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, TestObjectCreator.AALSHI, TestObjectCreator.RevenueChargeCode.PK);
				arInvoice.Factory.Save();
			}

			var arToApConverter = new ARTransactionToAPTransactionConverterBase(new NotificationBuffer());
			Assert("Precondition: PayableAllowUserToStoreARDoc", !AccountingConfigurationRegistry.Instance.PayableAllowUserToStoreARDoc.Value);
			var (apInvoice, _) = arToApConverter.ConvertToAPTransactionFromARTransaction(arInvoice, new BusinessObjectFactory(), false, false, generateInvoicePdf: true, false);
			AssertEquals("AllEDocs.Count", 0, apInvoice.DocManagerInfo.AllEDocs.Count);

			AccountingConfigurationRegistry.Instance.PayableAllowUserToStoreARDoc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			(apInvoice, _) = arToApConverter.ConvertToAPTransactionFromARTransaction(arInvoice, new BusinessObjectFactory(), false, false, generateInvoicePdf: true, false);
			AssertEquals("Postcondition: AllEDocs.Count", 1, apInvoice.DocManagerInfo.AllEDocs.Count);
		}

		public void TestShouldNotGenerateAndAttachInvoicePdf_WhenARInvoiceCompanyIsCurrentCompany()
		{
			AccountingConfigurationRegistry.Instance.PayableAllowUserToStoreARDoc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, TestObjectCreator.AALSHI, TestObjectCreator.RevenueChargeCode.PK);
			arInvoice.Factory.Save();

			AssertEquals("Precondition: arInvoice.AH_GC", GlbCompany.CurrentCompany.PK, arInvoice.AH_GC);
			var arToApConverter = new ARTransactionToAPTransactionConverterBase(new NotificationBuffer());
			var (apInvoice, _) = arToApConverter.ConvertToAPTransactionFromARTransaction(arInvoice, new BusinessObjectFactory(), false, false, generateInvoicePdf: true, false);
			AssertEquals("AllEDocs.Count", 0, apInvoice.DocManagerInfo.AllEDocs.Count);

			InvoicingBase arInvoice1;
			using (Env.SetTemporaryUserContext(TestObjectCreator.Staff.PK.ToGuid(), TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), TestObjectCreator.DefaultDepartmentPK))
			{
				arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, TestObjectCreator.AALSHI, TestObjectCreator.RevenueChargeCode.PK);
				arInvoice1.Factory.Save();
			}
			var (apInvoice1, _) = arToApConverter.ConvertToAPTransactionFromARTransaction(arInvoice1, new BusinessObjectFactory(), false, false, generateInvoicePdf: true, false);
			AssertEquals("Postcondition: AllEDocs.Count", 1, apInvoice1.DocManagerInfo.AllEDocs.Count);
		}

		public void TestShouldReportError_WhenFileNotFoundExceptionThrown()
		{
			InvoicingBase arInvoice;
			var arToApConverter = new ARTransactionToAPTransactionConverterBase(new NotificationBuffer());
			var errorHandled = false;

			AccountingConfigurationRegistry.Instance.PayableAllowUserToStoreARDoc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (new DisposableAction(() => InvoicingBase.ShouldClearAttachedFileContent_ForTestOnly = true, () => InvoicingBase.ShouldClearAttachedFileContent_ForTestOnly = false))
			{
				using (Env.SetTemporaryUserContext(TestObjectCreator.Staff.PK.ToGuid(), TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), TestObjectCreator.DefaultDepartmentPK))
				{
					arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, TestObjectCreator.AALSHI, TestObjectCreator.RevenueChargeCode.PK);
					arInvoice.Factory.Save();
				}

				var expectCaption = "File not found";
				var expectMessage = @"Could not generate AR Invoice to attach to eDocs.

Please check if the registry: 'Accounting -> Receivable Defaults -> Form Configurations -> Invoice -> AR Invoice Menu Item Name' is overridden.
* If it is overridden, please check if the registry has a valid value and if the menu path is valid.
* If it is not overridden or the registry value is valid, please contact support.";

				arInvoice.ShowError = (message, caption) =>
				{
					errorHandled = true;
					AssertEquals(expectMessage, message);
					AssertEquals(expectCaption, caption);
				};

				Assert("Precondition: PayableAllowUserToStoreARDoc", AccountingConfigurationRegistry.Instance.PayableAllowUserToStoreARDoc.Value);
				AssertNotEquals("Precondition: arInvoice.AH_GC", GlbCompany.CurrentCompany.PK, arInvoice.AH_GC);

				arToApConverter.ConvertToAPTransactionFromARTransaction(arInvoice, new BusinessObjectFactory(), false, false, generateInvoicePdf: true, false);
				Assert("Error should be handled", errorHandled);
			}
		}

		public void TestShouldReportError_WhenUnableToFindInvoiceDocumentCommandExceptionThrown()
		{
			var newMenu = Factory.NewWithValidTestData<StmMenuItem>();
			newMenu.SU_MenuName = "Test Menu Name";
			Factory.Save();

			InvoicingBase arInvoice;
			var arToApConverter = new ARTransactionToAPTransactionConverterBase(new NotificationBuffer());
			var errorHandled = false;

			AccountingConfigurationRegistry.Instance.PayableAllowUserToStoreARDoc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ARInvoiceMenuItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newMenu.PK.ToGuid());
			using (Env.SetTemporaryUserContext(TestObjectCreator.Staff.PK.ToGuid(), TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), TestObjectCreator.DefaultDepartmentPK))
			{
				arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, TestObjectCreator.AALSHI, TestObjectCreator.RevenueChargeCode.PK);
				arInvoice.Factory.Save();
			}

			var expectCaption = "Invoice Document Command not found";
			var expectMessage = FormattableString.Invariant($@"Could not generate AR Invoice to attach to eDocs.
Unable to find Invoice document command for transaction {arInvoice.AH_TransactionNum} with menu name: Test Menu Name, menu path: . Total commands found: 0.
Please check if the registry: 'Accounting -> Receivable Defaults -> Form Configurations -> Invoice -> AR Invoice Menu Item Name' is overridden.
* If it is overridden, please check if the registry has a valid value and if the menu path is valid.
* If it is not overridden or the registry value is valid, please contact support.");

			arInvoice.ShowError = (message, caption) =>
			{
				errorHandled = true;
				AssertEquals(expectMessage, message);
				AssertEquals(expectCaption, caption);
			};

			Assert("Precondition: PayableAllowUserToStoreARDoc", AccountingConfigurationRegistry.Instance.PayableAllowUserToStoreARDoc.Value);
			AssertNotEquals("Precondition: arInvoice.AH_GC", GlbCompany.CurrentCompany.PK, arInvoice.AH_GC);

			arToApConverter.ConvertToAPTransactionFromARTransaction(arInvoice, new BusinessObjectFactory(), false, false, generateInvoicePdf: true, false);
			Assert("Error should be handled", errorHandled);
		}

		public void TestConvertToAPTransactionFromARTransaction_GenerateAndAttachInvoicePdf_MultiplePDFs()
		{
			AccountingConfigurationRegistry.Instance.PayableAllowUserToStoreARDoc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertConvertToAPTransactionFromARTransactionGenerateAndAttachInvoicePdfMultiplePDFs("C001", "SHP1", "00001001", false);
				AssertConvertToAPTransactionFromARTransactionGenerateAndAttachInvoicePdfMultiplePDFs("C002", "SHP2", "00001002", true);
			}

			void AssertConvertToAPTransactionFromARTransactionGenerateAndAttachInvoicePdfMultiplePDFs(string consolNum, string shipmentNumber, string transactionNumber, bool printExtraDocs)
			{
				InvoicingBase arInvoice;
				var arToApConverter = new ARTransactionToAPTransactionConverterBase(new NotificationBuffer());
				var containers = printExtraDocs ? 5 : 4;
				var transactionCategory = printExtraDocs ? InvoiceTypesList.Codes.DestinationChargesInvoice_Batching : InvoiceTypesList.Codes.DestinationChargesInvoice;
				var consol = TestObjectCreator.CreateConsol(consolNum: consolNum);
				for (var idx = 0; idx < containers; idx++)
				{
					var container = consol.Containers.AddNew();
					container.JC_ContainerNum = $"ABC00000{idx.ToString()}";
				}

				using (Env.SetTemporaryUserContext(TestObjectCreator.Staff.PK.ToGuid(), TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var shipment = TestObjectCreator.CreateShipment(shipmentNumber, "AUSYD", "USLAX", consol);
					var job = TestObjectCreator.CreateJob(shipment);

					arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>(transactionNumber, TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
					arInvoice.AH_TransactionCategory = transactionCategory;
					TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Revenue, arInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "line 1 desc", 100m);
					TestObjectCreator.CreateCharge(arInvoice.Lines[0]);
					arInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
					Factory.Save();
				}

				var (apInvoice, _) = arToApConverter.ConvertToAPTransactionFromARTransaction(arInvoice, new BusinessObjectFactory(), true, false, generateInvoicePdf: true, false);
				AssertEquals("AP Invoice should have invoice attached in EDocs", printExtraDocs ? 3 : 1, apInvoice.DocManagerInfo.AllEDocs.Count);

				var invoiceDoc = apInvoice.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName == $"Invoice {apInvoice.AH_TransactionNum}.pdf");
				AssertNotNull(invoiceDoc);
				AssertEquals("Invoice", invoiceDoc.Description);
				AssertEquals("INV", invoiceDoc.DocType);
				Assert(invoiceDoc.IsSystemGenerated);

				if (printExtraDocs)
				{
					var containerListDoc = apInvoice.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName == $"Invoice {apInvoice.AH_TransactionNum} Container Details.pdf");
					AssertNotNull(containerListDoc);
					AssertEquals("Container List", containerListDoc.Description);
					AssertEquals("CLI", containerListDoc.DocType);
					Assert(containerListDoc.IsSystemGenerated);

					var periodicInvoiceDoc = apInvoice.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName == $"Invoice {apInvoice.AH_TransactionNum} Periodic Details.pdf");
					AssertNotNull(periodicInvoiceDoc);
					AssertEquals("Invoice", periodicInvoiceDoc.Description);
					AssertEquals("INV", periodicInvoiceDoc.DocType);
					Assert(periodicInvoiceDoc.IsSystemGenerated);
				}
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		protected GlbCompany differentCompany;
		protected GlbBranch differentBranch1;
		protected OrgHeader differentCompanyOrgProxy;
		protected OrgHeader differentBranchOrgProxy;
		protected AccountingPeriodTestHelper periodManagementTestHelper;

		protected override void SetUp()
		{
			base.SetUp();

			periodManagementTestHelper = new AccountingPeriodTestHelper();

			differentCompany = TestObjectCreator.CreateNewCompany("ABC", "CN");

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

			Factory.Save();
		}

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;
	}
}
