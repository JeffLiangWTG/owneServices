using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(InstructionConfiguration))]
sealed class InstructionConfigurationTest : InstructionConfigurationAbstractTest<InstructionConfiguration>
{
	public override void TestFiscalReferencesSupport()
	{
		AssertEquals(true, configuration.FiscalReferencesSupportOnCPC42And63Only(CreateDeclaration()));
	}

	public override void TestFiscalReferencesSupportOnCPC42And63Only()
	{
		AssertEquals(true, configuration.FiscalReferencesSupportOnCPC42And63Only(CreateDeclaration()));
	}

	public override void TestAuthorisationsSupport()
	{
		AssertEquals(true, configuration.AuthorisationsSupport(CreateDeclaration()));
	}

	public void TestSupplyChainActorReferencesSupport()
	{
		var declaration = CreateDeclaration();
		declaration.JE_MessageType = "EXP";

		AssertEquals("No entry instruction", true, configuration.AdditionalSupplyChainActorSupport(declaration));

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "B1";
		CombineAssertions(() =>
		{
			AssertEquals("Export B1", true, configuration.AdditionalSupplyChainActorSupport(declaration));

			entryInstruction.CEI_Style = "B2";
			AssertEquals("Export B2", true, configuration.AdditionalSupplyChainActorSupport(declaration));

			entryInstruction.CEI_Style = "B3";
			AssertEquals("Export B3", true, configuration.AdditionalSupplyChainActorSupport(declaration));

			entryInstruction.CEI_Style = "B4";
			AssertEquals("Export B4", true, configuration.AdditionalSupplyChainActorSupport(declaration));

			entryInstruction.CEI_Style = "C1";
			AssertEquals("Export C1", true, configuration.AdditionalSupplyChainActorSupport(declaration));

			entryInstruction.CEI_Style = "C2";
			AssertEquals("Export C2", false, configuration.AdditionalSupplyChainActorSupport(declaration));

			declaration.JE_MessageType = "IMP";
			entryInstruction.CEI_Style = "H1";
			AssertEquals("Import H1", true, configuration.AdditionalSupplyChainActorSupport(declaration));

			entryInstruction.CEI_Style = "H2";
			AssertEquals("Import H2", true, configuration.AdditionalSupplyChainActorSupport(declaration));

			entryInstruction.CEI_Style = "H3";
			AssertEquals("Import H3", true, configuration.AdditionalSupplyChainActorSupport(declaration));

			entryInstruction.CEI_Style = "H4";
			AssertEquals("Import H4", true, configuration.AdditionalSupplyChainActorSupport(declaration));

			entryInstruction.CEI_Style = "H5";
			AssertEquals("Import H5", true, configuration.AdditionalSupplyChainActorSupport(declaration));

			entryInstruction.CEI_Style = "H6";
			AssertEquals("Import H6", true, configuration.AdditionalSupplyChainActorSupport(declaration));

			entryInstruction.CEI_Style = "I1";
			AssertEquals("Import I1", true, configuration.AdditionalSupplyChainActorSupport(declaration));

			entryInstruction.CEI_Style = "I2";
			AssertEquals("Import I2", false, configuration.AdditionalSupplyChainActorSupport(declaration));

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "H1";
			AssertEquals("Import I2 and H1", false, configuration.AdditionalSupplyChainActorSupport(declaration));

			entryInstruction.CEI_Style = "H2";
			entryInstruction.CEI_Style = "I2";
			AssertEquals("Import I2 and H1 (switched positions) ", false,
				configuration.AdditionalSupplyChainActorSupport(declaration));
		});
	}

	public override void TestAdditionalSupplyChainActorSupport()
	{
		AssertEquals(true, configuration.AuthorisationsSupport(CreateDeclaration()));
	}

	public override void TestGuaranteesSupport() => CombineAssertions(() =>
	{
		var declaration = CreateDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		AssertEquals(true, configuration.GuaranteesSupport(declaration, entryInstruction));

		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		AssertEquals(false, configuration.GuaranteesSupport(declaration, entryInstruction));
	});

	public override void TestSealsSupport()
	{
		AssertEquals(false, configuration.SealsSupport(CreateDeclaration()));
	}

	public override void TestUseEoriForAuthorisationReference() => AssertEquals(false, configuration.UseEoriForAuthorisationReference);

	public override void TestAdditionalInfosSupport()
	{
		var declaration = CreateDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		AssertEquals(true, configuration.AdditionalInfosSupport(declaration, entryInstruction));
	}

	public override void TestSupportingDocumentsSupport()
	{
		AssertEquals(true, configuration.SupportingDocumentsSupport(CreateDeclaration()));
	}

	public override void TestPreviousDocumentsSupport()
	{
		AssertEquals(true, configuration.PreviousDocumentsSupport(CreateDeclaration()));
	}

	public override void TestRequestedDocumentsSupport()
	{
		AssertEquals(false, configuration.RequestedDocumentsSupport(CreateDeclaration()));
	}

	public override void TestSpecialProceduresSupport()
	{
		var declaration = CreateDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		AssertEquals(false, configuration.SpecialProceduresSupport(declaration, entryInstruction));
	}
}
