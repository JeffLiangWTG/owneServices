
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.AUS.Modules
{
	public abstract class ClientControllerRegistration
	{
		public static readonly ClientControllerID ProductImportAndExport = new ClientControllerID("ProductImportAndExport");
		public static readonly ClientControllerID OriginPreferenceMapping = new ClientControllerID("OriginPreferenceMapping");
	}
}