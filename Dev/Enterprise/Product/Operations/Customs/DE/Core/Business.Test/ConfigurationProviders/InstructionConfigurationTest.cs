using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(InstructionConfiguration))]
	class InstructionConfigurationTest : InstructionConfigurationAbstractTest<InstructionConfiguration>
	{
		[ExpectNoExceptions]
		public void TestGetSupportingDocumentValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				NUnit.Framework.Assert.That(configuration.GetSupportingDocumentValidationDecider(entryInstruction), Is.TypeOf<UCC6ImportSupportingDocumentValidationDecider>(), "IsUCC6 IMP");
			}
		}

		[ExpectNoExceptions]
		public override void TestAdditionalInfosSupport()
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			NUnit.Framework.Assert.That(configuration.AdditionalInfosSupport(declaration, entryInstruction), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAdditionalSupplyChainActorSupport()
		{
			NUnit.Framework.Assert.That(configuration.AdditionalSupplyChainActorSupport(CreateDeclaration()), Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAuthorisationsSupport()
		{
			NUnit.Framework.Assert.That(configuration.AuthorisationsSupport(CreateDeclaration()), Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestFiscalReferencesSupport()
		{
			NUnit.Framework.Assert.That(configuration.FiscalReferencesSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestFiscalReferencesSupportOnCPC42And63Only()
		{
			NUnit.Framework.Assert.That(configuration.FiscalReferencesSupportOnCPC42And63Only(CreateDeclaration()), Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestGuaranteesSupport()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			NUnit.Framework.Assert.That(configuration.GuaranteesSupport(declaration, entryInstruction), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestPreviousDocumentsSupport()
		{
			NUnit.Framework.Assert.That(configuration.PreviousDocumentsSupport(CreateDeclaration()), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestRequestedDocumentsSupport()
		{
			NUnit.Framework.Assert.That(configuration.RequestedDocumentsSupport(CreateDeclaration()), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestSealsSupport()
		{
			NUnit.Framework.Assert.That(configuration.SealsSupport(CreateDeclaration()), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestSpecialProceduresSupport()
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			NUnit.Framework.Assert.That(configuration.SpecialProceduresSupport(declaration, entryInstruction), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestSupportingDocumentsSupport()
		{
			NUnit.Framework.Assert.That(configuration.SupportingDocumentsSupport(CreateDeclaration()), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestUseEoriForAuthorisationReference()
		{
			NUnit.Framework.Assert.That(configuration.UseEoriForAuthorisationReference, Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestImportValidationDecider()
		{
			var instructionConfiguration = new InstructionConfigurationForTest();
			NUnit.Framework.Assert.That(instructionConfiguration.GetImportValidationDeciderExposed(null), Is.TypeOf<UCC6ImportEntryInstructionValidationDecider>());
		}

		[ExpectNoExceptions]
		public void TestExportValidationDecider()
		{
			var instructionConfiguration = new InstructionConfigurationForTest();
			NUnit.Framework.Assert.That(instructionConfiguration.GetExportValidationDeciderExposed(null), Is.TypeOf<UCC6ExportEntryInstructionValidationDecider>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;

		class InstructionConfigurationForTest : InstructionConfiguration
		{
			public EU.Business.Declaration.IEntryInstructionValidationDecider GetImportValidationDeciderExposed(CusEntryInstruction cusEntryInstruction)
				=> GetImportValidationDecider(cusEntryInstruction);

			public EU.Business.Declaration.IEntryInstructionValidationDecider GetExportValidationDeciderExposed(CusEntryInstruction cusEntryInstruction)
				=> GetExportValidationDecider(cusEntryInstruction);
		}
	}
}
