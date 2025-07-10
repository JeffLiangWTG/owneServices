using System;
using CargoWise.Definitions;
using Enterprise.Client.NIP;
using Enterprise.ZArchitecture.Modules;
#if DEBUG
using Enterprise.Client.NIP.Module;
#endif

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		protected ClientOverride()
		{
		}

		#region Instance

		public static ClientOverride Instance
		{
			get { return instance ?? (instance = new ClientOverride()); }
		}
		[ThreadStatic]
		static ClientOverride instance;

		#endregion

		#region IClientHook Members

#if DEBUG
		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();

			ClientOverrideModuleIdentifier consolModuleOverrideID = new ClientOverrideModuleIdentifier(ModuleIDs.JobConsol);
			ClientOverrideModuleInfo consolOverrideModuleInfo = new ClientOverrideModuleInfo(consolModuleOverrideID, typeof(NIPJobConsolModule).Assembly.FullName, typeof(NIPJobConsolModule).FullName);
			moduleOverrides.AddModuleOverride(consolOverrideModuleInfo);
			return moduleOverrides;
		}
#endif

		public override Clients Client
		{
			get { return Clients.NIP; }
		}

		public override string ClientDisplayName
		{
			get { return "Nippon Express (Australia) Pty Ltd"; }
		}

		#region Additional Registry Item Set

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return NIPDataRegistry.Instance; }
		}

		#endregion

		#endregion
	}
}
