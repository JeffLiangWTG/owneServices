using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public class ManifestSupportingDocSendingObjectCollection : NonPersistentBusinessObjectCollection<SupportingDocSendingObject>
{
	public ManifestSupportingDocSendingObjectCollection(ISupportingDocObject manifest) : base(manifest.Factory)
	{
		this.manifest = manifest;
	}

	protected ISupportingDocObject manifest;

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		return CreateSupportingDocSendingObject();
	}

	protected virtual BusinessObject CreateSupportingDocSendingObject()
	{
		return SupportingDocSendingObject.New(manifest);
	}
}
