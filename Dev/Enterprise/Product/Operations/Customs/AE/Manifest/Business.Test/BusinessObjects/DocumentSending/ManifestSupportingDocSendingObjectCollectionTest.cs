using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(ManifestSupportingDocSendingObjectCollection))]
sealed class ManifestSupportingDocSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ManifestSupportingDocSendingObjectCollection>
{
	protected override ManifestSupportingDocSendingObjectCollection GetCollectionToTest() => new ManifestSupportingDocSendingObjectCollection(manifest);

	protected override BusinessObject GetNewElementToAddToTheCollection() => SupportingDocSendingObject.New(manifest);

	protected override void SetUp()
	{
		base.SetUp();
		manifest = Factory.New<AsycudaManifestHeader>();
	}

	AsycudaManifestHeader manifest;
}
