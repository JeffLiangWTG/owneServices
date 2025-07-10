using CargoWise.Application;
using CargoWise.Database.Abstractions.Extensions;
using Enterprise.Integration.ZArchitecture;

namespace Enterprise.ZArchitecture.Modules
{
	public sealed class ClientSpecificExtensionObjectsSource : IExtensionObjectsSource
	{
		public string DisplayName => ClientHookLoader.Instance.ClientHook?.ClientDisplayName;
		public string ExtensionCode => ObjectFactory.Get<IEnterpriseCodeRetriever>().EnterpriseCodeFromRegistry;
		public IExtensionObjects ExtensionObjects => ClientHookLoader.Instance.ClientHook?.DbSchemaExtensionObjects;
	}
}
