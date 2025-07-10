using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(DrawbackEntryLineFilterBusinessObject))]
	sealed class DrawbackEntryLineFilterBusinessObjectTest : Customs.Module.Testing.EntryLineFilterBusinessObjectTest
	{
		public void TestGetNatureTypeQuery()
		{
			var referenceDecN10 = Factory.New<JobDeclaration>();
			referenceDecN10.JE_GB = Declaration1.JE_GB;
			referenceDecN10.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			referenceDecN10.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			referenceDecN10.JE_OH_Importer = importer.PK;
			var referenceDecN10EntryHeader = referenceDecN10.CustomsEntryHeaders.AddNew();
			referenceDecN10EntryHeader.EntryNumber = "N10";
			var referenceDecN10EntryLine = referenceDecN10EntryHeader.MergedLines.AddNew();
			referenceDecN10EntryLine.CL_AdValoremTariff = "01011000";
			var referenceDecN10InvoiceHeader = referenceDecN10.Invoices.AddNew();
			referenceDecN10InvoiceHeader.JZ_IncoTerm = "FOB";
			referenceDecN10InvoiceHeader.JZ_InvoiceAmount = 7500m;
			referenceDecN10InvoiceHeader.JZ_RX_NKInvoice_Currency = referenceDecN10InvoiceHeader.LocalCurrencyCode;
			var referenceDecN10InvoiceLine = referenceDecN10InvoiceHeader.JobComInvoiceLines.AddNew();
			referenceDecN10InvoiceLine.JI_CL = referenceDecN10EntryLine.PK;
			referenceDecN10InvoiceLine.JI_OP = part.PK;
			referenceDecN10InvoiceLine.JI_PartNo = "PART1";
			referenceDecN10InvoiceLine.JI_InvoiceQuantity = 20m;
			referenceDecN10InvoiceLine.JI_InvoiceUQ = "NO";
			referenceDecN10InvoiceLine.JI_CustomsQuantity = 20m;
			referenceDecN10InvoiceLine.JI_CustomsUnitQty = "NO";
			var referenceDecN20 = Factory.New<JobDeclaration>();
			referenceDecN20.JE_GB = Declaration1.JE_GB;
			referenceDecN20.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			referenceDecN20.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			referenceDecN20.JE_OH_Importer = importer.PK;
			var referenceDecN20EntryHeader = referenceDecN20.CustomsEntryHeaders.AddNew();
			referenceDecN20EntryHeader.EntryNumber = "N20";
			var referenceDecN20EntryLine = referenceDecN20EntryHeader.MergedLines.AddNew();
			referenceDecN20EntryLine.CL_AdValoremTariff = "01011000";
			var referenceDecN20InvoiceHeader = referenceDecN10.Invoices.AddNew();
			referenceDecN20InvoiceHeader.JZ_IncoTerm = "FOB";
			referenceDecN20InvoiceHeader.JZ_InvoiceAmount = 7500m;
			referenceDecN20InvoiceHeader.JZ_RX_NKInvoice_Currency = referenceDecN20InvoiceHeader.LocalCurrencyCode;
			var referenceDecN20InvoiceLine = referenceDecN20InvoiceHeader.JobComInvoiceLines.AddNew();
			referenceDecN20InvoiceLine.JI_CL = referenceDecN20EntryLine.PK;
			referenceDecN20InvoiceLine.JI_OP = part.PK;
			referenceDecN20InvoiceLine.JI_PartNo = "PART1";
			referenceDecN20InvoiceLine.JI_InvoiceQuantity = 20m;
			referenceDecN20InvoiceLine.JI_InvoiceUQ = "NO";
			referenceDecN20InvoiceLine.JI_CustomsQuantity = 20m;
			referenceDecN20InvoiceLine.JI_CustomsUnitQty = "NO";
			referenceDecN20InvoiceLine.JI_IsPackToBondForLine = false;
			Factory.Save();
			ModuleTextFilter entryNumberQuery = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.EntryNumber];
			entryNumberQuery.Property = "N10";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Pre-Condition", 1, filterCollection.Count);
			entryNumberQuery.Property = "N20";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Pre-Condition", 1, filterCollection.Count);
			referenceDecN20InvoiceLine.JI_IsPackToBondForLine = true;
			Factory.Save();
			entryNumberQuery.Property = "N20";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Pre-Condition", 0, filterCollection.Count);
		}

		public void TestDrawbackDateFilterAdded()
		{
			AssertNotNull("Drawback Date filter applies to AU system - should be added here", filterBO[DeclarationFilterConstants.DrawbackDate]);
		}

		public void TestImportEntriesShownAreEarlierThanDrawbackEntry()
		{
			var importDec1 = Factory.New<BaseJobDeclaration>();
			importDec1.JE_OH_Importer = importer1.PK;
			importDec1.JE_DateOfFirstArrival = ZDateTime.Now.AddDays(-3);
			importDec1.JE_DateOfArrival = ZDateTime.Now.AddDays(-2);
			importDec1.JE_SystemCreateTimeUtc = createDate.AddDays(-4);
			importDec1.JE_OwnerRef = "IMPDEC1";
			importDec1.JE_DeclarationReference = "JOB1";
			var iD1entryHeader = importDec1.CustomsEntryHeaders.AddNew();
			var iD1entrynumber = Factory.New<CusEntryNumber>();
			iD1entrynumber.CE_ParentID = iD1entryHeader.PK;
			iD1entrynumber.CE_ParentTable = iD1entryHeader.TableName;
			iD1entrynumber.CE_EntryNum = "ENTRY1";
			iD1entrynumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var iD1invHeader1 = importDec1.Invoices.AddNew();
			iD1invHeader1.JZ_InvoiceDate = ZDateTime.Now;
			var iD1line1 = iD1entryHeader.MergedLines.AddNew();
			iD1line1.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			iD1line1.CL_AdValoremTariff = "1111.11.11";
			iD1line1.CL_LineNumber = 1;
			var iD1invLine1 = iD1invHeader1.JobComInvoiceLines.AddNew();
			iD1invLine1.JI_CL = iD1line1.PK;
			iD1invLine1.JI_CC = class2.PK;
			var iD1line2 = iD1entryHeader.MergedLines.AddNew();
			iD1line2.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			iD1line2.CL_AdValoremTariff = "2222.22.22";
			iD1line2.CL_LineNumber = 2;
			var iD1invLine2 = iD1invHeader1.JobComInvoiceLines.AddNew();
			iD1invLine2.JI_CL = line2.PK;
			iD1invLine2.JI_OP = part2.PK;
			iD1invLine2.JI_CC = class2.PK;
			Factory.Save();
			filterCollection = new GlobalDrawbackCusEntryLineCollection(Factory, drawbackDeclaration);
			filterCollection.Load();
			AssertEquals("Pre-Condition - 6 jobs found, (5 from Base, 1 dec from above)", 6, filterCollection.Count);
			importDec1.JE_EntrySubmittedDate = createDate.AddDays(-4);
			var importDec2 = Factory.New<BaseJobDeclaration>();
			importDec2.JE_OH_Importer = importer1.PK;
			importDec2.JE_DateOfFirstArrival = ZDateTime.Now.AddDays(-3);
			importDec2.JE_DateOfArrival = ZDateTime.Now.AddDays(-2);
			importDec2.JE_SystemCreateTimeUtc = createDate.AddDays(-6);
			importDec2.JE_EntrySubmittedDate = createDate.AddDays(-6);
			importDec2.JE_OwnerRef = "IMPDEC2";
			importDec2.JE_DeclarationReference = "JOB2";
			var iD2entryHeader = importDec2.CustomsEntryHeaders.AddNew();
			var iD2entrynumber = Factory.New<CusEntryNumber>();
			iD2entrynumber.CE_ParentID = iD2entryHeader.PK;
			iD2entrynumber.CE_ParentTable = iD2entryHeader.TableName;
			iD2entrynumber.CE_EntryNum = "ENTRY2";
			iD2entrynumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var iD2invHeader2 = importDec2.Invoices.AddNew();
			iD2invHeader2.JZ_InvoiceDate = ZDateTime.Now;
			var iD2line1 = iD2entryHeader.MergedLines.AddNew();
			iD2line1.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			iD2line1.CL_AdValoremTariff = "1111.11.11";
			iD2line1.CL_LineNumber = 1;
			var iD2invLine = iD2invHeader2.JobComInvoiceLines.AddNew();
			iD2invLine.JI_CL = line1.PK;
			iD2invLine.JI_CC = class2.PK;
			drawbackDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
			var drawbackInvoice = drawbackDeclaration.Invoices.AddNew();
			drawbackInvoice.JZ_InvoiceDate = createDate.AddDays(-8);
			Factory.Save();
			filterCollection = new GlobalDrawbackCusEntryLineCollection(Factory, drawbackDeclaration);
			filterCollection.Load();
			AssertEquals("7 import jobs now in system", 7, filterCollection.Count);
			filterCollection = new GlobalDrawbackCusEntryLineCollection(Factory, drawbackDeclaration);
			filterCollection.Load(filterBO.Filter);
			AssertEquals("5 jobs found, (Import jobs submitted after earliest drawback invoice date are excluded)", 5, filterCollection.Count);
		}

		public void TestGetConsolidatedEntryInvoiceLineNumberForLinkedDeclaration()
		{
			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<Declaration.Business.ConsolidatedDeclaration>(Factory);
			var leadDeclaration = consolidatedDeclaration.LeadDeclaration as JobDeclaration;
			leadDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			leadDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			leadDeclaration.JE_DeclarationReference = "B00000001";
			leadDeclaration.JE_OH_Importer = importer.PK;

			var leadEntryHeader = leadDeclaration.CustomsEntryHeaders[0];
			leadEntryHeader.EntryNumber = "AAAGMM44C";
			var leadEntryLine1 = leadEntryHeader.MergedLines.AddNew();
			leadEntryLine1.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			leadEntryLine1.CL_AdValoremTariff = "4444.44.44";
			leadEntryLine1.CL_LineNumber = 1;
			leadEntryLine1.ZA_AggregateEntryLineNumber = 1;

			var leadEntryLine2 = leadEntryHeader.MergedLines.AddNew();
			leadEntryLine2.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			leadEntryLine2.CL_AdValoremTariff = "4444.44.44";
			leadEntryLine2.CL_LineNumber = 2;
			leadEntryLine2.ZA_AggregateEntryLineNumber = 2;

			var memberDeclaration = consolidatedDeclaration.JobDeclarations[1] as JobDeclaration;
			memberDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			memberDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			memberDeclaration.JE_DeclarationReference = "B00000002";
			memberDeclaration.JE_OH_Importer = importer.PK;

			var memberEntryHeader = memberDeclaration.CustomsEntryHeaders[0];
			memberEntryHeader.EntryNumber = "AAAGMM44C";
			var memberEntryLine1 = memberEntryHeader.MergedLines.AddNew();
			memberEntryLine1.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			memberEntryLine1.CL_AdValoremTariff = "4444.44.44";
			memberEntryLine1.CL_LineNumber = 1;
			memberEntryLine1.ZA_AggregateEntryLineNumber = 3;

			var memberEntryLine2 = memberEntryHeader.MergedLines.AddNew();
			memberEntryLine2.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			memberEntryLine2.CL_AdValoremTariff = "4444.44.44";
			memberEntryLine2.CL_LineNumber = 2;
			memberEntryLine2.ZA_AggregateEntryLineNumber = 4;

			Factory.Save();

			var drawbackDeclaration = Factory.New<JobDeclaration>();
			drawbackDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			drawbackDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			drawbackDeclaration.JE_OH_Importer = importer.PK;
			var drawbackHeader = drawbackDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var drawbackInvoiceLine = drawbackHeader.JobComInvoiceLines.AddNew();
			drawbackInvoiceLine.JI_IsPackToBondForLine = false;
			drawbackInvoiceLine.JI_Tariff = "4444.44.44";

			using (EntryLineModule module = (EntryLineModule)ZModuleFactory.Instance.Create(ModuleIDs.EntryLine))
			{
				var filter = module.FilterBusinessObject;
				ModuleTextFilter entryNumberQuery = (ModuleTextFilter)filter[DeclarationFilterConstants.NumberFilterTypes.EntryNumber];
				entryNumberQuery.Property = "AAAGMM44C";
				var filterCollection = new GlobalCusEntryLineCollection(Factory, drawbackDeclaration);
				filterCollection.Load(filter.Filter);
				AssertEquals("Entry line found", 4, filterCollection.Count);
				AssertEquals("Entry line 1 EffectiveLineNumber", (ZShort)1, filterCollection[0].EffectiveLineNumber);
				AssertEquals("Entry line 2 EffectiveLineNumber", (ZShort)2, filterCollection[1].EffectiveLineNumber);
				AssertEquals("Entry line 3 EffectiveLineNumber", (ZShort)3, filterCollection[2].EffectiveLineNumber);
				AssertEquals("Entry line 4 EffectiveLineNumber", (ZShort)4, filterCollection[3].EffectiveLineNumber);
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new DrawbackEntryLineFilterBusinessObject(filterCollection);

		JobDeclaration Declaration1 => (JobDeclaration)declaration1;

		OrgHeader importer;
		AUOrgSupplierPart part;
		protected override void SetUp()
		{
			base.SetUp();
			importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ORG1";
			part = Factory.New<AUOrgSupplierPart>();
			part.OP_PartNum = "PART1";
			part.OP_Desc = "PART 1 DESCRIPTION";
			part.OP_StockKeepingUnit = "NO";
		}
	}
}
