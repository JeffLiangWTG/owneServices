using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSOfficeCode))]
	public class EMCSOfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<EMCSOfficeCode>
	{
		public void TestLookups()
		{
			AssertType<EMCSOfficeCodeLookups>(officeCode.Lookups);
		}

		public void TestValidation()
		{
			AssertType<EMCSOfficeCodeValidation>(officeCode.Validation);
		}

		protected override IEnumerable<EMCSOfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<EMCSJobDeclaration>().CustomsOffices.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => officeCode;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<EMCSJobDeclaration>().CustomsOffices.AddNew();

		protected override void SetUp()
		{
			base.SetUp();
			officeCode = Factory.New<EMCSJobDeclaration>().CustomsOffices.AddNew();
		}
		EMCSOfficeCode officeCode;
	}
}
