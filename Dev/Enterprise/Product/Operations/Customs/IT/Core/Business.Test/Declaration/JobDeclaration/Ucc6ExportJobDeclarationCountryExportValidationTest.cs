using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportJobDeclarationCountryExportValidationTest : TestCaseWithFactory
{
	public void TestJE_GoodsOrigin_CountryDestinationWillBeIgnored_WhenInvoiceLinesHaveSameValue()
	{
		var goodsOriginInfo = declaration.JE_GoodsOriginInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When EXP UCC6", () =>
			{
				invoiceLine1.JI_RN_NKCountryOfExport = "ES";
				invoiceLine2.JI_RN_NKCountryOfExport = "ES";
				declaration.JE_GoodsOrigin = "FR";
				AssertHasWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);

				invoiceLine2.JI_RN_NKCountryOfExport = "FR";
				declaration.Validation.ValidateJE_GoodsOrigin();
				AssertNoWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);

				invoiceLine2.JI_RN_NKCountryOfExport = "ES";
				declaration.JE_GoodsOrigin = "";
				AssertNoWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);

				invoiceLine2.JI_RN_NKCountryOfExport = "";
				declaration.JE_GoodsOrigin = "FR";
				AssertNoWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);

				invoiceLine2.JI_RN_NKCountryOfExport = "ES";
				declaration.JE_GoodsOrigin = "ES";
				AssertNoWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);
			});
		}
	}

	public void TestJE_GoodsOrigin_CountryDestinationWillBeIgnored_WhenInvoiceLineHaveDifferentValues()
	{
		var goodsOriginInfo = declaration.JE_GoodsOriginInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When EXP UCC6", () =>
			{
				invoiceLine1.JI_RN_NKCountryOfExport = "ES";
				invoiceLine2.JI_RN_NKCountryOfExport = "BE";
				declaration.JE_GoodsOrigin = "FR";
				AssertHasWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);

				invoiceLine2.JI_RN_NKCountryOfExport = "ES";
				declaration.Validation.ValidateJE_GoodsOrigin();
				AssertNoWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);

				invoiceLine2.JI_RN_NKCountryOfExport = "BE";
				declaration.JE_GoodsOrigin = "";
				AssertNoWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);

				invoiceLine2.JI_RN_NKCountryOfExport = "";
				declaration.JE_GoodsOrigin = "FR";
				AssertNoWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);

				invoiceLine2.JI_RN_NKCountryOfExport = "BE";
				declaration.JE_GoodsOrigin = "ES";
				AssertNoWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);
			});
		}
	}

	public void TestJE_GoodsOrigin_MustBeDeclareAtHeaderOrLineLevel()
	{
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				var goodsOriginInfo = declaration.JE_GoodsOriginInfo;

				invoiceLine1.JI_RN_NKCountryOfExport = "";
				invoiceLine2.JI_RN_NKCountryOfExport = "";
				declaration.JE_GoodsOrigin = "";
				declaration.Validation.ValidateJE_GoodsOrigin();
				AssertHasMessageErrorContaining(goodsOriginInfo, ExpectedMessageErrorCountryOfExportMustBeDeclaredAtHeaderOrLineLevel);

				declaration.JE_GoodsOrigin = "ES";
				AssertNoMessageErrorContaining(goodsOriginInfo, ExpectedMessageErrorCountryOfExportMustBeDeclaredAtHeaderOrLineLevel);

				invoiceLine2.JI_RN_NKCountryOfExport = "DE";
				declaration.JE_GoodsOrigin = "";
				AssertNoMessageErrorContaining(goodsOriginInfo, ExpectedMessageErrorCountryOfExportMustBeDeclaredAtHeaderOrLineLevel);
			});
		}
	}

	public void TestJE_GoodsOrigin_WhenDeclarationHasNoInvoiceLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("When Declaration has no Invoice Lines", () =>
			{
				var goodsOriginInfo = declaration.JE_GoodsOriginInfo;

				declaration.JE_GoodsOrigin = "DE";
				AssertNoWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);
				AssertNoWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);
				AssertNoMessageErrorContaining(goodsOriginInfo, ExpectedMessageErrorCountryOfExportMustBeDeclaredAtHeaderOrLineLevel);

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				var invoiceLine2 = invoice.InvoiceLines.AddNew();

				invoiceLine1.JI_RN_NKCountryOfExport = "ES";
				invoiceLine2.JI_RN_NKCountryOfExport = "ES";
				declaration.Validation.ValidateJE_GoodsOrigin();
				AssertHasWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);

				invoiceLine2.JI_RN_NKCountryOfExport = "FR";
				declaration.Validation.ValidateJE_GoodsOrigin();
				AssertHasWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);

				invoiceLine1.JI_RN_NKCountryOfExport = "";
				invoiceLine2.JI_RN_NKCountryOfExport = "";
				declaration.JE_GoodsOrigin = "";
				AssertHasMessageErrorContaining(goodsOriginInfo, ExpectedMessageErrorCountryOfExportMustBeDeclaredAtHeaderOrLineLevel);
			});
		}
	}

	public void TestJE_GoodsOrigin_CountryDestinationWillBeIgnored_WhenDeclarationIsNotUCC6()
	{
		var goodsOriginInfo = declaration.JE_GoodsOriginInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When Declaration is UCC6", () =>
			{
				invoiceLine1.JI_RN_NKCountryOfExport = "ES";
				invoiceLine2.JI_RN_NKCountryOfExport = "ES";
				declaration.JE_GoodsOrigin = "FR";
				AssertHasWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			CombineAssertions("When Declaration is not UCC6", () =>
			{
				declaration.Validation.ValidateJE_GoodsOrigin();
				AssertNoMessageErrorContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When Declaration is UCC6", () =>
			{
				invoiceLine1.JI_RN_NKCountryOfExport = "ES";
				invoiceLine2.JI_RN_NKCountryOfExport = "BE";
				declaration.JE_GoodsOrigin = "FR";
				declaration.Validation.ValidateJE_GoodsOrigin();
				AssertHasWarningContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			CombineAssertions("When Declaration is not UCC6", () =>
			{
				declaration.Validation.ValidateJE_GoodsOrigin();
				AssertNoMessageErrorContaining(goodsOriginInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);
			});
		}
	}

	public void TestJE_GoodsOrigin_MustBeDeclareAtHeaderOrLineLevel_WhenDeclarationIsNotUCC6()
	{
		var goodsOriginInfo = declaration.JE_GoodsOriginInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When Declaration is UCC6", () =>
			{
				invoiceLine1.JI_RN_NKCountryOfExport = "";
				invoiceLine2.JI_RN_NKCountryOfExport = "";
				declaration.JE_GoodsOrigin = "";
				declaration.Validation.ValidateJE_GoodsOrigin();
				AssertHasMessageErrorContaining(goodsOriginInfo, ExpectedMessageErrorCountryOfExportMustBeDeclaredAtHeaderOrLineLevel);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			CombineAssertions("When Declaration is not UCC6", () =>
			{
				declaration.Validation.ValidateJE_GoodsOrigin();
				AssertNoMessageErrorContaining(goodsOriginInfo, ExpectedMessageErrorCountryOfExportMustBeDeclaredAtHeaderOrLineLevel);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		var invoice = declaration.Invoices.AddNew();
		invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine2 = invoice.InvoiceLines.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine1;
	JobComInvoiceLine invoiceLine2;

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
	{
		return ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);
	}

	const string ExpectedWarningMessageInvoiceLinesHaveSameValue = "The Country of Export declared in the header will be ignored because all the lines have the same value, which differs from the value declared in the header.";
	const string ExpectedWarningMessageInvoiceLinesHaveDifferentValues = "The Country of Export declared in the header will be ignored because all lines have values, which different from the header one.";
	const string ExpectedMessageErrorCountryOfExportMustBeDeclaredAtHeaderOrLineLevel = "Country of Export must be declared at header or line level.";
}
