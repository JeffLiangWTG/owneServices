namespace Enterprise.Customs.AE.Manifest.Business;

public sealed class ManifestController : Integration.Customs.AEManifest.IManifestController
{
	public object InterchangeSegmentProvider => new ManifestInterchangeSegmentProvider();

	public object MessageAttacheeProvider => new ManifestMessageBillProvider();
}
