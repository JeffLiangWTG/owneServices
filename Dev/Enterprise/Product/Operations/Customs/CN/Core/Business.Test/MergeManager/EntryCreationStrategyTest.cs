using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	public class EntryCreationStrategyTest : Customs.Business.Testing.EntryCreationStrategyTest
	{
		public override void TestGetKeyForHeader()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			testItems.EntryInstruction.CEI_DateForDuty = new ZDate(2004, 12, 12);

			var strategy = new Customs.Business.EntryCreationStrategy(testItems.JobDeclaration);
			AssertEquals("Header Valuation Date As String", true, strategy.GetKeyForHeader(testItems.InvoiceLine).Contains(new ZDate(2004, 12, 12)));
		}

		public void TestLineIsValidForMerge()
		{
			var items = CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import, DecTypeList.Codes.Both);
			var declaration = items.JobDeclaration;

			var parentStrategy = new ParentEntryCreationStrategy(declaration, EntryTypeList.Codes.CustomsEntry);
			var childStrategy = new ChildEntryCreationStrategy(declaration, EntryTypeList.Codes.RecordListing);
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertEquals("InvoiceLine not related to Instruction, LineIsValidForMerge() should return false", true, !parentStrategy.LineIsValidForMerge(invoiceLine));
			AssertEquals("InvoiceLine not related to Instruction, LineIsValidForMerge() should return false", true, !childStrategy.LineIsValidForMerge(invoiceLine));

			invoiceLine.JI_CEI = items.EntryInstruction.PK;
			AssertEquals("Normal InvoiceLine, LineIsValidForMerge() should return true", true, parentStrategy.LineIsValidForMerge(invoiceLine));
			AssertEquals("Normal InvoiceLine, LineIsValidForMerge() should return false", true, !childStrategy.LineIsValidForMerge(invoiceLine));

			declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("Normal InvoiceLine, LineIsValidForMerge() should return true", true, parentStrategy.LineIsValidForMerge(invoiceLine));
			AssertEquals("Normal InvoiceLine, LineIsValidForMerge() should return true", true, childStrategy.LineIsValidForMerge(invoiceLine));

			var instruction = items.EntryInstruction;
			instruction.Delete();
			AssertEquals("Instruction deleted, LineIsValidForMerge() should return false", true, !parentStrategy.LineIsValidForMerge(invoiceLine));
			AssertEquals("Instruction deleted, LineIsValidForMerge() should return false", true, !childStrategy.LineIsValidForMerge(invoiceLine));
		}

		public void TestGetKeyForLine()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var tariff = helper.CreateCustomsTariff("2713200000", "00000", "00422", "00069", "00352", "00010", "00009", "00005", "99999");
			helper.CreateTariffUOM(tariff.PK, "CU1", "002");
			helper.CreateTariffUOM(tariff.PK, "CU2", "UQ");
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("00422", "品牌类型");
			helper.CreateAdditionalElement("00069", "出口享惠情况");
			helper.CreateAdditionalElement("00352", "加工方法");
			helper.CreateAdditionalElement("00010", "包装规格");
			helper.CreateAdditionalElement("00009", "GTIN");
			helper.CreateAdditionalElement("00005", "CAS");
			helper.CreateAdditionalElement("99999", "其他");
			Factory.Save();

			var testData = CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import, DecTypeList.Codes.Both);
			var declaration = testData.JobDeclaration;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var strategy = new ParentEntryCreationStrategy(declaration, EntryTypeList.Codes.CustomsEntry);
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = testData.EntryInstruction.PK;

			var jobComInvLineRefs = invoiceLine.InvoiceLineRefs.AddNew();
			jobComInvLineRefs.JG_ReferenceType = Constants.JobComInvLineRefType.FormulaPricingRecordNumber;
			jobComInvLineRefs.JG_ReferenceNumber = "999";

			invoiceLine.JI_ProductManualNo = 100010;
			invoiceLine.JI_ProductManualNo2 = 200020;
			AssertEquals(true, !strategy.GetKeyForLine(invoiceLine).Contains(new ZInt(100010)));
			AssertEquals(true, !strategy.GetKeyForLine(invoiceLine).Contains(new ZInt(200020)));
			testData.EntryInstruction.CEI_ManualNo = "Z011011011";
			instruction2.CEI_ManualNo = "Z022022022";

			invoiceLine.JI_Tariff = "2713200000";
			testData.EntryInstruction.CEI_CIQRequires = true;
			invoiceLine.JI_TradeUnitQty = "035";
			invoiceLine.JI_CustomsUnitQty = "002";
			invoiceLine.JI_CustomsSecondUnitQty = "UQ";
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceLine.JI_DutyMode = "D";
			invoiceLine.CertificateOfOrigin = "123456789";
			invoiceLine.CertificateOfOriginType = "C";
			invoiceLine.ItemNoOnCertOfOrigin = 2;
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_RN_NKCountryOfExport = "US";
			invoiceLine.JI_NameOfGoods = "AAAAA";
			invoiceLine.XC_GoodsSpecModel = "1|3||1千克/箱|GTIN|CAS|无其他";
			invoiceLine.JI_NameOfGoods2 = "BBBBB";
			invoiceLine.XC_GoodsSpecModel2 = "1|3||1千克/箱";
			invoiceLine.JI_CIQOriginState = "CCC";
			invoiceLine.JI_OriginDistrict = "DDD";
			invoiceLine.JI_OriginRegion = "EEE";
			invoiceLine.JI_DestinationDistrict = "FFF";
			invoiceLine.JI_DestinationRegion = "GGG";
			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			invoiceLine.JI_CIQTariff = "III";
			invoiceLine.JI_CIQEndUse = "JJ";
			invoiceLine.JI_SecondaryPreference = "01";

			invoiceLine.CIQIngredient = "Ingredient Note";
			invoiceLine.JI_CIQQualityGuaranteePeriod = 12;
			var manufacturerAddr = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Manufacturer 1", "", "", "");
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddr.PK;
			var manufacturerPK = manufacturerAddr.Header.PK;
			var substance = DGSubstanceTestHelper.Create("0000", "B", "IMO");
			invoiceLine.UNDGs.FirstItemForBinding[0].DI_DG = substance.PK;
			invoiceLine.JI_NonDangerousChemicalFlag = true;
			invoiceLine.JI_PackageTypeOfUNDG = "1A1";
			invoiceLine.JI_NDescription = "SPEC";
			invoiceLine.JI_BrandName = "MODEL";
			invoiceLine.JI_Model = "BRAND";

			var pq1 = invoiceLine.CIQProductQualifications.AddNew();
			pq1.CSI_Code = "PQ1";
			pq1.CSI_ReferenceNumber = "NUM1";
			pq1.CSI_UnitOfQuantity = "UQ";
			pq1.CSI_LineNo = 1;

			var optionCollection = new CodeDescriptionOptionCollectionParent(Factory, invoiceLine.CargoAttributes).OptionCollection;
			optionCollection.SelectedCodes = new List<ZString>
			{
				"18"
			};
			optionCollection.RefreshSelectionCollection();

			var actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			var expectedKeys = new IZType[]
			{
				(ZInt)100010,
				(ZInt)200020,
				new ZString("035"),
				new ZString("002"),
				new ZString("UQ"),
				new ZString("USD"),
				new ZString("D"),
				new ZString("PQ1|NUM1|1|UQ"),
				new ZString("CN"),
				new ZString("US"),
				new ZString("AAAAA"),
				new ZString("999"),
				new ZString("1|3|GTIN|CAS"),
				new ZString("BBBBB"),
				new ZString("1|3||"),
				new ZString("CCC"),
				new ZString("DDD"),
				new ZString("EEE"),
				new ZString("FFF"),
				new ZString("GGG"),
				new ZString(Constants.PrimaryPreferenceCodes.FreeTradeAgreement),
				new ZString("01"),
				new ZString("123456789"),
				new ZInt(2),
				new ZString("CN"),
				new ZString("C"),
				new ZString("III"),
				new ZString("JJ"),
				new ZString("首次进出口"),
				new ZString("Ingredient Note"),
				new ZInt(12),
				manufacturerPK,
				new ZString("0000B"),
				new ZBool(true),
				new ZString("1A1"),
				new ZString("SPEC"),
				new ZString("MODEL"),
				new ZString("BRAND"),
			};
			AssertArrayEqualsByElements(expectedKeys, actualKeys.Skip(actualKeys.Count - expectedKeys.Length).ToArray());

			invoiceLine.JI_NonDangerousChemicalFlag = true;
			invoiceLine.JI_PackageTypeOfUNDG = "1A1";
			actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			expectedKeys = new IZType[]
			{
				(ZInt)100010,
				(ZInt)200020,
				new ZString("035"),
				new ZString("002"),
				new ZString("UQ"),
				new ZString("USD"),
				new ZString("D"),
				new ZString("PQ1|NUM1|1|UQ"),
				new ZString("CN"),
				new ZString("US"),
				new ZString("AAAAA"),
				new ZString("999"),
				new ZString("1|3|GTIN|CAS"),
				new ZString("BBBBB"),
				new ZString("1|3||"),
				new ZString("CCC"),
				new ZString("DDD"),
				new ZString("EEE"),
				new ZString("FFF"),
				new ZString("GGG"),
				new ZString(Constants.PrimaryPreferenceCodes.FreeTradeAgreement),
				new ZString("01"),
				new ZString("123456789"),
				new ZInt(2),
				new ZString("CN"),
				new ZString("C"),
				new ZString("III"),
				new ZString("JJ"),
				new ZString("首次进出口"),
				new ZString("Ingredient Note"),
				new ZInt(12),
				manufacturerPK,
				new ZString("0000B"),
				new ZBool(true),
				new ZString("1A1"),
				new ZString("SPEC"),
				new ZString("MODEL"),
				new ZString("BRAND"),
			};
			AssertArrayEqualsByElements(expectedKeys, actualKeys.Skip(actualKeys.Count - expectedKeys.Length).ToArray());

			invoiceLine.JI_CIQTariff = "";
			testData.EntryInstruction.CEI_CIQRequires = false;
			actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			expectedKeys = new IZType[]
			{
				(ZInt)100010,
				(ZInt)200020,
				new ZString("035"),
				new ZString("002"),
				new ZString("UQ"),
				new ZString("USD"),
				new ZString("D"),
				ZString.Empty,
				new ZString("CN"),
				new ZString("US"),
				new ZString("AAAAA"),
				new ZString("999"),
				new ZString("1|3|GTIN|CAS"),
				new ZString("BBBBB"),
				new ZString("1|3||"),
				new ZString("CCC"),
				new ZString("DDD"),
				new ZString("EEE"),
				new ZString("FFF"),
				new ZString("GGG"),
				new ZString(Constants.PrimaryPreferenceCodes.FreeTradeAgreement),
				new ZString("01"),
				new ZString("123456789"),
				new ZInt(2),
				new ZString("CN"),
				new ZString("C"),
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZInt.Zero,
				ZGuid.Empty,
				ZString.Empty,
				ZBool.False,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
			};
			AssertArrayEqualsByElements(expectedKeys, actualKeys.Skip(actualKeys.Count - expectedKeys.Length).ToArray());
			invoiceLine.JI_CIQTariff = "III";
			testData.EntryInstruction.CEI_CIQRequires = true;

			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			expectedKeys = new IZType[]
			{
				(ZInt)100010,
				ZInt.Zero,
				new ZString("035"),
				new ZString("002"),
				new ZString("UQ"),
				new ZString("USD"),
				new ZString("D"),
				new ZString("PQ1|NUM1|1|UQ"),
				new ZString("CN"),
				new ZString("US"),
				new ZString("AAAAA"),
				new ZString("999"),
				new ZString("1|3|GTIN|CAS"),
				ZString.Empty,
				ZString.Empty,
				new ZString("CCC"),
				ZString.Empty,
				ZString.Empty,
				new ZString("FFF"),
				new ZString("GGG"),
				new ZString(Constants.PrimaryPreferenceCodes.FreeTradeAgreement),
				new ZString("01"),
				new ZString("123456789"),
				new ZInt(2),
				new ZString("CN"),
				new ZString("C"),
				new ZString("III"),
				new ZString("JJ"),
				new ZString("首次进出口"),
				new ZString("Ingredient Note"),
				new ZInt(12),
				manufacturerPK,
				new ZString("0000B"),
				new ZBool(true),
				new ZString("1A1"),
				new ZString("SPEC"),
				new ZString("MODEL"),
				new ZString("BRAND"),
			};
			AssertArrayEqualsByElements(expectedKeys, actualKeys.Skip(actualKeys.Count - expectedKeys.Length).ToArray());

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			expectedKeys = new IZType[]
			{
				(ZInt)100010,
				ZInt.Zero,
				new ZString("035"),
				new ZString("002"),
				new ZString("UQ"),
				new ZString("USD"),
				new ZString("D"),
				new ZString("PQ1|NUM1|1|UQ"),
				new ZString("CN"),
				new ZString("US"),
				new ZString("AAAAA"),
				new ZString("999"),
				new ZString("1|3|GTIN|CAS"),
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				new ZString("123456789"),
				new ZInt(2),
				new ZString("CN"),
				new ZString("C"),
				new ZString("III"),
				new ZString("JJ"),
				new ZString("首次进出口"),
				new ZString("Ingredient Note"),
				new ZInt(12),
				manufacturerPK,
				new ZString("0000B"),
				new ZBool(true),
				new ZString("1A1"),
				new ZString("SPEC"),
				new ZString("MODEL"),
				new ZString("BRAND"),
			};
			AssertArrayEqualsByElements(expectedKeys, actualKeys.Skip(actualKeys.Count - expectedKeys.Length).ToArray());

			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			expectedKeys = new IZType[]
			{
				(ZInt)100010,
				ZInt.Zero,
				new ZString("035"),
				new ZString("002"),
				new ZString("UQ"),
				new ZString("USD"),
				new ZString("D"),
				new ZString("PQ1|NUM1|1|UQ"),
				new ZString("CN"),
				new ZString("US"),
				new ZString("AAAAA"),
				new ZString("999"),
				new ZString("1|3|GTIN|CAS"),
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				new ZString("123456789"),
				new ZInt(2),
				new ZString("CN"),
				new ZString("C"),
				new ZString("III"),
				new ZString("JJ"),
				new ZString("首次进出口"),
				new ZString("Ingredient Note"),
				new ZInt(12),
				manufacturerPK,
				new ZString("0000B"),
				new ZBool(true),
				new ZString("1A1"),
				new ZString("SPEC"),
				new ZString("MODEL"),
				new ZString("BRAND"),
			};
			AssertArrayEqualsByElements(expectedKeys, actualKeys.Skip(actualKeys.Count - expectedKeys.Length).ToArray());
		}

		public void TestGetKeyForLineMergeRuleSelections()
		{
			var items = CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import, DecTypeList.Codes.CustomsEntry);
			var declaration = items.JobDeclaration;
			var strategy = new ParentEntryCreationStrategy(declaration, EntryTypeList.Codes.CustomsEntry);
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00000", "00423", "00352", "99999");
			helper.CreateAdditionalElement("00423", "针入度");
			helper.CreateAdditionalElement("00352", "加工方法");
			invoiceLine.JI_Tariff = "2713200000";
			invoiceLine.XC_GoodsSpecModel2 = "|DDDDD";
			invoiceLine.XC_GoodsSpecModel = "CCCCC||无必报要素";

			invoiceLine.JI_ProductVersion = "PV0001";
			invoiceLine.TradeUnitPrice = 100.00m;
			invoiceLine.JI_Description = "Description";

			var mergeKeys = strategy.GetKeyForLine(invoiceLine);

			AssertEquals(true, !mergeKeys.Contains(new ZString("PV0001")));
			AssertEquals(true, !mergeKeys.Contains(new ZDecimal(100.00m)));
			AssertEquals(true, !mergeKeys.Contains(new ZString("|DDDDD|")));
			AssertEquals(true, !mergeKeys.Contains(new ZString("CCCCC||无必报要素")));

			declaration.MergingRuleOptions[0].Selected = true;
			declaration.MergingRuleOptions[1].Selected = true;
			declaration.MergingRuleOptions[2].Selected = true;
			declaration.MergingRuleOptions[3].Selected = true;

			mergeKeys = strategy.GetKeyForLine(invoiceLine);
			AssertEquals(true, mergeKeys.Contains(new ZString("PV0001")));
			AssertEquals(true, mergeKeys.Contains(new ZDecimal(100.00m)));
			AssertEquals(true, !mergeKeys.Contains(new ZString("|DDDDD|")));
			AssertEquals(true, mergeKeys.Contains(new ZString("CCCCC||无必报要素")));
		}

		public void TestMergeImportCustomsEntry()
		{
			var items = CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import, DecTypeList.Codes.CustomsEntry);
			items.EntryInstruction.CEI_ManualNo = "D0000110101";

			var invoiceLine11 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = items.EntryInstruction.PK;
			invoiceLine11.JI_Tariff = "T1001";

			var instruction2 = items.JobDeclaration.CustomsEntryInstructions.AddNew();
			var invoiceLine12 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine12.JI_CEI = instruction2.PK;
			invoiceLine12.JI_Tariff = "T1002";

			var invoiceLine13 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine13.JI_CEI = items.EntryInstruction.PK;
			invoiceLine13.JI_Tariff = "T1001";

			var invoiceLine14 = (JobComInvoiceLine)items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine14.JI_CEI = items.EntryInstruction.PK;
			invoiceLine14.JI_Tariff = "T1001";
			invoiceLine14.JI_ProductManualNo = 10001;

			var invoiceLine15 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine15.JI_CEI = items.EntryInstruction.PK;
			invoiceLine15.JI_Tariff = "T1002";

			var invoice2 = items.JobDeclaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "CNY";
			var invoiceLine21 = invoice2.InvoiceLines.AddNew();
			invoiceLine21.JI_CEI = items.EntryInstruction.PK;

			var invoiceLine16 = (JobComInvoiceLine)items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine16.JI_CEI = items.EntryInstruction.PK;
			invoiceLine16.JI_Tariff = "T1001";
			invoiceLine16.JI_TradeUnitQty = "035";

			var invoiceLine17 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine17.JI_CEI = items.EntryInstruction.PK;
			invoiceLine17.JI_Tariff = "T1001";
			invoiceLine17.JI_CountryOfOrigin = "NJ";

			var invoiceLine18 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine18.JI_CEI = items.EntryInstruction.PK;
			invoiceLine18.JI_Tariff = "T1001";
			invoiceLine18.JI_RN_NKCountryOfExport = "NY";

			var invoiceLine19 = (JobComInvoiceLine)items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine19.JI_CEI = items.EntryInstruction.PK;
			invoiceLine19.JI_Tariff = "T1001";
			invoiceLine19.JI_DutyMode = "D";

			items.JobDeclaration.DoMerge();

			var entryHeaders = items.JobDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
			AssertEquals("There should be 2 Entry Headers for there are 2 Instructions.", 2, entryHeaders.Length);
			AssertEquals("Entries should all be CUS.", true, entryHeaders.All(entry => entry.CH_MessageType == EntryTypeList.Codes.CustomsEntry));
			AssertEquals("Line 17 should NOT be merged after WI00251175, JI_CountryOfOrigin will be part of MergeKeys even for CUS IMP Jobs.", true, entryHeaders.Any(entry => entry.MergedLines.Count == 8));
			AssertEquals("Lines 12,15 should be merged.", true, entryHeaders.Any(entry => entry.MergedLines.Count == 1));
		}

		public void TestMergeExportCustomsEntry()
		{
			var items = CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Export, DecTypeList.Codes.CustomsEntry);

			var invoiceLine11 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = items.EntryInstruction.PK;
			invoiceLine11.JI_Tariff = "T1001";
			invoiceLine11.JI_RN_NKCountryOfExport = "NY";

			var invoiceLine12 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine12.JI_CEI = items.EntryInstruction.PK;
			invoiceLine12.JI_Tariff = "T1001";
			invoiceLine12.JI_RN_NKCountryOfExport = "";

			items.JobDeclaration.DoMerge();

			var entryHeaders = items.JobDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
			AssertEquals(1, entryHeaders.Length);
			AssertEquals(true, entryHeaders.All(entry => entry.CH_MessageType == EntryTypeList.Codes.CustomsEntry));
			AssertEquals(2, entryHeaders.First().MergedLines.Count);
		}

		public void TestMergeExportRecordListing()
		{
			var items = CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Export, DecTypeList.Codes.RecordListing);
			items.EntryInstruction.CEI_ManualNo = "D0000110101";

			var invoiceLine11 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = items.EntryInstruction.PK;
			invoiceLine11.JI_Tariff = "T1001";

			var instruction2 = items.JobDeclaration.CustomsEntryInstructions.AddNew();
			var invoiceLine12 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine12.JI_CEI = instruction2.PK;
			invoiceLine12.JI_Tariff = "T1002";

			var invoiceLine13 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine13.JI_CEI = items.EntryInstruction.PK;
			invoiceLine13.JI_Tariff = "T1001";

			var invoiceLine14 = (JobComInvoiceLine)items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine14.JI_CEI = items.EntryInstruction.PK;
			invoiceLine14.JI_Tariff = "T1001";
			invoiceLine14.JI_ProductManualNo = 10001;

			var invoiceLine15 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine15.JI_CEI = items.EntryInstruction.PK;
			invoiceLine15.JI_Tariff = "T1002";

			var invoice2 = items.JobDeclaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "CNY";
			var invoiceLine21 = invoice2.InvoiceLines.AddNew();
			invoiceLine21.JI_CEI = items.EntryInstruction.PK;

			var invoiceLine16 = (JobComInvoiceLine)items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine16.JI_CEI = items.EntryInstruction.PK;
			invoiceLine16.JI_Tariff = "T1001";
			invoiceLine16.JI_TradeUnitQty = "035";

			var invoiceLine17 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine17.JI_CEI = items.EntryInstruction.PK;
			invoiceLine17.JI_Tariff = "T1001";
			invoiceLine17.JI_CountryOfOrigin = "NJ";

			var invoiceLine18 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine18.JI_CEI = items.EntryInstruction.PK;
			invoiceLine18.JI_Tariff = "T1001";
			invoiceLine18.JI_RN_NKCountryOfExport = "NY";

			var invoiceLine19 = (JobComInvoiceLine)items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine19.JI_CEI = items.EntryInstruction.PK;
			invoiceLine19.JI_Tariff = "T1001";
			invoiceLine19.JI_DutyMode = "D";

			items.JobDeclaration.DoMerge();

			var entryHeaders = items.JobDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
			AssertEquals("There should be 2 Entry Headers.", 2, entryHeaders.Length);
			AssertEquals("Both of the entry header should be REC.", true, entryHeaders.All(entry => entry.CH_MessageType == EntryTypeList.Codes.RecordListing));
			AssertEquals("Line 18 should NOT be merged after WI00251175, JI_RN_NKCountryOfExport will be part of MergeKeys even for EXP REC Jobs.", true, entryHeaders.Any(entry => entry.MergedLines.Count == 8));
			AssertEquals("Lines 12,15 should be merged.", true, entryHeaders.Any(entry => entry.MergedLines.Count == 1));
		}

		public void TestMergeImportRecordListing()
		{
			var items = CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import, DecTypeList.Codes.RecordListing);

			var invoiceLine11 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = items.EntryInstruction.PK;
			invoiceLine11.JI_Tariff = "T1001";
			invoiceLine11.JI_CountryOfOrigin = "NJ";

			var invoiceLine12 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine12.JI_CEI = items.EntryInstruction.PK;
			invoiceLine12.JI_Tariff = "T1001";
			invoiceLine12.JI_CountryOfOrigin = "";

			items.JobDeclaration.DoMerge();

			var entryHeaders = items.JobDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
			AssertEquals(1, entryHeaders.Length);
			AssertEquals(true, entryHeaders.All(entry => entry.CH_MessageType == EntryTypeList.Codes.RecordListing));
			AssertEquals(2, entryHeaders.First().MergedLines.Count);
		}

		public void TestMergeImportBoth()
		{
			var items = CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import, DecTypeList.Codes.Both);
			var childInstruction = items.JobDeclaration.CustomsEntryInstructions.AddNew();

			var invoiceLine11 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = items.EntryInstruction.PK;
			invoiceLine11.JI_Tariff = "T1001";

			var invoiceLine12 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine12.JI_CEI = items.EntryInstruction.PK;
			invoiceLine12.JI_Tariff = "T1001";
			invoiceLine12.JI_RN_NKCountryOfExport = "NJ";

			items.JobDeclaration.DoMerge();

			var entryHeaders = items.JobDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
			AssertEquals(2, entryHeaders.Length);

			var cusEntryHeader = entryHeaders.First(entry => entry.CH_MessageType == EntryTypeList.Codes.CustomsEntry);
			var recEntryHeader = entryHeaders.First(entry => entry.CH_MessageType == EntryTypeList.Codes.RecordListing);
			AssertEquals(items.EntryInstruction.PK, cusEntryHeader.CH_CEI_Instruction);
			AssertEquals(childInstruction.PK, recEntryHeader.CH_CEI_Instruction);

			AssertEquals(2, cusEntryHeader.MergedLines.Count);
			AssertEquals(2, recEntryHeader.MergedLines.Count);

			AssertNotNull("CUS Entry Line should be stored on JI_CL", cusEntryHeader.MergedLines.FindByPK(invoiceLine11.JI_CL));
			AssertNotNull("CUS Entry Line should be stored on JI_CL", cusEntryHeader.MergedLines.FindByPK(invoiceLine12.JI_CL));
			AssertNotNull("REC Entry Line should be stored on BU_CL", recEntryHeader.MergedLines.FindByPK(invoiceLine11.AdditionalEntryLineLinks.First().BU_CL));
			AssertNotNull("REC Entry Line should be stored on BU_CL", recEntryHeader.MergedLines.FindByPK(invoiceLine12.AdditionalEntryLineLinks.First().BU_CL));
		}

		public void TestMergeExportBoth()
		{
			var items = CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Export, DecTypeList.Codes.Both);
			var childInstruction = items.JobDeclaration.CustomsEntryInstructions.AddNew();

			var invoiceLine11 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = items.EntryInstruction.PK;
			invoiceLine11.JI_Tariff = "T1001";

			var invoiceLine12 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine12.JI_CEI = items.EntryInstruction.PK;
			invoiceLine12.JI_Tariff = "T1001";
			invoiceLine12.JI_RN_NKCountryOfExport = "NJ";

			items.JobDeclaration.DoMerge();

			var entryHeaders = items.JobDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
			AssertEquals(2, entryHeaders.Length);

			var cusEntryHeader = entryHeaders.First(entry => entry.CH_MessageType == EntryTypeList.Codes.CustomsEntry);
			var recEntryHeader = entryHeaders.First(entry => entry.CH_MessageType == EntryTypeList.Codes.RecordListing);
			AssertEquals(items.EntryInstruction.PK, recEntryHeader.CH_CEI_Instruction);
			AssertEquals(childInstruction.PK, cusEntryHeader.CH_CEI_Instruction);

			AssertEquals(2, cusEntryHeader.MergedLines.Count);
			AssertEquals(2, recEntryHeader.MergedLines.Count);

			AssertNotNull("REC Entry Line should be stored on JI_CL", recEntryHeader.MergedLines.FindByPK(invoiceLine11.JI_CL));
			AssertNotNull("REC Entry Line should be stored on JI_CL", recEntryHeader.MergedLines.FindByPK(invoiceLine12.JI_CL));
			AssertNotNull("CUS Entry Line should be stored on BU_CL", cusEntryHeader.MergedLines.FindByPK(invoiceLine11.AdditionalEntryLineLinks.First().BU_CL));
			AssertNotNull("CUS Entry Line should be stored on BU_CL", cusEntryHeader.MergedLines.FindByPK(invoiceLine12.AdditionalEntryLineLinks.First().BU_CL));
		}

		public void TestDeletingAfterRemerging()
		{
			var items = CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import, DecTypeList.Codes.Both);
			items.JobDeclaration.CustomsEntryInstructions.AddNew();

			var invoiceLine11 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = items.EntryInstruction.PK;
			invoiceLine11.JI_Tariff = "T1001";

			var invoiceLine12 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine12.JI_CEI = items.EntryInstruction.PK;
			invoiceLine12.JI_Tariff = "T1002";

			var invoiceLine13 = items.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine13.JI_CEI = items.EntryInstruction.PK;
			invoiceLine13.JI_Tariff = "T1003";

			items.JobDeclaration.DoMerge();
			var entryHeaders = items.JobDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
			AssertEquals(2, entryHeaders.Length);
			var cusEntryHeader = entryHeaders.First(entry => entry.CH_MessageType == EntryTypeList.Codes.CustomsEntry);
			var recEntryHeader = entryHeaders.First(entry => entry.CH_MessageType == EntryTypeList.Codes.RecordListing);

			items.JobDeclaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			items.JobDeclaration.DoMerge();

			entryHeaders = items.JobDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
			AssertEquals(1, entryHeaders.Length);
			AssertEquals("REC entry deleted", true, recEntryHeader.IsDeleted);
			AssertSame("CUS entry reused", cusEntryHeader, entryHeaders.First(entry => entry.CH_MessageType == EntryTypeList.Codes.CustomsEntry));
			AssertEquals(3, entryHeaders.First(entry => entry.CH_CEI_Instruction == items.EntryInstruction.PK).MergedLines.Count);

			items.JobDeclaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			items.JobDeclaration.DoMerge();
			AssertEquals("CUS entry deleted", true, cusEntryHeader.IsDeleted);
			entryHeaders = items.JobDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToArray();
			AssertEquals(1, entryHeaders.Length);
			AssertEquals(true, entryHeaders.All(entry => entry.CH_MessageType == EntryTypeList.Codes.RecordListing));
			AssertEquals(3, entryHeaders.First(entry => entry.CH_CEI_Instruction == items.EntryInstruction.PK).MergedLines.Count);
		}

		public override void TestIsEntryHeaderValidToBeReused()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			invoiceLine1.JI_CEI = instruction1.PK;
			invoiceLine2.JI_CEI = instruction2.PK;

			declaration.DoMerge();

			AssertEquals(2, declaration.ActiveEntryHeaders.Count);

			var entry1 = declaration.ActiveEntryHeaders[0];
			var entry2 = declaration.ActiveEntryHeaders[1];

			entry2.ManuallySetEntryNumber(CusEntryNumberTypes.China.DeclarationUnifiedNumber, "UNI2020061801");

			var instruction3 = declaration.CustomsEntryInstructions.AddNew();
			var instruction4 = declaration.CustomsEntryInstructions.AddNew();

			invoiceLine1.JI_CEI = instruction3.PK;
			invoiceLine2.JI_CEI = instruction4.PK;

			declaration.DoMerge();

			AssertEquals(3, declaration.ActiveEntryHeaders.Count);

			var entry3 = declaration.ActiveEntryHeaders[0];
			var entry4 = declaration.ActiveEntryHeaders[1];
			var entry5 = declaration.ActiveEntryHeaders[2];

			AssertSame(entry1, entry3);
			AssertSame(entry2, entry4);
			AssertEquals("The Entry Header with empty Declaration Unified Number should be reused.", "", entry3.DeclarationUnifiedNumber);
			AssertEquals("The Entry Header with non-empty Declaration Unified Number should not be reused.", "UNI2020061801", entry4.DeclarationUnifiedNumber);
			AssertEquals("A new Entry Header will be generated with a new Local Reference Number and empty Declaration Unified Number.", "", entry5.DeclarationUnifiedNumber);
		}

		internal static CNEntryHeaderTestData CreateDeclarationWithInstruction(BusinessObjectFactory factory, ZString messageType, ZString messageSubtype)
		{
			var declaration = factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = messageType;
			declaration.JE_MessageSubType = messageSubtype;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice1 = declaration.Invoices.AddNew();

			return new CNEntryHeaderTestData(declaration, null, null, invoice1, null, instruction1);
		}
	}
}
