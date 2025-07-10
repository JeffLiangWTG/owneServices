using System;
using CargoWise.Definitions;
using Enterprise.Client.KNA;
using Enterprise.Client.KNA.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		protected ClientOverride() { }

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
			get { return Clients.KNA; }
		}

		public override string ClientDisplayName
		{
			get { return "Kuehne & Nagel Australia"; }
		}

		#region AdditionalUserVisibleRegistryItems

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return KNADataRegistry.Instance; }
		}

		#endregion

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();
			ClientOverrideModuleIdentifier moduleId = new ClientOverrideModuleIdentifier(ModuleIDs.SupplierPart);
			ClientOverrideModuleInfo moduleInfo = new ClientOverrideModuleInfo(moduleId, typeof(KNAOrgSupplierPartModuleOverride).Assembly.FullName,
				typeof(KNAOrgSupplierPartModuleOverride).FullName, Core.Constants.CountryCodes.Australia);
			moduleOverrides.AddModuleOverride(moduleInfo);
			return moduleOverrides;
		}

		#endregion
	}
}
