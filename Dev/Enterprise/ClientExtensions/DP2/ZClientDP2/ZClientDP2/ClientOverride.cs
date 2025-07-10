using System;
using CargoWise.Definitions;
using Enterprise.Client.DP2;
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
			get { return Clients.DP2; }
		}

		public override string ClientDisplayName
		{
			get { return "DP World Australia Logistics Pty Ltd"; }
		}

		#region ModuleOverrides

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();
			ClientOverrideModuleIdentifier aRTransactionID = new ClientOverrideModuleIdentifier(ModuleIDs.ARTransaction);
			ClientOverrideModuleInfo aRTransactionModuleInfo = new ClientOverrideModuleInfo(
				aRTransactionID, typeof(DP2ARTransactionModuleOverride).Assembly.FullName,
				typeof(DP2ARTransactionModuleOverride).FullName);
			moduleOverrides.AddModuleOverride(aRTransactionModuleInfo);
			return moduleOverrides;
		}

		#endregion

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return DP2DataRegistry.Instance; }
		}
		#endregion
	}
}
