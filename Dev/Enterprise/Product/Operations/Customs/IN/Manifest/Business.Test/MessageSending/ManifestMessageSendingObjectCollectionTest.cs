using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(ManifestMessageSendingObjectCollection))]
sealed class ManifestMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ManifestMessageSendingObjectCollection>
{
	public void TestAllowNew()
	{
		AssertEquals("AllowNew should be false", expected: false, GetCollectionToTest().AllowNew);
	}

	protected override ManifestMessageSendingObjectCollection GetCollectionToTest()
	{
		return new ManifestMessageSendingObjectCollection(Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var manifestHeader = Factory.New<CGMAsycudaManifestHeader>();
		return new ManifestMessageSendingObject(manifestHeader);
	}
}
