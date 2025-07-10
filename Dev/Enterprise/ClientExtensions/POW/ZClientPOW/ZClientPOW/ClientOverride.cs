using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using Enterprise.Client.ZClientPOW.GUI;
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

		#region IClientHook Members

		protected override void InitialiseCore()
		{
			POWMenu.Initialise();
		}

		public override Clients Client
		{
			get { return Clients.POW; }
		}

		public override string ClientDisplayName
		{
			get { return "POW"; }
		}

		public override string HelpWebPage
		{
			get { return ""; }
		}

		protected static readonly DatabaseObjectCreateScript[] TableCreationScripts = System.Array.Empty<DatabaseObjectCreateScript>();
		protected static readonly DatabaseViewAndRoutineCreateScript[] ViewAndRoutinesCreationScripts = System.Array.Empty<DatabaseViewAndRoutineCreateScript>();

		public override IExtensionObjects DbSchemaExtensionObjects { get; } = new ExtensionObjects(ImmutableArray<DatabaseObjectCreateScript>.Empty, ImmutableArray<DatabaseViewAndRoutineCreateScript>.Empty);

		#endregion
	}
}
