using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC141CConsigneeProvider))]
	sealed class CC141CConsigneeProviderTest : PartyProviderAbstractTest<CC141CConsigneeProvider>
	{
		protected override bool ExpectNameToBeNullWithIdentificationNumber => false;

		protected override bool ExpectNameToBeNullWithoutIdentificationNumber => false;

		protected override bool ExpectAddressToBeNullWithIdentificationNumber => false;

		protected override bool ExpectAddressToBeNullWithoutIdentificationNumber => false;

		protected override bool ExpectProviderIncludesContactPerson => false;
	}
}
