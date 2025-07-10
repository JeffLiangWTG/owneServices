using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CarrierProvider))]
	class CarrierProviderTest : PartyProviderAbstractTest<CarrierProvider>
	{
		protected override bool ExpectProviderIncludesContactPerson => false;
	}
}
