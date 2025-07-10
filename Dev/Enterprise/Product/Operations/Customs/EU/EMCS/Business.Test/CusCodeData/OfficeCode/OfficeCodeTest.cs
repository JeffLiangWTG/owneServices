using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(OfficeCode))]
	class OfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<OfficeCode>
	{
		public void TestLookups()
		{
			AssertType<OfficeCodeLookups>(officeCode.Lookups);
		}

		public void TestValidation()
		{
			AssertType<OfficeCodeValidation>(officeCode.Validation);
		}

		protected override IEnumerable<OfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (OfficeCode)factory.New<EMCSJobDeclaration>().CustomsOffices.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => officeCode;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<EMCSJobDeclaration>().CustomsOffices.AddNew();

		protected override void SetUp()
		{
			base.SetUp();
			officeCode = (OfficeCode)Factory.New<EMCSJobDeclaration>().CustomsOffices.AddNew();
		}
		OfficeCode officeCode;
	}
}
