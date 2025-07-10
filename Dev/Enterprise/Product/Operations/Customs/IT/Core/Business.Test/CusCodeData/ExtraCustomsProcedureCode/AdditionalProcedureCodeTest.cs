using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(AdditionalProcedureCode))]
sealed class AdditionalProcedureCodeTest : Customs.Business.Testing.CusCodeDataTest<AdditionalProcedureCode>
{
	public void TestValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var additionalProcedueCode = declaration
			.Invoices.AddNew()
			.InvoiceLines.AddNew()
			.AdditionalProcedureCodes.AddNew();

		AssertType<AdditionalProcedureCodeValidation>("When is not EXP UCC6", additionalProcedueCode.Validation);

		declaration.JE_MessageType = "EXP";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertType<ExportUcc6AdditionalProcedureCodeValidation>("When is EXP UCC6", additionalProcedueCode.Validation);
		}
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		return factory.NewWithValidTestData<AdditionalProcedureCode>();
	}

	protected override IEnumerable<AdditionalProcedureCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return factory.New<JobDeclaration>()
			.Invoices.AddNew()
			.InvoiceLines.AddNew()
			.AdditionalProcedureCodes.AddNew();
	}
}
