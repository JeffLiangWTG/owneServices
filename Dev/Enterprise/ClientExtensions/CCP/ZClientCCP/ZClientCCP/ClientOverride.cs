using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using Enterprise.Client.ZClientCCP.GUI;
using Enterprise.Client.ZClientCCP.Module;
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

		#region ModuleOverride

		protected override ModuleOverrides GetModuleOverrides()
		{
			var overrides = new ModuleOverrides();

			ClientOverrideModuleIdentifier commercialInvoiceID = new ClientOverrideModuleIdentifier(ModuleIDs.CommercialInvoice);

			ClientOverrideModuleInfo auCommercialInvoiceModuleInfo = new ClientOverrideModuleInfo(
				commercialInvoiceID, typeof(AUCommercialInvoiceModuleOverride),
				Enterprise.Core.Constants.CountryCodes.Australia);
			overrides.AddModuleOverride(auCommercialInvoiceModuleInfo);

			ClientOverrideModuleInfo sGCommercialInvoiceModuleInfo = new ClientOverrideModuleInfo(
				commercialInvoiceID, typeof(SGCommercialInvoiceModuleOverride),
				Enterprise.Core.Constants.CountryCodes.Singapore);
			overrides.AddModuleOverride(sGCommercialInvoiceModuleInfo);

			ClientOverrideModuleInfo uSCommercialInvoiceModuleInfo = new ClientOverrideModuleInfo(
				commercialInvoiceID, typeof(USCommercialInvoiceModuleOverride),
				Enterprise.Core.Constants.CountryCodes.UnitedStates);
			overrides.AddModuleOverride(uSCommercialInvoiceModuleInfo);

			ClientOverrideModuleInfo pRCommercialInvoiceModuleInfo = new ClientOverrideModuleInfo(
				commercialInvoiceID, typeof(USCommercialInvoiceModuleOverride),
				Enterprise.Core.Constants.CountryCodes.PuertoRico);
			overrides.AddModuleOverride(pRCommercialInvoiceModuleInfo);

			return overrides;
		}

		#endregion

		#region IClientHook Members

		protected override void InitialiseCore()
		{
			CCPMenu.Initialise();
		}

		public override Clients Client
		{
			get { return Clients.CCP; }
		}

		public override string ClientDisplayName
		{
			get { return "CCP"; }
		}

		public override string HelpWebPage
		{
			get { return ""; }
		}

		protected static readonly ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts = ImmutableArray<DatabaseObjectCreateScript>.Empty;
		protected static readonly ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutinesCreationScripts = ImmutableArray<DatabaseViewAndRoutineCreateScript>.Empty;

		public override IExtensionObjects DbSchemaExtensionObjects { get; } = new ExtensionObjects(TableCreationScripts, ViewAndRoutinesCreationScripts);

		#endregion
	}
}
