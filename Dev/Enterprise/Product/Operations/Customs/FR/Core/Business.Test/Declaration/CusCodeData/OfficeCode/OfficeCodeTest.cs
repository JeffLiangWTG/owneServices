using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing;

[TestedType(typeof(OfficeCode))]
sealed class OfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<OfficeCode>
{
	public void TestGetNewValidation()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			var office = declaration.CustomsOffices.AddNew();
			AssertType<OfficeCodeValidation>(office.Validation);
		});
	}
	protected override IEnumerable<OfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return factory.New<JobDeclaration>().CustomsOffices.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().CustomsOffices.AddNew();

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<JobDeclaration>().CustomsOffices.AddNew();
}
