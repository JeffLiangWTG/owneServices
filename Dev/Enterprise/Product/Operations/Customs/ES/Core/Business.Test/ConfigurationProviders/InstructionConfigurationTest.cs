using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing;

[TestedType(typeof(InstructionConfiguration))]
class InstructionConfigurationTest : InstructionConfigurationAbstractTest<InstructionConfiguration>
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

	public override void TestAuthorisationsSupport() => AssertEquals(true, configuration.AuthorisationsSupport(CreateDeclaration()));

	public override void TestAdditionalSupplyChainActorSupport()
	{
		var declaration = CreateDeclaration();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		AssertEquals(true, configuration.AdditionalSupplyChainActorSupport(declaration));

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		AssertEquals(true, configuration.AdditionalSupplyChainActorSupport(declaration));

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals(false, configuration.AdditionalSupplyChainActorSupport(declaration));
	}

	public override void TestFiscalReferencesSupport()
	{
		var declaration = CreateDeclaration();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			AssertEquals("Import", false, configuration.FiscalReferencesSupport(declaration));
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertEquals("Export", false, configuration.FiscalReferencesSupport(declaration));
		});
	}

	public override void TestGuaranteesSupport()
	{
		var declaration = CreateDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		AssertEquals(false, configuration.GuaranteesSupport(declaration, entryInstruction));
	}

	public override void TestFiscalReferencesSupportOnCPC42And63Only() => AssertEquals(true, configuration.FiscalReferencesSupportOnCPC42And63Only(CreateDeclaration()));

	public override void TestSealsSupport()
	{
		CombineAssertions(() =>
		{
			var declaration = CreateDeclaration();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			AssertEquals("SealsSupport is false when Import", false, configuration.SealsSupport(declaration));

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertEquals("SealsSupport is false when export", false, configuration.SealsSupport(declaration));
		});
	}

	public override void TestUseEoriForAuthorisationReference() => AssertEquals(false, configuration.UseEoriForAuthorisationReference);

	public override void TestAdditionalInfosSupport()
	{
		var declaration = CreateDeclaration();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("AdditionalInfosTabPage visible when export and AES", true, configuration.AdditionalInfosSupport(declaration, null));

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("AdditionalInfosTabPage visible when import and AES", true, configuration.AdditionalInfosSupport(declaration, null));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				AssertEquals("AdditionalInfosTabPage visible when export EXS and AES", true, configuration.AdditionalInfosSupport(declaration, entryInstruction));
			}

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("AdditionalInfosTabPage visible when export and AES1.1", true, configuration.AdditionalInfosSupport(declaration, null));

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("AdditionalInfosTabPage visible when import and AES1.1", true, configuration.AdditionalInfosSupport(declaration, null));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				AssertEquals("AdditionalInfosTabPage visible when export EXS and AES1.1", true, configuration.AdditionalInfosSupport(declaration, entryInstruction));
			}
		});
	}

	public override void TestSupportingDocumentsSupport()
	{
		AssertEquals(true, configuration.SupportingDocumentsSupport(CreateDeclaration()));
	}

	public override void TestPreviousDocumentsSupport()
	{
		var declaration = CreateDeclaration();

		CombineAssertions("PreviousDocuments should be supported only when JE_MessageType = IMP and declaration is UCC6.", () =>
		{
			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Import and UCC6.", true, configuration.PreviousDocumentsSupport(declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Export and UCC6.", false, configuration.PreviousDocumentsSupport(declaration));
			}

			using (RegistryTemporarySetterHelper.SetESImportMessageVersion(IMPORTVersionNumberList.Codes.Ics))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Import and ICS.", false, configuration.PreviousDocumentsSupport(declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Export and ICS.", false, configuration.PreviousDocumentsSupport(declaration));
			}
		});
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

	protected override EU.Business.Declaration.JobDeclaration CreateDeclaration() => Factory.New<JobDeclaration>();
}
