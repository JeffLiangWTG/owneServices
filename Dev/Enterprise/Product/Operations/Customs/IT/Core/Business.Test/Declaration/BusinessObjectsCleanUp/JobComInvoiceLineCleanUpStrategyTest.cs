using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobComInvoiceLineCleanUpStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When invoiceLine is null", () => new JobComInvoiceLineCleanUpStrategy(invoiceLine: null));
		AssertExceptionThrown<ArgumentNullException>("When invoiceLine.Declaration is null", () => new JobComInvoiceLineCleanUpStrategy(Factory.New<JobComInvoiceLine>()));
	}

	public void TestCleanUpJI_OA_ExporterAddress_ZAddress()
	{
		invoiceLine.JI_OA_ExporterAddress_ZAddress.OrgPK = ZGuid.BrettsGuid;

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			strategy.CleanUp();
			AssertEquals("JI_OA_ExporterAddress_ZAddress.OrgPK", ZGuid.BrettsGuid, invoiceLine.JI_OA_ExporterAddress_ZAddress.OrgPK);
		}

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			strategy.CleanUp();
			AssertEquals("JI_OA_ExporterAddress_ZAddress.OrgPK", ZGuid.Empty, invoiceLine.JI_OA_ExporterAddress_ZAddress.OrgPK);
		}
	}

	public void TestCleanUpCusSupplyChainActorReferences()
	{
		invoiceLine.CusSupplyChainActorReferences.AddNew();
		invoiceLine.CusSupplyChainActorReferences.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("CusSupplyChainActorReferences Count", 2, invoiceLine.CusSupplyChainActorReferences.Count);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("CusSupplyChainActorReferences Count", 0, invoiceLine.CusSupplyChainActorReferences.Count);
		}
	}

	public void TestCleanUpZG_CusNumber()
	{
		invoiceLine.ZG_CusNumber = "AA";

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("ZG_CusNumber", "AA", invoiceLine.ZG_CusNumber);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("ZG_CusNumber", "", invoiceLine.ZG_CusNumber);
		}
	}

	public void TestCleanUpCusAuthorizationUsages()
	{
		declaration.JE_MessageType = "EXP";
		invoiceLine.CusAuthorizationUsages.AddNew();
		invoiceLine.CusAuthorizationUsages.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("CusAuthorizationUsages Count", 2, invoiceLine.CusAuthorizationUsages.Count);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("CusAuthorizationUsages Count", 0, invoiceLine.CusAuthorizationUsages.Count);
		}
	}

	public void TestCleanUpJI_OA_ConsigneeAddress_ZAddress()
	{
		declaration.JE_MessageType = "EXP";
		invoiceLine.JI_OA_ConsigneeAddress_ZAddress.OrgPK = ZGuid.BrettsGuid;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("JI_OA_ConsigneeAddress_ZAddress.OrgPK", ZGuid.BrettsGuid, invoiceLine.JI_OA_ConsigneeAddress_ZAddress.OrgPK);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("JI_OA_ConsigneeAddress_ZAddress.OrgPK", ZGuid.Empty, invoiceLine.JI_OA_ConsigneeAddress_ZAddress.OrgPK);
		}
	}

	public void TestCleanUpFiscalReferences()
	{
		invoiceLine.FiscalReferences.AddNew();
		invoiceLine.FiscalReferences.AddNew();

		declaration.JE_MessageType = "IMP";
		strategy.CleanUp();
		AssertEquals("FiscalReferences Count", 2, invoiceLine.FiscalReferences.Count);

		declaration.JE_MessageType = "XXX";
		strategy.CleanUp();
		AssertEquals("FiscalReferences Count", 0, invoiceLine.FiscalReferences.Count);
	}

	public void TestCleanUpJI_RN_NKCountryOfExport()
	{
		declaration.JE_MessageType = "EXP";
		invoiceLine.JI_RN_NKCountryOfExport = "AU";

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("JI_RN_NKCountryOfExport", "AU", invoiceLine.JI_RN_NKCountryOfExport);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("JI_RN_NKCountryOfExport", "", invoiceLine.JI_RN_NKCountryOfExport);
		}
	}

	public void TestCleanUpAdditionalProcedureCodes()
	{
		invoiceLine.AdditionalProcedureCodes.AddNew();
		invoiceLine.AdditionalProcedureCodes.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("AdditionalProcedureCodes Count", 2, invoiceLine.AdditionalProcedureCodes.Count);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("AdditionalProcedureCodes Count", 0, invoiceLine.AdditionalProcedureCodes.Count);
		}
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		strategy = new JobComInvoiceLineCleanUpStrategy(invoiceLine);
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	ICleanUpStrategy strategy;
}
