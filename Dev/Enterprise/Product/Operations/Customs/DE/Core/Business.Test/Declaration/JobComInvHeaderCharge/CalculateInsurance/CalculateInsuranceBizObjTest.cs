using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing;

[TestedType(typeof(CalculateInsuranceBizObj))]
sealed class CalculateInsuranceBizObjTest : NonPersistentBusinessObjectTestCase
{
	public void TestIsDutiablePercentEnabled_ShouldBeFalseForDE()
	{
		var bizObj = (CalculateInsuranceBizObj)GetNewBusinessObject();

		AssertEquals(false, bizObj.IsDutiablePercentEnabled);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var chargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;
		var bizObj = new CalculateInsuranceBizObj(chargeFactory, invoice);
		return bizObj;
	}
}
