using System;
using CargoWise.Definitions;
using Enterprise.Client.ATL.DocWrappers;
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
			DocATLAPPayment.RegisterThisSubTypeOverride();
		}

		public override Clients Client
		{
			get { return Clients.ATL; }
		}

		public override string ClientDisplayName
		{
			get { return "All Transport Network Inc"; }
		}

		#endregion
	}
}
