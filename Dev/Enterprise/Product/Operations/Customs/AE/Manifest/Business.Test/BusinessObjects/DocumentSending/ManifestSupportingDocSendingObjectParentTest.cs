using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(ManifestSupportingDocSendingObjectParent))]
sealed class ManifestSupportingDocSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestSendingObjectsCollection() => CombineAssertions(() =>
	{
		var testWrapper = new ManifestSupportingDocSendingObjectParent(manifest);
		AssertEquals(0, testWrapper.SendingObjectsCollection.Count);
		AssertEquals(0, testWrapper.SendingObjectsCollection.Count);
		testWrapper.SendingObjectsCollection.AddNew();
		testWrapper.SendingObjectsCollection.AddNew();
		AssertEquals(2, testWrapper.SendingObjectsCollection.Count);
	});

	public void TestTopLevelBusinessObject()
	{
		var testWrapper = new ManifestSupportingDocSendingObjectParent(manifest);
		AssertType<AsycudaManifestHeader>(testWrapper.TopLevelBusinessObject);
	}

	public void TestSecurityCheckpointToSendWithMessageError()
	{
		var testWrapper = new ManifestSupportingDocSendingObjectParent(manifest);
		AssertEquals(Env.Security.CustomsDeclarationSendWithMessageErrors, testWrapper.SecurityCheckpointToSendWithMessageError);
	}

	protected override BusinessObject GetNewBusinessObject() => new ManifestSupportingDocSendingObjectParent(manifest);

	protected override void SetUp()
	{
		base.SetUp();
		manifest = Factory.New<AsycudaManifestHeader>();
	}

	AsycudaManifestHeader manifest;
}
