using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobComInvoiceHeaderCleanUpStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When invoiceHeader is null", () => new JobComInvoiceHeaderCleanUpStrategy(invoiceHeader: null));
		AssertExceptionThrown<ArgumentNullException>("When invoiceHeader.Declaration is null", () => new JobComInvoiceHeaderCleanUpStrategy(Factory.New<JobComInvoiceHeader>()));
	}

	public void TestCleanUpZG_AgreedPlaceCode()
	{
		invoiceHeader.ZG_AgreedPlaceCode = "10";

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("ZG_AgreedPlaceCode", "10", invoiceHeader.ZG_AgreedPlaceCode);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("ZG_AgreedPlaceCode", "", invoiceHeader.ZG_AgreedPlaceCode);
		}
	}

	public void TestCleanUpJZ_AdditionalTerms()
	{
		invoiceHeader.JZ_AdditionalTerms = "10";

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("JZ_AdditionalTerms", "10", invoiceHeader.JZ_AdditionalTerms);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("JZ_AdditionalTerms", "", invoiceHeader.JZ_AdditionalTerms);
		}
	}

	public void TestCleanUpAdditionalInfos()
	{
		declaration.JE_MessageType = "EXP";
		invoiceHeader.AdditionalInfos.AddNew();
		invoiceHeader.AdditionalInfos.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("AdditionalInfos Count", 2, invoiceHeader.AdditionalInfos.Count);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("AdditionalInfos Count", 0, invoiceHeader.AdditionalInfos.Count);
		}
	}

	public void TestCleanUpPreviousDocuments()
	{
		invoiceHeader.PreviousDocuments.AddNew();
		invoiceHeader.PreviousDocuments.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("PreviousDocuments Count", 2, invoiceHeader.PreviousDocuments.Count);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("PreviousDocuments Count", 0, invoiceHeader.PreviousDocuments.Count);
		}
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoiceHeader = declaration.Invoices.AddNew();
		strategy = new JobComInvoiceHeaderCleanUpStrategy(invoiceHeader);
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	ICleanUpStrategy strategy;
}
