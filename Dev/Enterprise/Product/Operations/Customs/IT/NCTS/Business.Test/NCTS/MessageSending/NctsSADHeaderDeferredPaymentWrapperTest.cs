using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsSADHeaderDeferredPaymentWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When movementHeader is null ", () => new NctsSADHeaderDeferredPaymentWrapper(null));
		AssertNoExceptionThrown(() => new NctsSADHeaderDeferredPaymentWrapper(nctsHeader.MovementHeader));
	}

	public void TestAuthorizationReference()
	{
		nctsHeader.MovementHeader.DefermentAccountNumber = ZString.Empty;
		AssertEquals(nameof(wrapper.AuthorizationReference), ZString.Empty, wrapper.AuthorizationReference);

		nctsHeader.MovementHeader.DefermentAccountNumber = "123456D";
		AssertEquals(nameof(wrapper.AuthorizationReference), "123456", wrapper.AuthorizationReference);
	}

	public void TestCinOfAuthorizationReference()
	{
		nctsHeader.MovementHeader.DefermentAccountNumber = ZString.Empty;
		AssertEquals(nameof(wrapper.CinOfAuthorizationReference), ZString.Empty, wrapper.CinOfAuthorizationReference);

		nctsHeader.MovementHeader.DefermentAccountNumber = "123456D";
		AssertEquals(nameof(wrapper.CinOfAuthorizationReference), "D", wrapper.CinOfAuthorizationReference);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		wrapper = new NctsSADHeaderDeferredPaymentWrapper(nctsHeader.MovementHeader);
	}
	NctsHeader nctsHeader;
	NctsSADHeaderDeferredPaymentWrapper wrapper;
}
