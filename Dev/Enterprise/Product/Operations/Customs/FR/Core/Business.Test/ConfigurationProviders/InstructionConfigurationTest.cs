using System;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Testing
{
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

		public void TestGetAdditionalInfoValidationDecider()
		{
			var declaration = CreateDeclaration();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertType<UCC6ImportAdditionalInfoValidationDecider>("IsUCC6 IMP", configuration.GetAdditionalInfoValidationDecider(entryInstruction));
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

		public void TestGetPreviousDocumentValidationDecider()
		{
			var declaration = CreateDeclaration();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertType<UCC6ImportPreviousDocumentValidationDecider>("IsUCC6 IMP", configuration.GetPreviousDocumentValidationDecider(entryInstruction));
			}
		}

		public override void TestFiscalReferencesSupport() => AssertEquals(true, configuration.FiscalReferencesSupport(CreateDeclaration()));

		public override void TestFiscalReferencesSupportOnCPC42And63Only() => AssertEquals(false, configuration.FiscalReferencesSupportOnCPC42And63Only(CreateDeclaration()));

		public override void TestAuthorisationsSupport() => AssertEquals(true, configuration.AuthorisationsSupport(CreateDeclaration()));

		public override void TestPreviousDocumentsSupport()
		{
			var declaration = CreateDeclaration();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(true, configuration.PreviousDocumentsSupport(declaration));

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals(false, configuration.PreviousDocumentsSupport(declaration));
		}

		public override void TestSupportingDocumentsSupport()
		{
			var declaration = CreateDeclaration();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(true, configuration.SupportingDocumentsSupport(declaration));

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals(false, configuration.SupportingDocumentsSupport(declaration));
		}

		public override void TestAdditionalInfosSupport()
		{
			var declaration = CreateDeclaration();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(true, configuration.AdditionalInfosSupport(declaration, entryInstruction));

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals(false, configuration.AdditionalInfosSupport(declaration, entryInstruction));
		}

		public override void TestAdditionalSupplyChainActorSupport()
		{
			var declaration = CreateDeclaration();

			CombineAssertions(() =>
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Export && Not UCC6", false, configuration.AdditionalSupplyChainActorSupport(declaration));

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import && Not UCC6", false, configuration.AdditionalSupplyChainActorSupport(declaration));

				using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
				using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
				{
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					AssertEquals("Export && UCC6", false, configuration.AdditionalSupplyChainActorSupport(declaration));

					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					AssertEquals("Import && UCC6", true, configuration.AdditionalSupplyChainActorSupport(declaration));
				}
			});
		}

		public override void TestGuaranteesSupport() => CombineAssertions(() =>
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("Declaration is Delta G so configuration.GuaranteesSupport should return false.", false, configuration.GuaranteesSupport(declaration, entryInstruction));

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("Declaration is Delta IE with no entry instruction style so configuration.GuaranteesSupport should return true.", false, configuration.GuaranteesSupport(declaration, entryInstruction));

			foreach (var style in new DeltaIEImportDeclarationTypeList().GetAllCodes())
			{
				entryInstruction.CEI_Style = style;
				switch (style)
				{
					case DeltaIEImportDeclarationTypeList.Codes.H1:
					case DeltaIEImportDeclarationTypeList.Codes.H2:
					case DeltaIEImportDeclarationTypeList.Codes.H3:
					case DeltaIEImportDeclarationTypeList.Codes.H4:
						AssertEquals($"Declaration is Delta IE with entry instruction style {entryInstruction.CEI_Style} so configuration.GuaranteesSupport should return true.", true, configuration.GuaranteesSupport(declaration, entryInstruction));
						break;
					default:
						AssertEquals($"Declaration is Delta IE with entry instruction style {entryInstruction.CEI_Style} so configuration.GuaranteesSupport should return false.", false, configuration.GuaranteesSupport(declaration, entryInstruction));
						break;
				}
			}
		});

		public override void TestSealsSupport() => AssertEquals(false, configuration.SealsSupport(CreateDeclaration()));

		public override void TestUseEoriForAuthorisationReference() => AssertEquals(false, configuration.UseEoriForAuthorisationReference);

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
}
