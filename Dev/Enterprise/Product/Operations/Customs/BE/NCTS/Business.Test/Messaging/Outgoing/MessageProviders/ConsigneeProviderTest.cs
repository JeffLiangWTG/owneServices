using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(ConsigneeProvider))]
	sealed class ConsigneeProviderTest : PartyProviderAbstractTest<ConsigneeProvider>
	{
		protected override bool ExpectProviderIncludesContactPerson => false;
	}
}
