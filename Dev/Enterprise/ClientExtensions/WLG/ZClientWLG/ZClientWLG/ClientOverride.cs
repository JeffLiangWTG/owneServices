using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.WLG;
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
			WLG.WakoStatement.RegisterThisSubTypeOverride();
			WLG.DocWLGARInvoice.RegisterThisSubTypeOverride();
			WLG.WLGInvoicingBaseDocumentSupporter.RegisterThisSubTypeOverride();
		}

		public override Clients Client
		{
			get { return Clients.WLG; }
		}

		public override string ClientDisplayName
		{
			get { return "Wako"; }
		}

		public override string HelpWebPage
		{
			get { return ""; }
		}

		protected override ControllerOverrides GetControllerOverrides() => new ControllerOverrides();

		ITypeDeciderDictionary fClientTypeDeciders;
		public override ITypeDeciderDictionary ClientTypeDeciders
		{
			get
			{
				if (fClientTypeDeciders == null)
				{
					Dictionary<Type, ITypeDecider> result = new Dictionary<Type, ITypeDecider>();
					result.Add(typeof(ARInvoice), new TypeDeciderImpl(typeof(WLGARInvoice)));
					fClientTypeDeciders = new TypeDeciderDictionary(result);
				}
				return fClientTypeDeciders;
			}
		}

		#endregion
	}
}
