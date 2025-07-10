using System.Diagnostics.CodeAnalysis;
using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Shared.CW
{
	/// <summary>
	/// Shared registry instance, for all registry access by Shared project code.
	/// Use this instead of the registry singletons such as SystemDataRegistry and DataRegistry.
	/// The process controller will initialize this with an implementation that uses a single db connection.
	/// For other applications it will fallback to a simple implementation using the current thread <see cref="DirectSharedRegistrySettings"/>
	///
	/// Note, for unit tests the instance is reset after each test (in ZEnvironmentListener.AfterEachTest).
	/// </summary>
	public static class SharedRegistry
	{
		public static ISharedRegistrySettings Instance
		{
			get => instance ?? (instance = new DirectSharedRegistrySettings());
			set => instance = value;
		}
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static ISharedRegistrySettings instance;

		public static ISharedRegistrySettings InstanceWithoutAutoCreate => instance;
	}
}
