using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(AsycudaBillFindBoxCollection))]
sealed class AsycudaBillFindBoxCollectionTest : ActiveBusinessObjectCollectionTestCase<AsycudaBillFindBoxCollection>
{
	protected override AsycudaBillFindBoxCollection GetCollectionToTest()
	{
		return new AsycudaBillFindBoxCollection(Factory);
	}

	[ExpectNoExceptions]
	public void TestModuleIDAttribute() => CombineAssertions(() =>
	{
		var attribute = (ModuleIDAttribute)Attribute.GetCustomAttribute(typeof(AsycudaBillFindBoxCollection), typeof(ModuleIDAttribute));
		NUnit.Framework.Assert.That(attribute, Is.Not.Null);
		NUnit.Framework.Assert.That(attribute.ModuleId, Is.EqualTo(ModuleId.ManifestBill));
	});
}
