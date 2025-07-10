using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Definitions;
using Enterprise.Client.TGE;
using Enterprise.Client.TGE.Business;
using Enterprise.Client.TGE.Module;
using Enterprise.ZArchitecture.Environment;
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
		}

		protected override void UninitialiseCore()
		{
		}

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return TGEDataRegistry.Instance; }
		}

		public override Clients Client
		{
			get { return Clients.TGE; }
		}

		public override string ClientDisplayName
		{
			get { return "Toll Transport"; }
		}

		protected override ModuleOverrides GetModuleOverrides()
		{
			if (Globals.IsDebugMode)
			{
				var moduleOverrides = new ModuleOverrides();
				moduleOverrides.AddModuleOverride(new ClientOverrideModuleInfo(
					new ClientOverrideModuleIdentifier(ModuleIDs.JobShipment),
					typeof(TGEShipmentModuleOverride).Assembly.FullName,
					typeof(TGEShipmentModuleOverride).FullName,
					Core.Constants.CountryCodes.Australia));
				return moduleOverrides;
			}
			return base.GetModuleOverrides();
		}

		public override IEnumerable<ILogSubscriber> LogSubscribers
		{
			get
			{
				List<ILogSubscriber> logSubscriberList = new List<ILogSubscriber>();
				if (TGEDataRegistry.Instance.EnableCSSDataExport && !TGEDataRegistry.Instance.CSSExportDirectory.IsEmpty &&
					Directory.Exists(TGEDataRegistry.Instance.CSSExportDirectory) && !TGEDataRegistry.Instance.CSSImportCustomsStatusCodes.Count.Equals(0)
					&& !TGEDataRegistry.Instance.CSSExportCustomsStatusCodes.Count.Equals(0))
				{
					logSubscriberList.Add(new CSSLogSubscriber());
				}
				return logSubscriberList;
			}
		}

		#endregion
	}
}
