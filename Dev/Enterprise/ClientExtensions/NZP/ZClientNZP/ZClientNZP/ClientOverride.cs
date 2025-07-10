using System;
using CargoWise.Definitions;
using Enterprise.Client.NZP;
using Enterprise.Client.NZP.CMS;
using Enterprise.Client.NZP.Module;
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

		protected override void InitialiseCore()
		{
			CMSBatchNumberFountain.Initialise();
		}

		public override Clients Client
		{
			get { return Clients.NZP; }
		}

		public override string ClientDisplayName
		{
			get { return "New Zealand Post"; }
		}

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return NZPDataRegistry.Instance; }
		}

		protected override ModuleOverrides GetModuleOverrides()
		{
			var	moduleOverrides = new ModuleOverrides();
			ClientOverrideModuleIdentifier aRTransactionID = new ClientOverrideModuleIdentifier(ModuleIDs.ARTransaction);
			ClientOverrideModuleInfo aRTransactionInfo = new ClientOverrideModuleInfo(aRTransactionID, typeof(CMSModule).Assembly.FullName, typeof(CMSModule).FullName);
			moduleOverrides.AddModuleOverride(aRTransactionInfo);
			return moduleOverrides;
		}

		#endregion
	}
}
