using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	partial class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStyleList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("CN", "A", "0110", "", "", "TEST1", "IMP");
			helper.CreateRefCusProcedure("CN", "A", "0139", "", "", "TEST2", "EXP");
			helper.CreateRefCusProcedure("CN", "A", "0314", "", "", "TEST3", "IMP,EXP");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var styleListForImport = instruction1.Lookups.StyleList;
			AssertContainsExactElementsInAnyOrder(new[] { "0110", "0314" }, styleListForImport.GetAllCodes());
			AssertSame(styleListForImport, instruction2.Lookups.StyleList);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var styleListForExport = instruction1.Lookups.StyleList;
			AssertContainsExactElementsInAnyOrder(new[] { "0139", "0314" }, styleListForExport.GetAllCodes());
			AssertSame(styleListForExport, instruction2.Lookups.StyleList);
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			instruction2.CEI_CEI_Parent = instruction1.PK;
			AssertSame(styleListForImport, instruction1.Lookups.StyleList);
			AssertSame(styleListForExport, instruction2.Lookups.StyleList);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertSame(styleListForImport, instruction1.Lookups.StyleList);
			AssertSame(styleListForExport, instruction2.Lookups.StyleList);
		}

		public void TestPackageTypeList()
		{
			var lookups = Factory.New<CusEntryInstruction>().Lookups;
			var list = lookups.PackageTypeList;
			AssertEquals("PackageTypeList Count", 14, list.Count);
			Assert("PackageTypeList", list is UntranslatableCodeDescriptionPairList);
			AssertEquals("PackageTypeList Descriptions", "\u6563\u88c5", list.GetDescriptionFromCode("00"));
		}

		[TestDate(2016, 5, 5)]
		public void TestLevyTypes()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var list = testItems.EntryInstruction.Lookups.LevyTypes;
			AssertSame(LevyTypeList.GetSupportedLevyTypeList(Factory, true), list);
			AssertEquals(71, list.Count);
			Assert(list.ContainsCode("101"));
			AssertEquals("一般征税", list.GetDescriptionFromCode("101"));
			Assert(list.ContainsCode("408"));
			AssertEquals("重大技术装备", list.GetDescriptionFromCode("408"));
			Assert(list.ContainsCode("999"));
			AssertEquals("例外减免", list.GetDescriptionFromCode("999"));
			testItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var listEXP = testItems.EntryInstruction.Lookups.LevyTypes;
			Assert(!ReferenceEquals(list, listEXP));
			AssertSame(LevyTypeList.GetSupportedLevyTypeList(Factory, false), listEXP);
			AssertEquals(26, listEXP.Count);
			Assert(listEXP.ContainsCode("101"));
			Assert(listEXP.ContainsCode("413"));
			Assert(listEXP.ContainsCode("999"));
		}

		public void TestCIQRelations()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var testList = testItems.EntryInstruction.Lookups.CIQRelations;
			AssertSame(Factory.GetCachedValue<CIQRelation>(), testList);
		}

		public void TestEntryDocumentSubmissionType()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var testList = testItems.EntryInstruction.Lookups.EntryDocumentSubmissionType;
			AssertEquals("EntryDocumentSubmissionTypes Count", 5, testList.Count);
			Assert("EntryDocumentSubmissionTypes Type", testList is UntranslatableCodeDescriptionPairList);
			AssertEquals("EntryDocumentSubmissionTypes Descriptions", "\u6709\u7eb8\u62a5\u5173", testList.GetDescriptionFromCode("0"));
		}

		public void TestParents()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = Factory.New<CusEntryInstruction>();
			instruction1.CEI_Style = "1";
			var instruction2 = Factory.New<CusEntryInstruction>();
			instruction2.CEI_Style = "2";
			var instruction3 = Factory.New<CusEntryInstruction>();
			instruction3.CEI_Style = "3";
			var instruction4 = Factory.New<CusEntryInstruction>();
			instruction4.CEI_Style = "1";
			declaration.CustomsEntryInstructions.Add(instruction1);
			declaration.CustomsEntryInstructions.Add(instruction2);
			declaration.CustomsEntryInstructions.Add(instruction3);
			declaration.CustomsEntryInstructions.Add(instruction4);
			AssertContainsExactElementsInAnyOrder(new[] { instruction2.PK, instruction3.PK, instruction4.PK }, instruction1.Lookups.Parents.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { instruction1.PK, instruction3.PK, instruction4.PK }, instruction2.Lookups.Parents.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { instruction1.PK, instruction2.PK, instruction4.PK }, instruction3.Lookups.Parents.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { instruction1.PK, instruction2.PK, instruction3.PK }, instruction4.Lookups.Parents.Select(x => x.PK));
			instruction2.CEI_CEI_Parent = instruction1.PK;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZGuid>(), instruction1.Lookups.Parents.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { instruction1.PK, instruction3.PK, instruction4.PK }, instruction2.Lookups.Parents.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { instruction4.PK }, instruction3.Lookups.Parents.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { instruction3.PK }, instruction4.Lookups.Parents.Select(x => x.PK));
			instruction4.CEI_CEI_Parent = instruction3.PK;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZGuid>(), instruction1.Lookups.Parents.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { instruction1.PK }, instruction2.Lookups.Parents.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZGuid>(), instruction3.Lookups.Parents.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { instruction3.PK }, instruction4.Lookups.Parents.Select(x => x.PK));
		}

		public void TestIntelligentDeclarationTypeList()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var list = instruction.Lookups.IntelligentDeclarationTypeList;
			AssertContainsExactElementsInExactOrder(new[] { "0", "1", "2" }, list.GetAllCodes());
			AssertSame("cached", list, instruction.Lookups.IntelligentDeclarationTypeList);
		}
	}
}
