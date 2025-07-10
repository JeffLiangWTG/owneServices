using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestsSubclassesOf(typeof(InstructionConfiguration))]
	public abstract class InstructionConfigurationAbstractTest<L> : TestCaseWithFactory
		where L : InstructionConfiguration
	{
		public abstract void TestFiscalReferencesSupport();

		public abstract void TestFiscalReferencesSupportOnCPC42And63Only();

		public abstract void TestAuthorisationsSupport();

		public abstract void TestAdditionalSupplyChainActorSupport();

		public abstract void TestGuaranteesSupport();

		public abstract void TestSealsSupport();

		public abstract void TestUseEoriForAuthorisationReference();

		public abstract void TestAdditionalInfosSupport();

		public abstract void TestSupportingDocumentsSupport();

		public abstract void TestPreviousDocumentsSupport();

		public abstract void TestRequestedDocumentsSupport();

		public abstract void TestSpecialProceduresSupport();

		protected virtual JobDeclaration CreateDeclaration() => Factory.New<JobDeclaration>();

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (L)Activator.CreateInstance(typeof(L));
		}
		protected L configuration;
	}

	[TestedType(typeof(InstructionConfiguration))]
	sealed class InstructionConfigurationBaseOnlyTest : InstructionConfigurationAbstractTest<InstructionConfiguration>
	{
		[ExpectNoExceptions]
		public override void TestFiscalReferencesSupport()
		{
			var declaration = CreateDeclaration();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.FiscalReferencesSupport(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IsUCC6 IMP");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.FiscalReferencesSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IsUCC6 EXP");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					NUnit.Framework.Assert.That(configuration.FiscalReferencesSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "!IsUCC6");
				}
			});
		}

		[ExpectNoExceptions]
		public override void TestFiscalReferencesSupportOnCPC42And63Only()
		{
			NUnit.Framework.Assert.That(configuration.FiscalReferencesSupportOnCPC42And63Only(CreateDeclaration()), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAuthorisationsSupport()
		{
			NUnit.Framework.Assert.That(configuration.AuthorisationsSupport(CreateDeclaration()), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAdditionalSupplyChainActorSupport()
		{
			NUnit.Framework.Assert.That(configuration.AdditionalSupplyChainActorSupport(CreateDeclaration()), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestGuaranteesSupport()
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(configuration.GuaranteesSupport(declaration, entryInstruction), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Default config");

				using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionGuaranteesSupportConfiguration(declaration, false), false))
				{
					configuration = declaration.Configuration.InstructionConfiguration;
					declaration.JE_MessageType = "EXP";
					NUnit.Framework.Assert.That(configuration.GuaranteesSupport(declaration, entryInstruction), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Not UCC6, EXP");

					declaration.JE_MessageType = "IMP";
					NUnit.Framework.Assert.That(configuration.GuaranteesSupport(declaration, entryInstruction), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Not UCC6, IMP");
				}

				using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionGuaranteesSupportConfiguration(declaration, false), true))
				{
					configuration = declaration.Configuration.InstructionConfiguration;
					declaration.JE_MessageType = "EXP";
					NUnit.Framework.Assert.That(configuration.GuaranteesSupport(declaration, entryInstruction), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "UCC6, EXP, no GuaranteeSupport");

					declaration.JE_MessageType = "IMP";
					NUnit.Framework.Assert.That(configuration.GuaranteesSupport(declaration, entryInstruction), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "UCC6, IMP, no GuaranteeSupport");
				}

				using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionGuaranteesSupportConfiguration(declaration, true), true))
				{
					configuration = declaration.Configuration.InstructionConfiguration;
					declaration.JE_MessageType = "EXP";
					NUnit.Framework.Assert.That(configuration.GuaranteesSupport(declaration, entryInstruction), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "UCC6, EXP, GuaranteeSupport");

					declaration.JE_MessageType = "IMP";
					NUnit.Framework.Assert.That(configuration.GuaranteesSupport(declaration, entryInstruction), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "UCC6, IMP, GuaranteeSupport");
				}
			});
		}

		[ExpectNoExceptions]
		public override void TestSealsSupport()
		{
			NUnit.Framework.Assert.That(configuration.SealsSupport(CreateDeclaration()), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestUseEoriForAuthorisationReference() => NUnit.Framework.Assert.That(configuration.UseEoriForAuthorisationReference, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));

		[ExpectNoExceptions]
		public void TestIsAESFullUCC6()
		{
			var today = ZDateTime.Today;

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_DateForDuty = today;

			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESPLUS, "EUN", today, true))
				{
					NUnit.Framework.Assert.That(configuration.IsAESFullUCC6(entryInstruction), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "When DateForDuty is equal to effective date, IsAESFullUCC6");

					entryInstruction.CEI_DateForDuty = today.AddDays(1);
					NUnit.Framework.Assert.That(configuration.IsAESFullUCC6(entryInstruction), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "When DateForDuty greater equal to effective date, IsAESFullUCC6");
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESPLUS, "ABC", today, true))
				{
					entryInstruction.CEI_DateForDuty = today;
					NUnit.Framework.Assert.That(configuration.IsAESFullUCC6(entryInstruction), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "When DateForDuty is equal to effective date but not EUN valid configuration, IsAESFullUCC6");
				}
			});
		}

		[ExpectNoExceptions]
		public override void TestAdditionalInfosSupport()
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			NUnit.Framework.Assert.That(configuration.AdditionalInfosSupport(declaration, entryInstruction), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "AdditionalInfosSupport");
		}

		[ExpectNoExceptions]
		public override void TestSupportingDocumentsSupport()
		{
			NUnit.Framework.Assert.That(configuration.SupportingDocumentsSupport(CreateDeclaration()), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "SupportingDocumentsSupport");
		}

		[ExpectNoExceptions]
		public override void TestPreviousDocumentsSupport()
		{
			NUnit.Framework.Assert.That(configuration.PreviousDocumentsSupport(CreateDeclaration()), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestRequestedDocumentsSupport()
		{
			NUnit.Framework.Assert.That(configuration.RequestedDocumentsSupport(CreateDeclaration()), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestSpecialProceduresSupport()
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			NUnit.Framework.Assert.That(configuration.SpecialProceduresSupport(declaration, entryInstruction), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetValidationDecider()
		{
			CombineAssertions(() =>
			{
				var declaration = CreateDeclaration();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(entryInstruction), NUnit.Framework.Is.TypeOf<UCC6ExportEntryInstructionValidationDecider>(), "UCC6 EXP");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(entryInstruction), NUnit.Framework.Is.TypeOf<UCC6ImportEntryInstructionValidationDecider>(), "UCC6 IMP");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(entryInstruction), NUnit.Framework.Is.EqualTo(default(IEntryInstructionValidationDecider)), "Not UCC6 EXP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(entryInstruction), NUnit.Framework.Is.EqualTo(default(IEntryInstructionValidationDecider)), "Not UCC6 IMP - should be [null]");
				}
			});
		}

		[ExpectNoExceptions]
		public void TestGetSupportingDocumentValidationDecider()
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetSupportingDocumentValidationDecider(entryInstruction), NUnit.Framework.Is.TypeOf<UCC6ImportSupportingDocumentValidationDecider>(), "IsUCC6 IMP");
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.GetSupportingDocumentValidationDecider(entryInstruction), NUnit.Framework.Is.EqualTo(default(ISupportingDocumentValidationDecider)), "IsUCC6 EXP - should be [null]");
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetSupportingDocumentValidationDecider(entryInstruction), NUnit.Framework.Is.EqualTo(default(ISupportingDocumentValidationDecider)), "!IsUCC6 - should be [null]");
			}
		}

		[ExpectNoExceptions]
		public void TestGetAdditionalInfoValidationDecider()
		{
			var declaration = CreateDeclaration();
			NUnit.Framework.Assert.That(declaration, NUnit.Framework.Is.Not.EqualTo(default(JobDeclaration)), "[PRE-CONDITION] declaration should not be null - should not be [null]");
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			NUnit.Framework.Assert.That(entryInstruction, NUnit.Framework.Is.Not.EqualTo(default(CusEntryInstruction)), "[PRE-CONDITION] entryInstruction should not be null - should not be [null]");
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetAdditionalInfoValidationDecider(entryInstruction), NUnit.Framework.Is.TypeOf<UCC6ImportAdditionalInfoValidationDecider>(), "IsUCC6 IMP ");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetAdditionalInfoValidationDecider(entryInstruction), NUnit.Framework.Is.EqualTo(default(IAdditionalInfoValidationDecider)), "IsUCC6 EXP - should be [null]");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetAdditionalInfoValidationDecider(entryInstruction), NUnit.Framework.Is.EqualTo(default(IAdditionalInfoValidationDecider)), "!IsUCC6 IMP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetAdditionalInfoValidationDecider(entryInstruction), NUnit.Framework.Is.EqualTo(default(IAdditionalInfoValidationDecider)), "!IsUCC6 EXP - should be [null]");
				}
			});
		}

		[ExpectNoExceptions]
		public void TestGetCusFiscalReferenceValidationDecider()
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetCusFiscalReferenceValidationDecider(entryInstruction), NUnit.Framework.Is.TypeOf<UCC6ImportCusFiscalReferenceValidationDecider>(), "IsUCC6 IMP");
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.GetCusFiscalReferenceValidationDecider(entryInstruction), NUnit.Framework.Is.EqualTo(default(ICusFiscalReferenceValidationDecider)), "IsUCC6 EXP - should be [null]");
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetCusFiscalReferenceValidationDecider(entryInstruction), NUnit.Framework.Is.EqualTo(default(ICusFiscalReferenceValidationDecider)), "!IsUCC6 - should be [null]");
			}
		}

		[ExpectNoExceptions]
		public void TestGetCusAuthorizationUsageValidationDecider()
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetCusAuthorizationUsageValidationDecider(entryInstruction), NUnit.Framework.Is.TypeOf<UCC6ImportCusAuthorizationUsageValidationDecider>(), "IsUCC6 IMP");
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.GetCusAuthorizationUsageValidationDecider(entryInstruction), NUnit.Framework.Is.TypeOf<UCC6ExportCusAuthorizationUsageValidationDecider>(), "IsUCC6 EXP");
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetCusAuthorizationUsageValidationDecider(entryInstruction), NUnit.Framework.Is.EqualTo(default(ICusAuthorizationUsageValidationDecider)), "!IsUCC6 - should be [null]");
			}
		}

		[ExpectNoExceptions]
		public void TestGetPreviousDocumentValidationDecider()
		{
			CombineAssertions(() =>
			{
				var declaration = CreateDeclaration();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(entryInstruction), NUnit.Framework.Is.EqualTo(default(IPreviousDocumentValidationDecider)), "UCC6 EXP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(entryInstruction), NUnit.Framework.Is.TypeOf<UCC6ImportPreviousDocumentValidationDecider>(), "UCC6 IMP");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(entryInstruction), NUnit.Framework.Is.EqualTo(default(IPreviousDocumentValidationDecider)), "Not UCC6 EXP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(entryInstruction), NUnit.Framework.Is.EqualTo(default(IPreviousDocumentValidationDecider)), "Not UCC6 IMP - should be [null]");
				}
			});
		}
	}
}
