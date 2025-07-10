using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;
using UniversalTransactionInfo = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.EInvoice.Testing
{
	public class VietnamEInvoiceAdjustmentCreatorTest : TestCaseWithFactory
	{
		public void TestVietnamEInvoiceAdjustment()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_Code = "AAA";
			sequence.XD_SequenceClass = "TXI";
			sequence.XD_Prefix = prefix;
			sequence.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
			sequence.XD_IsActive = true;

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 0.2m, 20m, 2m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			invoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			invoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			invoice.AH_TransactionReference = "TR/21E0000390";
			invoice.AH_XD_ComplianceBook = sequence.PK;
			invoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			invoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			invoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			invoice.AH_ComplianceDocumentDate = ZDate.Today;
			TestObjectCreator.Debtor.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			invoice.AH_ChequeOrReference = "SellReference";

			var invoiceLine = invoice.Lines.Cast<ARInvoiceLine>().First();
			invoiceLine.AL_AT = taxRate.PK;
			invoiceLine.AL_Desc = "Line1\r\nLine2\r\nLine3";
			invoiceLine.AL_OSTaxAmount = 2m;
			invoiceLine.AL_OverseasTotal = 22m;
			invoiceLine.AL_LocalTaxAmount = 10m;
			invoiceLine.AL_LocalExTaxAmount = 100m;

			var creditNote = TestObjectCreator.CreateARCreditNoteWithLine("CRD001", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1m, "Incorrect amount", null, TestObjectCreator.CC1, 60m, ZDateTime.UtcNow, false);
			creditNote.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			creditNote.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			creditNote.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			creditNote.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			creditNote.AH_OC_InvoiceContactOverride = overrideContact.PK;
			creditNote.AH_TransactionBelongsToGroup = invoice.PK;
			creditNote.AH_TransactionReference = "TR/21E0000391";
			creditNote.AH_XD_ComplianceBook = sequence.PK;
			creditNote.AH_ComplianceDocumentDate = ZDate.Today;

			var creditNoteLine = creditNote.Lines.Cast<ARCreditNoteLine>().First();
			creditNoteLine.AL_AT = taxRate.PK;

			TestObjectCreator.SetCustomsCodeForOrgHeader(invoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var eInvoiceBatch = CreateEInvoicingBatch(creditNote);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "en");

			var mails = new List<ZString>();
			mails.Add("contact2@client2.com");
			var contactInfo = new AdditionalContactInfo()
			{
				Name = "test contact1",
				Mails = new List<ZString>() { "contact2@client2.com" },
				Phone = "13002191911"
			};

			var additionalTransactionInfoForVietnamEInvoice = new AdditionalTransactionInfoForVietnamEInvoice()
			{
				TransactionPK = creditNote.PK,
				TransactionReference = creditNote.AH_TransactionReference,
				OriginalTransactionPK = creditNote.PK,
				OriginalTransactionReference = creditNote.AH_TransactionReference,
				VATRegistrationNum = "0104128565-999",
				BankName = "bank test",
				AccountNumber = "account test number",
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					OriginalSeriesPrefix = invoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
					OriginalIsComplianceNumberFormatDefault = true,
					SeriesPrefix = creditNote.ComplianceBook?.XD_Prefix ?? ZString.Empty,
					IsComplianceNumberFormatDefault = true,
				},
				CompanyPK = creditNote.Company.PK,
				BranchPK = creditNote.Branch.PK,
				ComplianceDocumentDate = creditNote.AH_ComplianceDocumentDate,
				FormattedAddress = "test formatted address",
				InvoiceCreatingStaff = creditNote.CheckRequesterUserFullName,
				RectifyReason = creditNote.AH_Desc,
				RectifyDate = creditNote.AH_PostDate.Date.ToString("yyyy-MM-dd HH:mm"),
				RectifySupportingDocumentNumber = creditNote.SupportingDocumentNumber,
				ContactInfo = contactInfo,
			};

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => additionalTransactionInfoForVietnamEInvoice);

			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, invoice, creditNote, "0100233488", "0104128565-999", "bank test", "account test number", "en", sequenceNumber: additionalTransactionInfoForVietnamEInvoice.SequenceNumber, originalSequenceNumber: additionalTransactionInfoForVietnamEInvoice.OriginalSequenceNumber, contactInfo: contactInfo);
		}

		public void TestVietnamEInvoiceAdjustmentCreateInLocalCurrency()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 0.2m, 20m, 2m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			invoice.AH_TransactionReference = "TR/21E0000390";
			invoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			invoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			invoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			invoice.AH_ComplianceDocumentDate = ZDate.Today;
			TestObjectCreator.Debtor.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			invoice.AH_ChequeOrReference = "SellReference";
			invoice.AH_RX_NKTransactionCurrency = "USD";

			var invoiceLine = invoice.Lines.Cast<ARInvoiceLine>().First();
			invoiceLine.AL_AT = taxRate.PK;
			invoiceLine.AL_Desc = "Line1\r\nLine2\r\nLine3";
			invoiceLine.AL_OSTaxAmount = 2m;
			invoiceLine.AL_OverseasTotal = 22m;
			invoiceLine.AL_LocalTaxAmount = 10m;
			invoiceLine.AL_LocalExTaxAmount = 100m;

			Factory.Save();

			var creditNote = TestObjectCreator.CreateARCreditNoteWithLine("CRD001", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1m, "Incorrect amount", null, TestObjectCreator.CC1, 60m, ZDateTime.UtcNow, false);
			creditNote.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			creditNote.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			creditNote.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			creditNote.AH_TransactionBelongsToGroup = invoice.PK;
			creditNote.AH_TransactionReference = "TR/21E0000391";
			creditNote.AH_ComplianceDocumentDate = ZDate.Today;
			creditNote.AH_RX_NKTransactionCurrency = "USD";
			creditNote.AH_ExchangeRate = 0.3m;

			var creditNoteLine = creditNote.Lines.Cast<ARCreditNoteLine>().First();
			creditNoteLine.AL_AT = taxRate.PK;

			var eInvoiceBatch = CreateEInvoicingBatch(creditNote);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamAlwaysIssueElectronicInvoicesInLocalCurrency.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.VietnamExportAmountInWordsBasedOnInvoicedCurrency.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var additionalTransactionInfoForVietnamEInvoice = new AdditionalTransactionInfoForVietnamEInvoice()
			{
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					OriginalSeriesPrefix = invoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
					OriginalIsComplianceNumberFormatDefault = true,
					SeriesPrefix = creditNote.ComplianceBook?.XD_Prefix ?? ZString.Empty,
					IsComplianceNumberFormatDefault = true,
				},
				CompanyPK = creditNote.Company.PK,
				BranchPK = creditNote.Branch.PK,
			};

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => additionalTransactionInfoForVietnamEInvoice);

			AssertNotNull("EInvoice", eInvoice);
			AssertEquals("AUD", eInvoice.Inv.Curr);
			AssertEquals(1m, eInvoice.Inv.Exrt);
			AssertEquals(200m, eInvoice.Inv.Sum);
			AssertEquals(220m, eInvoice.Inv.Total);
			AssertEquals(20m, eInvoice.Inv.Vat);
			AssertEquals("Hai trăm hai mươi  dong", eInvoice.Inv.Word);

			var item = eInvoice.Inv.Items[0];
			AssertEquals(200m, item.Amount);
			AssertEquals(20m, item.VAT);
			AssertEquals(220m, item.Total);
			AssertEquals(200m, item.Price);
		}

		public void TestVietnamEInvoiceAdjustment_PeriodicInvoice_MultipleJob()
		{
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);

			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_Code = "AAA";
			sequence.XD_SequenceClass = "TXI";
			sequence.XD_Prefix = prefix;
			sequence.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
			sequence.XD_IsActive = true;

			var declaration = (BaseJobDeclaration)TestObjectCreator.CreateDeclaration("D0001");
			declaration.JE_OH_Supplier = TestObjectCreator.AALSHI.PK;
			declaration.JE_OH_Importer = TestObjectCreator.ABIGAS.PK;
			declaration.JE_GoodsDescription = "GoodsDescription";
			declaration.JE_OwnerRef = "OwnerRef";
			declaration.DocsAndCartage.JP_OrderItemsAsString = "TestOrderReference";
			declaration.JE_MasterBill = "MasterBill";
			declaration.JE_HouseBill = "HouseBilling";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_DateAtFinalDestination = ZDateTime.Now.AddDays(-1);
			declaration.JE_DateAtOrigin = ZDateTime.Now;
			declaration.JE_OH_ShippingLine = TestObjectCreator.Creditor1.PK;
			declaration.JE_TotalNoOfPacks = 2;
			declaration.JE_TotalNoOfPacksPackType = "PKG";
			declaration.JE_TotalWeight = 30;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalVolume = 60M;
			declaration.JE_TotalVolumeUnit = "M3";
			declaration.JE_RL_NKOrigin = "VNHAN";
			declaration.JE_RL_NKFinalDestination = "TWTPE";

			var transport = declaration.TransportsIncludingRelated.AddNew();
			transport.JW_VoyageFlight = "TestFlight";
			transport.JW_Vessel = "TestVessel";
			transport.JW_ETD = ZDateTime.Now.AddDays(-1);

			var job = TestObjectCreator.CreateJob(declaration);
			TestObjectCreator.Staff.GS_FullName = "StaffFullName";
			job.JH_GS_NKRepSales = TestObjectCreator.Staff.GS_Code;
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);

			var jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = TestObjectCreator.CC1.PK;
			jobCharge.JR_LocalSellAmt = 100m;
			jobCharge.JR_OSSellAmt = 100m;
			jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			jobCharge.JR_AT_SellGSTRate = taxRate.PK;

			var consol = TestObjectCreator.CreateConsol();
			consol.JK_MasterBillNum = "TestMAWB";

			var transport2 = consol.MostInterestingTransportForBinding[0];
			transport2.JW_VoyageFlight = "TestFlight";
			transport2.JW_Vessel = "TestVessel";

			var shipment = TestObjectCreator.CreateShipment("S00001007");
			shipment.ConsigneePK = TestObjectCreator.Creditor1.PK;
			shipment.ConsignorPK = TestObjectCreator.Creditor2.PK;
			shipment.JS_RL_NKOrigin = "VNHAN";
			shipment.JS_RL_NKDestination = "TWTPE";
			shipment.JS_E_DEP = ZDateTime.Now.AddDays(-1);
			shipment.JS_E_ARV = ZDateTime.Now;
			shipment.JS_HouseBill = "ShipmentHouseBill";
			shipment.JS_OuterPacks = 2;
			shipment.JS_F3_NKPackType = "CTN";
			shipment.JS_TotalPackageCount = 2;
			shipment.JS_ActualWeight = 1.5M;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualChargeable = 1.7M;
			shipment.JS_ActualVolume = 5.2M;
			shipment.JS_UnitOfVolume = "M3";
			shipment.DocsAndCartage.JP_OrderItemsAsString = "TestOrderReference";
			consol.Shipments.Add(shipment);

			var declaration2 = TestObjectCreator.CreateDeclaration("D0002");
			declaration2.JE_JS = shipment.PK;
			declaration2.JE_OwnerRef = "OwnerReference";

			var job2 = TestObjectCreator.CreateJob(shipment);
			TestObjectCreator.Staff.GS_FullName = "StaffFullName";
			job2.JH_GS_NKRepSales = TestObjectCreator.Staff.GS_Code;
			TestObjectCreator.SetExchangeRate(job2, TestObjectCreator.AUD, 1);

			var jobCharge2 = job2.Charges.AddNew();
			jobCharge2.JR_AC = TestObjectCreator.CC1.PK;
			jobCharge2.JR_LocalSellAmt = 100m;
			jobCharge2.JR_OSSellAmt = 100m;
			jobCharge2.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			jobCharge2.JR_AT_SellGSTRate = taxRate.PK;

			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor, OrgAddressType.Receivables, true, "Address1", "Address2", "Test City", "01", "VN", "1234", "", "");
			address.PrimaryOrgAddressAdditionalInfoDetail = "Additional Information";
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "VNLAP";
			TestObjectCreator.Debtor.Addresses.Add(address);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 15M, TestObjectCreator.Debtor);
			arInvoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			arInvoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			arInvoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			arInvoice.AH_Desc = "TEST";
			arInvoice.AH_InvoiceDate = ZDateTime.Today;
			arInvoice.AH_PostDate = ZDateTime.Today;
			arInvoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			arInvoice.AH_XD_ComplianceBook = sequence.PK;
			arInvoice.AH_TransactionReference = "TR/21E0000391";
			TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 0m, 10m, 0m, 0m, 10m, 0m, 0m);
			arInvoice.Lines[0].GenericCharge = TestObjectCreator.CC1.PK;
			arInvoice.AH_OA_InvoiceAddressOverride = address.PK;
			arInvoice.Header.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			arInvoice.AH_ChequeOrReference = "SellReference";
			arInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var line = arInvoice.Lines.Cast<InvoicingLineBase>().First();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_JH = job.PK;
			line.AL_AT = taxRate.PK;
			line.AL_LineAmount = 100m;
			jobCharge.JR_AL_ARLine = line.PK;

			var line2 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 0m, 10m, 0m, 0m, 10m, 0m, 0m);
			line2.AL_AC = TestObjectCreator.CC1.PK;
			line2.AL_JH = job2.PK;
			line2.AL_AT = taxRate.PK;
			line2.AL_LineAmount = 100m;
			jobCharge2.JR_AL_ARLine = line2.PK;

			var creditNote = ((IAmending)arInvoice).GenerateAmendingTransaction(TransactionTypes.CreditNote) as ARCreditNote;
			creditNote.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			creditNote.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			creditNote.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			creditNote.AH_XD_ComplianceBook = sequence.PK;
			creditNote.AH_TransactionReference = "TR/21E0000391";

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var eInvoiceBatch = CreateEInvoicingBatch(creditNote);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "en");
			var transactionLineHouseBillDictionary = ((TransactionHeaderWithLines)creditNote).Lines.Select(x => x.Job).ToDictionary(j => j.JH_JobNum, j => ((Job)j).JH_HouseBillNo);
			var additionalTransactionInfoForVietnamEInvoice = new AdditionalTransactionInfoForVietnamEInvoice()
			{
				TransactionPK = creditNote.PK,
				TransactionReference = creditNote.AH_TransactionReference,
				OriginalTransactionPK = creditNote.PK,
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				VATRegistrationNum = "0104128565-999",
				BankName = "bank test",
				AccountNumber = "account test number",
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					OriginalSeriesPrefix = arInvoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
					OriginalIsComplianceNumberFormatDefault = true,
					SeriesPrefix = creditNote.ComplianceBook?.XD_Prefix ?? ZString.Empty,
					IsComplianceNumberFormatDefault = true,
				},
				CompanyPK = creditNote.Company.PK,
				BranchPK = creditNote.Branch.PK,
				ComplianceDocumentDate = creditNote.AH_ComplianceDocumentDate,
				FormattedAddress = "test formatted address",
				InvoiceCreatingStaff = creditNote.CheckRequesterUserFullName,
				RectifyReason = creditNote.AH_Desc,
				RectifyDate = creditNote.AH_PostDate.ToString("yyyy-MM-dd HH:mm"),
				RectifySupportingDocumentNumber = creditNote.SupportingDocumentNumber,
				TransactionLineHouseBillDictionary = transactionLineHouseBillDictionary
			};

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => additionalTransactionInfoForVietnamEInvoice);

			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, creditNote, "0100233488", "0104128565-999", "bank test", "account test number", "en", sequenceNumber: additionalTransactionInfoForVietnamEInvoice.SequenceNumber, originalSequenceNumber: additionalTransactionInfoForVietnamEInvoice.OriginalSequenceNumber);
		}

		public void TestVietnamEInvoiceAdjustment_HasColoadMasterShipment()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);

			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_Code = "AAA";
			sequence.XD_SequenceClass = "TXI";
			sequence.XD_Prefix = prefix;
			sequence.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
			sequence.XD_IsActive = true;

			var consol = TestObjectCreator.CreateConsol("AUSYD", "AUMEL", "C0001");
			var shipment = TestObjectCreator.CreateShipment("S00001007");
			TestObjectCreator.Creditor1.OH_FullName = "Creditor1";
			TestObjectCreator.Creditor2.OH_FullName = "Creditor2";
			shipment.ConsigneePK = TestObjectCreator.Creditor1.PK;
			shipment.ConsignorPK = TestObjectCreator.Creditor2.PK;
			consol.Shipments.Add(shipment);

			var masterShipment = TestObjectCreator.CreateShipment("S00001006");
			TestObjectCreator.Creditor3.OH_FullName = "Creditor3";
			TestObjectCreator.Creditor4.OH_FullName = "Creditor4";
			masterShipment.ConsigneePK = TestObjectCreator.Creditor3.PK;
			masterShipment.ConsignorPK = TestObjectCreator.Creditor4.PK;
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			consol.Shipments.Add(masterShipment);

			var job = TestObjectCreator.CreateJob(shipment);
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);

			var jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = TestObjectCreator.CC1.PK;
			jobCharge.JR_LocalSellAmt = 100m;
			jobCharge.JR_OSSellAmt = 100m;
			jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			jobCharge.JR_AT_SellGSTRate = taxRate.PK;

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);
			arInvoice.AH_JH = job.PK;
			arInvoice.AH_TransactionReference = "AP/19E1";
			arInvoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			var line = arInvoice.Lines.Cast<InvoicingLineBase>().First();
			line.AL_JH = job.PK;
			line.AL_AT = taxRate.PK;
			jobCharge.JR_AL_ARLine = line.PK;

			var creditNote = TestObjectCreator.CreateARCreditNoteWithLine("CRD001", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1m, "Incorrect amount", null, TestObjectCreator.CC1, 60m, ZDateTime.UtcNow, false);
			creditNote.AH_JH = job.PK;
			creditNote.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			creditNote.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			creditNote.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			creditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
			creditNote.AH_XD_ComplianceBook = sequence.PK;
			creditNote.AH_ComplianceDocumentDate = ZDate.Today;

			var creditNoteLine = creditNote.Lines.Cast<ARCreditNoteLine>().First();
			creditNoteLine.AL_JH = job.PK;
			creditNoteLine.AL_AT = taxRate.PK;

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var eInvoiceBatch = CreateEInvoicingBatch(creditNote);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "en");

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice()
			{
				TransactionPK = creditNote.PK,
				OriginalTransactionPK = creditNote.PK,
				OriginalTransactionReference = creditNote.AH_TransactionReference,
				CompanyPK = creditNote.Company.PK,
				BranchPK = creditNote.Branch.PK,
				ComplianceDocumentDate = creditNote.AH_ComplianceDocumentDate,
				TransactionParentID = shipment.JS_UniqueConsignRef,
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					SeriesPrefix = creditNote.ComplianceBook?.XD_Prefix ?? ZString.Empty,
				},
			});

			AssertNotNull("EInvoice", eInvoice);
			AssertEquals("Consignee is Creditor1", "Creditor1", eInvoice.Inv.Consignee);
			AssertEquals("Consignor is Creditor2", "Creditor2", eInvoice.Inv.Consignor);
		}

		public void TestVietnamEInvoiceAdjustment_TypeRef()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_Code = "AAA";
			sequence.XD_SequenceClass = "TXI";
			sequence.XD_Prefix = prefix;
			sequence.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
			sequence.XD_IsActive = true;
			sequence.XD_MaximumNumberDigits = 8;

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 0.2m, 20m, 2m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			invoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			invoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			invoice.AH_TransactionReference = "TR/21E0000390";
			invoice.AH_XD_ComplianceBook = sequence.PK;
			invoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			invoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			invoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			invoice.AH_ComplianceDocumentDate = ZDate.Today;
			TestObjectCreator.Debtor.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			invoice.AH_ChequeOrReference = "SellReference";

			var invoiceLine = invoice.Lines.Cast<ARInvoiceLine>().First();
			invoiceLine.AL_AT = taxRate.PK;
			invoiceLine.AL_Desc = "Line1\r\nLine2\r\nLine3";
			invoiceLine.AL_OSTaxAmount = 2m;
			invoiceLine.AL_OverseasTotal = 22m;
			invoiceLine.AL_LocalTaxAmount = 10m;
			invoiceLine.AL_LocalExTaxAmount = 100m;

			var creditNote = TestObjectCreator.CreateARCreditNoteWithLine("CRD001", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1m, "Incorrect amount", null, TestObjectCreator.CC1, 60m, ZDateTime.UtcNow, false);
			creditNote.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			creditNote.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			creditNote.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			creditNote.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			creditNote.AH_OC_InvoiceContactOverride = overrideContact.PK;
			creditNote.AH_TransactionBelongsToGroup = invoice.PK;
			creditNote.AH_TransactionReference = "TR/21E0000391";
			creditNote.AH_XD_ComplianceBook = sequence.PK;
			creditNote.AH_ComplianceDocumentDate = ZDate.Today;

			var creditNoteLine = creditNote.Lines.Cast<ARCreditNoteLine>().First();
			creditNoteLine.AL_AT = taxRate.PK;

			TestObjectCreator.SetCustomsCodeForOrgHeader(invoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var eInvoiceBatch = CreateEInvoicingBatch(creditNote);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "en");

			Func<AdditionalTransactionInfoForVietnamEInvoice> additionalTransactionInfoForVietnamEInvoice = delegate
			{
				return new AdditionalTransactionInfoForVietnamEInvoice()
				{
					TransactionPK = creditNote.PK,
					TransactionReference = creditNote.AH_TransactionReference,
					OriginalTransactionPK = creditNote.PK,
					OriginalTransactionReference = creditNote.AH_TransactionReference,
					VATRegistrationNum = "0104128565-999",
					BankName = "bank test",
					AccountNumber = "account test number",
					ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
					{
						OriginalSeriesPrefix = invoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
						OriginalIsComplianceNumberFormatDefault = true,
						OriginalComplianceSequenceMaximumNumberDigits = invoice.ComplianceBook.XD_MaximumNumberDigits,
						SeriesPrefix = creditNote.ComplianceBook?.XD_Prefix ?? ZString.Empty,
						IsComplianceNumberFormatDefault = true,
						ComplianceSequenceMaximumNumberDigits = creditNote.ComplianceBook.XD_MaximumNumberDigits,
					},
					CompanyPK = creditNote.Company.PK,
					BranchPK = creditNote.Branch.PK,
					ComplianceDocumentDate = creditNote.AH_ComplianceDocumentDate,
					FormattedAddress = "test formatted address",
					InvoiceCreatingStaff = creditNote.CheckRequesterUserFullName,
					RectifyReason = creditNote.AH_Desc,
					RectifyDate = creditNote.AH_PostDate.Date.ToString("yyyy-MM-dd HH:mm"),
					RectifySupportingDocumentNumber = creditNote.SupportingDocumentNumber
				};
			};

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, additionalTransactionInfoForVietnamEInvoice);

			AssertNotNull("EInvoice", eInvoice);
			AssertEquals("Compliance sequence maximum number digits for credit notes is 8", 8, creditNote.ComplianceBook.XD_MaximumNumberDigits.ToZInt());
			AssertEquals("type_ref is 1 when Compliance sequence maximum number digits for credit notes is 8", 1, eInvoice.Inv.TypeRef);

			sequence.XD_MaximumNumberDigits = 7;

			(eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, additionalTransactionInfoForVietnamEInvoice);

			AssertNotNull("EInvoice", eInvoice);
			AssertEquals("Compliance sequence maximum number digits for credit notes is 7", 7, creditNote.ComplianceBook.XD_MaximumNumberDigits.ToZInt());
			AssertEquals("type_ref is 0 when Compliance sequence maximum number digits for credit notes is 7", 0, eInvoice.Inv.TypeRef);
		}

		public void TestVietnamEInvoiceAdjustment_SendType()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_Code = "AAA";
			sequence.XD_SequenceClass = "TXI";
			sequence.XD_Prefix = prefix;
			sequence.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
			sequence.XD_IsActive = true;
			sequence.XD_MaximumNumberDigits = 8;

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 0.2m, 20m, 2m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			invoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			invoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			invoice.AH_TransactionReference = "TR/21E0000390";
			invoice.AH_XD_ComplianceBook = sequence.PK;
			invoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			invoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			invoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			invoice.AH_ComplianceDocumentDate = ZDate.Today;
			TestObjectCreator.Debtor.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			invoice.AH_ChequeOrReference = "SellReference";

			var invoiceLine = invoice.Lines.Cast<ARInvoiceLine>().First();
			invoiceLine.AL_AT = taxRate.PK;
			invoiceLine.AL_Desc = "Line1\r\nLine2\r\nLine3";
			invoiceLine.AL_OSTaxAmount = 2m;
			invoiceLine.AL_OverseasTotal = 22m;
			invoiceLine.AL_LocalTaxAmount = 10m;
			invoiceLine.AL_LocalExTaxAmount = 100m;

			var creditNote = TestObjectCreator.CreateARCreditNoteWithLine("CRD001", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1m, "Incorrect amount", null, TestObjectCreator.CC1, 60m, ZDateTime.UtcNow, false);
			creditNote.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			creditNote.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			creditNote.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			creditNote.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			creditNote.AH_OC_InvoiceContactOverride = overrideContact.PK;
			creditNote.AH_TransactionBelongsToGroup = invoice.PK;
			creditNote.AH_TransactionReference = "TR/21E0000391";
			creditNote.AH_XD_ComplianceBook = sequence.PK;
			creditNote.AH_ComplianceDocumentDate = ZDate.Today;

			var creditNoteLine = creditNote.Lines.Cast<ARCreditNoteLine>().First();
			creditNoteLine.AL_AT = taxRate.PK;

			TestObjectCreator.SetCustomsCodeForOrgHeader(invoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var eInvoiceBatch = CreateEInvoicingBatch(creditNote);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "en");
			AccountingConfigurationRegistry.Instance.EInvoicingSendType.SetValue(creditNote.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Func<AdditionalTransactionInfoForVietnamEInvoice> additionalTransactionInfoForVietnamEInvoice = delegate
			{
				return new AdditionalTransactionInfoForVietnamEInvoice()
				{
					TransactionPK = creditNote.PK,
					TransactionReference = creditNote.AH_TransactionReference,
					OriginalTransactionPK = creditNote.PK,
					OriginalTransactionReference = creditNote.AH_TransactionReference,
					VATRegistrationNum = "0104128565-999",
					BankName = "bank test",
					AccountNumber = "account test number",
					ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
					{
						OriginalSeriesPrefix = invoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
						OriginalIsComplianceNumberFormatDefault = true,
						OriginalComplianceSequenceMaximumNumberDigits = invoice.ComplianceBook.XD_MaximumNumberDigits,
						SeriesPrefix = creditNote.ComplianceBook?.XD_Prefix ?? ZString.Empty,
						IsComplianceNumberFormatDefault = true,
						ComplianceSequenceMaximumNumberDigits = creditNote.ComplianceBook.XD_MaximumNumberDigits,
					},
					CompanyPK = creditNote.Company.PK,
					BranchPK = creditNote.Branch.PK,
					ComplianceDocumentDate = creditNote.AH_ComplianceDocumentDate,
					FormattedAddress = "test formatted address",
					InvoiceCreatingStaff = creditNote.CheckRequesterUserFullName,
					RectifyReason = creditNote.AH_Desc,
					RectifyDate = creditNote.AH_PostDate.Date.ToString("yyyy-MM-dd HH:mm"),
					RectifySupportingDocumentNumber = creditNote.SupportingDocumentNumber
				};
			};

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, additionalTransactionInfoForVietnamEInvoice);

			AssertNotNull("EInvoice", eInvoice);
			AssertEquals("EInvoicingSendType is true", true, AccountingConfigurationRegistry.Instance.EInvoicingSendType.GetFallBackValueAtAllLevels(creditNote.Company.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("sendtype is 2 when EInvoicingSendType is true", 2, eInvoice.Inv.SendType);

			AccountingConfigurationRegistry.Instance.EInvoicingSendType.SetValue(creditNote.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			(eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, additionalTransactionInfoForVietnamEInvoice);

			AssertNotNull("EInvoice", eInvoice);
			AssertEquals("EInvoicingSendType is false", false, AccountingConfigurationRegistry.Instance.EInvoicingSendType.GetFallBackValueAtAllLevels(creditNote.Company.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("sendtype is 1 when EInvoicingSendType is false", 1, eInvoice.Inv.SendType);
		}

		public void TestMessageTypeAndPayloadWhenAmendWithInvoice()
		{
			TestAmendScenario(TransactionTypes.Invoice, VietnamEInvoiceAPICommandList.Codes.AdjustReceivablesInvoice, 1, "DCT", 1);
		}

		public void TestItemTypeAndStatusWhenAmendWithCreditNote()
		{
			TestAmendScenario(TransactionTypes.CreditNote, null, 0, "DCG", 0);
		}

		void TestAmendScenario(string amendTransactionType, string expectedMessageType, int expectedUd, string expectedItemType, int expectedItemStatus)
		{
			var (batch, address) = CreateBatchAndAddress();

			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				GlbBranch.CurrentBranch.SetCountry("VN");

				var shipment = TestObjectCreator.CreateShipment("S00001007");
				var job = TestObjectCreator.CreateJob(shipment);
				var arInvoiceOriginal = CreateARInvoiceOriginal(GlbBranch.CurrentBranch, job, address);
				Factory.Save();

				InvoicingBase amendTransaction;
				if (amendTransactionType == TransactionTypes.CreditNote)
				{
					amendTransaction = (ARCreditNote)testObjectCreator.AmendARTransaction(amendTransactionType, arInvoiceOriginal).amendTransaction;
				}
				else
				{
					amendTransaction = (ARInvoice)testObjectCreator.AmendARTransaction(amendTransactionType, arInvoiceOriginal).amendTransaction;
				}
				Factory.Save();

				var (eInvoice, jsonPayload) = ConvertAndGetPayload(batch, amendTransaction);
				CombineAssertions(() =>
				{
					AssertEquals("ud", expectedUd, jsonPayload["inv"]["ud"].ToObject<int>());
					if (expectedMessageType != null)
					{
						AssertEquals("Message Type", expectedMessageType, eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
					}

					var item = jsonPayload["inv"]["items"][0];
					AssertEquals("item.type", expectedItemType, item["type"].ToObject<string>());
					AssertEquals("item.status", expectedItemStatus, item["status"].ToObject<int>());
				});
			}
		}

		(AccEInvoicingBatch batch, OrgAddress address) CreateBatchAndAddress()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);

			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor, OrgAddressType.Receivables, true, "Address1", "Address2", "Test City", "01", "VN", "1234", "", "");
			address.PrimaryOrgAddressAdditionalInfoDetail = "Additional Information";
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "VNLAP";
			TestObjectCreator.Debtor.Addresses.Add(address);

			return (batch, address);
		}

		ARInvoice CreateARInvoiceOriginal(GlbBranch branch, Job job, OrgAddress address)
		{
			var arInvoiceOriginal = Factory.NewWithPrimaryKey<ARInvoice>(Guid.NewGuid());
			arInvoiceOriginal.AH_GC = branch.GB_GC;
			arInvoiceOriginal.AH_OH = TestObjectCreator.Debtor.PK;
			arInvoiceOriginal.AH_JH = job?.PK ?? ZGuid.Empty;
			arInvoiceOriginal.AH_Desc = "TEST";
			arInvoiceOriginal.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoiceOriginal.AH_TransactionType = TransactionTypes.Invoice;
			arInvoiceOriginal.AH_TransactionNum = TestObjectCreator.GetRandomString(5);
			arInvoiceOriginal.AH_GB = branch.PK;
			arInvoiceOriginal.AH_GE = GlbDepartment.CurrentDepartment.PK;
			arInvoiceOriginal.AH_InvoiceDate = ZDateTime.Today;
			arInvoiceOriginal.AH_PostDate = ZDateTime.Today;
			arInvoiceOriginal.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			arInvoiceOriginal.AH_TransactionReference = "XI12345";

			TestObjectCreator.CreateInvoiceLine(arInvoiceOriginal, TestObjectCreator.EUR, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
			arInvoiceOriginal.Lines[0].GenericCharge = TestObjectCreator.CC1.PK;
			arInvoiceOriginal.AH_OA_InvoiceAddressOverride = address?.PK ?? ZGuid.Empty;
			arInvoiceOriginal.Header.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			arInvoiceOriginal.AH_ChequeOrReference = "SellReference";

			return arInvoiceOriginal;
		}

		(GlobalElectronicInvoicing eInvoice, JObject jsonPayload) ConvertAndGetPayload(AccEInvoicingBatch batch, object transaction)
		{
			var invoicingTransaction = (InvoicingBase)transaction;
			var line = invoicingTransaction.Lines.Cast<InvoicingLineBase>().First();
			line.AL_LineAmount = 100m;
			line.AL_OSAmount = 50m;

			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoicingTransaction, Core.Constants.EInvoicingPivotState.Batched, Constants.EInvoicingPivotActionType.Adjustment);
			TestObjectCreator.SetCustomsCodeForOrgHeader(invoicingTransaction.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "12345675");

			Factory.Save();

			var converter = new TransactionBatchToGEIConverterForVietnam();
			var (eInvoice, _, _) = converter.Convert(batch);
			var jsonPayload = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(eInvoice.Payload)));

			return (eInvoice, jsonPayload);
		}

		void AssertEInvoice(VietnamEInvoiceAdjustment eInvoice, InvoicingBase invoice, InvoicingBase creditNote, string vietnamProxyVatNumber, string debtorVatNumber = "", string bankName = "", string accountNumber = "", string language = "vi", ForwardingConsol consol = null, ForwardingShipment shipment = null, BaseJobDeclaration declaration = null, string sequenceNumber = "", string originalSequenceNumber = "", AdditionalContactInfo contactInfo = null)
		{
			var originalSeq = $"{form}-{prefix}-{originalSequenceNumber}";
			var seq = sequenceNumber;

			CombineAssertions(() =>
			{
				// User
				AssertNullOrEmpty(eInvoice.User.Username);
				AssertNullOrEmpty(eInvoice.User.Password);
				// Lang
				AssertEquals("language", language, eInvoice.Language);
				// Inv.Adj
				AssertEquals("rdt", creditNote.AH_PostDate.ToString("yyyy-MM-dd HH:mm"), eInvoice.Inv.Adj.Rdt);
				AssertEquals("rea", creditNote.AH_Desc, eInvoice.Inv.Adj.Rea);
				AssertEquals("ref", creditNote.SupportingDocumentNumber, eInvoice.Inv.Adj.Ref);
				AssertEquals("seq", originalSeq, eInvoice.Inv.Adj.Seq);
				// Inv
				AssertEquals("aun", 1, eInvoice.Inv.Aun);
				AssertEquals("sid", creditNote.PK.ToString(), eInvoice.Inv.Sid);
				AssertEquals("idt", creditNote.AH_ComplianceDocumentDate.ToString("yyyy-MM-dd HH:mm"), eInvoice.Inv.Idt);
				AssertEquals("type", VietnamEInvoiceHelper.EInvoiceType, eInvoice.Inv.Type);
				AssertEquals("form", creditNote.Branch.PK == TestObjectCreator.NonCurrentBranch.PK ? form : string.Empty, eInvoice.Inv.Form);
				AssertEquals("serial", creditNote.ComplianceBook?.XD_Prefix ?? ZString.Empty, eInvoice.Inv.Serial);
				AssertEquals("bname", creditNote.Header.OH_FullName, eInvoice.Inv.Bname);
				AssertEquals("seq", seq, eInvoice.Inv.Seq);
				AssertEquals("buyer", contactInfo?.Name ?? ZString.Empty, eInvoice.Inv.Buyer);
				AssertEquals("btax", debtorVatNumber, eInvoice.Inv.Btax);
				AssertEquals("baddr", "test formatted address", eInvoice.Inv.Baddr);
				AssertEquals("btel", contactInfo?.Phone ?? ZString.Empty, eInvoice.Inv.Btel);
				AssertEquals("bmail", VietnamEInvoiceHelper.GetContactEmails(contactInfo), eInvoice.Inv.Bmail);
				AssertEquals("baym", "TM/CK", eInvoice.Inv.Paym);
				AssertEquals("curr", creditNote.AH_RX_NKTransactionCurrency, eInvoice.Inv.Curr);
				AssertEquals("exrt", creditNote.AH_ExchangeRate, eInvoice.Inv.Exrt);
				AssertEquals("bacc", accountNumber, eInvoice.Inv.Bacc);
				AssertEquals("bbank", bankName, eInvoice.Inv.Bbank);
				AssertEquals("note", creditNote.AH_Desc, eInvoice.Inv.Note);
				AssertEquals("sumv", creditNote.AH_LocalExTaxAmount, eInvoice.Inv.Sumv);
				AssertEquals("sum", creditNote.AH_OSExTaxAmount, eInvoice.Inv.Sum);
				AssertEquals("vatv", creditNote.AH_LocalTaxAmount, eInvoice.Inv.Vatv);
				AssertEquals("vat", creditNote.AH_OSTaxAmount, eInvoice.Inv.Vat);
				AssertEquals("word", VietnamEInvoiceHelper.GetCurrencyToWord(creditNote.Company.PK.ToGuid(), creditNote.Branch.PK.ToGuid(), creditNote.TransactionCurrency.Code, (double)creditNote.AH_LocalTotalAmount, (double)creditNote.AH_LocalTotalAmount), eInvoice.Inv.Word);
				AssertContains("word2", "dong", eInvoice.Inv.Word);
				AssertEquals("totalv", creditNote.AH_LocalTotalAmount, eInvoice.Inv.Totalv);
				AssertEquals("discount", "", eInvoice.Inv.Discount);
				AssertEquals("total", creditNote.AH_OSTotalAmount, eInvoice.Inv.Total);
				AssertEquals("c0", creditNote.AH_DueDate.ToShortDateString() ?? string.Empty, eInvoice.Inv.DueDate);

				if (shipment != null && creditNote.Job.JH_ParentID == shipment.PK)
				{
					AssertEquals("c1", shipment.Consignor?.OH_FullName, eInvoice.Inv.Consignor);
					AssertEquals("c2", shipment.JS_UniqueConsignRef, eInvoice.Inv.Reference);
					AssertEquals("c3", shipment.Consignee?.OH_FullName, eInvoice.Inv.Consignee);
					AssertEquals("c4", shipment.JS_E_DEP.ToShortDateString(), eInvoice.Inv.ETD);
					AssertEquals("c5", consol != null ? consol.JK_JX_JV_NKVessel + " / " + consol.JK_JX_JV_VoyageFlight : string.Empty, eInvoice.Inv.VoyageFlightJourneyDetails);
					AssertEquals("c6", shipment.JS_E_ARV.ToShortDateString(), eInvoice.Inv.ETA);
					AssertEquals("c7", consol?.JK_MasterBillNum, eInvoice.Inv.MasterBill);
					AssertEquals("c8", shipment.JS_HouseBill, eInvoice.Inv.HouseBill);

					var additionalInfoC9 = new AdditionalInfoC9()
					{
						CreditTerms = "Cash on Delivery",
						Origin = shipment.Origin != null ? string.Format($"{shipment.Origin.Code} - {shipment.Origin.Description}.{shipment.Origin.Country.RN_DescMultilingual}") : string.Empty,
						Destination = shipment.Destination != null ? string.Format($"{shipment.Destination.Code} - {shipment.Destination.Description}.{shipment.Destination.Country.RN_DescMultilingual}") : string.Empty,
						Incoterms = shipment.JS_INCO + " - " + shipment.GetINCOTermDescription(Factory),
						SellReference = creditNote.AH_ChequeOrReference,
						TransactionNumber = creditNote.AH_ConsolidatedInvoiceRef,
						TransportMode = shipment.TransportMode,
					};

					var additionalInfoC10 = new AdditionalInfoC10()
					{
						OwnerReference = shipment.Declarations?.FirstOrDefault()?.JE_OwnerRef,
						OrderReference = shipment.DocsAndCartage?.JP_OrderItemsAsString ?? ZString.Empty,
						InvoiceDepartmentDesc = creditNote.Department.GE_Desc,
						ExternalSystemDebtorCode = creditNote.Header.CompanyData.OB_ARExternalDebtorCode,
						GoodDescription = shipment.JS_GoodsDescription,
						Package = shipment.JS_OuterPacks.ToString() + " " + shipment.JS_F3_NKPackType,
					};

					var additionalInfoC11 = new AdditionalInfoC11()
					{
						Weight = shipment.JS_ActualWeight.ToString() + " " + shipment.JS_UnitOfWeight,
						ChargeableWeight = shipment.JS_ActualChargeable.ToString() + " " + shipment.JS_ChargeableUnit,
						Volume = shipment.JS_ActualVolume.ToString() + " " + shipment.JS_UnitOfVolume,
						SendingAgent = string.Empty,
						ReceivingAgent = string.Empty,
						LineCarrier = string.Empty,
					};

					var additionalInfoC12 = new AdditionalInfoC12()
					{
						InvoiceCreatingStaff = creditNote.CheckRequesterUserFullName,
					};

					var additionalInfoC13 = new AdditionalInfoC13();

					if (shipment.Job != null)
					{
						additionalInfoC10.JobDepartmentDesc = shipment.Job.Department.GE_Desc;
						additionalInfoC12.JobOperationStaff = shipment.Job.RepOps.GS_FullName;
						additionalInfoC13.JobSalesStaff = shipment.Job.RepSales.GS_FullName;
					}

					AssertEquals("c9", JsonConvert.SerializeObject(additionalInfoC9), eInvoice.Inv.AdditionalInfoC9);
					AssertEquals("c10", JsonConvert.SerializeObject(additionalInfoC10), eInvoice.Inv.AdditionalInfoC10);
					AssertEquals("c11", JsonConvert.SerializeObject(additionalInfoC11), eInvoice.Inv.AdditionalInfoC11);
					AssertEquals("c12", JsonConvert.SerializeObject(additionalInfoC12), eInvoice.Inv.AdditionalInfoC12);
					AssertEquals("c13", JsonConvert.SerializeObject(additionalInfoC13), eInvoice.Inv.AdditionalInfoC13);
				}
				else if (consol != null && creditNote.Job.JH_ParentID == consol.PK)
				{
					AssertEquals("c1", string.Empty, eInvoice.Inv.Consignor);
					AssertEquals("c2", consol.JK_UniqueConsignRef, eInvoice.Inv.Reference);
					AssertEquals("c3", string.Empty, eInvoice.Inv.Consignee);
					AssertEquals("c4", consol.MostInterestingTransportForBinding[0].JW_ETDForBinding.ToShortDateString(), eInvoice.Inv.ETD);
					AssertEquals("c5", consol.JK_JX_JV_NKVessel + " / " + consol.JK_JX_JV_VoyageFlight, eInvoice.Inv.VoyageFlightJourneyDetails);
					AssertEquals("c6", consol.MostInterestingTransportForBinding[0].JW_ETAForBinding.ToShortDateString(), eInvoice.Inv.ETA);
					AssertEquals("c7", consol.JK_MasterBillNum, eInvoice.Inv.MasterBill);
					AssertEquals("c8", string.Empty, eInvoice.Inv.HouseBill);

					var additionalInfoC9 = new AdditionalInfoC9()
					{
						CreditTerms = "Cash on Delivery",
						Origin = consol.LoadPort != null ? string.Format($"{consol.LoadPort.Code} - {consol.LoadPort.Description}.{consol.LoadPort.Country.RN_DescMultilingual}") : string.Empty,
						Destination = consol.DischargePort != null ? string.Format($"{consol.DischargePort.Code} - {consol.DischargePort.Description}.{consol.DischargePort.Country.RN_DescMultilingual}") : string.Empty,
						Incoterms = string.Empty,
						SellReference = creditNote.AH_ChequeOrReference,
						TransactionNumber = creditNote.AH_ConsolidatedInvoiceRef,
						TransportMode = string.Empty,
					};

					var additionalInfoC10 = new AdditionalInfoC10()
					{
						OwnerReference = string.Empty,
						OrderReference = string.Empty,
						InvoiceDepartmentDesc = creditNote.Department.GE_Desc,
						ExternalSystemDebtorCode = creditNote.Header.CompanyData.OB_ARExternalDebtorCode,
						GoodDescription = string.Empty,
						Package = consol.JK_TotalShipmentQuantity.ToString() + " PACKS",
					};

					var additionalInfoC11 = new AdditionalInfoC11()
					{
						Weight = consol.JK_TotalShipmentWeight.ToString() + " " + consol.JK_TotalShipmentWeightUnit,
						ChargeableWeight = consol.JK_TotalShipmentChargeable.ToString() + " " + consol.JK_ConsolChargeableUnit,
						Volume = consol.JK_TotalShipmentVolume.ToString() + " " + consol.JK_TotalShipmentVolumeUnit,
						SendingAgent = consol.SendingForwarder?.OH_FullName ?? string.Empty,
						ReceivingAgent = consol.ReceivingForwarder?.OH_FullName ?? string.Empty,
						LineCarrier = ((OrgHeader)consol.JK_OA_ShippingLineAddress_ZAddress?.OrgHeader).OH_FullName,
					};

					var additionalInfoC12 = new AdditionalInfoC12()
					{
						InvoiceCreatingStaff = creditNote.CheckRequesterUserFullName,
					};

					var additionalInfoC13 = new AdditionalInfoC13();

					if (consol.Job != null)
					{
						additionalInfoC10.JobDepartmentDesc = consol.Job.Department.GE_Desc;
						additionalInfoC12.JobOperationStaff = consol.Job.RepOps.GS_FullName;
						additionalInfoC13.JobSalesStaff = consol.Job.RepSales.GS_FullName;
					}

					AssertEquals("c9", JsonConvert.SerializeObject(additionalInfoC9), eInvoice.Inv.AdditionalInfoC9);
					AssertEquals("c10", JsonConvert.SerializeObject(additionalInfoC10), eInvoice.Inv.AdditionalInfoC10);
					AssertEquals("c11", JsonConvert.SerializeObject(additionalInfoC11), eInvoice.Inv.AdditionalInfoC11);
					AssertEquals("c12", JsonConvert.SerializeObject(additionalInfoC12), eInvoice.Inv.AdditionalInfoC12);
					AssertEquals("c13", JsonConvert.SerializeObject(additionalInfoC13), eInvoice.Inv.AdditionalInfoC13);
				}
				else if (declaration != null && creditNote.Job.JH_ParentID == declaration.PK)
				{
					AssertEquals("c1", declaration.Supplier.OH_FullName, eInvoice.Inv.Consignor);
					AssertEquals("c2", declaration.JE_DeclarationReference, eInvoice.Inv.Reference);
					AssertEquals("c3", declaration.Importer.OH_FullName, eInvoice.Inv.Consignee);
					AssertEquals("c4", declaration.JE_DateAtFinalDestination.ToShortDateString(), eInvoice.Inv.ETD);
					AssertEquals("c5", "", eInvoice.Inv.VoyageFlightJourneyDetails);
					AssertEquals("c6", declaration.JE_DateAtOrigin.ToShortDateString(), eInvoice.Inv.ETA);
					AssertEquals("c7", declaration.JE_MasterBill, eInvoice.Inv.MasterBill);
					AssertEquals("c8", declaration.JE_HouseBill, eInvoice.Inv.HouseBill);

					var additionalInfo = new AdditionalInfoC9()
					{
						CreditTerms = "Cash on Delivery",
						Origin = declaration.Origin != null ? string.Format($"{declaration.Origin.Code} - {declaration.Origin.Description}.{declaration.Origin.Country.RN_DescMultilingual}") : string.Empty,
						Destination = declaration.FinalDestination != null ? string.Format($"{declaration.FinalDestination.Code} - {declaration.FinalDestination.Description}.{declaration.FinalDestination.Country.RN_DescMultilingual}") : string.Empty,
						Incoterms = declaration.IncoTerm + " - " + declaration.GetINCOTermDescription(Factory),
						SellReference = creditNote.AH_ChequeOrReference,
						TransactionNumber = creditNote.AH_ConsolidatedInvoiceRef,
						TransportMode = declaration.TransportMode,
					};

					var additionalInfoC10 = new AdditionalInfoC10()
					{
						OwnerReference = declaration.JE_OwnerRef,
						OrderReference = declaration.DocsAndCartage.JP_OrderItemsAsString,
						InvoiceDepartmentDesc = creditNote.Department.GE_Desc,
						ExternalSystemDebtorCode = creditNote.Header.CompanyData.OB_ARExternalDebtorCode,
						GoodDescription = declaration.JE_GoodsDescription,
						Package = declaration.JE_TotalNoOfPacks.ToString() + " PACKS",
					};

					var additionalInfoC11 = new AdditionalInfoC11()
					{
						Weight = declaration.JE_TotalWeight.ToString() + " " + declaration.JE_TotalWeightUnit,
						ChargeableWeight = string.Empty,
						Volume = declaration.JE_TotalVolume.ToString() + " " + declaration.JE_TotalVolumeUnit,
						SendingAgent = string.Empty,
						ReceivingAgent = string.Empty,
						LineCarrier = declaration.ShippingLine.OH_FullName,
					};

					var additionalInfoC12 = new AdditionalInfoC12()
					{
						InvoiceCreatingStaff = creditNote.CheckRequesterUserFullName,
					};

					var additionalInfoC13 = new AdditionalInfoC13();

					if (declaration.Job != null)
					{
						additionalInfoC10.JobDepartmentDesc = declaration.Job.Department.GE_Desc;
						additionalInfoC12.JobOperationStaff = declaration.Job.RepOps.GS_FullName;
						additionalInfoC13.JobSalesStaff = declaration.Job.RepSales.GS_FullName;
					}

					AssertEquals("c9", JsonConvert.SerializeObject(additionalInfo), eInvoice.Inv.AdditionalInfoC9);
					AssertEquals("c10", JsonConvert.SerializeObject(additionalInfoC10), eInvoice.Inv.AdditionalInfoC10);
					AssertEquals("c11", JsonConvert.SerializeObject(additionalInfoC11), eInvoice.Inv.AdditionalInfoC11);
					AssertEquals("c12", JsonConvert.SerializeObject(additionalInfoC12), eInvoice.Inv.AdditionalInfoC12);
					AssertEquals("c13", JsonConvert.SerializeObject(additionalInfoC13), eInvoice.Inv.AdditionalInfoC13);
				}

				// Item
				var line = creditNote.Lines.Cast<TransactionLine>().First();
				var osTaxAmount = TaxAmountCalculator.SplitOSTotalToTaxAndExTaxAmounts(line.AL_GSTVAT, () => Export.Business.ExchangeRate.LocalToForeign(line.AL_LineAmount, line.AL_ExchangeRate, line.TransactionCurrency.RX_SubUnitRatio, line.Company.GC_IsReciprocal), line.AL_OSAmount).oSTaxAmount;
				AssertEquals("type", VietnamEInvoiceHelper.CreditNoteType, eInvoice.Inv.Items[0].Type);
				AssertEquals("vrt", line.AL_TaxRateCalc.ToString(), eInvoice.Inv.Items[0].VRT);
				AssertEquals("name", line.AL_Desc.Replace("\r\n", " "), eInvoice.Inv.Items[0].Name);
				AssertEquals("unit", "override unit", eInvoice.Inv.Items[0].Unit);
				AssertEquals("amount", (line.AL_OSAmount - osTaxAmount) * -1, eInvoice.Inv.Items[0].Amount);
				AssertEquals("amountv", line.AL_LocalExTaxAmount, eInvoice.Inv.Items[0].LocalAmount);
				AssertEquals("price", (line.AL_OSAmount - osTaxAmount) * -1, eInvoice.Inv.Items[0].Price);
				AssertEquals("quantity", 1m, eInvoice.Inv.Items[0].Quantity);
				AssertEquals("vat", osTaxAmount * -1, eInvoice.Inv.Items[0].VAT);
				AssertEquals("vatv", line.AL_LocalTaxAmount, eInvoice.Inv.Items[0].LocalTax);
				AssertEquals("total", line.AL_OverseasTotal, eInvoice.Inv.Items[0].Total);
				AssertEquals("totalv", line.AL_LocalTotalAmount, eInvoice.Inv.Items[0].LocalTotal);
				AssertEquals("status", 0, eInvoice.Inv.Items[0].Status);
				AssertEquals("c0", line.AL_LocalTaxAmount.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals), eInvoice.Inv.Items[0].CustomizedLocalTax);
				AssertEquals("c1", line.AL_LocalTotalAmount.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals), eInvoice.Inv.Items[0].CustomizedLocalTotal);
				AssertEquals("c2", line.AL_LocalExTaxAmount.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals), eInvoice.Inv.Items[0].CustomizedLocalAmount);
				var isMultipleJobTransaction = creditNote.Lines.Select(x => x.Job).Distinct().Count() > 1;
				if (isMultipleJobTransaction)
				{
					AssertEquals("c3", ((Job)line.Job).JH_HouseBillNo, eInvoice.Inv.Items[0].HouseBill);
				}
				else
				{
					AssertEquals("c3", ZString.Empty, eInvoice.Inv.Items[0].HouseBill);
				}
				AssertEquals("stax", vietnamProxyVatNumber, eInvoice.Inv.Stax);
			});
		}

		(VietnamEInvoiceAdjustment EInvoice, INotifications ValidationErrors) CreateVietnamEInvoice(AccEInvoicingBatch eInvoiceBatch, Action<UniversalTransactionInfo> modifyTransactionInfo = null, Func<AdditionalTransactionInfoForVietnamEInvoice> createAdditionalTransactionInfoForVietnamEInvoice = null)
		{
			var universalTransactionBatch = CreateUniversalTransactionBatch(eInvoiceBatch);
			var universalTransaction = universalTransactionBatch.TransactionCollection[0];
			modifyTransactionInfo?.Invoke(universalTransaction);
			var notifications = new Logger();
			var eInvoiceCreator = new VietnamEInvoiceAdjustmentCreator(universalTransaction, createAdditionalTransactionInfoForVietnamEInvoice?.Invoke());
			var eInvoice = eInvoiceCreator.Create();
			return (eInvoice, notifications);
		}

		AccEInvoicingBatch CreateEInvoicingBatch(InvoicingBase invoice)
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Batched, Core.Constants.EInvoicingPivotActionType.Adjustment);
			return batch;
		}

		UniversalTransactionBatch CreateUniversalTransactionBatch(AccEInvoicingBatch batch)
		{
			var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
			var exportor = new TransactionBatchExporter(dataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
			var transactionBatch = exportor.CreateTransactionBatch(batch);
			return transactionBatch;
		}

		readonly string form = "FormNumber";
		readonly string prefix = "TR/21E";

		protected override void SetUp()
		{
			base.SetUp();
			Helper.SetupControlAccounts();

			var collecetion = new UnitMeasurementTextOverrideCollection();
			var unit = collecetion.AddNew();
			unit.UnitMeasurement = "Unit";
			unit.TextOverride = "override unit";
			AccountingConfigurationRegistry.Instance.UnitMeasurementTextOverride.SetTemporaryValue(Guid.Empty, TestObjectCreator.NonCurrentBranch.PK.ToGuid(), Guid.Empty, collecetion);
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "admin");
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "123456");
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingFormNumber.SetTemporaryValue(Guid.Empty, TestObjectCreator.NonCurrentBranch.PK.ToGuid(), Guid.Empty, form);
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected EInvoicingTestHelper Helper => helper ?? (helper = new EInvoicingTestHelper(TestObjectCreator));
		EInvoicingTestHelper helper;
	}
}
