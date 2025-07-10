using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(OfficeCode))]
	class OfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<OfficeCode>
	{
		public void TestValidation()
		{
			var officeCode = GetNewOfficeCode();
			AssertType<OfficeCodeValidation>("Validation Type", officeCode.Validation);
		}
		public void TestLookups()
		{
			var officeCode = GetNewOfficeCode();
			AssertType<OfficeCodeLookups>("Lookups Type", officeCode.Lookups);
		}

		protected override IEnumerable<OfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().CustomsOffices.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().CustomsOffices.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<JobDeclaration>().CustomsOffices.AddNew();

		OfficeCode GetNewOfficeCode() => (OfficeCode)GetNewBusinessObject();
	}
}
