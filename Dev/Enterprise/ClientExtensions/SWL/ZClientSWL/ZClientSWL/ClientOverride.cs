using System;
using CargoWise.Definitions;
using Enterprise.Client.SWL;
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
			get { return Clients.SWL; }
		}

		public override string ClientDisplayName
		{
			get { return "Seaway Logistics"; }
		}

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return SWLDataRegistry.Instance; }
		}

		protected override ControllerOverrides GetControllerOverrides()
		{
			var controllerOverrides = new ControllerOverrides();

			var orgControllerId = new ClientOverrideControllerID(ControllerIDs.Organisation);
			var controllerIdInfo = new ClientOverrideControllerInfo(
				orgControllerId,
				typeof(SWLOrganisationControllerOverride).Assembly.FullName,
				typeof(SWLOrganisationControllerOverride).FullName);

			controllerOverrides.AddControllerOverride(controllerIdInfo);
			return controllerOverrides;
		}

		protected override ControllerInfo[] NewClientControllersCore
		{
			get { return newClientControllers ?? (newClientControllers = new[] { new ControllerInfo(ControllerIDs.ShipnetSetupPlugIn, typeof(ShipnetSetupPlugInController)) }); }
		}

		ControllerInfo[] newClientControllers;

		#endregion

	}
}
