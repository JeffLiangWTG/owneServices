using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADDeferredPaymentWrapperTest : TestCaseWithFactory
{
	public void TestAuthorizationReference()
	{
		AssertEquals(ZString.Empty, deferredPaymentWrapper.AuthorizationReference);
		jobDeclaration.JE_DefermentAccountNumber = "123456D";
		AssertEquals("123456", deferredPaymentWrapper.AuthorizationReference);
	}

	public void TestCinOfAuthorizationReference()
	{
		AssertEquals(ZString.Empty, deferredPaymentWrapper.CinOfAuthorizationReference);
		jobDeclaration.JE_DefermentAccountNumber = "123456D";
		AssertEquals("D", deferredPaymentWrapper.CinOfAuthorizationReference);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new SADDeferredPaymentWrapper(null));
		AssertNoExceptionThrown(() => new SADDeferredPaymentWrapper(jobDeclaration));
	}

	protected override void SetUp()
	{
		base.SetUp();

		jobDeclaration = Factory.New<JobDeclaration>();
		deferredPaymentWrapper = new SADDeferredPaymentWrapper(jobDeclaration);
	}
	JobDeclaration jobDeclaration;
	SADDeferredPaymentWrapper deferredPaymentWrapper;
}
