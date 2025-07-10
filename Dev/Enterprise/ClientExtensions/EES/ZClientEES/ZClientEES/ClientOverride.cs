using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.EES;
using Enterprise.Client.EES.Business;
using Enterprise.Freight.Forwarding.Business;
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

		public override Clients Client
		{
			get { return Clients.EES; }
		}

		public override string ClientDisplayName
		{
			get { return "EES Shipping Pty Ltd"; }
		}

		#region IClientHook Members

		#region Core
		protected override void InitialiseCore()
		{
		}
		protected override void UninitialiseCore()
		{
		}
		#endregion

		#region Registry
		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get
			{
				return EESDataRegistry.Instance;
			}
		}
		#endregion

		#region ClientTypeDeciders
		public override ITypeDeciderDictionary ClientTypeDeciders
		{
			get
			{
				if (fClientTypeDeciders == null)
				{
					Dictionary<Type, ITypeDecider> result = new Dictionary<Type, ITypeDecider>();
					result.Add(typeof(ForwardingShipment), new TypeDeciderImpl(typeof(EESForwardingShipment)));
					fClientTypeDeciders = new TypeDeciderDictionary(result);
				}
				return fClientTypeDeciders;
			}
		}
		ITypeDeciderDictionary fClientTypeDeciders;
		#endregion

		#endregion
	}
}
