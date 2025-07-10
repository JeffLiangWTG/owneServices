using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(DEOfficeCode))]
	class DEOfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<DEOfficeCode>
	{
		[ExpectNoExceptions]
		public void TestLookups()
		{
			var officeCode = (DEOfficeCode)GetNewBusinessObject();
			NUnit.Framework.Assert.That(officeCode.Lookups, NUnit.Framework.Is.TypeOf<DEOfficeCodeLookups>());
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			var officeCode = (DEOfficeCode)GetNewBusinessObject();
			NUnit.Framework.Assert.That(officeCode.Validation, NUnit.Framework.Is.TypeOf<DEOfficeCodeValidation>());
		}

		protected override IEnumerable<DEOfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().CustomsOffices.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().CustomsOffices.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<JobDeclaration>().CustomsOffices.AddNew();
	}
}
