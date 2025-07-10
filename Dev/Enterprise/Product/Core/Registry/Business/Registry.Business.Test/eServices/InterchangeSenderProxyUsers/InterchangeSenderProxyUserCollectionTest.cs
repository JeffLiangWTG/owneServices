using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InterchangeSenderProxyUserCollection))]
	sealed class InterchangeSenderProxyUserCollectionTest : RegistryBusinessObjectCollectionTestCase<InterchangeSenderProxyUserCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => false;

		protected override InterchangeSenderProxyUserCollection GetCollectionToTest()
		{
			return new InterchangeSenderProxyUserCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new InterchangeSenderProxyUser(Factory);
		}
	}
}
