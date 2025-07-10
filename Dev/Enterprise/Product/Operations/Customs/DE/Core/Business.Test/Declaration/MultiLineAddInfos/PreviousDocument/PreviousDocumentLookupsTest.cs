using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class PreviousDocumentLookupsTest : EU.Business.Declaration.MultiLineAddInfos.Testing.PreviousDocumentLookupsTest
	{
		public void TestProcedureList_Import()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var procedureList = lookups.ProcedureList;
			CombineAssertions(() =>
			{
				AssertType<PreviousProcedureList>("Import List", procedureList);
				AssertSame("Cached", procedureList, lookups.ProcedureList);
			});
		}

		public void TestProcedureList()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var expectedResult = new CodeDescriptionPairList();
			expectedResult.AddPair(PreviousProcedureList.Codes._ATAV, PreviousProcedureList.Descriptions._ATAV);
			expectedResult.AddPair(PreviousProcedureList.Codes._ATZL, PreviousProcedureList.Descriptions._ATZL);
			var procedureList = previousDocument.Lookups.ProcedureList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Export List", expectedResult, procedureList);
				AssertSame("Cached", procedureList, previousDocument.Lookups.ProcedureList);
			});
		}

		public void TestSubTypeList_Import()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var list = lookups.SubTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodeAsString", "AWB, REG, ULD", list.CodesAsString);
				AssertSame("Cached", list, lookups.SubTypeList);
			});
		}

		public void TestSubTypeList_Import_ATNEU()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var notAirCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE185103", "BERLIN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var notAirAttribute = helper.CreateCusCodeListAttribute(notAirCodeList.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "DEP");
			helper.CreateTransportModeForCusCodeAttribute(notAirAttribute.PK, RefTransportModeList.Codes.SEA);

			var airCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE185104", "SHYZAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var airAttribute = helper.CreateCusCodeListAttribute(airCodeList.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "DES");
			helper.CreateTransportModeForCusCodeAttribute(airAttribute.PK, RefTransportModeList.Codes.AIR);
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;

			CombineAssertions(() =>
			{
				declaration.JE_CustomsOffice = "DE185103";
				AssertEquals("SubTypeList when isOfficeAir is False", "REG", lookups.SubTypeList.CodesAsString);
				AssertSame("Cached", lookups.SubTypeList, lookups.SubTypeList);

				declaration.JE_CustomsOffice = "DE185104";
				AssertEquals("SubTypeList when isOfficeAir is True", "AWB, REG, ULD", lookups.SubTypeList.CodesAsString);
			});
		}

		public void TestStatusList()
		{
			var list = lookups.StatusList;
			CombineAssertions(() =>
			{
				AssertEquals("CodeAsString", "N, Y", list.CodesAsString);
				AssertSame("Cached", list, lookups.StatusList);
			});
		}

		public void TestUnitOfQuantityList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "de", eun);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", "Kilograms", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "NA", "NA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var list = lookups.UnitOfQuantityList;
			CombineAssertions(() =>
			{
				AssertEquals("CodeAsString", "KG", list.CodesAsString);
				AssertSame("Cached", list, lookups.UnitOfQuantityList);
			});
		}

		public void TestUnitOfQuantityList_ExportInvoiceLine_PreviousDocuments()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			AssertUnitOfQuantityList_PreviousDocuments(previousDocument);
		}

		public void TestUnitOfQuantityList_ExportCusClassPartPivot_PreviousDocuments()
		{
			var (_, previousDocument) = PreviousDocumentTest.CreatePreviousDocumentWithPivotParent(Factory);
			AssertDocumentCodeList_PreviousDocuments(previousDocument);
		}

		public void TestUnitOfQuantity2List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", "Kilograms", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var list = lookups.UnitOfQuantity2List;
			CombineAssertions(() =>
			{
				AssertEquals("CodeAsString", "KG", list.CodesAsString);
				AssertSame("Cached", list, lookups.UnitOfQuantity2List);
			});
		}

		public void TestDocumentCodeList_Export_InvoiceHeader()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, "AI44E");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E, "DC40E");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E, Core.Constants.CountryCodes.Germany);

			var countryCode = Core.Constants.CountryCodes.Germany;
			var refCode = UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E;
			var code1 = helper.CreateNewOrGetExistingCusCodeList(countryCode, refCode, "DE01", "DE01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(countryCode, refCode, "DE02", "DE02 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(countryCode, refCode, "DE03", "Invalid attribute", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(countryCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AA44E, "DE04", "Invalid type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code5 = helper.CreateNewOrGetExistingCusCodeList(countryCode, refCode, "DE05", "Invalid date", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddMonths(-1));
			var code6 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, refCode, "IT01", "Invalid grouping", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code7 = helper.CreateNewOrGetExistingCusCodeList(countryCode, refCode, SupportingDocumentTypes._9ZZY, "Code will get filtered", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code8 = helper.CreateNewOrGetExistingCusCodeList(countryCode, refCode, SupportingDocumentTypes._9ZZX, "Code will get filtered", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code5.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code6.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code7.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code8.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header);
			Factory.Save();

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var previousDocument = invoiceHeader.PreviousDocuments.AddNew();
			var list = previousDocument.Lookups.DocumentCodeList;
			list.Load();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "DE01", "DE02" }, list.Select(x => x.ZZD_Code));
		}

		public void TestDocumentCodeList_ExportInvoiceLine_PreviousDocuments()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			AssertDocumentCodeList_PreviousDocuments(previousDocument);
		}

		public void TestDocumentCodeList_ExportCusClassPartPivot_PreviousDocuments()
		{
			var (_, previousDocument) = PreviousDocumentTest.CreatePreviousDocumentWithPivotParent(Factory);
			AssertDocumentCodeList_PreviousDocuments(previousDocument);
		}

		public void TestInvoiceLineNumberList()
		{
			var anotherCusEntryInstructionPK = declaration.CustomsEntryInstructions.AddNew().PK;
			var cusEntryInstructionPK = previousDocument.Parent.PK;
			invoiceHeader.JZ_InvoiceNumber = "XYZ001";
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CEI = anotherCusEntryInstructionPK;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_CEI = cusEntryInstructionPK;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "ABC123";
			var invoiceLine3 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine3.JI_LineNo = 1;
			invoiceLine3.JI_CEI = cusEntryInstructionPK;

			AssertArrayEqualsByElements("", new[]
				{
					new CodeDescriptionPair("1", "Inv. No. ABC123"),
					new CodeDescriptionPair("2", "Inv. No. XYZ001")
				}, previousDocument.Lookups.InvoiceLineNumberList.ToArray());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			previousDocument = declaration.CustomsEntryInstructions.AddNew().PreviousDocuments.AddNew();
			lookups = new PreviousDocumentLookups(previousDocument);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		PreviousDocument previousDocument;
		PreviousDocumentLookups lookups;

		void AssertDocumentCodeList_PreviousDocuments(PreviousDocument previousDocument)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E, "DC40E");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E, Core.Constants.CountryCodes.Germany);

			var countryCode = Core.Constants.CountryCodes.Germany;
			var refCode = UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E;
			var code1 = helper.CreateNewOrGetExistingCusCodeList(countryCode, refCode, "DE01", "DE01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(countryCode, refCode, "DE02", "DE02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);

			var code3 = helper.CreateNewOrGetExistingCusCodeList(countryCode, refCode, SupportingDocumentTypes._9ZZY, "Code will get filtered", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(countryCode, refCode, SupportingDocumentTypes._9ZZX, "Code will get filtered", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code3.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			Factory.Save();

			var list = previousDocument.Lookups.DocumentCodeList;
			list.Load();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "DE02" }, list.Select(x => x.ZZD_Code));
		}

		void AssertUnitOfQuantityList_PreviousDocuments(PreviousDocument previousDocument)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E, "DC40E", Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit, "MeasurementUnit",
				UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E, Core.Constants.CountryCodes.Germany);

			var codeWithoutAttributeUOM = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, "DC40E", "C621", "C621", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithoutAttributeUOM.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			var codeWithAttributeUOM = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, "DC40E", "C651", "C651", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithAttributeUOM.PK, UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit, "MeasurementUnit");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithAttributeUOM.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "MIL", "Kilograms", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KLT", "Kilograms", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGM", "Kilograms", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var list = previousDocument.Lookups.UnitOfQuantityList;
			AssertNullOrEmpty(list.CodesAsString);

			previousDocument.CSI_Code = "C621";
			AssertNullOrEmpty(previousDocument.Lookups.UnitOfQuantityList.CodesAsString);

			previousDocument.CSI_Code = "C651";
			AssertEquals("CodeAsString", "KGM, KLT, MIL", previousDocument.Lookups.UnitOfQuantityList.CodesAsString);
		}
	}
}
