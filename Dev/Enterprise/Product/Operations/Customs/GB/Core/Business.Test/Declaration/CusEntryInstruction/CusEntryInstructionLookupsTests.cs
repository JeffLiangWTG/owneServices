using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class CusEntryInstructionLookupsTests : BusinessObjectLookupsTestCase
	{
		public void TestCDSEntrySubStyleList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = "IMP";
			dec.JE_DeclarationType = "H1";
			Assert("Entry Sub Style lookup should have 4 codes", dec.CusEntryInstruction.Lookups.EntrySubStyleList.Count == 4);
		}

		[TestDate(2018, 9, 29)]
		public void TestCDSCusEntryInstructionLookups_Imports()
		{
			SetupRefData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = "IMP";
			declaration.JE_DeclarationType = "";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertType(typeof(CusEntryInstructionLookups), instruction.Lookups);
			AssertSame(instruction.Lookups.EntrySubStyleList, instruction.Lookups.EntrySubStyleList);
			AssertSame(instruction.Lookups.DeclarationTypeList, instruction.Lookups.DeclarationTypeList);

			var testData = new List<(string, IEnumerable<string>)>
			{
				(string.Empty, allCodes),
				(ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse, codesADYZ),
				(ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing, codesAD),
				(ImportDeclarationTypeList.Codes.DeclarationForTemporaryAdmission, codesADYZ),
				(ImportDeclarationTypeList.Codes.DeclarationForInwardProcessing, codesADYZ),
				(ImportDeclarationTypeList.Codes.DeclarationForGoodsFromTheSpecialFiscalTerritories, codesADYZ),
				(ImportDeclarationTypeList.Codes.SuperReducedDataSetDeclaration, codesAD),
				(ImportDeclarationTypeList.Codes.ReducedDataSetDeclaration, codesADYZ),
				(ImportDeclarationTypeList.Codes.ImportSimplifiedDeclaration, codesBCEF),
				(ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration, codesQ),
				(ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I, codesJK),
				(ImportDeclarationTypeList.Codes.ImportClearanceRequestC21N, codesJK),
				(ImportDeclarationTypeList.Codes.BulkImportReducedDataSet, codesJK),
				("XXX", allCodes),
			};
			AssertEntrySubStyleListForStyles(instruction, testData);
		}

		[TestDate(2018, 9, 29)]
		public void TestCDSCusEntryInstructionLookups_Exports()
		{
			SetupRefData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = "EXP";
			declaration.JE_DeclarationType = "";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertType(typeof(CusEntryInstructionLookups), instruction.Lookups);
			AssertSame(instruction.Lookups.EntrySubStyleList, instruction.Lookups.EntrySubStyleList);
			AssertSame(instruction.Lookups.DeclarationTypeList, instruction.Lookups.DeclarationTypeList);

			var testData = new List<(string, IEnumerable<string>)>
			{
				(string.Empty, allCodes),
				(ExportDeclarationTypeList.Codes.DeclarationForExport, codesADYZ),
				(ExportDeclarationTypeList.Codes.DeclarationForOutwardProcessing, codesAD),
				(ExportDeclarationTypeList.Codes.DeclarationForDispatchOfGoods, codesADYZ),
				(ExportDeclarationTypeList.Codes.SimplifiedDeclarationForExport, codesBCEF),
				(ExportDeclarationTypeList.Codes.ExportClearanceRequestC21E, codesJK),
				(ExportDeclarationTypeList.Codes.ExportClearanceRequestC21EEIDRNOP, codesJK),
				("XXX", allCodes),
			};
			AssertEntrySubStyleListForStyles(instruction, testData);
		}

		void AssertEntrySubStyleListForStyles(CusEntryInstruction instruction, List<(string, IEnumerable<string>)> testData)
		{
			foreach (var (style, expectedCodes) in testData)
			{
				instruction.CEI_Style = style;
				AssertContainsExactElementsInAnyOrder(expectedCodes, instruction.Lookups.EntrySubStyleList.GetAllCodes());
			}
		}

		IEnumerable<string> allCodes
		{
			get
			{
				yield return EntrySubStyleCodeList.Codes.A;
				yield return EntrySubStyleCodeList.Codes.B;
				yield return EntrySubStyleCodeList.Codes.C;
				yield return EntrySubStyleCodeList.Codes.D;
				yield return EntrySubStyleCodeList.Codes.E;
				yield return EntrySubStyleCodeList.Codes.F;
				yield return EntrySubStyleCodeList.Codes.J;
				yield return EntrySubStyleCodeList.Codes.K;
				yield return EntrySubStyleCodeList.Codes.X;
				yield return EntrySubStyleCodeList.Codes.Y;
				yield return EntrySubStyleCodeList.Codes.Z;
			}
		}
		IEnumerable<string> codesAD
		{
			get
			{
				yield return EntrySubStyleCodeList.Codes.A;
				yield return EntrySubStyleCodeList.Codes.D;
			}
		}
		IEnumerable<string> codesADYZ
		{
			get
			{
				yield return EntrySubStyleCodeList.Codes.A;
				yield return EntrySubStyleCodeList.Codes.D;
				yield return EntrySubStyleCodeList.Codes.Y;
				yield return EntrySubStyleCodeList.Codes.Z;
			}
		}
		IEnumerable<string> codesBCEF
		{
			get
			{
				yield return EntrySubStyleCodeList.Codes.B;
				yield return EntrySubStyleCodeList.Codes.C;
				yield return EntrySubStyleCodeList.Codes.E;
				yield return EntrySubStyleCodeList.Codes.F;
			}
		}
		IEnumerable<string> codesJK
		{
			get
			{
				yield return EntrySubStyleCodeList.Codes.J;
				yield return EntrySubStyleCodeList.Codes.K;
			}
		}
		IEnumerable<string> codesQ
		{
			get
			{
				yield return EntrySubStyleCodeList.Codes.Q;
			}
		}

		void SetupRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("ENSUB", "Entry Sub Style");
			helper.CreateNewOrGetExistingCusCodeList("CDS", "ENSUB", "A", "Standard customs declaration (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateNewOrGetExistingCusCodeList("CDS", "ENSUB", "B", "Simplified declaration on an occasional basis (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateNewOrGetExistingCusCodeList("CDS", "ENSUB", "C", "Simplified declaration with regular use (pre-authorised) (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateNewOrGetExistingCusCodeList("CDS", "ENSUB", "D", "Standard customs declaration (Goods not arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateNewOrGetExistingCusCodeList("CDS", "ENSUB", "E", "Simplified declaration on an occasional basis (Goods not arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateNewOrGetExistingCusCodeList("CDS", "ENSUB", "F", "Simplified declaration with regular use (pre-authorised) (Goods not arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateNewOrGetExistingCusCodeList("CDS", "ENSUB", "J", "C21 (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateNewOrGetExistingCusCodeList("CDS", "ENSUB", "K", "C21 (Goods not arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateNewOrGetExistingCusCodeList("CDS", "ENSUB", "X", "Supplementary declaration covered by types B and E (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateNewOrGetExistingCusCodeList("CDS", "ENSUB", "Y", "Supplementary declaration covered by types C and F (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			helper.CreateNewOrGetExistingCusCodeList("CDS", "ENSUB", "Z", "Supplementary declarations for Entry in Declarants Records (Goods arrived)", new ZDateTime(2018, 9, 28), new ZDateTime(2018, 9, 30));
			Factory.Save();
		}

		public void TestCDSCusEntryInstructionLookupsLoadingTheCorrectDescription()
		{
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusProcedure");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "A", "11", "11", "111", "One", "IMP", group: "IFD,ISD");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "D", "44", "44", "444", "Four", "EXP", group: "EFD,ESD");
			helper.CreateRefCusProcedure(GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "F", "66", "66", "666", "Six", "IMP", group: "H1,I1,H5");
			helper.CreateRefCusProcedure(GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "G", "77", "77", "777", "Seven", "EXP", group: "AA,BB");
			helper.CreateRefCusProcedure("ZA", "B", "33", "33", "333", "Three", "IMP", group: "IFW");
			helper.CreateRefCusProcedure("ZA", "A", "55", "55", "555", "Five", "EXP", group: "ESD");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = "IMP";
			AssertEquals("List should be based on RefCusProcedure data not CodeDescriptionPairList", 3, dec.CusEntryInstruction.Lookups.DeclarationTypeList.Count);
			AssertEquals(ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse, dec.CusEntryInstruction.CEI_StyleInfo.Value);
			AssertEquals(ImportDeclarationTypeList.Descriptions.DeclarationForReleaseForFreeCirculationOrEndUse, dec.CusEntryInstruction.CEI_DescriptionInfo.Value);
		}

		public void TestGetDefinedDeclarationTypeList_ShouldReturnDifferentTypeListDependOnMessageType()
		{
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusProcedure");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "A", "11", "11", "111", "One", "IMP", group: "H1");
			helper.CreateRefCusProcedure(GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "D", "44", "44", "444", "Four", "EXP", group: "B1");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entryInstruction = declaration.CusEntryInstruction;

			declaration.JE_MessageType = "IMP";
			AssertEquals("H1", entryInstruction.Lookups.DeclarationTypeList[0].Code);
			AssertEquals(new ImportDeclarationTypeList().GetDescriptionFromCode("H1"), entryInstruction.Lookups.DeclarationTypeList[0].Description);

			declaration.JE_MessageType = "EXP";
			AssertEquals("B1", entryInstruction.Lookups.DeclarationTypeList[0].Code);
			AssertEquals(new ExportDeclarationTypeList().GetDescriptionFromCode("B1"), entryInstruction.Lookups.DeclarationTypeList[0].Description);
		}
	}
}
