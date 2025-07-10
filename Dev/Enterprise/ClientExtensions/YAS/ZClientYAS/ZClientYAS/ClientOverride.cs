using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.YAS;
using Enterprise.Client.YAS.Business;
using Enterprise.Client.YAS.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;

#if DEBUG
using Enterprise.Client.YAS.Module;
#endif

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		protected ClientOverride() { }

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
			YASMenu.Initialise();
			DocYASForwardingShipment.RegisterThisSubTypeOverride();
			DocYASAWB.RegisterThisSubTypeOverride();
		}

		public override Clients Client
		{
			get { return Clients.YAS; }
		}

		public override string ClientDisplayName
		{
			get { return "YAS"; }
		}

#if DEBUG
		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();

			ClientOverrideModuleIdentifier moduleId = new ClientOverrideModuleIdentifier(ModuleIDs.JobShipment);
			ClientOverrideModuleInfo moduleInfo = new ClientOverrideModuleInfo(moduleId, typeof(YASShipmentModuleOverride).Assembly.FullName, typeof(YASShipmentModuleOverride).FullName, "AU");
			moduleOverrides.AddModuleOverride(moduleInfo);
			return moduleOverrides;
		}
#endif

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get
			{
				return YASDataRegistry.Instance;
			}
		}

		public override ITypeDeciderDictionary ClientTypeDeciders
		{
			get
			{
				if (clientTypeDeciders == null)
				{
					Dictionary<Type, ITypeDecider> result = new Dictionary<Type, ITypeDecider>();
					result.Add(typeof(ForwardingShipment), new TypeDeciderImpl(typeof(YASForwardingShipment)));

					clientTypeDeciders = new TypeDeciderDictionary(result);
				}
				return clientTypeDeciders;
			}
		}
		ITypeDeciderDictionary clientTypeDeciders;

		#endregion
	}
}
