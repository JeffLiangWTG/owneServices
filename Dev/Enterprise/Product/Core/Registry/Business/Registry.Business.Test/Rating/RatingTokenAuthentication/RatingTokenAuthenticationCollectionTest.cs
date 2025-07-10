using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RatingTokenAuthenticationCollection))]
	class RatingTokenAuthenticationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<RatingTokenAuthenticationCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RatingTokenAuthenticationCollection GetCollectionToTest()
		{
			return new RatingTokenAuthenticationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RatingTokenAuthentication();
		}
	}
}
