using System;
using CargoWise.Definitions;
using Enterprise.Client.AWH;
using Enterprise.ZArchitecture.Modules;

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

		public override Clients Client
		{
			get { return Clients.AWH; }
		}

		public override string ClientDisplayName
		{
			get { return "AWH Logistics"; }
		}

		#region ModuleOverrides

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();
			ClientOverrideModuleIdentifier aRTransactionID = new ClientOverrideModuleIdentifier(ModuleIDs.ARTransaction);
			ClientOverrideModuleInfo aRTransactionModuleInfo = new ClientOverrideModuleInfo(
				aRTransactionID, typeof(AWHARTransactionModuleOverride).Assembly.FullName,
				typeof(AWHARTransactionModuleOverride).FullName);
			moduleOverrides.AddModuleOverride(aRTransactionModuleInfo);
			return moduleOverrides;
		}

		#endregion

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return AWHDataRegistry.Instance; }
		}
		#endregion
	}
}
