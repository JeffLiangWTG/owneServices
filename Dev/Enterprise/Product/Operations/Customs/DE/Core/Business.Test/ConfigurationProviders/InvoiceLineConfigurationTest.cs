using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(InvoiceLineConfiguration))]
	class InvoiceLineConfigurationTest : InvoiceLineConfigurationAbstractTest<InvoiceLineConfiguration>
	{
		[ExpectNoExceptions]
		public void TestGetSupportingDocumentValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var invocieLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				NUnit.Framework.Assert.That(configuration.GetSupportingDocumentValidationDecider(invocieLine), Is.TypeOf<UCC6ImportSupportingDocumentValidationDecider>(), "IsUCC6 IMP");
			}
		}

		[ExpectNoExceptions]
		public override void TestInflateItemPriceByValuationMarkup()
		{
			NUnit.Framework.Assert.That(configuration.InflateItemPriceByValuationMarkup(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestCountryOfSupplyMustBeTheSameForAllLinesOnInstruction()
		{
			NUnit.Framework.Assert.That(configuration.CountryOfSupplyMustBeTheSameForAllLinesOnInstruction(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestDefaultCountryOfSupplyFromSupplier()
		{
			NUnit.Framework.Assert.That(configuration.DefaultCountryOfSupplyFromSupplier(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestMethodOfPaymentVisibleOnImportControl()
		{
			NUnit.Framework.Assert.That(configuration.MethodOfPaymentVisibleOnImportControl(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAdditionalInfosSupport()
		{
			NUnit.Framework.Assert.That(configuration.AdditionalInfosSupport(declaration), Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestSupportingDocumentsSupport()
		{
			NUnit.Framework.Assert.That(configuration.SupportingDocumentsSupport(declaration), Is.EqualTo(true).Using(CustomComparers.TypeComparison));
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
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(configuration.TaxSupport(Factory.New<DummyBusinessObject>()), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Enabled when business object is not IImportExport");
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.TaxSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Disabled for export");
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.TaxSupport(declaration), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Enabled for import");
			});
		}

		[ExpectNoExceptions]
		public override void TestCountryOfDestinationVisibleOnImportControl()
		{
			NUnit.Framework.Assert.That(configuration.CountryOfDestinationVisibleOnImportControl(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestCountryOfDestinationVisibleOnExportControl()
		{
			NUnit.Framework.Assert.That(configuration.CountryOfDestinationVisibleOnExportControl(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestFiscalReferencesSupport()
		{
			NUnit.Framework.Assert.That(configuration.FiscalReferencesSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestVehicleSupport()
		{
			NUnit.Framework.Assert.That(configuration.VehicleSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestOrganizationsSupport()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.OrganizationsSupport(declaration), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Enabled for export");
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.OrganizationsSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Disabled for import");
			});
		}

		[ExpectNoExceptions]
		public override void TestValueIndicatorsSupport()
		{
			NUnit.Framework.Assert.That(configuration.ValueIndicatorsSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		[ExpectNoExceptions]
		public override void TestAuthorisationsForInvoiceLineSupport()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					NUnit.Framework.Assert.That(configuration.AuthorisationsSupportForInvoiceLine(declaration), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "UCC6");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					NUnit.Framework.Assert.That(configuration.AuthorisationsSupportForInvoiceLine(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Not UCC6");
				}
			});
		}

		[ExpectNoExceptions]
		public override void TestInvoiceLinePaymentSupport()
		{
			NUnit.Framework.Assert.That(configuration.InvoiceLinePaymentSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAdditionalSupplyChainActorSupport()
		{
			NUnit.Framework.Assert.That(configuration.AdditionalSupplyChainActorSupport(declaration), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		JobDeclaration declaration;
	}

	[TestedType(typeof(InvoiceLineConfiguration))]
	class InvoiceLineConfigurationForCusClassPartPivotTest : InvoiceLineConfigurationAbstractTest<InvoiceLineConfiguration>
	{
		[ExpectNoExceptions]
		public override void TestInflateItemPriceByValuationMarkup()
		{
			NUnit.Framework.Assert.That(configuration.InflateItemPriceByValuationMarkup(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestCountryOfSupplyMustBeTheSameForAllLinesOnInstruction()
		{
			NUnit.Framework.Assert.That(configuration.CountryOfSupplyMustBeTheSameForAllLinesOnInstruction(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestDefaultCountryOfSupplyFromSupplier()
		{
			NUnit.Framework.Assert.That(configuration.DefaultCountryOfSupplyFromSupplier(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestMethodOfPaymentVisibleOnImportControl()
		{
			NUnit.Framework.Assert.That(configuration.MethodOfPaymentVisibleOnImportControl(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAdditionalInfosSupport()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(configuration.AdditionalInfosSupport(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "BTH");
				partPivot.CI_ChildType = ClassificationType.IMP;
				NUnit.Framework.Assert.That(configuration.AdditionalInfosSupport(partPivot), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IMP");
				partPivot.CI_ChildType = ClassificationType.EXP;
				NUnit.Framework.Assert.That(configuration.AdditionalInfosSupport(partPivot), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "EXP");
			});
		}

		[ExpectNoExceptions]
		public override void TestSupportingDocumentsSupport()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(configuration.SupportingDocumentsSupport(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "BTH");
				partPivot.CI_ChildType = ClassificationType.IMP;
				NUnit.Framework.Assert.That(configuration.SupportingDocumentsSupport(partPivot), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IMP");
				partPivot.CI_ChildType = ClassificationType.EXP;
				NUnit.Framework.Assert.That(configuration.SupportingDocumentsSupport(partPivot), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "EXP");
			});
		}

		[ExpectNoExceptions]
		public override void TestPreviousDocumentsSupport()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(configuration.PreviousDocumentsSupport(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "BTH");
				partPivot.CI_ChildType = ClassificationType.IMP;
				NUnit.Framework.Assert.That(configuration.PreviousDocumentsSupport(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IMP");
				partPivot.CI_ChildType = ClassificationType.EXP;
				NUnit.Framework.Assert.That(configuration.PreviousDocumentsSupport(partPivot), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "EXP");
			});
		}

		[ExpectNoExceptions]
		public override void TestTaxSupport()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(configuration.TaxSupport(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "BTH");
				partPivot.CI_ChildType = ClassificationType.IMP;
				NUnit.Framework.Assert.That(configuration.TaxSupport(partPivot), Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IMP");
				partPivot.CI_ChildType = ClassificationType.EXP;
				NUnit.Framework.Assert.That(configuration.TaxSupport(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "EXP");
			});
		}

		[ExpectNoExceptions]
		public override void TestCountryOfDestinationVisibleOnImportControl()
		{
			NUnit.Framework.Assert.That(configuration.CountryOfDestinationVisibleOnImportControl(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestCountryOfDestinationVisibleOnExportControl()
		{
			NUnit.Framework.Assert.That(configuration.CountryOfDestinationVisibleOnExportControl(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		public override void TestFiscalReferencesSupport()
		{
			Assert("FiscalReferences doesn't apply to CusClassPartPivot", true);
		}

		[ExpectNoExceptions]
		public override void TestVehicleSupport()
		{
			NUnit.Framework.Assert.That(configuration.VehicleSupport(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		public override void TestAuthorisationsForInvoiceLineSupport()
		{
			Assert("AuthorisationsForInvoiceLine doesn't apply to CusClassPartPivot", true);
		}

		[ExpectNoExceptions]
		public override void TestOrganizationsSupport()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(configuration.OrganizationsSupport(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "BTH");
				partPivot.CI_ChildType = ClassificationType.IMP;
				NUnit.Framework.Assert.That(configuration.OrganizationsSupport(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IMP");
				partPivot.CI_ChildType = ClassificationType.EXP;
				NUnit.Framework.Assert.That(configuration.OrganizationsSupport(partPivot), Is.EqualTo(false).Using(CustomComparers.TypeComparison), "EXP");
			});
		}

		public override void TestInvoiceLinePaymentSupport()
		{
			Assert("InvoiceLinePayment doesn't apply to CusClassPartPivot", true);
		}

		public override void TestValueIndicatorsSupport()
		{
			Assert("ValueIndicators doesn't apply to CusClassPartPivot", true);
		}

		[ExpectNoExceptions]
		public override void TestAdditionalSupplyChainActorSupport()
		{
			NUnit.Framework.Assert.That(configuration.AdditionalSupplyChainActorSupport(partPivot),	Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			partPivot = Factory.New<CusClassPartPivot>();
		}

		CusClassPartPivot partPivot;
	}
}
