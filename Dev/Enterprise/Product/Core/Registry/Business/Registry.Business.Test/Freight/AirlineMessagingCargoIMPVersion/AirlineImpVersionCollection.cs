using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Freight.AirlineMessagingCargoIMPVersion;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Freight.AirlineMessagingCargoIMPVersion
{
	[TestedType(typeof(AirlineImpVersionCollection))]
	sealed class AirlineImpVersionCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<AirlineImpVersionCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override AirlineImpVersionCollection GetCollectionToTest()
		{
			return new AirlineImpVersionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AirlineImpVersion();
		}
	}
}
