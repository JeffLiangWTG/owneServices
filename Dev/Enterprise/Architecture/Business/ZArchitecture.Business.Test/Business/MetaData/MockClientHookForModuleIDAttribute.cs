using System.Collections.Generic;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class MockClientHookForModuleIDAttribute : ClientHook
	{
		protected override NewClientModuleInfo[] NewClientModulesCore
		{
			get
			{
				var modules = new List<NewClientModuleInfo>();

				modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations.Name,
					"Test",
					new ModuleInfo(TestNewClientModulesCore,
						"Assamblyname", "classfullname")));
				return modules.ToArray();
			}
		}

		public static ClientOverrideModuleIdentifier TestNewClientModulesCore = new ClientOverrideModuleIdentifier(ModuleIDs.Opportunity, "TestName");

		#region IClientHook Members

		protected override void UninitialiseCore()
		{
		}

		protected override void InitialiseCore()
		{
		}

		public override Clients Client
		{
			get { return Clients.EDI; }
		}

		public override string ClientDisplayName
		{
			get { return ""; }
		}

		public override string HelpWebPage
		{
			get { return ""; }
		}

		protected override ControllerOverrides GetControllerOverrides() => null;

		protected override ModuleOverrides GetModuleOverrides()
		{
			var overrides = new ModuleOverrides();

			ClientOverrideModuleIdentifier eDIOrgModuleOverrideID = new ClientOverrideModuleIdentifier(ModuleIDs.Organisation, (NoResString)"ModuleOverrides");
			ClientOverrideModuleInfo eDIOrgNewClientModuleInfo = new ClientOverrideModuleInfo(
				eDIOrgModuleOverrideID, "typeof(Module)",
				"typeof(Module).FullName");
			overrides.AddModuleOverride(eDIOrgNewClientModuleInfo);
			return overrides;
		}

		public override IExtensionObjects DbSchemaExtensionObjects => null;

		#endregion
	}
}
