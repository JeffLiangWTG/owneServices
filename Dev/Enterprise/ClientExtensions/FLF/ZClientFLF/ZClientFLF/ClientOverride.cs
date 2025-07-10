using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.FLF;
using Enterprise.Freight.Forwarding.Business;
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
			FLFStatement.RegisterThisSubTypeOverride();
			DocFLFARInvoice.RegisterThisSubTypeOverride();
		}

		public override Clients Client
		{
			get
			{
				return Clients.FLF;
			}
		}

		public override string ClientDisplayName
		{
			get
			{
				return "Fresh Live & Frozen Aircargo";
			}
		}

		#region ClientTypeDeciders

		public override ITypeDeciderDictionary ClientTypeDeciders
		{
			get
			{
				if (fClientTypeDeciders == null)
				{
					Dictionary<Type, ITypeDecider> dictionary = new Dictionary<Type, ITypeDecider>();
					dictionary.Add(typeof(ForwardingConsol), new TypeDeciderImpl(typeof(FLFForwardingConsol)));
					fClientTypeDeciders = new TypeDeciderDictionary(dictionary);
				}
				return fClientTypeDeciders;
			}
		}

		ITypeDeciderDictionary fClientTypeDeciders;

		#endregion

		#endregion
	}
}
