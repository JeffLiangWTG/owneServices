using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportJobDeclarationCountryDestinationValidationTest : TestCaseWithFactory
{
	public void TestJE_GoodsDestination_CountryDestinationWillBeIgnored_WhenInvoiceLinesHaveSameValue()
	{
		var goodsDestinationInfo = declaration.JE_GoodsDestinationInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When EXP UCC6", () =>
			{
				invoiceLine1.ZG_CountryOfDestination = "ES";
				invoiceLine2.ZG_CountryOfDestination = "ES";
				declaration.JE_GoodsDestination = "FR";
				AssertHasWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);

				invoiceLine2.ZG_CountryOfDestination = "FR";
				declaration.Validation.ValidateJE_GoodsDestination();
				AssertNoWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);

				invoiceLine2.ZG_CountryOfDestination = "ES";
				declaration.JE_GoodsDestination = "";
				AssertNoWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);

				invoiceLine2.ZG_CountryOfDestination = "";
				declaration.JE_GoodsDestination = "FR";
				AssertNoWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);

				invoiceLine2.ZG_CountryOfDestination = "ES";
				declaration.JE_GoodsDestination = "ES";
				AssertNoWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);
			});
		}
	}

	public void TestJE_GoodsDestination_CountryDestinationWillBeIgnored_WhenInvoiceLineHaveDifferentValues()
	{
		var goodsDestinationInfo = declaration.JE_GoodsDestinationInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When EXP UCC6", () =>
			{
				invoiceLine1.ZG_CountryOfDestination = "ES";
				invoiceLine2.ZG_CountryOfDestination = "BE";
				declaration.JE_GoodsDestination = "FR";
				AssertHasWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);

				invoiceLine2.ZG_CountryOfDestination = "ES";
				declaration.Validation.ValidateJE_GoodsDestination();
				AssertNoWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);

				invoiceLine2.ZG_CountryOfDestination = "BE";
				declaration.JE_GoodsDestination = "";
				AssertNoWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);

				invoiceLine2.ZG_CountryOfDestination = "";
				declaration.JE_GoodsDestination = "FR";
				AssertNoWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);

				invoiceLine2.ZG_CountryOfDestination = "BE";
				declaration.JE_GoodsDestination = "ES";
				AssertNoWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);
			});
		}
	}

	public void TestJE_GoodsDestination_MustBeDeclareAtHeaderOrLineLevel()
	{
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				var goodsDestinationInfo = declaration.JE_GoodsDestinationInfo;

				invoiceLine1.ZG_CountryOfDestination = "";
				invoiceLine2.ZG_CountryOfDestination = "";
				declaration.JE_GoodsDestination = "";
				declaration.Validation.ValidateJE_GoodsDestination();
				AssertHasMessageErrorContaining(goodsDestinationInfo, ExpectedMessageErrorCountryOfDestinationMustBeDeclaredAtHeaderOrLineLevel);

				declaration.JE_GoodsDestination = "ES";
				AssertNoMessageErrorContaining(goodsDestinationInfo, ExpectedMessageErrorCountryOfDestinationMustBeDeclaredAtHeaderOrLineLevel);

				invoiceLine2.ZG_CountryOfDestination = "DE";
				declaration.JE_GoodsDestination = "";
				AssertNoMessageErrorContaining(goodsDestinationInfo, ExpectedMessageErrorCountryOfDestinationMustBeDeclaredAtHeaderOrLineLevel);
			});
		}
	}

	public void TestJE_GoodsDestination_WhenDeclarationHasNoInvoiceLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("When Declaration has no Invoice Lines", () =>
			{
				var goodsDestinationInfo = declaration.JE_GoodsDestinationInfo;

				declaration.JE_GoodsDestination = "DE";
				AssertNoWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);
				AssertNoWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);
				AssertNoMessageErrorContaining(goodsDestinationInfo, ExpectedMessageErrorCountryOfDestinationMustBeDeclaredAtHeaderOrLineLevel);

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				var invoiceLine2 = invoice.InvoiceLines.AddNew();

				invoiceLine1.ZG_CountryOfDestination = "ES";
				invoiceLine2.ZG_CountryOfDestination = "ES";
				declaration.Validation.ValidateJE_GoodsDestination();
				AssertHasWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);

				invoiceLine2.ZG_CountryOfDestination = "FR";
				declaration.Validation.ValidateJE_GoodsDestination();
				AssertHasWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);

				invoiceLine1.ZG_CountryOfDestination = "";
				invoiceLine2.ZG_CountryOfDestination = "";
				declaration.JE_GoodsDestination = "";
				AssertHasMessageErrorContaining(goodsDestinationInfo, ExpectedMessageErrorCountryOfDestinationMustBeDeclaredAtHeaderOrLineLevel);
			});
		}
	}

	public void TestJE_GoodsDestination_CountryDestinationWillBeIgnored_WhenDeclarationIsNotUCC6()
	{
		var goodsDestinationInfo = declaration.JE_GoodsDestinationInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When Declaration is UCC6", () =>
			{
				invoiceLine1.ZG_CountryOfDestination = "ES";
				invoiceLine2.ZG_CountryOfDestination = "ES";
				declaration.JE_GoodsDestination = "FR";
				AssertHasWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			CombineAssertions("When Declaration is not UCC6", () =>
			{
				declaration.Validation.ValidateJE_GoodsDestination();
				AssertNoMessageErrorContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveSameValue);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When Declaration is UCC6", () =>
			{
				invoiceLine1.ZG_CountryOfDestination = "ES";
				invoiceLine2.ZG_CountryOfDestination = "BE";
				declaration.JE_GoodsDestination = "FR";
				declaration.Validation.ValidateJE_GoodsDestination();
				AssertHasWarningContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			CombineAssertions("When Declaration is not UCC6", () =>
			{
				declaration.Validation.ValidateJE_GoodsDestination();
				AssertNoMessageErrorContaining(goodsDestinationInfo, ExpectedWarningMessageInvoiceLinesHaveDifferentValues);
			});
		}
	}

	public void TestJE_GoodsDestination_MustBeDeclareAtHeaderOrLineLevel_WhenDeclarationIsNotUCC6()
	{
		var goodsDestinationInfo = declaration.JE_GoodsDestinationInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When Declaration is UCC6", () =>
			{
				invoiceLine1.ZG_CountryOfDestination = "";
				invoiceLine2.ZG_CountryOfDestination = "";
				declaration.JE_GoodsDestination = "";
				declaration.Validation.ValidateJE_GoodsDestination();
				AssertHasMessageErrorContaining(goodsDestinationInfo, ExpectedMessageErrorCountryOfDestinationMustBeDeclaredAtHeaderOrLineLevel);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			CombineAssertions("When Declaration is not UCC6", () =>
			{
				declaration.Validation.ValidateJE_GoodsDestination();
				AssertNoMessageErrorContaining(goodsDestinationInfo, ExpectedMessageErrorCountryOfDestinationMustBeDeclaredAtHeaderOrLineLevel);
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

	const string ExpectedWarningMessageInvoiceLinesHaveSameValue = "The Country of Destination declared in the header will be ignored because all the lines have the same value, which differs from the value declared in the header.";
	const string ExpectedWarningMessageInvoiceLinesHaveDifferentValues = "The Country of Destination declared in the header will be ignored because all lines have values, which different from the header one.";
	const string ExpectedMessageErrorCountryOfDestinationMustBeDeclaredAtHeaderOrLineLevel = "Country of Destination must be declared at header or line level.";
}
