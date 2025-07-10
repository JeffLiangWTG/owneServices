using System;
using CargoWise.Definitions;
using Enterprise.Client.ECU.ConsolExport;
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
			get { return fInstance ?? (fInstance = new ClientOverride()); }
		}
		[ThreadStatic]
		static ClientOverride fInstance;

		#endregion

		#region IClientHook Members

		protected override void InitialiseCore()
		{
			ECUActionMenu.Initialise();
			ECUFileCounter.Initialise();
		}

		public override Clients Client
		{
			get { return Clients.ECU; }
		}

		public override string ClientDisplayName
		{
			get { return "ECU"; }
		}

		#endregion
	}
}
