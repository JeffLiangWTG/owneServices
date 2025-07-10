using CargoWise.Definitions;
using Enterprise.Client.HEN.GUI;
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
		static ClientOverride instance;

		#endregion

		public override Clients Client
		{
			get { return Clients.HEN; }
		}

		public override string ClientDisplayName
		{
			get { return "Henning Harders (Australia) Pty Ltd"; }
		}

		protected override void InitialiseCore()
		{
			HENMenu.Initialise();
		}
	}
}
