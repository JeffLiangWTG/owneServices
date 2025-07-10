using CargoWise.Definitions;
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

		public override Clients Client
		{
			get { return Clients.ASA; }
		}

		public override string ClientDisplayName
		{
			get { return "ASA"; }
		}
	}
}
