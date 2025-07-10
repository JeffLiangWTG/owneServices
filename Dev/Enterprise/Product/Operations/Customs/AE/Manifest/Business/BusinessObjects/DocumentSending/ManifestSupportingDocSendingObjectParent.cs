using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.AE.Manifest.Business;

public class ManifestSupportingDocSendingObjectParent : BaseMessageSendingObjectParent<SupportingDocSendingObject>
{
	public ManifestSupportingDocSendingObjectParent(AsycudaManifestHeader manifest) : base(manifest.Factory)
	{
		ParentManifest = manifest;
	}
	public readonly AsycudaManifestHeader ParentManifest;

	public override BusinessObject TopLevelBusinessObject => ParentManifest;

	public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

	protected override NonPersistentBusinessObjectCollection<SupportingDocSendingObject> GetSendingObjectsCollectionCore()
	{
		return new ManifestSupportingDocSendingObjectCollection(ParentManifest);
	}
}
