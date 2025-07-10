using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Customs.ES.Business.UniversalReferenceConstants.RefCusCodeListTypes;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class PreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUOMList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DEF", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateCusCodeList(CountryCodes.Spain, "ZZ", "AH3", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateCusCodeList("DGC", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "GHI", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "JKL", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var previousDoc = invLine.PreviousDocuments.AddNew();
			AssertEquals("ABC, DEF", previousDoc.Lookups.CustomsUQList.CodesAsString);
		}

		public void TestCodeList()
		{
			var esCode = CountryCodes.Spain;
			var dc40ACode = RefCusCodeListTypes.DC40A;
			var dc40ICode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection;
			var dc40ECode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection;
			var dc40WCode = RefCusCodeListTypes.DC40W;
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var code1 = helper.CreateNewOrGetExistingCusCodeList(esCode, dc40ACode, "Code1", "Description 1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var code2 = helper.CreateNewOrGetExistingCusCodeList(esCode, dc40ACode, "Code2", "Description 2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var code3 = helper.CreateNewOrGetExistingCusCodeList(esCode, dc40ICode, "Code3", "Description 3", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var code4 = helper.CreateNewOrGetExistingCusCodeList(esCode, dc40ECode, "Code4", "Description 4", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var code5 = helper.CreateNewOrGetExistingCusCodeList(esCode, dc40WCode, "Code5", "Description 5", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

			void AssertCodes(string message, ZQuery filter, bool code1Visible, bool code2Visible, bool code3Visible, bool code4Visible, bool code5Visible)
			{
				AssertEquals($"Code1 {message}", code1Visible, Factory.Load<ZZRefCusCodeListCombined>(code1.PK).MatchesFilter(filter));
				AssertEquals($"Code2 {message}", code2Visible, Factory.Load<ZZRefCusCodeListCombined>(code2.PK).MatchesFilter(filter));
				AssertEquals($"Code3 {message}", code3Visible, Factory.Load<ZZRefCusCodeListCombined>(code3.PK).MatchesFilter(filter));
				AssertEquals($"Code4 {message}", code4Visible, Factory.Load<ZZRefCusCodeListCombined>(code4.PK).MatchesFilter(filter));
				AssertEquals($"Code5 {message}", code5Visible, Factory.Load<ZZRefCusCodeListCombined>(code5.PK).MatchesFilter(filter));
			}
			Factory.Save();

			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				var previousDocument = invLine.PreviousDocuments.AddNew();

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;

				var invoice2 = declaration.Invoices.AddNew();
				var invLine2 = invoice2.InvoiceLines.AddNew();
				invLine2.JI_CEI = entryInstruction.PK;
				var previousDocInvLine = invLine2.PreviousDocuments.AddNew();
				var previousDocInv = invoice2.PreviousDocuments.AddNew();
				var previousDocDec = declaration.PreviousDocuments.AddNew();

				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					var list = (ZZRefCusCodeListCombinedCollection)previousDocument.Lookups.CodeList;
					AssertCodes("InvLine equals export and AES", list.CompleteFilter, true, true, false, false, false);

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					list = (ZZRefCusCodeListCombinedCollection)previousDocument.Lookups.CodeList;
					AssertCodes("InvLine equals import and AES", list.CompleteFilter, false, false, true, false, false);

					list = (ZZRefCusCodeListCombinedCollection)previousDocInvLine.Lookups.CodeList;
					AssertCodes("InvLine is H2 and AES", list.CompleteFilter, false, false, false, false, true);

					list = (ZZRefCusCodeListCombinedCollection)previousDocInv.Lookups.CodeList;
					AssertCodes("Invoice is H2 and AES", list.CompleteFilter, false, false, false, false, true);

					list = (ZZRefCusCodeListCombinedCollection)previousDocDec.Lookups.CodeList;
					AssertCodes("Declaration is H2 and AES", list.CompleteFilter, false, false, false, false, true);
				}

				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					var list = (ZZRefCusCodeListCombinedCollection)previousDocument.Lookups.CodeList;
					AssertCodes("InvLine equals export and AES1.1", list.CompleteFilter, true, true, false, false, false);

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					list = (ZZRefCusCodeListCombinedCollection)previousDocument.Lookups.CodeList;
					AssertCodes("InvLine equals import and AES1.1", list.CompleteFilter, false, false, true, false, false);

					list = (ZZRefCusCodeListCombinedCollection)previousDocInvLine.Lookups.CodeList;
					AssertCodes("InvLine is H2 and AES1.1", list.CompleteFilter, false, false, false, false, true);

					list = (ZZRefCusCodeListCombinedCollection)previousDocInv.Lookups.CodeList;
					AssertCodes("Invoice is H2 and AES1.1", list.CompleteFilter, false, false, false, false, true);

					list = (ZZRefCusCodeListCombinedCollection)previousDocDec.Lookups.CodeList;
					AssertCodes("Declaration is H2 and AES1.1", list.CompleteFilter, false, false, false, false, true);
				}
			});
		}

		public void TestCodeListT2LAndT2C()
		{
			var esCode = CountryCodes.Spain;
			var dc40ACode = RefCusCodeListTypes.DC40A;
			var dc40ICode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection;
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var code1 = helper.CreateNewOrGetExistingCusCodeList(esCode, dc40ACode, "Code1", "Description 1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var code2 = helper.CreateCusCodeListWithAttribute(esCode, dc40ACode, "Code2", "Description 2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "IsT2LPous", "Y");
			var code3 = helper.CreateNewOrGetExistingCusCodeList(esCode, dc40ICode, "Code3", "Description 3", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

			void AssertAll(string message, bool code1Visible, bool code2Visible, bool code3Visible, params PreviousDocument[] prevDocs)
			{
				foreach (var prevDoc in prevDocs)
				{
					var name = prevDoc.Parent.GetType().Name;
					var filter = ((ZZRefCusCodeListCombinedCollection)prevDoc.Lookups.CodeList).CompleteFilter;
					AssertEquals($"{name} {message} Code1", code1Visible, Factory.Load<ZZRefCusCodeListCombined>(code1.PK).MatchesFilter(filter));
					AssertEquals($"{name} {message} Code2", code2Visible, Factory.Load<ZZRefCusCodeListCombined>(code2.PK).MatchesFilter(filter));
					AssertEquals($"{name} {message} Code3", code3Visible, Factory.Load<ZZRefCusCodeListCombined>(code3.PK).MatchesFilter(filter));
				}
			}

			Factory.Save();

			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				var previousDocument = invLine.PreviousDocuments.AddNew();

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

				var invoice2 = declaration.Invoices.AddNew();
				var invLine2 = invoice2.InvoiceLines.AddNew();
				invLine2.JI_CEI = entryInstruction2.PK;
				var previousDocInvLine = invLine2.PreviousDocuments.AddNew();
				var previousDocInvHeader = invoice2.PreviousDocuments.AddNew();
				var previousDocDec = declaration.PreviousDocuments.AddNew();

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertAll("equals export no EntryInstruction", true, true, false, previousDocument);

				invLine.JI_CEI = entryInstruction1.PK;
				AssertAll("equals export and T2L", false, true, false, previousDocInvLine, previousDocInvHeader, previousDocDec);

				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertAll("equals export and no T2C or T2L", true, true, false, previousDocInvLine, previousDocInvHeader, previousDocDec);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				AssertAll("equals export and T2C", false, true, false, previousDocInvLine, previousDocInvHeader, previousDocDec);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertAll("equals import and T2C", false, true, false, previousDocInvLine, previousDocInvHeader, previousDocDec);

				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertAll("equals import and no T2C or T2L", false, false, true, previousDocInvLine, previousDocInvHeader, previousDocDec);

				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				AssertAll("equals import and T2L", false, true, false, previousDocInvLine, previousDocInvHeader, previousDocDec);
			});
		}

		public void TestPackageCodeList()
		{
			RefCusCodeList Create_CusCodeList_RefData(UniversalReferenceTestDataHelper refHelper, string countryCode, string refDataType, string code)
			{
				var dateMIN = ZDateTime.Today.AddMonths(-1);
				var dateMAX = ZDateTime.Today.AddMonths(3);

				return refHelper.CreateCusCodeList(countryCode, refDataType, code, dateMIN, dateMAX);
			}

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var unitedNationsPackageTypes = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes;
			var packageTypes = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes;
			var eun = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, "UnitedNationsRecommendations", helper.CreateNewOrGetExistingDataGrouping(eun, "EUN"));
			helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Spain, "ES");
			helper.CreateNewOrGetExistingCusCodeType(unitedNationsPackageTypes, "UNPKG");
			helper.CreateNewOrGetExistingCusCodeType(packageTypes, "PKG");
			Create_CusCodeList_RefData(helper, eun, unitedNationsPackageTypes, "1A");
			Create_CusCodeList_RefData(helper, eun, packageTypes, "1B");
			Create_CusCodeList_RefData(helper, CountryCodes.Spain, unitedNationsPackageTypes, "1C");
			Create_CusCodeList_RefData(helper, eun, unitedNationsPackageTypes, "1D");
			Create_CusCodeList_RefData(helper, eun, unitedNationsPackageTypes, "1E");

			Factory.Save();

			var lookups = new PreviousDocumentLookups(Factory.New<PreviousDocument>());
			var packageCodeList = lookups.PackageCodeList;

			CombineAssertions(() =>
			{
				AssertEquals("Codes", "1A, 1D, 1E", packageCodeList.CodesAsString);
				AssertSame("Cached", lookups.PackageCodeList, packageCodeList);
			});
		}

		public void TestSubTypeCodeList()
		{
			string[] default_SubTypeCodes = ["Y", "P", "Z", "X"];
			string[] h1_NMRN_SubTypeCodes = ["DVD", "RUN", "TRS", "IDA", "DUA", "DUE", "NIM"];

			CombineAssertions("CSI_SubType (Class) lookup codes", () =>
			{
				AssertImportSubTypeLookupCodes(
					(IMPORTVersionNumberList.Codes.Ics, false,
					IMPDeclarationTypeList.Codes.IM, EntrySubStyleList.Codes.A,
					UniversalReferenceConstants.PreviousDocumentType.NMRN),
					default_SubTypeCodes);

				AssertImportSubTypeLookupCodes(
					(IMPORTVersionNumberList.Codes.Ics, true,
					IMPDeclarationTypeList.Codes.IM, EntrySubStyleList.Codes.A,
					UniversalReferenceConstants.PreviousDocumentType.NMRN),
					h1_NMRN_SubTypeCodes);

				AssertImportSubTypeLookupCodes(
					(IMPORTVersionNumberList.Codes.H1, false,
					IMPDeclarationTypeList.Codes.IM, EntrySubStyleList.Codes.A,
					UniversalReferenceConstants.PreviousDocumentType.NMRN),
					h1_NMRN_SubTypeCodes);

				AssertImportSubTypeLookupCodes(
					(IMPORTVersionNumberList.Codes.H1, true,
					IMPDeclarationTypeList.Codes.IM, EntrySubStyleList.Codes.T2C,
					UniversalReferenceConstants.PreviousDocumentType.NMRN),
					default_SubTypeCodes);

				AssertImportSubTypeLookupCodes(
					(IMPORTVersionNumberList.Codes.H1, true,
					IMPDeclarationTypeList.Codes.IM, EntrySubStyleList.Codes.A,
					UniversalReferenceConstants.PreviousDocumentType.C651),
					[]);

				AssertExportSubTypeLookupCodes(
					(EXPORTVersionNumberList.Codes.Aes,
					EntrySubStyleList.Codes.A,
					UniversalReferenceConstants.PreviousDocumentType.NMRN),
					default_SubTypeCodes);

				AssertExportSubTypeLookupCodes(
					(EXPORTVersionNumberList.Codes.Aes11,
					EntrySubStyleList.Codes.A,
					UniversalReferenceConstants.PreviousDocumentType.NMRN),
					default_SubTypeCodes);
			});

			void AssertImportSubTypeLookupCodes(
				(string importMessageVersion,
				bool importUCC6Funcs,
				string instructionStyle,
				string instructionSubStyle,
				string previousDocumentType) testCaseSetup,
				string[] expectedDocumentSubTypeCodes)
			{
				using (RegistryTemporarySetterHelper.SetESImportMessageVersion(testCaseSetup.importMessageVersion))
				using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(testCaseSetup.importUCC6Funcs))
				{
					var (previousDocDeclaration, previousDocEntryInstruction, previousDocInvoiceHeader, previousDocInvoiceLine) = SetupPreviousDocuments(
						Common.Shared.SharedJobMessageTypeList.Codes.Import,
						testCaseSetup.instructionStyle,
						testCaseSetup.instructionSubStyle,
						testCaseSetup.previousDocumentType);

					var testCaseName = $"Import version: {testCaseSetup.importMessageVersion}; UCC6 FUNCS: {testCaseSetup.importUCC6Funcs}; CSI_Code: {testCaseSetup.previousDocumentType}";
					AssertPreviousDocumentsLookup($"{testCaseName}; Declaration", expectedDocumentSubTypeCodes, previousDocEntryInstruction);
					AssertPreviousDocumentsLookup($"{testCaseName}; Entry Instruction", expectedDocumentSubTypeCodes, previousDocEntryInstruction);
					AssertPreviousDocumentsLookup($"{testCaseName}; Invoice Header", expectedDocumentSubTypeCodes, previousDocInvoiceHeader);
					AssertPreviousDocumentsLookup($"{testCaseName}; Invoice Line", expectedDocumentSubTypeCodes, previousDocInvoiceLine);
				}
			}

			void AssertExportSubTypeLookupCodes(
				(string exportMessageVersion,
				string instructionSubStyle,
				string previousDocumentType) testCaseSetup,
				string[] expectedDocumentSubTypeCodes)
			{
				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(testCaseSetup.exportMessageVersion))
				{
					var(previousDocDeclaration, previousDocEntryInstruction, previousDocInvoiceHeader, previousDocInvoiceLine) = SetupPreviousDocuments(
						Common.Shared.SharedJobMessageTypeList.Codes.Export,
						string.Empty,
						testCaseSetup.instructionSubStyle,
						testCaseSetup.previousDocumentType);

					var testCaseName = $"Export version: {testCaseSetup.exportMessageVersion}; CSI_Code: {testCaseSetup.previousDocumentType}";
					AssertPreviousDocumentsLookup($"{testCaseName}; Declaration", expectedDocumentSubTypeCodes, previousDocEntryInstruction);
					AssertPreviousDocumentsLookup($"{testCaseName}; Entry Instruction", expectedDocumentSubTypeCodes, previousDocEntryInstruction);
					AssertPreviousDocumentsLookup($"{testCaseName}; Invoice Header", expectedDocumentSubTypeCodes, previousDocInvoiceHeader);
					AssertPreviousDocumentsLookup($"{testCaseName}; Invoice Line", expectedDocumentSubTypeCodes, previousDocInvoiceLine);
				}
			}

			(PreviousDocument Declaration, PreviousDocument EntryInstruction, PreviousDocument InvoiceHeader, PreviousDocument InvoiceLine) SetupPreviousDocuments(string messageType, string instructionStyle, string instructionSubStyle, string previousDocumentType)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = messageType;

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

				entryInstruction.CEI_Style = instructionStyle;
				entryInstruction.CEI_SubStyle = instructionSubStyle;

				var previousDocDeclaration = declaration.PreviousDocuments.AddNew();
				previousDocDeclaration.CSI_Code = previousDocumentType;

				var previousDocEntryInstruction = entryInstruction.PreviousDocuments.AddNew();
				previousDocEntryInstruction.CSI_Code = previousDocumentType;

				var previousDocInvoiceHeader = invoiceHeader.PreviousDocuments.AddNew();
				previousDocInvoiceHeader.CSI_Code = previousDocumentType;

				var previousDocInvoiceLine = invoiceLine.PreviousDocuments.AddNew();
				previousDocInvoiceLine.CSI_Code = previousDocumentType;

				return (previousDocDeclaration, previousDocEntryInstruction, previousDocInvoiceHeader, previousDocInvoiceLine);
			}

			void AssertPreviousDocumentsLookup(string testCase, string[] expectedDocumentSubTypeCodes, PreviousDocument previousDocument)
			{
				var expected = string.Join(", ", expectedDocumentSubTypeCodes);
				var actual = string.Join(", ", previousDocument.Lookups.SubTypeList.GetAllCodes());
				AssertEquals(testCase, expected, actual);
			}
		}
	}
}
