using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestsSubclassesOf(typeof(InvoiceHeaderConfiguration))]
	public abstract class InvoiceHeaderConfigurationAbstractTest<H> : TestCaseWithFactory
	where H : InvoiceHeaderConfiguration
	{
		public abstract void TestInvoicePaymentSupport();

		public abstract void TestAdditionalInfosSupport();

		public abstract void TestSupportingDocumentsSupport();

		public abstract void TestPreviousDocumentsSupport();

		public abstract void TestTaxSupport();

		public abstract void TestValueIndicatorsSupport();

		public abstract void TestAgreedPlaceCodeSupport();

		public abstract void TestExportCostCalculationsTotalsUISupport();

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (H)Activator.CreateInstance(typeof(H));
		}

		protected H configuration;
	}

	[TestedType(typeof(InvoiceHeaderConfiguration))]
	sealed class InvoiceHeaderConfigurationBaseTest : InvoiceHeaderConfigurationAbstractTest<InvoiceHeaderConfiguration>
	{
		[ExpectNoExceptions]
		public override void TestInvoicePaymentSupport()
		{
			NUnit.Framework.Assert.That(configuration.InvoicePaymentSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAdditionalInfosSupport()
		{
			NUnit.Framework.Assert.That(configuration.AdditionalInfosSupport(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestSupportingDocumentsSupport()
		{
			NUnit.Framework.Assert.That(configuration.SupportingDocumentsSupport(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestPreviousDocumentsSupport()
		{
			NUnit.Framework.Assert.That(configuration.PreviousDocumentsSupport(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestTaxSupport()
		{
			NUnit.Framework.Assert.That(configuration.TaxSupport(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMultipleSupportingDocumentsForAllInvoiceNumbersSupport()
		{
			NUnit.Framework.Assert.That(configuration.MultipleSupportingDocumentsForAllInvoiceNumbersSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestValueIndicatorsSupport()
		{
			CombineAssertions("When declaration is UCC6", () => { AssertValueIndicatorsSupport(isUCC6: true, expectedSupportForImport: true, expectedSupportForExport: false); });
			CombineAssertions("When declaration is not UCC6", () => { AssertValueIndicatorsSupport(isUCC6: false, expectedSupportForImport: false, expectedSupportForExport: false); });
			NUnit.Framework.Assert.That(configuration.ValueIndicatorsSupport(businessObject: Factory.New<DummyBusinessObject>()), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "When BusinessObject is not a JobDeclaration");
		}

		[ExpectNoExceptions]
		public override void TestAgreedPlaceCodeSupport()
		{
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					NUnit.Framework.Assert.That(configuration.AgreedPlaceCodeSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "UCC6 is disabled");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					NUnit.Framework.Assert.That(configuration.AgreedPlaceCodeSupport(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "UCC6 is enabled");
				}
			});
		}

		[ExpectNoExceptions]
		public override void TestExportCostCalculationsTotalsUISupport()
		{
			NUnit.Framework.Assert.That(configuration.ExportCostCalculationsTotalsUISupport, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetSupportingDocumentValidationDecider()
		{
			var invoiceHeader = declaration.Invoices.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetSupportingDocumentValidationDecider(invoiceHeader), NUnit.Framework.Is.TypeOf<UCC6ImportSupportingDocumentValidationDecider>(), "IsUCC6 IMP");
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.GetSupportingDocumentValidationDecider(invoiceHeader), NUnit.Framework.Is.EqualTo(default(ISupportingDocumentValidationDecider)), "IsUCC6 EXP - should be [null]");
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetSupportingDocumentValidationDecider(invoiceHeader), NUnit.Framework.Is.EqualTo(default(ISupportingDocumentValidationDecider)), "!IsUCC6 - should be [null]");
			}
		}

		[ExpectNoExceptions]
		public void TestGetAdditionalInfoValidationDecider()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			NUnit.Framework.Assert.That(invoiceHeader, NUnit.Framework.Is.Not.EqualTo(default(JobComInvoiceHeader)), "[PRE-CONDITION] invoiceHeader should not be null - should not be [null]");
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetAdditionalInfoValidationDecider(invoiceHeader), NUnit.Framework.Is.TypeOf<UCC6ImportAdditionalInfoValidationDecider>(), "IsUCC6 IMP ");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetAdditionalInfoValidationDecider(invoiceHeader), NUnit.Framework.Is.EqualTo(default(IAdditionalInfoValidationDecider)), "IsUCC6 EXP - should be [null]");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetAdditionalInfoValidationDecider(invoiceHeader), NUnit.Framework.Is.EqualTo(default(IAdditionalInfoValidationDecider)), "!IsUCC6 IMP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetAdditionalInfoValidationDecider(invoiceHeader), NUnit.Framework.Is.EqualTo(default(IAdditionalInfoValidationDecider)), "!IsUCC6 EXP - should be [null]");
				}
			});
		}

		[ExpectNoExceptions]
		public void TestGetValidationDecider()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(invoiceHeader), NUnit.Framework.Is.EqualTo(default(IInvoiceHeaderValidationDecider)), "UCC6 EXP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(invoiceHeader), NUnit.Framework.Is.TypeOf<UCC6ImportInvoiceHeaderValidationDecider>(), "UCC6 IMP");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(invoiceHeader), NUnit.Framework.Is.EqualTo(default(IInvoiceHeaderValidationDecider)), "Not UCC6 EXP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(invoiceHeader), NUnit.Framework.Is.EqualTo(default(IInvoiceHeaderValidationDecider)), "Not UCC6 IMP - should be [null]");
				}
			});
		}

		[ExpectNoExceptions]
		public void TestGetPreviousDocumentValidationDecider()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(invoiceHeader), NUnit.Framework.Is.EqualTo(default(IPreviousDocumentValidationDecider)), "UCC6 EXP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(invoiceHeader), NUnit.Framework.Is.TypeOf<UCC6ImportPreviousDocumentValidationDecider>(), "UCC6 IMP");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(invoiceHeader), NUnit.Framework.Is.EqualTo(default(IPreviousDocumentValidationDecider)), "Not UCC6 EXP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(invoiceHeader), NUnit.Framework.Is.EqualTo(default(IPreviousDocumentValidationDecider)), "Not UCC6 IMP - should be [null]");
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;

		[ExpectNoExceptions]
		void AssertValueIndicatorsSupport(bool isUCC6, bool expectedSupportForImport, bool expectedSupportForExport)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.ValueIndicatorsSupport(declaration), NUnit.Framework.Is.EqualTo(expectedSupportForImport).Using(CustomComparers.TypeComparison), $"For Import, {nameof(configuration.ValueIndicatorsSupport)}");
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.ValueIndicatorsSupport(declaration), NUnit.Framework.Is.EqualTo(expectedSupportForExport).Using(CustomComparers.TypeComparison), $"For Export, {nameof(configuration.ValueIndicatorsSupport)}");
			}
		}
	}
}
