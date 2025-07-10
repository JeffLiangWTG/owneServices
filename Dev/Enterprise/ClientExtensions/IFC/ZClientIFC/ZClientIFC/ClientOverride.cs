using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.IFC;
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

		public override Clients Client
		{
			get { return Clients.IFC; }
		}

		public override string ClientDisplayName
		{
			get { return "International Freight Consolidators"; }
		}

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return IFCDataRegistry.Instance; }
		}

		#region ClientTypeDeciders

		public override ITypeDeciderDictionary ClientTypeDeciders
		{
			get
			{
				if (fClientTypeDeciders == null)
				{
					Dictionary<Type, ITypeDecider> result = new Dictionary<Type, ITypeDecider>();
					result.Add(ObjectFactory.GetType<DocumentScanning.Integration.IStorageDocsValueObjectDataAdapter>(), new TypeDeciderImpl(typeof(IFCStorageDocsValueObjectDataAdapter)));
					fClientTypeDeciders = new TypeDeciderDictionary(result);
				}
				return fClientTypeDeciders;
			}
		}

		TypeDeciderDictionary fClientTypeDeciders;

		#endregion

		#region LogSubscriberes

		public override IEnumerable<ILogSubscriber> LogSubscribers
		{
			get
			{
				List<ILogSubscriber> logSubscriberList = new List<ILogSubscriber>();
				logSubscriberList.Add(new DocTypeLogSubscriber());
				return logSubscriberList;
			}
		}

		#endregion

		#endregion
	}
}
