using CargoWise.Application;

namespace Enterprise.Customs.AE.Business;

static class ManifestController
{
	public static Integration.Customs.AEManifest.IManifestController New() => ObjectFactory.Get<Integration.Customs.AEManifest.IManifestController>();
}
