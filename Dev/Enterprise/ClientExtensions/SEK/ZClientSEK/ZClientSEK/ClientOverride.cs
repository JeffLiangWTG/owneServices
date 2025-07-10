using System;
using CargoWise.Definitions;
using Enterprise.Client.SEK;
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
			SEKDocAWB.RegisterThisSubTypeOverride();
		}

		public override Clients Client
		{
			get { return Clients.SEK; }
		}

		public override string ClientDisplayName
		{
			get { return "SEKO Global Logistics"; }
		}

		#endregion
	}
}
