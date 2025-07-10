using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

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
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertType<UCC6ImportSupportingDocumentValidationDecider>("IsUCC6 IMP", configuration.GetSupportingDocumentValidationDecider(entryInstruction));
	}

	public override void TestAdditionalInfosSupport()
	{
		var declaration = CreateDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		AssertEquals("AdditionalInfosSupport", true, configuration.AdditionalInfosSupport(declaration, entryInstruction));
	}

	public override void TestAdditionalSupplyChainActorSupport()
	{
		AssertEquals(false, configuration.AdditionalSupplyChainActorSupport(CreateDeclaration()));
	}

	public override void TestAuthorisationsSupport()
	{
		AssertEquals(true, configuration.AuthorisationsSupport(CreateDeclaration()));
	}

	public override void TestFiscalReferencesSupport()
	{
		var declaration = CreateDeclaration();
		CombineAssertions(() =>
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertEquals("IsUCC6 IMP", true, configuration.FiscalReferencesSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertEquals("IsUCC6 EXP", false, configuration.FiscalReferencesSupport(declaration));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				AssertEquals("!IsUCC6", false, configuration.FiscalReferencesSupport(declaration));
			}
		});
	}

	public override void TestFiscalReferencesSupportOnCPC42And63Only()
	{
		AssertEquals(true, configuration.FiscalReferencesSupportOnCPC42And63Only(CreateDeclaration()));
	}

	public override void TestGuaranteesSupport() => CombineAssertions(() =>
	{
		var declaration = CreateDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		AssertEquals("Export declaration, Guarantees are not supported", false, configuration.GuaranteesSupport(declaration, entryInstruction));

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertEquals("Import declaration, Guarantees are supported", true, configuration.GuaranteesSupport(declaration, entryInstruction));
	});

	public override void TestPreviousDocumentsSupport()
	{
		AssertEquals(true, configuration.PreviousDocumentsSupport(CreateDeclaration()));
	}

	public override void TestRequestedDocumentsSupport()
	{
		AssertEquals(false, configuration.RequestedDocumentsSupport(CreateDeclaration()));
	}

	public override void TestSealsSupport()
	{
		AssertEquals(false, configuration.SealsSupport(CreateDeclaration()));
	}

	public override void TestSpecialProceduresSupport()
	{
		var declaration = CreateDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		AssertEquals(false, configuration.SpecialProceduresSupport(declaration, entryInstruction));
	}

	public override void TestSupportingDocumentsSupport()
	{
		AssertEquals(true, configuration.SupportingDocumentsSupport(CreateDeclaration()));
	}

	public override void TestUseEoriForAuthorisationReference()
	{
		AssertEquals(false, configuration.UseEoriForAuthorisationReference);
	}
}
