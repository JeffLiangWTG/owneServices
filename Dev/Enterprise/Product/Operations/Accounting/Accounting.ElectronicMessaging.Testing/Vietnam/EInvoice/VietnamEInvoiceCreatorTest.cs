using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;
using UniversalTransactionInfo = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.Testing
{
	public class VietnamEInvoiceCreatorTest : TestCaseWithFactory
	{
		public void TestVietnamEInvoiceCreation()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var complianceSequence1 = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence1.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence1.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence1.XD_Code = "AAA";
			complianceSequence1.XD_SequenceClass = "TXI";
			complianceSequence1.XD_Prefix = "PREFIX/";
			complianceSequence1.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
			complianceSequence1.XD_IsActive = true;

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 0.2m, 20m, 2m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			arInvoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			arInvoice.AH_TransactionReference = "PREFIX/AP/19E1";
			arInvoice.AH_XD_ComplianceBook = complianceSequence1.PK;
			arInvoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			arInvoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			arInvoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			arInvoice.AH_ComplianceDocumentDate = ZDate.Today;
			TestObjectCreator.Debtor.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			arInvoice.AH_ChequeOrReference = "SellReference";

			var line = arInvoice.Lines.Cast<ARInvoiceLine>().First();
			line.AL_AT = taxRate.PK;
			line.AL_Desc = "Line1\r\nLine2\r\nLine3";
			line.AL_OSTaxAmount = 2m;
			line.AL_OverseasTotal = 22m;
			line.AL_LocalTaxAmount = 10m;
			line.AL_LocalExTaxAmount = 100m;
			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "en");

			var contactInfo = new AdditionalContactInfo()
			{
				Name = "test contact1",
				Mails = new List<ZString>() { "contact1@client1.com", "contact2@client2.com", "contact3@client3.com" },
				Phone = "13002191911"
			};

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() {
				OriginalTransactionPK = arInvoice.PK,
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				TransactionReference = arInvoice.AH_TransactionReference,
				VATRegistrationNum = "0104128565-999",
				BankName = "bank test",
				AccountNumber = "account test number",
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					SeriesPrefix = arInvoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
					IsComplianceNumberFormatDefault = true,
					ComplianceSequenceMaximumNumberDigits = arInvoice.ComplianceBook.XD_MaximumNumberDigits,
				},
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK,
				ComplianceDocumentDate = arInvoice.AH_ComplianceDocumentDate,
				FormattedAddress = "test formatted address",
				InvoiceCreatingStaff = arInvoice.CheckRequesterUserFullName,
				ContactInfo = contactInfo,
			});

			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", line.AL_OSExTaxAmount, 1m, "0100233488", "0104128565-999", "bank test", "account test number", "en", contactInfo: contactInfo);
		}

		public void TestVietnamEInvoiceCreation_TaxRateMIDVAT()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("MIDVAT", "Rate", 7);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var complianceSequence1 = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence1.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence1.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence1.XD_Code = "AAA";
			complianceSequence1.XD_SequenceClass = "TXI";
			complianceSequence1.XD_Prefix = "PREFIX/";
			complianceSequence1.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
			complianceSequence1.XD_IsActive = true;

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 0.2m, 20m, 2m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			arInvoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			arInvoice.AH_TransactionReference = "PREFIX/AP/19E1";
			arInvoice.AH_XD_ComplianceBook = complianceSequence1.PK;
			arInvoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			arInvoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			arInvoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			arInvoice.AH_ComplianceDocumentDate = ZDate.Today;
			TestObjectCreator.Debtor.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			arInvoice.AH_ChequeOrReference = "SellReference";

			var line = arInvoice.Lines.Cast<ARInvoiceLine>().First();
			line.AL_AT = taxRate.PK;
			line.AL_Desc = "Line1\r\nLine2\r\nLine3";
			line.AL_OSTaxAmount = 2m;
			line.AL_OverseasTotal = 22m;
			line.AL_LocalTaxAmount = 10m;
			line.AL_LocalExTaxAmount = 100m;
			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "en");

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice()
			{
				OriginalTransactionPK = arInvoice.PK,
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				TransactionReference = arInvoice.AH_TransactionReference,
				VATRegistrationNum = "0104128565-999",
				BankName = "bank test",
				AccountNumber = "account test number",
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					SeriesPrefix = arInvoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
					IsComplianceNumberFormatDefault = true,
				},
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK,
				ComplianceDocumentDate = arInvoice.AH_ComplianceDocumentDate,
				FormattedAddress = "test formatted address",
				InvoiceCreatingStaff = arInvoice.CheckRequesterUserFullName,
			});

			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", line.AL_OSExTaxAmount, 1m, "0100233488", "0104128565-999", "bank test", "account test number", "en");
		}

		public void TestVietnamEInvoiceCreation_TaxRateNOTREPORT()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("NOTREPORT", "Rate", 0);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var complianceSequence1 = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence1.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence1.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence1.XD_Code = "AAA";
			complianceSequence1.XD_SequenceClass = "TXI";
			complianceSequence1.XD_Prefix = "PREFIX/";
			complianceSequence1.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
			complianceSequence1.XD_IsActive = true;

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 0.2m, 20m, 2m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			arInvoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			arInvoice.AH_TransactionReference = "PREFIX/AP/19E1";
			arInvoice.AH_XD_ComplianceBook = complianceSequence1.PK;
			arInvoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			arInvoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			arInvoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			arInvoice.AH_ComplianceDocumentDate = ZDate.Today;
			TestObjectCreator.Debtor.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			arInvoice.AH_ChequeOrReference = "SellReference";

			var line = arInvoice.Lines.Cast<ARInvoiceLine>().First();
			line.AL_AT = taxRate.PK;
			line.AL_Desc = "Line1\r\nLine2\r\nLine3";
			line.AL_OSTaxAmount = 2m;
			line.AL_OverseasTotal = 22m;
			line.AL_LocalTaxAmount = 10m;
			line.AL_LocalExTaxAmount = 100m;
			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "en");

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice()
			{
				OriginalTransactionPK = arInvoice.PK,
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				TransactionReference = arInvoice.AH_TransactionReference,
				VATRegistrationNum = "0104128565-999",
				BankName = "bank test",
				AccountNumber = "account test number",
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					SeriesPrefix = arInvoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
					IsComplianceNumberFormatDefault = true,
				},
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK,
				ComplianceDocumentDate = arInvoice.AH_ComplianceDocumentDate,
				FormattedAddress = "test formatted address",
				InvoiceCreatingStaff = arInvoice.CheckRequesterUserFullName,
			});

			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", line.AL_OSExTaxAmount, 1m, "0100233488", "0104128565-999", "bank test", "account test number", "en");
		}

		public void TestVietnamEInvoiceCreation_ExportAmountInWordsBasedOnInvoicedCurrency()
		{
			using (AccountingConfigurationRegistry.Instance.VietnamExportAmountInWordsBasedOnInvoicedCurrency.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
				var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
				var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

				var complianceSequence1 = Factory.NewWithValidTestData<AccComplianceSequence>();
				complianceSequence1.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				complianceSequence1.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				complianceSequence1.XD_Code = "AAA";
				complianceSequence1.XD_SequenceClass = "TXI";
				complianceSequence1.XD_Prefix = "PREFIX/";
				complianceSequence1.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
				complianceSequence1.XD_IsActive = true;

				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 0.2m, 20m, 2m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
				arInvoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
				arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
				arInvoice.AH_TransactionReference = "PREFIX/AP/19E1";
				arInvoice.AH_XD_ComplianceBook = complianceSequence1.PK;
				arInvoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
				arInvoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
				arInvoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
				arInvoice.AH_ComplianceDocumentDate = ZDate.Today;
				arInvoice.AH_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
				TestObjectCreator.Debtor.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
				arInvoice.AH_ChequeOrReference = "SellReference";

				var line = arInvoice.Lines.Cast<ARInvoiceLine>().First();
				line.AL_AT = taxRate.PK;
				line.AL_Desc = "Line1\r\nLine2\r\nLine3";
				line.AL_OSTaxAmount = 2m;
				line.AL_OverseasTotal = 22m;
				line.AL_LocalTaxAmount = 10m;
				line.AL_LocalExTaxAmount = 100m;
				line.AL_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
				var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

				TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

				Factory.Save();

				AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "en");

				var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice()
				{
					OriginalTransactionPK = arInvoice.PK,
					OriginalTransactionReference = arInvoice.AH_TransactionReference,
					TransactionReference = arInvoice.AH_TransactionReference,
					VATRegistrationNum = "0104128565-999",
					BankName = "bank test",
					AccountNumber = "account test number",
					ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
					{
						SeriesPrefix = arInvoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
						IsComplianceNumberFormatDefault = true,
					},
					CompanyPK = arInvoice.Company.PK,
					BranchPK = arInvoice.Branch.PK,
					ComplianceDocumentDate = arInvoice.AH_ComplianceDocumentDate,
					FormattedAddress = "test formatted address",
					InvoiceCreatingStaff = arInvoice.CheckRequesterUserFullName,
				});

				AssertNotNull("EInvoice", eInvoice);
				AssertEInvoice(eInvoice, arInvoice, "unit", line.AL_OSExTaxAmount, 1m, "0100233488", "0104128565-999", "bank test", "account test number", "en");
			}
		}

		public void TestVietnamEInvoiceCreateInLocalCurrency()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 0.2m, 20m, 2m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			arInvoice.AH_TransactionReference = "PREFIX/AP/19E1";
			arInvoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			arInvoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			arInvoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			arInvoice.AH_ComplianceDocumentDate = ZDate.Today;
			arInvoice.AH_RX_NKTransactionCurrency = "USD";
			TestObjectCreator.Debtor.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			arInvoice.AH_ChequeOrReference = "SellReference";

			var line = arInvoice.Lines.Cast<ARInvoiceLine>().First();
			line.AL_AT = taxRate.PK;
			line.AL_Desc = "Line1\r\nLine2\r\nLine3";
			line.AL_OSTaxAmount = 2m;
			line.AL_OverseasTotal = 22m;
			line.AL_LocalTaxAmount = 10m;
			line.AL_LocalExTaxAmount = 100m;
			line.AL_RX_NKTransactionCurrency = "USD";
			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamAlwaysIssueElectronicInvoicesInLocalCurrency.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.VietnamExportAmountInWordsBasedOnInvoicedCurrency.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice()
			{
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					SeriesPrefix = arInvoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
					IsComplianceNumberFormatDefault = true,
				},
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK,
			});

			AssertNotNull("EInvoice", eInvoice);

			AssertEquals("AUD", eInvoice.Inv.Curr);
			AssertEquals(1m, eInvoice.Inv.Exrt);
			AssertEquals(100m, eInvoice.Inv.Sum);
			AssertEquals(110m, eInvoice.Inv.Total);
			AssertEquals(10m, eInvoice.Inv.Vat);
			AssertEquals("Một trăm mười dong", eInvoice.Inv.Word);

			var item = eInvoice.Inv.Items[0];
			AssertEquals(100m, item.Amount);
			AssertEquals(10m, item.VAT);
			AssertEquals(110m, item.Total);
			AssertEquals(100m, item.Price);
		}

		public void TestVietnamEInvoiceCreateByShipment()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "AUMEL", "C0001");
			consol.JK_MasterBillNum = "TestMAWB";

			var consol1 = testObjectCreator.CreateConsol("AUMEL", "CNSHA", "C0002");
			consol1.JK_MasterBillNum = "TestMAWB1";

			var transport = consol.MostInterestingTransportForBinding[0];
			transport.JW_VoyageFlight = "TestFlight";
			transport.JW_Vessel = "TestVessel";

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
			consol.Shipments.Add(shipment);

			shipment.Consols.Add(consol1);
			AssertEquals("Precondition", 2, shipment.Consols.Count);

			shipment.DocsAndCartage.JP_OrderItemsAsString = "TestOrderReference";

			var declaration = TestObjectCreator.CreateDeclaration("D0001");
			declaration.JE_JS = shipment.PK;
			declaration.JE_OwnerRef = "OwnerReference";

			var job = TestObjectCreator.CreateJob(shipment);
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);
			job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			job.JH_GS_NKRepOps = TestObjectCreator.GS1.GS_Code;
			job.JH_GS_NKRepSales = TestObjectCreator.Staff.GS_Code;

			var jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = TestObjectCreator.CC1.PK;
			jobCharge.JR_LocalSellAmt = 100m;
			jobCharge.JR_OSSellAmt = 100m;
			jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			jobCharge.JR_AT_SellGSTRate = taxRate.PK;

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);
			arInvoice.AH_JH = job.PK;
			arInvoice.AH_TransactionReference = "AP/19E1";
			arInvoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			arInvoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			var line = arInvoice.Lines.Cast<InvoicingLineBase>().First();
			line.AL_JH = job.PK;
			line.AL_AT = taxRate.PK;
			jobCharge.JR_AL_ARLine = line.PK;

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);
			arInvoice.AH_TransactionReference = "AP/19E1";
			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() {
				OriginalTransactionPK = arInvoice.PK,
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				VATRegistrationNum = "0104128565-999",
				BankName = "bank test",
				AccountNumber = "account test number",
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK,
				IsConsolInvoice = false,
				ETA = shipment.JS_E_ARV.ToShortDateString(),
				ETD = shipment.JS_E_DEP.ToShortDateString(),
				OrderReference = shipment.DocsAndCartage.JP_OrderItemsAsString,
				TransactionNumber = arInvoice.AH_TransactionNum,
				TransactionReference = arInvoice.AH_TransactionReference,
				MasterBillNumber = consol.JK_MasterBillNum,
				TransportInfo = consol.JK_JX_JV_NKVessel + " / " + consol.JK_JX_JV_VoyageFlight,
				Package = shipment.JS_OuterPacks.ToString() + " " + shipment.JS_F3_NKPackType,
				Weight = shipment.JS_ActualWeight.ToString() + " KG",
				ChargeableWeight = shipment.JS_ActualChargeable.ToString() + " " + shipment.JS_ChargeableUnit,
				Volume = shipment.JS_ActualVolume.ToString() + " M3",
				OriginPort = string.Format($"{shipment.Origin.Code} - {shipment.Origin.RL_PortName}.{shipment.Origin.Country.RN_DescMultilingual}"),
				DestinationPort = string.Format($"{shipment.Destination.Code} - {shipment.Destination.RL_PortName}.{shipment.Destination.Country.RN_DescMultilingual}"),
				FormattedAddress = "test formatted address",
				TransportMode = shipment.TransportMode,
				InvoiceCreatingStaff = arInvoice.CheckRequesterUserFullName,
				JobInfo = new AdditionalJobInfo()
				{
					JobDepartmentDesc = TestObjectCreator.NonCurrentDepartment.GE_Desc,
					JobOperationStaff = TestObjectCreator.GS1.GS_FullName,
					JobSalesStaff = TestObjectCreator.Staff.GS_FullName
				},
				ConsolID = consol.JK_UniqueConsignRef,
				TransactionParentID = shipment.JS_UniqueConsignRef,
			});
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", 100m, 1m, "0100233488", "0104128565-999", "bank test", "account test number", consol: consol, shipment: shipment);
		}

		public void TestVietnamEInvoiceCreateByConsol()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var consol = TestObjectCreator.CreateGatewayConsol(sendingGatewayCompany: GlbCompany.CurrentCompany, receivingGatewayCompany: GlbCompany.CurrentCompany);
			consol.JK_OA_ShippingLineAddress = TestObjectCreator.Creditor1.MainAddress.PK;
			consol.JK_MasterBillNum = "TestMAWB";
			consol.JK_RL_NKLoadPort = "VNHAN";

			var transport = consol.MostInterestingTransportForBinding[0];
			transport.JW_ETDForBinding = ZDateTime.Now.AddDays(-1);
			transport.JW_ETAForBinding = ZDateTime.Now;
			transport.JW_VoyageFlight = "TestFlight";
			transport.JW_Vessel = "TestVessel";

			var job = TestObjectCreator.CreateJob(consol);
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);
			job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			job.JH_GS_NKRepOps = TestObjectCreator.GS1.GS_Code;
			job.JH_GS_NKRepSales = TestObjectCreator.Staff.GS_Code;

			var jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = TestObjectCreator.CC1.PK;
			jobCharge.JR_LocalSellAmt = 100m;
			jobCharge.JR_OSSellAmt = 100m;
			jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			jobCharge.JR_AT_SellGSTRate = taxRate.PK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_OuterPacks = 2;
			shipment1.JS_F3_NKPackType = "CTN";
			shipment1.JS_TotalPackageCount = 2;
			shipment1.JS_ActualWeight = 1.5M;
			shipment1.JS_UnitOfWeight = "KG";
			shipment1.JS_ActualChargeable = 1.7M;
			shipment1.JS_ActualVolume = 5.2M;
			shipment1.JS_UnitOfVolume = "M3";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_OuterPacks = 2;
			shipment2.JS_F3_NKPackType = "CTN";
			shipment2.JS_TotalPackageCount = 2;
			shipment2.JS_ActualWeight = 1.5M;
			shipment2.JS_UnitOfWeight = "KG";
			shipment2.JS_ActualChargeable = 1.7M;
			shipment2.JS_ActualVolume = 5.2M;
			shipment2.JS_UnitOfVolume = "M3";

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);
			arInvoice.AH_JH = job.PK;
			arInvoice.AH_TransactionReference = "AP/19E1";
			arInvoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			arInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var line = arInvoice.Lines.Cast<InvoicingLineBase>().First();
			line.AL_JH = job.PK;
			line.AL_AT = taxRate.PK;
			jobCharge.JR_AL_ARLine = line.PK;

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);
			arInvoice.AH_TransactionReference = "AP/19E1";

			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice()
			{
				OriginalTransactionPK = arInvoice.PK,
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				VATRegistrationNum = "0104128565-999",
				BankName = "bank test",
				AccountNumber = "account test number",
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK,
				IsConsolInvoice = true,
				ETA = consol.JK_JX_JB_E_ARV.ToShortDateString(),
				ETD = consol.JK_JX_JA_E_DEP.ToShortDateString(),
				SendingAgent = consol.SendingForwarder?.OH_FullName ?? ZString.Empty,
				ReceivingAgent = consol.ReceivingForwarder?.OH_FullName ?? ZString.Empty,
				Carrier = TestObjectCreator.Creditor1.OH_FullName,
				TransactionNumber = arInvoice.AH_TransactionNum,
				TransactionReference = arInvoice.AH_TransactionReference,
				MasterBillNumber = consol.JK_MasterBillNum,
				TransportInfo = consol.JK_JX_JV_NKVessel + " / " + consol.JK_JX_JV_VoyageFlight,
				Package = consol.JK_TotalShipmentQuantity.ToString() + " PACKS",
				Weight = consol.JK_TotalShipmentWeight.ToString() + " KG",
				ChargeableWeight = consol.JK_TotalShipmentChargeable.ToString() + " " + consol.JK_ConsolChargeableUnit,
				Volume = consol.JK_TotalShipmentVolume.ToString() + " M3",
				OriginPort = string.Format($"{consol.LoadPort.Code} - {consol.LoadPort.RL_PortName}.{consol.LoadPort.Country.RN_DescMultilingual}"),
				DestinationPort = string.Format($"{consol.DischargePort.Code} - {consol.DischargePort.RL_PortName}.{consol.DischargePort.Country.RN_DescMultilingual}"),
				FormattedAddress = "test formatted address",
				InvoiceCreatingStaff = arInvoice.CheckRequesterUserFullName,
				JobInfo = new AdditionalJobInfo()
				{
					JobDepartmentDesc = TestObjectCreator.NonCurrentDepartment.GE_Desc,
					JobOperationStaff = TestObjectCreator.GS1.GS_FullName,
					JobSalesStaff = TestObjectCreator.Staff.GS_FullName
				},
			});
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", 100m, 1m, "0100233488", "0104128565-999", "bank test", "account test number", consol: consol);
		}

		public void TestVietnamEInvoiceCreateByDeclaration()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var declaration = (BaseJobDeclaration)TestObjectCreator.CreateDeclaration("B0001");
			declaration.JE_OH_Supplier = TestObjectCreator.AALSHI.PK;
			declaration.JE_OH_Importer = TestObjectCreator.ABIGAS.PK;
			declaration.JE_GoodsDescription = "GoodsDescription";
			declaration.JE_OwnerRef = "OwnerRef";
			declaration.DocsAndCartage.JP_OrderItemsAsString = "TestOrderReference";
			declaration.JE_MasterBill = "MasterBill";
			declaration.JE_HouseBill = "HouseBilling";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_DateAtOrigin = ZDateTime.Now.AddDays(-1);
			declaration.JE_DateAtFinalDestination = ZDateTime.Now;
			declaration.JE_OH_ShippingLine = TestObjectCreator.Creditor1.PK;
			declaration.JE_TotalNoOfPacks = 2;
			declaration.JE_TotalWeight = 30;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalVolume = 60M;
			declaration.JE_TotalVolumeUnit = "M3";
			declaration.JE_RL_NKOrigin = "VNHAN";
			declaration.JE_RL_NKFinalDestination = "TWTPE";

			var transport = declaration.TransportsIncludingRelated.AddNew();
			transport.JW_ETDForBinding = ZDateTime.Now.AddDays(-1);
			transport.JW_ETAForBinding = ZDateTime.Now;
			transport.JW_VoyageFlight = "TestFlight";
			transport.JW_Vessel = "TestVessel";

			var job = TestObjectCreator.CreateJob(declaration);
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);
			job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			job.JH_GS_NKRepOps = TestObjectCreator.GS1.GS_Code;
			job.JH_GS_NKRepSales = TestObjectCreator.Staff.GS_Code;

			var jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = TestObjectCreator.CC1.PK;
			jobCharge.JR_LocalSellAmt = 100m;
			jobCharge.JR_OSSellAmt = 100m;
			jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			jobCharge.JR_AT_SellGSTRate = taxRate.PK;

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);
			arInvoice.AH_JH = job.PK;
			arInvoice.AH_TransactionReference = "AP/19E1";
			arInvoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			arInvoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var line = arInvoice.Lines.Cast<InvoicingLineBase>().First();
			line.AL_JH = job.PK;
			line.AL_AT = taxRate.PK;
			jobCharge.JR_AL_ARLine = line.PK;

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);
			arInvoice.AH_TransactionReference = "AP/19E1";

			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice()
			{
				OriginalTransactionPK = arInvoice.PK,
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				TransactionReference = arInvoice.AH_TransactionReference,
				VATRegistrationNum = "0104128565-999",
				BankName = "bank test",
				AccountNumber = "account test number",
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK,
				IsDeclarationInvoice = true,
				ETA = declaration.JE_DateAtOrigin.ToShortDateString(),
				ETD = declaration.JE_DateAtFinalDestination.ToShortDateString(),
				SendingAgent = ZString.Empty,
				ReceivingAgent = ZString.Empty,
				Carrier = TestObjectCreator.Creditor1.OH_FullName,
				TransactionNumber = arInvoice.AH_TransactionNum,
				MasterBillNumber = declaration.JE_MasterBill,
				TransportMode = "AIR",
				TransportInfo = "",
				OrderReference = "TestOrderReference",
				Package = "2 PACKS",
				Weight = "30 KG",
				ChargeableWeight = ZString.Empty,
				Volume = "60 M3",
				OriginPort = string.Format($"{declaration.Origin.Code} - {declaration.Origin.RL_PortName}.{declaration.Origin.Country.RN_DescMultilingual}"),
				DestinationPort = string.Format($"{declaration.FinalDestination.Code} - {declaration.FinalDestination.RL_PortName}.{declaration.FinalDestination.Country.RN_DescMultilingual}"),
				FormattedAddress = "test formatted address",
				InvoiceCreatingStaff = arInvoice.CheckRequesterUserFullName,
				JobInfo = new AdditionalJobInfo()
				{
					JobDepartmentDesc = TestObjectCreator.NonCurrentDepartment.GE_Desc,
					JobOperationStaff = TestObjectCreator.GS1.GS_FullName,
					JobSalesStaff = TestObjectCreator.Staff.GS_FullName
				},
			});
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", 100m, 1m, "0100233488", "0104128565-999", "bank test", "account test number", declaration: declaration);
		}

		public void TestVietnamEInvoiceCreateByDeclaration_EmptyHouseBill()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var declaration = (BaseJobDeclaration)TestObjectCreator.CreateDeclaration("B0001");
			declaration.JE_OH_Supplier = TestObjectCreator.AALSHI.PK;
			declaration.JE_OH_Importer = TestObjectCreator.ABIGAS.PK;
			declaration.JE_GoodsDescription = "GoodsDescription";
			declaration.JE_OwnerRef = "OwnerRef";
			declaration.DocsAndCartage.JP_OrderItemsAsString = "TestOrderReference";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_DateAtOrigin = ZDateTime.Now.AddDays(-1);
			declaration.JE_DateAtFinalDestination = ZDateTime.Now;
			declaration.JE_OH_ShippingLine = TestObjectCreator.Creditor1.PK;
			declaration.JE_TotalNoOfPacks = 2;
			declaration.JE_TotalWeight = 30;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalVolume = 60M;
			declaration.JE_TotalVolumeUnit = "M3";
			declaration.JE_RL_NKOrigin = "VNHAN";
			declaration.JE_RL_NKFinalDestination = "TWTPE";

			var job = TestObjectCreator.CreateJob(declaration);
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);
			job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			job.JH_GS_NKRepOps = TestObjectCreator.GS1.GS_Code;
			job.JH_GS_NKRepSales = TestObjectCreator.Staff.GS_Code;

			var jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = TestObjectCreator.CC1.PK;
			jobCharge.JR_LocalSellAmt = 100m;
			jobCharge.JR_OSSellAmt = 100m;
			jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			jobCharge.JR_AT_SellGSTRate = taxRate.PK;

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);
			arInvoice.AH_JH = job.PK;
			arInvoice.AH_TransactionReference = "AP/19E1";
			arInvoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			arInvoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var line = arInvoice.Lines.Cast<InvoicingLineBase>().First();
			line.AL_JH = job.PK;
			line.AL_AT = taxRate.PK;
			jobCharge.JR_AL_ARLine = line.PK;

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);
			arInvoice.AH_TransactionReference = "AP/19E1";

			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice()
			{
				OriginalTransactionPK = arInvoice.PK,
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				TransactionReference = arInvoice.AH_TransactionReference,
				VATRegistrationNum = "0104128565-999",
				BankName = "bank test",
				AccountNumber = "account test number",
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK,
				IsDeclarationInvoice = true,
				ETA = declaration.JE_DateAtOrigin.ToShortDateString(),
				ETD = declaration.JE_DateAtFinalDestination.ToShortDateString(),
				SendingAgent = ZString.Empty,
				ReceivingAgent = ZString.Empty,
				Carrier = TestObjectCreator.Creditor1.OH_FullName,
				TransactionNumber = arInvoice.AH_TransactionNum,
				TransportMode = "AIR",
				TransportInfo = "",
				OrderReference = "TestOrderReference",
				Package = "2 PACKS",
				Weight = "30 KG",
				ChargeableWeight = ZString.Empty,
				Volume = "60 M3",
				OriginPort = string.Format($"{declaration.Origin.Code} - {declaration.Origin.RL_PortName}.{declaration.Origin.Country.RN_DescMultilingual}"),
				DestinationPort = string.Format($"{declaration.FinalDestination.Code} - {declaration.FinalDestination.RL_PortName}.{declaration.FinalDestination.Country.RN_DescMultilingual}"),
				FormattedAddress = "test formatted address",
				InvoiceCreatingStaff = arInvoice.CheckRequesterUserFullName,
				JobInfo = new AdditionalJobInfo()
				{
					JobDepartmentDesc = TestObjectCreator.NonCurrentDepartment.GE_Desc,
					JobOperationStaff = TestObjectCreator.GS1.GS_FullName,
					JobSalesStaff = TestObjectCreator.Staff.GS_FullName
				},
			});
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", 100m, 1m, "0100233488", "0104128565-999", "bank test", "account test number", declaration: declaration);
		}

		public void TestVietnamEInvoiceCreate_PeriodicInvoice_MultipleJob()
		{
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
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
			arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			arInvoice.AH_Desc = "TEST";
			arInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			arInvoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			arInvoice.AH_InvoiceDate = ZDateTime.Today;
			arInvoice.AH_PostDate = ZDateTime.Today;
			arInvoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			arInvoice.AH_TransactionReference = "AP/19E1";
			TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
			arInvoice.Lines[0].GenericCharge = TestObjectCreator.CC1.PK;
			arInvoice.AH_OA_InvoiceAddressOverride = address.PK;
			arInvoice.Header.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			arInvoice.AH_ChequeOrReference = "SellReference";
			arInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var line = arInvoice.Lines.Cast<InvoicingLineBase>().First();
			line.AL_JH = job.PK;
			line.AL_AT = taxRate.PK;
			line.AL_LineAmount = 100m;
			jobCharge.JR_AL_ARLine = line.PK;

			var line2 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
			line2.AL_JH = job2.PK;
			line2.AL_AT = taxRate.PK;
			line2.AL_LineAmount = 100m;
			jobCharge2.JR_AL_ARLine = line2.PK;

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "en");
			var transactionLineHouseBillDictionary = ((TransactionHeaderWithLines)arInvoice).Lines.Select(x => x.Job).ToDictionary(j => j.JH_JobNum, j => ((Business.JobInvoicing.Job)j).JH_HouseBillNo);
			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice()
			{
				OriginalTransactionPK = arInvoice.PK,
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				VATRegistrationNum = "0104128565-999",
				BankName = "bank test",
				AccountNumber = "account test number",
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					SeriesPrefix = arInvoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
					IsComplianceNumberFormatDefault = true,
				},
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK,
				ComplianceDocumentDate = arInvoice.AH_ComplianceDocumentDate,
				FormattedAddress = "test formatted address",
				InvoiceCreatingStaff = arInvoice.CheckRequesterUserFullName,
				TransactionLineHouseBillDictionary = transactionLineHouseBillDictionary,
				TransactionReference = arInvoice.AH_TransactionReference,
			});

			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", line.AL_OSAmount, 1m, "0100233488", "0104128565-999", "bank test", "account test number", "en");
		}

		public void TestVietnamEInvoiceCreate_TypeRef()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var complianceSequence1 = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence1.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence1.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence1.XD_Code = "AAA";
			complianceSequence1.XD_SequenceClass = "TXI";
			complianceSequence1.XD_Prefix = "PREFIX/";
			complianceSequence1.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
			complianceSequence1.XD_IsActive = true;
			complianceSequence1.XD_MaximumNumberDigits = 8;

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 0.2m, 20m, 2m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			arInvoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			arInvoice.AH_TransactionReference = "PREFIX/AP/19E1";
			arInvoice.AH_XD_ComplianceBook = complianceSequence1.PK;
			arInvoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			arInvoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			arInvoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			arInvoice.AH_ComplianceDocumentDate = ZDate.Today;
			TestObjectCreator.Debtor.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			arInvoice.AH_ChequeOrReference = "SellReference";

			var line = arInvoice.Lines.Cast<ARInvoiceLine>().First();
			line.AL_AT = taxRate.PK;
			line.AL_Desc = "Line1\r\nLine2\r\nLine3";
			line.AL_OSTaxAmount = 2m;
			line.AL_OverseasTotal = 22m;
			line.AL_LocalTaxAmount = 10m;
			line.AL_LocalExTaxAmount = 100m;
			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			Factory.Save();

			Func<AdditionalTransactionInfoForVietnamEInvoice> createAdditionalTransactionInfoForVietnamEInvoice = delegate
			{
				return new AdditionalTransactionInfoForVietnamEInvoice()
				{
					OriginalTransactionPK = arInvoice.PK,
					OriginalTransactionReference = arInvoice.AH_TransactionReference,
					VATRegistrationNum = "0104128565-999",
					BankName = "bank test",
					AccountNumber = "account test number",
					ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
					{
						SeriesPrefix = arInvoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
						IsComplianceNumberFormatDefault = true,
						ComplianceSequenceMaximumNumberDigits = arInvoice.ComplianceBook.XD_MaximumNumberDigits,
					},
					CompanyPK = arInvoice.Company.PK,
					BranchPK = arInvoice.Branch.PK,
					ComplianceDocumentDate = arInvoice.AH_ComplianceDocumentDate,
					FormattedAddress = "test formatted address",
					InvoiceCreatingStaff = arInvoice.CheckRequesterUserFullName,
				};
			};

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, createAdditionalTransactionInfoForVietnamEInvoice);

			AssertNotNull("EInvoice", eInvoice);
			AssertEquals("type_ref", 1, eInvoice.Inv.TypeRef);

			complianceSequence1.XD_MaximumNumberDigits = 7;

			(eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, createAdditionalTransactionInfoForVietnamEInvoice);

			AssertNotNull("EInvoice", eInvoice);
			AssertEquals("type_ref", 0, eInvoice.Inv.TypeRef);
		}

		public void TestVietnamEInvoiceCreate_SendType()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var complianceSequence1 = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence1.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence1.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence1.XD_Code = "AAA";
			complianceSequence1.XD_SequenceClass = "TXI";
			complianceSequence1.XD_Prefix = "PREFIX/";
			complianceSequence1.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
			complianceSequence1.XD_IsActive = true;
			complianceSequence1.XD_MaximumNumberDigits = 8;

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 0.2m, 20m, 2m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			arInvoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			arInvoice.AH_TransactionReference = "PREFIX/AP/19E1";
			arInvoice.AH_XD_ComplianceBook = complianceSequence1.PK;
			arInvoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			arInvoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			arInvoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			arInvoice.AH_ComplianceDocumentDate = ZDate.Today;
			TestObjectCreator.Debtor.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			arInvoice.AH_ChequeOrReference = "SellReference";

			var line = arInvoice.Lines.Cast<ARInvoiceLine>().First();
			line.AL_AT = taxRate.PK;
			line.AL_Desc = "Line1\r\nLine2\r\nLine3";
			line.AL_OSTaxAmount = 2m;
			line.AL_OverseasTotal = 22m;
			line.AL_LocalTaxAmount = 10m;
			line.AL_LocalExTaxAmount = 100m;
			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			Factory.Save();

			AccountingConfigurationRegistry.Instance.EInvoicingSendType.SetValue(arInvoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Func<AdditionalTransactionInfoForVietnamEInvoice> createAdditionalTransactionInfoForVietnamEInvoice = delegate
			{
				return new AdditionalTransactionInfoForVietnamEInvoice()
				{
					OriginalTransactionPK = arInvoice.PK,
					OriginalTransactionReference = arInvoice.AH_TransactionReference,
					VATRegistrationNum = "0104128565-999",
					BankName = "bank test",
					AccountNumber = "account test number",
					ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
					{
						SeriesPrefix = arInvoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
						IsComplianceNumberFormatDefault = true,
						ComplianceSequenceMaximumNumberDigits = arInvoice.ComplianceBook.XD_MaximumNumberDigits,
					},
					CompanyPK = arInvoice.Company.PK,
					BranchPK = arInvoice.Branch.PK,
					ComplianceDocumentDate = arInvoice.AH_ComplianceDocumentDate,
					FormattedAddress = "test formatted address",
					InvoiceCreatingStaff = arInvoice.CheckRequesterUserFullName,
				};
			};

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, createAdditionalTransactionInfoForVietnamEInvoice);

			AssertNotNull("EInvoice", eInvoice);
			AssertEquals("sendtype", 2, eInvoice.Inv.SendType);

			AccountingConfigurationRegistry.Instance.EInvoicingSendType.SetValue(arInvoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			(eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, createAdditionalTransactionInfoForVietnamEInvoice);

			AssertNotNull("EInvoice", eInvoice);
			AssertEquals("sendtype", 1, eInvoice.Inv.SendType);
		}

		public void TestVietnamEInvoiceCreateByShipment_HasColoadMasterShipment()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);

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

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);
			arInvoice.AH_TransactionReference = "AP/19E1";
			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice()
			{
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK,
				ConsolID = consol.JK_UniqueConsignRef,
				TransactionParentID = shipment.JS_UniqueConsignRef,
			});
			AssertNotNull("EInvoice", eInvoice);
			AssertEquals("Consignee is Creditor1", "Creditor1", eInvoice.Inv.Consignee);
			AssertEquals("Consignor is Creditor2", "Creditor2", eInvoice.Inv.Consignor);
		}

		public void TestFallbackVatNumber()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			arInvoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			arInvoice.Lines.Cast<AccTransactionLines>().First().AL_AT = taxRate.PK;
			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);
			arInvoice.AH_TransactionReference = "AP/19E1";
			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");
			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", TransactionReference = arInvoice.AH_TransactionReference, });
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", 100m, 1m, "0100233488");
		}

		public void TestRoundExchangeRateUp()
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 9.995m, 999.50m, 99.95m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);
			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", });
			AssertNotNull("EInvoice", eInvoice);
			AssertEquals("Exchange rate should be 10.00", 10.00m, eInvoice.Inv.Exrt);
		}

		public void TestRoundExchangeRateDown()
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 9.994m, 999.40m, 99.94m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);
			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", });
			AssertNotNull("EInvoice", eInvoice);
			AssertEquals("Exchange rate should be 9.99", 9.99m, eInvoice.Inv.Exrt);
		}

		public void TestMaxLengthOfOrderReference()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001007");
			shipment.DocsAndCartage.JP_OrderItemsAsString = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890Test";
			Assert("Order Reference length is longer than 100", shipment.DocsAndCartage.JP_OrderItemsAsString.Length > 100);

			var job = TestObjectCreator.CreateJob(shipment);
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);
			arInvoice.AH_JH = job.PK;

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);
			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", OrderReference = shipment.DocsAndCartage.JP_OrderItemsAsString });
			AssertNotNull("EInvoice", eInvoice);
			AssertNotNull("AdditionalInfoC10", eInvoice.Inv.AdditionalInfoC10);

			var additionalInfo = JsonConvert.DeserializeObject<AdditionalInfoC10>(eInvoice.Inv.AdditionalInfoC10);

			AssertEquals("Order Reference length is 100", 100, additionalInfo.OrderReference.Length);
			AssertEquals("OrderReference is 1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890", "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890", additionalInfo.OrderReference);
		}

		public void TestVietnamEInvoiceCreationWithCMTCharge()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var complianceSequence1 = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence1.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence1.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence1.XD_Code = "AAA";
			complianceSequence1.XD_SequenceClass = "TXI";
			complianceSequence1.XD_Prefix = "PREFIX/";
			complianceSequence1.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
			complianceSequence1.XD_IsActive = true;

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 0.2m, 20m, 2m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			arInvoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			arInvoice.AH_TransactionReference = "PREFIX/AP/19E1";
			arInvoice.AH_XD_ComplianceBook = complianceSequence1.PK;
			arInvoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			arInvoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			arInvoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			arInvoice.AH_ComplianceDocumentDate = ZDate.Today;
			TestObjectCreator.Debtor.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			arInvoice.AH_ChequeOrReference = "SellReference";

			var line = arInvoice.Lines.Cast<ARInvoiceLine>().First();
			line.AL_AT = taxRate.PK;
			line.AL_Desc = "NotCMTLine";
			line.AL_OSTaxAmount = 2m;
			line.AL_OverseasTotal = 22m;
			line.AL_LocalTaxAmount = 10m;
			line.AL_LocalExTaxAmount = 100m;

			var commentLine = (ARInvoiceLine)arInvoice.Lines.AddNew();
			commentLine.AL_Desc = "CMTLine";
			commentLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			commentLine.AL_OSExTaxAmount = 100m;
			commentLine.AL_AC = TestObjectCreator.CommentChargeCode.PK;
			commentLine.AL_AT = ZGuid.Empty;
			commentLine.ChargeCode.AC_Code = "CMT";

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			Factory.Save();

			AssertEquals("Invoice have two lines", 2, arInvoice.Lines.Count);

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "en");

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice()
			{
				OriginalTransactionPK = arInvoice.PK,
				OriginalTransactionReference = arInvoice.AH_TransactionReference,
				VATRegistrationNum = "0104128565-999",
				BankName = "bank test",
				AccountNumber = "account test number",
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					SeriesPrefix = arInvoice.ComplianceBook?.XD_Prefix ?? ZString.Empty,
					IsComplianceNumberFormatDefault = true
				},
				CompanyPK = arInvoice.Company.PK,
				BranchPK = arInvoice.Branch.PK,
				ComplianceDocumentDate = arInvoice.AH_ComplianceDocumentDate,
				FormattedAddress = "test formatted address",
				InvoiceCreatingStaff = arInvoice.CheckRequesterUserFullName,
			});

			AssertNotNull("EInvoice", eInvoice);
			AssertEquals("Should only have one Item because CMT charge won't be export.", 1, eInvoice.Inv.Items.Count);
			AssertEquals("The export line is not CMT charge", "NotCMTLine", eInvoice.Inv.Items.First().Name);
		}

		#region Test Rating Basis

		[TestDate(2020, 8, 1)]
		public void TestUnit()
		{
			var (jobCharge, arInvoice) = CreateJobChargeAndARInvoice();

			CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 3m, 0m, 0m, 200m, 0m, "Unit", "custom", "Unit", "custom", "UNT");

			Factory.Save();

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", TransactionReference = arInvoice.AH_TransactionReference, });
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", 200m, 3m, "0100233488");
		}

		[TestDate(2020, 8, 1)]
		public void TestRatingBasis()
		{
			var (jobCharge, arInvoice) = CreateJobChargeAndARInvoice();
			arInvoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK.ToGuid();

			CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 200m, 0m, 0m, 2m, 0m, "KG", "Weight", "KG", "Weight", "UNT");

			Factory.Save();

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", TransactionReference = arInvoice.AH_TransactionReference, });
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "branch kg", 2m, 200m, "0100233488");
		}

		[TestDate(2020, 8, 1)]
		public void TestMinimumRatingBasis()
		{
			var (jobCharge, arInvoice) = CreateJobChargeAndARInvoice();

			CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 1m, 2m, 0m, 0m, 0m, "", "", "1", "", "MIN");

			Factory.Save();

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", TransactionReference = arInvoice.AH_TransactionReference, });
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", 100m, 1m, "0100233488");
		}

		[TestDate(2020, 8, 1)]
		public void TestCustomRatingBasis()
		{
			var (jobCharge, arInvoice) = CreateJobChargeAndARInvoice();

			CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 200m, 0m, 0m, 3m, 0m, "Custom", "custom", "Custom", "custom", "UNT");

			Factory.Save();

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", TransactionReference = arInvoice.AH_TransactionReference, });
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", 3m, 200m, "0100233488");
		}

		[TestDate(2020, 8, 1)]
		public void TestChargePercentageRatingBasis()
		{
			var (jobCharge, arInvoice) = CreateJobChargeAndARInvoice();

			CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 200m, 0m, 0m, 2m, 0m, "AUD", "custom", "100", "custom", "UNT");

			Factory.Save();

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", TransactionReference = arInvoice.AH_TransactionReference, });
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", 100m, 1m, "0100233488");
		}

		[TestDate(2020, 8, 1)]
		public void TestContainerCountRatingBasis()
		{
			var (jobCharge, arInvoice) = CreateJobChargeAndARInvoice();

			CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 1m, 0m, 0m, 200m, 0m, "AUD", "custom", "CN", "custom", "UNT");
			CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 1m, 0m, 0m, 200m, 0m, "AUD", "custom", "CN", "custom", "UNT");

			Factory.Save();

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", TransactionReference = arInvoice.AH_TransactionReference, });
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "cn", 200m, 2m, "0100233488");
		}

		[TestDate(2020, 8, 1)]
		public void TestOneJobChargeHasMultipleRatingBasisWithDifferentUnits()
		{
			var (jobCharge, arInvoice) = CreateJobChargeAndARInvoice();

			CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 1m, 0m, 0m, 50m, 0m, "20GP", "custom", "CN", "custom", "UNT");
			CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 1m, 0m, 0m, 50m, 0m, "40GP", "custom", "CN", "custom", "UNT");

			Factory.Save();

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			Factory.Save();

			var (eInvoice, _) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", TransactionReference = arInvoice.AH_TransactionReference, });
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", 100m, 1m, "0100233488");
		}

		[TestDate(2020, 8, 1)]
		public void TestOneJobChargeHasMultipleRatingBasisWithDifferentUnits_ContainsNullUnit()
		{
			var (jobCharge, arInvoice) = CreateJobChargeAndARInvoice();

			CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 0m, 0m, 0m, 0m, 0m, "PKG", "custom", "PKG", "custom", "UNT");
			CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 1m, 750m, 0m, 0m, 0m, null, null, "1", null, "FLT");

			Factory.Save();

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			Factory.Save();

			var (eInvoice, _) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", TransactionReference = arInvoice.AH_TransactionReference, });
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", 100m, 1m, "0100233488");
		}

		[TestDate(2020, 8, 1)]
		public void TestFlatPlusPerUnitRatingBasis()
		{
			var (jobCharge, arInvoice) = CreateJobChargeAndARInvoice();

			CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 125m, 0m, 0m, 2m, 0m, "AUD", "custom", "UNT", "custom", "UNT");
			CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 1m, 0m, 0m, 0m, 3.5m, "", "", "1", "", "FLT");

			Factory.Save();

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", TransactionReference = arInvoice.AH_TransactionReference, });
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", 100m, 1m, "0100233488");
		}

		[TestDate(2020, 8, 1)]
		public void TestFlatRatingBasis()
		{
			var (jobCharge, arInvoice) = CreateJobChargeAndARInvoice();

			CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 1m, 0m, 0m, 0m, 3m, "", "", "1", "", "FLT");

			Factory.Save();

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", TransactionReference = arInvoice.AH_TransactionReference, });
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unt", 100m, 1m, "0100233488");
		}

		[TestDate(2020, 8, 1)]
		public void TestUnitQuantityPrice_DifferentCurrency()
		{
			var (jobCharge, arInvoice) = CreateJobChargeAndARInvoice();

			var jobPaymentBasis = CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 3m, 0m, 0m, 200m, 0m, "Shipment", "custom", "Shipment", "custom", "UNT");

			Factory.Save();

			var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

			Factory.Save();

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", TransactionReference = arInvoice.AH_TransactionReference, });
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "Shipment", 200m, 3m, "0100233488");

			jobPaymentBasis.PBS_RX_NKRateCurrency = TestObjectCreator.USD.Code;

			Factory.Save();

			(eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", TransactionReference = arInvoice.AH_TransactionReference, });
			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, arInvoice, "unit", 100m, 1, "0100233488");
		}

		[TestDate(2020, 8, 1)]
		public void TestUnitQuantityPrice_ForeignChargeCurrencyWithAlwaysIssueElectronicInvoicesInLocalCurrencyRegistry()
		{
			using (AccountingConfigurationRegistry.Instance.VietnamAlwaysIssueElectronicInvoicesInLocalCurrency.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				GlbCompany.CurrentCompany.SetCountry(CountryCodes.VietNam);
				GlbCompany.CurrentCompany.SetCurrency(TestObjectCreator.VND.Code);
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 100;

				var (jobCharge, arInvoice) = CreateJobChargeAndARInvoice(TestObjectCreator.VND);
				jobCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.Code;
				var jobPaymentBasis = CreateShipmentJobPaymentBasis(jobCharge.PK.ToGuid(), 3m, 0m, 0m, 200m, 0m, "Shipment", "custom", "Shipment", "custom", "UNT");
				jobPaymentBasis.PBS_RX_NKRateCurrency = "VND";

				Factory.Save();

				var eInvoiceBatch = CreateEInvoicingBatch(arInvoice);

				Factory.Save();

				var (eInvoice, _) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice() { OriginalTransactionPK = arInvoice.PK, OriginalTransactionReference = arInvoice.AH_TransactionReference, CompanyPK = arInvoice.Company.PK, BranchPK = arInvoice.Branch.PK, FormattedAddress = "test formatted address", TransactionReference = arInvoice.AH_TransactionReference, });
				AssertNotNull("EInvoice", eInvoice);
				AssertEInvoice(eInvoice, arInvoice, "unit", 100m, 1, "0100233488");
			}
		}

		(JobCharge, InvoicingBase) CreateJobChargeAndARInvoice(RefCurrency curr = null)
		{
			curr = curr ?? TestObjectCreator.AUD;
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);

			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var shipmentID = "S00001007";
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment(shipmentID));
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.AUD, 1);

			var jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = TestObjectCreator.CC1.PK;
			jobCharge.JR_LocalSellAmt = 100m;
			jobCharge.JR_OSSellAmt = 100m;
			jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			jobCharge.JR_AT_SellGSTRate = taxRate.PK;

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", curr, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);
			arInvoice.AH_JH = job.PK;
			arInvoice.AH_TransactionReference = "AP/19E1";
			arInvoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			arInvoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			var line = arInvoice.Lines.Cast<InvoicingLineBase>().First();
			line.AL_JH = job.PK;
			line.AL_AT = taxRate.PK;
			jobCharge.JR_AL_ARLine = line.PK;

			TestObjectCreator.SetCustomsCodeForOrgHeader(arInvoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			return (jobCharge, arInvoice);
		}

		JobPaymentBasis CreateShipmentJobPaymentBasis(Guid jobChargePK, decimal chargeableAmount, decimal minRate, decimal maxRate, decimal perUnitRate, decimal flatRate, string chargeableUnit, string chargeableUnitType, string rateUnit, string rateUnitType, string rateReference)
		{
			var jobPaymentBasis = Factory.NewWithValidTestData<JobPaymentBasis>();
			jobPaymentBasis.PBS_JR = jobChargePK;
			jobPaymentBasis.PBS_IsCost = false;
			jobPaymentBasis.PBS_AdapterType = "Shipment";
			jobPaymentBasis.PBS_AdapterID = shipmentId;
			jobPaymentBasis.PBS_ChargeableAmount = chargeableAmount;
			jobPaymentBasis.PBS_FlatRate = flatRate;
			jobPaymentBasis.PBS_MinRate = minRate;
			jobPaymentBasis.PBS_MaxRate = maxRate;
			jobPaymentBasis.PBS_PerUnitRate = perUnitRate;
			jobPaymentBasis.PBS_ChargeableUnit = chargeableUnit;
			jobPaymentBasis.PBS_ChargeableUnitType = chargeableUnitType;
			jobPaymentBasis.PBS_RX_NKRateCurrency = "AUD";
			jobPaymentBasis.PBS_RateUnit = rateUnit;
			jobPaymentBasis.PBS_RateUnitType = rateUnitType;
			jobPaymentBasis.PBS_RateReference = rateReference;

			return jobPaymentBasis;
		}

		#endregion

		void AssertEInvoice(VietnamEInvoice eInvoice, InvoicingBase arTransaction, string unit, decimal price, decimal quantity, string vietnamProxyVatNumber, string debtorVatNumber = "", string bankName = "", string accountNumber = "", string language = "vi", ForwardingConsol consol = null, ForwardingShipment shipment = null, BaseJobDeclaration declaration = null, AdditionalContactInfo contactInfo = null)
		{
			CombineAssertions(() =>
			{
				// User
				AssertNullOrEmpty(eInvoice.User.Username);
				AssertNullOrEmpty(eInvoice.User.Password);
				// Lang
				AssertEquals("language", language, eInvoice.Language);
				// Inv
				AssertEquals("sid", arTransaction.PK.ToString(), eInvoice.Inv.Sid);
				AssertEquals("idt", arTransaction.AH_ComplianceDocumentDate.ToString("yyyy-MM-dd HH:mm"), eInvoice.Inv.Idt);
				AssertEquals("type", "01GTKT", eInvoice.Inv.Type);
				AssertEquals("form", arTransaction.Branch.PK == TestObjectCreator.NonCurrentBranch.PK ? "FormNumber" : string.Empty, eInvoice.Inv.Form);
				AssertEquals("serial", arTransaction.ComplianceBook?.XD_Prefix ?? ZString.Empty, eInvoice.Inv.Serial);
				AssertEquals("seq", "AP/19E1", eInvoice.Inv.Seq);
				AssertEquals("aun", 1, eInvoice.Inv.Aun);
				AssertEquals("bcode", arTransaction.Header.OH_Code, eInvoice.Inv.Bcode);
				AssertEquals("bname", arTransaction.Header.OH_FullName, eInvoice.Inv.Bname);
				AssertEquals("buyer", contactInfo?.Name ?? ZString.Empty, eInvoice.Inv.Buyer);
				AssertEquals("btax", debtorVatNumber, eInvoice.Inv.Btax);
				AssertEquals("baddr", "test formatted address", eInvoice.Inv.Baddr);
				AssertEquals("btel", contactInfo?.Phone ?? ZString.Empty, eInvoice.Inv.Btel);
				AssertEquals("bmail", VietnamEInvoiceHelper.GetContactEmails(contactInfo), eInvoice.Inv.Bmail);
				AssertEquals("sendfile", 1, eInvoice.Inv.Sendfile);
				AssertEquals("baym", "TM/CK", eInvoice.Inv.Paym);
				AssertEquals("curr", arTransaction.AH_RX_NKTransactionCurrency, eInvoice.Inv.Curr);
				AssertEquals("exrt", arTransaction.AH_ExchangeRate, eInvoice.Inv.Exrt);
				AssertEquals("bacc", accountNumber, eInvoice.Inv.Bacc);
				AssertEquals("bbank", bankName, eInvoice.Inv.Bbank);
				AssertEquals("note", arTransaction.AH_Desc, eInvoice.Inv.Note);
				AssertEquals("sumv", arTransaction.AH_LocalExTaxAmount, eInvoice.Inv.Sumv);
				AssertEquals("sum", arTransaction.AH_OSExTaxAmount, eInvoice.Inv.Sum);
				AssertEquals("vatv", arTransaction.AH_LocalTaxAmount, eInvoice.Inv.Vatv);
				AssertEquals("vat", arTransaction.AH_OSTaxAmount, eInvoice.Inv.Vat);

				var useOSCurrency = AccountingConfigurationRegistry.Instance.VietnamExportAmountInWordsBasedOnInvoicedCurrency.GetFallBackValueAtAllLevels(arTransaction.Company.PK.ToGuid(), arTransaction.Branch.PK.ToGuid(), Guid.Empty);
				var total = useOSCurrency ? (double)arTransaction.AH_OSTotal : (double)arTransaction.AH_LocalTotalAmount;
				var currencyCode = useOSCurrency ? arTransaction.AH_RX_NKTransactionCurrency.ToString() : Core.Constants.CurrencyCodes.VietNam;

				var currencyToWord = new CurrencyToWords_VI_VN().ConvertToWords(total, currencyCode);
				currencyToWord = currencyToWord.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture) + currencyToWord.Substring(1, currencyToWord.Length - 1);

				AssertEquals("word", currencyToWord, eInvoice.Inv.Word);
				if (!useOSCurrency)
				{
					AssertContains("word2", "dong", eInvoice.Inv.Word);
				}
				AssertEquals("totalv", arTransaction.AH_LocalTotalAmount, eInvoice.Inv.Totalv);
				AssertEquals("discount", "", eInvoice.Inv.Discount);
				AssertEquals("total", arTransaction.AH_OSTotalAmount, eInvoice.Inv.Total);

				var expectTypeRef = (arTransaction.ComplianceBook?.XD_MaximumNumberDigits ?? 0) == 8 ? 1 : 0;
				AssertEquals("type_ref", expectTypeRef, eInvoice.Inv.TypeRef);

				var expectSendType = AccountingConfigurationRegistry.Instance.EInvoicingSendType.GetFallBackValueAtAllLevels(arTransaction.Company.PK.ToGuid(), Guid.Empty, Guid.Empty) ? 2 : 1;
				AssertEquals("sendtype", expectSendType, eInvoice.Inv.SendType);
				AssertEquals("c0", arTransaction.AH_DueDate.ToShortDateString() ?? string.Empty, eInvoice.Inv.DueDate);

				if (shipment != null && arTransaction.Job.JH_ParentID == shipment.PK)
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
						SellReference = arTransaction.AH_ChequeOrReference,
						TransactionNumber = arTransaction.AH_ConsolidatedInvoiceRef,
						TransportMode = shipment.TransportMode,
					};

					var additionalInfoC10 = new AdditionalInfoC10()
					{
						OwnerReference = shipment.Declarations?.FirstOrDefault()?.JE_OwnerRef,
						OrderReference = shipment.DocsAndCartage?.JP_OrderItemsAsString ?? ZString.Empty,
						InvoiceDepartmentDesc = arTransaction.Department.GE_Desc,
						ExternalSystemDebtorCode = arTransaction.Header.CompanyData.OB_ARExternalDebtorCode,
						GoodDescription = shipment.JS_GoodsDescription,
						Package = shipment.JS_OuterPacks.ToString() + " " + shipment.JS_F3_NKPackType,
						ConsolID = shipment.Consols.Cast<ForwardingConsol>().OrderBy(x => x.JK_UniqueConsignRef).First().JK_UniqueConsignRef,
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
						InvoiceCreatingStaff = arTransaction.CheckRequesterUserFullName,
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
				else if (consol != null && arTransaction.Job.JH_ParentID == consol.PK)
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
						SellReference = arTransaction.AH_ChequeOrReference,
						TransactionNumber = arTransaction.AH_ConsolidatedInvoiceRef,
						TransportMode = string.Empty,
					};

					var additionalInfoC10 = new AdditionalInfoC10()
					{
						OwnerReference = string.Empty,
						OrderReference = string.Empty,
						InvoiceDepartmentDesc = arTransaction.Department.GE_Desc,
						ExternalSystemDebtorCode = arTransaction.Header.CompanyData.OB_ARExternalDebtorCode,
						GoodDescription = string.Empty,
						Package = consol.JK_TotalShipmentQuantity.ToString() + " PACKS",
						ConsolID = string.Empty,
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
						InvoiceCreatingStaff = arTransaction.CheckRequesterUserFullName,
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
				else if (declaration != null && arTransaction.Job.JH_ParentID == declaration.PK)
				{
					AssertEquals("c1", declaration.Supplier.OH_FullName, eInvoice.Inv.Consignor);
					AssertEquals("c2", declaration.JE_DeclarationReference, eInvoice.Inv.Reference);
					AssertEquals("c3", declaration.Importer.OH_FullName, eInvoice.Inv.Consignee);
					AssertEquals("c4", declaration.JE_DateAtFinalDestination.ToShortDateString(), eInvoice.Inv.ETD);
					AssertEquals("c5", "", eInvoice.Inv.VoyageFlightJourneyDetails);
					AssertEquals("c6", declaration.JE_DateAtOrigin.ToShortDateString(), eInvoice.Inv.ETA);
					AssertEquals("c7", declaration.JE_MasterBill, eInvoice.Inv.MasterBill);
					AssertEquals("c8", declaration.JE_HouseBill, eInvoice.Inv.HouseBill);

					var additionalInfoC9 = new AdditionalInfoC9()
					{
						CreditTerms = "Cash on Delivery",
						Origin = declaration.Origin != null ? string.Format($"{declaration.Origin.Code} - {declaration.Origin.Description}.{declaration.Origin.Country.RN_DescMultilingual}") : string.Empty,
						Destination = declaration.FinalDestination != null ? string.Format($"{declaration.FinalDestination.Code} - {declaration.FinalDestination.Description}.{declaration.FinalDestination.Country.RN_DescMultilingual}") : string.Empty,
						Incoterms = declaration.IncoTerm + " - " + declaration.GetINCOTermDescription(Factory),
						SellReference = arTransaction.AH_ChequeOrReference,
						TransactionNumber = arTransaction.AH_ConsolidatedInvoiceRef,
						TransportMode = declaration.TransportMode,
					};

					var additionalInfoC10 = new AdditionalInfoC10()
					{
						OwnerReference = declaration.JE_OwnerRef,
						OrderReference = declaration.DocsAndCartage.JP_OrderItemsAsString,
						InvoiceDepartmentDesc = arTransaction.Department.GE_Desc,
						ExternalSystemDebtorCode = arTransaction.Header.CompanyData.OB_ARExternalDebtorCode,
						GoodDescription = declaration.JE_GoodsDescription,
						Package = declaration.JE_TotalNoOfPacks.ToString() + " PACKS",
						ConsolID = string.Empty,
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
						InvoiceCreatingStaff = arTransaction.CheckRequesterUserFullName,
					};

					var additionalInfoC13 = new AdditionalInfoC13();

					if (declaration.Job != null)
					{
						additionalInfoC10.JobDepartmentDesc = declaration.Job.Department.GE_Desc;
						additionalInfoC12.JobOperationStaff = declaration.Job.RepOps.GS_FullName;
						additionalInfoC13.JobSalesStaff = declaration.Job.RepSales.GS_FullName;
					}

					AssertEquals("c9", JsonConvert.SerializeObject(additionalInfoC9), eInvoice.Inv.AdditionalInfoC9);
				}

				// Item
				var line = arTransaction.Lines.Cast<TransactionLine>().First();
				var osTaxAmount = TaxAmountCalculator.SplitOSTotalToTaxAndExTaxAmounts(line.AL_GSTVAT, () => ExchangeRate.LocalToForeign(line.AL_LineAmount, line.AL_ExchangeRate, line.TransactionCurrency.RX_SubUnitRatio, line.Company.GC_IsReciprocal), line.AL_OSAmount).oSTaxAmount;
				AssertEquals("type", "", eInvoice.Inv.Items[0].Type);

				if (line.TaxRate.AT_Code == "NOTREPORT")
				{
					AssertEquals("vrt", "-1", eInvoice.Inv.Items[0].VRT);
				}
				else
				{
					AssertEquals("vrt", line.AL_TaxRateCalc.ToString(), eInvoice.Inv.Items[0].VRT);
				}

				AssertEquals("line", line.AL_Sequence, eInvoice.Inv.Items[0].Line);
				AssertEquals("name", line.AL_Desc.Replace("\r\n", " "), eInvoice.Inv.Items[0].Name);
				AssertEquals("unit", unit, eInvoice.Inv.Items[0].Unit);
				AssertEquals("amount", line.AL_OSAmount - osTaxAmount, eInvoice.Inv.Items[0].Amount);
				AssertEquals("amountv", line.AL_LocalExTaxAmount, eInvoice.Inv.Items[0].LocalAmount);
				AssertEquals("price", price, eInvoice.Inv.Items[0].Price);
				AssertEquals("quantity", quantity, eInvoice.Inv.Items[0].Quantity);
				AssertEquals("vat", osTaxAmount, eInvoice.Inv.Items[0].VAT);
				AssertEquals("vatv", line.AL_LocalTaxAmount, eInvoice.Inv.Items[0].LocalTax);
				AssertEquals("total", line.AL_OverseasTotal, eInvoice.Inv.Items[0].Total);
				AssertEquals("totalv", line.AL_LocalTotalAmount, eInvoice.Inv.Items[0].LocalTotal);
				AssertEquals("c0", line.AL_LocalTaxAmount.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals), eInvoice.Inv.Items[0].CustomizedLocalTax);
				AssertEquals("c1", line.AL_LocalTotalAmount.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals), eInvoice.Inv.Items[0].CustomizedLocalTotal);
				AssertEquals("c2", line.AL_LocalExTaxAmount.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals), eInvoice.Inv.Items[0].CustomizedLocalAmount);
				var isMultipleJobTransaction = arTransaction.Lines.Select(x => x.Job).Distinct().Count() > 1;
				if (isMultipleJobTransaction)
				{
					AssertEquals("c3", ((Business.JobInvoicing.Job)line.Job).JH_HouseBillNo, eInvoice.Inv.Items[0].HouseBill);
				}
				else
				{
					AssertEquals("c3", ZString.Empty, eInvoice.Inv.Items[0].HouseBill);
				}
				//stax
				AssertEquals("stax", vietnamProxyVatNumber, eInvoice.Inv.Stax);
			});
		}

		(VietnamEInvoice EInvoice, INotifications ValidationErrors) CreateVietnamEInvoice(AccEInvoicingBatch eInvoiceBatch, Action<UniversalTransactionInfo> modifyTransactionInfo = null, Func<AdditionalTransactionInfoForVietnamEInvoice> createAdditionalTransactionInfoForVietnamEInvoice = null)
		{
			var universalTransactionBatch = CreateUniversalTransactionBatch(eInvoiceBatch);
			var universalTransaction = universalTransactionBatch.TransactionCollection[0];
			modifyTransactionInfo?.Invoke(universalTransaction);
			var notifications = new Logger();
			var eInvoiceCreator = new VietnamEInvoiceCreator(universalTransaction, createAdditionalTransactionInfoForVietnamEInvoice?.Invoke());
			var eInvoice = eInvoiceCreator.Create();
			return (eInvoice, notifications);
		}

		AccEInvoicingBatch CreateEInvoicingBatch(InvoicingBase invoice)
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Batched);
			return batch;
		}

		UniversalTransactionBatch CreateUniversalTransactionBatch(AccEInvoicingBatch batch)
		{
			var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
			var exportor = new TransactionBatchExporter(dataAccess, new PopulateOptionalXUTFieldsSetting(populateShipments: true));
			var transactionBatch = exportor.CreateTransactionBatch(batch);
			return transactionBatch;
		}

		void SetUnitMeasurementTextOverrideRegistryItem()
		{
			var companyCollecetion = new UnitMeasurementTextOverrideCollection();
			var branchCollection = new UnitMeasurementTextOverrideCollection();

			var unit = companyCollecetion.AddNew();
			unit.UnitMeasurement = "Unit";
			unit.TextOverride = "unit";

			var unt = companyCollecetion.AddNew();
			unt.UnitMeasurement = "UNT";
			unt.TextOverride = "unt";

			var cn = companyCollecetion.AddNew();
			cn.UnitMeasurement = "CN";
			cn.TextOverride = "cn";

			var kg = companyCollecetion.AddNew();
			kg.UnitMeasurement = "KG";
			kg.TextOverride = "kg";
			
			var branchKg = branchCollection.AddNew();
			branchKg.UnitMeasurement = "KG";
			branchKg.TextOverride = "branch kg";

			companyCollecetion.Add(unit);
			companyCollecetion.Add(unt);
			companyCollecetion.Add(cn);
			companyCollecetion.Add(kg);
			branchCollection.Add(unit);
			branchCollection.Add(unt);
			branchCollection.Add(cn);
			branchCollection.Add(branchKg);
			AccountingConfigurationRegistry.Instance.UnitMeasurementTextOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyCollecetion);
			AccountingConfigurationRegistry.Instance.UnitMeasurementTextOverride.SetValue(Guid.Empty, TestObjectCreator.NonCurrentBranch.PK.ToGuid(), Guid.Empty, branchCollection);
		}

		const string shipmentId = "S00001007";

		protected override void SetUp()
		{
			base.SetUp();
			Helper.SetupControlAccounts();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "admin");
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "123456");
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingFormNumber.SetTemporaryValue(Guid.Empty, TestObjectCreator.NonCurrentBranch.PK.ToGuid(), Guid.Empty, "FormNumber");

			SetUnitMeasurementTextOverrideRegistryItem();
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected EInvoicingTestHelper Helper => helper ?? (helper = new EInvoicingTestHelper(TestObjectCreator));
		EInvoicingTestHelper helper;
	}
}
