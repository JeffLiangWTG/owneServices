using System;
using CargoWise.Definitions;
using Enterprise.Client.TEL;
using Enterprise.ZArchitecture.Environment;
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
			get { return Clients.TEL; }
		}

		public override string ClientDisplayName
		{
			get { return "Transways Logistics International"; }
		}

		/* Comment out this override if you don't need it but don't delete it
		 * as chances are you'll need it pretty soon.
		 *
		 * By all means delete this comment though if you do need the override.
		 */
		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return TELDataRegistry.Instance; }
		}

		#region ModuleOverrides
		protected override ModuleOverrides GetModuleOverrides()
		{
			if (Globals.IsDebugMode)
			{
				var moduleOverrides = new ModuleOverrides();

				ClientOverrideModuleIdentifier concolID = new ClientOverrideModuleIdentifier(ModuleIDs.JobConsol);
				ClientOverrideModuleInfo consolModuleInfo = new ClientOverrideModuleInfo(concolID, typeof(TEL.Modules.TELJobConsolModule).Assembly.FullName, typeof(TEL.Modules.TELJobConsolModule).FullName);
				moduleOverrides.AddModuleOverride(consolModuleInfo);
				return moduleOverrides;
			}
			return base.GetModuleOverrides();
		}
		#endregion

	}
}
