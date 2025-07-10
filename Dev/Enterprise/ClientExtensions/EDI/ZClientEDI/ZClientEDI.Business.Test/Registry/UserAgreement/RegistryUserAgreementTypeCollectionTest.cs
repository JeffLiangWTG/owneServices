using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Testing
{
	[TestedType(typeof(RegistryUserAgreementTypeCollection))]
	class RegistryUserAgreementTypeCollectionTest : RegistryBusinessObjectCollectionTestCase<RegistryUserAgreementTypeCollection>
	{
		protected override RegistryUserAgreementTypeCollection GetCollectionToTest()
		{
			return new RegistryUserAgreementTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RegistryUserAgreementType();
		}

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;
	}
}
