using System;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using Enterprise.Client.M1A;
using Enterprise.Client.M1A.Business;
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

		public override Clients Client
		{
			get { return Clients.M1A; }
		}

		public override string ClientDisplayName
		{
			get { return "Mach 1"; }
		}

		protected override void InitialiseCore()
		{
			M1AInvoicingBaseDocumentSupporter.RegisterThisSubTypeOverride();
			M1ADocARInvoice.RegisterThisSubTypeOverride();
		}

		public override IExtensionObjects DbSchemaExtensionObjects { get; } = new M1AClientDbSchemaUpgradeInfo();
	}
}
