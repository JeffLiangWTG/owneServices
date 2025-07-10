using System;
using CargoWise.Definitions;
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
			get { return Clients.FTN; }
		}

		public override string ClientDisplayName
		{
			get { return "Fatton Logistics"; }
		}

		#endregion
	}
}
