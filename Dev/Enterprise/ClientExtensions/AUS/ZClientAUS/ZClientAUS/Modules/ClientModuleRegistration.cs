
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.AUS.Modules
{
	public enum AUSModuleId
	{
		ProductImportAndExport,
		OriginPreferenceMapping
	}

	public abstract class ClientModuleRegistration
	{
		public static readonly ModuleIdentifier ProductImportAndExport = new ClientModuleIdentifier(AUSModuleId.ProductImportAndExport, "Set-up Product Import & Export");
		public static readonly ModuleIdentifier OriginPreferenceMapping = new ClientModuleIdentifier(AUSModuleId.OriginPreferenceMapping, "Origin-Preference Mapping");
	}
}