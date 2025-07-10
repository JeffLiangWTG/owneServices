using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(JPCusReferenceValidation))]
sealed class JPCusReferenceValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCFR_Reference()
	{
		var reference = Factory.New<CusReferenceForTest>();
		ValidationTestHelper.AssertErrorIfNotEntered(reference.CFR_ReferenceInfo);
	}
}

sealed class CusReferenceForTest(BusinessObjectFactory factory, DataRow row) : CusReference(factory, row)
{
	protected override CusReferenceValidation GetNewValidation() => new JPCusReferenceValidation(this);
}
