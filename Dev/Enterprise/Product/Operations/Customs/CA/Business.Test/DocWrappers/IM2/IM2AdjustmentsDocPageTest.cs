using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(IM2AdjustmentsDocPage))]
	sealed class IM2AdjustmentsDocPageTest : NonPersistentBusinessObjectTestCase
	{
		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		public void TestGetDocLineForGSTGSD()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0301100000", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff1, "CU1", "KGM");
			var tariff2 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0301100001", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff2, "CU1", "KGM");
			Factory.Save();

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationTariff;

			var invoice0 = CreateJobComInvoice(declaration, "INV000");
			var invoiceLine0 = CreateJobComInvoiceLine(invoice0, "0301100000");
			var invoiceLine1 = CreateJobComInvoiceLine(invoice0, "0301100001");
			var gSTDuty = invoiceLine1.DutiesAndTaxes.AddNew(Registry.EntryChargeTypeList.Codes.TotalGSTAmount);
			gSTDuty.C1_Amount = 100m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var entryline1 = declaration.B3EntryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "0301100000");
			var entryline2 = declaration.B3EntryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "0301100001");
			AssertEquals(100m, entryline2.Fees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalGSTAmount));
			AssertEquals(0m, entryline2.Fees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalGSTDirectAmount));

			var im2 = declaration.GetNewCopyToB2Declaration();
			im2.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
			var newInvoiceLine0 = im2.InvoiceLines.Cast<JobComInvoiceLine>().LastOrDefault(x => x.JI_Tariff == "0301100000");
			newInvoiceLine0.JI_CustomsQuantity = 50m;

			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.DoMerge();
			var im2Entryline = im2.B3EntryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "0301100001");
			AssertEquals(0m, im2Entryline.Fees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalGSTAmount));
			AssertEquals(100m, im2Entryline.Fees.GetAmount(Registry.EntryChargeTypeList.Codes.TotalGSTDirectAmount));

			var docLine = new IM2AdjustmentsDocPage().GetPages(null, im2).ToList();
			AssertEquals(1, docLine.Count);
			var lineNumber = entryline1.CL_LineNumber.ToString();
			AssertEquals(lineNumber, docLine[0].AsAccountForDocLine1.OriginalLineNo);
			AssertEquals(lineNumber, docLine[0].AsClaimForDocLine1.OriginalLineNo);
			AssertEquals(ZString.Empty, docLine[0].AsAccountForDocLine2.OriginalLineNo);
			AssertEquals(ZString.Empty, docLine[0].AsClaimForDocLine2.OriginalLineNo);
		}

		public void TestGetSubHeaderDicForTRM()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationOrTariffOverMultipleInvoices;

			var invoice0 = CreateJobComInvoice(declaration, "INV000");
			var invoiceLine0 = CreateJobComInvoiceLine(invoice0, "0301100000");

			var invoice1 = CreateJobComInvoice(declaration, "INV001");
			var invoiceLine1 = CreateJobComInvoiceLine(invoice1, "0301100001");

			var invoice2 = CreateJobComInvoice(declaration, "INV002");
			var invoiceLine2 = CreateJobComInvoiceLine(invoice2, "0301100002");

			var invoice3 = CreateJobComInvoice(declaration, "INV003");
			var invoiceLine20 = CreateJobComInvoiceLine(invoice3, "0301100000");
			var invoiceLine21 = CreateJobComInvoiceLine(invoice3, "0301100001");
			var invoiceLine22 = CreateJobComInvoiceLine(invoice3, "0301100002");

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var im2 = declaration.GetNewCopyToB2Declaration();
			var newInvoiceLine0 = im2.InvoiceLines.Cast<JobComInvoiceLine>().LastOrDefault(x => x.JI_Tariff == "0301100000");
			newInvoiceLine0.JI_CustomsQuantity = 50m;

			var newInvoiceLine1 = im2.InvoiceLines.Cast<JobComInvoiceLine>().LastOrDefault(x => x.JI_Tariff == "0301100001");
			newInvoiceLine1.JI_CustomsQuantity = 50m;

			var newInvoiceLine2 = im2.InvoiceLines.Cast<JobComInvoiceLine>().LastOrDefault(x => x.JI_Tariff == "0301100002");
			newInvoiceLine2.JI_CustomsQuantity = 50m;

			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();

			im2.DoMerge();

			var docLine = new IM2AdjustmentsDocPage().GetPages(null, im2).ToList();
			AssertEquals(2, docLine.Count);
			AssertEquals("1", docLine[0].SubHeaderNo);
			AssertEquals(ZString.Empty, docLine[1].SubHeaderNo);
		}

		JobComInvoiceLine CreateJobComInvoiceLine(JobComInvoiceHeader invoice0, string tariff)
		{
			var invoiceLine = invoice0.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff;
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_InvoiceQuantity = 10;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 35 CHARACTERS";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_PageNumber = 1;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_CFIAUSStateOfOrigin = invoiceLine.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			invoiceLine.CA_TreatmentCode = "03";
			return invoiceLine;
		}

		JobComInvoiceHeader CreateJobComInvoice(JobDeclaration declaration, ZString invoiceNumber)
		{
			var invoice0 = declaration.Invoices.AddNew();
			invoice0.JZ_RW_NKOriginState = USStatesList.Codes.Texas;
			invoice0.JZ_InvoiceNumber = invoiceNumber;
			invoice0.CA_RN_NKExport = USStatesList.Codes.California;
			invoice0.JZ_InvoiceDate = ZDateTime.Today.AddDays(-5);
			invoice0.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-3);
			invoice0.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			return invoice0;
		}

		public void TestDocPageMember()
		{
			var im2 = GetCopiedIM2();
			im2.InvoiceLines[0].JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			var docLine = new IM2AdjustmentsDocPage().GetPages(null, im2).ToList();
			AssertEquals(1, docLine.Count);
			AssertEquals("1", docLine[0].SubHeaderNo);
			im2.InvoiceLines[0].CA_TreatmentCode = "04";
			docLine = new IM2AdjustmentsDocPage().GetPages(null, im2).ToList();
			AssertEquals(1, docLine.Count);
			AssertEquals("1", docLine[0].SubHeaderNo);
			AssertEquals("04", docLine[0].TariffTreatment);
		}

		public void TestGetSubHeaderDic()
		{
			var imDeclaration = GetCopiedIM2With2Lines();
			Factory.Save();
			AssertNoExceptionThrown(() => new IM2AdjustmentsDocPage().GetPages(null, imDeclaration).ToList());

			AssertEquals("Declaration has an Invoice Heasder", 2, imDeclaration.Invoices.Count);

			var header = imDeclaration.Invoices[0];
			AssertEquals("Header has two Invoice Lines", 2, header.InvoiceLines.Count);
			var entryLines = imDeclaration.B3EntryHeader.AllEntryLines.Cast<CusEntryLine>();
			AssertNoExceptionThrown(() => new IM2AdjustmentsDocPage().GetSubHeaderDic(entryLines));
		}

		public void TestWI00363121()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301100000";
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_InvoiceQuantity = 10;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 35 CHARACTERS";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_PageNumber = 1;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_CFIAUSStateOfOrigin = invoiceLine.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			invoiceLine.CA_TreatmentCode = "02";
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301100001";
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_InvoiceQuantity = 10;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 35 CHARACTERS";
			invoiceLine.JI_LineNo = 2;
			invoiceLine.CA_PageNumber = 1;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_CFIAUSStateOfOrigin = invoiceLine.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			invoiceLine.CA_TreatmentCode = "02";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var im2 = declaration.GetNewCopyToB2Declaration();
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.DoMerge();
			Factory.Save();

			declaration.InvoiceLines[1].CA_TreatmentCode = "10";
			declaration.DoMerge();
			Factory.Save();
			im2.InvoiceLines[0].CA_TreatmentCode = "10";
			im2.InvoiceLines[1].CA_TreatmentCode = "10";
			im2.DoMerge();
			Factory.Save();

			var docLine = new IM2AdjustmentsDocPage().GetPages(null, im2).ToList();
			AssertEquals(2, docLine.Count);
			AssertEquals("1", docLine[0].SubHeaderNo);
			AssertEquals("10", docLine[0].TariffTreatment);
			AssertEquals("2", docLine[1].SubHeaderNo);
			AssertEquals("10", docLine[1].TariffTreatment);

			Assert(!docLine[0].AsAccountForDocLine1.OriginalLineNo.IsEmpty);
			Assert(!docLine[0].AsClaimForDocLine1.OriginalLineNo.IsEmpty);
			Assert(docLine[0].AsAccountForDocLine2.OriginalLineNo.IsEmpty);
			Assert(docLine[0].AsClaimForDocLine2.OriginalLineNo.IsEmpty);

			Assert(!docLine[1].AsAccountForDocLine1.OriginalLineNo.IsEmpty);
			Assert(!docLine[1].AsClaimForDocLine1.OriginalLineNo.IsEmpty);
			Assert(docLine[1].AsAccountForDocLine2.OriginalLineNo.IsEmpty);
			Assert(docLine[1].AsClaimForDocLine2.OriginalLineNo.IsEmpty);
		}

		public void TestSplitLine()
		{
			var im2 = GetCopiedIM2();
			im2.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			im2.CA_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = im2.Invoices[0];
			var invoiceLine1 = invoice.JobComInvoiceLines[0];
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301100000";
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_InvoiceQuantity = 10;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 35 CHARACTERS";
			invoiceLine.JI_LineNo = 2;
			invoiceLine.CA_PageNumber = 1;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_CFIAUSStateOfOrigin = invoiceLine.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			invoiceLine.CA_TreatmentCode = "03";
			invoiceLine.CA_PreviousB3SubHeaderNo = 1;
			invoiceLine.CA_PreviousB3LineNo = 1;
			im2.DoMerge();
			Factory.Save();

			var docLine = new IM2AdjustmentsDocPage().GetPages(null, im2).ToList();
			AssertEquals(1, docLine.Count);
			AssertEquals("1", docLine[0].AsAccountForDocLine1.OriginalLineNo);
			AssertEquals("1", docLine[0].AsClaimForDocLine1.OriginalLineNo);
			AssertEquals(ZString.Empty, docLine[0].AsAccountForDocLine2.OriginalLineNo);
			AssertEquals("1/SL", docLine[0].AsClaimForDocLine2.OriginalLineNo);
		}

		public void TestSplitLine_MatchSubHeader()
		{
			var im2 = GetCopiedIM2();
			im2.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			im2.CA_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = im2.Invoices[0];
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301100000";
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_InvoiceQuantity = 10;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 35 CHARACTERS";
			invoiceLine.JI_LineNo = 2;
			invoiceLine.CA_PageNumber = 1;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_CFIAUSStateOfOrigin = invoiceLine.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			invoiceLine.CA_TreatmentCode = "03";
			invoiceLine.CA_PreviousB3SubHeaderNo = 1;
			invoiceLine.CA_PreviousB3LineNo = 1;
			im2.DoMerge();
			Factory.Save();

			var docLine = new IM2AdjustmentsDocPage().GetPages(null, im2).ToList();
			AssertEquals(1, docLine.Count);
			AssertEquals("1", docLine[0].AsAccountForDocLine1.OriginalLineNo);
			AssertEquals("1/SL", docLine[0].AsClaimForDocLine1.OriginalLineNo);
			AssertEquals(ZString.Empty, docLine[0].AsAccountForDocLine2.OriginalLineNo);
			AssertEquals(ZString.Empty, docLine[0].AsClaimForDocLine2.OriginalLineNo);
		}

		JobDeclaration GetCopiedIM2()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0301100000", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff1, "CU1", "KGM");
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301100000";
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_InvoiceQuantity = 10;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 35 CHARACTERS";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_PageNumber = 1;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_CFIAUSStateOfOrigin = invoiceLine.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			invoiceLine.CA_TreatmentCode = "03";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			var im2 = declaration.GetNewCopyToB2Declaration();
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.DoMerge();
			return im2;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var im2 = GetCopiedIM2();
			im2.InvoiceLines[0].JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			var docLine = new IM2AdjustmentsDocPage().GetPages(null, im2).ToList();
			return docLine.ElementAt(0);
		}

		JobDeclaration declaration;
		JobDeclaration GetCopiedIM2With2Lines()
		{
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RW_NKOriginState = USStatesList.Codes.Texas;
			invoice.CA_RN_NKExport = USStatesList.Codes.California;
			invoice.JZ_InvoiceDate = ZDateTime.Today.AddDays(-5);
			invoice.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-3);
			invoice.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301100000";
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_InvoiceQuantity = 10;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 35 CHARACTERS";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_PageNumber = 1;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_CFIAUSStateOfOrigin = invoiceLine.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			invoiceLine.CA_TreatmentCode = "03";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0301100000";
			invoiceLine2.JI_CustomsQuantity = 20;
			invoiceLine2.JI_CustomsUnitQty = "KGM";
			invoiceLine2.JI_InvoiceQuantity = 10;
			invoiceLine2.JI_InvoiceUQ = "NMB";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine2.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 35 CHARACTERS2";
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.CA_PageNumber = 1;
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine2.CA_CFIAUSStateOfOrigin = invoiceLine2.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			invoiceLine2.CA_TreatmentCode = "03";

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RW_NKOriginState = USStatesList.Codes.Texas;
			invoice2.CA_RN_NKExport = USStatesList.Codes.California;
			invoice2.JZ_InvoiceDate = ZDateTime.Today.AddDays(-5);
			invoice2.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-3);
			invoice2.JZ_RX_NKInvoice_Currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates).RX_Code;
			var invoiceLine1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0301100000";
			invoiceLine1.JI_CustomsQuantity = 20;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			invoiceLine1.JI_InvoiceQuantity = 10;
			invoiceLine1.JI_InvoiceUQ = "NMB";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine1.JI_Description = "1 LONG GOODS DESCRIPTION WHICH EXCEEDS 35 CHARACTERS";
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.CA_PageNumber = 1;
			invoiceLine1.JI_LinePrice = 100m;
			invoiceLine1.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine1.CA_CFIAUSStateOfOrigin = invoiceLine1.AddInfoLookups.CFIAStatesOfOrigin[0].Code;
			invoiceLine1.CA_TreatmentCode = "03";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var im2 = declaration.GetNewCopyToB2Declaration();
			im2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			im2.DoMerge();
			return im2;
		}
	}
}
