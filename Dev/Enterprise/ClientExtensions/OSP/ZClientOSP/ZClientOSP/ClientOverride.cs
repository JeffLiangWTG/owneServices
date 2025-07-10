using System;
using CargoWise.Definitions;
using Enterprise.Client.OSP.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client
{
	class ClientOverride : ClientHook
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

		public override Clients Client
		{
			get { return Clients.OSP; }
		}

		public override string ClientDisplayName
		{
			get { return "Oceanbridge Shipping Limited"; }
		}

		#region ModuleOverrides

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();

			ClientOverrideModuleInfo consolInfo = new ClientOverrideModuleInfo(new ClientOverrideModuleIdentifier(ModuleIDs.JobConsol), typeof(OSPConsolModuleOverride).Assembly.FullName, typeof(OSPConsolModuleOverride).FullName);
			moduleOverrides.AddModuleOverride(consolInfo);
			return moduleOverrides;
		}

		#endregion
	}
}
