using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderConfiguration))]
	class InvoiceHeaderConfigurationTest : InvoiceHeaderConfigurationAbstractTest<InvoiceHeaderConfiguration>
	{
		[ExpectNoExceptions]
		public void TestGetSupportingDocumentValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				NUnit.Framework.Assert.That(configuration.GetSupportingDocumentValidationDecider(invoiceHeader), Is.TypeOf<UCC6ImportSupportingDocumentValidationDecider>(), "IsUCC6 IMP");
			}
		}

		[ExpectNoExceptions]
		public override void TestAdditionalInfosSupport()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(configuration.AdditionalInfosSupport(Factory.New<DummyBusinessObject>()), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Disabled when business object is not IImportExport");
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.AdditionalInfosSupport(declaration), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Enabled for export");
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.AdditionalInfosSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Disabled for import");
			});
		}

		[ExpectNoExceptions]
		public override void TestSupportingDocumentsSupport()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(
					configuration.SupportingDocumentsSupport(Factory.New<DummyBusinessObject>()),
					Is.EqualTo(true).Using(CustomComparers.TypeComparison),
					"Enabled when business object is not IImportExport"
				);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

				NUnit.Framework.Assert.That(
					configuration.SupportingDocumentsSupport(declaration),
					Is.EqualTo(true).Using(CustomComparers.TypeComparison),
					"Enabled for export"
				);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

				NUnit.Framework.Assert.That(
					configuration.SupportingDocumentsSupport(declaration),
					Is.EqualTo(true).Using(CustomComparers.TypeComparison),
					"Enabled for import"
				);
			});
		}

		[ExpectNoExceptions]
		public override void TestPreviousDocumentsSupport()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.PreviousDocumentsSupport(declaration), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Enabled for export");
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.PreviousDocumentsSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Disabled for import");
			});
		}

		[ExpectNoExceptions]
		public override void TestTaxSupport()
		{
			NUnit.Framework.Assert.That(configuration.TaxSupport(declaration), Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestValueIndicatorsSupport()
		{
			NUnit.Framework.Assert.That(configuration.ValueIndicatorsSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAgreedPlaceCodeSupport()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(configuration.AgreedPlaceCodeSupport(Factory.New<DummyBusinessObject>()), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Disabled when business object is not IImportExport");
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.AgreedPlaceCodeSupport(declaration), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Enabled for export");
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.AgreedPlaceCodeSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Disabled for import");
			});
		}

		[ExpectNoExceptions]
		public override void TestInvoicePaymentSupport()
		{
			NUnit.Framework.Assert.That(configuration.InvoicePaymentSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestExportCostCalculationsTotalsUISupport()
		{
			NUnit.Framework.Assert.That(configuration.ExportCostCalculationsTotalsUISupport, Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
