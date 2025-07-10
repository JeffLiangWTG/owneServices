using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(DeclarationConfiguration))]
	public class DeclarationConfigurationTest : EU.Business.Testing.DeclarationConfigurationAbstractTest<DeclarationConfiguration, InstructionConfiguration, InvoiceHeaderConfiguration, InvoiceLineConfiguration, EntryHeaderConfiguration, EntryLineConfiguration>
	{
		public void TestUseEucdmSupportingDocumentGoodsShipment()
		{
			AssertEquals(false, configuration.UseEucdmSupportingDocumentGoodsShipment);
		}

		public override void TestUCCAdditionalInfosSupport()
		{
			CombineAssertions(() =>
			{
				AssertUCCAdditionalInfosSupport(
					(EXPORTVersionNumberList.Codes.Aes, IMPORTVersionNumberList.Codes.H1, true),
					expected: (export: true, import: true));

				AssertUCCAdditionalInfosSupport(
					(EXPORTVersionNumberList.Codes.Aes, IMPORTVersionNumberList.Codes.H1, false),
					expected: (export: true, import: true));

				AssertUCCAdditionalInfosSupport(
					(EXPORTVersionNumberList.Codes.Aes, IMPORTVersionNumberList.Codes.Ics, true),
					expected: (export: true, import: true));

				AssertUCCAdditionalInfosSupport(
					(EXPORTVersionNumberList.Codes.Aes, IMPORTVersionNumberList.Codes.Ics, false),
					expected: (export: true, import: false));

				AssertUCCAdditionalInfosSupport(
					(EXPORTVersionNumberList.Codes.Aes11, IMPORTVersionNumberList.Codes.H1, true),
					expected: (export: true, import: true));

				AssertUCCAdditionalInfosSupport(
					(EXPORTVersionNumberList.Codes.Aes11, IMPORTVersionNumberList.Codes.H1, false),
					expected: (export: true, import: true));

				AssertUCCAdditionalInfosSupport(
					(EXPORTVersionNumberList.Codes.Aes11, IMPORTVersionNumberList.Codes.Ics, true),
					expected: (export: true, import: true));

				AssertUCCAdditionalInfosSupport(
					(EXPORTVersionNumberList.Codes.Aes11, IMPORTVersionNumberList.Codes.Ics, false),
					expected: (export: true, import: false));
			});

			void AssertUCCAdditionalInfosSupport(
				(string exportMessageVersion, string importMessageVersion, bool importUCC6Funcs) testCaseSetup,
				(bool export, bool import) expected)
			{
				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(testCaseSetup.exportMessageVersion))
				using (RegistryTemporarySetterHelper.SetESImportMessageVersion(testCaseSetup.importMessageVersion))
				using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(testCaseSetup.importUCC6Funcs))
				{
					var setupDescription = $"(ESExportMessageVersion: {testCaseSetup.exportMessageVersion}, ESImportMessageVersion: {testCaseSetup.importMessageVersion}, FUNCS ImportMessageVersionUCC6: {testCaseSetup.importUCC6Funcs})";

					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					AssertEquals($"UCCAdditionalInfosSupport for Export {setupDescription}", expected.export, configuration.UCCAdditionalInfosSupport(declaration));

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					AssertEquals($"UCCAdditionalInfosSupport for Import {setupDescription}", expected.import, configuration.UCCAdditionalInfosSupport(declaration));
				}
			}
		}

		public override void TestMiscAdditionalInfosSupport()
		{
			AssertEquals(true, configuration.MiscAdditionalInfosSupport(declaration));
		}

		public override void TestMiscSupportingDocumentsSupport()
		{
			AssertEquals(true, configuration.MiscSupportingDocumentsSupport(declaration));
		}

		public override void TestMiscPreviousDocumentsSupport()
		{
			AssertEquals(false, configuration.MiscPreviousDocumentsSupport(declaration));
		}

		public override void TestMiscGuaranteesSupport()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Supported for non IImportExport", true, configuration.MiscGuaranteesSupport(Factory.New<DummyBusinessObject>()));
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Misc Guarantees not supported for non import", false, configuration.MiscGuaranteesSupport(declaration));
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Misc Guarantees supported for import", true, configuration.MiscGuaranteesSupport(declaration));
			});
		}

		public override void TestLockNumberOfEntryLinesForRegisteredEntry()
		{
			AssertEquals(true, configuration.LockNumberOfEntryLinesForRegisteredEntry);
		}

		public override void TestIsUCC5()
		{
			AssertEquals(false, configuration.IsUCC5(Factory.New<DummyBusinessObject>()));
		}

		public override void TestIsUCC6()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();

			CombineAssertions(() =>
			{
				AssertIsUCC6(
					(EXPORTVersionNumberList.Codes.Aes, IMPORTVersionNumberList.Codes.Ics, importUCC6Funcs: false),
					expected: (exportUCC6: true, importUCC6: false, importUCC6ForNull: true, importUCC6ForNonIImportExport: true));

				AssertIsUCC6(
					(EXPORTVersionNumberList.Codes.Aes, IMPORTVersionNumberList.Codes.Ics, importUCC6Funcs: true),
					expected: (exportUCC6: true, importUCC6: true, importUCC6ForNull: true, importUCC6ForNonIImportExport: true));

				AssertIsUCC6(
					(EXPORTVersionNumberList.Codes.Aes, IMPORTVersionNumberList.Codes.H1, importUCC6Funcs: false),
					expected: (exportUCC6: true, importUCC6: true, importUCC6ForNull: true, importUCC6ForNonIImportExport: true));

				AssertIsUCC6(
					(EXPORTVersionNumberList.Codes.Aes, IMPORTVersionNumberList.Codes.H1, importUCC6Funcs: true),
					expected: (exportUCC6: true, importUCC6: true, importUCC6ForNull: true, importUCC6ForNonIImportExport: true));

				AssertIsUCC6(
					(EXPORTVersionNumberList.Codes.Aes11, IMPORTVersionNumberList.Codes.Ics, importUCC6Funcs: false),
					expected: (exportUCC6: true, importUCC6: false, importUCC6ForNull: true, importUCC6ForNonIImportExport: true));

				AssertIsUCC6(
					(EXPORTVersionNumberList.Codes.Aes11, IMPORTVersionNumberList.Codes.Ics, importUCC6Funcs: true),
					expected: (exportUCC6: true, importUCC6: true, importUCC6ForNull: true, importUCC6ForNonIImportExport: true));

				AssertIsUCC6(
					(EXPORTVersionNumberList.Codes.Aes11, IMPORTVersionNumberList.Codes.H1, importUCC6Funcs: false),
					expected: (exportUCC6: true, importUCC6: true, importUCC6ForNull: true, importUCC6ForNonIImportExport: true));

				AssertIsUCC6(
					(EXPORTVersionNumberList.Codes.Aes11, IMPORTVersionNumberList.Codes.H1, importUCC6Funcs: true),
					expected: (exportUCC6: true, importUCC6: true, importUCC6ForNull: true, importUCC6ForNonIImportExport: true));
			});

			void AssertIsUCC6(
				(string exportMessageVersion, string importMessageVersion, bool importUCC6Funcs) testCaseSetup,
				(bool exportUCC6, bool importUCC6, bool importUCC6ForNull, bool importUCC6ForNonIImportExport) expected)
			{
				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(testCaseSetup.exportMessageVersion))
				using (RegistryTemporarySetterHelper.SetESImportMessageVersion(testCaseSetup.importMessageVersion))
				using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(testCaseSetup.importUCC6Funcs))
				{
					var testCondition = $"(Export version: {testCaseSetup.exportMessageVersion}; Import version: {testCaseSetup.importMessageVersion}; FUNCS IMUC6: {testCaseSetup.importUCC6Funcs})";

					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					AssertEquals($"UCC6 for Export {testCondition}", expected.exportUCC6, configuration.IsUCC6(declaration));

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					AssertEquals($"UCC6 for Import {testCondition}", expected.importUCC6, configuration.IsUCC6(declaration));

					AssertEquals($"UCC6 for null object {testCondition}", expected.importUCC6ForNull, configuration.IsUCC6(null));

					AssertEquals($"UCC6 dummyBO (not IImportExport) object {testCondition}", expected.importUCC6ForNonIImportExport, configuration.IsUCC6(dummyBO));
				}
			}
		}

		public void TestIsTransitionPeriodAES30()
		{
			CombineAssertions(() =>
			{
				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
				{
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
					{
						declaration.JE_MessageType = "EXP";
						AssertEquals("When AESTP is set to false, declaration is EXP and CustomsMessageVersion is AES", true, configuration.IsTransitionPeriodAES30(declaration));

						declaration.JE_MessageType = "IMP";
						AssertEquals("When AESTP is set to false, declaration is IMP and CustomsMessageVersion is AES", false, configuration.IsTransitionPeriodAES30(declaration));
					}

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
					{
						declaration.JE_MessageType = "EXP";
						AssertEquals("When AESTP is set to true, declaration is EXP and CustomsMessageVersion is AES", true, configuration.IsTransitionPeriodAES30(declaration));

						declaration.JE_MessageType = "IMP";
						AssertEquals("When AESTP is set to true, declaration is IMP and CustomsMessageVersion is AES", false, configuration.IsTransitionPeriodAES30(declaration));
					}
				}

				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
				{
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
					{
						declaration.JE_MessageType = "EXP";
						AssertEquals("When AESTP is set to false, declaration is EXP and CustomsMessageVersion is AES1.1", false, configuration.IsTransitionPeriodAES30(declaration));

						declaration.JE_MessageType = "IMP";
						AssertEquals("When AESTP is set to false, declaration is IMP and CustomsMessageVersion is AES1.1", false, configuration.IsTransitionPeriodAES30(declaration));
					}

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
					{
						declaration.JE_MessageType = "EXP";
						AssertEquals("When AESTP is set to true, declaration is EXP and CustomsMessageVersion is AES1.1", true, configuration.IsTransitionPeriodAES30(declaration));

						declaration.JE_MessageType = "IMP";
						AssertEquals("When AESTP is set to true, declaration is IMP and CustomsMessageVersion is AES1.1", false, configuration.IsTransitionPeriodAES30(declaration));
					}
				}
			});
		}

		public void TestHasImportUCC6Functionality()
		{
			CombineAssertions(() =>
			{
				AssertHasImportUCC6Functionality(importMessageVersion: IMPORTVersionNumberList.Codes.Ics, importUCC6Funcs: true);
				AssertHasImportUCC6Functionality(importMessageVersion: IMPORTVersionNumberList.Codes.Ics, importUCC6Funcs: false);
				AssertHasImportUCC6Functionality(importMessageVersion: IMPORTVersionNumberList.Codes.H1, importUCC6Funcs: true);
				AssertHasImportUCC6Functionality(importMessageVersion: IMPORTVersionNumberList.Codes.H1, importUCC6Funcs: false);
			});

			void AssertHasImportUCC6Functionality(string importMessageVersion, bool importUCC6Funcs)
			{
				using (RegistryTemporarySetterHelper.SetESImportMessageVersion(importMessageVersion))
				using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(importUCC6Funcs))
				{
					var testCondition = $"(Registry Import version: {importMessageVersion}; FUNCS IMUC6: {importUCC6Funcs})";

					AssertEquals($"HasImportUCC6Functionality {testCondition}", importUCC6Funcs, DeclarationConfiguration.HasImportUCC6Functionality());
				}
			}
		}

		public override void TestIsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice()
		{
			AssertEquals(false, configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice);
		}

		public override void TestUseUniversalFeeCalculation()
		{
			AssertEquals(true, configuration.UseUniversalFeeCalculation(declaration));
		}

		public override void TestDV1DetailsSupport()
		{
			AssertEquals(false, configuration.DV1DetailsSupport(declaration));
		}

		public override void TestShouldCheckLegalByDeclarantType()
		{
			AssertEquals(false, configuration.ShouldCheckLegalByDeclarantType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;
	}
}
