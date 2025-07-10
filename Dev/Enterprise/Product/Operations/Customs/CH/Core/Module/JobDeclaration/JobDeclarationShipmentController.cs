using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CH.Module;

public class JobDeclarationShipmentController : Customs.Module.JobDeclarationShipmentController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

	protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);
}
