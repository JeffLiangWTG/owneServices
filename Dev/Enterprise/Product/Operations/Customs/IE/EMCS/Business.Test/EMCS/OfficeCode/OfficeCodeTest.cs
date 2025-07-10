using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(OfficeCode))]
	sealed class OfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<OfficeCode>
	{
		public void TestOfficeCodesUseDesInsteadOfCaa()
		{
			Assert("Should use DES instead of CAA", officeCode.OfficeCodesUseDesInsteadOfCaa);
		}

		protected override BusinessObject GetNewBusinessObject() => officeCode;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<EMCSJobDeclaration>().CustomsOffices.AddNew();

		protected override void SetUp()
		{
			base.SetUp();
			officeCode = Factory.New<EMCSJobDeclaration>().CustomsOffices.AddNew();
		}
		OfficeCode officeCode;
	}
}
