using CargoWise.Definitions;
using Enterprise.Client.SCP;
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

		#region IClientHook Members

		protected override void InitialiseCore()
		{
			DocSkyLiftAWB.RegisterThisSubTypeOverride();
		}

		public override Clients Client
		{
			get { return Clients.SkyLift; }
		}

		public override string ClientDisplayName
		{
			get { return "SkyLift"; }
		}
		#endregion
	}
	}
