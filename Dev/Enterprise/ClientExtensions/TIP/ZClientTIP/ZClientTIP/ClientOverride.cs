using System;
using CargoWise.Definitions;
using Enterprise.Client.TIP;
using Enterprise.Client.TIP.DocWrappers;
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
			DocTIPAPPayment.RegisterThisSubTypeOverride();
			DocTIPBatchARInvoiceLineTransactionLine.RegisterThisSubTypeOverride();
			DocTIPARBatchInvoice.RegisterThisSubTypeOverride();
		}

		public override Clients Client
		{
			get { return Clients.TIP; }
		}

		public override string ClientDisplayName
		{
			get { return "Toll International Pty Ltd"; }
		}

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return TIPDataRegistry.Instance; }
		}

		#endregion
	}
}
