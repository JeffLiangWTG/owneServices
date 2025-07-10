using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class EntryCreationStrategyTest : TestCaseWithFactory
	{
		public void TestTriggeringPointForValidationDefaultedFromRegistryOnlyIfNotInDB()
		{
			var triggerPointsConfiguration = new TriggerPointsConfiguration()
			{
				ImportTriggerPoint = TriggerPointsCodeList.Codes.PAB,
				ExportTriggerPoint = TriggerPointsCodeList.Codes.REC
			};

			FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, triggerPointsConfiguration);

			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.CH_TriggeringPointForValidation = TriggerPointsCodeList.Codes.NUL;
			Factory.Save();
			Assert("Prerequisite", entryHeader.IsInDatabase);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryHeader.CH_JE = declaration.PK;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();

			entryHeader = declaration.CustomsEntryHeaders.First();
			AssertEquals("TriggeringPointForValidation should still be NULL because it should not overriden by registry settings when it is in database.", TriggerPointsCodeList.Codes.PAB, entryHeader.CH_TriggeringPointForValidation);
		}

		public void TestTriggeringPointForValidationDefaultedFromRegistryOnlyIfEmpty()
		{
			var triggerPointsConfiguration = new TriggerPointsConfiguration()
			{
				ImportTriggerPoint = TriggerPointsCodeList.Codes.PAB,
				ExportTriggerPoint = TriggerPointsCodeList.Codes.REC
			};

			FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, triggerPointsConfiguration);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders.First();
			AssertEquals("TriggeringPointForValidation should have been defaulted as per import registry settings.", TriggerPointsCodeList.Codes.PAB, entryHeader.CH_TriggeringPointForValidation);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			merger.DoMerge();
			Assert("Prerequisite", !entryHeader.IsInDatabase);
			AssertEquals("TriggeringPointForValidation should still be the import one, because it should not be overwritten when it is not empty.", TriggerPointsCodeList.Codes.PAB, entryHeader.CH_TriggeringPointForValidation);
		}

		public void TestTriggeringPointForValidationDefaultingWhenRegistryNotSet()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders.First();
			AssertEquals("TriggeringPointForValidation should have been defaulted to NUL when entry CH_TriggeringPointForValidation is empty and registry not set.", TriggerPointsCodeList.Codes.NUL, entryHeader.CH_TriggeringPointForValidation);

			Factory.Save();
			declaration.Reload();
			entryHeader = declaration.CustomsEntryHeaders.First();
			AssertEquals("TriggeringPointForValidation defaulted from registry should be persistent.", TriggerPointsCodeList.Codes.NUL, entryHeader.CH_TriggeringPointForValidation);
		}

		public void TestTriggeringPointForValidationDefaultingFromRegistry()
		{
			var triggerPointsConfiguration = new TriggerPointsConfiguration()
			{
				ImportTriggerPoint = TriggerPointsCodeList.Codes.PAB,
				ExportTriggerPoint = TriggerPointsCodeList.Codes.REC
			};

			FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, triggerPointsConfiguration);

			var importDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			importDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			importDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			var merger = new LineMerger(importDeclaration);
			merger.DoMerge();
			var entryHeader = importDeclaration.CustomsEntryHeaders.First();
			AssertEquals("TriggeringPointForValidation should have been defaulted as per import registry settings.", TriggerPointsCodeList.Codes.PAB, entryHeader.CH_TriggeringPointForValidation);

			Factory.Save();
			importDeclaration.Reload();
			entryHeader = importDeclaration.CustomsEntryHeaders.First();
			AssertEquals("TriggeringPointForValidation defaulted from registry should be persistent.", TriggerPointsCodeList.Codes.PAB, entryHeader.CH_TriggeringPointForValidation);

			var exportDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			exportDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			exportDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			merger = new LineMerger(exportDeclaration);
			merger.DoMerge();
			entryHeader = exportDeclaration.CustomsEntryHeaders.First();
			AssertEquals("TriggeringPointForValidation should have been defaulted as per export registry settings.", TriggerPointsCodeList.Codes.REC, entryHeader.CH_TriggeringPointForValidation);

			Factory.Save();
			exportDeclaration.Reload();
			entryHeader = exportDeclaration.CustomsEntryHeaders.First();
			AssertEquals("TriggeringPointForValidation defaulted from registry should be persistent.", TriggerPointsCodeList.Codes.REC, entryHeader.CH_TriggeringPointForValidation);
		}

		public void TestAdditionalInfos()
		{
			var declaration = Factory.New<JobDeclaration>();

			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "A";

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = cei.PK;

			var informationAdditionalInfo1 = invoiceLine1.AdditionalInfos.AddNew();
			informationAdditionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo1.CSI_Code = "INF1";
			informationAdditionalInfo1.CSI_ReferenceNumber = "REF1";

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = cei.PK;

			var informationAdditionalInfo2 = invoiceLine2.AdditionalInfos.AddNew();
			informationAdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			informationAdditionalInfo2.CSI_Code = "INF1";
			informationAdditionalInfo2.CSI_ReferenceNumber = "REF1";

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Invoice lines won't merge when their additional information don't share same CSI_SubType.", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			informationAdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo2.CSI_Code = "INF2";
			informationAdditionalInfo2.CSI_ReferenceNumber = "REF1";
			merger.DoMerge();

			AssertEquals("Invoice lines won't merge when their additional information don't share same CSI_Code.", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			informationAdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo2.CSI_Code = "INF1";
			informationAdditionalInfo2.CSI_ReferenceNumber = "REF2";
			merger.DoMerge();

			AssertEquals("Invoice lines won't merge when their additional information don't share same CSI_ReferenceNumber.", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			informationAdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo2.CSI_Code = "INF1";
			informationAdditionalInfo2.CSI_ReferenceNumber = "REF1";
			merger.DoMerge();

			AssertEquals("Invoice lines will merge only when their additional information share same CSI_SubType, CSI_Code and CSI_ReferenceNumber.", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestGetKeyForLineNotContainsHeaderSupplierIfG1()
		{
			var declaration = Factory.New<JobDeclaration>();

			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "A";

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceSupplier1 = Factory.NewWithValidTestData<OrgHeader>();
			invoiceHeader1.JZ_OH_Supplier = invoiceSupplier1.PK;

			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = cei.PK;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceSupplier2 = Factory.NewWithValidTestData<OrgHeader>();
			invoiceHeader2.JZ_OH_Supplier = invoiceSupplier2.PK;

			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = cei.PK;

			var merger = new LineMerger(declaration);

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = "IMP";
			merger.DoMerge();
			AssertEquals("Using G1 in Import mode, invoice lines from different invoices could merge even when their invoice suppliers differ", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			declaration.JE_MessageType = "EXP";
			merger.DoMerge();
			AssertEquals("Using G1 in Export mode, invoice lines from different invoices could merge even when their invoice suppliers differ", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			merger.DoMerge();
			AssertEquals("Using G2 in Export mode, invoice lines from different invoices don't merge when their invoice suppliers differ", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			declaration.JE_MessageType = "IMP";
			merger.DoMerge();
			AssertEquals("Using G2 in Import mode, invoice lines from different invoices don't merge when their invoice suppliers differ", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestGetKeyForLineNotContainsHeaderBuyerIfG1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "A";

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceBuyer1 = Factory.NewWithValidTestData<OrgHeader>();
			invoiceHeader1.JZ_OH_Buyer = invoiceBuyer1.PK;

			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = cei.PK;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceBuyer2 = Factory.NewWithValidTestData<OrgHeader>();
			invoiceHeader2.JZ_OH_Buyer = invoiceBuyer2.PK;

			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = cei.PK;

			var merger = new LineMerger(declaration);

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = "IMP";
			merger.DoMerge();
			AssertEquals("Using G1 in Import mode, invoice lines from different invoices don't merge even when their invoice buyers differ", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			declaration.JE_MessageType = "EXP";
			merger.DoMerge();
			AssertEquals("Using G1 in Export mode, invoice lines from different invoices could merge even when their invoice buyers differ", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			merger.DoMerge();
			AssertEquals("Using G2 in Export mode, invoice lines from different invoices don't merge when their invoice buyers differ", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			declaration.JE_MessageType = "IMP";
			merger.DoMerge();
			AssertEquals("Using G2 in Import mode, invoice lines from different invoices don't merge when their invoice buyers differ", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestGetKeyForHeaderContainsIncoTermPlace()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "A";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTermPlace = "1";
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = cei.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTermPlace = "3";
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = cei.PK;

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("The Inco Term Place should not be merged in any conditions.", 1, declaration.CustomsEntryHeaders.Count);
		}

		public void TestGetKeyForHeaderContainsIncoTermPlaceCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "A";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.ZG_AgreedPlaceCode = "1";
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = cei.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.ZG_AgreedPlaceCode = "1";
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = cei.PK;

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			invoice2.ZG_AgreedPlaceCode = "3";
			merger.DoMerge();
			AssertEquals("Invoice lines should't merge because their header don't have same Incoterm Place Code", 2, declaration.CustomsEntryHeaders.Count);
		}

		public void TestGetKeyForLineContainsPackType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "A";

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackType = "CT";
			package1.CW_PackQty = 10;

			var package2 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package2.CW_PackType = "CT";
			package2.CW_PackQty = 10;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = cei.PK;

			var linkPackageCollection = invoiceLine1.PackagesForInvoiceLinesForBindingOnly;
			linkPackageCollection[0].IsLinked = true;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = cei.PK;
			var linkPackageCollection2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly;
			linkPackageCollection2[1].IsLinked = true;

			AssertEquals("CT", invoiceLine1.SelectedPackType);
			AssertEquals("CT", invoiceLine2.SelectedPackType);
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			package2.CW_PackType = "1A";
			AssertEquals("CT", invoiceLine1.SelectedPackType);
			AssertEquals("1A", invoiceLine2.SelectedPackType);
			merger.DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestGetKeyForLineContainsPreviousDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var doc1 = invoiceLine1.PreviousDocuments.AddNew();

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var doc2 = invoiceLine2.PreviousDocuments.AddNew();

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			var doc3 = invoiceLine3.PreviousDocuments.AddNew();

			doc1.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			doc2.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			doc3.CSI_Code = PreviousDocumentCodeList.Codes.IST;

			doc1.CSI_ReferenceNumber = "test";
			doc2.CSI_ReferenceNumber = "test";
			doc3.CSI_ReferenceNumber = "test";

			doc1.CSI_LineNo = 1;
			doc2.CSI_LineNo = 1;
			doc3.CSI_LineNo = 1;

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			doc1.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			doc2.CSI_Code = PreviousDocumentCodeList.Codes.EU;
			doc3.CSI_Code = PreviousDocumentCodeList.Codes.IST;

			doc1.CSI_ReferenceNumber = "test";
			doc2.CSI_ReferenceNumber = "test";
			doc3.CSI_ReferenceNumber = "test";

			doc1.CSI_LineNo = 1;
			doc2.CSI_LineNo = 1;
			doc3.CSI_LineNo = 1;

			merger.DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			doc1.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			doc2.CSI_Code = PreviousDocumentCodeList.Codes.EU;
			doc3.CSI_Code = PreviousDocumentCodeList.Codes.IST;

			doc1.CSI_ReferenceNumber = "test";
			doc2.CSI_ReferenceNumber = "test";
			doc3.CSI_ReferenceNumber = "test2";

			doc1.CSI_LineNo = 1;
			doc2.CSI_LineNo = 1;
			doc3.CSI_LineNo = 1;

			merger.DoMerge();
			AssertEquals(3, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			doc1.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			doc2.CSI_Code = PreviousDocumentCodeList.Codes.EU;
			doc3.CSI_Code = PreviousDocumentCodeList.Codes.IST;

			doc1.CSI_ReferenceNumber = "test";
			doc2.CSI_ReferenceNumber = "test";
			doc3.CSI_ReferenceNumber = "test";

			doc1.CSI_LineNo = 1;
			doc2.CSI_LineNo = 1;
			doc3.CSI_LineNo = 2;

			merger.DoMerge();
			AssertEquals(3, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			doc1.CSI_Code = PreviousDocumentCodeList.Codes.EU;
			doc2.CSI_Code = PreviousDocumentCodeList.Codes.EU;
			doc3.CSI_Code = PreviousDocumentCodeList.Codes.IST;

			doc1.CSI_ReferenceNumber = "test";
			doc2.CSI_ReferenceNumber = "test";
			doc3.CSI_ReferenceNumber = "test";

			doc1.CSI_LineNo = 2;
			doc2.CSI_LineNo = 1;
			doc3.CSI_LineNo = 1;

			merger.DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestGetKeyForLineContainsPreviousDocument_FromInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var invoice1 = declaration.Invoices.AddNew();
			var doc1 = invoice1.PreviousDocuments.AddNew();
			doc1.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			doc1.CSI_ReferenceNumber = "001";
			doc1.CSI_LineNo = 1;
			var invoiceLineA = invoice1.JobComInvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			var doc2 = invoice1.PreviousDocuments.AddNew();
			doc2.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			doc2.CSI_ReferenceNumber = "002";
			doc2.CSI_LineNo = 1;
			var invoiceLineB = invoice2.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestGetKeyForLineContainsPreviousInbondMovement()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var doc1 = invoiceLine1.PreviousDocuments.AddNew();

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var doc2 = invoiceLine2.PreviousDocuments.AddNew();

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			var doc3 = invoiceLine3.PreviousDocuments.AddNew();

			doc1.CSI_Code = PreviousDocumentCodeList.Codes.IM;
			doc2.CSI_Code = PreviousDocumentCodeList.Codes.IM;
			doc3.CSI_Code = PreviousDocumentCodeList.Codes.IM;

			doc1.CSI_ReferenceNumber = "test";
			doc2.CSI_ReferenceNumber = "test";
			doc3.CSI_ReferenceNumber = "test";

			doc1.CSI_LineNo = 1;
			doc2.CSI_LineNo = 2;
			doc3.CSI_LineNo = 3;

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			doc1.CSI_Code = PreviousDocumentCodeList.Codes.IM;
			doc2.CSI_Code = PreviousDocumentCodeList.Codes.EU;
			doc3.CSI_Code = PreviousDocumentCodeList.Codes.IM;

			doc1.CSI_ReferenceNumber = "test";
			doc2.CSI_ReferenceNumber = "test";
			doc3.CSI_ReferenceNumber = "test";

			merger.DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			doc1.CSI_Code = PreviousDocumentCodeList.Codes.IM;
			doc2.CSI_Code = PreviousDocumentCodeList.Codes.EU;
			doc3.CSI_Code = PreviousDocumentCodeList.Codes.IM;

			doc1.CSI_ReferenceNumber = "test";
			doc2.CSI_ReferenceNumber = "test";
			doc3.CSI_ReferenceNumber = "test2";

			merger.DoMerge();
			AssertEquals(3, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeKeyInvoiceForSupportingDocs()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";

			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var inv1 = dec.Invoices.AddNew();
			inv1.JobComInvoiceLines.AddNew();

			var supDoc1 = inv1.SupportingDocuments.AddNew();
			supDoc1.CSI_Code = "AAA";
			supDoc1.CSI_ReferenceNumber = "1er doc";
			supDoc1.CSI_Quantity = 1;

			var merger = new LineMerger(dec);
			merger.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);

			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.JobComInvoiceLines.AddNew();

			var supDoc2 = inv2.SupportingDocuments.AddNew();
			supDoc2.CSI_Code = "BBB";
			supDoc2.CSI_Quantity = 2;

			merger = new LineMerger(dec);
			merger.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);

			supDoc2.CSI_Code = "AAA";
			supDoc2.CSI_ReferenceNumber = "2eme doc";
			merger = new LineMerger(dec);
			merger.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);

			supDoc2.CSI_Code = "N380";
			supDoc1.CSI_Code = "N380";

			merger = new LineMerger(dec);
			merger.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeKeyInvoiceLineForSupportingDocs()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";

			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var inv1 = dec.Invoices.AddNew();
			var invoiceline1 = inv1.JobComInvoiceLines.AddNew();

			var supDoc1 = invoiceline1.SupportingDocuments.AddNew();
			supDoc1.CSI_Code = "AAA";
			supDoc1.CSI_ReferenceNumber = "1er doc";
			supDoc1.CSI_Quantity = 1;

			var merger = new LineMerger(dec);
			merger.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);

			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.JobComInvoiceLines.AddNew();

			var supDoc2 = invLine2.SupportingDocuments.AddNew();
			supDoc2.CSI_Code = "BBB";
			supDoc2.CSI_Quantity = 2;

			merger = new LineMerger(dec);
			merger.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);

			supDoc2.CSI_Code = "AAA";
			supDoc2.CSI_ReferenceNumber = "2eme doc";
			merger = new LineMerger(dec);
			merger.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);

			supDoc2.CSI_Code = "N380";
			supDoc1.CSI_Code = "N380";

			merger = new LineMerger(dec);
			merger.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeKeyInvoiceLineForZG_CountryOfDispatch()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";

			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var inv1 = dec.Invoices.AddNew();
			var invoiceline1 = inv1.JobComInvoiceLines.AddNew();
			invoiceline1.ZG_CountryOfDispatch = "DE";

			var merger = new LineMerger(dec);
			merger.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);

			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.JobComInvoiceLines.AddNew();
			invLine2.ZG_CountryOfDispatch = "ES";

			merger = new LineMerger(dec);
			merger.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);

			invLine2.ZG_CountryOfDispatch = "DE";
			merger = new LineMerger(dec);
			merger.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);
		}
	}
}
