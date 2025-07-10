using CargoWise.Definitions;
using Enterprise.Client.GCG.DocWrappers;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		public static ClientOverride Instance
		{
			get { return new ClientOverride(); }
		}

		#region IClientHook Members

		protected override void InitialiseCore()
		{
			DocGCGARInvoice.RegisterThisSubTypeOverride();
			GCGInvoicingBaseDocumentSupporter.RegisterThisSubTypeOverride();
		}

		public override Clients Client
		{
			get { return Clients.GCG; }
		}

		public override string ClientDisplayName
		{
			get { return "G C F Griffin Pty Ltd"; }
		}

		#endregion
	}
}
