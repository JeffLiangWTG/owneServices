using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using Enterprise.Client.FSH;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		protected ClientOverride()
		{
		}

		public static ClientOverride Instance
		{
			get { return new ClientOverride(); }
		}

		protected override void InitialiseCore()
		{
			//FSHMenu.Initialise();
		}

		protected override void UninitialiseCore()
		{
			//FSHMenu.Uninitialise();
		}

		public override Clients Client
		{
			get { return Clients.FSH; }
		}

		public override string ClientDisplayName
		{
			get { return "Fortune Shipping"; }
		}

		public override string HelpWebPage
		{
			get { return ""; }
		}

		protected static readonly ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts = ImmutableArray<DatabaseObjectCreateScript>.Empty;
		protected static readonly ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutinesCreationScripts = ImmutableArray<DatabaseViewAndRoutineCreateScript>.Empty;

		public override IExtensionObjects DbSchemaExtensionObjects { get; } = new ExtensionObjects(TableCreationScripts, ViewAndRoutinesCreationScripts);

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();
			ClientOverrideModuleIdentifier consolID = new ClientOverrideModuleIdentifier(ModuleIDs.JobConsol);
			ClientOverrideModuleInfo consolModuleInfo = new ClientOverrideModuleInfo(consolID, typeof(FSHConsolModuleOverride).Assembly.FullName, typeof(FSHConsolModuleOverride).FullName);
			moduleOverrides.AddModuleOverride(consolModuleInfo);
			return moduleOverrides;
		}
	}
}
