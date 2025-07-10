using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.MasterFiles;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestsSubclassesOf(typeof(InvoiceLineConfiguration))]
	public abstract class InvoiceLineConfigurationAbstractTest<L> : TestCaseWithFactory
		where L : InvoiceLineConfiguration
	{
		public abstract void TestInflateItemPriceByValuationMarkup();

		public abstract void TestCountryOfSupplyMustBeTheSameForAllLinesOnInstruction();

		public abstract void TestDefaultCountryOfSupplyFromSupplier();

		public abstract void TestMethodOfPaymentVisibleOnImportControl();

		public abstract void TestAdditionalInfosSupport();

		public abstract void TestSupportingDocumentsSupport();

		public abstract void TestPreviousDocumentsSupport();

		public abstract void TestTaxSupport();

		public abstract void TestCountryOfDestinationVisibleOnImportControl();

		public abstract void TestCountryOfDestinationVisibleOnExportControl();

		public abstract void TestFiscalReferencesSupport();

		public abstract void TestVehicleSupport();

		public abstract void TestAuthorisationsForInvoiceLineSupport();

		public abstract void TestOrganizationsSupport();

		public abstract void TestInvoiceLinePaymentSupport();

		public abstract void TestValueIndicatorsSupport();

		public abstract void TestAdditionalSupplyChainActorSupport();

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (L)Activator.CreateInstance(typeof(L));
		}
		protected L configuration;
	}

	[TestedType(typeof(InvoiceLineConfiguration))]
	sealed class InvoiceLineConfigurationBaseTest : InvoiceLineConfigurationAbstractTest<InvoiceLineConfiguration>
	{
		[ExpectNoExceptions]
		public override void TestInflateItemPriceByValuationMarkup()
		{
			NUnit.Framework.Assert.That(configuration.InflateItemPriceByValuationMarkup(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestCountryOfSupplyMustBeTheSameForAllLinesOnInstruction()
		{
			NUnit.Framework.Assert.That(configuration.CountryOfSupplyMustBeTheSameForAllLinesOnInstruction(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestDefaultCountryOfSupplyFromSupplier()
		{
			NUnit.Framework.Assert.That(configuration.DefaultCountryOfSupplyFromSupplier(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestMethodOfPaymentVisibleOnImportControl()
		{
			NUnit.Framework.Assert.That(configuration.MethodOfPaymentVisibleOnImportControl(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
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
			NUnit.Framework.Assert.That(configuration.TaxSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestCountryOfDestinationVisibleOnImportControl()
		{
			NUnit.Framework.Assert.That(configuration.CountryOfDestinationVisibleOnImportControl(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestCountryOfDestinationVisibleOnExportControl()
		{
			NUnit.Framework.Assert.That(configuration.CountryOfDestinationVisibleOnExportControl(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestFiscalReferencesSupport()
		{
			NUnit.Framework.Assert.That(configuration.FiscalReferencesSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestVehicleSupport()
		{
			NUnit.Framework.Assert.That(configuration.VehicleSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAuthorisationsForInvoiceLineSupport()
		{
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					NUnit.Framework.Assert.That(configuration.AuthorisationsSupportForInvoiceLine(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "UCC6");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					NUnit.Framework.Assert.That(configuration.AuthorisationsSupportForInvoiceLine(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Not UCC6");
				}
			});
		}

		[ExpectNoExceptions]
		public override void TestOrganizationsSupport()
		{
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.OrganizationsSupport(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IsUCC6 ON EXP");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.OrganizationsSupport(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IsUCC6 ON IMP");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.OrganizationsSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IsUCC6 OFF EXP");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.OrganizationsSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IsUCC6 OFF IMP");
				}
			});
		}

		[ExpectNoExceptions]
		public override void TestInvoiceLinePaymentSupport()
		{
			NUnit.Framework.Assert.That(configuration.InvoiceLinePaymentSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestValueIndicatorsSupport()
		{
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.ValueIndicatorsSupport(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "UCC6 Import");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.ValueIndicatorsSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "UCC6 Export");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.FiscalReferencesSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Import");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.FiscalReferencesSupport(declaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Export");
				}
			});
		}

		[ExpectNoExceptions]
		public override void TestAdditionalSupplyChainActorSupport()
		{
			NUnit.Framework.Assert.That(configuration.AdditionalSupplyChainActorSupport(declaration), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetValidationDecider()
		{
			CombineAssertions(() =>
			{
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(invoiceLine), NUnit.Framework.Is.TypeOf<UCC6ExportInvoiceLineValidationDecider>(), "IsUCC6 ON EXP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(invoiceLine), NUnit.Framework.Is.TypeOf<UCC6ImportInvoiceLineValidationDecider>(), "IsUCC6 ON IMP");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(invoiceLine), NUnit.Framework.Is.EqualTo(default(IInvoiceLineValidationDecider)), "IsUCC6 OFF EXP - should be [null]");
					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetValidationDecider(invoiceLine), NUnit.Framework.Is.EqualTo(default(IInvoiceLineValidationDecider)), "IsUCC6 OFF IMP - should be [null]");
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;
	}

	[TestedType(typeof(InvoiceLineConfiguration))]
	sealed class InvoiceLineConfigurationBaseForCusClassPartPivotTest : InvoiceLineConfigurationAbstractTest<InvoiceLineConfiguration>
	{
		[ExpectNoExceptions]
		public override void TestInflateItemPriceByValuationMarkup()
		{
			NUnit.Framework.Assert.That(configuration.InflateItemPriceByValuationMarkup(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestCountryOfSupplyMustBeTheSameForAllLinesOnInstruction()
		{
			NUnit.Framework.Assert.That(configuration.CountryOfSupplyMustBeTheSameForAllLinesOnInstruction(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestDefaultCountryOfSupplyFromSupplier()
		{
			NUnit.Framework.Assert.That(configuration.DefaultCountryOfSupplyFromSupplier(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestMethodOfPaymentVisibleOnImportControl()
		{
			NUnit.Framework.Assert.That(configuration.MethodOfPaymentVisibleOnImportControl(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAdditionalInfosSupport()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(configuration.AdditionalInfosSupport(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "BTH");
				partPivot.CI_ChildType = ClassificationType.IMP;
				NUnit.Framework.Assert.That(configuration.AdditionalInfosSupport(partPivot), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IMP");
				partPivot.CI_ChildType = ClassificationType.EXP;
				NUnit.Framework.Assert.That(configuration.AdditionalInfosSupport(partPivot), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "EXP");
			});
		}

		[ExpectNoExceptions]
		public override void TestSupportingDocumentsSupport()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(configuration.SupportingDocumentsSupport(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "BTH");
				partPivot.CI_ChildType = ClassificationType.IMP;
				NUnit.Framework.Assert.That(configuration.SupportingDocumentsSupport(partPivot), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IMP");
				partPivot.CI_ChildType = ClassificationType.EXP;
				NUnit.Framework.Assert.That(configuration.SupportingDocumentsSupport(partPivot), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "EXP");
			});
		}

		[ExpectNoExceptions]
		public override void TestPreviousDocumentsSupport()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(configuration.PreviousDocumentsSupport(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "BTH");
				partPivot.CI_ChildType = ClassificationType.IMP;
				NUnit.Framework.Assert.That(configuration.PreviousDocumentsSupport(partPivot), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IMP");
				partPivot.CI_ChildType = ClassificationType.EXP;
				NUnit.Framework.Assert.That(configuration.PreviousDocumentsSupport(partPivot), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "EXP");
			});
		}

		[ExpectNoExceptions]
		public override void TestTaxSupport()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(configuration.TaxSupport(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "BTH");
				partPivot.CI_ChildType = ClassificationType.IMP;
				NUnit.Framework.Assert.That(configuration.TaxSupport(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IMP");
				partPivot.CI_ChildType = ClassificationType.EXP;
				NUnit.Framework.Assert.That(configuration.TaxSupport(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "EXP");
			});
		}

		[ExpectNoExceptions]
		public override void TestCountryOfDestinationVisibleOnImportControl()
		{
			NUnit.Framework.Assert.That(configuration.CountryOfDestinationVisibleOnImportControl(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestCountryOfDestinationVisibleOnExportControl()
		{
			NUnit.Framework.Assert.That(configuration.CountryOfDestinationVisibleOnExportControl(partPivot), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestFiscalReferencesSupport()
		{
			NUnit.Framework.Assert.That(configuration.FiscalReferencesSupport(jobDeclaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestVehicleSupport()
		{
			NUnit.Framework.Assert.That(configuration.VehicleSupport(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAuthorisationsForInvoiceLineSupport()
		{
			NUnit.Framework.Assert.That(configuration.AuthorisationsSupportForInvoiceLine(jobDeclaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "No support for this in InvoiceLineConfigurationBaseForCusClassPartPivot");
		}

		[ExpectNoExceptions]
		public override void TestOrganizationsSupport()
		{
			NUnit.Framework.Assert.That(configuration.OrganizationsSupport(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestInvoiceLinePaymentSupport()
		{
			NUnit.Framework.Assert.That(configuration.CountryOfSupplyMustBeTheSameForAllLinesOnInstruction(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestValueIndicatorsSupport()
		{
			NUnit.Framework.Assert.That(configuration.ValueIndicatorsSupport(jobDeclaration), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAdditionalSupplyChainActorSupport()
		{
			NUnit.Framework.Assert.That(configuration.AdditionalSupplyChainActorSupport(partPivot), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSupportingDocumentsValidationDecider()
		{
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, true))
			{
				jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetSupportingDocumentValidationDecider(invoiceLine), NUnit.Framework.Is.TypeOf<UCC6ImportSupportingDocumentValidationDecider>(), "IsUCC6 IMP");
				jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.GetSupportingDocumentValidationDecider(invoiceLine), NUnit.Framework.Is.EqualTo(default(ISupportingDocumentValidationDecider)), "IsUCC6 EXP - should be [null]");
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, false))
			{
				jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetSupportingDocumentValidationDecider(invoiceLine), NUnit.Framework.Is.EqualTo(default(ISupportingDocumentValidationDecider)), "!IsUCC6 - should be [null]");
			}
		}

		[ExpectNoExceptions]
		public void TestGetCusFiscalReferenceValidationDecider()
		{
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, true))
			{
				jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetCusFiscalReferenceValidationDecider(invoiceLine), NUnit.Framework.Is.TypeOf<UCC6ImportCusFiscalReferenceValidationDecider>(), "IsUCC6 IMP");
				jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.GetCusFiscalReferenceValidationDecider(invoiceLine), NUnit.Framework.Is.EqualTo(default(ICusFiscalReferenceValidationDecider)), "IsUCC6 EXP - should be [null]");
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, false))
			{
				jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetCusFiscalReferenceValidationDecider(invoiceLine), NUnit.Framework.Is.EqualTo(default(ICusFiscalReferenceValidationDecider)), "!IsUCC6 - should be [null]");
			}
		}

		[ExpectNoExceptions]
		public void TestGetAdditionalInfoValidationDecider()
		{
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			NUnit.Framework.Assert.That(invoiceHeader, NUnit.Framework.Is.Not.EqualTo(default(JobComInvoiceHeader)), "[PRE-CONDITION] invoiceHeader should not be null - should not be [null]");
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			NUnit.Framework.Assert.That(invoiceLine, NUnit.Framework.Is.Not.EqualTo(default(JobComInvoiceLine)), "[PRE-CONDITION] invoiceLine should not be null - should not be [null]");
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, configurationValue: true))
				{
					jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetAdditionalInfoValidationDecider(invoiceLine), NUnit.Framework.Is.TypeOf<UCC6ImportAdditionalInfoValidationDecider>(), "IsUCC6 IMP ");
					jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetAdditionalInfoValidationDecider(invoiceLine), NUnit.Framework.Is.EqualTo(default(IAdditionalInfoValidationDecider)), "IsUCC6 EXP - should be [null]");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, configurationValue: false))
				{
					jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetAdditionalInfoValidationDecider(invoiceLine), NUnit.Framework.Is.EqualTo(default(IAdditionalInfoValidationDecider)), "!IsUCC6 IMP - should be [null]");
					jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetAdditionalInfoValidationDecider(invoiceLine), NUnit.Framework.Is.EqualTo(default(IAdditionalInfoValidationDecider)), "!IsUCC6 EXP - should be [null]");
				}
			});
		}

		[ExpectNoExceptions]
		public void TestGetCusAuthorizationUsageValidationDecider()
		{
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, true))
			{
				jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetCusAuthorizationUsageValidationDecider(invoiceLine), NUnit.Framework.Is.TypeOf<UCC6ImportCusAuthorizationUsageValidationDecider>(), "IsUCC6 IMP");
				jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				NUnit.Framework.Assert.That(configuration.GetCusAuthorizationUsageValidationDecider(invoiceLine), NUnit.Framework.Is.EqualTo(default(ICusAuthorizationUsageValidationDecider)), "IsUCC6 EXP - should be [null]");
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, false))
			{
				jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				NUnit.Framework.Assert.That(configuration.GetCusAuthorizationUsageValidationDecider(invoiceLine), NUnit.Framework.Is.EqualTo(default(ICusAuthorizationUsageValidationDecider)), "!IsUCC6 - should be [null]");
			}
		}

		[ExpectNoExceptions]
		public void TestGetPreviousDocumentValidationDecider()
		{
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, true))
				{
					jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(invoiceLine), NUnit.Framework.Is.EqualTo(default(IPreviousDocumentValidationDecider)), "UCC6 EXP - should be [null]");
					jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(invoiceLine), NUnit.Framework.Is.TypeOf<UCC6ImportPreviousDocumentValidationDecider>(), "UCC6 IMP");
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, false))
				{
					jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(invoiceLine), NUnit.Framework.Is.EqualTo(default(IPreviousDocumentValidationDecider)), "Not UCC6 EXP - should be [null]");
					jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					NUnit.Framework.Assert.That(configuration.GetPreviousDocumentValidationDecider(invoiceLine), NUnit.Framework.Is.EqualTo(default(IPreviousDocumentValidationDecider)), "Not UCC6 IMP - should be [null]");
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			partPivot = Factory.New<CusClassPartPivot>();
			jobDeclaration = Factory.New<JobDeclaration>();
		}

		CusClassPartPivot partPivot;
		JobDeclaration jobDeclaration;
	}
}
