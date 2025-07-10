using System;
using CargoWise.Definitions;
using Enterprise.Client.ELG;
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

		public override Clients Client
		{
			get { return Clients.ELG; }
		}

		public override string ClientDisplayName
		{
			get { return "Elite Group Logistics"; }
		}

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return ELGDataRegistry.Instance; }
		}

		#region ModuleOverrides

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();

			ClientOverrideModuleInfo aRTransactionIDInfo = new ClientOverrideModuleInfo(
				new ClientOverrideModuleIdentifier(ModuleIDs.ARTransaction),
				typeof(ELGExportModuleStrip).Assembly.FullName,
				typeof(ELGExportModuleStrip).FullName);
			moduleOverrides.AddModuleOverride(aRTransactionIDInfo);
			return moduleOverrides;
		}

		#endregion

	}
}
