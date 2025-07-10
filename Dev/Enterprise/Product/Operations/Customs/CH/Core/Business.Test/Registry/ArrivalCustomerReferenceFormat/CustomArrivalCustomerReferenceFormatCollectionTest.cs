using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CustomArrivalCustomerReferenceFormatCollection))]
class CustomArrivalCustomerReferenceFormatCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CustomArrivalCustomerReferenceFormatCollection>
{
	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => true;

	protected override CustomArrivalCustomerReferenceFormatCollection GetCollectionToTest()
	{
		var fallbackLevel = new FallbackLevel(Env.CurrentCompany, null, null);
		return new CustomArrivalCustomerReferenceFormatCollection(fallbackLevel, Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var fallbackLevel = new FallbackLevel(Env.CurrentCompany, null, null);
		return new CustomArrivalCustomerReferenceFormat(fallbackLevel, Factory);
	}
}
