using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class DeltaIECusEntryInstructionLookupsTest : TestCaseWithFactory
	{
		public void TestDeclarationTypeList_DependingOnApplicationCode()
		{
			Db.Connection.ExecuteNonQuery("DELETE FROM RefDatabase_RefCusProcedure");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "EX", "01", "00", "000", "Export DeltaIE", "EXP", group: "H1");
			helper.CreateRefCusProcedure(CountryCodes.France, "EX", "01", "00", "000", "Export DeltaG", "EXP", group: "10");
			Factory.Save();

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var instruction2 = Factory.New<CusEntryInstruction>();
			instruction2.CEI_JE = declaration2.PK;
			AssertEquals("List should be based on DB records having DIE as data grouping.", "H1", instruction2.Lookups.DeclarationTypeList[0].Code);
		}

		public void TestCPCList()
		{
			Db.Connection.ExecuteNonQuery("DELETE FROM RefDatabase_RefCusProcedure");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(CountryCodes.France, "", "10", "00", "000", "Export DeltaG Procedure Group 10", EU.Business.MessageTypeList.Codes.Export, group: "B1");
			helper.CreateRefCusProcedure(CountryCodes.France, "", "21", "00", "000", "Export DeltaG Procedure Group 21", EU.Business.MessageTypeList.Codes.Export, group: "B2");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "22", "00", "000", "Export DeltaIE Procedure Group B1", EU.Business.MessageTypeList.Codes.Export, group: "B1");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "31", "00", "000", "Export DeltaIE Procedure Group B2", EU.Business.MessageTypeList.Codes.Export, group: "B2");
			helper.CreateRefCusProcedure(CountryCodes.France, "", "01", "00", "000", "Import DeltaG Procedure Group 01", EU.Business.MessageTypeList.Codes.Import, group: "B1");
			helper.CreateRefCusProcedure(CountryCodes.France, "", "02", "00", "000", "Import DeltaG Procedure Group 02", EU.Business.MessageTypeList.Codes.Import, group: "B2");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "07", "00", "000", "Import DeltaIE Procedure Group B1", EU.Business.MessageTypeList.Codes.Import, group: "B1");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "40", "00", "000", "Import DeltaIE Procedure Group B2", EU.Business.MessageTypeList.Codes.Import, group: "B2");
			Factory.Save();

			CombineAssertions("List should filter on DIE Data Grouping, Message Type, Declaration Application Code and Declaration Type", () =>
			{
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Export, "B1", "22");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Export, "B2", "31");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Import, "B1", "07");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Import, "B2", "40");
			});

			void AssertProcedureList(string messageType, string declarationType, string expectedResult)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = messageType;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = declarationType;
				AssertEquals(expectedResult, entryInstruction.Lookups.CPCList.CodesAsString);
			}
		}

		public void TestGetEntrySubstyleList()
		{
			var refCusCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(refCusCodeType, "CodeTypeDescription");
			helper.CreateCusCodeList("FR", refCusCodeType, "C", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("FR", refCusCodeType, "F", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("FR", refCusCodeType, "L", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("FR", refCusCodeType, "M", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("FR", refCusCodeType, "N", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			AssertEquals("For Delta IE Import declarations, EntrySubStyleList should contain data from RefCusCodeList with Type: ENSUB and DataGrouping: FR.", "C, F, L, M, N", entryInstruction.Lookups.EntrySubStyleList.CodesAsString);

			entryInstruction.CEI_Style = DeltaIEImportDeclarationTypeList.Codes.I1;
			AssertEquals("For an I1 Declaration type, CEI_SubStyle must only be C or F.", "C, F", entryInstruction.Lookups.EntrySubStyleList.CodesAsString);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("For DeltaIE Export declarations, EntrySubStyleList should return EntrySubstyleCodePairList.", new EntrySubstyleCodePairList().CodesAsString, entryInstruction.Lookups.EntrySubStyleList.CodesAsString);
		}
	}
}
