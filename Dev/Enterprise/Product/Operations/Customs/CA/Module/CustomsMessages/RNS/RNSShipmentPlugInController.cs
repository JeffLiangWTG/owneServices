using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.GUI.PlugIns;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CA.Module
{
	class RNSShipmentPlugInController : BaseRNSPlugInController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.RNSShipmentPlugIn; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ForwardingShipment); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new RNSPlugIn(new RNSPlugInSupportShipmentWrapper((ForwardingShipment)businessEntity));
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
