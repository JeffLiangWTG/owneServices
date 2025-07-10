using CargoWise.Definitions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.AuditDataServices.Subscription.Testing
{
	public class TestClientOverride : ClientHook
	{
		public TestClientOverride(Clients client)
		{
			ClientOverride = client;
		}

		readonly Clients ClientOverride;
		public override Clients Client => ClientOverride;

		public override string ClientDisplayName
		{
			get { return "For Test"; }
		}
	}
}
