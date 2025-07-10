using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(InstructionConfiguration))]
	class InstructionConfigurationTest : InstructionConfigurationAbstractTest<InstructionConfiguration>
	{
		public override void TestFiscalReferencesSupport()
		{
			var declaration = CreateDeclaration();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals(false, configuration.FiscalReferencesSupport(declaration));
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals(true, configuration.FiscalReferencesSupport(declaration));
			}
		}

		public override void TestFiscalReferencesSupportOnCPC42And63Only() => AssertEquals(true, configuration.FiscalReferencesSupportOnCPC42And63Only(CreateDeclaration()));

		public override void TestAuthorisationsSupport() => AssertEquals(true, configuration.AuthorisationsSupport(CreateDeclaration()));

		public override void TestAdditionalSupplyChainActorSupport()
		{
			var declaration = CreateDeclaration();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				AssertEquals("Export", true, configuration.AdditionalSupplyChainActorSupport(declaration));
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import", true, configuration.AdditionalSupplyChainActorSupport(declaration));
			});
		}

		public override void TestGuaranteesSupport() => CombineAssertions(() =>
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Export", false, configuration.GuaranteesSupport(declaration, entryInstruction));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Import", true, configuration.GuaranteesSupport(declaration, entryInstruction));
		});

		public override void TestSealsSupport() => AssertEquals(false, configuration.SealsSupport(CreateDeclaration()));

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
			AssertEquals(true, configuration.RequestedDocumentsSupport(CreateDeclaration()));
		}

		public override void TestSpecialProceduresSupport()
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals(false, configuration.SpecialProceduresSupport(declaration, entryInstruction));
				entryInstruction.CEI_Style = "H1";
				AssertEquals(true, configuration.SpecialProceduresSupport(declaration, entryInstruction));
				entryInstruction.CEI_Style = "H3";
				AssertEquals(true, configuration.SpecialProceduresSupport(declaration, entryInstruction));
				entryInstruction.CEI_Style = "H4";
				AssertEquals(true, configuration.SpecialProceduresSupport(declaration, entryInstruction));

				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertEquals("When Import & UCC5", true, configuration.SpecialProceduresSupport(declaration, entryInstruction));
			});
		}

		public void TestGetAdditionalInfoValidationDecider()
		{
			var declaration = CreateDeclaration();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportAdditionalInfoValidationDecider>("IsUCC6 IMP", configuration.GetAdditionalInfoValidationDecider(entryInstruction));
			}
		}

		public void TestGetSupportingDocumentValidationDecider()
		{
			var declaration = CreateDeclaration();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportSupportingDocumentValidationDecider>("IsUCC6 IMP", configuration.GetSupportingDocumentValidationDecider(entryInstruction));
			}
		}

		public void TestGetCusAuthorizationUsageValidationDecider()
		{
			var declaration = CreateDeclaration();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportCusAuthorizationUsageValidationDecider>("IsUCC6 IMP", configuration.GetCusAuthorizationUsageValidationDecider(entryInstruction));
			}
		}
	}
}
