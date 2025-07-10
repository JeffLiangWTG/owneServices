using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestEntrySubStyleListCombinations()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var lookups = instruction.Lookups;

		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToEFTAMember;

		var entrySubStyleList = lookups.EntrySubStyleList;

		CombineAssertions(() =>
		{
			AssertEquals("EntrySubStyleList should not has EXS when is Export but Entry Style is not EX", "A, B, C, T2L, X, Y, Z", entrySubStyleList.CodesAsString);
			AssertEquals("EntrySubStyleList description for T2L code when is Export and Entry Style is not EX", "Registro/Visado T2L POUS", entrySubStyleList.GetDescriptionFromCode("T2L"));

			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			entrySubStyleList = lookups.EntrySubStyleList;
			AssertEquals(typeof(ExsEntrySubStyleList), entrySubStyleList.GetType());
			AssertEquals("EntrySubStyleList should has EXS when is Export and Entry Style is EX", "A, B, C, EXS, T2L, X, Y, Z", entrySubStyleList.CodesAsString);
			AssertEquals("EntrySubStyleList description for T2L code when is Export and Entry Style is EX", "Registro/Visado T2L POUS", entrySubStyleList.GetDescriptionFromCode("T2L"));
			AssertSame("EntrySubStyleList should be cached", entrySubStyleList, lookups.EntrySubStyleList);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.ProofOfUnionStatus))
			{
				entrySubStyleList = lookups.EntrySubStyleList;
				AssertEquals(typeof(EntrySubStyleList), entrySubStyleList.GetType());
				AssertEquals("EntrySubStyleList should be the default when is Import, no H2 (and EST2LMessageVersion in registry is POUS)", "A, B, C, T2C, T2L, X, Y, Z", entrySubStyleList.CodesAsString);
				AssertEquals("EntrySubStyleList from code T2L when is import", "Alta Indirecta T2L POUS", entrySubStyleList.GetDescriptionFromCode("T2L"));
				AssertEquals("EntrySubStyleList from code T2C when is import", "Datado T2L POUS", entrySubStyleList.GetDescriptionFromCode("T2C"));
			}

			instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			entrySubStyleList = lookups.EntrySubStyleList;
			AssertEquals("EntrySubStyleList should be A, B, X and Z when is Import and H2", "A, B, X, Z", entrySubStyleList.CodesAsString);
			AssertSame("EntrySubStyleList should be cached", entrySubStyleList, lookups.EntrySubStyleList);

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
			{
				instruction.CEI_Style = ZString.Empty;
				entrySubStyleList = lookups.EntrySubStyleList;
				AssertEquals(typeof(EntrySubStyleList), entrySubStyleList.GetType());
				AssertEquals("EntrySubStyleList should be the default when is Import, no H2 (and EST2LMessageVersion in registry is NOPOUS)", "A, B, C, T2C, T2L, X, Y, Z", entrySubStyleList.CodesAsString);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.RequestJecAndReceptionPous))
			{
				entrySubStyleList = lookups.EntrySubStyleList;
				AssertEquals(typeof(EntrySubStyleList), entrySubStyleList.GetType());
				AssertEquals("EntrySubStyleList should be the default when is Import, no H2 (and EST2LMessageVersion in registry is POUS2)", "A, B, C, T2C, T2L, X, Y, Z", entrySubStyleList.CodesAsString);
				AssertEquals("EntrySubStyleList from code T2L when is import", "Alta Indirecta T2L POUS", entrySubStyleList.GetDescriptionFromCode("T2L"));
				AssertEquals("EntrySubStyleList from code T2C when is import", "Datado T2L POUS", entrySubStyleList.GetDescriptionFromCode("T2C"));
			}

			var entryInstructionsWithNoDeclaration = Factory.New<CusEntryInstruction>();
			AssertNull("[PRE-CONDITION]", entryInstructionsWithNoDeclaration.JobDeclaration);
			AssertEquals("EntrySubStyleList default", "A, B, C, T2C, T2L, X, Y, Z", entryInstructionsWithNoDeclaration.Lookups.EntrySubStyleList.CodesAsString);
			AssertEquals("EntrySubStyleList from code T2L when default", "Registro/Visado T2L POUS", entryInstructionsWithNoDeclaration.Lookups.EntrySubStyleList.GetDescriptionFromCode("T2L"));
			AssertEquals("EntrySubStyleList from code T2C when default", "Datado T2L POUS", entryInstructionsWithNoDeclaration.Lookups.EntrySubStyleList.GetDescriptionFromCode("T2C"));
		});
	}

	public void TestDeclarationTypeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var lookups = instruction.Lookups;
		CombineAssertions(() =>
		{
			AssertEquals("EntryStyleList defualt when EntrySubStyle is empty", "H2, IM", lookups.DeclarationTypeList.CodesAsString);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			AssertEquals("EntryStyleList is IM when EntrySubStyle is C", "IM", lookups.DeclarationTypeList.CodesAsString);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("EntryStyleList defualt when EntrySubStyle is A", "H2, IM", lookups.DeclarationTypeList.CodesAsString);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("EntryStyleList is IM when EntrySubStyle is T2C", "IM", lookups.DeclarationTypeList.CodesAsString);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("EntryStyleList defualt when EntrySubStyle is B", "H2, IM", lookups.DeclarationTypeList.CodesAsString);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("EntryStyleList is IM when EntrySubStyle is T2L", "IM", lookups.DeclarationTypeList.CodesAsString);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.X;
			AssertEquals("EntryStyleList defualt when EntrySubStyle is X", "H2, IM", lookups.DeclarationTypeList.CodesAsString);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
			AssertEquals("EntryStyleList is IM when EntrySubStyle is Y", "IM", lookups.DeclarationTypeList.CodesAsString);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
			AssertEquals("EntryStyleList defualt when EntrySubStyle is Z", "H2, IM", lookups.DeclarationTypeList.CodesAsString);
		});
	}
}
