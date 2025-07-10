using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(InstructionConfiguration))]
sealed class InstructionConfigurationTest : InstructionConfigurationAbstractTest<InstructionConfiguration>
{
	public void TestEntryInstructionValidationDecider()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertType<UCC6ImportEntryInstructionValidationDecider>(configuration.GetValidationDecider(entryInstruction));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertNull(configuration.GetValidationDecider(entryInstruction));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertNull(configuration.GetValidationDecider(entryInstruction));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertNull(configuration.GetValidationDecider(entryInstruction));
		}
	}

	public void TestGetSupportingDocumentValidationDecider()
	{
		var declaration = CreateDeclaration();
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertType<UCC6ImportSupportingDocumentValidationDecider>("IsUCC6 IMP", configuration.GetSupportingDocumentValidationDecider(entryInstruction));
		}
	}

	public override void TestFiscalReferencesSupportOnCPC42And63Only()
	{
		AssertEquals(true, configuration.FiscalReferencesSupportOnCPC42And63Only(CreateDeclaration()));
	}

	public override void TestAuthorisationsSupport()
	{
		AssertEquals(true, configuration.AuthorisationsSupport(CreateDeclaration()));
	}

	public override void TestAdditionalSupplyChainActorSupport()
	{
		var declaration = CreateDeclaration();

		CombineAssertions("When UCC6 true", () =>
		{
			declaration.JE_MessageType = "IMP";
			AssertEquals("Pre: For IMP IsUCC6", true, declaration.Configuration.IsUCC6(declaration));
			AssertEquals(true, configuration.AdditionalSupplyChainActorSupport(declaration));
		});

		CombineAssertions("When UCC6 false", () =>
		{
			declaration.JE_MessageType = "EXP";
			AssertEquals("Pre: For EXP IsUCC6", false, declaration.Configuration.IsUCC6(declaration));
			AssertEquals(false, configuration.AdditionalSupplyChainActorSupport(declaration));
		});
	}

	public override void TestGuaranteesSupport() => CombineAssertions(() =>
	{
		var declaration = CreateDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		declaration.JE_MessageType = "EXP";
		AssertEquals("Declaration is EXP so configuration.GuaranteesSupport should return false.", false, configuration.GuaranteesSupport(declaration, entryInstruction));

		declaration.JE_MessageType = "IMP";
		AssertEquals($"Declaration is IMP with no entry instruction style so configuration.GuaranteesSupport should return false.", false, configuration.GuaranteesSupport(declaration, entryInstruction));

		foreach (var style in new ImportUCC6DeclarationTypeList().GetAllCodes())
		{
			entryInstruction.CEI_Style = style;
			switch (style)
			{
				case ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1:
				case ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3:
				case ImportUCC6DeclarationTypeList.Codes.RegimeSpecialePerfezionamentoAttivoH4:
					AssertEquals($"Declaration is IMP with entry instruction style {entryInstruction.CEI_Style} so configuration.GuaranteesSupport should return true.", true, configuration.GuaranteesSupport(declaration, entryInstruction));
					break;
				default:
					AssertEquals($"Declaration is IMP with entry instruction style {entryInstruction.CEI_Style} so configuration.GuaranteesSupport should return false.", false, configuration.GuaranteesSupport(declaration, entryInstruction));
					break;
			}
		}
	});

	public override void TestSealsSupport()
	{
		var declaration = CreateDeclaration();

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertEquals("When declaration is UCC6, SealsSupport", false, configuration.SealsSupport(declaration));
		}

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			AssertEquals("When declaration is not UCC6, SealsSupport", true, configuration.SealsSupport(declaration));
		}
	}

	public override void TestUseEoriForAuthorisationReference() => AssertEquals(false, configuration.UseEoriForAuthorisationReference);

	public override void TestFiscalReferencesSupport()
	{
		var declaration = CreateDeclaration();

		declaration.JE_MessageType = "IMP";
		AssertEquals(true, configuration.FiscalReferencesSupport(declaration));

		declaration.JE_MessageType = "EXP";
		AssertEquals(false, configuration.FiscalReferencesSupport(declaration));
	}

	public override void TestAdditionalInfosSupport()
	{
		var declaration = CreateDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertEquals("Import", false, configuration.AdditionalInfosSupport(declaration, entryInstruction));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("Export (Non-UCC6)", false, configuration.AdditionalInfosSupport(declaration, entryInstruction));

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertEquals("Export UCC6", true, configuration.AdditionalInfosSupport(declaration, entryInstruction));
		}

		AssertEquals("When Declaration is null", false, configuration.AdditionalInfosSupport(declaration: null, entryInstruction: null));
	}

	public override void TestSupportingDocumentsSupport()
	{
		var declaration = CreateDeclaration();

		declaration.JE_MessageType = "IMP";
		AssertEquals("When declaration is import, SupportingDocumentsSupport", true, configuration.SupportingDocumentsSupport(declaration));

		declaration.JE_MessageType = "EXP";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertEquals("When declaration is export UCC6, SupportingDocumentsSupport", true, configuration.SupportingDocumentsSupport(declaration));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			AssertEquals("When declaration is export not UCC6, SupportingDocumentsSupport", false, configuration.SupportingDocumentsSupport(declaration));
		}
	}

	public override void TestPreviousDocumentsSupport()
	{
		var declaration = CreateDeclaration();

		declaration.JE_MessageType = "IMP";
		AssertEquals("When declaration is import, PreviousDocumentsSupport", true, configuration.PreviousDocumentsSupport(declaration));

		declaration.JE_MessageType = "EXP";
		AssertEquals("When declaration is Export, PreviousDocumentsSupport", false, configuration.PreviousDocumentsSupport(declaration));

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertEquals("When declaration is Ucc6 Export, PreviousDocumentsSupport", true, configuration.PreviousDocumentsSupport(declaration));
		}
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
