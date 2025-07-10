using System;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.CH.Business.Testing;

internal class IncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
{
	public void TestFactoryType()
	{
		AssertType<IncoTermAndCustomsChargeFactory>(incoTermAndChargeFactory);
	}

	public void TestGetOverseasFreight()
	{
		AssertEquals(nameof(CustomsChargeCode.IsDutiableDeemedForThisCharge), false, ((IncoTermAndCustomsChargeFactory)incoTermAndChargeFactory).GetOverseasFreight().IsDutiableDeemedForThisCharge);
	}

	protected override string GetCountryContext() => Core.Constants.CountryCodes.Switzerland;

	protected override Type GetCustomsChargeCodeProviderActualType() => typeof(IncoTermAndCustomsChargeFactory);
}
