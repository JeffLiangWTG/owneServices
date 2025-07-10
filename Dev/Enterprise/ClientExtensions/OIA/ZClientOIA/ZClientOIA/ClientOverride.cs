using System;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using Enterprise.Client.OIA;
using Enterprise.Client.OIA.Business;
using Enterprise.Client.OIA.Module;
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

		protected override void InitialiseCore()
		{
			base.InitialiseCore();
			OIAGLExportProcessor.RegisterThisSubTypeOverride();
		}

		public override Clients Client
		{
			get { return Clients.OIA; }
		}

		public override string ClientDisplayName
		{
			get { return "OIA Global Logistics"; }
		}

		#region ModuleOverrides

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();

			moduleOverrides.AddModuleOverride(new OIAGLClientOverrideModuleInfo());
			moduleOverrides.AddModuleOverride(new OIAGLChinaClientOverrideModuleInfo());
			moduleOverrides.AddModuleOverride(new ClientOverrideModuleInfo(new ClientOverrideModuleIdentifier(ModuleIDs.GLJournal),
													typeof(OIAGLJournalModule).Assembly.FullName,
													typeof(OIAGLJournalModule).FullName, Core.Constants.CountryCodes.Taiwan));
			return moduleOverrides;
		}

		#endregion

		public override IExtensionObjects DbSchemaExtensionObjects { get; } = new OIAClientDbSchemaUpgradeInfo();
	}
}
