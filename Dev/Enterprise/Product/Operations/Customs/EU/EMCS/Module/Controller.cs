using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.EMCS.GUI;
using Enterprise.Customs.EU.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.EMCS.Module
{
	public class Controller : Customs.Module.JobDeclarationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(EMCSJobDeclaration);

		public override ControllerID ID => ControllerIDs.Customs.EU.EMCS;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.EMCS;

		protected override ZArchitecture.GUI.IZForm GetFormCore(IBusiness businessEntity) => new EMCSDeclarationForm((EMCSJobDeclaration)businessEntity);

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);
	}
}
