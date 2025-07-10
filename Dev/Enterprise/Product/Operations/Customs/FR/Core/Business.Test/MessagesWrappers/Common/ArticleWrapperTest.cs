using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.FR.Business.Declaration.Testing.CusEntryInstructionTest;
using CusEntryInstruction = Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class ArticleWrapperTest : TestCaseWithFactory
	{
		public void TestSecondMessageSupportingDocumentsInCaseOfMultipleEntriesSent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			var cei2 = declaration.CustomsEntryInstructions.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2203001010";
			invoiceLine1.JI_CEI = cei1.PK;

			var supportingDocument1 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "DOC1";
			supportingDocument1.CSI_ReferenceNumber = "AAAAAAAA";
			supportingDocument1.CSI_DateOfIssue = new ZDateTime(2022, 01, 01);

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8400102030";
			invoiceLine2.JI_CEI = cei2.PK;

			var supportingDocument2_1 = invoiceLine2.SupportingDocuments.AddNew();
			supportingDocument2_1.CSI_Code = "DOC21";
			supportingDocument2_1.CSI_ReferenceNumber = "BBBBBBBB";
			supportingDocument2_1.CSI_DateOfIssue = new ZDateTime(2022, 01, 01);

			var supportingDocument2_2 = invoiceLine2.SupportingDocuments.AddNew();
			supportingDocument2_2.CSI_Code = "DOC22";
			supportingDocument2_2.CSI_ReferenceNumber = "CCCCCCC";
			supportingDocument2_2.CSI_DateOfIssue = new ZDateTime(2022, 01, 01);

			var merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();

			var entryHeader1 = declaration.CustomsEntryHeaders.First(x => x.InvoiceLines.Any(l => l.JI_Tariff == "2203001010"));
			var snapshot = entryHeader1.Snapshots.AddNew();
			snapshot.CES_MessageType = EntryActionCodeList.Codes.VAL;
			snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
			snapshot.CES_SnapshotXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<FrenchEntryLineChildSnapshot xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
<CusEntryLine>
<LineNumber>1</LineNumber>
<ChildData Type=""DOC"">
<Code>DOC1</Code>
<Reference>AAAAAAAA</Reference>
</ChildData>
</CusEntryLine>
</FrenchEntryLineChildSnapshot>";

			var entryHeader2 = declaration.CustomsEntryHeaders.First(x => x.InvoiceLines.Any(l => l.JI_Tariff == "8400102030"));
			var snapshot2 = entryHeader2.Snapshots.AddNew();
			snapshot2.CES_MessageType = EntryActionCodeList.Codes.VAL;
			snapshot2.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
			snapshot2.CES_SnapshotXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<FrenchEntryLineChildSnapshot xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
<CusEntryLine>
<LineNumber>1</LineNumber>
<ChildData Type=""DOC"">
<Code>DOC21</Code>
<Reference>BBBBBBBB</Reference>
</ChildData>
</CusEntryLine>
</FrenchEntryLineChildSnapshot>";

			errorCollector = new ErrorCollector();

			var entryLine1 = entryHeader1.MergedLines[0];
			var articleWrapper1 = new ArticleWrapper(entryHeader1, entryLine1);

			var entryLine2 = entryHeader2.MergedLines[0];
			var articleWrapper2 = new ArticleWrapper(entryHeader2, entryLine2);

			var invoiceHeaderDoc1 = articleWrapper2.SecondMessageSupportingDocuments.Cast<DocumentWrapper>().Where(x => x.Code == "DOC21" && x.RefNumber == "BBBBBBBB");
			AssertEquals("DOC21 document should not be written out in second message because it showed in second entry header snapshot.", 0, invoiceHeaderDoc1.Count());

			var invoiceHeaderDoc2 = articleWrapper2.SecondMessageSupportingDocuments.Cast<DocumentWrapper>().Where(x => x.Code == "DOC22" && x.RefNumber == "CCCCCCC");
			AssertEquals("DOC22 document should have been written out in second message because it didn't show in second entry header snapshot.", 1, invoiceHeaderDoc2.Count());
		}

		public void TestIsPlacingGoodsUnderBW()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;

			var entryHeader = (Declaration.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = (Declaration.CusEntryLine)entryHeader.AllEntryLines.AddNew();
			var invoiceline = entryLine.InvoiceLines.AddNew();

			var wrapper = new ArticleWrapper(entryHeader, entryLine);

			CombineAssertions(() =>
			{
				AssertEquals("No linked instruction", false, wrapper.IsPlacingGoodsUnderBW);

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = DeltaGImportDeclarationTypeList.Codes.PlacingGoodsUnderBW;
				entryHeader.CH_CEI_Instruction = instruction.PK;
				invoiceline.JI_CEI = instruction.PK;
				wrapper = new ArticleWrapper(entryHeader, entryLine);
				AssertEquals("71P", true, wrapper.IsPlacingGoodsUnderBW);

				instruction.CEI_Style = DeltaGImportDeclarationTypeList.Codes.EntryForHomeUseOfUnionGoods;
				AssertEquals("Not 71P", false, wrapper.IsPlacingGoodsUnderBW);
			});
		}

		public void TestShippingIdReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var entryHeader = (Declaration.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_ParentID = declaration.PK;
			entryNum.CE_EntryNum = "RCANumber";
			entryNum.CE_ParentTable = entryHeader.TableName;
			entryNum.CE_RN_NKCountryCode = "FR";
			entryNum.CE_EntryType = FranceAdditionalReferenceNumberTypes.Codes.CommonAccessReference;
			Factory.Save();

			var entryLine = (Declaration.CusEntryLine)entryHeader.AllEntryLines.AddNew();
			var invoiceLine = entryLine.InvoiceLines.AddNew();

			var errorCollector = new ErrorCollector();
			var wrapper = new ArticleWrapper(entryHeader, entryLine);

			AssertEquals("RCANumber", wrapper.ShippingIdReference);
		}

		public void TestHasSpecificRegime()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_Style = DeltaGImportDeclarationTypeList.Codes.ImportationForInwardProcessing;

			var entryHeader = (Declaration.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = (Declaration.CusEntryLine)entryHeader.AllEntryLines.AddNew();
			var invoiceLine = entryLine.InvoiceLines.AddNew();

			var errorCollector = new ErrorCollector();
			var wrapper = new ArticleWrapper(entryHeader, entryLine);
			AssertNull(instruction.SpecificRegimeAuthorisation);
			AssertEquals(false, wrapper.HasSpecificRegimeAuthorisation);
			AssertNull(wrapper.EcoRegimeDatas);
			AssertNull(wrapper.EcoRegimeAuthorization);

			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			usage.AGC_OH_Owner = importer.PK;
			usage.AGC_Number = "12345678";

			var authorisationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationHeader.CPH_Number = "12345678";
			authorisationHeader.CPH_IsSingleUse = true;
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			authorisationHeader.CPH_OH_PermitHolder = importer.PK;
			authorisationHeader.CPH_RN_NKCountryCode = "FR";

			AssertNotNull(instruction.SpecificRegimeAuthorisation);
			Assert(wrapper.HasSpecificRegimeAuthorisation);
			AssertNotNull(wrapper.EcoRegimeDatas);
			AssertNotNull(wrapper.EcoRegimeAuthorization);
		}

		public void TestEcoRegimeAuthorizationWhenEntryInstructionIsNull()
		{
			var declaration = Factory.New<JobDeclaration>();

			var warehouseOrg = Factory.NewWithValidTestData<OrgHeader>();
			warehouseOrg.SetupAccount(OrgCusAccountCodeList.Codes.DGE, ZString.Empty, "EXPACCOUNT", ZString.Empty, ZString.Empty, "9DD94F85");

			declaration.WarehouseDocAddress.OrganisationPK = warehouseOrg.PK;
			var entryHeader = (Declaration.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = (Declaration.CusEntryLine)entryHeader.AllEntryLines.AddNew();
			var invoiceLine = entryLine.InvoiceLines.AddNew();
			var errorCollector = new ErrorCollector();
			var wrapper = new ArticleWrapper(entryHeader, entryLine);

			AssertNull("EcoRegimeAuthorization should be null", wrapper.EcoRegimeAuthorization);

			var cei = Factory.New<CusEntryInstruction>();
			cei.CEI_Style = DeltaGImportDeclarationTypeList.Codes.ImportationForInwardProcessing;
			entryHeader.CH_CEI_Instruction = cei.PK;

			AssertNull("EcoRegimeAuthorization should be null because an entry instruction has been set up against the entry header but no authorization matching its style has been setup.", wrapper.EcoRegimeAuthorization);

			var usage = cei.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			usage.AGC_OH_Owner = warehouseOrg.PK;
			usage.AGC_Number = "12345678";

			var authorisationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationHeader.CPH_Number = "12345678";
			authorisationHeader.CPH_IsSingleUse = true;
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			authorisationHeader.CPH_OH_PermitHolder = warehouseOrg.PK;
			authorisationHeader.CPH_RN_NKCountryCode = "FR";

			AssertNotNull("EcoRegimeAuthorization should not be null because an entry instruction has been set up against the entry header AND an authorization matching its style has been setup.", wrapper.EcoRegimeAuthorization);
		}

		public void TestEcoRegimeAuthorizationDependingTEETypeOfAuthorization()
		{
			var declaration = Factory.New<JobDeclaration>();

			var warehouseOrg = Factory.NewWithValidTestData<OrgHeader>();
			warehouseOrg.SetupAccount(OrgCusAccountCodeList.Codes.DGE, ZString.Empty, "EXPACCOUNT", ZString.Empty, ZString.Empty, "9DD94F85");

			declaration.WarehouseDocAddress.OrganisationPK = warehouseOrg.PK;
			var entryHeader = (Declaration.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = (Declaration.CusEntryLine)entryHeader.AllEntryLines.AddNew();
			var invoiceLine = entryLine.InvoiceLines.AddNew();
			var errorCollector = new ErrorCollector();
			var wrapper = new ArticleWrapper(entryHeader, entryLine);

			var cei = Factory.New<CusEntryInstruction>();
			cei.CEI_Style = DeltaGExportDeclarationTypeList.Codes.TemporaryExportWithEI;
			entryHeader.CH_CEI_Instruction = cei.PK;

			var usage = cei.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.TemporaryExportation;
			usage.AGC_OH_Owner = warehouseOrg.PK;
			usage.AGC_Number = "12345678";

			var authorisationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationHeader.CPH_Number = "12345678";
			authorisationHeader.CPH_IsSingleUse = true;
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryExportation;
			authorisationHeader.CPH_OH_PermitHolder = warehouseOrg.PK;
			authorisationHeader.CPH_RN_NKCountryCode = "FR";

			AssertNotNull("EcoRegimeAuthorization should not be null because an entry instruction has been set up against the entry header AND an authorization matching its style has been setup.", wrapper.EcoRegimeAuthorization);
			AssertEquals("EcoRegimeAuthorizationNumber", ZString.Empty, wrapper.EcoRegimeAuthorization.EcoRegimeAuthorizationNumber);
			AssertEquals("EcoRegimeCountryCode", ZString.Empty, wrapper.EcoRegimeAuthorization.EcoRegimeCountryCode);
		}

		ArticleWrapper CreateWrapperForSecondMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203001010";

			var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "DOC1";
			supportingDocument1.CSI_ReferenceNumber = "AAAAAAAA";
			supportingDocument1.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);

			var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "DOC2";
			supportingDocument2.CSI_ReferenceNumber = "BBBBBBBB";

			var additionalCode1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			additionalCode1.CY_Code = "V910";

			var additionalCode2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			additionalCode2.CY_Code = "V911";

			invoiceLine.JI_SupplementaryCode1 = "B001";
			invoiceLine.JI_SupplementaryCode2 = "B002";

			var dTP1 = invoiceLine.SupportingDocuments.AddNew();
			dTP1.FillWithValidTestData();
			dTP1.CSI_Code = "DTP1";
			dTP1.CSI_IsDTP = true;

			var dTP2 = invoiceLine.SupportingDocuments.AddNew();
			dTP2.FillWithValidTestData();
			dTP2.CSI_Code = "DTP2";
			dTP2.CSI_IsDTP = true;

			var merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var snapshot = entryHeader.Snapshots.AddNew();
			snapshot.CES_MessageType = EntryActionCodeList.Codes.VAL;
			snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
			snapshot.CES_SnapshotXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<FrenchEntryLineChildSnapshot xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
<CusEntryLine>
<LineNumber>1</LineNumber>
<ChildData Type=""CAN"">
<Code>V910</Code>
</ChildData>
<ChildData Type=""CAC"">
<Code>B001</Code>
</ChildData>
<ChildData Type=""DOC"">
<Code>DOC1</Code>
<Reference>AAAAAAAA</Reference>
</ChildData>
<ChildData Type=""DOC"">
<Code>DTP1</Code>
</ChildData>
</CusEntryLine>
</FrenchEntryLineChildSnapshot>";
			var entryLine = entryHeader.MergedLines[0];

			errorCollector = new ErrorCollector();

			var articleWrapper = new ArticleWrapper(entryHeader, entryLine);

			return articleWrapper;
		}

		public void TestSecondMessageSupportingDocuments()
		{
			var wrapper = CreateWrapperForSecondMessage();

			var invoiceLineDoc = wrapper.SecondMessageSupportingDocuments.Cast<DocumentWrapper>().Where(x => x.Code == "DOC1" && x.RefNumber == "AAAAAAAA");
			AssertEquals("DOC1 document should be excluded from second message because it shows in snapshot.", 0, invoiceLineDoc.Count());

			var invoiceHeaderDoc = wrapper.SecondMessageSupportingDocuments.Cast<DocumentWrapper>().Where(x => x.Code == "DOC2" && x.RefNumber == "BBBBBBBB");
			AssertEquals("DOC2 document should be written out in second message because it doesn't show in snapshot.", 1, invoiceHeaderDoc.Count());
		}

		public void TestSecondMessageCETariffAdditionalCodes()
		{
			var wrapper = CreateWrapperForSecondMessage();

			var sup1AdditionalCodes = wrapper.SecondMessageCETariffAdditionalCodes.Cast<ITariffAdditionalCode>().Where(x => x.Code == "B001");
			AssertEquals("B001 code should be excluded from second message because it shows in snapshot.", 0, sup1AdditionalCodes.Count());
			var sup2AdditionalCodes = wrapper.SecondMessageCETariffAdditionalCodes.Cast<ITariffAdditionalCode>().Where(x => x.Code == "B002");
			AssertEquals("B002 code should be written out in second message because it doesn't show in snapshot.", 1, sup2AdditionalCodes.Count());
		}

		public void TestSecondMessageFRTariffAdditionalCodes()
		{
			var wrapper = CreateWrapperForSecondMessage();

			var add1NationalCodes = wrapper.SecondMessageFRTariffAdditionalCodes.Cast<ITariffAdditionalCode>().Where(x => x.Code == "V910");
			AssertEquals("V901 code should be excluded from second message because it shows in snapshot.", 0, add1NationalCodes.Count());
			var add2NationalCodes = wrapper.SecondMessageFRTariffAdditionalCodes.Cast<ITariffAdditionalCode>().Where(x => x.Code == "V911");
			AssertEquals("V902 code should be written out in second message because it doesn't show in snapshot.", 1, add2NationalCodes.Count());
		}

		public void TestSecondMessagePartDispos()
		{
			var wrapper = CreateWrapperForSecondMessage();

			var dtp1Codes = wrapper.SecondMessagePartDispos.Cast<ITariffAdditionalCode>().Where(x => x.Code == "DTP1");
			AssertEquals("DTP1 code should be excluded from second message because it shows in snapshot.", 0, dtp1Codes.Count());
			var dtp2Codes = wrapper.SecondMessagePartDispos.Cast<ITariffAdditionalCode>().Where(x => x.Code == "DTP2");
			AssertEquals("DTP2 code should be written out in second message because it doesn't show in snapshot.", 1, dtp2Codes.Count());
		}

		public void TestGetSupportingDocumentsReturnsNoDuplicates()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2700", isImport: true, hasPermitAttribute: true));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203001010";

			var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "2700";
			supportingDocument1.CSI_ReferenceNumber = "AAAAAAAA";
			supportingDocument1.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			supportingDocument1.CSI_Description = "APPLE";
			supportingDocument1.CSI_LineNo = 1;

			var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "2700";
			supportingDocument2.CSI_ReferenceNumber = "AAAAAAAA";
			supportingDocument2.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			supportingDocument1.CSI_Description = "ORANGES";
			supportingDocument2.CSI_LineNo = 2;

			var supportingDocument3 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = "BBBB";
			supportingDocument3.CSI_ReferenceNumber = "BBBBBBBB";
			supportingDocument3.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			supportingDocument3.CSI_LineNo = 3;

			var supportingDocument4 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument4.CSI_Code = "BBBB";
			supportingDocument4.CSI_ReferenceNumber = "BBBBBBBB";
			supportingDocument4.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			supportingDocument4.CSI_LineNo = 4;

			var merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			var entryLine = entryHeader.MergedLines[0];

			errorCollector = new ErrorCollector();

			var articleWrapper = new ArticleWrapper(entryHeader, entryLine);

			AssertEquals("Documents with code 2700 didn't automatically merge because their description differ.", 3, entryLine.SupportingDocuments.Count());
			AssertEquals("Documents sent to Customs should have permits merged using code reference and date.", 2, entryLine.SupportingDocumentsToCustoms.Count());

			var supportingDocumentList = articleWrapper.SupportingDocuments.ToList();
			AssertNotNull(supportingDocumentList);
			AssertEquals(2, supportingDocumentList.Count);
		}

		public void TestCustomsStatisticalAndVatValue()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			var cei = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 50;
			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine.JI_Tariff = "2203001010";
			invoiceLine.JI_LinePrice = 50;
			invoiceLine.JI_CEI = cei.PK;

			var statableCharge = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, 5.10m, Enterprise.Core.Constants.CurrencyCodes.France);
			statableCharge.J7_IsIncludedInITOT = false;
			statableCharge.J7_IsDutiable = false;
			statableCharge.J7_IsStatisticalValueApplicable = true;
			statableCharge.J7_IsGSTApplicable = false;

			var dutiableCharge = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, 17.66m, Enterprise.Core.Constants.CurrencyCodes.France);
			dutiableCharge.J7_IsIncludedInITOT = false;
			dutiableCharge.J7_IsDutiable = true;
			dutiableCharge.J7_IsStatisticalValueApplicable = false;
			dutiableCharge.J7_IsGSTApplicable = false;

			var vatableCharge = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, 33.42m, Enterprise.Core.Constants.CurrencyCodes.France);
			vatableCharge.J7_IsIncludedInITOT = false;
			vatableCharge.J7_IsDutiable = false;
			vatableCharge.J7_IsStatisticalValueApplicable = false;
			vatableCharge.J7_IsGSTApplicable = true;

			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge(shutUp);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = (Declaration.CusEntryLine)entryHeader.AllEntryLines[0];
			var articleWrapper = new ArticleWrapper(entryHeader, entryLine);

			AssertEquals(55.10m, articleWrapper.StatisticalAmount);
			AssertEquals(67.66m, articleWrapper.CustomsValue);
			AssertEquals(83.42m, articleWrapper.TVAAssessedAmount);
		}

		public void TestShouldSendCustomsStatisticAndVatValues()
		{
			var declaration = cusEntryHeader.Declaration;
			declaration.JE_MessageType = "EXP";
			var articleWrapper = new ArticleWrapper(cusEntryHeader, cusFrEntryLine);

			AssertEquals(true, articleWrapper.ShouldSendCustomsStatisticAndVatValues);

			declaration.JE_MessageType = "IMP";
			AssertEquals(false, articleWrapper.ShouldSendCustomsStatisticAndVatValues);

			cusEntryHeader.EntryInstruction.ZG_BypassCode = ValuationBypassCodeList.Codes.VBC_A;
			AssertEquals(true, articleWrapper.ShouldSendCustomsStatisticAndVatValues);

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			AssertEquals(false, articleWrapper.ShouldSendCustomsStatisticAndVatValues);

			declaration.JE_MessageType = "EXP";
			AssertEquals(true, articleWrapper.ShouldSendCustomsStatisticAndVatValues);
		}

		public void TestEntryLineCharges()
		{
			var declaration = cusEntryHeader.Declaration;
			var invoiceLine = declaration.FilteredInvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault();

			AssertEquals(0m, articleFrWrapper.Commission.Amount);
			AssertEquals(0m, articleFrWrapper.OthCosts.Amount);
			AssertEquals(0m, articleFrWrapper.PackingCosts.Amount);
			AssertEquals(0m, articleFrWrapper.Fee.Amount);
			AssertEquals(0m, articleFrWrapper.Resale.Amount);
			AssertEquals(0m, articleFrWrapper.CustomsCosts.Amount);
			AssertEquals(0m, articleFrWrapper.AssemblyCosts.Amount);

			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge, 1m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, true);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge, 2m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, true);
			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);

			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.AdjustmentCharge, 10m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, false, false, true);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.AdjustmentCharge, 20m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, false, false, true);
			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.AdjustmentCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, true, true, false);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.AdjustmentCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, true, true, false);

			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, 100m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, true);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, 200m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, true);
			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);

			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 1000m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, true);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 2000m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, true);
			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);

			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, 10000m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, true);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, 20000m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, true);
			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);

			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, 100000m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, 200000m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);
			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, false);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, false);

			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, 1000000m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, true);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, 2000000m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, true);
			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, false);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, false);

			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, 10000000m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, true);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, 20000000m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, true);
			CreateLineCharge(invoiceLine, false, UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);
			CreateLineCharge(invoiceLine, true, UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);

			declaration.Factory.Save();

			AssertEquals(3m, articleFrWrapper.Commission.Amount);
			AssertEquals(10m, articleFrWrapper.OthCosts.Amount);
			AssertEquals(300m, articleFrWrapper.PackingCosts.Amount);
			AssertEquals(3000m, articleFrWrapper.Fee.Amount);
			AssertEquals(30000m, articleFrWrapper.Resale.Amount);
			AssertEquals(300000m, articleFrWrapper.CustomsCosts.Amount);
			AssertEquals(1000000m, articleFrWrapper.AssemblyCosts.Amount);
		}

		void CreateLineCharge(JobComInvoiceLine invoiceLine, bool isApportioned, ZString chargeCode, ZDecimal amount, ZString currency, bool isIncludedInInvoice, bool isIncludedInInvoiceLine, bool isDutiable, bool isStatable, bool isVatable)
		{
			if (isApportioned)
			{
				var charge = invoiceLine.ApportionedCharges.AddNew(chargeCode, amount, currency);
				charge.J7_ChargeType = chargeCode;
				charge.J7_Amount = amount;
				charge.J7_RX_NKCurrency = currency;
				charge.J7_IsIncludedInITOT = isIncludedInInvoice;
				charge.J7_IsNotIncludedInInvoice = !isIncludedInInvoiceLine;
				charge.J7_IsDutiable = isDutiable;
				charge.J7_IsStatisticalValueApplicable = isStatable;
				charge.J7_IsGSTApplicable = isVatable;
				charge.J7_IsSystem = true;
			}
			else
			{
				var charge = invoiceLine.Charges.AddNew(chargeCode, amount, currency);
				charge.J7_ChargeType = chargeCode;
				charge.J7_Amount = amount;
				charge.J7_RX_NKCurrency = currency;
				charge.J7_IsIncludedInITOT = isIncludedInInvoice;
				charge.J7_IsNotIncludedInInvoice = !isIncludedInInvoiceLine;
				charge.J7_IsDutiable = isDutiable;
				charge.J7_IsStatisticalValueApplicable = isStatable;
				charge.J7_IsGSTApplicable = isVatable;
			}
		}

		public void TestQuotaRefNumbers()
		{
			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2203001010";
			invoiceLine1.JI_ConcessionOrder = "123456";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2203001010";
			invoiceLine2.JI_ConcessionOrder = ZString.Empty;

			var merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			var entryLine = entryHeader.MergedLines[0];

			errorCollector = new ErrorCollector();

			var articleWrapper = new ArticleWrapper(entryHeader, entryLine);
			AssertEquals("Wrapper should left empty quota reference numbers aside", 1, articleWrapper.QuotaRefNumber.Count());
		}

		public void TestSupplementaryAndThirdUnits()
		{
			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2203001010";
			invoiceLine1.JI_CustomsSecondQuantity = 10;
			invoiceLine1.JI_CustomsSecondUnitQty = "DTN1";
			invoiceLine1.JI_CustomsThirdQuantity = 50;
			invoiceLine1.JI_CustomsThirdUnitQty = "HLT1";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2203001010";
			invoiceLine2.JI_CustomsSecondQuantity = 10;
			invoiceLine2.JI_CustomsSecondUnitQty = "DTN1";
			invoiceLine2.JI_CustomsThirdQuantity = 50;
			invoiceLine2.JI_CustomsThirdUnitQty = "HLT1";

			var merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			var entryLine = entryHeader.MergedLines[0];

			errorCollector = new ErrorCollector();

			var articleWrapper = new ArticleWrapper(entryHeader, entryLine);
			AssertEquals("The supp unit code should be the 3 first digits of entry line's first invoice line supp unit", "DTN", articleWrapper.SuppUnit.Code);
			AssertEquals("The supp unit qualifier should be the 4th digit of entry line's first invoice line supp unit", "1", articleWrapper.SuppUnit.Qualif);
			AssertEquals("The supp unit quantity should be the sum of all invoice lines supp quantities of the entry line", "20", articleWrapper.SuppUnit.Qty.ToString());
			AssertEquals("The third unit code should be the 3 first digits of entry line's first invoice line third unit", "HLT", articleWrapper.ThirdUnit.Code);
			AssertEquals("The third unit qualifier should be the 4th digit of entry line's first invoice line third unit", "1", articleWrapper.ThirdUnit.Qualif);
			AssertEquals("The third unit qualifier should be the sum of all invoice lines third quantities of the entry line", "100", articleWrapper.ThirdUnit.Qty.ToString());
		}

		public void TestDuplicateAdditionalNationalCodes()
		{
			var invoiceLine = (JobComInvoiceLine)cusFrEntryLine.InvoiceLines[0];
			var additionalCode1 = Factory.New<SupplementaryCode>();
			additionalCode1.CY_Code = "V901";
			invoiceLine.AdditionalSupplementaryCodes.Add(additionalCode1);
			var additionalCode2 = Factory.New<SupplementaryCode>();
			additionalCode2.CY_Code = "V901";
			invoiceLine.AdditionalSupplementaryCodes.Add(additionalCode2);

			Assert("FRTariffAdditionalCodes should contain 1 item", articleFrWrapper.FRTariffAdditionalCodes.Count() == 1);
		}

		public void TestTariffAdditionalCodes()
		{
			var invoiceLine = (JobComInvoiceLine)cusFrEntryLine.InvoiceLines[0];
			var additionalCode1 = Factory.New<SupplementaryCode>();
			additionalCode1.CY_Code = "V901";
			invoiceLine.AdditionalSupplementaryCodes.Add(additionalCode1);
			var additionalCode2 = Factory.New<SupplementaryCode>();
			additionalCode2.CY_Code = "V902";
			invoiceLine.AdditionalSupplementaryCodes.Add(additionalCode2);
			var additionalCode3 = Factory.New<SupplementaryCode>();
			additionalCode3.CY_Code = "B001";
			invoiceLine.AdditionalSupplementaryCodes.Add(additionalCode3);
			invoiceLine.JI_SupplementaryCode1 = "B002";
			invoiceLine.JI_SupplementaryCode2 = "B003";
			AssertContainsExactElementsInAnyOrder("FR additional code list should gather V901 & V902.", new[] { "V901", "V902" }, articleFrWrapper.FRTariffAdditionalCodes.Select(x => x.Code));
			AssertContainsExactElementsInAnyOrder("CE additional code list should gather B001, B002 & B003.", new[] { "B001", "B002", "B003" }, articleFrWrapper.CETariffAdditionalCodes.Select(x => x.Code));
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestArticleWrapperConstructor()
		{
			Activator.CreateInstance(typeof(ArticleWrapper), new object[] { null, null });

			AssertNotNull(cusFrEntryLine);
			AssertNotNull(cusUsEntryLine);
		}

		public void TestGetWarehouseType_WhenInward()
		{
			var factory = cusEntryHeader.Factory;
			var inwardProcedure = RefDataHelper.CreateInwardCusProcedure(factory);

			var invoiceLine = cusFrEntryLine.InvoiceLines[0];
			invoiceLine.JI_Procedure = inwardProcedure.ZZ6_ProcedureCode + inwardProcedure.ZZ6_PreviousProcedureCode + inwardProcedure.ZZ6_Concession;

			var warehouseAddress = factory.New<OrgAddress>();
			cusEntryHeader.EntryInstruction.CEI_OA_Warehouse2 = warehouseAddress.PK;
			cusEntryHeader.EntryInstruction.CEI_Style = "71P";

			var auth = warehouseAddress.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("WHS 001");
			AssertEquals(WarehouseTypeList.Codes.Type_U, articleFrWrapper.WarehouseType);

			auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			AssertEquals(WarehouseTypeList.Codes.Type_R, articleFrWrapper.WarehouseType);

			auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			AssertEquals(WarehouseTypeList.Codes.Type_S, articleFrWrapper.WarehouseType);
		}

		public void TestGetWarehouseType_WhenOutward()
		{
			var factory = cusEntryHeader.Factory;
			var inwardProcedure = RefDataHelper.CreateOutwardCusProcedure(factory);

			var invoiceLine = cusFrEntryLine.InvoiceLines[0];
			invoiceLine.JI_Procedure = inwardProcedure.ZZ6_ProcedureCode + inwardProcedure.ZZ6_PreviousProcedureCode + inwardProcedure.ZZ6_Concession;

			var warehouseAddress = factory.New<OrgAddress>();
			cusEntryHeader.EntryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;
			cusEntryHeader.EntryInstruction.CEI_Style = "71P";

			var auth = warehouseAddress.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("WHS 001");
			AssertEquals(WarehouseTypeList.Codes.Type_U, articleFrWrapper.WarehouseType);

			auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			AssertEquals(WarehouseTypeList.Codes.Type_R, articleFrWrapper.WarehouseType);

			auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			AssertEquals(WarehouseTypeList.Codes.Type_S, articleFrWrapper.WarehouseType);
		}

		public void TestGetWarehouseType_WhenEmpty()
		{
			AssertEquals(ZString.Empty, articleFrWrapper.WarehouseType);
		}

		public void TestGetWarehouseTypeIsEmptyIfEntryInstructionNot71P()
		{
			var factory = cusEntryHeader.Factory;
			var inwardProcedure = RefDataHelper.CreateOutwardCusProcedure(factory);

			var invoiceLine = cusFrEntryLine.InvoiceLines[0];
			invoiceLine.JI_Procedure = inwardProcedure.ZZ6_ProcedureCode + inwardProcedure.ZZ6_PreviousProcedureCode + inwardProcedure.ZZ6_Concession;

			var warehouseAddress = factory.New<OrgAddress>();
			cusEntryHeader.EntryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;

			var auth = warehouseAddress.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("WHS 001");

			cusEntryHeader.EntryInstruction.CEI_Style = DeltaGImportDeclarationTypeList.Codes.ImportationForInwardProcessing;
			AssertEquals("WarehouseType is only available for 71P style of entry instruction.", ZString.Empty, articleFrWrapper.WarehouseType);

			cusEntryHeader.EntryInstruction.CEI_Style = DeltaGImportDeclarationTypeList.Codes.PlacingGoodsUnderBW;
			AssertEquals("WarehouseType is  available for 71P", WarehouseTypeList.Codes.Type_U, articleFrWrapper.WarehouseType);
		}

		[TestDate(2022, 4, 21)]
		public void TestGetInvoiceLinePrice()
		{
			AssertEquals(60m, articleUsWrapper.InvoiceLinePrice);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, articleUsWrapper.CurrencyCode);
			AssertEquals(50m, articleFrWrapper.InvoiceLinePrice);
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, articleFrWrapper.CurrencyCode);
		}

		public void TestDeliveryDepartment()
		{
			errorCollector.WipeErrors();

			AssertEquals("DeliveryDepartment should be null because importer post code is null.", "", articleFrWrapper.DeliveryDepartment);

			errorCollector.WipeErrors();
			cusEntryHeader.Declaration.Importer.MainAddress.Postcode = "24130";
			var address1 = cusEntryHeader.Declaration.Importer.Addresses.AddNew();
			address1.OA_PostCode = "12345";
			cusEntryHeader.Declaration.ImporterDocumentaryAddress.OrganisationPK = cusEntryHeader.Declaration.Importer.PK;
			cusEntryHeader.Declaration.ImporterDocumentaryAddress.E2_OA_Address = address1.PK;
			AssertEquals("DeliveryDepartment should match the 2 first digits of the ImporterDocumentaryAddress post code.", "12", articleFrWrapper.DeliveryDepartment);
			AssertEquals("DeliveryDepartment should match the 2 first digits of the ImporterDocumentaryAddress post code.", 0, errorCollector.ErrorCount);
		}

		public void TestExpeditionDepartment()
		{
			errorCollector.WipeErrors();

			AssertEquals("ExpeditionDepartment should be null because importer post code is null.", "", articleFrWrapper.ExpeditionDepartment);
			AssertEquals("ExpeditionDepartment should be null because importer post code is null.", 0, errorCollector.ErrorCount);

			errorCollector.WipeErrors();

			RefDataHelper.SetUpRefUNLOCO(Factory, "FR001", "01", "Department01", "FR");
			RefDataHelper.SetUpRefUNLOCO(Factory, "FR75C", "75C", "Department75C", "FR");
			RefDataHelper.SetUpRefUNLOCO(Factory, "FR69M", "69M", "Department69M", "FR");
			RefDataHelper.SetUpRefUNLOCO(Factory, "FR6AE", "6AE", "Department6AE", "FR");

			var customsOffice = Factory.New<ZZRefCusCodeListCombined>();
			customsOffice.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.France;
			customsOffice.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			customsOffice.ZZD_Code = "FR002300";
			customsOffice.ZZD_StartDate = ZDateTime.Today;
			customsOffice.ZZD_EndDate = ZDateTime.Today.AddYears(1);

			var attribute = customsOffice.Attributes.AddNew();
			attribute.ZZE_Value = "12345";
			attribute.ZZE_ZXE_NKName = "PostCode";
			Factory.Save();

			cusEntryHeader.Declaration.JE_CustomsOffice = "FR002300";

			AssertEquals("ExpeditionDepartment should match the 2 first digits of the Customs Office post code.", "12", articleFrWrapper.ExpeditionDepartment);
			AssertEquals("ExpeditionDepartment should match the 2 first digits of the Customs Office post code.", 0, errorCollector.ErrorCount);

			cusEntryHeader.Declaration.Origin.RL_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusEntryHeader.Declaration.JE_RL_NKOrigin = "FR001";
			AssertEquals("ExpeditionDepartment should match the state code of France if the port is in France.", "01", articleFrWrapper.ExpeditionDepartment);

			cusEntryHeader.Declaration.JE_RL_NKOrigin = "FR75C";
			AssertEquals("ExpeditionDepartment should be Customs friendly.", "75", articleFrWrapper.ExpeditionDepartment);

			cusEntryHeader.Declaration.JE_RL_NKOrigin = "FR69M";
			AssertEquals("ExpeditionDepartment should be Customs friendly.", "69", articleFrWrapper.ExpeditionDepartment);

			cusEntryHeader.Declaration.JE_RL_NKOrigin = "FR6AE";
			AssertEquals("ExpeditionDepartment should be Customs friendly.", "67", articleFrWrapper.ExpeditionDepartment);
		}

		public void TestGetDispoPart()
		{
			List<ITariffAdditionalCode> dtpList = articleFrWrapper.PartDispos.ToList();

			AssertNotNull(dtpList);
			AssertEquals(3, dtpList.Count);
		}

		public void SetupMappingOfAuthorisationDocumentCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.AUTDC, MapDirectionList.Codes.BTH, "Authorisation document code", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "OPO", "C019", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "IPO", "C601", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "TEA", "C516", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "CWP", "C517", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "CW1", "C518", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "CW2", "C519", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "EUS", "N990", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);

			Factory.Save();
		}

		public void TestGetSupportingDocuments()
		{
			SetupMappingOfAuthorisationDocumentCodes();

			var declaration = cusEntryHeader.Declaration;
			var factory = declaration.Factory;
			var cei = declaration.CustomsEntryInstructions[0];
			var importer = declaration.Importer;

			var authHeader = factory.New<CusAuthorisationHeader>();
			authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authHeader.CPH_OH_PermitHolder = importer.PK;
			authHeader.CPH_Number = "TST_ATH_001";
			authHeader.CPH_StartDate = new ZDate(2023, 08, 07);
			authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			authHeader.CPH_IsAdHoc = false;

			var usage = cei.CusAuthorizationUsages.AddNew();
			usage.AGC_Number = "TST_ATH_001";
			usage.AGC_OH_Owner = importer.PK;

			var merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();

			var articleWrapper = new ArticleWrapper(cusEntryHeader, cusFrEntryLine);

			var supportingDocumentList = articleWrapper.SupportingDocuments.ToList();
			AssertNotNull(supportingDocumentList);
			AssertEquals(3, supportingDocumentList.Count);

			cei.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();
			articleWrapper = new ArticleWrapper(cusEntryHeader, cusFrEntryLine);
			supportingDocumentList = articleWrapper.SupportingDocuments.ToList();
			Assert("OutwardProcessing authorisation document code should be C019.", supportingDocumentList.Any(x => x.Code == "C019"));

			cei.CEI_Style = DeltaGImportDeclarationTypeList.Codes.ImportationForInwardProcessing;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();
			articleWrapper = new ArticleWrapper(cusEntryHeader, cusFrEntryLine);
			supportingDocumentList = articleWrapper.SupportingDocuments.ToList();
			Assert("InwardProcessing authorisation document code should be C601.", supportingDocumentList.Any(x => x.Code == "C601"));

			cei.CEI_Style = DeltaGImportDeclarationTypeList.Codes.TemporaryImportation;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission;
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission;
			merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();
			articleWrapper = new ArticleWrapper(cusEntryHeader, cusFrEntryLine);
			supportingDocumentList = articleWrapper.SupportingDocuments.ToList();
			Assert("TemporaryAdmission authorisation document code should be C516.", supportingDocumentList.Any(x => x.Code == "C516"));

			cei.CEI_Style = DeltaGExportDeclarationTypeList.Codes.PlacingGoodsUnderBW;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();
			articleWrapper = new ArticleWrapper(cusEntryHeader, cusFrEntryLine);
			supportingDocumentList = articleWrapper.SupportingDocuments.ToList();
			Assert("CustomsWarehousingCWP authorisation document code should be C517.", supportingDocumentList.Any(x => x.Code == "C517"));

			cei.CEI_Style = DeltaGExportDeclarationTypeList.Codes.PlacingGoodsUnderBW;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();
			articleWrapper = new ArticleWrapper(cusEntryHeader, cusFrEntryLine);
			supportingDocumentList = articleWrapper.SupportingDocuments.ToList();
			Assert("CustomsWarehousingCW1 authorisation document code should be C518.", supportingDocumentList.Any(x => x.Code == "C518"));

			cei.CEI_Style = DeltaGExportDeclarationTypeList.Codes.PlacingGoodsUnderBW;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();
			articleWrapper = new ArticleWrapper(cusEntryHeader, cusFrEntryLine);
			supportingDocumentList = articleWrapper.SupportingDocuments.ToList();
			Assert("CustomsWarehousingCW2 authorisation document code should be C519.", supportingDocumentList.Any(x => x.Code == "C519"));
		}

		public void TestGetGetEntryLineDescription()
		{
			ZString description = "SDescription of the entry line";
			ZString blobDescription = "SDescription of the entry lineDescription of the entry lineDescription of the entry lineDescription of the entry lineDescription of the entry lineE";
			blobDescription += "Description of the entry lineDescription of the entry lineDescription of the entry lineDescription of the entry lineDescription of the entry line";
			blobDescription += "Description of the entry lineDescription of the entry lineDescription of the entry lineDescription of the entry lineDescription of the entry lineE";

			cusFrEntryLine.CL_Description = description;
			AssertEquals(description, articleFrWrapper.EntryLineDescription);
			cusFrEntryLine.CL_Description = blobDescription;
			AssertNotEquals(blobDescription, articleFrWrapper.EntryLineDescription);
			AssertEquals(ArticleWrapper.EntrylineDescriptionMaxLength, articleFrWrapper.EntryLineDescription.Length);
		}

		public void TestGetSpecMenTexts()
		{
			var specMenList = articleFrWrapper.SpecMens;
			AssertNotNull(specMenList);
			AssertEquals(0, specMenList.ToList().Count);
		}

		public void TestGetContainers()
		{
			List<ZString> containersNumberList = articleFrWrapper.Containers.ToList();
			AssertNotNull(containersNumberList);
			AssertEquals(1, containersNumberList.Count);
			AssertEquals("123456", containersNumberList[0]);
		}

		public void TestPreviousDocuments()
		{
			Assert(articleFrWrapper.PreviousDocument != null);
			AssertEquals("Invoice Line Previous Document", articleFrWrapper.PreviousDocument.RefNumber);

			cusEntryHeader.Declaration.InvoiceLines[0].PreviousDocuments.RemoveAndDeleteAll();
			AssertEquals("Invoice Header Previous Document", articleFrWrapper.PreviousDocument.RefNumber);

			cusEntryHeader.Declaration.Invoices[0].PreviousDocuments.RemoveAndDeleteAll();
			AssertEquals("Declaration Previous Document", articleFrWrapper.PreviousDocument.RefNumber);

			cusEntryHeader.Declaration.PreviousDocuments.RemoveAndDeleteAll();
			AssertNull(articleFrWrapper.PreviousDocument);
		}

		public void TestGetWarehouseCode()
		{
			var factory = cusEntryHeader.Factory;

			var authorisationOwner = factory.NewWithValidTestData<OrgHeader>();

			var authorisationHeader = factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisationHeader.CPH_OH_PermitHolder = authorisationOwner.PK;
			authorisationHeader.CPH_Number = "AUT001";
			authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing;

			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.AUT;
			authorisationRule.CPR_ValueFrom = "12345";

			var authorisationUsage = cusEntryHeader.EntryInstruction.CusAuthorizationUsages.AddNew();
			authorisationUsage.AGC_Number = "AUT001";
			authorisationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			authorisationUsage.AGC_OH_Owner = authorisationOwner.PK;

			factory.Save();
			AssertEquals("Prerequisite", "12345", authorisationUsage.AGC_AuthorizationShortCode);

			var inwardProcedure = RefDataHelper.CreateInwardCusProcedure(factory);
			var invoiceLine = cusFrEntryLine.InvoiceLines[0];
			invoiceLine.JI_Procedure = inwardProcedure.ZZ6_ProcedureCode + inwardProcedure.ZZ6_PreviousProcedureCode + inwardProcedure.ZZ6_Concession;
			AssertEquals("WarehouseReference should return the authorisation short code.", "12345", articleFrWrapper.WarehouseReference);

			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.CNT;
			authorisationRule.CPR_ValueFrom = "12345";
			factory.Save();
			AssertEquals(ZString.Empty, authorisationUsage.AGC_AuthorizationShortCode);
			AssertEquals(ZString.Empty, articleFrWrapper.WarehouseReference);
		}

		public void TestGetWarehouseCode_WhenEmpty()
		{
			AssertEquals(ZString.Empty, articleFrWrapper.WarehouseReference);
			AssertEquals(0, errorCollector.ErrorCount);
		}

		public void TestPreCalcEntryLines()
		{
			cusFrEntryLine.RandomLine.JI_TariffBypassCode = ZString.Empty;
			AssertEquals(3, articleFrWrapper.PreCalcEntryLines.Count());

			var articleWrapper1 = articleFrWrapper.PreCalcEntryLines.Cast<IPreCalcEntryLine>().FirstOrDefault(x => x.Tax.TaxCode == "A445");
			CombineAssertions(() =>
			{
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_ChargeType, "A445", articleWrapper1.Tax.TaxCode);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_MethodOfCalculation, "0", articleWrapper1.Tax.TaxType);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_Rate, 5.0m, articleWrapper1.Tax.TaxRate);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_BaseValue, 10.0m, articleWrapper1.Tax.TaxAssessed);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_ChargeAmount, 20.0m, articleWrapper1.Tax.TaxAmount);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_MethodOfPayment, "A", articleWrapper1.Tax.TaxMethodOfPayment);
			});

			var articleWrapper2 = articleFrWrapper.PreCalcEntryLines.Cast<IPreCalcEntryLine>().FirstOrDefault(x => x.Tax.TaxCode == "A325");
			CombineAssertions(() =>
			{
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_ChargeType, "A325", articleWrapper2.Tax.TaxCode);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_MethodOfCalculation, "1", articleWrapper2.Tax.TaxType);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_Rate, 2.0m, articleWrapper2.Tax.TaxRate);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_BaseValue, 30.0m, articleWrapper2.Tax.TaxAssessed);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_ChargeAmount, 40.0m, articleWrapper2.Tax.TaxAmount);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_MethodOfPayment, "B", articleWrapper2.Tax.TaxMethodOfPayment);
			});

			var articleWrapper3 = articleFrWrapper.PreCalcEntryLines.Cast<IPreCalcEntryLine>().FirstOrDefault(x => x.Tax.TaxCode == "A241");
			CombineAssertions(() =>
			{
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_ChargeType, "A241", articleWrapper3.Tax.TaxCode);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_MethodOfCalculation, "0", articleWrapper3.Tax.TaxType);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_Rate, 4m, articleWrapper3.Tax.TaxRate);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_BaseValue, 50m, articleWrapper3.Tax.TaxAssessed);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_ChargeAmount, 60m, articleWrapper3.Tax.TaxAmount);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_MethodOfPayment, "D", articleWrapper3.Tax.TaxMethodOfPayment);
			});

			cusFrEntryLine.RandomLine.JI_TariffBypassCode = "1";
			AssertEquals(4, articleFrWrapper.PreCalcEntryLines.Count());

			articleWrapper1 = articleFrWrapper.PreCalcEntryLines.Cast<IPreCalcEntryLine>().FirstOrDefault(x => x.Tax.TaxCode == "A445");
			CombineAssertions(() =>
			{
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_ChargeType, "A445", articleWrapper1.Tax.TaxCode);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_MethodOfCalculation, "0", articleWrapper1.Tax.TaxType);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_Rate, 5.0m, articleWrapper1.Tax.TaxRate);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_BaseValue, 10.0m, articleWrapper1.Tax.TaxAssessed);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_ChargeAmount, 20.0m, articleWrapper1.Tax.TaxAmount);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_MethodOfPayment, "A", articleWrapper1.Tax.TaxMethodOfPayment);
			});

			articleWrapper2 = articleFrWrapper.PreCalcEntryLines.Cast<IPreCalcEntryLine>().FirstOrDefault(x => x.Tax.TaxCode == "A325");
			CombineAssertions(() =>
			{
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_ChargeType, "A325", articleWrapper2.Tax.TaxCode);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_MethodOfCalculation, "1", articleWrapper2.Tax.TaxType);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_Rate, 2.0m, articleWrapper2.Tax.TaxRate);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_BaseValue, 30.0m, articleWrapper2.Tax.TaxAssessed);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_ChargeAmount, 40.0m, articleWrapper2.Tax.TaxAmount);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_MethodOfPayment, "B", articleWrapper2.Tax.TaxMethodOfPayment);
			});

			articleWrapper3 = articleFrWrapper.PreCalcEntryLines.Cast<IPreCalcEntryLine>().FirstOrDefault(x => x.Tax.TaxCode == "A240");
			CombineAssertions(() =>
			{
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_ChargeType, "A240", articleWrapper3.Tax.TaxCode);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_MethodOfCalculation, "0", articleWrapper3.Tax.TaxType);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_Rate, 3.0m, articleWrapper3.Tax.TaxRate);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_BaseValue, 40.0m, articleWrapper3.Tax.TaxAssessed);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_ChargeAmount, 50.0m, articleWrapper3.Tax.TaxAmount);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_MethodOfPayment, "C", articleWrapper3.Tax.TaxMethodOfPayment);
			});

			var articleWrapper4 = articleFrWrapper.PreCalcEntryLines.Cast<IPreCalcEntryLine>().FirstOrDefault(x => x.Tax.TaxCode == "A241");
			CombineAssertions(() =>
			{
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_ChargeType, "A241", articleWrapper4.Tax.TaxCode);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_MethodOfCalculation, "0", articleWrapper4.Tax.TaxType);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_Rate, 4m, articleWrapper4.Tax.TaxRate);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_BaseValue, 50m, articleWrapper4.Tax.TaxAssessed);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_ChargeAmount, 60m, articleWrapper4.Tax.TaxAmount);
				AssertEquals(CusEntryLineFeeSchema.Constants.CF_MethodOfPayment, "D", articleWrapper4.Tax.TaxMethodOfPayment);
			});
		}

		public void TestSpecificRegimeProperties()
		{
			var emptyWrapper = new ArticleWrapper(Factory.New<Declaration.CusEntryHeader>(), Factory.New<Declaration.CusEntryLine>());
			CombineAssertions("SpecificRegime Properties for empty ArticleWrapper, should not throw exception and expects all empty value.", () =>
			{
				AssertEquals("ApplicantInwardNature", ZString.Empty, emptyWrapper.ApplicantInwardNature);
				AssertEquals("ApplicantDescription", ZString.Empty, emptyWrapper.ApplicantDescription);
				AssertEquals("ApplicantConditions", ZString.Empty, emptyWrapper.ApplicantConditions);
				AssertEquals("ApplicantPurOffice", ZString.Empty, emptyWrapper.ApplicantPurOffice);
				AssertEquals("ApplicantInwardLocation", ZString.Empty, emptyWrapper.ApplicantInwardLocation);
				AssertEquals("ApplicantTransFormality", ZString.Empty, emptyWrapper.ApplicantTransFormality);
				AssertEquals("SpecificInfos", ZString.Empty, emptyWrapper.SpecificInfos);
			});

			var dec = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = dec.PK;
			instruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_RL_NKClosestPort = "GBLON";
			var authHeader = Factory.New<CusAuthorisationHeader>();
			authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authHeader.CPH_StartDate = ZDate.Today;
			authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			authHeader.CPH_OH_PermitHolder = customer.PK;
			authHeader.CPH_Number = "TST_ATH_001";
			authHeader.CPH_PermitDescription = "CN Code 123456";
			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			usage.AGC_Number = "TST_ATH_001";
			usage.AGC_OH_Owner = customer.PK;

			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.NAT, "N1");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.CON, "C1");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.OFC, "O1").CPR_Description = "Square";
			CreateCusAuthorisationRule(authHeader, Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "L1").CPR_Description = "Hidden";
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.TRA, "T1");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.INF, "I1");

			var wrapper = new ArticleWrapper(entry, cusFrEntryLine);
			CombineAssertions("ArticleWrapper should return correct values on all SpecificRegime Properties.", () =>
			{
				AssertEquals("ApplicantInwardNature", "N1", wrapper.ApplicantInwardNature);
				AssertEquals("ApplicantDescription", "CN Code 123456", wrapper.ApplicantDescription);
				AssertEquals("ApplicantConditions", "C1", wrapper.ApplicantConditions);
				AssertEquals("ApplicantPurOffice", "O1", wrapper.ApplicantPurOffice);
				AssertEquals("ApplicantInwardLocation", "L1", wrapper.ApplicantInwardLocation);
				AssertEquals("ApplicantTransFormality", "T1", wrapper.ApplicantTransFormality);
				AssertEquals("SpecificInfos", "I1", wrapper.SpecificInfos);
			});
		}

		public void TestEcoRegimeDatas_PreviousInbondMovements()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_DeltaMode = "G1";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2203001010";
			invoiceLine1.JI_Procedure = "1021";

			var preInbondMovement1 = invoiceLine1.PreviousDocuments.AddNew();
			(preInbondMovement1.CSI_Code, preInbondMovement1.CSI_SubType, preInbondMovement1.CSI_ReferenceNumber) = ("IM", "Z", "Normal");

			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			invoiceLine2.JI_Tariff = "2203001010";
			invoiceLine2.JI_Procedure = "1021";

			var preInbondMovement2 = invoiceLine2.PreviousDocuments.AddNew();
			(preInbondMovement2.CSI_Code, preInbondMovement2.CSI_SubType, preInbondMovement2.CSI_ReferenceNumber) = ("IM", "Z", "Normal");

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_Style = DeltaGImportDeclarationTypeList.Codes.ImportationForInwardProcessing;

			var entryHeader = (Declaration.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.FRCustomsFallbackNumber = "Normal";

			var entryLine = (Declaration.CusEntryLine)entryHeader.AllEntryLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine1);
			entryLine.InvoiceLines.Add(invoiceLine2);

			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			usage.AGC_OH_Owner = importer.PK;
			usage.AGC_Number = "12345678";

			var authorisationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationHeader.CPH_Number = "12345678";
			authorisationHeader.CPH_IsSingleUse = true;
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			authorisationHeader.CPH_OH_PermitHolder = importer.PK;
			authorisationHeader.CPH_RN_NKCountryCode = "FR";

			var errorCollector = new ErrorCollector();
			var wrapper = new ArticleWrapper(entryHeader, entryLine);

			AssertNotNull(wrapper.EcoRegimeDatas);

			CombineAssertions(() =>
			{
				AssertEquals("Previous Inbond Movement Count", 2, wrapper.EcoRegimeDatas.DecEcos.Count());
				AssertContainsExactElementsInAnyOrder("CusDeclarationID", new[] { "Normal", "Normal" }, wrapper.EcoRegimeDatas.DecEcos.Select(_ => _.CusDeclarationID));
				AssertContainsExactElementsInAnyOrder("EcoRegimeDeclTypeCode", new[] { "1", "1" }, wrapper.EcoRegimeDatas.DecEcos.Select(_ => _.EcoRegimeDeclTypeCode));
			});
		}

		public void TestEntrySnapshot()
		{
			AssertEquals("[Prerequisite]: No snapshot created for cusEntryHeader originally.", 0, cusEntryHeader.Snapshots.Count);
			var snapshotDG = cusEntryHeader.Snapshots.AddNew();
			snapshotDG.CES_MessageType = DeclarationApplicationCodeList.Codes.DeltaG;
			snapshotDG.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
			snapshotDG.CES_SnapshotXml = "<Entry xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"></Entry>\r\n";

			var snapshotVAL = cusEntryHeader.Snapshots.AddNew();
			snapshotVAL.CES_MessageType = EntryActionCodeList.Codes.VAL;
			snapshotVAL.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
			snapshotVAL.CES_SnapshotXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<FrenchEntryLineChildSnapshot xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
<CusEntryLine>
<LineNumber>1</LineNumber>
<ChildData Type=""DOC"">
<Code>DOC21</Code>
<Reference>BBBBBBBB</Reference>
</ChildData>
</CusEntryLine>
</FrenchEntryLineChildSnapshot>";
			var wrapper = new ArticleWrapper(cusEntryHeader, cusFrEntryLine);

			AssertNoExceptionThrown("Only the snapshot of CES_MessageType VAL shall be considered, thus correctly parsed without exception.", () => { var entryLineInfo = wrapper.EntrySnapshot; });
		}

		protected override void SetUp()
		{
			base.SetUp();

			cusEntryHeader = CreateDeclaration(true).CustomsEntryHeaders[0];

			errorCollector = new ErrorCollector();

			cusFrEntryLine = cusEntryHeader.MergedLines.Cast<Declaration.CusEntryLine>().FirstOrDefault(a => a.CountryOfOrigin.Code == Core.Constants.CountryCodes.France);
			cusUsEntryLine = cusEntryHeader.MergedLines.Cast<Declaration.CusEntryLine>().FirstOrDefault(a => a.CountryOfOrigin.Code == Core.Constants.CountryCodes.UnitedStates);

			var fee1 = cusFrEntryLine.Fees.AddNew();
			fee1.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
			fee1.CF_ChargeType = "B01";
			fee1.NationalFeeTypeCode = "A445";
			fee1.CF_Rate = 5;
			fee1.CF_BaseValue = 10;
			fee1.CF_ChargeAmount = 20;
			fee1.CF_MethodOfPayment = "A";
			var fee2 = cusFrEntryLine.Fees.AddNew();
			fee2.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Override;
			var fee3 = cusFrEntryLine.Fees.AddNew();
			fee3.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
			fee3.CF_ChargeType = "A01";
			fee3.NationalFeeTypeCode = "A325";
			fee3.CF_Rate = 2;
			fee3.CF_BaseValue = 30;
			fee3.CF_ChargeAmount = 40;
			fee3.CF_MethodOfPayment = "B";
			var fee4 = cusFrEntryLine.Fees.AddNew();
			fee4.CF_RateOverrideReasonCode = ZString.Empty;
			fee4.CF_ChargeType = "A02";
			fee4.NationalFeeTypeCode = "A240";
			fee4.CF_Rate = 3;
			fee4.CF_BaseValue = 40;
			fee4.CF_ChargeAmount = 50;
			fee4.CF_MethodOfPayment = "C";
			var fee5 = cusFrEntryLine.Fees.AddNew();
			fee5.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Precalcule;
			fee5.CF_ChargeType = "A03";
			fee5.NationalFeeTypeCode = "A241";
			fee5.CF_Rate = 4;
			fee5.CF_BaseValue = 50;
			fee5.CF_ChargeAmount = 60;
			fee5.CF_MethodOfPayment = "D";

			articleFrWrapper = new ArticleWrapper(cusEntryHeader, cusFrEntryLine);
			articleUsWrapper = new ArticleWrapper(cusEntryHeader, cusUsEntryLine);
		}

		#region Methods

		public static JobDeclaration CreateDeclaration(bool import)
		{
			var factory = new BusinessObjectFactory();

			var declaration = factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();

			EU.Business.Declaration.CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "123456";

			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MasterBill = "UnitTest";

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var declarantAddress = factory.New<OrgAddress>();
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = OrgCusCode.FranceCodeTypes.Siret;

			declarantAddress.OA_OH = orgHeader.PK;
			declarantAddress.OA_Address1 = "Eugene Leroy Street ";
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			#region Organisation Registration number : SRT

			var orgCusCodeSrt = factory.New<OrgCusCode>();
			orgCusCodeSrt.OK_OA_PremisesAddress = declarantAddress.PK;
			orgCusCodeSrt.OK_CodeType = OrgCusCode.FranceCodeTypes.Siret;
			orgCusCodeSrt.OK_CustomsRegNo = "FR33159700500064";
			orgCusCodeSrt.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			orgCusCodeSrt.OK_OH = declarantAddress.OA_OH;

			orgHeader.CustomsCodes.Add(orgCusCodeSrt);

			#endregion

			#region Organisation Registration number : CBR

			var orgCusCodeCbr = factory.New<OrgCusCode>();
			orgCusCodeCbr.OK_OA_PremisesAddress = declarantAddress.PK;
			orgCusCodeCbr.OK_CodeType = OrgCusCode.CodeTypes.BrokerageRegistration;
			orgCusCodeCbr.OK_CustomsRegNo = "FR33159700500064";
			orgCusCodeCbr.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			orgCusCodeCbr.OK_OH = declarantAddress.OA_OH;

			orgHeader.CustomsCodes.Add(orgCusCodeCbr);

			#endregion

			var warehouse = factory.New<OrgHeader>();
			warehouse.FillWithValidTestData();
			declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;

			var declarationSupportingDocument = declaration.SupportingDocuments.AddNew();
			declarationSupportingDocument.FillWithValidTestData();
			declarationSupportingDocument.CSI_IsDTP = false;
			declarationSupportingDocument.CSI_ReferenceNumber = "DeclarationDoc";

			var declarationPreviousDocument = declaration.PreviousDocuments.AddNew();
			declarationPreviousDocument.CSI_Code = "PDOC";
			declarationPreviousDocument.CSI_ReferenceNumber = "Declaration Previous Document";

			var declarationDTP = declaration.SupportingDocuments.AddNew();
			declarationDTP.FillWithValidTestData();
			declarationDTP.CSI_Code = "DTPH";
			declarationDTP.CSI_IsDTP = true;

			declaration.WarehouseAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "06725U");

			var cei = declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();

			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoice.JZ_InvoiceAmount = 150;

			var invoiceHeaderSupportingDocument = invoice.SupportingDocuments.AddNew();
			invoiceHeaderSupportingDocument.FillWithValidTestData();
			invoiceHeaderSupportingDocument.CSI_IsDTP = false;
			invoiceHeaderSupportingDocument.CSI_ReferenceNumber = "InvoiceHeaderDoc";

			var invoiceHeaderPreviousDocument = invoice.PreviousDocuments.AddNew();
			invoiceHeaderPreviousDocument.CSI_Code = "PDOC";
			invoiceHeaderPreviousDocument.CSI_ReferenceNumber = "Invoice Header Previous Document";

			var invoiceHeaderDTP = invoice.SupportingDocuments.AddNew();
			invoiceHeaderDTP.FillWithValidTestData();
			invoiceHeaderDTP.CSI_Code = "DTPI";
			invoiceHeaderDTP.CSI_IsDTP = true;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2203001010";
			invoiceLine1.JI_Description = "Unit Test 1";
			invoiceLine1.JI_LinePrice = 50;
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2203001010";
			invoiceLine2.JI_Description = "Unit Test 2";
			invoiceLine2.JI_LinePrice = 100;
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine2.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			invoiceLine1.JI_CustomsSecondQuantity = 10;
			invoiceLine1.JI_CustomsSecondUnitQty = "DTN1";
			invoiceLine1.JI_CustomsThirdQuantity = 50;
			invoiceLine1.JI_CustomsThirdUnitQty = "HLT1";

			var additionalInfo = invoiceLine2.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = "10500";
			additionalInfo.CSI_Description = "Marchandises AT";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice2.JZ_InvoiceAmount = 100;

			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2203001010";
			invoiceLine3.JI_Description = "Unit Test 3";
			invoiceLine3.JI_LinePrice = 60;
			invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine1.JI_CustomsSecondQuantity = 10;
			invoiceLine1.JI_CustomsSecondUnitQty = "DTN1";
			invoiceLine1.JI_CustomsThirdQuantity = 50;
			invoiceLine1.JI_CustomsThirdUnitQty = "HLT1";

			var invoiceLine4 = invoice2.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "2203001010";
			invoiceLine4.JI_Description = "Unit Test 4";
			invoiceLine4.JI_LinePrice = 40;
			invoiceLine4.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine4.JI_CEI = cei.PK;
			invoiceLine1.JI_CustomsSecondQuantity = 10;
			invoiceLine1.JI_CustomsSecondUnitQty = "DTN1";
			invoiceLine1.JI_CustomsThirdQuantity = 50;
			invoiceLine1.JI_CustomsThirdUnitQty = "HLT1";

			var invoiceLinePreviousDocument = invoiceLine1.PreviousDocuments.AddNew();
			invoiceLinePreviousDocument.CSI_Code = "PDOC";
			invoiceLinePreviousDocument.CSI_ReferenceNumber = "Invoice Line Previous Document";

			var dTP1 = invoiceLine1.SupportingDocuments.AddNew();
			dTP1.FillWithValidTestData();
			dTP1.CSI_Code = "DTP1";
			dTP1.CSI_IsDTP = true;

			var dTP2 = invoiceLine1.SupportingDocuments.AddNew();
			dTP2.FillWithValidTestData();
			dTP2.CSI_Code = "DTPI";
			dTP2.CSI_IsDTP = true;

			var dTP3 = invoiceLine1.SupportingDocuments.AddNew();
			dTP3.FillWithValidTestData();
			dTP3.CSI_Code = "DTPH";
			dTP3.CSI_IsDTP = true;

			var supportingDocument1 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocument1.FillWithValidTestData();
			supportingDocument1.CSI_IsDTP = false;
			supportingDocument1.CSI_ReferenceNumber = "InvoiceLineDoc";

			var supportingDocument2 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocument2.FillWithValidTestData();
			supportingDocument2.CSI_IsDTP = false;
			supportingDocument2.CSI_ReferenceNumber = "InvoiceHeaderDoc";

			var supportingDocument3 = invoiceLine1.SupportingDocuments.AddNew();
			supportingDocument3.FillWithValidTestData();
			supportingDocument3.CSI_IsDTP = false;
			supportingDocument3.CSI_ReferenceNumber = "DeclarationDoc";

			var customOffice = declaration.CustomsOffices.AddNew();
			customOffice.CY_Code = "ENT";
			customOffice.CY_Type = "EUO";
			customOffice.CY_Data = "FR000130";

			var importer = factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = import ? "FRPAR" : "AUSYD";
			declaration.JE_OH_Importer = importer.PK;

			var supplier = factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_RL_NKClosestPort = import ? "AUSYD" : "FRPAR";
			declaration.JE_OH_Supplier = supplier.PK;

			if (import == ZBool.False)
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var undg1 = invoiceLine1.UNDGs.AddNew();
				undg1.DI_DG = UNDGSubstanceLoader.LoadSubstances(factory, "0004", "a", "IMO").First().PK;

				var undg2 = invoiceLine1.UNDGs.AddNew();
				undg2.DI_DG = UNDGSubstanceLoader.LoadSubstances(factory, "0014", "b", "IMO").First().PK;

				invoiceLine1.JI_CO = declaration.CusContainers[0].PK;

				declaration.CusContainers[0].CO_Seal = "SCELLE1";

				invoice.ZG_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.CreditCard;
				invoice2.ZG_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.CreditCard;
			}

			var shutUp = new SendsMessagesToCustomsShutterUpperer();

			declaration.DoMerge(shutUp);

			factory.Save();

			return declaration;
		}

		#endregion
		Declaration.CusEntryHeader cusEntryHeader;
		ArticleWrapper articleUsWrapper;
		ArticleWrapper articleFrWrapper;
		Declaration.CusEntryLine cusFrEntryLine;
		Declaration.CusEntryLine cusUsEntryLine;
		ErrorCollector errorCollector;
	}
}
