using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using WTG.NUnit;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestsSubclassesOf(typeof(DeclarationConfiguration))]
	public abstract class DeclarationConfigurationAbstractTest<D, I, H, L, Y, E> : TestCaseWithFactory
		where D : DeclarationConfiguration
		where I : InstructionConfiguration
		where H : InvoiceHeaderConfiguration
		where L : InvoiceLineConfiguration
		where Y : EntryHeaderConfiguration
		where E : EntryLineConfiguration
	{
		public abstract void TestUCCAdditionalInfosSupport();

		public abstract void TestMiscAdditionalInfosSupport();

		public abstract void TestMiscSupportingDocumentsSupport();

		public abstract void TestMiscPreviousDocumentsSupport();

		public abstract void TestMiscGuaranteesSupport();

		public abstract void TestDV1DetailsSupport();

		public abstract void TestUseUniversalFeeCalculation();

		public abstract void TestLockNumberOfEntryLinesForRegisteredEntry();

		public abstract void TestIsUCC5();

		public abstract void TestIsUCC6();

		public abstract void TestIsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice();

		public abstract void TestShouldCheckLegalByDeclarantType();

		[ExpectNoExceptions]
		public void TestInstructionConfiguration()
		{
			NUnit.Framework.Assert.That(configuration.InstructionConfiguration, NUnit.Framework.Is.TypeOf<I>());
		}

		[ExpectNoExceptions]
		public void TestInvoiceHeaderConfiguration()
		{
			NUnit.Framework.Assert.That(configuration.InvoiceHeaderConfiguration, NUnit.Framework.Is.TypeOf<H>());
		}

		[ExpectNoExceptions]
		public void TestInvoiceLineConfiguration()
		{
			NUnit.Framework.Assert.That(configuration.InvoiceLineConfiguration, NUnit.Framework.Is.TypeOf<L>());
		}

		[ExpectNoExceptions]
		public void TestEntryHeaderConfiguration()
		{
			NUnit.Framework.Assert.That(configuration.EntryHeaderConfiguration, NUnit.Framework.Is.TypeOf<Y>());
		}

		[ExpectNoExceptions]
		public void TestEntryLineConfiguration()
		{
			NUnit.Framework.Assert.That(configuration.EntryLineConfiguration, NUnit.Framework.Is.TypeOf<E>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (D)Activator.CreateInstance(typeof(D));
		}
		protected D configuration;
	}

	[TestedType(typeof(DeclarationConfiguration))]
	sealed class DeclarationConfigurationBaseOnlyTest : DeclarationConfigurationAbstractTest<DeclarationConfiguration, InstructionConfiguration, InvoiceHeaderConfiguration, InvoiceLineConfiguration, EntryHeaderConfiguration, EntryLineConfiguration>
	{
		[ExpectNoExceptions]
		public void TestGetLineSupporter()
		{
			CombineAssertions(() =>
			{
				var configuration = DeclarationConfiguration.GetConfiguration(Factory, Core.Constants.CountryCodes.EuropeanUnion);
				NUnit.Framework.Assert.That(configuration.GetType().Namespace, NUnit.Framework.Is.EqualTo("Enterprise.Customs.EU.Business"), "Country Code EU returns base");
				configuration = DeclarationConfiguration.GetConfiguration(Factory, ZString.Empty);
				NUnit.Framework.Assert.That(configuration.GetType().Namespace, NUnit.Framework.Is.EqualTo("Enterprise.Customs.EU.Business"), "Empty Country Code returns base");
				configuration = DeclarationConfiguration.GetConfiguration(Factory, Core.Constants.CountryCodes.Italy);
				NUnit.Framework.Assert.That(configuration.GetType().Namespace, NUnit.Framework.Is.EqualTo("Enterprise.Customs.IT.Business"), "Implemented Country Code returns country class");
			});
		}

		[ExpectNoExceptions]
		public override void TestUCCAdditionalInfosSupport()
		{
			NUnit.Framework.Assert.That(configuration.UCCAdditionalInfosSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestMiscAdditionalInfosSupport()
		{
			NUnit.Framework.Assert.That(configuration.MiscAdditionalInfosSupport(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestMiscSupportingDocumentsSupport()
		{
			NUnit.Framework.Assert.That(configuration.MiscSupportingDocumentsSupport(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestMiscPreviousDocumentsSupport()
		{
			NUnit.Framework.Assert.That(configuration.MiscPreviousDocumentsSupport(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestMiscGuaranteesSupport()
		{
			NUnit.Framework.Assert.That(configuration.MiscGuaranteesSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestDV1DetailsSupport()
		{
			NUnit.Framework.Assert.That(configuration.DV1DetailsSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestUseUniversalFeeCalculation()
		{
			NUnit.Framework.Assert.That(configuration.UseUniversalFeeCalculation(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestLockNumberOfEntryLinesForRegisteredEntry()
		{
			NUnit.Framework.Assert.That(configuration.LockNumberOfEntryLinesForRegisteredEntry, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestIsUCC5()
		{
			NUnit.Framework.Assert.That(configuration.IsUCC5(Factory.New<DummyBusinessObject>()), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestIsUCC6()
		{
			NUnit.Framework.Assert.That(configuration.IsUCC6(Factory.New<DummyBusinessObject>()), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestIsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice()
		{
			NUnit.Framework.Assert.That(configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUseEoriForFiscalReference()
		{
			NUnit.Framework.Assert.That(configuration.UseEoriForFiscalReference, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUseEucdmSupportingDocumentGoodsShipment()
		{
			NUnit.Framework.Assert.That(configuration.UseEucdmSupportingDocumentGoodsShipment, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUseEucdmSupportingDocumentGoodsShipmentAndItem()
		{
			NUnit.Framework.Assert.That(configuration.UseEucdmSupportingDocumentGoodsShipmentAndItem, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsTransitionPeriodAES30()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				NUnit.Framework.Assert.That(configuration.IsTransitionPeriodAES30(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				CombineAssertions("When AES Transition Period is valid", () =>
				{
					declaration.JE_MessageType = "EXP";
					NUnit.Framework.Assert.That(configuration.IsTransitionPeriodAES30(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "When declaration is EXP ");

					declaration.JE_MessageType = "IMP";
					NUnit.Framework.Assert.That(configuration.IsTransitionPeriodAES30(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "When declaration is not EXP");
				});
			}
		}

		[ExpectNoExceptions]
		public void TestUseIDDDocument()
		{
			NUnit.Framework.Assert.That(configuration.UseIDDDocument(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetValidationDecider()
		{
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(declaration), NUnit.Framework.Is.TypeOf<UCC6ExportDeclarationValidationDecider>(), "UCC6 EXP");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(declaration), NUnit.Framework.Is.TypeOf<UCC6ImportDeclarationValidationDecider>(), "UCC6 IMP");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(declaration), NUnit.Framework.Is.EqualTo(default(IDeclarationValidationDecider)), "Not UCC6 EXP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(declaration), NUnit.Framework.Is.EqualTo(default(IDeclarationValidationDecider)), "Not UCC6 IMP - should be [null]");
				}
			});
		}

		[ExpectNoExceptions]
		public void TestGetPackageValidationDecider()
		{
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetPackageValidationDecider(declaration), NUnit.Framework.Is.EqualTo(default(IPackageValidationDecider)), "UCC6 EXP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetPackageValidationDecider(declaration), NUnit.Framework.Is.TypeOf<UCC6ImportPackageValidationDecider>(), "UCC6 IMP");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetPackageValidationDecider(declaration), NUnit.Framework.Is.EqualTo(default(IPackageValidationDecider)), "Not UCC6 EXP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetPackageValidationDecider(declaration), NUnit.Framework.Is.EqualTo(default(IPackageValidationDecider)), "Not UCC6 IMP - should be [null]");
				}
			});
		}

		[ExpectNoExceptions]
		public void TestInvoiceLinePackageValidationDecider()
		{
			NUnit.Framework.Assert.That(configuration.GetInvoiceLinePackageValidationDecider(), NUnit.Framework.Is.TypeOf<InvoiceLinePackageValidationDecider>());
		}

		[ExpectNoExceptions]
		public void TestGetPreviousDocumentValidationDecider()
		{
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(declaration), NUnit.Framework.Is.EqualTo(default(IPreviousDocumentValidationDecider)), "UCC6 EXP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(declaration), NUnit.Framework.Is.TypeOf<UCC6ImportPreviousDocumentValidationDecider>(), "UCC6 IMP");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(declaration), NUnit.Framework.Is.EqualTo(default(IPreviousDocumentValidationDecider)), "Not UCC6 EXP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(declaration), NUnit.Framework.Is.EqualTo(default(IPreviousDocumentValidationDecider)), "Not UCC6 IMP - should be [null]");
				}
			});
		}

		[ExpectNoExceptions]
		public override void TestShouldCheckLegalByDeclarantType()
		{
			NUnit.Framework.Assert.That(configuration.ShouldCheckLegalByDeclarantType, NUnit.Framework.Is.EqualTo(false));
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			base.SetUp();
		}

		JobDeclaration declaration;
	}
}
