using System;
using CargoWise.Definitions;
using Enterprise.Client.AGS.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		public static ClientOverride Instance
		{
			get { return instance ?? (instance = new ClientOverride()); }
		}
		[ThreadStatic]
		static ClientOverride instance;

		public override Clients Client
		{
			get { return Clients.AGS; }
		}

		public override string ClientDisplayName
		{
			get { return "AGS World Transport"; }
		}

		protected override void InitialiseCore()
		{
			AGSStmALogValueObjectDataAdapter.RegisterThisSubTypeOverride();
		}
	}
}
