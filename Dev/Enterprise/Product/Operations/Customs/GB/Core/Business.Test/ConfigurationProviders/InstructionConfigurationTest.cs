using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(InstructionConfiguration))]
	public class InstructionConfigurationTest : InstructionConfigurationAbstractTest<InstructionConfiguration>
	{
		public void TestGetSupportingDocumentValidationDecider()
		{
			var declaration = CreateDeclaration();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertType<UCC6ImportSupportingDocumentValidationDecider>("IsUCC6 IMP", configuration.GetSupportingDocumentValidationDecider(entryInstruction));
			}
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
			AssertEquals(true, configuration.FiscalReferencesSupport(CreateDeclaration()));
		}

		public override void TestFiscalReferencesSupportOnCPC42And63Only()
		{
			AssertEquals("GB uses fiscal references for postponded VAT accounting, so don't limit fiscal references to CPCs 42 and 63 only.", false, configuration.FiscalReferencesSupportOnCPC42And63Only(CreateDeclaration()));
		}

		public override void TestGuaranteesSupport()
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(false, configuration.GuaranteesSupport(declaration, entryInstruction));
		}

		public override void TestSealsSupport()
		{
			AssertEquals(false, configuration.SealsSupport(CreateDeclaration()));
		}

		public override void TestUseEoriForAuthorisationReference() => AssertEquals(true, configuration.UseEoriForAuthorisationReference);

		public override void TestAdditionalInfosSupport()
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(true, configuration.AdditionalInfosSupport(declaration, entryInstruction));
		}

		public override void TestSupportingDocumentsSupport()
		{
			AssertEquals(false, configuration.SupportingDocumentsSupport(CreateDeclaration()));
		}

		public override void TestPreviousDocumentsSupport()
		{
			AssertEquals(false, configuration.PreviousDocumentsSupport(CreateDeclaration()));
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
}
