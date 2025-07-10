using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CustomsRegistryCollection))]
class CustomsRegistryNonPersistentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CustomsRegistryCollection>
{
	public void TestPreventNewAndRemove()
	{
		var collection = new CustomsRegistryCollection();
		CombineAssertions(() =>
		{
			AssertEquals("AllowNew", true, collection.AllowNew);
			AssertEquals("AllowRemove", true, collection.AllowRemove);
		});
	}

	public void TestDefaultValues()
	{
		var collection = new CustomsRegistryCollection();

		AssertType<CustomsRegistryCollection>(collection.DefaultCollection);
	}

	protected override CustomsRegistryCollection GetCollectionToTest() => new CustomsRegistryCollection();

	protected override BusinessObject GetNewElementToAddToTheCollection() => new CustomsRegistry();
}

[TestedType(typeof(CustomsRegistryCollection))]
class CustomsRegistryCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CustomsRegistryCollection>
{
	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => false;

	protected override CustomsRegistryCollection GetCollectionToTest() => new CustomsRegistryCollection();

	protected override BusinessObject GetNewElementToAddToTheCollection() => new CustomsRegistry();
}
