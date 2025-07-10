using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryInstructionLookupsTest : EU.Business.Declaration.Testing.CusEntryInstructionLookupsTest
{
	protected override ZString GetExpectedEntrySubStyleListCodesAsString() => ZString.Empty;

	protected override Type TypeOfDeclarationOfInstruction => typeof(JobDeclaration);

	public void TestDeclarationTypeListForImport()
	{
		SetUpRefDb();

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		var declarationTypeList = entryInstruction.Lookups.DeclarationTypeList;
		CombineAssertions(() =>
		{
			AssertEquals("DeclarationTypeList CodesAsString", "H1, H2, H3, H4, H5, I1, I2", declarationTypeList.CodesAsString);
			AssertEquals("'H1' Description", "Immissione in libera pratica e regime uso speciale - uso specifico - dichiarazione di uso finale", declarationTypeList.GetDescriptionFromCode("H1"));
			AssertEquals("'H2' Description", "Regime speciale - custodia - dichiarazione per il deposito doganale", declarationTypeList.GetDescriptionFromCode("H2"));
			AssertEquals("'I1' Description", "Dichiarazione semplificata di importazione", declarationTypeList.GetDescriptionFromCode("I1"));
		});
	}

	public void TestDeclarationTypeListForExport()
	{
		AssertDeclarationTypeListOnlyContainsLegacyValues(Common.EU.EUJobMessageTypeList.Codes.Export);

		CombineAssertions("For EXP and IsUCC6 = true", () =>
		{
			declaration.MessageVersion = "XML";
			AssertEquals("Pre: IsUCC6", true, declaration.IsUCC6);
			AssertEquals("CodeAsString", "B1, B2, B4, C1, C2", entryInstruction.Lookups.DeclarationTypeList.CodesAsString);
		});
	}

	public void TestDeclarationTypeListForAnyOtherMessageType()
	{
		SetUpRefDb();

		declaration.JE_MessageType = "ABC";
		AssertEquals("DeclarationTypeList CodesAsString", "", entryInstruction.Lookups.DeclarationTypeList.CodesAsString);
	}

	public void TestEntrySubStyleListDescription()
	{
		entryInstruction.CEI_Style = "H1";
		var entrySubStyleList = entryInstruction.Lookups.EntrySubStyleList;
		AssertEquals("'A' Description", "Standard declaration (Article 162 UCC)", entrySubStyleList.GetDescriptionFromCode("A"));
		AssertEquals("'D' Description", "Preliminary standard declaration (Under code A) (Article 171 UCC)", entrySubStyleList.GetDescriptionFromCode("D"));
		AssertEquals("'X' Description", "Supplementary declaration for simplified declarations (Code B, E)", entrySubStyleList.GetDescriptionFromCode("X"));
		AssertEquals("'Y' Description", "Supplementary declaration for simplified declarations (Code C, F)", entrySubStyleList.GetDescriptionFromCode("Y"));
		AssertEquals("'Z' Description", "Supplementary declaration (Article 182 UCC)", entrySubStyleList.GetDescriptionFromCode("Z"));

		entryInstruction.CEI_Style = "I1";
		entrySubStyleList = entryInstruction.Lookups.EntrySubStyleList;
		AssertEquals("'B' Description", "Simplified declaration (occasionally) (Article 166 Par. 1 UCC)", entrySubStyleList.GetDescriptionFromCode("B"));
		AssertEquals("'C' Description", "Simplified declaration (regularly) (Article 166 par. 2 UCC)", entrySubStyleList.GetDescriptionFromCode("C"));
		AssertEquals("'E' Description", "Preliminary simplified declaration (Under code B) (Article 171 UCC)", entrySubStyleList.GetDescriptionFromCode("E"));
		AssertEquals("'F' Description", "Preliminary simplified declaration (Under code C) (Article 171 UCC)", entrySubStyleList.GetDescriptionFromCode("F"));
	}

	public void TestEntrySubStyleListWithExportSpecificStyles()
	{
		declaration.JE_MessageType = "EXP";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("When Declaration is UCC6", () =>
			{
				AssertEntrySubStyleListCodesAsStringForDeclarationType("B1", "A, D, X, Y, Z");
				AssertEntrySubStyleListCodesAsStringForDeclarationType("B2", "A, D");
				AssertEntrySubStyleListCodesAsStringForDeclarationType("B4", "A, D");
				AssertEntrySubStyleListCodesAsStringForDeclarationType("C1", "B, C, E, F");
				AssertEntrySubStyleListCodesAsStringForDeclarationType("C2", "");
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("When Declaration is not UCC6", () =>
			{
				AssertEntrySubStyleListCodesAsStringForDeclarationType("COD", "A, D");
				AssertEntrySubStyleListCodesAsStringForDeclarationType("COL", "A, D");
				AssertEntrySubStyleListCodesAsStringForDeclarationType("DSE", "A, D");
			});
		}
	}

	public void TestEntrySubStyleListWithImportSpecificStyles()
	{
		declaration.JE_MessageType = "IMP";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("When Declaration is UCC6", () =>
			{
				AssertEntrySubStyleListCodesAsStringForDeclarationType("H1", "A, D, X, Y, Z");
				AssertEntrySubStyleListCodesAsStringForDeclarationType("H2", "A, D");
				AssertEntrySubStyleListCodesAsStringForDeclarationType("H3", "A, D");
				AssertEntrySubStyleListCodesAsStringForDeclarationType("H4", "A, D");
				AssertEntrySubStyleListCodesAsStringForDeclarationType("H5", "");
				AssertEntrySubStyleListCodesAsStringForDeclarationType("I1", "B, C, E, F");
				AssertEntrySubStyleListCodesAsStringForDeclarationType("I2", "");
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("When Declaration is not UCC6", () =>
			{
				AssertEntrySubStyleListCodesAsStringForDeclarationType("COD", "A, D");
				AssertEntrySubStyleListCodesAsStringForDeclarationType("COL", "A, D");
				AssertEntrySubStyleListCodesAsStringForDeclarationType("DSE", "A, D");
			});
		}

		CombineAssertions("Edge cases", () =>
		{
			AssertEntrySubStyleListCodesAsStringForDeclarationType("", "");
			AssertEntrySubStyleListCodesAsStringForDeclarationType("R", "");
		});
	}

	public void TestProcedureCodeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure("IT", "IM", "40", "", "", "40 DESCRIPTION - OTHER DESCRIPTION", "IMP");
		helper.CreateRefCusProcedure("IT", "IM", "40", "00", "00", "40 DESCRIPTION - SOME OTHER DESCRIPTION", "IMP");
		helper.CreateRefCusProcedure("IT", "IM", "71", "", "", "71 DESCRIPTION", "IMP");
		helper.CreateRefCusProcedure("IT", "IM", "51", "", "", "51 DESCRIPTION", "IMP");
		helper.CreateRefCusProcedure("IT", "EX", "10", "", "", "10 DESCRIPTION", "EXP");
		helper.CreateRefCusProcedure("IT", "EX", "20", "", "", "20 DESCRIPTION", "EXP");
		helper.CreateRefCusProcedure("IT", "EX", "30", "", "", "30 DESCRIPTION", "EXP");

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		declaration.JE_MessageType = "IMP";

		var procedureCodes = entryInstruction.Lookups.ProcedureCodeList;
		AssertEquals(3, procedureCodes.Count);
		AssertEquals("40 DESCRIPTION", procedureCodes.GetDescriptionFromCode("40"));
		AssertEquals("71 DESCRIPTION", procedureCodes.GetDescriptionFromCode("71"));
		AssertEquals("51 DESCRIPTION", procedureCodes.GetDescriptionFromCode("51"));

		declaration.JE_MessageType = "EXP";
		procedureCodes = entryInstruction.Lookups.ProcedureCodeList;
		AssertEquals(3, procedureCodes.Count);
		AssertEquals("10 DESCRIPTION", procedureCodes.GetDescriptionFromCode("10"));
		AssertEquals("20 DESCRIPTION", procedureCodes.GetDescriptionFromCode("20"));
		AssertEquals("30 DESCRIPTION", procedureCodes.GetDescriptionFromCode("30"));

		declaration.JE_MessageType = "XXX";
		procedureCodes = entryInstruction.Lookups.ProcedureCodeList;
		AssertEquals(0, procedureCodes.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;

	#region Implementation

	void AssertDeclarationTypeListOnlyContainsLegacyValues(ZString messageType)
	{
		SetUpRefDb();

		declaration.JE_MessageType = messageType;
		var declarationTypeList = entryInstruction.Lookups.DeclarationTypeList;
		CombineAssertions(() =>
		{
			AssertEquals("DeclarationTypeList CodesAsString", 3, declarationTypeList.Count);
			AssertEquals("'DSE' Description", SADDeclarationTypeList.Descriptions.DichiarazioneSemplificata, declarationTypeList.GetDescriptionFromCode("DSE"));
			AssertEquals("'COD' Description", SADDeclarationTypeList.Descriptions.ProceduraOrdinariaCODogana, declarationTypeList.GetDescriptionFromCode("COD"));
			AssertEquals("'COL' Description", SADDeclarationTypeList.Descriptions.ProceduraOrdinariaCOLuogo, declarationTypeList.GetDescriptionFromCode("COL"));
		});
	}

	void AssertEntrySubStyleListCodesAsStringForDeclarationType(string declarationType, string expectedSubStyleListCodesAsString)
	{
		entryInstruction.CEI_Style = declarationType;
		var entrySubStyleList = entryInstruction.Lookups.EntrySubStyleList;
		AssertEquals($"When Declaration type = {declarationType}, EntrySubStyleList CodesAsString", expectedSubStyleListCodesAsString, entrySubStyleList.CodesAsString);
	}

	void SetUpRefDb()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "A", "11", "11", "111", "One", Common.EU.EUJobMessageTypeList.Codes.Import, group: "COD,COL,DSE,H1,H2,H3,H4,H5,I1,I2");
		helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "A", "11", "11", "111", "One", Common.EU.EUJobMessageTypeList.Codes.Export, group: "COD,COL,DSE,B1,B2,B4,C1,C2");
	}

	#endregion
}
