using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DrawbackAmountCalculatorTest_AUCClass : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetValuesForRepresentativeShipmentBestFitOnlyLookForAUJobs()
		{
			var anotherFactory = new BusinessObjectFactory();

			var importer = anotherFactory.NewWithValidTestData<OrgHeader>();
			var part = anotherFactory.NewWithValidTestData<AUOrgSupplierPart>();
			part.OP_PartNum = "PART";
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var company = anotherFactory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var refDec = anotherFactory.NewWithValidTestData<BaseJobDeclaration>();
				refDec.JE_OH_Importer = importer.PK;
				refDec.JE_GB = branch.PK;
				var refEntryHeader = refDec.CustomsEntryHeaders.AddNew();
				refEntryHeader.FillWithValidTestData();
				var refEntryLine = refEntryHeader.MergedLines.AddNew();
				refEntryLine.FillWithValidTestData();
				var refInvoice = refDec.Invoices.AddNew();
				refInvoice.FillWithValidTestData();
				var refInvoiceLine = refInvoice.JobComInvoiceLines.AddNew();
				refInvoiceLine.FillWithValidTestData();
				refInvoiceLine.JI_PartNo = "PART";
				refInvoiceLine.JI_CL = refEntryLine.PK;
			}

			anotherFactory.Save();

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			drawback.JE_OH_Importer = importer.PK;
			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment;
			invoiceLine.JI_PartNo = "PART";
			var calculator = new DrawbackAmountCalculator(invoiceLine);
			calculator.Calculate();
		}

		public void TestSettingRefDecWithNoCustomsQuantity()
		{
			line1.JI_PartNo = ZString.Empty;
			line1.AddInfo.ZA_DAM_Hidden = "A";
			line1.AddInfo.ZA_DDN_Hidden = "ENTRY4";
			line1.AddInfo.ZA_DDL_Hidden = 1;
			AssertEquals("CustomsValue", 3000m, line1.DrawbackCustomsValue);
			AssertEquals("Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Claim Duty Amount", 150m, line1.DrawbackDutyAmount);

			line1.AddInfo.ZA_DDL_Hidden = 0;
			line1.JI_InvoiceUQ = Enterprise.Core.Constants.Area.SquareFoot;
			line1.JI_InvoiceQuantity = 8m;
			line1.JI_CustomsUnitQty = ZString.Empty;
			line1.JI_CustomsQuantity = 0;
			line1.AddInfo.ZA_DDL_Hidden = 1;
			AssertEquals("CustomsValue", 1486.45m, line1.DrawbackCustomsValue);
			AssertEquals("Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Claim Duty Amount", 74.32m, line1.DrawbackDutyAmount);
		}

		public void TestSettingRefDecWithNoCustomsQuantityAndNoCustomsUnits()
		{
			mergedDecLine1.JI_CustomsUnitQty = ZString.Empty;
			mergedDecLine1.JI_CustomsQuantity = 0m;
			mergedDecLine2.JI_CustomsUnitQty = ZString.Empty;
			mergedDecLine2.JI_CustomsQuantity = 0m;
			mergedDecLine3.JI_CustomsUnitQty = ZString.Empty;
			mergedDecLine3.JI_CustomsQuantity = 0m;

			line1.JI_PartNo = ZString.Empty;
			line1.AddInfo.ZA_DAM_Hidden = "A";
			line1.AddInfo.ZA_DDN_Hidden = "ENTRY4";
			line1.AddInfo.ZA_DDL_Hidden = 1;
			AssertEquals("CustomsValue", 3000m, line1.DrawbackCustomsValue);
			AssertEquals("Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Claim Duty Amount", 150m, line1.DrawbackDutyAmount);

			line1.AddInfo.ZA_DDL_Hidden = 0;
			line1.JI_InvoiceUQ = Enterprise.Core.Constants.Area.SquareFoot;
			line1.JI_InvoiceQuantity = 8m;
			line1.JI_CustomsUnitQty = ZString.Empty;
			line1.JI_CustomsQuantity = 0;
			line1.AddInfo.ZA_DDL_Hidden = 1;
			AssertEquals("CustomsValue", 1486.45m, line1.DrawbackCustomsValue);
			AssertEquals("Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Claim Duty Amount", 74.32m, line1.DrawbackDutyAmount);
		}

		public void TestSettingRefDecWithNoCustomsQuantityWithMergedDec()
		{
			line1.JI_PartNo = "PART3";
			line1.AddInfo.ZA_DAM_Hidden = "A";
			line1.AddInfo.ZA_DDN_Hidden = "ENTRY4";
			line1.AddInfo.ZA_DDL_Hidden = 1;
			AssertEquals("CustomsValue", 2000m, line1.DrawbackCustomsValue);
			AssertEquals("Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Claim Duty Amount", 100m, line1.DrawbackDutyAmount);

			line1.AddInfo.ZA_DDL_Hidden = 0;
			line1.JI_InvoiceUQ = Enterprise.Core.Constants.Area.SquareFoot;
			line1.JI_InvoiceQuantity = 5.5m;
			line1.JI_CustomsUnitQty = ZString.Empty;
			line1.JI_CustomsQuantity = 0;
			line1.AddInfo.ZA_DDL_Hidden = 1;
			AssertEquals("CustomsValue", 2043.87m, line1.DrawbackCustomsValue);
			AssertEquals("Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Claim Duty Amount", 102.19m, line1.DrawbackDutyAmount);
		}

		public void TestSettingRefDecWithNoCustomsQuantityAndNoCustomsUnitsWithMergedDec()
		{
			mergedDecLine1.JI_CustomsUnitQty = ZString.Empty;
			mergedDecLine1.JI_CustomsQuantity = 0m;
			mergedDecLine2.JI_CustomsUnitQty = ZString.Empty;
			mergedDecLine2.JI_CustomsQuantity = 0m;
			mergedDecLine3.JI_CustomsUnitQty = ZString.Empty;
			mergedDecLine3.JI_CustomsQuantity = 0m;

			line1.JI_PartNo = "PART3";
			line1.AddInfo.ZA_DAM_Hidden = "A";
			line1.AddInfo.ZA_DDN_Hidden = "ENTRY4";
			line1.AddInfo.ZA_DDL_Hidden = 1;
			AssertEquals("CustomsValue", 2000m, line1.DrawbackCustomsValue);
			AssertEquals("Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Claim Duty Amount", 100m, line1.DrawbackDutyAmount);

			line1.AddInfo.ZA_DDL_Hidden = 0;
			line1.JI_InvoiceUQ = Enterprise.Core.Constants.Area.SquareFoot;
			line1.JI_InvoiceQuantity = 5.5m;
			line1.JI_CustomsUnitQty = ZString.Empty;
			line1.JI_CustomsQuantity = 0;
			line1.AddInfo.ZA_DDL_Hidden = 1;
			AssertEquals("CustomsValue", 2043.87m, line1.DrawbackCustomsValue);
			AssertEquals("Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Claim Duty Amount", 102.19m, line1.DrawbackDutyAmount);
		}

		public void TestDrawbackMethodACalc()
		{
			line1.AddInfo.ZA_DDN_Hidden = "ENTRY2";
			line1.AddInfo.ZA_DDL_Hidden = 5;
			line1.JI_CustomsUnitQty = "NO";
			line1.JI_CustomsQuantity = 10m;
			line1.AddInfo.ZA_DAM_Hidden = "A";
			AssertEquals("Method A CustomsValue", 2500m, line1.DrawbackCustomsValue);
			AssertEquals("Method A Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Method A Claim Duty Amount", 100m, line1.DrawbackDutyAmount);
		}

		public void TestDrawbackMethodBAverageCalc()
		{
			line1.AddInfo.ZA_DDN_Hidden = "ENTRY2";
			line1.AddInfo.ZA_DDL_Hidden = 5;
			AssertNotNull(line1.DrawbackImportEntryLine);
			StmNote drawbackNote = Factory.New<StmNote>();
			drawbackNote.ST_ParentID = line1.PK;
			drawbackNote.ST_Table = "JOBCOMINVOICELINE";
			drawbackNote.ST_Description = "DrawbackEntryLines";
			drawbackNote.ST_NoteType = nameof(CargoWise.Definitions.StmNoteVisibility.DOC);
			drawbackNote.ST_NoteDataAsText = "ENTRY1*1*15,ENTRY2*5*5";
			line1.DrawbackCusEntryLineCollection.LoadFromNote();
			line1.JI_CustomsUnitQty = "NO";
			line1.JI_CustomsQuantity = 10m;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertNull(line1.DrawbackImportEntryLine);
			AssertEquals("Method B CustomsValue", 3125m, line1.DrawbackCustomsValue);
			AssertEquals("Method B Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Method B Claim Duty Amount", 150m, line1.DrawbackDutyAmount);
		}

		public void TestDrawbackMethodBNoEntryDoesNotCauseException()
		{
			invoice1.JZ_InvoiceDate = new ZDateTime(2007, 7, 1);
			referenceDec.ManualClearanceDate = new ZDateTime(2007, 6, 30);
			referenceDecEntryHeader1.EntryNumber = ZString.Empty;
			Factory.Save();

			line1.JI_CustomsUnitQty = "NO";
			line1.JI_CustomsQuantity = 10m;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("No dec selected", "ENTRY1", line1.DrawbackImportDeclarationNumber);
		}

		public void TestDrawbackMethodBLowestValue()
		{
			invoice1.JZ_InvoiceDate = new ZDateTime(2007, 7, 1);
			referenceDec.ManualClearanceDate = new ZDateTime(2007, 6, 30);
			Factory.Save();

			line1.JI_CustomsUnitQty = "NO";
			line1.JI_CustomsQuantity = 10m;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("Correct dec selected", "ENTRY2", line1.DrawbackImportDeclarationNumber);
			AssertEquals("Correct dec selected", 5, line1.DrawbackImportDeclarationLine);
			AssertEquals("Method B CustomsValue", 2500m, line1.DrawbackCustomsValue);
			AssertEquals("Method B Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Method B Claim Duty Amount", 100m, line1.DrawbackDutyAmount);

			referenceDecEntryLine2.CL_DutyPercent = 0m;
			Factory.Save();
			line1.JI_CustomsQuantity = 30m;
			line1.AddInfo.ZA_DAM_Hidden = "";
			line1.AddInfo.ZA_DDN_Hidden = "";
			line1.AddInfo.ZA_DDL_Hidden = 0;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("Correct dec selected", "ENTRY1", line1.DrawbackImportDeclarationNumber);
			AssertEquals("Correct dec selected", 1, line1.DrawbackImportDeclarationLine);
			AssertEquals("Method B CustomsValue", 10000m, line1.DrawbackCustomsValue);
			AssertEquals("Method B Duty Rate", 0m, line1.DrawbackDutyRate);
			AssertEquals("Method B Claim Duty Amount", 500m, line1.DrawbackDutyAmount);

			referenceDecEntryLine2.CL_DutyPercent = 7.5m;
			Factory.Save();
			line1.JI_CustomsQuantity = 20m;
			line1.AddInfo.ZA_DAM_Hidden = "";
			line1.AddInfo.ZA_DDN_Hidden = "";
			line1.AddInfo.ZA_DDL_Hidden = 0;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("Back to other one", "ENTRY2", line1.DrawbackImportDeclarationNumber);
			AssertEquals("Back to other one", 5, line1.DrawbackImportDeclarationLine);
			AssertEquals("Method B CustomsValue", 5000m, line1.DrawbackCustomsValue);
			AssertEquals("Method B Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Method B Claim Duty Amount", 200m, line1.DrawbackDutyAmount);

			JobDeclaration referenceDec2 = JobDeclaration.New(Factory);
			referenceDec2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			referenceDec2.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			referenceDec2.JE_OH_Importer = importer1.PK;
			referenceDec2.ManualClearanceDate = new ZDateTime(2006, 7, 1);
			CusEntryHeader referenceDec2EntryHeader = referenceDec2.CustomsEntryHeaders.AddNew();
			referenceDec2EntryHeader.EntryNumber = "ENTRY3";
			CusEntryLine referenceDec2EntryLine = referenceDec2EntryHeader.MergedLines.AddNew();
			referenceDec2EntryLine.CL_LineNumber = 2;
			referenceDec2EntryLine.CL_CustomsValue = 5000m;
			referenceDec2EntryLine.CL_DutyPercent = 0m;
			referenceDec2EntryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 200m);
			JobComInvoiceHeader referenceDec2InvoiceHeader = referenceDec2.Invoices.AddNew();
			referenceDec2InvoiceHeader.JZ_IncoTerm = "FOB";
			referenceDec2InvoiceHeader.JZ_InvoiceAmount = 15000m;
			referenceDec2InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine referenceDec2InvoiceLine = referenceDec2InvoiceHeader.JobComInvoiceLines.AddNew();

			referenceDec2InvoiceLine.JI_CL = referenceDec2EntryLine.PK;
			referenceDec2InvoiceLine.JI_PartNo = "PART1";
			referenceDec2InvoiceLine.JI_InvoiceQuantity = 20m;
			referenceDec2InvoiceLine.JI_InvoiceUQ = "NO";
			referenceDec2InvoiceLine.JI_CustomsQuantity = 20m;
			referenceDec2InvoiceLine.JI_CustomsUnitQty = "NO";
			Factory.Save();

			line1.AddInfo.ZA_DAM_Hidden = "";
			line1.AddInfo.ZA_DDN_Hidden = "";
			line1.AddInfo.ZA_DDL_Hidden = 0;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("ENTRY3", line1.DrawbackImportDeclarationNumber);
			AssertEquals(2, line1.DrawbackImportDeclarationLine);

			referenceDec2.ManualClearanceDate = new ZDateTime(2006, 6, 30);
			Factory.Save();

			line1.AddInfo.ZA_DAM_Hidden = "";
			line1.AddInfo.ZA_DDN_Hidden = "";
			line1.AddInfo.ZA_DDL_Hidden = 0;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("ENTRY2", line1.DrawbackImportDeclarationNumber);
			AssertEquals(5, line1.DrawbackImportDeclarationLine);

			referenceDec2.ManualClearanceDate = new ZDateTime(2007, 7, 1);
			Factory.Save();

			line1.AddInfo.ZA_DAM_Hidden = "";
			line1.AddInfo.ZA_DDN_Hidden = "";
			line1.AddInfo.ZA_DDL_Hidden = 0;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("ENTRY2", line1.DrawbackImportDeclarationNumber);
			AssertEquals(5, line1.DrawbackImportDeclarationLine);

			referenceDec2.ManualClearanceDate = new ZDateTime(2007, 1, 1);
			Factory.Save();

			line1.AddInfo.ZA_DAM_Hidden = "";
			line1.AddInfo.ZA_DDN_Hidden = "";
			line1.AddInfo.ZA_DDL_Hidden = 0;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("ENTRY3", line1.DrawbackImportDeclarationNumber);
			AssertEquals(2, line1.DrawbackImportDeclarationLine);

			referenceDec2InvoiceLine.JI_PartNo = "PART2";
			Factory.Save();

			line1.AddInfo.ZA_DAM_Hidden = "";
			line1.AddInfo.ZA_DDN_Hidden = "";
			line1.AddInfo.ZA_DDL_Hidden = 0;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("ENTRY2", line1.DrawbackImportDeclarationNumber);
			AssertEquals(5, line1.DrawbackImportDeclarationLine);

			referenceDec2InvoiceLine.JI_PartNo = "PART1";
			Factory.Save();

			line1.AddInfo.ZA_DAM_Hidden = "";
			line1.AddInfo.ZA_DDN_Hidden = "";
			line1.AddInfo.ZA_DDL_Hidden = 0;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("ENTRY3", line1.DrawbackImportDeclarationNumber);
			AssertEquals(2, line1.DrawbackImportDeclarationLine);

			referenceDec2.JE_OH_Importer = importer2.PK;
			Factory.Save();

			line1.AddInfo.ZA_DAM_Hidden = "";
			line1.AddInfo.ZA_DDN_Hidden = "";
			line1.AddInfo.ZA_DDL_Hidden = 0;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("ENTRY2", line1.DrawbackImportDeclarationNumber);
			AssertEquals(5, line1.DrawbackImportDeclarationLine);
		}

		public void TestDrawbackMethodCCalc()
		{
			invoice1.JZ_InvoiceDate = new ZDateTime(2005, 4, 1);
			line1.JI_CustomsUnitQty = "NO";
			line1.JI_CustomsQuantity = 10m;
			line1.JI_LinePrice = 10000m;
			line1.JI_Tariff = "8544.41.90 23";
			line1.AddInfo.ZA_PST = "GEN";
			line1.AddInfo.ZA_RNO = "001";
			invoice1.NotifyEffectiveDutyDateDirty();
			line1.AddInfo.ZA_DAM_Hidden = "C";
			AssertEquals("Method C CustomsValue", 3000m, line1.DrawbackCustomsValue);
			AssertEquals("Method C Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Method C Claim Duty Amount", 150m, line1.DrawbackDutyAmount);
		}

		public void TestDrawbackMethodACalcWithMergedEntry()
		{
			line1.AddInfo.ZA_DDN_Hidden = "ENTRY4";
			line1.AddInfo.ZA_DDL_Hidden = 1;
			line1.JI_PartNo = "PART3";
			line1.JI_CustomsUnitQty = "KG";
			line1.JI_CustomsQuantity = 2m;
			line1.AddInfo.ZA_DAM_Hidden = "A";
			AssertEquals("Method A CustomsValue", 400m, line1.DrawbackCustomsValue);
			AssertEquals("Method A Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Method A Claim Duty Amount", 20m, line1.DrawbackDutyAmount);

			line1.AddInfo.ZA_DDN_Hidden = "ENTRY4";
			line1.AddInfo.ZA_DDL_Hidden = 1;
			line1.JI_PartNo = "PART4";
			line1.JI_CustomsUnitQty = "KG";
			line1.JI_CustomsQuantity = 2m;
			line1.AddInfo.ZA_DAM_Hidden = "A";
			AssertEquals("Method A CustomsValue", 100m, line1.DrawbackCustomsValue);
			AssertEquals("Method A Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Method A Claim Duty Amount", 5m, line1.DrawbackDutyAmount);
		}

		public void TestDrawbackMethodBCalcWithMergedEntry()
		{
			line1.AddInfo.ZA_DDN_Hidden = "ENTRY4";
			line1.AddInfo.ZA_DDL_Hidden = 1;
			line1.JI_PartNo = "PART3";
			line1.JI_CustomsUnitQty = "KG";
			line1.JI_CustomsQuantity = 2m;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("Method A CustomsValue", 400m, line1.DrawbackCustomsValue);
			AssertEquals("Method A Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Method A Claim Duty Amount", 20m, line1.DrawbackDutyAmount);

			line1.AddInfo.ZA_DDN_Hidden = "ENTRY4";
			line1.AddInfo.ZA_DDL_Hidden = 1;
			line1.JI_PartNo = "PART4";
			line1.JI_CustomsUnitQty = "KG";
			line1.JI_CustomsQuantity = 2m;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("Method A CustomsValue", 100m, line1.DrawbackCustomsValue);
			AssertEquals("Method A Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Method A Claim Duty Amount", 5m, line1.DrawbackDutyAmount);
		}

		public void TestDrawbackMethodBAverageCalcWithMergedEntry()
		{
			StmNote drawbackNote = Factory.New<StmNote>();
			drawbackNote.ST_ParentID = line1.PK;
			drawbackNote.ST_Table = "JOBCOMINVOICELINE";
			drawbackNote.ST_Description = "DrawbackEntryLines";
			drawbackNote.ST_NoteType = nameof(CargoWise.Definitions.StmNoteVisibility.DOC);
			drawbackNote.ST_NoteDataAsText = "ENTRY4*1*4";
			line1.DrawbackCusEntryLineCollection.LoadFromNote();
			line1.JI_PartNo = "PART3";
			line1.JI_CustomsUnitQty = "KG";
			line1.JI_CustomsQuantity = 2;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("Method B CustomsValue", 400m, line1.DrawbackCustomsValue);
			AssertEquals("Method B Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Method B Claim Duty Amount", 20m, line1.DrawbackDutyAmount);
		}

		public void TestDrawbackMethodBLowestValueWithMergedEntry()
		{
			invoice1.JZ_InvoiceDate = new ZDateTime(2007, 8, 1);
			line1.JI_PartNo = "PART3";
			line1.JI_CustomsUnitQty = "KG";
			line1.JI_CustomsQuantity = 2m;
			line1.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("Correct dec selected", "ENTRY4", line1.DrawbackImportDeclarationNumber);
			AssertEquals("Correct dec selected", 1, line1.DrawbackImportDeclarationLine);
			AssertEquals("Method B CustomsValue", 400m, line1.DrawbackCustomsValue);
			AssertEquals("Method B Duty Rate", 5m, line1.DrawbackDutyRate);
			AssertEquals("Method B Claim Duty Amount", 20m, line1.DrawbackDutyAmount);
		}

		#region Implementation
		JobDeclaration testDec;
		JobComInvoiceHeader invoice1;
		JobComInvoiceLine line1;
		JobDeclaration referenceDec;
		CusEntryHeader referenceDecEntryHeaderN20;
		CusEntryHeader referenceDecEntryHeader1;
		CusEntryHeader referenceDecEntryHeader2;
		CusEntryLine referenceDecEntryLineN20;
		CusEntryLine referenceDecEntryLine1;
		CusEntryLine referenceDecEntryLine2;
		OrgHeader importer1;
		OrgHeader importer2;
		JobComInvoiceLine mergedDecLine1;
		JobComInvoiceLine mergedDecLine2;
		JobComInvoiceLine mergedDecLine3;
		protected override void SetUp()
		{
			base.SetUp();
			importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "ORG1";
			importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "ORG2";
			AUOrgSupplierPart part1 = AUOrgSupplierPart.New(Factory);
			part1.OP_PartNum = "PART1";
			part1.OP_Desc = "PART1";
			part1.OP_StockKeepingUnit = "NO";
			OrgPartRelation relation1 = part1.RelatedOrganisations.AddNew();
			relation1.OU_OH = importer1.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AUOrgSupplierPart part2 = AUOrgSupplierPart.New(Factory);
			part2.OP_PartNum = "PART2";
			part2.OP_Desc = "PART2";
			part2.OP_StockKeepingUnit = "NO";
			OrgPartRelation relation2 = part2.RelatedOrganisations.AddNew();
			relation2.OU_OH = importer1.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_OH_Importer = importer1.PK;
			invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_PartNo = "PART1";

			referenceDec = JobDeclaration.New(Factory);
			referenceDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			referenceDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			referenceDec.JE_OH_Importer = importer1.PK;

			referenceDecEntryHeaderN20 = referenceDec.CustomsEntryHeaders.AddNew();
			referenceDecEntryHeaderN20.EntryNumber = "N20";
			referenceDecEntryLineN20 = referenceDecEntryHeaderN20.MergedLines.AddNew();
			referenceDecEntryLineN20.CL_LineNumber = 18;
			referenceDecEntryLineN20.CL_CustomsValue = 2500m;
			referenceDecEntryLineN20.CL_DutyPercent = 5m;
			referenceDecEntryLineN20.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 100m);
			var referenceDecInvoiceHeaderN20 = referenceDec.Invoices.AddNew();
			referenceDecInvoiceHeaderN20.JZ_IncoTerm = "FOB";
			referenceDecInvoiceHeaderN20.JZ_InvoiceAmount = 7500m;
			referenceDecInvoiceHeaderN20.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var referenceDecInvoiceLineN20 = referenceDecInvoiceHeaderN20.JobComInvoiceLines.AddNew();
			referenceDecInvoiceLineN20.JI_CL = referenceDecEntryLineN20.PK;
			referenceDecInvoiceLineN20.JI_PartNo = "PART1";
			referenceDecInvoiceLineN20.JI_InvoiceQuantity = 20m;
			referenceDecInvoiceLineN20.JI_InvoiceUQ = "NO";
			referenceDecInvoiceLineN20.JI_CustomsQuantity = 20m;
			referenceDecInvoiceLineN20.JI_CustomsUnitQty = "NO";
			referenceDecInvoiceLineN20.JI_IsPackToBondForLine = true;

			referenceDecEntryHeader1 = referenceDec.CustomsEntryHeaders.AddNew();
			referenceDecEntryHeader1.EntryNumber = "ENTRY2";
			referenceDecEntryLine1 = referenceDecEntryHeader1.MergedLines.AddNew();
			referenceDecEntryLine1.CL_LineNumber = 5;
			referenceDecEntryLine1.CL_CustomsValue = 5000m;
			referenceDecEntryLine1.CL_DutyPercent = 5m;
			referenceDecEntryLine1.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 200m);
			JobComInvoiceHeader referenceDecInvoiceHeader = referenceDec.Invoices.AddNew();
			referenceDecInvoiceHeader.JZ_IncoTerm = "FOB";
			referenceDecInvoiceHeader.JZ_InvoiceAmount = 15000m;
			referenceDecInvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine referenceDecEntryLine1InvoiceLine = referenceDecInvoiceHeader.JobComInvoiceLines.AddNew();
			referenceDecEntryLine1InvoiceLine.JI_CL = referenceDecEntryLine1.PK;
			referenceDecEntryLine1InvoiceLine.JI_PartNo = "PART1";
			referenceDecEntryLine1InvoiceLine.JI_InvoiceQuantity = 20m;
			referenceDecEntryLine1InvoiceLine.JI_InvoiceUQ = "NO";
			referenceDecEntryLine1InvoiceLine.JI_CustomsQuantity = 20m;
			referenceDecEntryLine1InvoiceLine.JI_CustomsUnitQty = "NO";
			referenceDecEntryHeader2 = referenceDec.CustomsEntryHeaders.AddNew();
			referenceDecEntryHeader2.EntryNumber = "ENTRY1";
			referenceDecEntryLine2 = referenceDecEntryHeader2.MergedLines.AddNew();
			referenceDecEntryLine2.CL_LineNumber = 1;
			referenceDecEntryLine2.CL_CustomsValue = 10000m;
			referenceDecEntryLine2.CL_DutyPercent = 5m;
			referenceDecEntryLine2.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 500m);
			JobComInvoiceLine referenceDecEntryLine2InvoiceLine = referenceDecInvoiceHeader.JobComInvoiceLines.AddNew();
			referenceDecEntryLine2InvoiceLine.JI_CL = referenceDecEntryLine2.PK;
			referenceDecEntryLine2InvoiceLine.JI_PartNo = "PART1";
			referenceDecEntryLine2InvoiceLine.JI_InvoiceQuantity = 30m;
			referenceDecEntryLine2InvoiceLine.JI_InvoiceUQ = "NO";
			referenceDecEntryLine2InvoiceLine.JI_CustomsQuantity = 30m;
			referenceDecEntryLine2InvoiceLine.JI_CustomsUnitQty = "NO";

			AUOrgSupplierPart part3 = AUOrgSupplierPart.New(Factory);
			part3.OP_PartNum = "PART3";
			part3.OP_Desc = "PART3";
			part3.OP_StockKeepingUnit = "KG";
			OrgPartRelation relation3 = part3.RelatedOrganisations.AddNew();
			relation3.OU_OH = importer1.PK;
			relation3.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AUOrgSupplierPart part4 = AUOrgSupplierPart.New(Factory);
			part4.OP_PartNum = "PART4";
			part4.OP_Desc = "PART4";
			part4.OP_StockKeepingUnit = "KG";
			OrgPartRelation relation4 = part4.RelatedOrganisations.AddNew();
			relation4.OU_OH = importer1.PK;
			relation4.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			JobDeclaration mergedDec = JobDeclaration.New(Factory);
			mergedDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			mergedDec.JE_ExportDate = new ZDateTime(2007, 7, 1);
			mergedDec.ManualClearanceDate = new ZDateTime(2007, 7, 5);
			mergedDec.JE_OH_Importer = importer1.PK;
			mergedDec.JE_MergeBy = "TRF";

			JobComInvoiceHeader mergedDecInvoice = mergedDec.Invoices.AddNew();
			mergedDecInvoice.JZ_InvoiceAmount = 3000m;
			mergedDecInvoice.JZ_RX_NKInvoice_Currency = "AUD";
			mergedDecInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			mergedDecLine1 = mergedDecInvoice.JobComInvoiceLines.AddNew();
			mergedDecLine1.JI_PartNo = "PART3";
			mergedDecLine1.JI_Tariff = "1704.90.00 44";
			mergedDecLine1.JI_LinePrice = 1200m;
			mergedDecLine1.JI_CountryOfOrigin = "CA";
			mergedDecLine1.JI_InvoiceUQ = Enterprise.Core.Constants.Area.SquareCentimetre;
			mergedDecLine1.JI_InvoiceQuantity = 3000;
			mergedDecLine1.JI_CustomsQuantity = 6m;

			mergedDecLine2 = mergedDecInvoice.JobComInvoiceLines.AddNew();
			mergedDecLine2.JI_PartNo = "PART4";
			mergedDecLine2.JI_Tariff = "1704.90.00 44";
			mergedDecLine2.JI_LinePrice = 1000m;
			mergedDecLine2.JI_InvoiceUQ = Enterprise.Core.Constants.Area.SquareCentimetre;
			mergedDecLine2.JI_InvoiceQuantity = 10000;
			mergedDecLine2.JI_CustomsQuantity = 20m;
			mergedDecLine2.JI_CountryOfOrigin = "CA";

			mergedDecLine3 = mergedDecInvoice.JobComInvoiceLines.AddNew();
			mergedDecLine3.JI_PartNo = "PART3";
			mergedDecLine3.JI_Tariff = "1704.90.00 44";
			mergedDecLine3.JI_LinePrice = 800m;
			mergedDecLine3.JI_CountryOfOrigin = "CA";
			mergedDecLine3.JI_InvoiceUQ = Enterprise.Core.Constants.Area.SquareCentimetre;
			mergedDecLine3.JI_InvoiceQuantity = 2000;
			mergedDecLine3.JI_CustomsQuantity = 4m;

			LineMerger merger = new LineMerger(mergedDec);
			merger.DoMerge();

			AssertEquals(1, mergedDec.CustomsEntryHeaders.Count);
			AssertEquals(1, mergedDec.CustomsEntryHeaders[0].AllEntryLines.Count);
			AssertEquals(150m, mergedDec.CustomsEntryHeaders[0].AllEntryLines[0].DutyAmount);
			mergedDec.CustomsEntryHeaders[0].EntryNumber = "ENTRY4";

			Factory.Save();
		}

		//protected override void TearDown()
		//{
		//	base.TearDown();
		//	useCustomsReferenceDataRegItem?.Dispose();
		//}

		//IDisposable useCustomsReferenceDataRegItem;

		OrgHeader fConsignee;
		protected OrgHeader Consignee
		{
			get
			{
				if (fConsignee == null)
				{
					fConsignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
				}
				return fConsignee;
			}
		}
		#endregion
	}
}
