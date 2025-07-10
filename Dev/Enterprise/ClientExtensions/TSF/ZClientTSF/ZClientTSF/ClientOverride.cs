using System;
using CargoWise.Definitions;
using Enterprise.Client.TSF.DocWrappers;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
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
			DocTSFForwardingShipment.RegisterThisSubTypeOverride();
		}

		public override Clients Client
		{
			get { return Clients.TSF; }
		}

		public override string ClientDisplayName
		{
			get { return "Transtar International Freight (VIC) Pty Ltd"; }
		}

		#endregion
	}
}
