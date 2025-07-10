using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.GUI.PlugIns;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CA.Module
{
	class RNSMFTallyShipmentsPlugInController : BaseRNSPlugInController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.RNSMFTallyShipmentsPlugIn; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(TallyContainer); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new RNSCurrentDependentPlugIn(new RNSPlugInSupportTallyWrapper((TallyContainer)businessEntity));
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
