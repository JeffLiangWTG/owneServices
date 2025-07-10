using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class CommercialReferenceAdditionalTextValidatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When movement header is null", () => new CommercialReferenceAdditionalTextValidator(null));
		AssertNoExceptionThrown("When movement header is an initialized object", () => new CommercialReferenceAdditionalTextValidator(Factory.New<NctsDepartureMovementHeader>()));
	}
}
